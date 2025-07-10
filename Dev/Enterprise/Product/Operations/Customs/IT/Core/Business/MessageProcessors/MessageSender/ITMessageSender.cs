using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business;

public class ITMessageSender
{
	public ITMessageSender(BusinessObjectFactory factory, IOutgoingCustomsMessageCreationStrategy messageCreationStrategy, ISendableCustomsEntry sendableEntry)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		this.messageCreationStrategy = Argument.NotNull(messageCreationStrategy, nameof(messageCreationStrategy));
		this.sendableEntry = Argument.NotNull(sendableEntry, nameof(sendableEntry));
	}

	readonly BusinessObjectFactory factory;
	readonly IOutgoingCustomsMessageCreationStrategy messageCreationStrategy;
	readonly ISendableCustomsEntry sendableEntry;

	public ITEDIMessage Send()
	{
		sendableEntry.PreProcessBeforeSending();
		var message = messageCreationStrategy.GenerateMessage();
		sendableEntry.MarkAsSent(message);
		sendableEntry.ConsumeGuarantee(factory, message);

		return message;
	}
}
