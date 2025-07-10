
namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class CMRReferenceFileBuilderConstants
	{
		public const string EmailSubjectName = "AU CMR Reference Files Update";
		public const string ZipAttachmentName = "CMRReferenceFiles.zip";

		public const string CodeListFileName = "CDLST";
		public const string ExchangeRatesFileName = "XCHGRATE";
		public const string QuantityUnitConversionFileName = "QNTUTCNV";

		public const string ActionIndicator = "ActionIndicator";
		public const string CharactersToKeep = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz 0123456798.<>()*&^%$#@![]\|{}+=-_\/?,~`:;";

		public static class TablesMappings
		{
			public const string TableName = "TableMappings";
			public const string CustomsTableNameColumn = "CustomsTableName";
			public const string ReferenceFileTableNameColumn = "ReferenceFileTableName";
			public const string ReferenceFilePrefixColumn = "ReferenceFilePrefix";
			public const string CustomsFileNameColumn = "CustomsFileName";
		}

		public static class TablesStructure
		{
			public const string TableName = "TableStructure";
			public const string CustomsTableNameColumn = "CustomsTableName";
			public const string DataItemColumn = "DataItem";
			public const string StartPositionColumn = "StartPosition";
			public const string LengthColumn = "Length";
			public const string FormatColumn = "Format";
			public const string UniqueIndexColumn = "UniqueIndex";
		}
	}
}
