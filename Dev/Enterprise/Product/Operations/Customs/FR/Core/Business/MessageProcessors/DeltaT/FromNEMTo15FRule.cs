using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FromNEMTo15FRule : IAutoSendNCTSMessageRule
	{
		public NctsMessageFunctionSet NewMessageFunction => new NctsMessageFunctionSet.DeclarationDataMessage();

		public ZBool CanSendMessage(NctsHeader header)
		{
			return header.DetailedDepartureStatusCode == NctsDetailedStatusList.Codes.BoardingNotification;
		}
	}
}
