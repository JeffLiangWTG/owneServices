
namespace Enterprise.Client.NZP
{
	public static class Constants
	{
		public const int NumberOfFields = 36;
		public const string RecordType = "I";
		public const string LineItem = "01";
		public const string PricingIndicator = "1";
		public const string Quantity = "1.00";
		public const string CashTransactionCode = "PSRC";
		public const string CreditTransactionCode = "PSRD";
		public const string CreditNotesTransactionCode = "CSRD";
		public const string CashCreditNotesTransactionCode = "CSRC";

		public const string ImportAirFreight = "DIAI";
		public const string ImportSeaFreight = "DISI";
		public const string ExportAirFreight = "DIAE";
		public const string ExportSeaFreight = "DISE";
		public const string TransTrasmanFreighter = "DITF";
		public const string CustomsClearance = "DICC";

		public const string CMSMenuItem = "Weekly CMS Data Export Files";
		public const string FileNameFormat = "yyyyMMddHHmm";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "New Zealand Post DateFormat, not to be changed if ZDateTime.ToShortDateString changes")]
		public const string DateFormat = "dd-MMM-yyyy"; // New Zealand Post DateFormat, not to be changed if ZDateTime.ToShortDateString changes
	}
}
