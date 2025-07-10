using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Module
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	public class DeclarationFilterConstants : Customs.Module.DeclarationFilterConstants
	{
		public const string EntryType = "Entry Type";
		public const string LocationOfGoods = "Location of Goods";
		public const string CTStatus = "CT Status";
		public const string EntryStyle = "Entry Style";
		public const string EntrySubstyle = "Entry Sub-style";
		public const string ExitPresentationStatus = "Exit Presentation Status";
		public const string DeclarationType = "Declaration Type";
		public const string IndirectExport = "Indirect Export";
		public const string SupportingDocumentType = "Supporting Document Type";
		public const string SupportingDocumentRef = "Supporting Document Reference";
		public const string SupportingDocumentIssueDate = "Supporting Document Issue Date";
		public const string SupportingDocumentExpiryDate = "Supporting Document Expiry Date";
		public const string PreviousDocumentClass = "Previous Document Class";
		public const string PreviousDocumentType = "Previous Document Type";
		public const string PreviousDocumentRef = "Previous Document Reference";
		public const string PreviousDocumentLineNo = "Previous Document Line No";
		public const string OriginInvoiceLine = "Origin - Invoice Line";

		public static ResourceString RequestedProcedureMultilingualDescription => ResString.GetMultilingualString("BD1B5AE7-1E56-45D2-AECE-606691BCB1F9", DeclarationFilterConstants.RequestedProcedure);
		public const string RequestedProcedure = "Requested Procedure";

		public static ResourceString PreviousProcedureMultilingualDescription => ResString.GetMultilingualString("A63DC730-D399-4D00-BE77-A03B663635AA", DeclarationFilterConstants.PreviousProcedure);
		public const string PreviousProcedure = "Previous Procedure";

		public static ResourceString AdditionalProcedureMultilingualDescription => ResString.GetMultilingualString("EF9D6B7E-2B31-4E4E-918F-0FC7A3146B05", DeclarationFilterConstants.AdditionalProcedure);
		public const string AdditionalProcedure = "Additional Procedure";

		public new class NumberFilterTypes : Customs.Module.DeclarationFilterConstants.NumberFilterTypes
		{
			public const string CustomsProcedureCode = "Customs Procedure Code (CPC)";
			public const string Vin = "VIN";
			public const string VehicleBrand = "Vehicle Brand";
			public const string VehicleModel = "Vehicle Model";
			public const string CustomsOffice = "Customs Office";
			public const string GuaranteeReference = "Guarantee Reference";
			public const string PackingType = "Packing Type";
			public const string PackingMarks = "Packing Marks";
			public const string SealNumber = "Seal #";
			public const string Preference = "Preference";
		}

		public new class DateFilterTypes : Customs.Module.DeclarationFilterConstants.DateFilterTypes
		{
			public const string ClearanceDate = "Clearance Date";
		}

		public class InlandTransportDetails : Customs.Module.DeclarationFilterConstants
		{
			public const string TransportIdInland = "Transport ID (inland)";
			public const string TransportModeInland = "Transport Mode (inland)";
			public const string TransportNationality = "Transport Nationality";
			public const string TransportNationalityInland = "Transport Nationality (inland)";
		}
	}
}
