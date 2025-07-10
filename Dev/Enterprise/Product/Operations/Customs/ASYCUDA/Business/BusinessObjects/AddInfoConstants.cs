namespace Enterprise.Customs.ASYCUDA.Business;

public static class AddInfoConstants
{
	public static class Header
	{
		public const string CustomsOffice = "CustomsOffice";
		public const string ConveyanceNationality = "ConveyanceNationality";
		public const string SpecificCircumstanceIndicator = "SpecificCircumstanceIndicator";
		public const string ManifestNumber = "ManifestNumber";
		public const string MasterInformation = "MasterInformation";
		public const string Trailer1 = "Trailer1";
		public const string Trailer2 = "Trailer2";
		public const string Trailer1CountryOfRegistration = "Trailer1CountryOfRegistration";
		public const string Trailer2CountryOfRegistration = "Trailer2CountryOfRegistration";
		public const string RadioCallSign = "VesselRadioCallSign";
		public const string ReEntryIndicator = "ReEntryIndicator";
		public const string SplitConsignmentIndicator = "SplitConsignmentIndicator";
		public const string VesselCarrierCode = nameof(VesselCarrierCode);
		public const string VesselScreeningStatus = nameof(VesselScreeningStatus);
		public const string VesselVesselType = nameof(VesselVesselType);
		public const string VesselYearOfConstruction = nameof(VesselYearOfConstruction);
		public const string VesselNetTonnage = nameof(VesselNetTonnage);
	}

	public static class Bill
	{
		public const string ABL_LocationOfGoods = "ABL_LocationOfGoods";
		public const string ABL_LocationInformation = "ABL_LocationInformation";
		public const string ABL_MarksAndNumbers = "ABL_MarksAndNumbers";
		public const string ABL_ShipmentType = "ABL_ShipmentType";
		public const string ABL_UCRNumber = "ABL_UCRNumber";
		public const string MatchingReference = "MatchingReference";
		public const string DutyAmount = "DutyAmount";
		public const string TaxAmount = "TaxAmount";
		public const string TransportDocumentType = "TransportDocumentType";
	}

	public static class Pack
	{
		public const string PackAddInfoType = "PAC";
		public const string PackAddInfoTypeDescription = "Pack Lines";
		public const string ConsignmentReference = "ConsignmentReference";
		public const string MatchingReference = "MatchingReference";
	}

	public static class PackedItem
	{
		public const string PackingItemAddInfoType = "PAC";
		public const string PackingItemAddInfoTypeDescription = "Asycuda Country Specific Packing Item Details";
		public const string Country = "Country";
		public const string CustomsNumber = "CustomsNumber";
		public const string CustomsValue = "CustomsValue";
		public const string DutyValue = "DutyValue";
		public const string TaxValue = "TaxValue";
		public const string CountryOfDestination = "CountryOfDestination";
		public const string CustomsQty = "CustomsQty";
		public const string CustomsUQ = "CustomsUQ";
	}
}
