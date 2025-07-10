using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public sealed class EXTINFMessageSender : ExitControlMessageSender
	{
		public EXTINFMessageSender(ExitControlMessageSendingObject sendingObject) : base(sendingObject)
		{
		}

		protected override string MessageType => ExitDeclarationMessageBuilderLoader.ExitInformation;
	}
}
