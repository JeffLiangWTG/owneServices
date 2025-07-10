using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	[DebuggerDisplay("Height = {Height}, MaxLineLength = {MaxLineLength}")]
	public class TextLine
	{
		public TextLine(ZString line)
		{
			this.line = line;
			Height = 1;
		}

		readonly ZString line;

		public int MaxLineLength
		{
			get { return maxLineLength; }
			set
			{
				if (maxLineLength != value)
				{
					maxLineLength = value;
					Height = CalculateTextHeight(line);
				}
			}
		}
		int maxLineLength;

		public int Height { get; private set; }

		int CalculateTextHeight(ZString text)
		{
			int result = 1;

			if (MaxLineLength > 0 && text.Length > MaxLineLength)
			{
				KeyValuePair<ZString, ZString> tuple = Split(text);

				ZString reminder = tuple.Value;

				if (!reminder.IsEmpty)
				{
					result += CalculateTextHeight(reminder);
				}
			}

			return result;
		}

		ZString[] SplitText(ZString text)
		{
			ZString[] result = null;

			if (MaxLineLength > 0 && text.Length > MaxLineLength)
			{
				KeyValuePair<ZString, ZString> tuple = Split(text);

				ZString heading = tuple.Key;
				ZString reminder = tuple.Value;

				List<ZString> lines = new List<ZString>();

				lines.Add(heading);

				if (!reminder.IsEmpty)
				{
					lines.AddRange(SplitText(reminder));
				}

				result = lines.ToArray();
			}

			return result ?? new[] { text };
		}

		KeyValuePair<ZString, ZString> Split(ZString text)
		{
			ZString heading = text.Substring(0, MaxLineLength);
			ZString reminder = text.Substring(MaxLineLength, text.Length - MaxLineLength);

			int index = heading.LastIndexOf(' ');

			if (reminder.Length > 0 && !char.IsWhiteSpace(reminder[0]) && index > 0)
			{
				heading = text.Substring(0, index);
				reminder = text.Substring(heading.Length, text.Length - heading.Length);
			}

			return new KeyValuePair<ZString, ZString>(heading, reminder.TrimStart());
		}

		public override string ToString()
		{
			return line;
		}

		public ZString[] ToStringArray()
		{
			return SplitText(line);
		}
	}
}
