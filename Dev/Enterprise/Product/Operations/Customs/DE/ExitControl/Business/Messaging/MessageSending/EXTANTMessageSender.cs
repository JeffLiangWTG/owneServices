using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public sealed class EXTANTMessageSender : ExitControlMessageSender
	{
		public EXTANTMessageSender(ExitControlMessageSendingObject sendingObject) : base(sendingObject)
		{
		}

		protected override string MessageType => ExitDeclarationMessageBuilderLoader.ExitAnticipation;
	}
}
