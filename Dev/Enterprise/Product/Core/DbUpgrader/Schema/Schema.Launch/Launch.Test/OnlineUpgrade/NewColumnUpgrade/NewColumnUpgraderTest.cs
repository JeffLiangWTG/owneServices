using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing
{
	[UseSnapshotProtection]
	class NewColumnUpgraderTest : TestCase
	{
		public void TestNewColumnUpgrade()
		{
			// NOT NULL Columns of type varchar(max), nvarchar(max), varbinary(max), xml, text, ntext, image, hierarchyid, geometry, geography, or CLR UDTS, can't be added in an online operation

			using ((Db.Instance as IDbUpgradeSupport).ElevateToAdminConnectionForUpgrade())
			{
				var upgrader = new NewColumnUpgrader(manager, Db.DatabaseName, templateDb);
				AssertEquals("Column TST_NewColum_Nullable exists?", false, ColumnExists(Db.Connection, "TST_NewColum_Nullable"));
				AssertEquals("Column TST_NewColum_HasDefault exists?", false, ColumnExists(Db.Connection, "TST_NewColum_HasDefault"));
				AssertEquals("Column TST_NewColum_HasNoDefault exists?", false, ColumnExists(Db.Connection, "TST_NewColum_HasNoDefault"));
				AssertEquals("Column TST_NewColum_Identity exists?", false, ColumnExists(Db.Connection, "TST_NewColum_Identity"));
				AssertEquals("Column TST_NewColum_Computed_HasParent exists?", false, ColumnExists(Db.Connection, "TST_NewColum_Computed_HasParent"));
				AssertEquals("Column TST_NewColum_Computed_HasNoParent exists?", false, ColumnExists(Db.Connection, "TST_NewColum_Computed_HasNoParent"));
				AssertEquals("Column TST_NewColum_Sparse exists?", false, ColumnExists(Db.Connection, "TST_NewColum_Sparse"));

				// NOT NULL offline-only types
				AssertEquals("Column TST_varchar_max exists?", false, ColumnExists(Db.Connection, "TST_varchar_max"));
				AssertEquals("Column TST_nvarchar_max exists?", false, ColumnExists(Db.Connection, "TST_nvarchar_max"));
				AssertEquals("Column TST_varbinary_max exists?", false, ColumnExists(Db.Connection, "TST_varbinary_max"));
				AssertEquals("Column TST_xml exists?", false, ColumnExists(Db.Connection, "TST_xml"));
				AssertEquals("Column TST_geography exists?", false, ColumnExists(Db.Connection, "TST_geography"));

				// not supported types
				AssertEquals("Column TST_text exists?", false, ColumnExists(Db.Connection, "TST_text"));
				AssertEquals("Column TST_ntext exists?", false, ColumnExists(Db.Connection, "TST_ntext"));
				AssertEquals("Column TST_image exists?", false, ColumnExists(Db.Connection, "TST_image"));
				AssertEquals("Column TST_hierarchyid exists?", false, ColumnExists(Db.Connection, "TST_hierarchyid"));
				AssertEquals("Column TST_geometry exists?", false, ColumnExists(Db.Connection, "TST_geometry"));

				upgrader.Run();

				AssertEquals("Column TST_NewColum_Nullable exists?", true, ColumnExists(Db.Connection, "TST_NewColum_Nullable"));
				AssertEquals("Column TST_NewColum_HasDefault exists?", true, ColumnExists(Db.Connection, "TST_NewColum_HasDefault"));
				AssertEquals("Column TST_NewColum_HasNoDefault exists?", false, ColumnExists(Db.Connection, "TST_NewColum_HasNoDefault"));
				AssertEquals("Column TST_NewColum_Identity exists?", false, ColumnExists(Db.Connection, "TST_NewColum_Identity"));
				AssertEquals("Column TST_NewColum_Computed_HasParent exists?", true, ColumnExists(Db.Connection, "TST_NewColum_Computed_HasParent"));
				AssertEquals("Column TST_NewColum_Computed_HasNoParent exists?", false, ColumnExists(Db.Connection, "TST_NewColum_Computed_HasNoParent"));

				// SPASE columns are supported
				AssertEquals("Column TST_NewColum_Sparse exists?", true, ColumnExists(Db.Connection, "TST_NewColum_Sparse"));
				AssertEquals(
					"TST_NewColum_Sparse column's info is expected",
					true,
					GetColumnInfo(Db.Connection, "TST_NewColum_Sparse")
						.ToHashSet()
						.IsSupersetOf(new List<(string Name, object Value)>
						{
							("ColName", "TST_NewColum_Sparse"),
							("ColType", "char"),
							("ColLength", 1),
							("ColNullOrNotNull", "NULL"),
							("ColSparse", true),
						}));

				AssertEquals("Column TST_NewColum_Sparse2 exists?", true, ColumnExists(Db.Connection, "TST_NewColum_Sparse2"));
				AssertEquals(
					"TST_NewColum_Sparse2 column's info is expected",
					true,
					GetColumnInfo(Db.Connection, "TST_NewColum_Sparse2")
						.ToHashSet()
						.IsSupersetOf(new List<(string Name, object Value)>
						{
							("ColName", "TST_NewColum_Sparse2"),
							("ColType", "decimal"),
							("ColLength", 9),
							("ColNullOrNotNull", "NULL"),
							("ColSparse", true),
							("ColPrecision", (byte)19),
							("ColScale", (byte)5),
						}));

				// NOT NULL offline-only types should be ignored
				AssertEquals("Column TST_varchar_max exists?", false, ColumnExists(Db.Connection, "TST_varchar_max"));
				AssertEquals("Column TST_nvarchar_max exists?", false, ColumnExists(Db.Connection, "TST_nvarchar_max"));
				AssertEquals("Column TST_varbinary_max exists?", false, ColumnExists(Db.Connection, "TST_varbinary_max"));
				AssertEquals("Column TST_xml exists?", false, ColumnExists(Db.Connection, "TST_xml"));
				AssertEquals("Column TST_geography exists?", false, ColumnExists(Db.Connection, "TST_geography"));

				// not supported types should be ignored
				AssertEquals("Column TST_text exists?", false, ColumnExists(Db.Connection, "TST_text"));
				AssertEquals("Column TST_ntext exists?", false, ColumnExists(Db.Connection, "TST_ntext"));
				AssertEquals("Column TST_image exists?", false, ColumnExists(Db.Connection, "TST_image"));
				AssertEquals("Column TST_hierarchyid exists?", false, ColumnExists(Db.Connection, "TST_hierarchyid"));
				AssertEquals("Column TST_geometry exists?", false, ColumnExists(Db.Connection, "TST_geometry"));

				AssertContains("Warning: Invalid column name 'TST_NewColum_HasNoDefault'.", string.Join(System.Environment.NewLine, manager.OutputTextCollection.Cast<string>()));
			}
		}

		#region Implementation

		bool ColumnExists(DbConnection connection, string columnName)
		{
			return connection.Exists("FROM sys.columns WHERE name = @columnName"
				, cmd =>
				{
					cmd.AddParameter("@columnName", SqlDbType.NVarChar, 128, columnName);
				});
		}

		static List<(string Name, object Value)> GetColumnInfo(DbConnection connection, string columnName)
		{
			var results = new List<(string Name, object Value)>();
			connection.ExecuteReader(
				@"
SELECT
	TabSchema                 = CurSch.name,
	TabName                   = CurTab.name,
	ColName                   = CurCol.name,
	ColType                   = CurTyp.name,
	ColLength                 = IIF(CurCol.max_length > 0 AND CurTyp.name in (N'nchar', N'nvarchar'), CurCol.max_length / 2, CurCol.max_length),
	ColNullOrNotNull          = IIF(CurCol.is_nullable = 1, 'NULL', 'NOT NULL'),
	ColSparse                 = CurCol.is_sparse,
	ColPrecision              = CurCol.precision,
	ColScale                  = CurCol.scale,
	ColDefault                = CurDefault.definition,
	ColXmlSchema              = CurXmlSchema.name,
	IdentitySeed              = CurIdCol.seed_value,
	IdentityIncrement         = CurIdCol.increment_value,
	IsComputedColumnChange    = CurCol.is_computed,
	ComputedColumnDefinition  = CurCptCol.definition,
	IsPersistedComputedColumn = CurCptCol.is_persisted
FROM
	sys.schemas                           AS CurSch
	JOIN sys.tables                       AS CurTab ON CurTab.schema_id = CurSch.schema_id
	JOIN sys.columns                      AS CurCol ON CurCol.object_id = CurTab.object_id
	JOIN sys.types                        AS CurTyp ON CurTyp.user_type_id = CurCol.user_type_id
	LEFT JOIN sys.default_constraints     AS CurDefault ON CurDefault.object_id = CurCol.default_object_id
	LEFT JOIN sys.identity_columns        AS CurIdCol ON CurIdCol.object_id = CurCol.object_id AND CurIdCol.column_id = CurCol.column_id
	LEFT JOIN sys.computed_columns        AS CurCptCol ON CurCptCol.object_id = CurCol.object_id AND CurCptCol.column_id = CurCol.column_id
	LEFT JOIN sys.xml_schema_collections  AS CurXmlSchema ON CurXmlSchema.xml_collection_id = CurCol.xml_collection_id AND CurCol.xml_collection_id <> 0
WHERE 1=1
	AND CurCol.name = @columnName
",
				cmd =>
				{
					cmd.AddParameter("@columnName", SqlDbType.NVarChar, 128, columnName);
				},
				reader =>
				{
					for (var i = 0; i < reader.FieldCount; i++)
					{
						results.Add((reader.GetName(i), reader[i]));
					}
				});
			return results;
		}

		IAuxiliaryDbCreator templateDbCreator;
		readonly UpgradeManagerForTestWithOutputBuffer manager = new UpgradeManagerForTestWithOutputBuffer();

		readonly IEnumerable<string> pk = new string[] { "TST_PK uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED" };
		readonly IEnumerable<string> newColumnList = new string[]
		{
			"TST_NewColum_Nullable     char(1) NULL",
			"TST_NewColum_HasDefault   char(1) NOT NULL DEFAULT ('N')",
			"TST_NewColum_HasNoDefault char(1) NOT NULL",
			"TST_NewColum_Identity     int IDENTITY (1,1) NOT NULL",
			"TST_NewColum_Computed_HasParent   AS TST_NewColum_HasDefault + '_Computed'",
			"TST_NewColum_Computed_HasNoParent AS TST_NewColum_HasNoDefault + '_Computed'",
			"TST_NewColum_Sparse       char(1) Sparse NULL",
			"TST_NewColum_Sparse2      DECIMAL(19,5) SPARSE",

			// NOT NULL offline-only types
			"TST_varchar_max   varchar(max)   NOT NULL DEFAULT ''",
			"TST_nvarchar_max  nvarchar(max)  NOT NULL DEFAULT ''",
			"TST_varbinary_max varbinary(max) NOT NULL DEFAULT 0x",
			"TST_xml           xml            NOT NULL DEFAULT ''",
			"TST_geography     geography      NOT NULL DEFAULT CONVERT(geography, 'POLYGON EMPTY')",

			// not supported types
			"TST_text          text               NULL",
			"TST_ntext         ntext              NULL",
			"TST_image         image              NULL",
			"TST_hierarchyid   hierarchyid        NULL",
			"TST_geometry      geometry           NULL",
		};

		const string templateDb = "TestNewColumnUpgrader_TemplateDb";
		const string targetTable = "_TargetTable";

		protected override void SetUp()
		{
			base.SetUp();

			TablePreSynchroniser.CreatePreAddDb_ForTest();
			CreateTemplateDatabase();
			DropTestTable();
			CreateTestTable();
		}

		protected override void TearDown()
		{
			DropTestTable();
			DropTemplateDatabase();
			TablePreSynchroniser.DropPreAddDb_ForTest();

			base.TearDown();
		}

		void DropTestTable()
		{
			Db.Connection.ExecuteNonQuery(string.Format("if (OBJECT_ID(N'[dbo].[{0}]', N'U') is NOT NULL) DROP TABLE [dbo].[{0}];", targetTable));
		}

		void CreateTestTable()
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"-- CreateTestTable
CREATE TABLE [dbo].[{0}]
(
	{1}
);
"
				, targetTable                                                    // 0
				, string.Join(",\r\n\t", pk) // 1
				);

			Db.Connection.ExecuteNonQuery(sql);
		}

		void CreateTemplateDatabase()
		{
			templateDbCreator = new TablePreSynchroniserTestTemplateDbCreator(templateDb, targetTable, pk.Union(newColumnList));
			templateDbCreator.CreateDropExisting();
		}

		void DropTemplateDatabase()
		{
			templateDbCreator.Drop();
		}

		#endregion // Implementation
	}
}
