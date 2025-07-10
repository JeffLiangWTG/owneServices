using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.IT.Business.Declaration;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

public class H1MessageWrapper : IH1Message
{
	public H1MessageWrapper(CusEntryHeader entryHeader, IMessageSendingWrapperFactory messageSendingWrapperFactory, IDeclarationAmendment amendment = null)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		this.messageSendingWrapperFactory = Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));
		this.amendment = amendment;
	}

	readonly CusEntryHeader entryHeader;
	readonly IMessageSendingWrapperFactory messageSendingWrapperFactory;
	readonly IDeclarationAmendment amendment;

	IH1Header IH1Message.MessageHeader => messageHeader ?? (messageHeader = new H1HeaderWrapper(entryHeader, messageSendingWrapperFactory, amendment));
	IH1Header messageHeader;

	IReadOnlyCollection<IH1Item> IH1Message.Items => items ?? (items = GetItems().ToCollection());
	IReadOnlyCollection<IH1Item> items;

	IEnumerable<IH1Item> GetItems()
	{
		return entryHeader
			.MergedLines
			.Cast<CusEntryLine>()
			.Select(entryLine => new H1ItemWrapper(entryLine, messageSendingWrapperFactory));
	}
}
