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

sealed class H4ItemWrapper : IH4Item
{
	public H4ItemWrapper(CusEntryLine entryLine, IMessageSendingWrapperFactory messageSendingWrapperFactory)
	{
		Argument.NotNull(entryLine, nameof(entryLine));
		Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));

		entryLineWrapperLazy = new Lazy<ICusEntryLineCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewCusEntryLineCustomsMessageWrapper(entryLine));
	}

	readonly Lazy<ICusEntryLineCustomsMessageWrapper> entryLineWrapperLazy;

	int IH4Item.ItemNumber => EntryLineWrapper.ItemNumber;

	ICustomsProcedure IH4Item.Procedure => EntryLineWrapper.Procedure;

	IReadOnlyCollection<IPreviousDocument> IH4Item.PreviousDocuments => EntryLineWrapper.PreviousDocuments ?? Array.Empty<IPreviousDocument>();

	IReadOnlyCollection<IAdditionalInformation> IH4Item.AdditionalInformation => EntryLineWrapper.AdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<MessageBuilder.ISupportingDocument> IH4Item.SupportingDocuments => EntryLineWrapper.SupportingDocuments ?? Array.Empty<MessageBuilder.ISupportingDocument>();

	IEoriTrader IH4Item.Exporter => EntryLineWrapper.Exporter;

	IReadOnlyCollection<IAdditionalSupplyChainActor> IH4Item.AdditionalSupplyChainActors => EntryLineWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	IReadOnlyCollection<MessageBuilder.IFee> IH4Item.Fees => EntryLineWrapper.Fees ?? Array.Empty<MessageBuilder.IFee>();

	decimal IH4Item.TotalFeeAmount => EntryLineWrapper.TotalFeeAmount;

	string IH4Item.RelatedIndicator => EntryLineWrapper.RelatedIndicator;

	decimal IH4Item.ItemPrice => EntryLineWrapper.ItemPrice;

	int IH4Item.ValuationMethod => EntryLineWrapper.ValuationMethod;

	int IH4Item.Preferences => EntryLineWrapper.Preferences.GetValueOrDefault();

	string IH4Item.DestinationCountryCode => EntryLineWrapper.DestinationCountryCode;

	string IH4Item.DestinationStateCode => EntryLineWrapper.DestinationStateCode;

	string IH4Item.DispatchCountryCode => EntryLineWrapper.DispatchCountryCode;

	string IH4Item.OriginCountryCode => EntryLineWrapper.OriginCountryCode;

	string IH4Item.PreferredOriginCountryCode => EntryLineWrapper.PreferredOriginCountryCode;

	DateTime? IH4Item.AcceptanceDate => null;

	decimal IH4Item.NetMass => EntryLineWrapper.NetMass;

	decimal? IH4Item.SupplementaryUnit => EntryLineWrapper.SupplementaryUnit;

	decimal IH4Item.GrossMass => EntryLineWrapper.GrossMass;

	string IH4Item.GoodsDescription => EntryLineWrapper.GoodsDescription;

	IReadOnlyCollection<IPackage> IH4Item.Packages => EntryLineWrapper.Packages ?? Array.Empty<IPackage>();

	string IH4Item.CusCode => EntryLineWrapper.CusCode;

	string IH4Item.NcCode => EntryLineWrapper.NcCode;

	string IH4Item.TaricCode => EntryLineWrapper.TaricCode;

	IReadOnlyCollection<string> IH4Item.AdditionalCodes => EntryLineWrapper.AdditionalCodes ?? Array.Empty<string>();

	IReadOnlyCollection<string> IH4Item.NationalAdditionalCodes => EntryLineWrapper.NationalAdditionalCodes ?? Array.Empty<string>();

	IReadOnlyCollection<string> IH4Item.Containers => EntryLineWrapper.Containers ?? Array.Empty<string>();

	int IH4Item.TransactionNature => EntryLineWrapper.TransactionNature;

	decimal IH4Item.StatisticalValue => EntryLineWrapper.StatisticalValue;

	#region Implementation

	ICusEntryLineCustomsMessageWrapper EntryLineWrapper => entryLineWrapperLazy.Value;

	#endregion
}
