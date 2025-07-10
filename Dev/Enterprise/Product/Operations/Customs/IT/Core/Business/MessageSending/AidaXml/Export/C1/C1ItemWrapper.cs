using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class C1ItemWrapper : IC1Item
{
	public C1ItemWrapper(CusEntryLine entryLine, CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory)
	{
		Argument.NotNull(entryLine, nameof(entryLine));
		Argument.NotNull(entryLine.Declaration, nameof(entryLine.Declaration));
		Argument.NotNull(exportMessageSendingWrapperFactory, nameof(exportMessageSendingWrapperFactory));

		EntryLineWrapper = exportMessageSendingWrapperFactory.GetNewCusEntryLineCustomsMessageWrapper(entryLine);
	}

	ICusEntryLineCustomsMessageWrapper EntryLineWrapper { get; }

	#region IC1Item

	int IC1Item.ItemNumber => EntryLineWrapper.ItemNumber;

	ICustomsProcedure IC1Item.Procedure => EntryLineWrapper.Procedure;

	IReadOnlyCollection<IPreviousDocument> IC1Item.PreviousDocuments => EntryLineWrapper.ExportPreviousDocuments ?? Array.Empty<IPreviousDocument>();

	IReadOnlyCollection<IAdditionalInformation> IC1Item.AdditionalInformation => EntryLineWrapper.ExportAdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<CargoWise.Customs.IT.MessageContracts.Declaration.ISupportingDocument> IC1Item.SupportingDocuments => EntryLineWrapper.SupportingDocuments ?? Array.Empty<CargoWise.Customs.IT.MessageContracts.Declaration.ISupportingDocument>();

	IReadOnlyCollection<IAdditionalReference> IC1Item.AdditionalReferences => EntryLineWrapper.ExportAdditionalReferences ?? Array.Empty<IAdditionalReference>();

	IReadOnlyCollection<IAuthorization> IC1Item.Authorizations => EntryLineWrapper.ExportAuthorizations ?? Array.Empty<IAuthorization>();

	IReadOnlyCollection<ITransportDocument> IC1Item.TransportDocuments => EntryLineWrapper.ExportTransportDocuments ?? Array.Empty<ITransportDocument>();

	IEoriTrader IC1Item.Consignor => EntryLineWrapper.ExportConsignor;

	IEoriTrader IC1Item.Consignee => EntryLineWrapper.ExportConsignee;

	IReadOnlyCollection<IAdditionalSupplyChainActor> IC1Item.AdditionalSupplyChainActors => EntryLineWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	string IC1Item.TransportChargesMethodOfPayment => EntryLineWrapper.ExportTransportChargesMethodOfPayment;

	string IC1Item.CountryOfDestination => EntryLineWrapper.CountryOfDestination;

	string IC1Item.CountryOfExport => EntryLineWrapper.CountryOfExport;

	string IC1Item.CountryOfOrigin => EntryLineWrapper.ExportCountryOfOrigin;

	decimal IC1Item.NetMass => EntryLineWrapper.NetMass;

	decimal IC1Item.GrossMass => EntryLineWrapper.GrossMass;

	string IC1Item.GoodsDescription => EntryLineWrapper.GoodsDescription;

	IReadOnlyCollection<IPackage> IC1Item.Packages => EntryLineWrapper.Packages ?? Array.Empty<IPackage>();

	string IC1Item.CusCode => EntryLineWrapper.CusCode;

	string IC1Item.HsTariffCode => EntryLineWrapper.ExportHsTariffCode;

	string IC1Item.NcTariffCode => EntryLineWrapper.ExportNcTariffCode;

	IReadOnlyCollection<string> IC1Item.NationalAdditionalCodes => EntryLineWrapper.NationalAdditionalCodes ?? Array.Empty<string>();

	IReadOnlyCollection<string> IC1Item.AdditionalCodes => EntryLineWrapper.AdditionalCodes ?? Array.Empty<string>();

	IReadOnlyCollection<string> IC1Item.DangerousGoodsCodes => EntryLineWrapper.DangerousGoodsCodes ?? Array.Empty<string>();

	#endregion
}
