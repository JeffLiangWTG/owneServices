using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.IT.Business.Declaration;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

sealed class I1MessageWrapper : II1Message
{
	public I1MessageWrapper(CusEntryHeader entryHeader, IMessageSendingWrapperFactory messageSendingWrapperFactory, IDeclarationAmendment amendment = null)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		this.messageSendingWrapperFactory = Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));
		this.amendment = amendment;
	}

	#region II1Message

	II1Header II1Message.Header => header ?? (header = new I1HeaderWrapper(entryHeader, messageSendingWrapperFactory, amendment));
	II1Header header;

	IReadOnlyCollection<II1Item> II1Message.Items => items ?? (items = GetItems());
	IReadOnlyCollection<II1Item> items;

	IReadOnlyCollection<II1Item> GetItems()
	{
		return entryHeader.MergedLines
			.Select(l => new I1ItemWrapper(l, messageSendingWrapperFactory))
			.ToCollection();
	}

	#endregion

	readonly CusEntryHeader entryHeader;
	readonly IMessageSendingWrapperFactory messageSendingWrapperFactory;
	readonly IDeclarationAmendment amendment;
}
