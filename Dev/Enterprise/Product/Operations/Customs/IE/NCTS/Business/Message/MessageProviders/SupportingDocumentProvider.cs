using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class SupportingDocumentProvider : DocumentProvider, ISupportingDocument
	{
		public SupportingDocumentProvider(CusSupportingInfo supportingDocument) : base(supportingDocument)
		{
		}

		public int LineItemNumber => document.CSI_ItemNumber;

		public string ComplementOfInformation => document.CSI_ReferenceNumber2;
	}
}
