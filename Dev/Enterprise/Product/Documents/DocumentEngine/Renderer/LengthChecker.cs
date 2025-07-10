using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Renderer
{
	class LengthChecker
	{
		public LengthChecker(Graphics graphics, Font font, int proposedWidthInXlsUnits)
		{
			this.graphics = graphics;
			this.font = font;
			this.proposedWidth = proposedWidthInXlsUnits / excelUnitsConverstionFactor;
		}

		readonly Graphics graphics;
		readonly Font font;
		readonly float proposedWidth;

		public int NumberOfCharsThatFitInProposedWidth(string text)
		{
			int minPossibleLength = 0;
			int maxPossibleLength = text.Length;

			int nextLengthToCheck = GuessNextLengthToCheck(text, maxPossibleLength);
			int lastLengthToCheck = maxPossibleLength;
			int lastBeforeThatLengthToCheck = maxPossibleLength;

			if (maxPossibleLength < nextLengthToCheck)
			{
				return maxPossibleLength;
			}

			while (nextLengthToCheck != lastLengthToCheck)
			{
				if (nextLengthToCheck == lastBeforeThatLengthToCheck)
				{
					return Math.Min(nextLengthToCheck, lastLengthToCheck);
				}

				if (nextLengthToCheck > lastLengthToCheck)
				{
					if (lastLengthToCheck > minPossibleLength)
					{
						minPossibleLength = lastLengthToCheck;
					}
				}
				else
				{
					if (lastLengthToCheck < maxPossibleLength)
					{
						maxPossibleLength = lastLengthToCheck;
					}
				}

				if (nextLengthToCheck > maxPossibleLength || nextLengthToCheck < minPossibleLength)
				{
					return GetBinaryChopFromMaxAndMin(text, minPossibleLength, maxPossibleLength);
				}

				lastBeforeThatLengthToCheck = lastLengthToCheck;
				lastLengthToCheck = nextLengthToCheck;
				nextLengthToCheck = GuessNextLengthToCheck(text.Substring(0, nextLengthToCheck), lastLengthToCheck);
			}

			return nextLengthToCheck;
		}

		int GuessNextLengthToCheck(string text, int lastLengthToCheck)
		{
			float actualWidth = GetWidthOfCharacterSequenceCore(text);
			unchecked //NaNs turned into 0
			{
				return (int)(proposedWidth / actualWidth * lastLengthToCheck);
			}
		}

		int GetBinaryChopFromMaxAndMin(string text, int minCursor, int maxCursor)
		{
			if ((maxCursor - minCursor) <= 1)
			{
				return maxCursor;
			}
			int cursor = (minCursor + maxCursor) >> 1;

			while (minCursor < maxCursor)
			{
				float width = GetWidthOfCharacterSequenceCore(text.Substring(0, cursor));

				if (width < proposedWidth)
				{
					minCursor = cursor;
					cursor += (maxCursor - cursor + 1) >> 1;
					if (cursor >= maxCursor)
					{
						break;
					}
				}
				else
				{
					maxCursor = cursor;
					cursor -= (cursor - minCursor + 1) >> 1;
					if (cursor <= minCursor)
					{
						break;
					}
				}
			}
			return cursor;
		}

		float GetWidthOfCharacterSequenceCore(string text)
		{
			try
			{
				if (text?.Length > GraphicsManager.MaxMeasurableStringLengthWithoutLineBreak)
				{
					var splits = ((ZString)text).Split(GraphicsManager.MaxMeasurableStringLengthWithoutLineBreak);
					var sizes = splits.Select(s => graphics.MeasureString(s, font, GraphicsManager.MaxMeasurableStringLengthWithoutLineBreak));
					return sizes.Sum(s => s.Width);
				}

				return graphics.MeasureString(text, font).Width;
			}
			catch (ExternalException ex) when (ex.IsGdiPlusException())
			{
				ex.ReportWithInvalidCharacters(text);
				throw;
			}
		}

		public int GetWidthOfCharacterSequence(string text)
		{
			return (int)GetWidthOfCharacterSequenceCore(text) * excelUnitsConverstionFactor;
		}

		const int excelUnitsConverstionFactor = Enterprise.DocumentEngine.MacroValueProviders.ShrinkToFit.XlsWidthConversionFactor;
	}
}
