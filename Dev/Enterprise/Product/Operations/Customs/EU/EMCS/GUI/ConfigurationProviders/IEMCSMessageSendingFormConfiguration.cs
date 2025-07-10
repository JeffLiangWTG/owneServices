using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public interface IEMCSMessageSendingFormConfiguration
	{
		bool IsOKToSend<TSendingAction>(EMCSMessageSendingActionParent<TSendingAction> parent) where TSendingAction : EMCSMessageSendingAction;
	}
}
