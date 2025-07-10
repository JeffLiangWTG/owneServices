namespace Enterprise.Customs.ES.Messaging
{
	public static class MessageSchema
	{
		public static class JobComInvoiceLineMessageSchema
		{
			public const int GoodsDescriptionMaxLengthExport = 350;
			public const int GoodsDescriptionMaxLengthExportAES = 512;
			public const int GoodsDescriptionMaxLengthEXS = 520;
			public const int GoodsDescriptionMaxLengthImportOrT2l = 250;
		}

		public static class NctsMessageSchema
		{
			public const int GoodsDescriptionMaxLengthDepartureAndTIR = 260;
		}
	}
}
