using System.Collections.Generic;
using System.Drawing;
using Enterprise.DocumentEngine.Renderer;

namespace Enterprise.DocumentEngine
{
	public class TextWrapper
	{
		public TextWrapper(string memo, int widthInXls, Font cellFont)
			: this(memo, widthInXls, cellFont, int.MaxValue)
		{
		}

		[System.ThreadStatic]
		static GraphicsManager GraphicsManager;

		public TextWrapper(string memo, int widthInXls, Font cellFont, int maxLines)
		{
			WrappedTextLines = new List<string>();
			RemainderAfterMaxLines = memo;
			LastBreakSeparator = "";
			var graphicsManager = GraphicsManager;
			if (graphicsManager == null)
			{
				GraphicsManager = new GraphicsManager();
				GraphicsManager.Graphics.PageUnit = GraphicsUnit.Millimeter;
				graphicsManager = GraphicsManager;
			}
			var checker = new LengthChecker(graphicsManager.Graphics, cellFont, widthInXls);

			string lineStillToWrap;
			foreach (string memoLine in memo.Split('\n'))
			{
				lineStillToWrap = memoLine.Replace('\t', ' ').Trim();
				do
				{
					string charsToCopy = string.Empty;
					if (lineStillToWrap.Length > 0)
					{
						int numberOfCharsToCopy = System.Math.Max(checker.NumberOfCharsThatFitInProposedWidth(lineStillToWrap), 1);

						charsToCopy = lineStillToWrap.Substring(0, numberOfCharsToCopy);
						LastBreakSeparator = "";
						if (numberOfCharsToCopy < lineStillToWrap.Length)
						{
							if (lineStillToWrap[numberOfCharsToCopy] != ' ')
							{
								int indexOfLastSpace = charsToCopy.LastIndexOf(' ');

								if (indexOfLastSpace > 0)
								{
									numberOfCharsToCopy = indexOfLastSpace;
									charsToCopy = charsToCopy.Substring(0, numberOfCharsToCopy);
									LastBreakSeparator = " ";
								}
							}
						}
						lineStillToWrap = lineStillToWrap.Remove(0, numberOfCharsToCopy).TrimStart();
					}

					WrappedTextLines.Add(charsToCopy.Trim());
					RemainderAfterMaxLines = RemainderAfterMaxLines.Remove(0, charsToCopy.Length).TrimStart();
				}
				while (lineStillToWrap.Length > 0 && WrappedTextLines.Count < maxLines);

				if (WrappedTextLines.Count == maxLines)
				{
					if (string.IsNullOrEmpty(lineStillToWrap) && !string.IsNullOrEmpty(RemainderAfterMaxLines))
					{
						LastBreakSeparator = "\r\n";
					}
					break;
				}
			}
		}

		public readonly string RemainderAfterMaxLines;
		public readonly List<string> WrappedTextLines;
		public readonly string LastBreakSeparator;

		public bool LastBreakWasAtTheEndOfAWord
		{
			get { return !string.IsNullOrEmpty(LastBreakSeparator) || string.IsNullOrEmpty(RemainderAfterMaxLines); }
		}
	}
}
