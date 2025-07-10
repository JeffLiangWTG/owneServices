using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class B4ItemWrapper : IB4Item
{
	public B4ItemWrapper(CusEntryLine entryLine, CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory)
	{
		Argument.NotNull(entryLine, nameof(entryLine));
		Argument.NotNull(entryLine.Declaration, nameof(entryLine.Declaration));
		Argument.NotNull(exportMessageSendingWrapperFactory, nameof(exportMessageSendingWrapperFactory));

		EntryLineWrapper = exportMessageSendingWrapperFactory.GetNewCusEntryLineCustomsMessageWrapper(entryLine);
	}

	#region IB4Item

	int IB4Item.ItemNumber => EntryLineWrapper.ItemNumber;

	ICustomsProcedure IB4Item.Procedure => EntryLineWrapper.Procedure;

	IReadOnlyCollection<IPreviousDocument> IB4Item.PreviousDocuments => EntryLineWrapper.ExportPreviousDocuments ?? Array.Empty<IPreviousDocument>();

	IReadOnlyCollection<IAdditionalInformation> IB4Item.AdditionalInformation => EntryLineWrapper.ExportAdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<CargoWise.Customs.IT.MessageContracts.Declaration.ISupportingDocument> IB4Item.SupportingDocuments
		=> EntryLineWrapper.SupportingDocuments ?? Array.Empty<CargoWise.Customs.IT.MessageContracts.Declaration.ISupportingDocument>();

	IReadOnlyCollection<IAuthorization> IB4Item.Authorizations => EntryLineWrapper.ExportAuthorizations ?? Array.Empty<IAuthorization>();

	IReadOnlyCollection<ITransportDocument> IB4Item.TransportDocuments => EntryLineWrapper.ExportTransportDocuments ?? Array.Empty<ITransportDocument>();

	IEoriTrader IB4Item.Consignee => EntryLineWrapper.ExportConsignee;

	IReadOnlyCollection<IAdditionalSupplyChainActor> IB4Item.AdditionalSupplyChainActors => EntryLineWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	string IB4Item.TransportChargesMethodOfPayment => EntryLineWrapper.ExportTransportChargesMethodOfPayment;

	string IB4Item.CountryOfDestination => EntryLineWrapper.CountryOfDestination;

	string IB4Item.CountryOfExport => EntryLineWrapper.CountryOfExport;

	string IB4Item.CountryOfOrigin => EntryLineWrapper.ExportCountryOfOrigin;

	string IB4Item.RegionOfDispatch => EntryLineWrapper.ExportRegionOfDispatch;

	decimal IB4Item.NetMass => EntryLineWrapper.NetMass;

	decimal? IB4Item.SupplementaryUnit => EntryLineWrapper.SupplementaryUnit;

	decimal IB4Item.GrossMass => EntryLineWrapper.GrossMass;

	string IB4Item.GoodsDescription => EntryLineWrapper.GoodsDescription;

	IReadOnlyCollection<IPackage> IB4Item.Packages => EntryLineWrapper.Packages ?? Array.Empty<IPackage>();

	string IB4Item.CusCode => EntryLineWrapper.CusCode;

	string IB4Item.HsTariffCode => EntryLineWrapper.ExportHsTariffCode;

	string IB4Item.NcTariffCode => EntryLineWrapper.ExportNcTariffCode;

	IReadOnlyCollection<string> IB4Item.DangerousGoodsCodes => EntryLineWrapper.DangerousGoodsCodes ?? Array.Empty<string>();

	int? IB4Item.NatureOfTransaction => EntryLineWrapper.ExportNatureOfTransaction;

	decimal? IB4Item.StatisticalValue => EntryLineWrapper.StatisticalValue;

	#endregion

	ICusEntryLineCustomsMessageWrapper EntryLineWrapper { get; }
}
