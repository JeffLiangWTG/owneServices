using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class DocumentProvider : IDocument
	{
		public DocumentProvider(CusSupportingInfo supportingInfo)
		{
			Type = supportingInfo.CSI_Code;
			Reference = supportingInfo.CSI_ReferenceNumber;
		}

		public string Type { get; }

		public string Reference { get; }
	}
}
