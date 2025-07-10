using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	public interface IWarehouseCustomsLineDetails
	{
		CommercialInvoiceLine InvoiceLine { get; }
		Country CountryOfOrigin { get; }
		ZDecimal? CustomsQuantity { get; }
		CodeDescriptionPair6Char CustomsQuantityUnit { get; }
		ZShort? EntryLineNumber { get; }
		ZString? EntryNumber { get; }
		ZShort? PreviousEntryLineNumber { get; }
		ZString? PreviousEntryNumber { get; }
		ZString? OrderNumber { get; }
		ZInt? OrderLineNo { get; }
		ZDecimal TILV { get; }
		ZDecimal ValueForDuty { get; }
		ZString AddInfos { get; }
		IEnumerable<IWarehouseCustomsLineAddInfo> AdditionalAddInfos { get; }
		IEnumerable<IWarehouseCustomsLinePackDetails> PackDetails { get; }
		IEnumerable<ZString> ExtraClassificationDetails { get; }
		OrganizationAddress SupplierAddress { get; }
		ZDecimal? CustomsSecondQuantity { get; }
		CodeDescriptionPair6Char CustomsSecondQuantityUnit { get; }
		ZString? Tariff { get; }
		ZString? PrimaryPreference { get; }

		ZString? NewOwnerProductCode { get; }
		ZString? NewOwnerPartAttribute1 { get; }
		ZString? NewOwnerPartAttribute2 { get; }
		ZString? NewOwnerPartAttribute3 { get; }
		ZString? NewOwnerSerialNumber { get; }

		IEnumerable<IWarehouseCustomsLineAllocationInfo> AllocationInfos { get; }

		ZDecimal? CustomsThirdQuantity { get; }
		CodeDescriptionPair6Char CustomsThirdQuantityUnit { get; }
		OrganizationAddress ManufacturerAddress { get; }

		ZDateTime? CustomsDeadline { get; }
		ZString? Style { get; }
		ZString? Procedure { get; }

		ZString? DataImportMatchingKey { get; }
		ZString? CountryOfDestination { get; }
		ZString? Remarks { get; }
		ZDecimal? CustomsFourthQuantity { get; }
		CodeDescriptionPair6Char CustomsFourthQuantityUnit { get; }
		ZDecimal? CustomsFifthQuantity { get; }
		CodeDescriptionPair6Char CustomsFifthQuantityUnit { get; }
	}
}
