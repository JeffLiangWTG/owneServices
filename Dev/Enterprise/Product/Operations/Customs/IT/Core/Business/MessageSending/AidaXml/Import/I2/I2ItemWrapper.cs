using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

sealed class I2ItemWrapper : II2Item
{
	public I2ItemWrapper(CusEntryLine entryLine, IMessageSendingWrapperFactory messageSendingWrapperFactory)
	{
		entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));

		entryLineWrapperLazy = new Lazy<ICusEntryLineCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewCusEntryLineCustomsMessageWrapper(entryLine));
	}

	readonly Lazy<ICusEntryLineCustomsMessageWrapper> entryLineWrapperLazy;

	int II2Item.ItemNumber => EntryLineWrapper.ItemNumber;

	IReadOnlyCollection<IPreviousDocument> II2Item.PreviousDocuments => EntryLineWrapper.PreviousDocuments ?? Array.Empty<IPreviousDocument>();

	decimal II2Item.GrossMass => EntryLineWrapper.GrossMass;

	IReadOnlyCollection<IPackage> II2Item.Packages => EntryLineWrapper.Packages ?? Array.Empty<IPackage>();

	IReadOnlyCollection<string> II2Item.Containers => EntryLineWrapper.Containers ?? Array.Empty<string>();

	#region Implementation

	ICusEntryLineCustomsMessageWrapper EntryLineWrapper => entryLineWrapperLazy.Value;

	#endregion
}
