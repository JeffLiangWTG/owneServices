using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.IT.Business.Declaration;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

sealed class I2MessageWrapper : II2Message
{
	public I2MessageWrapper(CusEntryHeader entryHeader, IMessageSendingWrapperFactory messageSendingWrapperFactory)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		this.messageSendingWrapperFactory = Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));
	}

	readonly CusEntryHeader entryHeader;
	readonly IMessageSendingWrapperFactory messageSendingWrapperFactory;

	II2Header II2Message.Header => header ?? (header = new I2HeaderWrapper(entryHeader, messageSendingWrapperFactory));
	II2Header header;

	IReadOnlyCollection<II2Item> II2Message.Items => items ?? (items = GetItems());
	IReadOnlyCollection<II2Item> items;

	#region Implementation

	IReadOnlyCollection<II2Item> GetItems()
	{
		return entryHeader.MergedLines
		.Select(x => new I2ItemWrapper(x, messageSendingWrapperFactory))
		.ToCollection();
	}

	#endregion
}
