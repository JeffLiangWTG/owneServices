namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class ValidationCaptions
	{
		public static string CountryOrUNLOCOForPlaceOfLoading => Res.GetString("53B6ECFC-FF67-4DF0-AB1B-DEE77799B0F6", "Country/Region Code or an UNLOCO for Place of Loading");

		public static string PlaceOfLoading => Res.GetString("7BB4A6B8-3351-435B-98C2-E45A3C71CBEF", "Place of Loading");

		public static string UnlocoPlaceOfUnloadingOrCountry => Res.GetString("B534F440-AC2C-4518-B05C-FF0D17576778", "UNLOCO (Place Of Unloading) Or a Country");

		public static string UnlocoPortOfLoadingOrCountry => Res.GetString("21B1F8A8-9FAE-4104-A981-E3528DD662C7", "UNLOCO (Port Of Loading) Or a Country");

		public static string PlaceOfLoadingLocationDescription => Res.GetString("61AEA86A-E287-4A4B-B63D-FFAB5F72AEFE", "Place Of Loading Location Description");

		public static string PlaceOfUnLoadingLocationDescription => Res.GetString("1E68E84E-2378-4DFB-B46B-8D63F6C5B016", "Place Of Unloading Location Description");

		public static string LocationForPlaceOfLoadingDescription => Res.GetString("DC063EB9-FF8B-4DD3-8422-EF86032F59F6", "Location for the Place of Loading");

		public static string EmptyHeaderAndHouseValue => Res.GetString("8B9F424B-164C-43BD-B336-66CAA39D9813", "[C0001-6] A Consignee Address: Organization must be declared at header or house level.");

		public static string HeaderLevelWillBeIgnoredHousesHaveSameValue => Res.GetString("C0DAF76B-5DC2-4716-8503-6BEBF59A9C6B", "[C0001-6] The Consignee Address: Organization declared in the header will be ignored because all the house consignments have the same value, which differs from the value declared in the header.");

		public static string HeaderLevelWillBeIgnoredHousesHaveDifferentValues => Res.GetString("57C1D270-8210-4EC3-8CEC-7E64BABC86A2", "[C0001-6] The Consignee Address: Organization declared in the header will be ignored because all house consignments have values, which different from the header one.");
	}
}
