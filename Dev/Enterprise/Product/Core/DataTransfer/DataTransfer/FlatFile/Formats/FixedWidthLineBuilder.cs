using System;

namespace Enterprise.DataTransfer.Business
{
	public class FixedWidthLineBuilder
	{
		public FixedWidthLineBuilder(char whitespaceCharacter, int length)
		{
			WhitespaceCharacter = whitespaceCharacter;
			LineLength = length;
			Line = CreateWhitespace(LineLength, WhitespaceCharacter);
		}

		string CreateWhitespace(int length, char whiteCharacter)
		{
			string blankLine = String.Empty;
			for (int i = 0; i < length; i++)
			{
				blankLine += whiteCharacter;
			}
			return blankLine;
		}

		public void AddDataToLine(string data, int positon, int length)
		{
			AddDataToLine(data, positon, length, false);
		}

		public void AddDataToLine(string data, int positon, int length, bool padRight)
		{
			AddDataToLine(data, positon, length, padRight, WhitespaceCharacter);
		}

		public void AddDataToLine(string data, int positon, int length, bool padRight, char whiteChar)
		{
			data = TruncateData(data, length, padRight, whiteChar);
			Line = Line.Remove(positon, data.Length);
			Line = Line.Insert(positon, data);
		}

		string TruncateData(string data, int length, bool padRight, char whiteChar)
		{
			if (data.Length > length)
			{
				data = data.Substring(0, length);
			}
			else
			{
				string whiteSpace = CreateWhitespace(length - data.Length, whiteChar);
				data = padRight ? whiteSpace + data : data + whiteSpace;
			}
			return data;
		}

		public override string ToString()
		{
			return Line;
		}

		string Line;
		readonly int LineLength;
		readonly char WhitespaceCharacter;
	}
}
