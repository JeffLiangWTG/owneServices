using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class InwardProcessingGoodsItemProvider : IInwardProcessingGoodsItem
	{
		public static InwardProcessingGoodsItemProvider NewOrNull(PreviousDocument previousDocument) => previousDocument == null ? null : new InwardProcessingGoodsItemProvider(previousDocument);

		InwardProcessingGoodsItemProvider(PreviousDocument previousDocument)
		{
			this.previousDocument = previousDocument;
		}
		readonly PreviousDocument previousDocument;

		public string ReferencedRegistrationNumber => previousDocument.CSI_ReferenceNumber;

		public int ReferencedSequenceNumber => previousDocument.CSI_LineNo;

		public bool AccessViaAtlasFlag => previousDocument.Status;

		public string GoodsRelatedInformation => previousDocument.CSI_Description;
	}
}
