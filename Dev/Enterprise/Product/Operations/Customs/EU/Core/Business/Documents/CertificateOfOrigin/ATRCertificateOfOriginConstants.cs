namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public static class ATRCertificateOfOriginConstants
	{
		public static class Length
		{
			public const int MaxLinesExporterInDocument = 4;
			public const int MaxLengthLineExporterInDocument = 30;
			public const int MaxLengthCountryOfExportationInDocument = 48;
			public const int MaxLengthCountryOfDestinationInDocument = 54;
			public const int MaxLengthTransportDetailsInDocument = 324;
			public const int MaxLinesItemsInDocument = 22;
			public const int MaxLengthLineItemsInDocument = 5;
			public const int MaxLengthLineMarksInDocument = 82;
			public const int MaxLinesMarksInDocument = 23;
			public const int MaxLinesGrossWeightInDocument = 22;
			public const int MaxLengthLineGrossWeightInDocument = 10;
			public const int MaxLength12FormInDocument = 20;
			public const int MaxLength12AATR1InDocument = 22;
			public const int MaxLength12CustomsInDocument = 40;
			public const int MaxLength12IssuingInDocument = 40;
			public const int MaxLength12PlaceInDocument = 21;
			public const int MaxLength13PlaceInDocument = 14;
			public const int MaxLength13SupplierDetailsInDocument = 66;
			public const string FieldForMaxLengthTest100 = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
			public const string FieldForMaxLengthTest50 = "1234567890123456789012345678901234567890123456789";
		}
	}
}
