namespace Enterprise.Customs.IT.Business;

public static class Ucc6XmlConstants
{
	public static class LocationOfGoods
	{
		public static class Qualifier
		{
			public const string CustomsOffice = "V";
			public const string AuthorizationNumber = "Y";
			public const string Address = "Z";
		}

		public static class Role
		{
			public const string AuthorizedPlace = "B";
			public const string ApprovedPlace = "C";
			public const string OtherPlace = "D";
		}
	}

	public static class EntryHeader
	{
		public const int ImportEntryLinesCountLimit = 999;
		public const int ExportEntryLinesCountLimit = 9999;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant Import, Export, Ncts, Pnts")]
	public static class ServiceTypePrefix
	{
		public const string Import = "imp";
		public const string Export = "exp";
		public const string Ncts = "tns";
		public const string Pnts = "tns";
	}

	public static class PreviousDocument
	{
		public const int ReferenceNumberMaxLength = 35;
		public const int QuantityMaxLength = 16;
	}

	public static class CustomsFieldMaxLength
	{
		public static class Trader
		{
			public const int Name = 70;
			public const int Address = 70;
			public const int PostCodeExport = 10;
			public const int City = 35;
		}

		public static class TransitionPeriod
		{
			public static class Trader
			{
				public const int Name = 35;
				public const int Address = 35;
				public const int PostCode = 9;
			}
		}

		public static class Ucc6ExportInvoiceLineAdditionalInfo
		{
			public const int Description = 70;
			public const int ReferenceNumber = 35;
		}

		public static class Ucc6ExportInvoiceHeaderAdditionalInfo
		{
			public const int Description = 70;
			public const int ReferenceNumber = 35;
		}
	}

	public static class GeoLocationDetails
	{
		public const int CountryCodeLength = 2;

		public const int UNLOCodeLength = 5;
	}

	public static class CustomsClearRequests
	{
		public static class ServiceId
		{
			public const string Ivisto = "richiestaIvistoNonFirmata";
			public const string Irildes = "richiestaIrildesNonFirmata";
		}
	}

	public static class DocumentManagementServiceRequest
	{
		public static class ServiceIds
		{
			public const string EadTadRequest = "richiestaDaeDat";
			public const string EFStatusRequest = "richiestaListaDocumentiDichiarazione";
			public const string Eur1Request = "richiestaEur1";
			public const string ReleaseProspectusRequest = "richiestaProspettoSvincolo";
			public const string AccountingSummaryRequest = "richiestaProspettoContabile";
			public const string AccountingSummaryDownload = "downloadProspettoContabile";
			public const string SummaryProspectusRequest = "richiestaProspettoSintesi";
			public const string SummaryProspectusDownload = "downloadProspettoSintesi";
		}
	}
}
