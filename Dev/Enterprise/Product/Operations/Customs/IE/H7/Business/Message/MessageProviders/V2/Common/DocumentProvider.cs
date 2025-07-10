using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.H7.Business
{
	public class DocumentProvider : IDocument
	{
		public DocumentProvider(AsycudaBill bill)
		{
			Type = TransportDocumentCodes._N703;
			Reference = bill.ABL_BillNumber;
		}

		public DocumentProvider(CusSupportingInfo supportingInfo)
		{
			Type = supportingInfo.CSI_Code;
			Reference = supportingInfo.CSI_ReferenceNumber;
		}

		public string Type { get; }

		public string Reference { get; }
	}
}
