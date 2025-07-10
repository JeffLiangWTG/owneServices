using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	sealed class CompactProducedDocumentsCertificatesBuilderForTest : CompactProducedDocumentsCertificatesBuilder
	{
		public void AppendSupportingDocument_Exposed(ZStringBuilder lineSb, SupportingDocument supportingDocument, bool isPhase5Departure) => base.AppendSupportingDocumentCore(lineSb, supportingDocument, isPhase5Departure);

		public void AppendCountrySpecificFields_Exposed(ZStringBuilder lineSb, SupportingDocument supportingDocument) => base.AppendCountrySpecificFields(lineSb, supportingDocument);
	}
}
