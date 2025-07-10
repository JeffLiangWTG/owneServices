using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class B1ItemWrapper : IB1Item
{
	public B1ItemWrapper(CusEntryLine entryLine, CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory)
	{
		Argument.NotNull(entryLine, nameof(entryLine));
		Argument.NotNull(entryLine.Declaration, nameof(entryLine.Declaration));
		Argument.NotNull(exportMessageSendingWrapperFactory, nameof(exportMessageSendingWrapperFactory));

		EntryLineWrapper = exportMessageSendingWrapperFactory.GetNewCusEntryLineCustomsMessageWrapper(entryLine);
	}

	ICusEntryLineCustomsMessageWrapper EntryLineWrapper { get; }

	int IB1Item.ItemNumber => EntryLineWrapper.ItemNumber;

	ICustomsProcedure IB1Item.Procedure => EntryLineWrapper.Procedure;

	IReadOnlyCollection<IPreviousDocument> IB1Item.PreviousDocuments => EntryLineWrapper.ExportPreviousDocuments ?? Array.Empty<IPreviousDocument>();

	IReadOnlyCollection<IAdditionalInformation> IB1Item.AdditionalInformation => EntryLineWrapper.ExportAdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<CargoWise.Customs.IT.MessageContracts.Declaration.ISupportingDocument> IB1Item.SupportingDocuments
		=> EntryLineWrapper.SupportingDocuments ?? Array.Empty<CargoWise.Customs.IT.MessageContracts.Declaration.ISupportingDocument>();

	IReadOnlyCollection<IAdditionalReference> IB1Item.AdditionalReferences => EntryLineWrapper.ExportAdditionalReferences ?? Array.Empty<IAdditionalReference>();

	IReadOnlyCollection<IAuthorization> IB1Item.Authorizations => EntryLineWrapper.ExportAuthorizations ?? Array.Empty<IAuthorization>();

	IReadOnlyCollection<ITransportDocument> IB1Item.TransportDocuments => EntryLineWrapper.ExportTransportDocuments ?? Array.Empty<ITransportDocument>();

	IEoriTrader IB1Item.Consignor => EntryLineWrapper.ExportConsignor;

	IEoriTrader IB1Item.Consignee => EntryLineWrapper.ExportConsignee;

	IReadOnlyCollection<IAdditionalSupplyChainActor> IB1Item.AdditionalSupplyChainActors => EntryLineWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	decimal IB1Item.TotalFeeAmount => EntryLineWrapper.TotalFeeAmount;

	IReadOnlyCollection<CargoWise.Customs.IT.MessageContracts.Declaration.IFee> IB1Item.Fees
		=> EntryLineWrapper.Fees ?? Array.Empty<CargoWise.Customs.IT.MessageContracts.Declaration.IFee>();

	string IB1Item.TransportChargesMethodOfPayment => EntryLineWrapper.ExportTransportChargesMethodOfPayment;

	string IB1Item.CountryOfDestination => EntryLineWrapper.CountryOfDestination;

	string IB1Item.CountryOfExport => EntryLineWrapper.CountryOfExport;

	string IB1Item.CountryOfOrigin => EntryLineWrapper.ExportCountryOfOrigin;

	string IB1Item.RegionOfDispatch => EntryLineWrapper.ExportRegionOfDispatch;

	decimal IB1Item.NetMass => EntryLineWrapper.NetMass;

	decimal? IB1Item.SupplementaryUnit => EntryLineWrapper.SupplementaryUnit;

	decimal IB1Item.GrossMass => EntryLineWrapper.GrossMass;

	string IB1Item.GoodsDescription => EntryLineWrapper.GoodsDescription;

	IReadOnlyCollection<IPackage> IB1Item.Packages => EntryLineWrapper.Packages ?? Array.Empty<IPackage>();

	string IB1Item.CusCode => EntryLineWrapper.CusCode;

	string IB1Item.HsTariffCode => EntryLineWrapper.ExportHsTariffCode;

	string IB1Item.NcTariffCode => EntryLineWrapper.ExportNcTariffCode;

	IReadOnlyCollection<string> IB1Item.NationalAdditionalCodes => EntryLineWrapper.NationalAdditionalCodes ?? Array.Empty<string>();

	IReadOnlyCollection<string> IB1Item.AdditionalCodes => EntryLineWrapper.AdditionalCodes ?? Array.Empty<string>();

	IReadOnlyCollection<string> IB1Item.DangerousGoodsCodes => EntryLineWrapper.DangerousGoodsCodes ?? Array.Empty<string>();

	int? IB1Item.NatureOfTransaction => EntryLineWrapper.ExportNatureOfTransaction;

	decimal? IB1Item.StatisticalValue => EntryLineWrapper.StatisticalValue;
}
