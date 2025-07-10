
namespace Enterprise.Client.AUS
{
	public abstract class AUSConstants
	{
		#region ClientAUSProductInterface

		public const string DropClientAUSProductInterfaceTable = "DROP TABLE ClientAUSProductInterface";
		public const string ClientAUSProductInterfaceTable = @"CREATE TABLE ClientAUSProductInterface (
									T5_PK					UniqueIdentifier NOT NULL DEFAULT NEWID(),
									T5_OH_Importer			UniqueIdentifier NOT NULL,
									T5_OH_Supplier			UniqueIdentifier NOT NULL,
									T5_OriginalProductCode	varchar(30) NOT NULL DEFAULT '',
									T5_OriginalProductDesc	varchar(80) NOT NULL DEFAULT '',
									T5_OriginalClass		varchar(35),
									T5_OP					UniqueIdentifier NOT NULL,
									T5_ImportDateTime       datetime,
									T5_NewProductCode		varchar(30),
									T5_NewProductDesc		varchar(80),
									T5_NewClass			    varchar(35),
									T5_ClosedDateTime       datetime,
									T5_ModifiedDateTime     datetime,
									T5_Indicator            char(1),
									T5_Status			    varchar(25),
									CONSTRAINT PK_UX_ClientAUSProductInterface PRIMARY KEY NONCLUSTERED (T5_PK))";

		#endregion

		#region ClientAUSProductImportRegistry

		public const string DropClientAUSProductImportRegistryTable = "DROP TABLE ClientAUSProductImportRegistry";
		public const string ClientAUSProductImportRegistryTable = @"CREATE TABLE ClientAUSProductImportRegistry (
									T6_PK						UniqueIdentifier NOT NULL DEFAULT NEWID(),
									T6_OH_Importer				UniqueIdentifier NOT NULL,
									T6_OH_Supplier				UniqueIdentifier NOT NULL,
									T6_DirectoryToStoreFiles	varchar(120) NOT NULL DEFAULT '',
									T6_AllProductsFileName		varchar(20) NOT NULL DEFAULT '',
									T6_ClientInvoicingFileName	varchar(20) NOT NULL DEFAULT '',
									T6_ProductUpdateFileName	varchar(20) NOT NULL DEFAULT '',
									T6_Email					varchar(80) DEFAULT '',
									T6_BackUpFileName			varchar(20) NOT NULL DEFAULT '',
									T6_DirectoryImportedParts	varchar(120) NOT NULL DEFAULT '',
									T6_DirectoryRejectedParts	varchar(120) NOT NULL DEFAULT '',
									CONSTRAINT PK_UX_ClientAUSProductImportRegistry PRIMARY KEY NONCLUSTERED (T6_PK))";

		#endregion

		#region ClientAUSOriginPreferenceMapping

		public const string DropClientAUSOriginPreferenceMappingTable = "DROP TABLE ClientAUSOriginPreferenceMapping";
		public const string ClientAUSOriginPreferenceMappingTable = @"CREATE TABLE ClientAUSOriginPreferenceMapping (
									T7_PK						UniqueIdentifier NOT NULL DEFAULT NEWID(),
									T7_OH_Importer				UniqueIdentifier NOT NULL,
									T7_OH_Supplier				UniqueIdentifier NOT NULL,
									T7_RN_NKOrigin				varchar(2) NOT NULL,
									T7_RN_NKPreferenceOrigin	varchar(2),
									T7_PreferenceSchemeType		varchar(4),
									T7_PreferenceRuleType		varchar(4),
									CONSTRAINT PK_UX_ClientAUSOriginPreferenceMapping PRIMARY KEY NONCLUSTERED (T7_PK))
									CREATE UNIQUE INDEX NR_UX__T7_OH_Importer__T7_OH_Supplier__T7_RN_NKOrigin ON ClientAUSOriginPreferenceMapping (T7_OH_Importer, T7_OH_Supplier, T7_RN_NKOrigin)";

		#endregion

		#region Data Mapping

		public const int RowFieldCount = 7;
		public const int InvoiceNumber = 0;
		public const int InvoiceNumberStartIndex = 0;
		public const int InvoiceNumberLength = 10;
		public const int PartNumber = 1;
		public const int PartNumberStartIndex = 10;
		public const int PartNumberLength = 15;
		public const int PartDescription = 2;
		public const int PartDescriptionStartIndex = 25;
		public const int PartDescriptionLength = 30;
		public const int Quantity = 3;
		public const int QuantityStartIndex = 55;
		public const int QuantityLength = 7;
		public const int LinePrice = 4;
		public const int LinePriceStartIndex = 62;
		public const int LinePriceLength = 9;
		public const int Classification = 5;
		public const int ClassificationStartIndex = 71;
		public const int ClassificationLength = 7;
		public const int Origin = 6;
		public const int OriginStartIndex = 78;
		public const int OriginLength = 2;

		public const string EndLine = "END       ";
		public const string EOFIndicator = "";

		#endregion
	}
}
