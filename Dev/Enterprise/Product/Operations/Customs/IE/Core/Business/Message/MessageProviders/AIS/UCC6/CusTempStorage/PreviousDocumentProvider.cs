using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class PreviousDocumentProvider : DocumentProvider, IPreviousDocument
	{
		public static new PreviousDocumentProvider New(CusSupportingInfo supportingInfo, int sequenceNumber)
		{
			PreviousDocumentProvider result = null;
			if (supportingInfo != null)
			{
				result = new PreviousDocumentProvider(supportingInfo.CSI_LineNo.ToString(), supportingInfo.CSI_Code, supportingInfo.CSI_ReferenceNumber, sequenceNumber.ToString());
			}
			return result;
		}

		PreviousDocumentProvider(string goodsItemIdentifier, string type, string referenceNumber, string sequenceNumber) : base(type, referenceNumber, sequenceNumber)
		{
			GoodsItemIdentifier = goodsItemIdentifier;
		}

		public string GoodsItemIdentifier { get; }
	}
}
