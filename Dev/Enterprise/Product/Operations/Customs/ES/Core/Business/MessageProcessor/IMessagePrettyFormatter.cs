using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public interface IMessagePrettyFormatter
	{
		ZString CreateMessageDetailsAccepted(string extraDataFromProcessing);
		ZString CreateMessageDetailsRejected();
	}
}
