using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using EUDocumentWrapperConstants = Enterprise.DocumentWrappers.Customs.EU.DocumentWrapperConstants;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	public class CompactProducedDocumentsCertificatesBuilder : ProducedDocumentsCertificatesBuilder
	{
		protected override void AppendSupportingDocumentCore(ZStringBuilder lineSb, SupportingDocument supportingDocument, bool isPhase5Departure = false)
		{
			if (isPhase5Departure)
			{
				AppendSupportingDocumentFieldsForPhase5(lineSb, supportingDocument);
			}
			else
			{
				AppendCountrySpecificFields(lineSb, supportingDocument);
			}
		}

		protected virtual void AppendCountrySpecificFields(ZStringBuilder lineSb, SupportingDocument supportingDocument)
		{
			var codes = RefCusCodeListTypes.GetCachedList(supportingDocument.Factory,
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
				Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS,
				ZDateTime.Today);

			var supportingDocumentCode = supportingDocument.CSI_Code;
			var codeDescription = codes.GetDescriptionFromCode(supportingDocumentCode);

			lineSb.AppendIfNotEmpty(string.IsNullOrEmpty(codeDescription) ? supportingDocumentCode : codeDescription);
			lineSb.AppendIfNotEmpty(supportingDocument.CSI_ReferenceNumber);
			lineSb.AppendIfNotEmpty(supportingDocument.CSI_Description);
		}

		void AppendSupportingDocumentFieldsForPhase5(ZStringBuilder lineSb, SupportingDocument supportingDocument)
		{
			lineSb.AppendIfNotEmpty(supportingDocument.CSI_Code);
			lineSb.AppendIfNotEmpty(supportingDocument.CSI_ReferenceNumber);
		}

		protected override ZString DocumentsDelimiter => EUDocumentWrapperConstants.Delimiters.SemiColonAndspace;

		protected override ZString FieldDelimiter => EUDocumentWrapperConstants.Delimiters.Dash;
	}
}
