using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.GB.Chief.GenericMessagingHarness;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Chief.CusRes
{
	public static class CusResEdifactParser
	{
		public static ZString GetStyleAndIcsDescription(ZString styleOfEntry, ZString icsOfEntry, string separator = " ")
		{
			var soe = string.Empty;
			var ics = string.Empty;
			if (!styleOfEntry.IsEmpty)
			{
				soe = string.Format("{0} ({1})", styleOfEntry, new ExportStyleOfEntries().GetDescriptionFromCode(styleOfEntry));
			}
			return soe + separator + ics;
		}

		public static string GetRejectionSummaryFromHtmlInterpretation(ZString html)
		{
			var sb = new ZStringBuilder();
			var errors = new List<ErrorFromChiefHumanReadable>();
			foreach (Match match in Regex.Matches(html, @"<tr><td>(?<CHIEFERROR>.*?)</td><td>(?<ORIGINALVALUE>.*?)</td><td>(?<SEGMENT>.*?)</td><td>(?<ELEMENT>.*?)</td><td>(?<ITEMNUMBER>.*?)</td><td>(?<BOXNUMBER>.*?)</td></tr>"))
			{
				var line = "";
				var errorText = "";
				var original = "";
				var box = "";
				Group groupEnum = match.Groups["CHIEFERROR"];
				if (groupEnum != null && groupEnum.Value != null)
				{
					errorText = groupEnum.Value;
				}
				Group groupItem = match.Groups["ITEMNUMBER"];
				if (groupItem != null && groupItem.Value != null)
				{
					line = groupItem.Value;
				}
				Group groupOriginal = match.Groups["ORIGINALVALUE"];
				if (groupOriginal != null && groupOriginal.Value != null)
				{
					original = groupOriginal.Value;
				}
				Group groupBox = match.Groups["BOXNUMBER"];
				if (groupBox != null && groupBox.Value != null)
				{
					box = groupBox.Value;
				}
				errors.Add(new ErrorFromChiefHumanReadable(line, errorText, original, box));
			}

			var grouped = (from ErrorFromChiefHumanReadable ed in errors.ToArray() select ed).GroupBy(e => e.Line);
			foreach (var g in grouped)
			{
				sb.Append(g.Key == "H" ? "Header" : "Line " + g.Key);
				foreach (var i in g)
				{
					if (string.IsNullOrEmpty(i.Original.Trim()))  // Impossible to distinguish between a real space and the &nbsp; that comes back for an empty table cell
					{
						sb.Append("	" + i.ErrorText + " " + i.Box);
					}
					else
					{
						sb.Append("	" + i.ErrorText + " ('" + i.Original + "') " + i.Box);
					}
				}
			}
			return sb.ToStringWithNewLineBetweenAppends();
		}

		public static string FormatPositionOfBadValue(string segmentId, string segmentNumber, string elementNumber, string compositeNumber)
		{   // e.g. NAD 14 5 1
			return segmentNumber != "0" ? string.Format("Seg {1} ({0}) {2}:{3}", segmentId, segmentNumber, elementNumber, compositeNumber) : "";
		}

		public static Edifact.D04A.Messages.CUSDEC.CUSDECMessage CreateCusdecFromOutboundMessage(EDIMessage outgoingMessage)
		{
			Edifact.D04A.Messages.CUSDEC.CUSDECMessage outgoingCusdec = null;

			try
			{
				UNCharacterSet charSetOfOutgoingMessage = new UNOACharacterSet();
				if (outgoingMessage.Interchange != null)
				{
					charSetOfOutgoingMessage = outgoingMessage.Interchange.GetCharacterSetFromInterchangeString();
				}
				else
				{
					charSetOfOutgoingMessage = outgoingMessage.EM_MessageText.StartsWith("UNH#") ? EdifactDelimiterConverter.McpDelimiters : EdifactDelimiterConverter.StandardDelimiters;
				}
				outgoingCusdec = (Edifact.D04A.Messages.CUSDEC.CUSDECMessage)(outgoingMessage.GetAutoEdifactMessageUsingNamedFactory(GbEdiMessageFactory.Factory, charSetOfOutgoingMessage));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{ }
			return outgoingCusdec;
		}

		public static int GetItemNumber(ZString cusdecText, int posOfSegmentToFind, string charSetSegmentDelimiter)
		{
			if (posOfSegmentToFind == 0)
			{
				return 0;  // header
			}

			int i = 1;

			try
			{
				// Chief cusdecs don't utilise CST.GoodsItemNumber
				// The branching diagram says that we'll have one header CST and then one to introduce each line item. So count 'em
				string cST = "CST";
				int[] lineNumbersOfCstSegments = cusdecText.FindLineNumbersWhichStartWithString(cST, charSetSegmentDelimiter);

				if (lineNumbersOfCstSegments.Length < 2)
				{
					return 0; // could not find at least 2 CSTs
				}
				if (lineNumbersOfCstSegments[1] > posOfSegmentToFind)
				{
					return 0; // before the first group 30, must be header
				}
				for (i = lineNumbersOfCstSegments.Length - 1; i > 0; i--)
				{
					if (posOfSegmentToFind < lineNumbersOfCstSegments[i])
					{
						continue;
					}
					return i;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{ }
			return -1;
		}
	}
}
