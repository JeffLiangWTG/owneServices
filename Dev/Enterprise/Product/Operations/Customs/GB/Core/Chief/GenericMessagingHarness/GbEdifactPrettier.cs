using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Common;
using Enterprise.Customs.GB.Chief.EdiFact.UKCINV;
using Enterprise.Edifact;
using Enterprise.Edifact.D04A.Messages.CUSDEC;
using Enterprise.Edifact.D04A.Messages.CUSRES;
using Enterprise.Edifact.D04A.Segments;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.Chief
{
	public class GbEdifactPrettier
	{
		protected ZString input;
		readonly UNCharacterSet characterSet;

		public GbEdifactPrettier(ZString input, UNCharacterSet characterSet, Customs.Business.CusdecMessageFunction how)
		{
			this.how = how;
			this.input = input;
			this.characterSet = characterSet;
		}

		public ZString MakeHumanReadable(bool showNumberedEdifact = true)
		{
			object d04AMessage = GbEdiMessageFactory.Factory.GetMessage(characterSet, input);
			CUSDECMessage cusDec = d04AMessage as CUSDECMessage;
			CUSRESMessage cusRes = d04AMessage as CUSRESMessage;

			if (cusDec != null)
			{
				string tableHtml = PrettifyCusdec(cusDec);
				if (showNumberedEdifact)
				{
					string numberedLines = GetSegmentsAsLinesAndNumberThem();
					return string.Format(CultureInfo.InvariantCulture, "{0} {1} <pre>{2} </pre>", MessagePrettierCss.CSS, tableHtml, numberedLines);
				}
				else
				{
					return string.Format(CultureInfo.InvariantCulture, "{0} {1}", MessagePrettierCss.CSS, tableHtml);
				}
			}
			else if (cusRes != null)
			{
				HtmlTableCreator table = new HtmlTableCreator(new string[] { "Key", "Value" });
				table.WriteRow("Message reference", cusRes.UNH[0].MessageReferenceNumber);
				table.WriteRow("BGM", cusRes.BGM[0].DocumentMessageName.DocumentNameCode); //etc....
				return table.ToHtml();
			}
			else
			{   // maybe CINV
				object d00AMessage = GbEdiMessageFactory.Factory.GetMessage(characterSet, input);
				UkCinvMessage cinv = d00AMessage as UkCinvMessage;
				if (cinv != null)
				{
					return ExplainCinv(MessagePrettierCss.CSS, cinv);
				}
			}

			return input;
		}

		string GetSegmentsAsLinesAndNumberThem()
		{
			string[] lines = EdifactSegmentPuller.GetArrayOfItemsBySplittingOnDelimiter(characterSet.SegmentDelimiter, characterSet.EscapeCharacter, input);
			return NumberLines(lines);
		}

		static ZString PrettifyCusdec(CUSDECMessage cUSDEC)
		{
			HtmlTableCreator table = new HtmlTableCreator(new string[] { "Field", "Value" });
			table.WriteRow("Message serial number", cUSDEC.UNH[0].MessageReferenceNumber);
			table.WriteRow("BGM type", cUSDEC.BGM[0].DocumentMessageName.DocumentNameCode.ToString());  //etc....
			foreach (Edifact.D04A.Messages.CUSDEC.SegmentGroup1 grp1 in cUSDEC.Group1)
			{
				foreach (RFFSegment rff in grp1.RFF)
				{
					string mainRef = rff.Reference.ReferenceIdentifier;
					string subRef = rff.Reference.DocumentLineIdentifier;
					string overallRef = string.Format("{0} {1}", mainRef, subRef);
					string refMeaning = GetReferenceMeaning(rff.Reference.ReferenceCodeQualifier.ToString());
					table.WriteRow("Ref/" + refMeaning, overallRef);
				}
			}
			foreach (FTXSegment ftx in cUSDEC.FTX)
			{
				string spaceNewLine = System.Environment.NewLine;
				table.WriteRow("Comment/" + ftx.TextSubjectCodeQualifier.ToString(), ftx.TextLiteral.FreeText1 +
																						spaceNewLine + ftx.TextLiteral.FreeText2 +
																						spaceNewLine + ftx.TextLiteral.FreeText3 +
																						spaceNewLine + ftx.TextLiteral.FreeText4 +
																						spaceNewLine + ftx.TextLiteral.FreeText5);
			}
			return table.ToHtml();
		}

		static string GetReferenceMeaning(string rffCode)
		{
			switch (rffCode)
			{
				case ChiefConstants.RffSegmentIdentifiers.ABT:
					return "Entry number & version";
				case ChiefConstants.RffSegmentIdentifiers.AAE:
					return "MRN";
				case ChiefConstants.RffSegmentIdentifiers.ABO:
					return "DUCR & part (w/checksum)";
				case ChiefConstants.RffSegmentIdentifiers.UCN:
					return "MUCR";
				case ChiefConstants.RffSegmentIdentifiers.ABS:
					return "ICS";
				case ChiefConstants.RffSegmentIdentifiers.AHZ:
					return "Style & route of entry";
				case ChiefConstants.RffSegmentIdentifiers.AES:
					return "Movement number";
				case ChiefConstants.RffSegmentIdentifiers.ABI:
					return "First DAN & prefix";
				case ChiefConstants.RffSegmentIdentifiers.DA:
					return "Second DAN & prefix";
			}
			return string.Empty;
		}

		string NumberLines(string[] lines)
		{
			string numberedLines = string.Empty;
			int i = 0;
			foreach (ZString line in lines)
			{
				i++;
				if (!line.IsEmpty)
				{
					numberedLines += string.Format("{0}	{1}{2}{3}", i, line, this.characterSet.SegmentDelimiter, System.Environment.NewLine);
				}
			}
			return numberedLines;
		}

		ZString ExplainCinv(string css, UkCinvMessage cinv)
		{
			string numberedLines = GetSegmentsAsLinesAndNumberThem();
			string explanation = null;
			GbDes242MessageFunction des242How = how as GbDes242MessageFunction;
			string interpretation = "";
			if (des242How != null)
			{
				explanation = des242How.FunctionHuman;
				interpretation = (new UkcinvUnderstander(cinv)).GetInterpretation("Request");
			}
			return string.Format("{0} <h2>{1}</h2> {3} <pre>{2} </pre>", css, explanation, numberedLines, interpretation);
		}

		readonly Customs.Business.CusdecMessageFunction how;
	}
}
