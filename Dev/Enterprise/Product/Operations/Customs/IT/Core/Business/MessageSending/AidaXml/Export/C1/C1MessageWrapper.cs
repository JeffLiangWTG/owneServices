using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class C1MessageWrapper : IC1Message
{
	public C1MessageWrapper(CusEntryHeader entryHeader, CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory, IDeclarationAmendment amendment = null)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		this.exportMessageSendingWrapperFactory = Argument.NotNull(exportMessageSendingWrapperFactory, nameof(exportMessageSendingWrapperFactory));
		this.amendment = amendment;
	}

	#region IC1Message

	IC1Header IC1Message.MessageHeader => messageHeader ?? (messageHeader = new C1HeaderWrapper(entryHeader, exportMessageSendingWrapperFactory, amendment));
	IC1Header messageHeader;

	IReadOnlyCollection<IC1Item> IC1Message.Items => items ?? (items = GetItems());
	IReadOnlyCollection<IC1Item> items;

	#endregion

	IReadOnlyCollection<IC1Item> GetItems()
	{
		return entryHeader.MergedLines
			.Select(line => new C1ItemWrapper(line, exportMessageSendingWrapperFactory))
			.ToCollection();
	}

	readonly CusEntryHeader entryHeader;
	readonly CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory;
	readonly IDeclarationAmendment amendment;
}
