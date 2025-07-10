using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Edifact.Auto;

namespace Enterprise.Customs.CA.Messaging
{
	public static class EdifactMessageExtension
	{
		public static ZString GetOriginalMessageNo(this SegmentGroup edifactMessage)
		{
			var originalMessageNo = ZString.Empty;
			if (edifactMessage is Edifact.D11B.Messages.GOVCBR.GOVCBRMessage d11bGovcbr)
			{
				originalMessageNo = GetOriginalMessageNo(d11bGovcbr);
			}
			else if (edifactMessage is Edifact.D00A.Messages.CUSRES.CUSRESMessage d00aCusres)
			{
				originalMessageNo = GetOriginalMessageNo(d00aCusres);
			}
			else if (edifactMessage is Edifact.D99B.Messages.CUSRES.CUSRESMessage d99bCusres)
			{
				originalMessageNo = GetOriginalMessageNo(d99bCusres);
			}
			return originalMessageNo;
		}

		static ZString GetOriginalMessageNo(Edifact.D11B.Messages.GOVCBR.GOVCBRMessage d11bGovcbr)
		{
			var result = ZString.Empty;
			var ftxMessageText = (from Edifact.D11B.Messages.GOVCBR.SegmentGroup13 group13 in d11bGovcbr.Group13
								  from Edifact.D11B.Segments.FTXSegment ftx in group13.FTX
								  where ftx.TextSubjectCodeQualifier == Enterprise.Edifact.D11B.Elements.TextSubjectCodeQualifierList.ErrorDescriptionFreeText
								  select ftx.TextLiteral.FreeText1
								 into errorText
								  select errorText).FirstOrDefault();
			var match = Regex.Match(ftxMessageText, @"^UMRN\((?<Msgnum>[0-9]+)\)SEGMENT(\w+)LINE([0-9]+)(ELE\sPOS|ELEM[0-9]+\()(([0-9]+)[,.]([0-9]+))\)?(.+)$");
			if (match.Success)
			{
				result = match.Groups["Msgnum"].Value;
			}
			return result;
		}

		static ZString GetOriginalMessageNo(Edifact.D00A.Messages.CUSRES.CUSRESMessage d00aCusres)
		{
			return (from Edifact.D00A.Messages.CUSRES.SegmentGroup4 group4 in d00aCusres.Group4
					from Edifact.D00A.Segments.ERPSegment erp in group4.ERP
					select erp.ErrorPointDetails.MessageItemIdentifier
					into msgNumber
					select msgNumber).FirstOrDefault();
		}

		static ZString GetOriginalMessageNo(Edifact.D99B.Messages.CUSRES.CUSRESMessage d99bCusres)
		{
			return (from Edifact.D99B.Messages.CUSRES.SegmentGroup4 group4 in d99bCusres.Group4
					from Edifact.D99B.Segments.ERPSegment erp in group4.ERP
					select erp.ErrorPointDetails.MessageItemNumber
					into msgNumber
					select msgNumber).FirstOrDefault();
		}

		public static IEnumerable<SyntaxError> GetSyntaxErrors(this SegmentGroup edifactMessage, ZString sourceMessageText)
		{
			var synTaxErrors = Enumerable.Empty<SyntaxError>();
			if (edifactMessage is Edifact.D11B.Messages.GOVCBR.GOVCBRMessage d11bGovcbr)
			{
				synTaxErrors = from Edifact.D11B.Messages.GOVCBR.SegmentGroup13 group13 in d11bGovcbr.Group13
							   from Edifact.D11B.Segments.FTXSegment ftx in group13.FTX
							   where ftx.TextSubjectCodeQualifier == Enterprise.Edifact.D11B.Elements.TextSubjectCodeQualifierList.ErrorDescriptionFreeText
							   select ftx.TextLiteral.FreeText1
				into errorText
							   select new SyntaxError(sourceMessageText, errorText);
			}
			else if (edifactMessage is Edifact.D00A.Messages.CUSRES.CUSRESMessage d00aCusres)
			{
				synTaxErrors = from Edifact.D00A.Messages.CUSRES.SegmentGroup4 group4 in d00aCusres.Group4
							   from Edifact.D00A.Segments.FTXSegment ftx in group4.FTX
							   where ftx.TextSubjectCodeQualifier == Enterprise.Edifact.D00A.Elements.TextSubjectCodeQualifierList.ErrorDescriptionFreeText
							   select ftx.TextLiteral.FreeTextValue1 + ftx.TextLiteral.FreeTextValue2
				into errorText
							   select new SyntaxError(sourceMessageText, errorText);
			}
			else if (edifactMessage is Edifact.D99B.Messages.CUSRES.CUSRESMessage d99bCusres)
			{
				synTaxErrors = from Edifact.D99B.Messages.CUSRES.SegmentGroup4 group4 in d99bCusres.Group4
							   from Edifact.D99B.Segments.FTXSegment ftx in group4.FTX
							   where ftx.TextSubjectCodeQualifier == Enterprise.Edifact.D99B.Elements.TextSubjectCodeQualifierList.ErrorDescriptionFreeText
							   select ftx.TextLiteral.FreeTextValue1 + ftx.TextLiteral.FreeTextValue2
				into errorText
							   select new SyntaxError(sourceMessageText, errorText);
			}
			return synTaxErrors;
		}
	}
}
