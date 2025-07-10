using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	[DebuggerDisplay("Lines = {Count}, MaxLineLength = {MaxLineLength}, Height = {Height}")]
	public class TextSection : List<TextLine>
	{
		public TextSection(int maxLineLength)
		{
			MaxLineLength = maxLineLength;
		}

		public int MaxLineLength
		{
			get { return maxLineLength; }
			set
			{
				if (maxLineLength != value)
				{
					maxLineLength = value;
					this.ForEach((textLine) => textLine.MaxLineLength = maxLineLength);
				}
			}
		}
		int maxLineLength;

		public int Height
		{
			get { return this.Sum((line) => line.Height); }
		}

		public new void Add(TextLine textLine)
		{
			if (textLine != null)
			{
				textLine.MaxLineLength = MaxLineLength;
				base.Add(textLine);
			}
		}

		public void Add(ZString text)
		{
			TextLine line = new TextLine(text);
			line.MaxLineLength = MaxLineLength;
			Add(line);
		}

		public void Merge(TextSection textSection)
		{
			if (textSection != null)
			{
				textSection.ForEach((textLine) => textLine.MaxLineLength = MaxLineLength);
				AddRange(textSection);
			}
		}

		public override string ToString()
		{
			return String.Join("\n", this.Select((lines) => lines.ToString()).ToArray());
		}

		public ZString[] ToStringArray()
		{
			return this.SelectMany((line) => line.ToStringArray()).ToArray();
		}
	}
}
