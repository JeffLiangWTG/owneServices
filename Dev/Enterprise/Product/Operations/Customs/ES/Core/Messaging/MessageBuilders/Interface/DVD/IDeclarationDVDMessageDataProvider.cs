using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IDeclarationDVDMessageDataProvider : IDVDCommonDataProvider
	{
		IDeclarationDVDHeader Header { get; }
		IReadOnlyCollection<IDeclarationDVDLine> Lines { get; }
	}

	public interface IDeclarationDVDHeader
	{
		ZString CustomsOffice { get; }
		ZString LRN { get; }
		ZString DeclarationType { get; }
		ZString DeclarationSubType { get; }
		ZDecimal TotalGrossMass { get; }
		ZString ExporterId { get; }
		ZString ConsigneeId { get; }
		ZString DeclarantUECode { get; }
		ZString DeclarantId { get; }
		ZString DeclarationEmail { get; }
		ZString DeclarationOtherEmail { get; }
		ZString RepresentativeId { get; }
		ZString RepresentativeType { get; }
		IReadOnlyCollection<IDeclarationDVDAuthorisation> Authorisations { get; }
		ZString TransportCode { get; }
		IReadOnlyCollection<IAdditionalSupplyChainActorCommon> AdditionalSupplyActors { get; }
		IReadOnlyCollection<IDeclarationDVDSupportingDocument> SupportingDocuments { get; }
		ZBool IsContainerised { get; }
		IDeclarationDVDLocationOfGoods LocationOfGoods { get; }
		ZString CountryOfDestination { get; }
		ZString CountryOfExport { get; }
		ZString UCRReferenceNumber { get; }
		IWarehouseCommon Warehouse { get; }
		IReadOnlyCollection<IDeclarationDVDPreviousDocument> PreviousDocuments { get; }
		IReadOnlyCollection<IDeclarationDVDGuarantee> Guarantees { get; }
		ZString CustomOfficeOfPresentation { get; }
	}

	public interface IDeclarationDVDLine
	{
		ZString LineNumber { get; }
		IReadOnlyCollection<IDeclarationDVDSupportingDocumentForLine> SupportingDocuments { get; }
		IReadOnlyCollection<IDeclarationDVDAdditionalInfo> AdditionalInfos { get; }
		IReadOnlyCollection<IAdditionalSupplyChainActorCommon> AdditionalSupplyActors { get; }
		ZString Description { get; }
		ZString CusCode { get; }
		ZString TariffCode { get; }
		ZString TariffCodeCombined { get; }
		IReadOnlyCollection<ZString> TariffAdditionalCodes { get; }
		IReadOnlyCollection<ZString> NationalAdditionalCodes { get; }
		ZString PreferenceCode { get; }
		ZString ReductionCode { get; }
		IReadOnlyCollection<IDeclarationDVDTax> Taxes { get; }
		ZDecimal NetMass { get; }
		ZDecimal GrossMass { get; }
		ZDecimal SupplementaryUnitsQty { get; }
		IReadOnlyCollection<ZString> Containers { get; }
		ZString CountryOfDestination { get; }
		ZString CountryOfExport { get; }
		ZString RequestedCPC { get; }
		ZString PreviousCPC { get; }
		IReadOnlyCollection<IDeclarationDVDEUAndNationalCodes> AdditionalProcedures { get; }
		ZString CountryOfOrigin { get; }
		IReadOnlyCollection<IDVDCommonPackage> Packages { get; }
		IReadOnlyCollection<IDeclarationDVDVehicle> Vehicles { get; }
		IReadOnlyCollection<IDeclarationDVDPreviousDocument> PreviousDocuments { get; }
		ZString UCRReferenceNumber { get; }
	}

	public interface IDeclarationDVDAuthorisation
	{
		ZString Type { get; }
		ZString OwnerId { get; }
	}

	public interface IDeclarationDVDSupportingDocument : IDeclarationDVDEUAndNationalCodes
	{
		ZString Number { get; }
		ZDateTime DocumentDate { get; }
	}

	public interface IDeclarationDVDSupportingDocumentForLine : IDeclarationDVDSupportingDocument
	{
		ZString UnitOfMeasure { get; }
		ZDecimal Quantity { get; }
		ZString Currency { get; }
		ZDecimal Amount { get; }
	}

	public interface IDeclarationDVDPreviousDocument : IDocumentsCommon
	{
		ZString LineNumber { get; }
		ZString UnitOfMeasure { get; }
		ZDecimal Quantity { get; }
	}

	public interface IDeclarationDVDAdditionalInfo : IDeclarationDVDEUAndNationalCodes
	{
		ZString Description { get; }
	}

	public interface IDeclarationDVDLocationOfGoods
	{
		ZString LocationCountry { get; }
		ZString LocationType { get; }
		ZString LocationQualifier { get; }
		ZString LocationId { get; }
		ZString LocationAdditionalId { get; }
		ZString LocationAddress { get; }
		ZString LocationCity { get; }
		ZString LocationPostCode { get; }
	}

	public interface IDeclarationDVDGuarantee
	{
		ZString GRNReference { get; }
		ZString NoGRNReference { get; }
		ZString AccessCode { get; }
		ZString Currency { get; }
		ZDecimal Amount { get; }
		ZString Office { get; }
	}

	public interface IDeclarationDVDTax
	{
		ZString BaseUnit { get; }
		ZDecimal BaseQuantity { get; }
		ZDecimal BaseAmount { get; }
	}

	public interface IDeclarationDVDVehicle : IVehicleCommon
	{
		ZString Type { get; }
	}

	public interface IDeclarationDVDEUAndNationalCodes
	{
		ZString EUCode { get; }
		ZString NationalCode { get; }
	}
}
