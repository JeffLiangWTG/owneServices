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

sealed class H3ItemWrapper : IH3Item
{
	public H3ItemWrapper(CusEntryLine entryLine, IMessageSendingWrapperFactory messageSendingWrapperFactory)
	{
		Argument.NotNull(entryLine, nameof(entryLine));
		Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));

		entryLineWrapper = new Lazy<ICusEntryLineCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewCusEntryLineCustomsMessageWrapper(entryLine));
	}

	readonly Lazy<ICusEntryLineCustomsMessageWrapper> entryLineWrapper;

	#region IH3Item

	int IH3Item.TransactionNature => EntryLineWrapper.TransactionNature;

	decimal IH3Item.StatisticalValue => EntryLineWrapper.StatisticalValue;

	IReadOnlyCollection<string> IH3Item.Containers => EntryLineWrapper.Containers ?? Array.Empty<string>();

	decimal? IH3Item.SupplementaryUnit => EntryLineWrapper.SupplementaryUnit;

	decimal IH3Item.GrossMass => EntryLineWrapper.GrossMass;

	string IH3Item.GoodsDescription => EntryLineWrapper.GoodsDescription;

	string IH3Item.CusCode => EntryLineWrapper.CusCode;

	string IH3Item.NcCode => EntryLineWrapper.NcCode;

	string IH3Item.TaricCode => EntryLineWrapper.TaricCode;

	IReadOnlyCollection<string> IH3Item.AdditionalCodes => EntryLineWrapper.AdditionalCodes ?? Array.Empty<string>();

	IReadOnlyCollection<string> IH3Item.NationalAdditionalCodes => EntryLineWrapper.NationalAdditionalCodes ?? Array.Empty<string>();

	IReadOnlyCollection<IPackage> IH3Item.Packages => EntryLineWrapper.Packages ?? Array.Empty<IPackage>();

	string IH3Item.DestinationCountryCode => EntryLineWrapper.DestinationCountryCode;

	string IH3Item.OriginCountryCode => EntryLineWrapper.OriginCountryCode;

	string IH3Item.DestinationStateCode => EntryLineWrapper.DestinationStateCode;

	string IH3Item.DispatchCountryCode => EntryLineWrapper.DispatchCountryCode;

	string IH3Item.PreferredOriginCountryCode => EntryLineWrapper.PreferredOriginCountryCode;

	DateTime? IH3Item.AcceptanceDate => null;

	IReadOnlyCollection<MessageBuilder.IFee> IH3Item.Fees => EntryLineWrapper.Fees ?? Array.Empty<MessageBuilder.IFee>();

	decimal IH3Item.TotalFeeAmount => EntryLineWrapper.TotalFeeAmount;

	decimal IH3Item.ItemPrice => EntryLineWrapper.ItemPrice;

	int IH3Item.ValuationMethod => EntryLineWrapper.ValuationMethod;

	int IH3Item.Preferences => EntryLineWrapper.Preferences.GetValueOrDefault();

	IEoriTrader IH3Item.Exporter => EntryLineWrapper.Exporter;

	IReadOnlyCollection<IAdditionalSupplyChainActor> IH3Item.AdditionalSupplyChainActors => EntryLineWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	IReadOnlyCollection<IPreviousDocument> IH3Item.PreviousDocuments => EntryLineWrapper.PreviousDocuments ?? Array.Empty<IPreviousDocument>();

	IReadOnlyCollection<IAdditionalInformation> IH3Item.AdditionalInformation => EntryLineWrapper.AdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<MessageBuilder.ISupportingDocument> IH3Item.SupportingDocuments => EntryLineWrapper.SupportingDocuments ?? Array.Empty<MessageBuilder.ISupportingDocument>();

	int IH3Item.ItemNumber => EntryLineWrapper.ItemNumber;

	ICustomsProcedure IH3Item.Procedure => EntryLineWrapper.Procedure;

	#endregion

	ICusEntryLineCustomsMessageWrapper EntryLineWrapper => entryLineWrapper.Value;
}
