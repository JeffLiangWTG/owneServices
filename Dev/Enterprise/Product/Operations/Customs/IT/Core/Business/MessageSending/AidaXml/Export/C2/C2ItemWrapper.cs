using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class C2ItemWrapper : IC2Item
{
	public C2ItemWrapper(CusEntryLine entryLine, CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory)
	{
		Argument.NotNull(entryLine, nameof(entryLine));
		Argument.NotNull(entryLine.Declaration, nameof(entryLine.Declaration));
		Argument.NotNull(exportMessageSendingWrapperFactory, nameof(exportMessageSendingWrapperFactory));

		EntryLineWrapper = exportMessageSendingWrapperFactory.GetNewCusEntryLineCustomsMessageWrapper(entryLine);
	}

	ICusEntryLineCustomsMessageWrapper EntryLineWrapper { get; }

	#region IC2Item

	int IC2Item.ItemNumber => EntryLineWrapper.ItemNumber;

	IReadOnlyCollection<IPreviousDocument> IC2Item.PreviousDocuments => EntryLineWrapper.ExportPreviousDocuments ?? Array.Empty<IPreviousDocument>();

	IReadOnlyCollection<IAuthorization> IC2Item.Authorizations => EntryLineWrapper.ExportAuthorizations ?? Array.Empty<IAuthorization>();

	string IC2Item.TransportChargesMethodOfPayment => EntryLineWrapper.ExportTransportChargesMethodOfPayment;

	#endregion
}
