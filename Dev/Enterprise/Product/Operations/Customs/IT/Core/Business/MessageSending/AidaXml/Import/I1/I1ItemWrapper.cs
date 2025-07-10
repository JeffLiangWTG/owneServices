using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;
using MessageBuilder = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

sealed class I1ItemWrapper : II1Item
{
	public I1ItemWrapper(CusEntryLine entryLine, IMessageSendingWrapperFactory messageSendingWrapperFactory)
	{
		Argument.NotNull(entryLine, nameof(entryLine));
		Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));

		entryLineWrapper = new Lazy<ICusEntryLineCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewCusEntryLineCustomsMessageWrapper(entryLine));
		entryHeaderWrapper = new Lazy<ICusEntryHeaderCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewCusEntryHeaderCustomsMessageWrapper(entryLine.Header));
	}

	readonly Lazy<ICusEntryLineCustomsMessageWrapper> entryLineWrapper;
	readonly Lazy<ICusEntryHeaderCustomsMessageWrapper> entryHeaderWrapper;

	#region II1Item

	int II1Item.ItemNumber => EntryLineWrapper.ItemNumber;

	ICustomsProcedure II1Item.Procedure => EntryLineWrapper.Procedure;

	IReadOnlyCollection<IPreviousDocument> II1Item.PreviousDocuments => EntryLineWrapper.PreviousDocuments ?? Array.Empty<IPreviousDocument>();

	IReadOnlyCollection<IAdditionalInformation> II1Item.AdditionalInformation => EntryLineWrapper.AdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<MessageBuilder.ISupportingDocument> II1Item.SupportingDocuments => EntryLineWrapper.SupportingDocuments ?? Array.Empty<MessageBuilder.ISupportingDocument>();

	IEoriTrader II1Item.Exporter => EntryLineWrapper.Exporter;

	IReadOnlyCollection<IAdditionalSupplyChainActor> II1Item.AdditionalSupplyChainActors => EntryLineWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	decimal? II1Item.ItemPrice
	{
		get
		{
			var invoiceCurrency = EntryHeaderWrapper.InvoiceCurrencyCode;
			return !invoiceCurrency.IsEmpty ? EntryLineWrapper.ItemPrice : null;
		}
	}

	int? II1Item.Preferences => EntryLineWrapper.Preferences;

	IReadOnlyCollection<string> II1Item.Containers => EntryLineWrapper.Containers ?? Array.Empty<string>();

	string II1Item.DispatchCountryCode => EntryLineWrapper.DispatchCountryCode;

	string II1Item.OriginCountryCode => EntryLineWrapper.OriginCountryCode;

	string II1Item.PreferredOriginCountryCode => EntryLineWrapper.PreferredOriginCountryCode;

	decimal? II1Item.SupplementaryUnit => EntryLineWrapper.SupplementaryUnit;

	decimal II1Item.GrossMass => EntryLineWrapper.GrossMass;

	string II1Item.GoodsDescription => EntryLineWrapper.GoodsDescription;

	IReadOnlyCollection<IPackage> II1Item.Packages => EntryLineWrapper.Packages ?? Array.Empty<IPackage>();

	string II1Item.CusCode => EntryLineWrapper.CusCode;

	string II1Item.NcCode => EntryLineWrapper.NcCode;

	string II1Item.TaricCode => EntryLineWrapper.TaricCode;

	IReadOnlyCollection<string> II1Item.AdditionalCodes => EntryLineWrapper.AdditionalCodes ?? Array.Empty<string>();

	IReadOnlyCollection<string> II1Item.NationalAdditionalCodes => EntryLineWrapper.NationalAdditionalCodes ?? Array.Empty<string>();

	decimal II1Item.NetMass => EntryLineWrapper.NetMass;

	string II1Item.ConcessionOrder => EntryLineWrapper.ConcessionOrder;

	#endregion

	#region Implementation

	ICusEntryLineCustomsMessageWrapper EntryLineWrapper => entryLineWrapper.Value;

	ICusEntryHeaderCustomsMessageWrapper EntryHeaderWrapper => entryHeaderWrapper.Value;

	#endregion
}
