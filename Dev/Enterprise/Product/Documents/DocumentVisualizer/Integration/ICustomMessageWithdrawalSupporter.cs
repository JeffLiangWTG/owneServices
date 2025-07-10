using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface ICustomMessageWithdrawalSupporter
	{
		object GetMessageWithdrawalReason();
		bool PopulateMessageWithdrawalReason(IDataObject dataObject, object reasonForSending);
	}
}
