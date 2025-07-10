using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.IT.Business.Declaration;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

sealed class H3MessageWrapper : IH3Message
{
	public H3MessageWrapper(CusEntryHeader entryHeader, IMessageSendingWrapperFactory messageSendingWrapperFactory, IDeclarationAmendment amendment = null)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		this.messageSendingWrapperFactory = Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));
		this.amendment = amendment;
	}

	readonly CusEntryHeader entryHeader;
	readonly IMessageSendingWrapperFactory messageSendingWrapperFactory;
	readonly IDeclarationAmendment amendment;

	#region IH3Message

	IH3Header IH3Message.Header => header ?? (header = new H3HeaderWrapper(entryHeader, messageSendingWrapperFactory, amendment));
	IH3Header header;

	IReadOnlyCollection<IH3Item> IH3Message.Items => items ?? (items = GetItems());
	IReadOnlyCollection<IH3Item> items;

	#endregion

	IReadOnlyCollection<IH3Item> GetItems()
	{
		return entryHeader.MergedLines
			.Select(l => new H3ItemWrapper(l, messageSendingWrapperFactory))
			.ToCollection();
	}
}
