
namespace Enterprise.Client.STI.Navision
{
	public static class Constants
	{
		public const string DataExportReference = "Navision";
		public const string Invoice = "Invoice";
		public const string CreditNote = "CreditNote";
		public const string GLAccount = "G/L Account";
		public const int Quantity = 1;
		public const int CompletionPercentage = 0;
		public const string Status = "Order";
		public const string ApplicationMethod = "Manual";
		public const string JobUsagePosting = "None";
		public const string BlankSpaceForLastColumn = " ";
		public const string DateTimeFileNameFormat = "yyyyMMddmmHHss";
		public const string DataExportMenuCaption = "To Navision";
		public const string DateTimeFormat = "ddMMyyyy";

		public static class FileNamePrefixes
		{
			public const string ShipmentFiles = "JOB";
			public const string InvoiceHeaderFiles = "HDR";
			public const string InvoiceLineFiles = "LIN";
			public const string OrganisationFiles = "ORG";
		}
	}
}
