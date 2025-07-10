using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class C2MessageWrapper : IC2Message
{
	public C2MessageWrapper(CusEntryHeader entryHeader, CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory, IDeclarationAmendment amendment = null)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		this.exportMessageSendingWrapperFactory = Argument.NotNull(exportMessageSendingWrapperFactory, nameof(exportMessageSendingWrapperFactory));
		this.amendment = amendment;
	}

	#region IC2Message

	IC2Header IC2Message.MessageHeader => messageHeader ?? (messageHeader = new C2HeaderWrapper(entryHeader, exportMessageSendingWrapperFactory, amendment));
	IC2Header messageHeader;

	IReadOnlyCollection<IC2Item> IC2Message.Items => items ?? (items = GetItems());
	IReadOnlyCollection<IC2Item> items;

	#endregion

	IReadOnlyCollection<IC2Item> GetItems()
	{
		return entryHeader.MergedLines
			.Select(line => new C2ItemWrapper(line, exportMessageSendingWrapperFactory))
			.ToCollection();
	}

	readonly CusEntryHeader entryHeader;
	readonly CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory;
	readonly IDeclarationAmendment amendment;
}
