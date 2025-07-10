
namespace Enterprise.Client.Wow.ServiceTasks.DeclarationInvoice
{
	public static class Constants
	{
		#region BodyRecord

		public static class BodyRecord
		{
			public const int FieldsCount = 7;

			public const int IndicateBodyRecord = 0;
			public const int IndicateBodyRecordMaxLength = 1;
			public const int JobNumber = 1;
			public const int JobNumberMaxLength = 9;
			public const int InvoiceNumberForLine = 2;
			public const int InvoiceNumberForLineMaxLength = 18;
			public const int IndentPONumber = 3;
			public const int IndentPONumberMaxLength = 12;
			public const int MaterialProductCode = 4;
			public const int MaterialProductCodeMaxLength = 18;
			public const int UnitPrice = 5;
			public const int UnitPriceMaxLength = 12;
			public const int Quantity = 6;
			public const int QuantityMaxLength = 10;
		}

		#endregion

		#region NotificationString

		public const string SuccessfullyExport = "Successfully exported commercial invoice Line data.";
		public const string NothingToExport = "Nothing to export.";
		public const string ExportDirectoryHasInvalidPath = "Registry item 'Export Commercial Invoice Data->Export Directory' has invalid path.";

		#endregion

	}
}
