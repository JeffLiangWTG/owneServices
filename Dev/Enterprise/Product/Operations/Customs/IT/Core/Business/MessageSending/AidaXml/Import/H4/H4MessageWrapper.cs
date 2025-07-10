using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.IT.Business.Declaration;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

sealed class H4MessageWrapper : IH4Message
{
	public H4MessageWrapper(CusEntryHeader entryHeader, IMessageSendingWrapperFactory messageSendingWrapperFactory, IDeclarationAmendment amendment = null)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		this.messageSendingWrapperFactory = Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));
		this.amendment = amendment;
	}

	readonly CusEntryHeader entryHeader;
	readonly IMessageSendingWrapperFactory messageSendingWrapperFactory;
	readonly IDeclarationAmendment amendment;

	IH4Header IH4Message.Header => header ?? (header = new H4HeaderWrapper(entryHeader, messageSendingWrapperFactory, amendment));
	IH4Header header;

	IReadOnlyCollection<IH4Item> IH4Message.Items => items ?? (items = GetItems());
	IReadOnlyCollection<IH4Item> items;

	IReadOnlyCollection<IH4Item> GetItems()
	{
		return entryHeader.MergedLines
			.Cast<CusEntryLine>()
			.Select(x => new H4ItemWrapper(x, messageSendingWrapperFactory))
			.Cast<IH4Item>()
			.ToCollection();
	}
}
