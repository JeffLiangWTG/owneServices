using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class B4MessageWrapper : IB4Message
{
	public B4MessageWrapper(CusEntryHeader entryHeader, CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		this.exportMessageSendingWrapperFactory = Argument.NotNull(exportMessageSendingWrapperFactory, nameof(exportMessageSendingWrapperFactory));
	}

	public B4MessageWrapper(CusEntryHeader entryHeader, CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory, IDeclarationAmendment amendment)
		: this(entryHeader, exportMessageSendingWrapperFactory)
	{
		this.amendment = amendment;
	}

	readonly CusEntryHeader entryHeader;
	readonly CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory;
	readonly IDeclarationAmendment amendment;

	#region IB4Message

	IB4Header IB4Message.MessageHeader => messageHeader ?? (messageHeader = new B4HeaderWrapper(entryHeader, exportMessageSendingWrapperFactory, amendment));
	IB4Header messageHeader;

	IReadOnlyCollection<IB4Item> IB4Message.Items => items ?? (items = GetItems());
	IReadOnlyCollection<IB4Item> items;

	#endregion

	IReadOnlyCollection<IB4Item> GetItems()
	{
		return entryHeader.MergedLines
			.Select(e => new B4ItemWrapper(e, exportMessageSendingWrapperFactory))
			.ToCollection();
	}
}
