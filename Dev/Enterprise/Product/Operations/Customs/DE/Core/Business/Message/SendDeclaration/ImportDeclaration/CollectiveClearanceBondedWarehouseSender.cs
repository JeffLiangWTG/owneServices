using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class CollectiveClearanceBondedWarehouseSender : ImportDeclarationSender
	{
		public CollectiveClearanceBondedWarehouseSender(ImportEntryMessageSendingAction action)
			: base(action.MessagingObject, ImportDeclarationMessageBuilderLoader.CollectiveClearanceBondedWarehouse, new ECWCCMMessageHeaderProvider(action.MessagingObject))
		{
		}

		protected override bool PreSend()
		{
			var shouldSend = base.PreSend();
			if (shouldSend)
			{
				if (entryHeader.EntryInstruction.CEI_OA_Warehouse.IsEmpty)
				{
					shouldSend = true;
				}
				else if (entryHeader.CH_WarehouseTransactionStatus.IsEmpty)
				{
					entryHeader.Declaration.MessageInitiator.NotifyUserOfAnInvalidOperation(Res.GetString("376c8ebf-9f76-4118-b7d5-bf0788fe277e", "There is no warehouse transaction created for this entry and 'From Warehouse' is provided."));
					shouldSend = false;
				}
			}
			return shouldSend;
		}
	}
}
