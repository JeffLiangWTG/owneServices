using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public interface IAutoSendNCTSMessageRule
	{
		ZBool CanSendMessage(NctsHeader header);
		NctsMessageFunctionSet NewMessageFunction { get; }
	}
}
