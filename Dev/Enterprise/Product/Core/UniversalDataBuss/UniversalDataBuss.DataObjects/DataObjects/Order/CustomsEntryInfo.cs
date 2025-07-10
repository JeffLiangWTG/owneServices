using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public class CustomsEntryInfo : IDataObject
	{
		[MaxLength(35)]
		public ZString? EntryKey { get; set; }
		public ZShort? EntryLineNumber { get; set; }
		[MaxLength(35)]
		public ZString? InwardsEntryKey { get; set; }
		public ZShort? InwardsEntryLineNumber { get; set; }
		public ZDateTime? EntryDate { get; set; }
		[MaxLength(35)]
		public ZString? DeclarationReference { get; set; }
		public Country CountryOfOrigin { get; set; }
		public ZDecimal? CustomsQuantity { get; set; }
		public CodeDescriptionPair6Char CustomsQuantityUnit { get; set; }
		public ZDecimal? ValueForDuty { get; set; }
		public ZDecimal? TILV { get; set; }
		[MaxLength(2048)]
		public ZString? AdditionalInformation { get; set; }
		public ZDecimal? CustomsSecondQuantity { get; set; }
		public CodeDescriptionPair6Char CustomsSecondUnitQty { get; set; }
		public ZDecimal? CustomsThirdQuantity { get; set; }
		public CodeDescriptionPair6Char CustomsThirdUnitQty { get; set; }
		public ZDecimal? CustomsFourthQuantity { get; }
		public CodeDescriptionPair6Char CustomsFourthQuantityUnit { get; }
		public ZDecimal? CustomsFifthQuantity { get; }
		public CodeDescriptionPair6Char CustomsFifthQuantityUnit { get; }
		public OrganizationAddress ManufacturerAddress { get; set; }

		[MaxLength(35)]
		public ZString? Tariff { get; set; }
		[MaxLength(10)]
		public ZString? PrimaryPreference { get; set; }
		public CodeDescriptionPair ZoneStatus { get; set; }
		public ZBool? IsFromOtherFTZWarehouse { get; set; }
		public CodeDescriptionPair OutwardType { get; set; }

		public ZDateTime? CustomsDeadline { get; set; }
		[MaxLength(7)]
		public ZString? InwardStyle { get; set; }
		[MaxLength(7)]
		public ZString? InwardProcedure { get; set; }
		[MaxLength(38)]
		public ZString? DataImportMatchingKey { get; set; }

		public ZBool? IsMainInwardsProcessedItem { get; set; }
		public ZBool? IsSecondaryInwardsProcessedItem { get; set; }
		public ZDecimal? TotalLiabilityAmount { get; set; }
		public ZDecimal? AllDutiesAmount { get; set; }
		public ZDecimal? VATAmount { get; set; }
	}
}
