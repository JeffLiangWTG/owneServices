using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;
using CusEntryLine = Enterprise.Customs.IT.Business.Declaration.CusEntryLine;
using CustomsBuilder = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

sealed class H1ItemWrapper : IH1Item
{
	public H1ItemWrapper(CusEntryLine entryLine, IMessageSendingWrapperFactory messageSendingWrapperFactory)
	{
		Argument.NotNull(entryLine, nameof(entryLine));
		Argument.NotNull(entryLine.Declaration, nameof(entryLine.Declaration));
		Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));

		entryLineWrapper = new Lazy<ICusEntryLineCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewCusEntryLineCustomsMessageWrapper(entryLine));
	}

	readonly Lazy<ICusEntryLineCustomsMessageWrapper> entryLineWrapper;

	#region IH1Item

	int IH1Item.ItemNumber => EntryLineWrapper.ItemNumber;

	ICustomsProcedure IH1Item.Procedure => EntryLineWrapper.Procedure;

	IReadOnlyCollection<IPreviousDocument> IH1Item.PreviousDocuments => EntryLineWrapper.PreviousDocuments ?? Array.Empty<IPreviousDocument>();

	IReadOnlyCollection<IAdditionalInformation> IH1Item.AdditionalInformation => EntryLineWrapper.AdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<CustomsBuilder.ISupportingDocument> IH1Item.SupportingDocuments
		=> EntryLineWrapper.SupportingDocuments ?? Array.Empty<CustomsBuilder.ISupportingDocument>();

	IEoriTrader IH1Item.Exporter => EntryLineWrapper.Exporter;

	IEoriTrader IH1Item.Seller => EntryLineWrapper.Seller;

	IEoriTrader IH1Item.Buyer => EntryLineWrapper.Buyer;

	IReadOnlyCollection<IAdditionalSupplyChainActor> IH1Item.AdditionalSupplyChainActors => EntryLineWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	IReadOnlyCollection<IFiscalReference> IH1Item.FiscalReferences => EntryLineWrapper.FiscalReferences ?? Array.Empty<IFiscalReference>();

	IReadOnlyCollection<CustomsBuilder.IFee> IH1Item.Fees
		=> EntryLineWrapper.Fees ?? Array.Empty<CustomsBuilder.IFee>();

	decimal IH1Item.TotalFeeAmount => EntryLineWrapper.TotalFeeAmount;

	IReadOnlyCollection<IAdditionOrDeduction> IH1Item.AdditionOrDeductions => EntryLineWrapper.AdditionOrDeductions ?? Array.Empty<IAdditionOrDeduction>();

	string IH1Item.RelatedIndicator => EntryLineWrapper.RelatedIndicator;

	decimal IH1Item.ItemPrice => EntryLineWrapper.ItemPrice;

	int IH1Item.ValuationMethod => EntryLineWrapper.ValuationMethod;

	int IH1Item.Preferences => EntryLineWrapper.Preferences.GetValueOrDefault();

	string IH1Item.DestinationCountryCode => EntryLineWrapper.DestinationCountryCode;

	string IH1Item.DestinationStateCode => EntryLineWrapper.DestinationStateCode;

	string IH1Item.DispatchCountryCode => EntryLineWrapper.DispatchCountryCode;

	string IH1Item.OriginCountryCode => EntryLineWrapper.OriginCountryCode;

	string IH1Item.PreferredOriginCountryCode => EntryLineWrapper.PreferredOriginCountryCode;

	DateTime? IH1Item.AcceptanceDate => null;

	decimal IH1Item.NetMass => EntryLineWrapper.NetMass;

	decimal? IH1Item.SupplementaryUnit => EntryLineWrapper.SupplementaryUnit;

	decimal IH1Item.GrossMass => EntryLineWrapper.GrossMass;

	string IH1Item.GoodsDescription => EntryLineWrapper.GoodsDescription;

	IReadOnlyCollection<IPackage> IH1Item.Packages => EntryLineWrapper.Packages ?? Array.Empty<IPackage>();

	string IH1Item.CusCode => EntryLineWrapper.CusCode;

	string IH1Item.NcCode => EntryLineWrapper.NcCode;

	string IH1Item.TaricCode => EntryLineWrapper.TaricCode;

	IReadOnlyCollection<string> IH1Item.AdditionalCodes => EntryLineWrapper.AdditionalCodes ?? Array.Empty<string>();

	IReadOnlyCollection<string> IH1Item.NationalAdditionalCodes => EntryLineWrapper.NationalAdditionalCodes ?? Array.Empty<string>();

	IReadOnlyCollection<string> IH1Item.Containers => EntryLineWrapper.Containers ?? Array.Empty<string>();

	string IH1Item.ConcessionOrder => EntryLineWrapper.ConcessionOrder;

	int IH1Item.TransactionNature => EntryLineWrapper.TransactionNature;

	decimal IH1Item.StatisticalValue => EntryLineWrapper.StatisticalValue;

	#endregion

	ICusEntryLineCustomsMessageWrapper EntryLineWrapper => entryLineWrapper.Value;
}
