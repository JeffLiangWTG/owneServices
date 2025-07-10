using System.Collections.Generic;
using System.Data;
using System.Linq;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.PreUpgrade.Public.Freight.Testing
{
	sealed class RenameColumnTransformationTest : TransactionedTestCase
	{
		public void TestAllDependenciesWhichPreventColumnRenameAreRemoved()
		{
			PrepareTestData();

			AssertEquals("ColComputedSource exists?", true, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColComputedSource"));
			AssertEquals("ColComputed exists?", true, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColComputed"));

			new RenameColumnTransformationForTesting().Run();
			AssertTransformationResults();

			new RenameColumnTransformationForTesting().Run();
			AssertTransformationResults();
		}

		public void TestUserDescription_IsUsefulByDefault()
		{
			var transform = new SimpleRenameTransform();

			transform.RenameColumnInfoList_Settable = new[]
			{
				new RenameColumnTransformationInfo("ProcessHooter", "Stickball", "Fiddlefracks"),
			};

			AssertEquals("Renaming column ProcessHooter.Stickball -> Fiddlefracks", transform.UserDescription);

			transform.RenameColumnInfoList_Settable = new[]
			{
				new RenameColumnTransformationInfo("ProcessHooter", "Stickball", "Fiddlefracks"),
				new RenameColumnTransformationInfo("ProcessHedder", "ProfessorFrink", "Makes you laff, makes you think"),
			};

			AssertEquals("Renaming columns: ProcessHooter.Stickball -> Fiddlefracks; ProcessHedder.ProfessorFrink -> Makes you laff, makes you think", transform.UserDescription);
		}

		class SimpleRenameTransform : RenameColumnTransformation
		{
			internal IEnumerable<IRenameColumnTransformationInfo> RenameColumnInfoList_Settable { get; set; }

			public override IEnumerable<IRenameColumnTransformationInfo> RenameColumnInfoList => RenameColumnInfoList_Settable;
		}

		void PrepareTestData()
		{
			var sqlText = @"
CREATE TABLE [Table_RenameColumnTransformationTest]
(
	ColPk                         int NOT NULL,
	ColUk                         int NOT NULL,
	ColFk                         int NOT NULL,
	ColWithCheck                  int NOT NULL,
	ColWithDefault                int NOT NULL,
	ColWithIndex                  int NOT NULL,
	ColWithStats                  int NOT NULL,
	ColWithProgrammabilityObjects int NOT NULL,
	ColUk1                        int NOT NULL,
	ColUk2                        int NOT NULL,
	ColComputedSource             int NOT NULL,
	ColComputed                   AS ColComputedSource,
	ColSparse                     char(1) SPARSE,
);

ALTER TABLE [Table_RenameColumnTransformationTest] WITH CHECK ADD
	CONSTRAINT [PK_Table_RenameColumnTransformationTest]      PRIMARY KEY CLUSTERED (ColPk),
	CONSTRAINT [UK_Table_RenameColumnTransformationTest]      UNIQUE (ColUk),
	CONSTRAINT [FK_Table_RenameColumnTransformationTest]      FOREIGN KEY (ColFk) REFERENCES [Table_RenameColumnTransformationTest] (ColPk),
	CONSTRAINT [Check01_Table_RenameColumnTransformationTest] CHECK (ColWithCheck != 0),
	CONSTRAINT [Check02_Table_RenameColumnTransformationTest] CHECK (SYSUTCDATETIME() > '2012-11-25'),
	CONSTRAINT [Default_Table_RenameColumnTransformationTest] DEFAULT 0 FOR ColWithDefault
;

CREATE UNIQUE NONCLUSTERED INDEX [Index_Table_RenameColumnTransformationTest] ON [Table_RenameColumnTransformationTest] (ColWithIndex);
CREATE NONCLUSTERED INDEX [Index2_ColUk_ColWithIndex] ON [Table_RenameColumnTransformationTest] (ColUk, ColWithIndex);
CREATE NONCLUSTERED INDEX [Index3_ColUk] ON [Table_RenameColumnTransformationTest] (ColUk) INCLUDE(ColWithIndex);
CREATE NONCLUSTERED INDEX [Index4_Filtered_ColUk] ON [Table_RenameColumnTransformationTest] (ColUk) WHERE (ColWithIndex > 0);

CREATE NONCLUSTERED INDEX [IndexRename1_ColUk1]              ON [Table_RenameColumnTransformationTest] (ColUk1);
CREATE NONCLUSTERED INDEX [IndexRename2_ColUk1_ColUk]        ON [Table_RenameColumnTransformationTest] (ColUk1, ColUk);
CREATE NONCLUSTERED INDEX [IndexRename3_ColUk1_ColUk_ColUk2] ON [Table_RenameColumnTransformationTest] (ColUk1, ColUk, ColUk2);
CREATE NONCLUSTERED INDEX [IndexRename4_ColUk_ColUk1]        ON [Table_RenameColumnTransformationTest] (ColUk, ColUk1);

CREATE STATISTICS [Stat_Table_RenameColumnTransformationTest] ON [Table_RenameColumnTransformationTest] (ColWithStats);
CREATE STATISTICS [Stat_Filtered_Table_RenameColumnTransformationTest] ON [Table_RenameColumnTransformationTest] (ColUk) WHERE (ColWithStats > 0);

INSERT [Table_RenameColumnTransformationTest] VALUES
	(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, NULL),
	(2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 'B')
;

";

			TestConnection.ExecuteNonQuery(sqlText);

			TestConnection.ExecuteNonQuery("CREATE VIEW [View_Table_RenameColumnTransformationTest] WITH SCHEMABINDING AS SELECT ColWithProgrammabilityObjects FROM dbo.[Table_RenameColumnTransformationTest];");
			TestConnection.ExecuteNonQuery("CREATE VIEW [View2_Table_RenameColumnTransformationTest] WITH SCHEMABINDING AS SELECT ColWithProgrammabilityObjects FROM dbo.[View_Table_RenameColumnTransformationTest];");
			TestConnection.ExecuteNonQuery("CREATE FUNCTION [Function_Table_RenameColumnTransformationTest]() RETURNS TABLE WITH SCHEMABINDING AS RETURN SELECT ColWithProgrammabilityObjects FROM dbo.[Table_RenameColumnTransformationTest];");
			TestConnection.ExecuteNonQuery("CREATE FUNCTION [Function2_Table_RenameColumnTransformationTest]() RETURNS TABLE AS RETURN SELECT ColWithProgrammabilityObjects FROM dbo.[Table_RenameColumnTransformationTest];");
			TestConnection.ExecuteNonQuery("CREATE PROCEDURE [Proc_Table_RenameColumnTransformationTest] AS SELECT ColWithProgrammabilityObjects FROM dbo.[Table_RenameColumnTransformationTest];");
			TestConnection.ExecuteNonQuery("CREATE TRIGGER [Trigger_Table_RenameColumnTransformationTest] ON [Table_RenameColumnTransformationTest] FOR INSERT AS IF UPDATE(ColWithProgrammabilityObjects) RETURN;");

			PrepareTestDataForADAW();
		}

		void PrepareTestDataForADAW()
		{
			var sqlText = @"
INSERT INTO [StmModuleFilter]
           ([S9_PK]
           ,[S9_GC]
           ,[S9_ModuleID]
           ,[S9_FilterData]
           ,[S9_IsPublished]
           ,[S9_SaveColumnLayout]
           ,[S9_RelatedEntityID]
           ,[S9_IsSystem]
           ,[S9_ColumnLayoutData]
           ,[S9_FilterName]
           ,[S9_FilterType]
           ,[S9_ParentID]
           ,[S9_ParentTableCode]
           ,[S9_GridColourLayoutID]
           ,[S9_SaveGridColourLayout])
     VALUES
           (newid()
           ,null
           ,'GLOWDataImportV2_ITest'
           ,dbo.CLRCompressStringAsBytes('
<MappingTables>
	<MappingTable>
		<FieldMappings>
			<FieldMapping>
				<SourceColumnIndex>1</SourceColumnIndex>
				<TargetColumnName>ColWithProgrammabilityObjects</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>true</ShouldMatch>
					<ShouldCreate>false</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
			<FieldMapping>
				<SourceColumnIndex>2</SourceColumnIndex>
				<TargetColumnName>ColWithStats</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>false</ShouldMatch>
					<ShouldCreate>true</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
			<FieldMapping>
				<SourceColumnIndex>3</SourceColumnIndex>
				<TargetColumnName>ChildBOCollection.ColWithIndex</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>true</ShouldMatch>
					<ShouldCreate>false</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
			<FieldMapping>
				<SourceColumnIndex>4</SourceColumnIndex>
				<TargetColumnName>ChildBOCollection2.ColWithIndex</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>true</ShouldMatch>
					<ShouldCreate>false</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
		</FieldMappings>
	</MappingTable>
</MappingTables>')
           ,0
           ,0
           ,null
           ,0
           ,null
           ,'Test'
           ,''
           ,null
           ,''
           ,null
           ,0)
";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		void AssertADAW()
		{
			var sqlText = @"
Select dbo.CLRUncompressAsString(S9_FilterData)
from dbo.StmModuleFilter
WHERE S9_ModuleID = 'GLOWDataImportV2_ITest'
";

			using (var cmd = TestConnection.Command(sqlText))
			{
				var templatedata = (string)cmd.ExecuteScalar();
				AssertEquals(@"<MappingTables>
	<MappingTable>
		<FieldMappings>
			<FieldMapping>
				<SourceColumnIndex>1</SourceColumnIndex>
				<TargetColumnName>ColWithProgrammabilityObjectsNew</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>true</ShouldMatch>
					<ShouldCreate>false</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
			<FieldMapping>
				<SourceColumnIndex>3</SourceColumnIndex>
				<TargetColumnName>ChildBOCollection.ColWithIndexNew</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>true</ShouldMatch>
					<ShouldCreate>false</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
			<FieldMapping>
				<SourceColumnIndex>4</SourceColumnIndex>
				<TargetColumnName>ChildBOCollection2.ColWithIndex</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>true</ShouldMatch>
					<ShouldCreate>false</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
		</FieldMappings>
	</MappingTable>
</MappingTables>".Replace("\t", ""), templatedata.Replace(" ", ""));
			}
		}

		void AssertTransformationResults()
		{
			AssertColumns();
			AssertDependentObjects();
			AssertADAW();
		}

		void AssertColumns()
		{
			AssertEquals("ColPk exists?", false, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColPk"));
			AssertEquals("ColUk exists?", false, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColUk"));
			AssertEquals("ColFk exists?", false, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColFk"));
			AssertEquals("ColWithCheck exists?", false, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColWithCheck"));
			AssertEquals("ColWithDefault exists?", false, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColWithDefault"));
			AssertEquals("ColWithIndex exists?", false, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColWithIndex"));
			AssertEquals("ColWithStats exists?", false, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColWithStats"));
			AssertEquals("ColWithChangeTracking exists?", false, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColWithChangeTracking"));
			AssertEquals("ColWithProgrammabilityObjects exists?", false, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColWithProgrammabilityObjects"));

			AssertEquals("ColPkNew exists?", true, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColPkNew"));
			AssertEquals("ColUkNew exists?", true, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColUkNew"));
			AssertEquals("ColFkNew exists?", true, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColFkNew"));
			AssertEquals("ColWithCheckNew exists?", true, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColWithCheckNew"));
			AssertEquals("ColWithDefaultNew exists?", true, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColWithDefaultNew"));
			AssertEquals("ColWithIndexNew exists?", true, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColWithIndexNew"));
			AssertEquals("ColWithStatsNew exists?", true, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColWithStatsNew"));
			AssertEquals("ColWithProgrammabilityObjectsNew exists?", true, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColWithProgrammabilityObjectsNew"));

			AssertEquals("ColUk1 exists?", true, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColUk1"));
			AssertEquals("ColUk2 exists?", true, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColUk2"));

			AssertEquals("ColComputedSource exists?", false, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColComputedSource"));
			AssertEquals("ColComputedSourceNew exists?", true, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColComputedSourceNew"));

			// SPARSE column
			AssertEquals("ColSparse exists?", false, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColSparse"));
			AssertEquals("ColSparseNew exists?", true, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColSparseNew"));
			AssertEquals(
				"ColSparseNew column's info is expected",
				true,
				GetColumnInfo("ColSparseNew")
					.ToHashSet()
					.IsSupersetOf(new List<(string Name, object Value)>
					{
						("ColName", "ColSparseNew"),
						("ColType", "char"),
						("ColLength", 1),
						("ColNullOrNotNull", "NULL"),
						("ColSparse", true),
					}));
		}

		void AssertDependentObjects()
		{
			AssertObjectExists("PK_Table_RenameColumnTransformationTest", true);
			AssertObjectExists("UK_Table_RenameColumnTransformationTest", true);
			AssertObjectExists("FK_Table_RenameColumnTransformationTest", true);
			// Dependent check constraint removed
			AssertObjectExists("Check01_Table_RenameColumnTransformationTest", false);
			AssertObjectExists("Check02_Table_RenameColumnTransformationTest", true);
			AssertObjectExists(DbObjectCreator.GenerateDefaultColumnConstraintName("Table_RenameColumnTransformationTest", "ColWithDefaultNew"), true);
			// Indexes
			var expectedIndexes = new string[]
				{
					"PK_Table_RenameColumnTransformationTest",
					"UK_Table_RenameColumnTransformationTest",
					"Index_Table_RenameColumnTransformationTest",
					"Index2_ColUkNew_ColWithIndexNew",
					"Index3_ColUkNew",
					"IndexRename1_ColUk1",
					"IndexRename2_ColUk1_ColUkNew",
					"IndexRename3_ColUk1_ColUkNew_ColUk2",
					"IndexRename4_ColUkNew_ColUk1",
				};
			AssertIndexes("Table_RenameColumnTransformationTest", expectedIndexes);
			// Dependent stats removed
			AssertStatisticExists("Stat_Table_RenameColumnTransformationTest", true);
			AssertStatisticExists("Stat_Filtered_Table_RenameColumnTransformationTest", false);
			// Views, Functions, Procedures and Triggers must be removed as renamed column references are not apdated.
			AssertObjectExists("View_Table_RenameColumnTransformationTest", false);
			AssertObjectExists("View2_Table_RenameColumnTransformationTest", false);
			AssertObjectExists("Function_Table_RenameColumnTransformationTest", false);
			AssertObjectExists("Function2_Table_RenameColumnTransformationTest", true);
			AssertObjectExists("Proc_Table_RenameColumnTransformationTest", true);
			AssertObjectExists("Trigger_Table_RenameColumnTransformationTest", false);

			// Dependent computed columns
			AssertEquals("ColComputed exists?", false, DbObjectCreator.ColumnExists(TestConnection, "Table_RenameColumnTransformationTest", "ColComputed"));
		}

		void AssertObjectExists(string objectName, bool expectedExists)
		{
			AssertEquals(objectName + " exists?", expectedExists, TestConnection.Exists(string.Format("FROM sys.objects WHERE name = '{0}'", objectName)));
		}

		void AssertIndexes(string tableName, IEnumerable<string> expectedNames)
		{
			var actualNames = new List<string>();
			var sql = string.Format(@"SELECT name FROM sys.indexes WHERE object_id = OBJECT_ID(N'{0}', N'U') ORDER BY name;", tableName);
			using (var cmd = TestConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					actualNames.Add((string)reader["name"]);
				}
			}

			AssertContainsExactElementsInAnyOrder("Indexes", expectedNames, actualNames);
		}

		void AssertStatisticExists(string statName, bool expectedExists)
		{
			AssertEquals(statName + " exists?", expectedExists, TestConnection.Exists(string.Format("FROM sys.stats WHERE name = '{0}'", statName)));
		}

		class RenameColumnTransformationForTesting : RenameColumnTransformation
		{
			public override IEnumerable<IRenameColumnTransformationInfo> RenameColumnInfoList
			{
				get
				{
					yield return new RenameColumnTransformationInfo("Table_RenameColumnTransformationTest", "ColPk", "ColPkNew");
					yield return new RenameColumnTransformationInfo("Table_RenameColumnTransformationTest", "ColUk", "ColUkNew");
					yield return new RenameColumnTransformationInfo("Table_RenameColumnTransformationTest", "ColFk", "ColFkNew");
					yield return new RenameColumnTransformationInfo("Table_RenameColumnTransformationTest", "ColWithCheck", "ColWithCheckNew");
					yield return new RenameColumnTransformationInfo("Table_RenameColumnTransformationTest", "ColWithDefault", "ColWithDefaultNew");
					yield return new RenameColumnTransformationInfo("Table_RenameColumnTransformationTest", "ColWithIndex", "ColWithIndexNew");
					yield return new RenameColumnTransformationInfo("Table_RenameColumnTransformationTest", "ColWithStats", "ColWithStatsNew");
					yield return new RenameColumnTransformationInfo("Table_RenameColumnTransformationTest", "ColWithChangeTracking", "ColWithChangeTrackingNew");
					yield return new RenameColumnTransformationInfo("Table_RenameColumnTransformationTest", "ColWithProgrammabilityObjects", "ColWithProgrammabilityObjectsNew");
					yield return new RenameColumnTransformationInfo("Table_RenameColumnTransformationTest", "ColComputedSource", "ColComputedSourceNew");
					yield return new RenameColumnTransformationInfo("Table_RenameColumnTransformationTest", "ColSparse", "ColSparseNew");
				}
			}

			protected override IEnumerable<AdAWMappingUpdateInfo> ADAWMappingUpdateInfos => new List<AdAWMappingUpdateInfo>
			{
				new AdAWMappingUpdateInfo("ITest", "ColWithProgrammabilityObjects", "ColWithProgrammabilityObjectsNew", true, false,null),
				new AdAWMappingUpdateInfo("ITest", "ColWithStats", "ColWithProgrammabilityObjectsNew", true, false, "ColWithProgrammabilityObjects"),
				new AdAWMappingUpdateInfo("ITest", "ChildBOCollection.ColWithIndex", "ChildBOCollection.ColWithIndexNew", true, false, null),
			};
		}

		List<(string Name, object Value)> GetColumnInfo(string columnName)
		{
			var results = new List<(string Name, object Value)>();
			TestConnection.ExecuteReader(
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
	}
}
