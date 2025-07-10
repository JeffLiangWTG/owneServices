using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public sealed class CustomsWarehouseGoodsItemProvider : ICustomsWarehouseGoodsItem
	{
		public static CustomsWarehouseGoodsItemProvider NewOrNull(PreviousDocument previousDocument) => previousDocument == null ? null : new CustomsWarehouseGoodsItemProvider(previousDocument);

		CustomsWarehouseGoodsItemProvider(PreviousDocument previousDocument)
		{
			this.previousDocument = previousDocument;
		}
		readonly PreviousDocument previousDocument;

		public string ReferencedRegistrationNumber => previousDocument.CSI_ReferenceNumber;

		public int ReferencedSequenceNumber => previousDocument.CSI_LineNo;

		public bool AccessViaATLASFlag => previousDocument.Status;

		public string CommodityCode => previousDocument.CSI_Tariff;

		public bool UsualProcessingFlag => usualProcessingFlag;

		public string Complement => previousDocument.CSI_Description;

		public IAmount CommercialAmount => factory.GetValue(ref commercialAmountCached, () => usualProcessingFlag && commercialQuantity > 0 ? new AmountProvider(previousDocument.CSI_Quantity, previousDocument.CSI_UnitOfQuantity) : null);
		CachedProperty<IAmount> commercialAmountCached;

		public IAmount DebitAmount => factory.GetValue(ref debitAmountCached, () => new AmountProvider(previousDocument.CSI_Quantity2, previousDocument.CSI_UnitOfQuantity2));
		CachedProperty<IAmount> debitAmountCached;

		BusinessObjectFactory factory => previousDocument.Factory;

		bool usualProcessingFlag => previousDocument.UsualProcessingFlag;

		decimal commercialQuantity => previousDocument.CSI_Quantity;
	}
}
