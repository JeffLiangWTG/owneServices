namespace Enterprise.Client.YAS.YASInvoiceImporter
{
	public static class InvoiceConstants
	{
		public const int NoOfFields = 5;
		public const string FileExtension = "dat";
		public static class Position
		{
			public const int InvoiceNo = 0;
			public const int PartNo = 10;
			public const int PartDescription = 30;
			public const int Qty = 60;
			public const int Value = 68;
		}

		public static class Length
		{
			public const int InvoiceNo = 10;
			public const int PartNo = 20;
			public const int PartDescription = 30;
			public const int Qty = 8;
			public const int Value = 12;
		}

		public static class FixedFieldPosition
		{
			public const int InvoiceNo = 0;
			public const int PartNo = 1;
			public const int PartDescription = 2;
			public const int Qty = 3;
			public const int Value = 4;
		}
	}
}
