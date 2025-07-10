using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.DocumentWrappers.Customs.EU;

namespace Enterprise.Customs.ES.DocumentWrappers.NCTS
{
	public class ESCompactPreviousDocumentsBuilder : CompactPreviousDocumentsBuilder
	{
		protected override IEnumerable<(ZString separator, ZString value)> GetCountrySpecificFields(PreviousDocument prevDoc)
		{
			var subType = prevDoc.CSI_SubType;
			yield return ("", subType);
			yield return (" ", prevDoc.CSI_Code);
			yield return (" ", prevDoc.CSI_ReferenceNumber);

			var lineNo = prevDoc.CSI_LineNo;

			if (subType == PreviousDocumentClassList.Codes.SummaryDeclaration && !lineNo.IsEmpty)
			{
				yield return (" ", lineNo.ToString());
			}
		}
	}
}
