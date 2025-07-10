using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public sealed class EXTNOTMessageSender : ExitControlMessageSender
	{
		public EXTNOTMessageSender(ExitControlMessageSendingObject sendingObject) : base(sendingObject)
		{
		}

		protected override string MessageType => ExitDeclarationMessageBuilderLoader.ExitNotification;
	}
}
