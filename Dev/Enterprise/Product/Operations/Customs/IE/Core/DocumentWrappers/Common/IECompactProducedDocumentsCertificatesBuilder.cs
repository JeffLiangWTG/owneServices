using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.DocumentWrappers
{
	public class IECompactProducedDocumentsCertificatesBuilder : CompactProducedDocumentsCertificatesBuilder
	{
		protected override void AppendCountrySpecificFields(ZStringBuilder lineSb, SupportingDocument supportingDocument)
		{
			var codes = RefCusCodeListTypes.GetCachedList(supportingDocument.Factory,
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
				Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS,
				ZDateTime.Today);

			var supportingDocumentCode = supportingDocument.CSI_Code;
			var codeDescription = codes.GetDescriptionFromCode(supportingDocumentCode);

			lineSb.AppendIfNotEmpty(supportingDocument.CSI_LineNo.ToString());
			lineSb.AppendIfNotEmpty(string.IsNullOrEmpty(codeDescription) ? supportingDocumentCode : codeDescription);
			lineSb.AppendIfNotEmpty(supportingDocument.CSI_ReferenceNumber);
			lineSb.AppendIfNotEmpty(supportingDocument.CSI_ItemNumber.ToString());
			lineSb.AppendIfNotEmpty(supportingDocument.CSI_ReferenceNumber2);
		}

		protected override ZString DocumentsDelimiter => DocumentWrapperConstants.Delimiters.SemiColonAndspace;
	}
}
