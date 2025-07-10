using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.FR.DocumentWrappers;

public class CompactPreviousDocumentsBuilder : Enterprise.DocumentWrappers.Customs.EU.CompactPreviousDocumentsBuilder
{
	protected override IEnumerable<(ZString separator, ZString value)> GetCountrySpecificFields(PreviousDocument prevDoc)
	{
		yield return ("", prevDoc.CSI_SubType);
		yield return (FieldDelimiter, prevDoc.CSI_Code);
		yield return (FieldDelimiter, prevDoc.CSI_Procedure);
		yield return (FieldDelimiter, prevDoc.CSI_ReferenceNumber);
		yield return (FieldDelimiter, prevDoc.CSI_ReferenceNumber2);
		yield return (" ", prevDoc.CSI_Status);
		yield return (FieldDelimiter, prevDoc.CSI_DateOfIssue.IsValid ? prevDoc.DateOfIssueInFormat : ZString.Empty);
		yield return (FieldDelimiter, prevDoc.CSI_CustomsOffice);

		var lineNo = prevDoc.CSI_LineNo;

		if (!lineNo.IsEmpty)
		{
			yield return (FieldDelimiter, lineNo.ToString());
		}
	}
}
