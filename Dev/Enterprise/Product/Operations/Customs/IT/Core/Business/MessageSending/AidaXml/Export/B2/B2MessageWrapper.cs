using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class B2MessageWrapper : IB2Message
{
	public B2MessageWrapper(CusEntryHeader entryHeader, CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		this.exportMessageSendingWrapperFactory = Argument.NotNull(exportMessageSendingWrapperFactory, nameof(exportMessageSendingWrapperFactory));
	}

	public B2MessageWrapper(CusEntryHeader entryHeader, CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory, IDeclarationAmendment amendment)
		: this(entryHeader, exportMessageSendingWrapperFactory)
	{
		this.amendment = amendment;
	}

	readonly CusEntryHeader entryHeader;
	readonly CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory;
	readonly IDeclarationAmendment amendment;

	#region IB2Message

	IB2Header IB2Message.MessageHeader => messageHeader ?? (messageHeader = new B2HeaderWrapper(entryHeader, exportMessageSendingWrapperFactory, amendment));
	IB2Header messageHeader;

	IReadOnlyCollection<IB2Item> IB2Message.Items => items ?? (items = GetItems());
	IReadOnlyCollection<IB2Item> items;

	#endregion

	IReadOnlyCollection<IB2Item> GetItems()
	{
		return entryHeader.MergedLines
			.Select(e => new B2ItemWrapper(e, exportMessageSendingWrapperFactory))
			.ToCollection();
	}
}
