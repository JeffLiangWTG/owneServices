using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class TemporaryStorageSendableCustomsEntry : ISendableCustomsEntry
{
	public TemporaryStorageSendableCustomsEntry(TemporaryStorageHeader header)
	{
		this.header = Argument.NotNull(header, nameof(header));
	}

	ZString ISendableCustomsEntry.CustomsProfile => header.AMA_CustomsProfile;

	void ISendableCustomsEntry.ConsumeGuarantee(BusinessObjectFactory factory, ITEDIMessage message)
	{
	}

	void ISendableCustomsEntry.MarkAsSent(IMessageType sentMessage)
	{
		header.AMA_MessageStatus = PNTSMessageStatusList.Codes.Sent;
	}

	void ISendableCustomsEntry.PreProcessBeforeSending()
	{
	}

	readonly TemporaryStorageHeader header;
}
