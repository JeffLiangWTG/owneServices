using CargoWise.Common;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class PreviousDocumentWrapper : CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces.IPreviousDocument
	{
		PreviousDocumentWrapper(CusSupportingInfo supportingInfo)
		{
			this.supportingInfo = Argument.NotNull(supportingInfo, nameof(supportingInfo));
		}

		readonly CusSupportingInfo supportingInfo;

		public string GoodsItemIdentifier => goodsItemIdentifier ?? (goodsItemIdentifier = supportingInfo.CSI_LineNo.ToString());
		string goodsItemIdentifier;

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = supportingInfo.CSI_ReferenceNumber);
		string referenceNumber;

		public string Type => type ?? (type = supportingInfo.CSI_Code);
		string type;

		public static PreviousDocumentWrapper New(CusSupportingInfo supportingInfo) => supportingInfo == null ? null : new PreviousDocumentWrapper(supportingInfo);
	}
}
