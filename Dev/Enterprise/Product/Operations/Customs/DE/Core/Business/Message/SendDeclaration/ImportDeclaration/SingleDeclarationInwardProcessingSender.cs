using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class SingleDeclarationInwardProcessingSender : ImportDeclarationSender
	{
		public SingleDeclarationInwardProcessingSender(ImportEntryMessageSendingAction action)
			: base(action.MessagingObject, ImportDeclarationMessageBuilderLoader.SingleDeclarationOutwardProcessing, new SCIDECMessageHeaderProvider(action.MessagingObject))
		{
		}

		protected override bool PreSend()
		{
			ResetEntryStatus();
			return base.PreSend();
		}
	}
}
