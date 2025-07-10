
namespace Enterprise.Client.HEN.Nissan
{
	public static class NissanConstants
	{
		public const string NissanErrorMessage = "File has incorrect formt";

		public const string InvoiceHeaderLineType = "DHRD";
		public const string InvoiceLineLineType = "SDTL";

		public const int LineTypeLength = 4;
		public const int LineTypeStartIndex = 0;
		public const int LineTypePosition = 0;

		public static class InvoiceHeader
		{
			public const int InvoiceNo = 1;
			public const int Currency = 2;
			public const int TotalAmount = 3;
			public const int GrossWeight = 4;

			public const int FieldCount = 5;

			public static class Length
			{
				public const int InvoiceNo = 8;
				public const int Currency = 3;
				public const int TotalAmount = 12;
				public const int GrossWeight = 12;
			}

			public static class StartIndex
			{
				public const int InvoiceNo = 24;
				public const int Currency = 504;
				public const int TotalAmount = 507;
				public const int GrossWeight = 519;
			}
		}

		public static class InvoiceLine
		{
			public const int ProductCode = 1;
			public const int InvoiceQty = 2;
			public const int InvoicePrice = 3;
			public const int GrossWeight = 4;
			public const int OrderNo = 5;
			public const int OrderLineNo = 6;
			public const int GoodsOrigin = 7;

			public const int FieldCount = 8;

			public static class Length
			{
				public const int ProductCode = 20;
				public const int InvoiceQty = 6;
				public const int InvoicePrice = 11;
				public const int GrossWeight = 8;
				public const int OrderNo = 12;
				public const int OrderLineNo = 4;
				public const int GoodsOrigin = 20;
			}

			public static class StartIndex
			{
				public const int ProductCode = 20;
				public const int InvoiceQty = 80;
				public const int InvoicePrice = 97;
				public const int GrossWeight = 108;
				public const int OrderNo = 116;
				public const int OrderLineNo = 128;
				public const int GoodsOrigin = 132;
			}
		}
	}
}
