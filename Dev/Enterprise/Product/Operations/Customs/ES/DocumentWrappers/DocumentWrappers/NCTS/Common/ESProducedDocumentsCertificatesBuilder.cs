using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.DocumentWrappers.Customs.EU;

namespace Enterprise.Customs.ES.DocumentWrappers.NCTS
{
	public class ESProducedDocumentsCertificatesBuilder : ProducedDocumentsCertificatesBuilder
	{
		protected override void AppendSupportingDocumentCore(ZStringBuilder lineSb, SupportingDocument supportingDocument, bool isPhase5Departure = false)
		{
			lineSb.AppendIfNotEmpty(supportingDocument.CSI_Code + ":");
			lineSb.AppendIfNotEmpty(supportingDocument.CSI_ReferenceNumber);
		}
		protected override ZString DocumentsDelimiter => DocumentWrapperConstants.Delimiters.SemiColonAndspace;
	}
}
