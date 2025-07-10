using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SummaryDeclarationGoodsItemProvider : ISummaryDeclarationGoodsItem
	{
		public static SummaryDeclarationGoodsItemProvider NewOrNull(PreviousDocument previousDocument) => previousDocument == null ? null : new SummaryDeclarationGoodsItemProvider(previousDocument);

		SummaryDeclarationGoodsItemProvider(PreviousDocument previousDocument)
		{
			this.previousDocument = previousDocument;
		}
		readonly PreviousDocument previousDocument;

		public int Quantity => previousDocument.CSI_Quantity.ToZInt();

		public string IdentificationByKeyKind => previousDocument.CSI_SubType;

		public string IdentificationByKeyNumber => previousDocument.CSI_ReferenceNumber;

		public string IdentificationByKeyCustodianIdentifier => previousDocument.CSI_ReferenceNumber2;

		public string IdentificationByRegistrationReferencedRegistrationNumber => previousDocument.CSI_ReferenceNumber;

		public int IdentificationByRegistrationReferencedSequenceNumber => previousDocument.CSI_LineNo;
	}
}
