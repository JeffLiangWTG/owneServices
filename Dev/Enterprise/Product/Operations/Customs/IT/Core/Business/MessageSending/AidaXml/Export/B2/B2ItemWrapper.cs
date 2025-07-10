using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class B2ItemWrapper : IB2Item
{
	public B2ItemWrapper(CusEntryLine entryLine, CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory)
	{
		Argument.NotNull(entryLine, nameof(entryLine));
		Argument.NotNull(entryLine.Declaration, nameof(entryLine.Declaration));
		Argument.NotNull(exportMessageSendingWrapperFactory, nameof(exportMessageSendingWrapperFactory));

		EntryLineWrapper = exportMessageSendingWrapperFactory.GetNewCusEntryLineCustomsMessageWrapper(entryLine);
	}

	#region IB2Item

	int IB2Item.ItemNumber => EntryLineWrapper.ItemNumber;

	ICustomsProcedure IB2Item.Procedure => EntryLineWrapper.Procedure;

	IReadOnlyCollection<IPreviousDocument> IB2Item.PreviousDocuments => EntryLineWrapper.ExportPreviousDocuments ?? Array.Empty<IPreviousDocument>();

	IReadOnlyCollection<IAdditionalInformation> IB2Item.AdditionalInformation => EntryLineWrapper.ExportAdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<CargoWise.Customs.IT.MessageContracts.Declaration.ISupportingDocument> IB2Item.SupportingDocuments
		=> EntryLineWrapper.SupportingDocuments ?? Array.Empty<CargoWise.Customs.IT.MessageContracts.Declaration.ISupportingDocument>();

	IReadOnlyCollection<IAdditionalReference> IB2Item.AdditionalReferences => EntryLineWrapper.ExportAdditionalReferences ?? Array.Empty<IAdditionalReference>();

	IReadOnlyCollection<IAuthorization> IB2Item.Authorizations => EntryLineWrapper.ExportAuthorizations ?? Array.Empty<IAuthorization>();

	IReadOnlyCollection<ITransportDocument> IB2Item.TransportDocuments => EntryLineWrapper.ExportTransportDocuments ?? Array.Empty<ITransportDocument>();

	IEoriTrader IB2Item.Consignor => EntryLineWrapper.ExportConsignor;

	IEoriTrader IB2Item.Consignee => EntryLineWrapper.ExportConsignee;

	IReadOnlyCollection<IAdditionalSupplyChainActor> IB2Item.AdditionalSupplyChainActors => EntryLineWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	string IB2Item.CountryOfDestination => EntryLineWrapper.CountryOfDestination;

	string IB2Item.CountryOfExport => EntryLineWrapper.CountryOfExport;

	string IB2Item.CountryOfOrigin => EntryLineWrapper.ExportCountryOfOrigin;

	string IB2Item.RegionOfDispatch => EntryLineWrapper.ExportRegionOfDispatch;

	decimal IB2Item.NetMass => EntryLineWrapper.NetMass;

	decimal? IB2Item.SupplementaryUnit => EntryLineWrapper.SupplementaryUnit;

	decimal IB2Item.GrossMass => EntryLineWrapper.GrossMass;

	string IB2Item.GoodsDescription => EntryLineWrapper.GoodsDescription;

	IReadOnlyCollection<IPackage> IB2Item.Packages => EntryLineWrapper.Packages ?? Array.Empty<IPackage>();

	string IB2Item.CusCode => EntryLineWrapper.CusCode;

	string IB2Item.HsTariffCode => EntryLineWrapper.ExportHsTariffCode;

	string IB2Item.NcTariffCode => EntryLineWrapper.ExportNcTariffCode;

	IReadOnlyCollection<string> IB2Item.NationalAdditionalCodes => EntryLineWrapper.NationalAdditionalCodes ?? Array.Empty<string>();

	IReadOnlyCollection<string> IB2Item.AdditionalCodes => EntryLineWrapper.AdditionalCodes ?? Array.Empty<string>();

	IReadOnlyCollection<string> IB2Item.DangerousGoodsCodes => EntryLineWrapper.DangerousGoodsCodes ?? Array.Empty<string>();

	int? IB2Item.NatureOfTransaction => EntryLineWrapper.ExportNatureOfTransaction;

	decimal? IB2Item.StatisticalValue => EntryLineWrapper.StatisticalValue;

	decimal IB2Item.TotalFeeAmount => EntryLineWrapper.TotalFeeAmount;

	IReadOnlyCollection<CargoWise.Customs.IT.MessageContracts.Declaration.IFee> IB2Item.Fees
		=> EntryLineWrapper.Fees ?? Array.Empty<CargoWise.Customs.IT.MessageContracts.Declaration.IFee>();

	string IB2Item.TransportChargesMethodOfPayment => EntryLineWrapper.ExportTransportChargesMethodOfPayment;

	#endregion

	ICusEntryLineCustomsMessageWrapper EntryLineWrapper { get; }
}
