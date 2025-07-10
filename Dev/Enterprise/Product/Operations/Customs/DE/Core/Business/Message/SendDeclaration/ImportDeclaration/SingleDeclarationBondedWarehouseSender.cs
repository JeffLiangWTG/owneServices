using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class SingleDeclarationBondedWarehouseSender : ImportDeclarationSender
	{
		public SingleDeclarationBondedWarehouseSender(ImportEntryMessageSendingAction action)
			: base(action.MessagingObject, ImportDeclarationMessageBuilderLoader.SingleDeclarationIntoBondedWarehouse, new SCWDECMessageHeaderProvider(action.MessagingObject))
		{
		}

		protected override bool PreSend()
		{
			ResetEntryStatus();
			return base.PreSend();
		}
	}
}
