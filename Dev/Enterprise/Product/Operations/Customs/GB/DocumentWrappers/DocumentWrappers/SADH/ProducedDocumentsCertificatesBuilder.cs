using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.DocumentWrappers
{
	public class ProducedDocumentsCertificatesBuilder : Enterprise.DocumentWrappers.Customs.EU.ProducedDocumentsCertificatesBuilder
	{
		protected override ZString GetSupportingDocumentCode(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument supportingDocument) =>
			supportingDocument is SupportingDocument d ? $"{d.CSI_Code}-[{d.CSI_Availability}{d.CSI_Actions}]" : base.GetSupportingDocumentCode(supportingDocument);
	}
}
