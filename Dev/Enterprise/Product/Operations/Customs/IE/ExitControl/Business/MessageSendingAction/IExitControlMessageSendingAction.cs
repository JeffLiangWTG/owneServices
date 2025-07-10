using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public interface IExitControlMessageSendingAction : IMessageSendingAction
	{
		CusExitReport MessagingObject { get; }
	}
}
