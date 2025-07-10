using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	[UseSnapshotProtection]
	sealed class ColumnSynchroniserOfflineOperationsTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		public void TestOfflineStrings()
		{
			// NOT NULL Columns of type varchar(max), nvarchar(max), varbinary(max), xml, text, ntext, image, hierarchyid, geometry, geography, or CLR UDTS, can't be added in an online operation

			// Offline-only compatible string types are ignored by "CharToChar" conversion
			// TST_1_char_2_varchar      char(10)     NOT NULL ->  varchar(max) NOT NULL -- ignored
			// TST_2_nchar_2_varchar     nchar(10)    NOT NULL ->  varchar(max) NOT NULL -- ignored (invalid conversion)
			// TST_3_varchar_2_varchar   varchar(10)  NOT NULL ->  varchar(max) NOT NULL -- ignored
			// TST_4_nvarchar_2_varchar  nvarchar(10) NOT NULL ->  varchar(max) NOT NULL -- ignored (invalid conversion)
			// TST_5_char_2_nvarchar     char(10)     NOT NULL -> nvarchar(max) NOT NULL -- ignored
			// TST_6_nchar_2_nvarchar    nchar(10)    NOT NULL -> nvarchar(max) NOT NULL -- ignored
			// TST_7_varchar_2_nvarchar  varchar(10)  NOT NULL -> nvarchar(max) NOT NULL -- ignored
			// TST_8_nvarchar_2_nvarchar nvarchar(10) NOT NULL -> nvarchar(max) NOT NULL -- ignored

			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_1_char_2_varchar", "char", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_2_nchar_2_varchar", "nchar", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_3_varchar_2_varchar", "varchar", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_4_nvarchar_2_varchar", "nvarchar", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_5_char_2_nvarchar", "char", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_6_nchar_2_nvarchar", "nchar", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_7_varchar_2_nvarchar", "varchar", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_8_nvarchar_2_nvarchar", "nvarchar", "NO", "10");

			var testSynchroniser = new ColumnSynchroniserForTesting(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testSynchroniser.ConvertCharToChar_Exposed);

			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_1_char_2_varchar", "char", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_2_nchar_2_varchar", "nchar", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_3_varchar_2_varchar", "varchar", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_4_nvarchar_2_varchar", "nvarchar", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_5_char_2_nvarchar", "char", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_6_nchar_2_nvarchar", "nchar", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_7_varchar_2_nvarchar", "varchar", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_8_nvarchar_2_nvarchar", "nvarchar", "NO", "10");
		}

		public void TestOfflineOperations()
		{
			// NOT NULL Columns of type varchar(max), nvarchar(max), varbinary(max), xml, text, ntext, image, hierarchyid, geometry, geography, or CLR UDTS, can't be added in an online operation

			// Offline-only string types
			// TST_1_char_2_varchar      char(10)     NOT NULL ->  varchar(max) NOT NULL
			// TST_2_nchar_2_varchar     nchar(10)    NOT NULL ->  varchar(max) NOT NULL
			// TST_3_varchar_2_varchar   varchar(10)  NOT NULL ->  varchar(max) NOT NULL
			// TST_4_nvarchar_2_varchar  nvarchar(10) NOT NULL ->  varchar(max) NOT NULL
			// TST_5_char_2_nvarchar     char(10)     NOT NULL -> nvarchar(max) NOT NULL
			// TST_6_nchar_2_nvarchar    nchar(10)    NOT NULL -> nvarchar(max) NOT NULL
			// TST_7_varchar_2_nvarchar  varchar(10)  NOT NULL -> nvarchar(max) NOT NULL
			// TST_8_nvarchar_2_nvarchar nvarchar(10) NOT NULL -> nvarchar(max) NOT NULL

			// Other Offline-only types
			// TST_varbinary_max varbinary(max) NOT NULL DEFAULT 0x,
			// TST_xml           xml            NOT NULL DEFAULT '',
			// TST_geography     geography      NOT NULL DEFAULT CONVERT(geography, 'POLYGON EMPTY'),
			// TST_text          text       ,
			// TST_ntext         ntext      ,
			// TST_image         image      ,
			// TST_hierarchyid   hierarchyid,
			// TST_geometry      geometry   ,

			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_1_char_2_varchar", "char", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_2_nchar_2_varchar", "nchar", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_3_varchar_2_varchar", "varchar", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_4_nvarchar_2_varchar", "nvarchar", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_5_char_2_nvarchar", "char", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_6_nchar_2_nvarchar", "nchar", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_7_varchar_2_nvarchar", "varchar", "NO", "10");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_8_nvarchar_2_nvarchar", "nvarchar", "NO", "10");

			var testSynchroniser = new ColumnSynchroniserForTesting(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testSynchroniser.DropAlterAndAddColumns);

			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_1_char_2_varchar", "varchar", "NO", "-1");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_2_nchar_2_varchar", "varchar", "NO", "-1");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_3_varchar_2_varchar", "varchar", "NO", "-1");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_4_nvarchar_2_varchar", "varchar", "NO", "-1");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_5_char_2_nvarchar", "nvarchar", "NO", "-1");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_6_nchar_2_nvarchar", "nvarchar", "NO", "-1");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_7_varchar_2_nvarchar", "nvarchar", "NO", "-1");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_8_nvarchar_2_nvarchar", "nvarchar", "NO", "-1");

			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_varbinary_max", "varbinary", "NO", "-1");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_xml", "xml", "NO", "-1");
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_geography", "geography", "NO", null);
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_text", "text", "YES", null);
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_ntext", "ntext", "YES", null);
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_image", "image", "YES", null);
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_hierarchyid", "hierarchyid", "YES", null);
			AssertColumnExistInMockMainDb("DboTableCharToCharTest", "TST_geometry", "geometry", "YES", null);
		}

		#region Implementation

		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockMainDb, new string[]
			{
				@"
				CREATE TABLE dbo.DboTableCharToCharTest
				(
					PK uniqueidentifier NOT NULL CONSTRAINT PK_DboTableCharToCharTest PRIMARY KEY NONCLUSTERED,

					TST_1_char_2_varchar      char(10)       NOT NULL DEFAULT '',
					TST_2_nchar_2_varchar     nchar(10)      NOT NULL DEFAULT '',
					TST_3_varchar_2_varchar   varchar(10)    NOT NULL DEFAULT '',
					TST_4_nvarchar_2_varchar  nvarchar(10)   NOT NULL DEFAULT '',
					TST_5_char_2_nvarchar     char(10)       NOT NULL DEFAULT '',
					TST_6_nchar_2_nvarchar    nchar(10)      NOT NULL DEFAULT '',
					TST_7_varchar_2_nvarchar  varchar(10)    NOT NULL DEFAULT '',
					TST_8_nvarchar_2_nvarchar nvarchar(10)   NOT NULL DEFAULT '',
				);

				",
			});
		}

		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, new string[]
			{
				@"
				CREATE TABLE dbo.DboTableCharToCharTest
				(
					PK uniqueidentifier NOT NULL CONSTRAINT PK_DboTableCharToCharTest PRIMARY KEY NONCLUSTERED,

					TST_1_char_2_varchar       varchar(max) NOT NULL DEFAULT '',
					TST_2_nchar_2_varchar      varchar(max) NOT NULL DEFAULT '',
					TST_3_varchar_2_varchar    varchar(max) NOT NULL DEFAULT '',
					TST_4_nvarchar_2_varchar   varchar(max) NOT NULL DEFAULT '',
					TST_5_char_2_nvarchar     nvarchar(max) NOT NULL DEFAULT '',
					TST_6_nchar_2_nvarchar    nvarchar(max) NOT NULL DEFAULT '',
					TST_7_varchar_2_nvarchar  nvarchar(max) NOT NULL DEFAULT '',
					TST_8_nvarchar_2_nvarchar nvarchar(max) NOT NULL DEFAULT '',

					TST_varbinary_max         varbinary(max) NOT NULL DEFAULT 0x,
					TST_xml                   xml            NOT NULL DEFAULT '',
					TST_geography             geography      NOT NULL DEFAULT CONVERT(geography, 'POLYGON EMPTY'),
					TST_text                  text       ,
					TST_ntext                 ntext      ,
					TST_image                 image      ,
					TST_hierarchyid           hierarchyid,
					TST_geometry              geometry   ,
				);

				",
			});
		}

		#endregion // Implementation
	}
}
