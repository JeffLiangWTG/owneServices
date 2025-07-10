using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.AES
{
	public class TransportDocumentProvider : CusSupportingInfoProvider, IDocument
	{
		public TransportDocumentProvider(CusSupportingInfo additionalReference) : base(additionalReference) { }

		public string Type => cusSupportingInfo.CSI_Code;

		public string Reference => cusSupportingInfo.CSI_ReferenceNumber;
	}
}
