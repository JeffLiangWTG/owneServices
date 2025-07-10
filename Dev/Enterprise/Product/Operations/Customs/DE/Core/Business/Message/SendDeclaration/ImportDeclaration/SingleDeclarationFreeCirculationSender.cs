using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class SingleDeclarationFreeCirculationSender : ImportDeclarationSender
	{
		public SingleDeclarationFreeCirculationSender(ImportEntryMessageSendingAction action)
			: base(action.MessagingObject, ImportDeclarationMessageBuilderLoader.SingleDeclarationFreeCirculation, new CFCDECMessageHeaderProvider(action.MessagingObject))
		{
		}

		protected override bool PreSend()
		{
			ResetEntryStatus();
			return base.PreSend();
		}
	}
}
