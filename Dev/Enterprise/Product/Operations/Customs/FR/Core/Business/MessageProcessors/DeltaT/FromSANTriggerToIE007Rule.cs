using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FromSANTriggerToIE007Rule : IAutoSendNCTSMessageRule
	{
		public NctsMessageFunctionSet NewMessageFunction => new NctsMessageFunctionSet.ArrivalNotificationMessage();

		public ZBool CanSendMessage(NctsHeader header) => true;
	}
}
