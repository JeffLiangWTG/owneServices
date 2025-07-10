using System;
using CargoWise.Types;

namespace Enterprise.DataTransfer.Business
{
	public abstract class FixedWidthFlatFileFormat : FlatFileFormat
	{
		#region Import

		protected virtual string GetValue(int position, int length, ZString dataLine)
		{
			return dataLine.SubstringSafe(position, length);
		}

		#endregion

		#region Generate Whitespace

		protected virtual string GenerateWhitespace(int numberOfWhitespaceChars, char whiteSpaceChar)
		{
			string whiteSpace = String.Empty;
			for (int i = 0; i < numberOfWhitespaceChars; i++)
			{
				whiteSpace += whiteSpaceChar;
			}
			return whiteSpace;
		}

		#endregion

		#region FillFixedLengthField

		protected virtual void FillFixedLengthField(ZString value, int lengthOfFileField, ZStringBuilder lineBuilder, bool padToLeft, char whiteSpaceChar)
		{
			if (value.Length == lengthOfFileField)
			{
				lineBuilder.Append(value);
			}
			else if (value.Length > lengthOfFileField)
			{
				lineBuilder.Append(value.Left(lengthOfFileField));
			}
			else
			{
				ZString newValue = (padToLeft) ? value.PadLeft(lengthOfFileField, whiteSpaceChar) : value.PadRight(lengthOfFileField, whiteSpaceChar);
				lineBuilder.Append(newValue);
			}
		}

		#endregion
	}
}
