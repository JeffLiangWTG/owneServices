using System;
using System.Collections.Generic;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;
using Enterprise.MasterFiles.Business;
using Argument = CargoWise.Common.Argument;
using CustomsMessageBuilder = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

sealed class JobDeclarationCustomsMessageWrapper : IJobDeclarationCustomsMessageWrapper
{
	public JobDeclarationCustomsMessageWrapper(JobDeclaration jobDeclaration)
	{
		this.jobDeclaration = Argument.NotNull(jobDeclaration, nameof(jobDeclaration));
		InitializeLazy();
	}

	#region IJobDeclarationCustomsMessageWrapper

	ZString IJobDeclarationCustomsMessageWrapper.BorderMeansOfTransportNationality => jobDeclaration.JE_RN_NKTransportNationality;

	ZString IJobDeclarationCustomsMessageWrapper.GoodsCountryOfDestination => jobDeclaration.JE_GoodsDestination;

	ZString IJobDeclarationCustomsMessageWrapper.GoodsCountryOfOrigin => jobDeclaration.JE_GoodsOrigin;

	ZInt IJobDeclarationCustomsMessageWrapper.ContainerModeForImportMessage => lazyContainerModeForImportMessage.Value;
	Lazy<ZInt> lazyContainerModeForImportMessage;

	ZBool IJobDeclarationCustomsMessageWrapper.IsContainerizedTransport => lazyIsContainerizedTransport.Value;
	Lazy<ZBool> lazyIsContainerizedTransport;

	ZString IJobDeclarationCustomsMessageWrapper.Ucr => jobDeclaration.JE_UCR;

	IArrivalMeansOfTransport IJobDeclarationCustomsMessageWrapper.ArrivalMeansOfTransport => lazyArrivalMeansOfTransport.Value;
	Lazy<IArrivalMeansOfTransport> lazyArrivalMeansOfTransport;

	int IJobDeclarationCustomsMessageWrapper.BorderTransportMode => lazyBorderTransportMode.Value;
	Lazy<int> lazyBorderTransportMode;

	IEoriTrader IJobDeclarationCustomsMessageWrapper.ImportDeclarant => GetCachedImportDeclarant();
	Lazy<IEoriTrader> lazyImportDeclarant;

	ZString IJobDeclarationCustomsMessageWrapper.DeclarationCustomsOffice => lazyDeclarationCustomsOffice.Value;
	Lazy<ZString> lazyDeclarationCustomsOffice;

	IEoriTrader IJobDeclarationCustomsMessageWrapper.Importer => lazyImporter.Value;
	Lazy<IEoriTrader> lazyImporter;

	int? IJobDeclarationCustomsMessageWrapper.InlandTransportMode => lazyInlandTransportMode.Value;
	Lazy<int?> lazyInlandTransportMode;

	ZString IJobDeclarationCustomsMessageWrapper.RegionOfDestination => (jobDeclaration.FinalDestination as ILocation)?.State?.RW_Code ?? ZString.Empty;

	IRepresentative IJobDeclarationCustomsMessageWrapper.Representative => lazyRepresentative.Value;
	Lazy<IRepresentative> lazyRepresentative;

	IEoriTrader IJobDeclarationCustomsMessageWrapper.Seller => lazySeller.Value;
	Lazy<IEoriTrader> lazySeller;

	ILocationOfGoods IJobDeclarationCustomsMessageWrapper.ImportLocationOfGoods => lazyLocationOfGoods.Value;
	Lazy<ILocationOfGoods> lazyLocationOfGoods;

	ZString IJobDeclarationCustomsMessageWrapper.SupervisingCustomsOffice => lazySupervisingCustomsOffice.Value;
	Lazy<ZString> lazySupervisingCustomsOffice;

	ZString IJobDeclarationCustomsMessageWrapper.PresentationCustomsOffice => lazyPresentationCustomsOffice.Value;
	Lazy<ZString> lazyPresentationCustomsOffice;

	IEoriTrader IJobDeclarationCustomsMessageWrapper.Buyer => lazyBuyer.Value;
	Lazy<IEoriTrader> lazyBuyer;

	ZString IJobDeclarationCustomsMessageWrapper.DutyPayerIdentificationNumber => lazyDutyPayerIdentificationNumber.Value;
	Lazy<ZString> lazyDutyPayerIdentificationNumber;

	IEoriTrader IJobDeclarationCustomsMessageWrapper.Supplier => lazySupplier.Value;
	Lazy<IEoriTrader> lazySupplier;

	string IJobDeclarationCustomsMessageWrapper.EntryStyle => jobDeclaration.JE_EntryStyle;

	IMeansOfTransport IJobDeclarationCustomsMessageWrapper.GetExportBorderMeansOfTransport(CusEntryInstruction entryInstruction)
		=> Ucc6ExportActiveBorderMeansOfTransferWrapper.GetNewOrNull(jobDeclaration, entryInstruction);

	string IJobDeclarationCustomsMessageWrapper.CarrierIdentificationNumber => lazyCarrierIdentificationNumber.Value;
	Lazy<string> lazyCarrierIdentificationNumber;

	ZString IJobDeclarationCustomsMessageWrapper.CountryOfExport => jobDeclaration.JE_GoodsOrigin;

	string IJobDeclarationCustomsMessageWrapper.ExitCustomsOffice => lazyExitCustomsOffice.Value;
	Lazy<ZString> lazyExitCustomsOffice;

	string IJobDeclarationCustomsMessageWrapper.ExportCustomsOffice => lazyExportCustomsOffice.Value;
	Lazy<ZString> lazyExportCustomsOffice;

	IEoriTrader IJobDeclarationCustomsMessageWrapper.Exporter => lazyExporter.Value;
	Lazy<IEoriTrader> lazyExporter;

	ZBool IJobDeclarationCustomsMessageWrapper.IsSecurityDeclaration => jobDeclaration.ZG_IsSecurityDeclaration;

	ZString IJobDeclarationCustomsMessageWrapper.SpecificCircumstanceIndicator => jobDeclaration.ZG_SpecificCircumstanceIndicator;

	CustomsMessageBuilder.Export.ILocationOfGoods IJobDeclarationCustomsMessageWrapper.ExportLocationOfGoods => lazyExportLocationOfGoods.Value;
	Lazy<CustomsMessageBuilder.Export.ILocationOfGoods> lazyExportLocationOfGoods;

	IEoriTrader IJobDeclarationCustomsMessageWrapper.ExportDeclarant => lazyExportDeclarant.Value;
	Lazy<IEoriTrader> lazyExportDeclarant;

	IReadOnlyCollection<IConsignmentCountryRouting> IJobDeclarationCustomsMessageWrapper.ConsignmentRoutings => lazyConsignmentRoutings.Value ?? Array.Empty<IConsignmentCountryRouting>();
	Lazy<IReadOnlyCollection<IConsignmentCountryRouting>> lazyConsignmentRoutings;

	#endregion

	#region Implementation

	void InitializeLazy()
	{
		lazyContainerModeForImportMessage = new Lazy<ZInt>(() => CustomsRulesProvider.ConvertContainerModeFromCargoWiseToIT(jobDeclaration.JE_ContainerMode) ? 1 : 0);
		lazyArrivalMeansOfTransport = new Lazy<IArrivalMeansOfTransport>(() => ArrivalMeansOfTransportWrapper.NewOrNull(jobDeclaration));
		lazySupplier = new Lazy<IEoriTrader>(GetSupplier);
		lazyDutyPayerIdentificationNumber = new Lazy<ZString>(GetCustomsDutyPayerIdentificationNumber);
		lazyBuyer = new Lazy<IEoriTrader>(GetBuyer);
		lazySeller = new Lazy<IEoriTrader>(GetSeller);
		lazyPresentationCustomsOffice = new Lazy<ZString>(GetPresentationCustomsOffice);
		lazySupervisingCustomsOffice = new Lazy<ZString>(GetSupervisingCustomsOffice);
		lazyLocationOfGoods = new Lazy<ILocationOfGoods>(() => LocationOfGoodsWrapper.NewOrNull(jobDeclaration));
		lazyRepresentative = new Lazy<IRepresentative>(() => RepresentativeWrapper.NewOrNull(jobDeclaration));
		lazyInlandTransportMode = new Lazy<int?>(GetInlandTransportMode);
		lazyImporter = new Lazy<IEoriTrader>(GetImporter);
		lazyDeclarationCustomsOffice = new Lazy<ZString>(() => XmlWrapperHelper.RemoveIsoCode(jobDeclaration.JE_CustomsOffice));
		lazyImportDeclarant = new Lazy<IEoriTrader>(GetImportDeclarant);
		lazyBorderTransportMode = new Lazy<int>(GetBorderMeansOfTransportMode);
		lazyIsContainerizedTransport = new Lazy<ZBool>(() => CustomsRulesProvider.ConvertContainerModeFromCargoWiseToIT(jobDeclaration.JE_ContainerMode));
		lazyCarrierIdentificationNumber = new Lazy<string>(GetCarrierIdentificationNumber);
		lazyExitCustomsOffice = new Lazy<ZString>(() => jobDeclaration.OfficeOfExit);
		lazyExportCustomsOffice = new Lazy<ZString>(() => jobDeclaration.JE_CustomsOffice);
		lazyExporter = new Lazy<IEoriTrader>(GetExporter);
		lazyExportLocationOfGoods = new Lazy<CustomsMessageBuilder.Export.ILocationOfGoods>(GetExportLocationOfGoods);
		lazyExportDeclarant = new Lazy<IEoriTrader>(GetExportDeclarant);
		lazyConsignmentRoutings = new Lazy<IReadOnlyCollection<IConsignmentCountryRouting>>(GetConsignmentRoutings);
	}

	IEoriTrader GetSeller()
	{
		var sellerAddress = jobDeclaration.SellerAddress;

		return sellerAddress != null
			? new TraderWrapper(sellerAddress)
			: null;
	}

	IEoriTrader GetBuyer()
	{
		var buyerAddress = jobDeclaration.Buyer?.MainAddress;

		return buyerAddress != null
			? new TraderWrapper(buyerAddress)
			: null;
	}

	ZString GetCustomsDutyPayerIdentificationNumber()
	{
		var paymentMethod = jobDeclaration.JE_PaymentMethod;

		return paymentMethod == DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14 || paymentMethod == ImportDefermentMethodList.Codes.DeclarantsAccountFromCustomsDecisions
			? ZString.Empty
			: jobDeclaration.Importer?.GetEoriCode(countryCode: true) ?? ZString.Empty;
	}

	IEoriTrader GetSupplier()
	{
		var supplierDocumentaryAddress = jobDeclaration.SupplierDocumentaryAddress;

		return !supplierDocumentaryAddress.IsEmpty
			? new TraderCustomsMessageWrapper(new TraderWrapper(supplierDocumentaryAddress))
			: null;
	}

	string GetCarrierIdentificationNumber()
	{
		ITrader carrierTrader = new EoriOrTcuTraderWrapper(jobDeclaration.ShippingLine?.MainAddress);
		var carrierIdentificationNumber = carrierTrader.IdentificationNumber;

		var declarant = GetCachedImportDeclarant();

		return carrierIdentificationNumber != declarant?.IdentificationNumber
			? carrierIdentificationNumber
			: string.Empty;
	}

	IEoriTrader GetCachedImportDeclarant() => lazyImportDeclarant.Value;

	IEoriTrader GetExporter()
	{
		var exporterAddress = jobDeclaration.ExporterDocAddress;

		return !exporterAddress.IsEmpty
			? new EoriOrTcuTraderWrapper(exporterAddress)
			: null;
	}

	CustomsMessageBuilder.Export.ILocationOfGoods GetExportLocationOfGoods()
	{
		var locationOfGoods = (Declaration.CusGoodsLocation)jobDeclaration.GoodsLocation;

		switch (locationOfGoods.CGL_Qualifier)
		{
			case Customs.Business.CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier:
				return new CustomsOfficeQualifierLocationOfGoodsWrapper(locationOfGoods);

			case Customs.Business.CusGoodsLocationQualifierList.Codes.AuthorizationNumber:
				return new AuthorizationNumberQualifierLocationOfGoodsWrapper(locationOfGoods);

			case Customs.Business.CusGoodsLocationQualifierList.Codes.Address:
				return new AddressQualifierLocationOfGoodsWrapper(locationOfGoods);

			default:
				return null;
		}
	}

	IEoriTrader GetExportDeclarant()
	{
		var declarantAddress = jobDeclaration.DeclarantAddress;

		return declarantAddress != null
			? new EoriOrTcuTraderWrapper(declarantAddress)
			: null;
	}

	IReadOnlyCollection<IConsignmentCountryRouting> GetConsignmentRoutings()
	{
		return jobDeclaration.ItineraryCountries
			.Select(country => new ConsignmentCountryRoutingWrapper(country))
			.ToCollection();
	}

	ZString GetPresentationCustomsOffice()
	{
		var presentationCustOffice = jobDeclaration
			.CustomsOffices
			.GetFirstElementHaving(EuOfficeCodesTypes.Codes.OfficeOfPresentation);

		return presentationCustOffice?.CY_Data ?? ZString.Empty;
	}

	ZString GetSupervisingCustomsOffice()
	{
		var declaration = jobDeclaration;

		var officeCode = declaration.IsImport
			? EuOfficeCodesTypes.Codes.AuthorityControlCode
			: EuOfficeCodesTypes.Codes.SupervisingOffice;

		var customsOffice = declaration.CustomsOffices?.GetFirstElementHaving(officeCode);
		return customsOffice?.CY_Data ?? ZString.Empty;
	}

	int? GetInlandTransportMode()
	{
		var declaration = jobDeclaration;

		var transportMode = declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportModeInland);
		if (int.TryParse(transportMode, out var result))
		{
			return result;
		}
		return null;
	}

	IEoriTrader GetImporter()
	{
		var importerDocumentaryAddress = jobDeclaration.ImporterDocumentaryAddress;

		return !importerDocumentaryAddress.IsEmpty
			? new TraderWrapper(importerDocumentaryAddress)
			: null;
	}

	IEoriTrader GetImportDeclarant()
	{
		var declarantAddress = jobDeclaration.DeclarantAddress;

		return declarantAddress != null
			? new TraderWrapper(declarantAddress)
			: null;
	}

	int GetBorderMeansOfTransportMode()
	{
		var declaration = jobDeclaration;

		var transportMode = declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportMode);
		if (int.TryParse(transportMode, out var result))
		{
			return result;
		}
		return 0;
	}

	#endregion

	readonly JobDeclaration jobDeclaration;
}
