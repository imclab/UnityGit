using UnityEngine;

namespace UnityGit.DiffView.State {
  public class WordDiffState : BaseState {
    public override void OnRefresh() {
      isDirty = false;
      if(content == null) {
        _lines = new Line[0];
        return;
      }

      string[] rawLines = content.Split('\n');
      int state = 0;
      LinesBuilder builder = new LinesBuilder();
      foreach(string rawLine in rawLines) {
        if(rawLine == "")
          continue;

        char selector;
        switch(state) {
          case 0: // Expecting file header info, OR possibly a segment marker.
            selector = rawLine[0];
            if(selector == '@')
              state = 1; // Got a segment marker, so handle accordingly.
            break;
          case 1: // Shouldn't happen...
            Debug.LogError("Uh, this should not have happened.");
            break;
          case 2: // In content-line segments...
            selector = rawLine[0];
            switch(selector) {
              case '@': state = 1; break;
              case '~': state = 3; break;
              case ' ': state = 4; break;
              case '+': state = 5; break;
              case '-': state = 6; break;
              default: state = 0; break;
            }
            break;
          case 3: case 4: case 5: case 6: // Shouldn't happen...
            Debug.LogError("Uh, this should not have happened.");
            break;
        }

        switch(state) {
          case 0: // Expecting file header info.
            builder.AddSegment(header, rawLine);
            builder.CommitLine();
            break;
          case 1: // Expecting segment marker.
            int splitPoint = rawLine.IndexOf("@@", 2);
            builder.AddSegment(marker, rawLine.Substring(0, splitPoint + 2));
            builder.AddSegment(header, rawLine.Substring(splitPoint + 2));
            builder.CommitLine();
            state = 2;
            break;
          case 2: // Shouldn't happen...
            Debug.LogError("Uh, this should not have happened.");
            break;
          case 3: // End-of-line.
            builder.CommitLine();
            state = 2;
            break;
          case 4: // Unchanged content segment.
            builder.AddSegment(unchanged, rawLine.Substring(1));
            state = 2;
            break;
          case 5: // Added content segment.
            builder.AddSegment(add, rawLine.Substring(1));
            state = 2;
            break;
          case 6: // Removed content segment.
            builder.AddSegment(remove, rawLine.Substring(1));
            state = 2;
            break;
        }
      }
      _lines = builder.ToArray();
    }
  }
}