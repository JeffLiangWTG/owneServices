using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IImportCommonDataProvider : IESEDIMessageCollectionProvider
	{
		ZString MRN { get; }
	}

	public interface IDUAImportDataProvider : IImportCommonDataProvider
	{
		ZBool IsCeutaOrMelilla { get; }
	}

	public interface IImportCommonHeader
	{
		ZString ShipmentType { get; }
		ZInt TotalLinesNum { get; }
		IImportImporterProvider Importer { get; }
		IImportDeclarantPartyIdProvider Declarant { get; }
		ZString DeclarationEmail { get; }
		ZString OtherEmail { get; }
		ZString OriginCountry { get; }
		ZString GoodsLocation { get; }
	}

	public interface IDUAImportCommonHeader : IImportCommonHeader
	{
		ZString CustomsOfficeOfDestination { get; }
		ZString Procedure { get; }
		ZInt TotalPackagesNum { get; }
		ZString CommercialReference { get; }
		ZBool IsContainerised { get; }
		ZString CurrencyCode { get; }
		ZDecimal TotalTributesAmount { get; }
		ZString PaymentMode { get; }
		ZString ClearanceGuarantee { get; }
		ZString PendenciesGuarantee { get; }
		IReadOnlyCollection<ZString> GRNGuarantees { get; }
		ZString PaymentModeCan { get; }
		ZString ClearanceGuaranteeCan { get; }
		ZString PendenciesGuaranteeCan { get; }
		IReadOnlyCollection<ZString> GRNGuaranteesCan { get; }
	}

	public interface IImportImporterProvider : IPartyProvider
	{
		ZBool IsIndividual { get; }
	}

	public interface IImportDeclarantPartyIdProvider : IPartyNameProvider
	{
		ZString Type { get; }
		ZBool IsAuthorized { get; }
	}

	public interface IImportCommonLine
	{
		ZInt LineNumber { get; }
		IReadOnlyCollection<ZString> Containers { get; }
		ZString GoodsDescription { get; }
		ZString TariffCode { get; }
		ZString OriginCountry { get; }
		ZString RequestedCPC { get; }
		ZString PreviousCPC { get; }
	}

	public interface IDUAImportCommonLine : IImportCommonLine
	{
		ZString ExternalPackagingType { get; }
		IReadOnlyCollection<IPackageCommonNumbers> InternalPackages { get; }
		IReadOnlyCollection<IVehicleCommon> Vehicles { get; }
		ZString OtherMeasurementUnitsCode { get; }
		ZDecimal OtherMeasurementUnitsNumber { get; }
		IReadOnlyCollection<ZString> TariffSupplementaryCodes { get; }
		ZString ProductTitleForSpecialTaxes { get; }
		ZString SpecialTaxesIndicator { get; }
		ZDecimal GrossWeightInKG { get; }
		ZString PreferenceCode { get; }
		ZString ReductionCode { get; }
		IReadOnlyCollection<ZString> ConcessionsCPC { get; }
		ZDecimal NetWeightInKG { get; }
		ZString Contingency { get; }
		ZString PrecedentDocumentType { get; }
		ZString PrecedentDocumentClass { get; }
		ZString PrecedentDocumentReference { get; }
		ZString SupplementaryUnitsCode { get; }
		ZDecimal SupplementaryUnitsNumber { get; }
		ZDecimal InvoiceValue { get; }
		IReadOnlyCollection<IImportCommonC44CertificateDocument> DocumentsAndCertificates { get; }
		IReadOnlyCollection<ZString> SpecialInstructions { get; }
		IReadOnlyCollection<IDUAImportDeclaredTax> DeclaredTaxes { get; }
		ZDecimal TotalValue { get; }
	}

	public interface IImportCommonC44CertificateDocument : IDocumentsCommon
	{
		ZString CertQuantityUnit { get; }
		ZDecimal CertQuantityAmount { get; }
		ZDateTime CertDate { get; }
	}

	public interface IDUAImportDeclaredTax
	{
		ZString TaxClass { get; }
		ZDecimal TaxableIncome { get; }
		ZDecimal TaxRate { get; }
		ZString MaxMinIndicator { get; }
		ZString FiscalUnit { get; }
		ZDecimal Fee { get; }
	}
}
