using System.Collections.Generic;
using System.Drawing;
using Enterprise.DocumentEngine.MacroValueProviders;

namespace Enterprise.DocumentEngine.Renderer
{
	internal class OverflowManager
	{
		internal OverflowManager(string macro, Font cellFont, int cellWidth)
		{
			this.NotesTitle = OverFlowToFollowPage.GetFieldTitle(macro);
			this.maximumRows = OverFlowToFollowPage.GetMaximumRows(macro);
			this.overflowBehaviour = OverFlowToFollowPage.GetOverflowBehavior(macro);
			this.cellFont = cellFont;
			this.cellWidth = cellWidth;
		}

		internal readonly string NotesTitle;
		readonly int maximumRows;
		readonly OverflowBehaviour overflowBehaviour;
		readonly int cellWidth;
		readonly Font cellFont;

		string fullText;
		internal List<string> FirstPageText;
		internal string OverflowText;
		internal void ManagerOverflow(string fullText)
		{
			if (this.fullText != fullText)
			{
				this.fullText = fullText;
				if (!string.IsNullOrEmpty(fullText))
				{
					switch (overflowBehaviour)
					{
						case OverflowBehaviour.WrapOverflowWithContinued:
							WrapOverflow(" (continued...)", "(cont.)");
							break;
						case OverflowBehaviour.WrapOverflowWithoutContinued:
							WrapOverflow("", "");
							break;
						case OverflowBehaviour.MoveAllContentWithContinued:
							MoveAllContent("(continued...)");
							break;
						case OverflowBehaviour.MoveAllContentWithoutContinued:
							MoveAllContent("");
							break;
						case OverflowBehaviour.MoveAllContentAndKeepOriginal:
							MoveAllContentAndKeepOriginal();
							break;
					}
				}
				else
				{
					FirstPageText = new List<string>();
					OverflowText = "";
				}
			}
		}

		void WrapOverflow(string suffix, string shortSuffix)
		{
			var wrapper = new TextWrapper(fullText, cellWidth, cellFont, maximumRows);
			var wrappedTextLines = wrapper.WrappedTextLines;
			var remainder = wrapper.RemainderAfterMaxLines;

			if (!string.IsNullOrEmpty(suffix) && remainder.Length > 0)
			{
				var graphicsManager = new GraphicsManager();
				graphicsManager.Graphics.PageUnit = GraphicsUnit.Millimeter;

				var lineLengthChecker = new LengthChecker(graphicsManager.Graphics, cellFont, cellWidth);
				var lengthOfSuffix = lineLengthChecker.GetWidthOfCharacterSequence(suffix);

				string lastLine = wrappedTextLines[wrappedTextLines.Count - 1];
				var lastLineWrapper = new TextWrapper(lastLine, cellWidth - lengthOfSuffix, cellFont, 1);

				if (lastLineWrapper.LastBreakWasAtTheEndOfAWord)
				{
					remainder = lastLineWrapper.RemainderAfterMaxLines + wrapper.LastBreakSeparator + remainder;
					lastLine = lastLineWrapper.WrappedTextLines[0] + suffix;
				}
				else
				{
					remainder = lastLine + wrapper.LastBreakSeparator + remainder;

					suffix = suffix.Trim();
					lengthOfSuffix = lineLengthChecker.GetWidthOfCharacterSequence(suffix);
					lastLine = lengthOfSuffix > cellWidth ? shortSuffix : suffix;
				}
				wrappedTextLines[wrappedTextLines.Count - 1] = lastLine;
			}

			FirstPageText = wrapper.WrappedTextLines;
			OverflowText = remainder.Trim();
		}

		void MoveAllContent(string replacement)
		{
			var replacementList = new List<string>();
			replacementList.Add(replacement);
			MoveAllContent(replacementList);
		}

		void MoveAllContent(List<string> replacement)
		{
			FirstPageText = replacement;
			OverflowText = fullText.Trim();
		}

		void MoveAllContentAndKeepOriginal()
		{
			var wrapper = new TextWrapper(fullText, cellWidth, cellFont, maximumRows);
			MoveAllContent(wrapper.WrappedTextLines);
		}
	}
}
