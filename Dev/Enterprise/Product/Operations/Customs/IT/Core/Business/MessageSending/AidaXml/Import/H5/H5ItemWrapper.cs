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

public sealed class H5ItemWrapper : IH5Item
{
	public H5ItemWrapper(CusEntryLine entryLine, IMessageSendingWrapperFactory messageSendingWrapperFactory)
	{
		Argument.NotNull(entryLine, nameof(entryLine));
		Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));

		entryLineWrapper = new Lazy<ICusEntryLineCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewCusEntryLineCustomsMessageWrapper(entryLine));
	}

	readonly Lazy<ICusEntryLineCustomsMessageWrapper> entryLineWrapper;

	DateTime? IH5Item.AcceptanceDate => null;

	IReadOnlyCollection<string> IH5Item.AdditionalCodes
		=> EntryLineWrapper.AdditionalCodes ?? Array.Empty<string>();

	IReadOnlyCollection<IAdditionalInformation> IH5Item.AdditionalInformation
		=> EntryLineWrapper.AdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<IAdditionalSupplyChainActor> IH5Item.AdditionalSupplyChainActors
		=> EntryLineWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	IReadOnlyCollection<IAdditionOrDeduction> IH5Item.AdditionOrDeductions
		=> EntryLineWrapper.AdditionOrDeductions ?? Array.Empty<IAdditionOrDeduction>();

	IReadOnlyCollection<string> IH5Item.Containers
		=> EntryLineWrapper.Containers ?? Array.Empty<string>();

	string IH5Item.DestinationCountryCode => EntryLineWrapper.DestinationCountryCode;

	string IH5Item.DestinationStateCode => EntryLineWrapper.DestinationStateCode;

	string IH5Item.DispatchCountryCode => EntryLineWrapper.DispatchCountryCode;

	IEoriTrader IH5Item.Exporter => EntryLineWrapper.Exporter;

	IReadOnlyCollection<MessageBuilder.IFee> IH5Item.Fees
		=> EntryLineWrapper.Fees ?? Array.Empty<MessageBuilder.IFee>();

	string IH5Item.GoodsDescription => EntryLineWrapper.GoodsDescription;

	decimal IH5Item.GrossMass => EntryLineWrapper.GrossMass;

	int IH5Item.ItemNumber => EntryLineWrapper.ItemNumber;

	decimal IH5Item.ItemPrice => EntryLineWrapper.ItemPrice;

	IReadOnlyCollection<string> IH5Item.NationalAdditionalCodes
		 => EntryLineWrapper.NationalAdditionalCodes ?? Array.Empty<string>();

	string IH5Item.CusCode => EntryLineWrapper.CusCode;

	string IH5Item.NcCode => EntryLineWrapper.NcCode;

	decimal IH5Item.NetMass => EntryLineWrapper.NetMass;

	string IH5Item.OriginCountryCode => EntryLineWrapper.OriginCountryCode;

	IReadOnlyCollection<IPackage> IH5Item.Packages
		=> EntryLineWrapper.Packages ?? Array.Empty<IPackage>();

	int IH5Item.Preferences => EntryLineWrapper.Preferences.GetValueOrDefault();

	string IH5Item.PreferredOriginCountryCode => EntryLineWrapper.PreferredOriginCountryCode;

	IReadOnlyCollection<IPreviousDocument> IH5Item.PreviousDocuments
		=> EntryLineWrapper.PreviousDocuments ?? Array.Empty<IPreviousDocument>();

	ICustomsProcedure IH5Item.Procedure => EntryLineWrapper.Procedure;

	string IH5Item.RelatedIndicator => EntryLineWrapper.RelatedIndicator;

	decimal IH5Item.StatisticalValue => EntryLineWrapper.StatisticalValue;

	decimal? IH5Item.SupplementaryUnit => EntryLineWrapper.SupplementaryUnit;

	IReadOnlyCollection<MessageBuilder.ISupportingDocument> IH5Item.SupportingDocuments
		=> EntryLineWrapper.SupportingDocuments ?? Array.Empty<MessageBuilder.ISupportingDocument>();

	string IH5Item.TaricCode => EntryLineWrapper.TaricCode;

	decimal IH5Item.TotalFeeAmount => EntryLineWrapper.TotalFeeAmount;

	int? IH5Item.TransactionNature => EntryLineWrapper.TransactionNature;

	int IH5Item.ValuationMethod => EntryLineWrapper.ValuationMethod;

	ICusEntryLineCustomsMessageWrapper EntryLineWrapper => entryLineWrapper.Value;
}
