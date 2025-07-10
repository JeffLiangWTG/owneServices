using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.IT.Business.Declaration;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

public sealed class H2MessageWrapper : IH2Message
{
	public H2MessageWrapper(CusEntryHeader entryHeader, IMessageSendingWrapperFactory messageSendingWrapperFactory, IDeclarationAmendment amendment = null)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		this.amendment = amendment;
		this.messageSendingWrapperFactory = Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));
	}

	readonly CusEntryHeader entryHeader;
	readonly IDeclarationAmendment amendment;
	readonly IMessageSendingWrapperFactory messageSendingWrapperFactory;

	IH2Header IH2Message.Header => header ?? (header = new H2HeaderWrapper(entryHeader, messageSendingWrapperFactory, amendment));
	IH2Header header;

	IReadOnlyCollection<IH2Item> IH2Message.Items => items ?? (items = GetItems());
	IReadOnlyCollection<IH2Item> items;

	IReadOnlyCollection<IH2Item> GetItems()
	{
		return entryHeader
			.MergedLines
			.Cast<CusEntryLine>()
			.Select(entryLine => new H2ItemWrapper(entryLine, messageSendingWrapperFactory))
			.ToCollection();
	}
}
