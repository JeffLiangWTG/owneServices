using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.Auto;

namespace Enterprise.Customs.GB.Business.Common
{
	public static class EdifactSegmentPuller
	{
		public static ZString GetOriginalCusDecSegment(SegmentGroup outgoingCusdec, ZString posOfSegment, UNCharacterSet charSet)
		{
			try
			{
				if (outgoingCusdec != null)
				{
					if (!posOfSegment.IsEmpty)
					{
						ZInt segNo = ZInt.ParseSafe(posOfSegment, 0);
						string[] segments = GetArrayOfItemsBySplittingOnDelimiter(charSet.SegmentDelimiter, charSet.EscapeCharacter, outgoingCusdec.ToString(charSet));
						return segNo > 0 ? segments[segNo - 1] : "";
					}
				}
			}
			catch (IndexOutOfRangeException)
			{
				return "(Too few segments in original message, " + posOfSegment + " is too high)";
			}

			return ZString.Empty;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static ZString GetBoxNumberFromSegment(ZString segmentText, int itemNumber, int elementNumber)
		{
			if (itemNumber == 0) // Header
			{
				// TODO - check this works when using an MCP character set
				if (segmentText.StartsWith("BGM", StringComparison.OrdinalIgnoreCase))
				{
					return "[1]";
				}
				else if (segmentText.StartsWith("CST", StringComparison.OrdinalIgnoreCase))
				{
					return "[1]";
				}
				else if (segmentText.StartsWith("LOC+14", StringComparison.OrdinalIgnoreCase))
				{
					return "[30]";
				}
				else if (segmentText.StartsWith("LOC+35", StringComparison.OrdinalIgnoreCase))
				{
					return "[15]";
				}
				else if (segmentText.StartsWith("LOC+36", StringComparison.OrdinalIgnoreCase))
				{
					return "[17]";
				}
				else if (segmentText.StartsWith("LOC+76", StringComparison.OrdinalIgnoreCase))
				{
					return "[61]";
				}
				else if (segmentText.StartsWith("FTX+ACB", StringComparison.OrdinalIgnoreCase))
				{
					return "[44]"; //AI 
				}
				else if (segmentText.StartsWith("RFF+ABI", StringComparison.OrdinalIgnoreCase))
				{
					return "[48]";
				}
				else if (segmentText.StartsWith("RFF", StringComparison.OrdinalIgnoreCase))
				{
					return "[44]";
				}
				else if (segmentText.StartsWith("SEL", StringComparison.OrdinalIgnoreCase))
				{
					return "[44]";
				}
				else if (segmentText.StartsWith("DOC+916", StringComparison.OrdinalIgnoreCase))
				{
					return "[44]";  // SD
				}
				else if (segmentText.StartsWith("TDT", StringComparison.OrdinalIgnoreCase))
				{
					return "[21]/[26]";
				}
				else if (segmentText.StartsWith("NAD+CZ", StringComparison.OrdinalIgnoreCase))
				{
					return "[2]";
				}
				else if (segmentText.StartsWith("NAD+CN", StringComparison.OrdinalIgnoreCase))
				{
					return "[8]";
				}
				else if (segmentText.StartsWith("NAD+DT", StringComparison.OrdinalIgnoreCase))
				{
					return "[14]";
				}
				else if (segmentText.StartsWith("MOA+144", StringComparison.OrdinalIgnoreCase))
				{
					return "[62]";
				}
				else if (segmentText.StartsWith("MOA+64", StringComparison.OrdinalIgnoreCase))
				{
					return "[63]";
				}
				else if (segmentText.StartsWith("MOA+70", StringComparison.OrdinalIgnoreCase))
				{
					return "[66]";
				}
				else if (segmentText.StartsWith("MOA+39", StringComparison.OrdinalIgnoreCase))
				{
					return "[41]";
				}
				else if (segmentText.StartsWith("MOA+103", StringComparison.OrdinalIgnoreCase))
				{
					return "[67]";
				}
				else if (segmentText.StartsWith("MOA+105", StringComparison.OrdinalIgnoreCase))
				{
					return "[68]";
				}
				else if (segmentText.StartsWith("MOA+52", StringComparison.OrdinalIgnoreCase))
				{
					return "[65]";
				}
				else if (segmentText.StartsWith("PCD+12", StringComparison.OrdinalIgnoreCase))
				{
					return "[65]";
				}
			}
			else  // item
			{
				if (segmentText.StartsWith("CST", StringComparison.OrdinalIgnoreCase))
				{
					if (elementNumber == 3)
					{
						return "[37]";
					}

					if (elementNumber == 7)
					{
						return "[36]";
					}

					if (elementNumber == 4)
					{
						return "[33]";
					}

					if (elementNumber == 5)
					{
						return "[33.2]";
					}

					if (elementNumber == 6)
					{
						return "[33.3]";
					}
					else
					{
						return "";
					}
				}
				else if (segmentText.StartsWith("FTX+ACB", StringComparison.OrdinalIgnoreCase))
				{
					return "[44]";
				}
				else if (segmentText.StartsWith("LOC+27", StringComparison.OrdinalIgnoreCase))
				{
					return "[34a]";
				}
				else if (segmentText.StartsWith("MEA+AAH", StringComparison.OrdinalIgnoreCase))
				{
					return "[35]";
				}
				else if (segmentText.StartsWith("MEA+AAR", StringComparison.OrdinalIgnoreCase))
				{
					return "[38]";
				}
				else if (segmentText.StartsWith("MEA+AAT", StringComparison.OrdinalIgnoreCase))
				{
					return "[44]";
				}
				else if (segmentText.StartsWith("PAC", StringComparison.OrdinalIgnoreCase))
				{
					return "[31]";
				}
				else if (segmentText.StartsWith("PCI", StringComparison.OrdinalIgnoreCase))
				{
					return "[31]";
				}
				else if (segmentText.StartsWith("RFF+AAQ", StringComparison.OrdinalIgnoreCase))
				{
					return "[31]";
				}
				else if (segmentText.StartsWith("MOA+123", StringComparison.OrdinalIgnoreCase))
				{
					return "[46]";
				}
				else if (segmentText.StartsWith("MOA+38", StringComparison.OrdinalIgnoreCase))
				{
					return "[41]";
				}
				else if (segmentText.StartsWith("RFF+ABJ", StringComparison.OrdinalIgnoreCase))
				{
					return "[39]";
				}
				else if (segmentText.StartsWith("IMD", StringComparison.OrdinalIgnoreCase))
				{
					return "[31]";
				}
				else if (segmentText.StartsWith("DOC+998", StringComparison.OrdinalIgnoreCase))
				{
					return "[40]";
				}
				else if (segmentText.StartsWith("DOC+916", StringComparison.OrdinalIgnoreCase))
				{
					return "[44]";
				}
				else if (segmentText.StartsWith("GEI", StringComparison.OrdinalIgnoreCase))
				{
					if (segmentText.Contains("VM", StringComparison.OrdinalIgnoreCase))
					{ return "[43]"; }
					if (segmentText.Contains("VA", StringComparison.OrdinalIgnoreCase))
					{ return "[45a]"; }
				}
				else if (segmentText.StartsWith("PCD+9", StringComparison.OrdinalIgnoreCase))
				{
					return "[45]";
				}
				else if (segmentText.StartsWith("TAX", StringComparison.OrdinalIgnoreCase))
				{
					return "[47]";
				}
				else if (segmentText.StartsWith("TAX", StringComparison.OrdinalIgnoreCase))
				{
					return "[47]";
				}
				else if (segmentText.StartsWith("MOA+161", StringComparison.OrdinalIgnoreCase))
				{
					return "[47]";
				}
			}
			return "";
		}

		public static ZString GetOriginalDataFromCusDecSegment(ZString segment, ZString elementPosition, ZString componentNumber, UNCharacterSet charSet)
		{
			try
			{
				ZString element;
				string[] elements;
				if (!elementPosition.IsEmpty && !segment.IsEmpty)
				{
					int elementNo = ZInt.ParseSafe(elementPosition, 0);
					elements = GetArrayOfItemsBySplittingOnDelimiter(charSet.ElementDelimiter, charSet.EscapeCharacter, segment);
					if (elementNo > 0)
					{
						element = elements[elementNo - 1];
						if (!componentNumber.IsEmpty && !element.IsEmpty)
						{
							ZInt componentNo = ZInt.ParseSafe(componentNumber, 0);
							string[] components = GetArrayOfItemsBySplittingOnDelimiter(charSet.SubElementDelimiter, charSet.EscapeCharacter, element);
							ZString component = components[componentNo - 1].Replace(charSet.EscapeCharacter + charSet.SubElementDelimiter, charSet.SubElementDelimiter);
							return component;
						}
						else
						{ return element; }
					}
					else
					{ return ""; }  // Do not want to return "NAD" or "LOC" when the error points to the entire segment
				}
				else
				{ return segment; }
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return ZString.Empty;
			}
		}

		public static string[] GetArrayOfItemsBySplittingOnDelimiter(string delimiter, string escape, ZString stringToSplit)
		{
			string negativeLookBehindPattern = string.Format(@"(?<!\{1})\{0}", delimiter, escape);   // e.g. (?<!\?)\:   This means "split on colon except when preceeded by a question mark". 
			return Regex.Split(stringToSplit, negativeLookBehindPattern);
		}
	}
}
