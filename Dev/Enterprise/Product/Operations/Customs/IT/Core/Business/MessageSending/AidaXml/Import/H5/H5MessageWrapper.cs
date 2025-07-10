using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.IT.Business.Declaration;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

public sealed class H5MessageWrapper : IH5Message
{
	public H5MessageWrapper(CusEntryHeader entryHeader, IMessageSendingWrapperFactory messageSendingWrapperFactory, IDeclarationAmendment amendment = null)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));
		Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));

		lazyHeader = new Lazy<IH5Header>(() => new H5HeaderWrapper(entryHeader, messageSendingWrapperFactory, amendment));
		lazyItems = new Lazy<IReadOnlyCollection<IH5Item>>(() => entryHeader.MergedLines.Select(x => new H5ItemWrapper(x, messageSendingWrapperFactory)).ToCollection());
	}

	IH5Header IH5Message.Header => lazyHeader.Value;
	readonly Lazy<IH5Header> lazyHeader;

	IReadOnlyCollection<IH5Item> IH5Message.Items => lazyItems.Value;
	readonly Lazy<IReadOnlyCollection<IH5Item>> lazyItems;
}
