using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public sealed class EXTPREMessageSender : ExitControlMessageSender
	{
		public EXTPREMessageSender(ExitControlMessageSendingObject sendingObject) : base(sendingObject)
		{
		}

		protected override string MessageType => ExitDeclarationMessageBuilderLoader.ExitPresentation;
	}
}
