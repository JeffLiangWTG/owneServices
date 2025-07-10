using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.Declaration;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class B1MessageWrapper : IB1Message
{
	public B1MessageWrapper(CusEntryHeader entryHeader, IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory, IDeclarationAmendment amendment = null)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		this.exportMessageSendingWrapperFactory = Argument.NotNull(exportMessageSendingWrapperFactory, nameof(exportMessageSendingWrapperFactory));
		this.amendment = amendment;
	}

	readonly CusEntryHeader entryHeader;
	readonly IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory;
	readonly IDeclarationAmendment amendment;

	IB1Header IB1Message.MessageHeader => messageHeader ?? (messageHeader = new B1HeaderWrapper(entryHeader, exportMessageSendingWrapperFactory, amendment));
	IB1Header messageHeader;

	IReadOnlyCollection<IB1Item> IB1Message.Items => items ?? (items = GetItems().ToList().AsReadOnly());
	IReadOnlyCollection<IB1Item> items;

	IEnumerable<IB1Item> GetItems()
	{
		return entryHeader
			.MergedLines
			.Cast<CusEntryLine>()
			.Select(entryLine => new B1ItemWrapper(entryLine, exportMessageSendingWrapperFactory));
	}
}
