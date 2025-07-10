using System;
using System.Collections.Specialized;
using System.Text;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class TextLayout
	{
		public TextLayout()
		{
			Clear();
		}

		public int VerticalPosition
		{
			get { return fVerticalPosition; }
		}

		public int HorizontalPosition
		{
			get { return fHorizontalPosition; }
		}

		public void CR()
		{
			fHorizontalPosition = 1;
		}

		public void LF()
		{
			fVerticalPosition++;
		}

		public void CRLF()
		{
			CR();
			LF();
		}

		public void Print(TextLayout printSegment)
		{
			for (int segment = 0; segment < printSegment.Collection.Count; segment++)
			{
				Print(printSegment.Collection[segment]);
			}
		}

		public void Print(string text)
		{
			if (Collection.Count > 0)
			{
				LF();
			}
			PrintAt(VerticalPosition, 1, text);
		}

		public void PrintAt(int vertical, TextLayout printSegment)
		{
			throw new Exception("Not yet written");
		}

		public void PrintAt(int vertical, int horizontal, TextLayout printSegment)
		{
			for (int segment = 0; segment < printSegment.Collection.Count; segment++)
			{
				PrintAt(vertical + segment, horizontal, printSegment.Collection[segment]);
			}
		}

		public void PrintAt(int vertical, int horizontal, string text)
		{
			CheckVertical(vertical);
			CheckHorizontal(horizontal);

			while (Collection.Count < vertical)
			{
				Collection.Add("");
			}

			fVerticalPosition = vertical;
			fHorizontalPosition = horizontal + text.Length;

			vertical--; // Switch to 0 index base
			horizontal--;   // Switch to 0 index base
			Collection[vertical] = Collection[vertical].PadRight(horizontal + text.Length);
			Collection[vertical] = Collection[vertical].Substring(0, horizontal) + text + Collection[vertical].Substring(horizontal + text.Length);
		}

		public void Clear()
		{
			Collection = new StringCollection();
			fVerticalPosition = 1;
			fHorizontalPosition = 1;
		}

		public string Text
		{
			get
			{
				StringBuilder result = new StringBuilder(Collection.Count);
				if (Collection.Count > 0)
				{
					result.Append(Collection[0].TrimEnd());
					for (int collectionLine = 1; collectionLine < Collection.Count; collectionLine++)
					{
						result.Append(System.Environment.NewLine + Collection[collectionLine].TrimEnd());
					}
				}
				return result.ToString();
			}
		}

		#region Implementation
		protected internal StringCollection Collection;
		protected int fVerticalPosition;
		protected int fHorizontalPosition;

		protected void CheckVertical(int vertical)
		{
			if (vertical < 1)
			{
				throw new ApplicationException("All vertical positions are indexed greater than 0");
			}
		}

		protected void CheckHorizontal(int horizontal)
		{
			if (horizontal < 1)
			{
				throw new ApplicationException("All horizontal positions are indexed greater than 0");
			}
		}

		#endregion
	}
}
