using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class StockTransferBondedWarehouseSender : ImportDeclarationSender
	{
		public StockTransferBondedWarehouseSender(ImportEntryMessageSendingAction action)
			: base(action.MessagingObject, ImportDeclarationMessageBuilderLoader.WarehouseStockTransfer, new CUSWATMessageHeaderProvider(action.MessagingObject))
		{
		}
	}
}
