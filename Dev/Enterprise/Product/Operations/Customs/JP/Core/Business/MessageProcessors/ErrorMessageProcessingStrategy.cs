using CargoWise.Common;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Business;

sealed class ErrorMessageProcessingStrategy : IErrorMessageProcessingStrategy
{
	public ErrorMessageProcessingStrategy(CusEntryHeader header)
	{
		Header = Argument.NotNull(header, nameof(header));
	}

	CusEntryHeader Header { get; }

	void IErrorMessageProcessingStrategy.ProcessMessage(EDIMessage message)
	{
		if (message != null && message.EM_LinkUniqueID == Header.PK && message.EM_LinkTable == Header.TableName)
		{
			Header.CustomsStatus = CustomsStatusList.Codes.Error;
			Header.MessageStatus = CustomsStatusList.Codes.Error;
		}

		message.EM_Status = EDIMessage.Status.ProcessedOK;
	}
}
