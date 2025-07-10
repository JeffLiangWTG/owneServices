using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing
{
	class OneOffAddAndPopulateTransformationShouldRunTest : TestCase
	{
		public void TestMapper()
		{
			var postOffTransformation = Mapper.GetAllMappings().First();
			transformation.Transformation = postOffTransformation.TransformationType;

			Test("It should not run when transformationVersion is equals to TransformationVersionBeforeUpgrade",
				false, postOffTransformation.MappedVersion);
			Test("It should not run when transformationVersion is higher than TransformationVersionBeforeUpgrade",
				false, new VersionLabel(postOffTransformation.MappedVersion.Major + 1, postOffTransformation.MappedVersion.Minor));
			Test("It should run when transformationVersion is lower than TransformationVersionBeforeUpgrade",
				true, new VersionLabel(postOffTransformation.MappedVersion.Major - 1, postOffTransformation.MappedVersion.Minor));

			void Test(string message, bool expectedResult, VersionLabel transformationVersionBeforeUpgrade)
			{
				// Arrange
				manager.TransformationVersionBeforeUpgradeOverridenValue = transformationVersionBeforeUpgrade;

				// Act
				var result = transformation.ShouldRun();

				// Assert
				AssertEquals(message, expectedResult, result);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			manager = new UpgradeManagerForTestWithOutputBuffer();
			transformation = new OneOffAddAndPopulateTransformationForMapperTest(manager, string.Empty, string.Empty);
		}

		UpgradeManagerForTestWithOutputBuffer manager;
		OneOffAddAndPopulateTransformationForMapperTest transformation;

		#endregion // Implementation

		#region Helper classes

		class OneOffAddAndPopulateTransformationForMapperTest : OneOffAddAndPopulateTransformation
		{
			public OneOffAddAndPopulateTransformationForMapperTest(IUpgradeManager manager, string dbBeingUpgraded, string templateDb) : base(manager, dbBeingUpgraded, templateDb)
			{
			}

			protected override string UserDescription { get; }
			protected override string StatusName { get; }
			protected override string WatermarkName { get; }

			protected override Type TwinOfflineTransformation => Transformation;
			public Type Transformation { get; set; }

			protected override void RunTransformation()
			{
				throw new NotImplementedException();
			}
		}

		#endregion // Helper classes
	}

	[UseSnapshotProtection]
	class OneOffAddAndPopulateTransformationTest : TestCase
	{
		public void TestAddsNewColumnIfNoData()
		{
			// Arrange
			CreateTestStructure();

			// Act
			transformation.Run();
			transformation.Run();

			// Assert
			var sql = $"SELECT TST_Value, TST_TempValue FROM {TargetTable}";
			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(sql));
		}

		public void TestAddsNewColumnWithData()
		{
			// Arrange
			CreateTestStructure();
			TargetTableCreateValues(1, "");
			TargetTableCreateValues(2, "aa");
			TargetTableCreateValues(3, "bbbccc");

			// Act
			transformation.Run();
			transformation.Run();

			// Assert
			AssertPopulatedValues(1, "_populated", 1);
			AssertPopulatedValues(2, "aa_populated", 2);
			AssertPopulatedValues(3, "bbbccc_populated", 3);
		}

		public void TestUsesProvidedFieldAsSourceSort()
		{
			// Arrange
			const string orderBy = "TST_SortingField";
			CreateTestStructure($"{orderBy} varchar(10)  NOT NULL");
			var populatingColumns = transformation.PopulatingColumns;
			transformation.PopulatingColumns = (populatingColumns.SqlSchemaName, populatingColumns.TableName, orderBy, populatingColumns.PopulatingColumns);
			transformation.StoreQueryToManager = true;

			// Act
			transformation.Run();
			transformation.Run();

			// Assert
			AssertEndsWith("Looking for order expression",
				"ORDER BY\r\n\tTST_SortingField\r\nOPTION (RECOMPILE);\r\n",
				manager.OutputTextCollection.Cast<string>().Single(s => s.StartsWith("-- Populating pre-add table")));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			TablePreSynchroniser.CreatePreAddDb_ForTest();
			CreateTemplateDatabase();

			manager = new UpgradeManagerForTestWithOutputBuffer();
			transformation = new OneOffAddAndPopulateTransformationForTest(manager, Db.DatabaseName, TemplateDb);
		}

		protected override void TearDown()
		{
			DropTemplateDatabase();
			TablePreSynchroniser.DropPreAddDb_ForTest();

			base.TearDown();
		}

		static void AssertPopulatedValues(int id, string expectedValue, int expectedTempValue)
		{
			var sql = $@"
SELECT
	CONCAT('TST_Value:', TST_Value, ';TST_TempValue:', TST_TempValue)
FROM
	dbo.[{TargetTable}]
WHERE
	TST_PK = {id};
";

			var expected = $"TST_Value:{expectedValue};TST_TempValue:{expectedTempValue}";

			using (var cmd = Db.Connection.Command(sql))
			{
				AssertEquals(expected, (string)cmd.ExecuteScalar());
			}
		}

		static void CreateTestStructure(params string[] additionalColumns)
		{
			var sql = $@"
if (OBJECT_ID(N'[dbo].[{TargetTable}]', N'U') is NOT NULL) DROP TABLE [dbo].[{TargetTable}];
CREATE TABLE [dbo].[{TargetTable}]
(
	TST_PK       int          NOT NULL PRIMARY KEY,
	TST_OldValue varchar(100) NOT NULL DEFAULT ''
	{(additionalColumns.Length == 0 ? string.Empty : $", {string.Join(",\r\n", additionalColumns)}")}
);
";

			Db.Connection.ExecuteNonQuery(sql);
		}

		static void TargetTableCreateValues(int id, string value)
		{
			var sql = $"INSERT dbo.[{TargetTable}] (TST_PK, TST_OldValue) VALUES ({id}, '{value}');";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		void CreateTemplateDatabase()
		{
			templateDbCreator = new TablePreSynchroniserTestTemplateDbCreator(TemplateDb, TargetTable, new[] { "TST_PK int NOT NULL PRIMARY KEY", "TST_OldValue varchar(100) NOT NULL DEFAULT ''", "TST_Value varchar(100) NOT NULL DEFAULT ''" });
			templateDbCreator.CreateDropExisting();
		}

		void DropTemplateDatabase()
		{
			templateDbCreator.Drop();
		}

		const string TemplateDb = "_testPreSynchroniser_TemplateDb";
		const string TargetTable = "_testPreSynchroniser_TargetTable";
		UpgradeManagerForTestWithOutputBuffer manager;
		IAuxiliaryDbCreator templateDbCreator;
		OneOffAddAndPopulateTransformationForTest transformation;

		#endregion // Implementation

		#region Helper classes

		class OneOffAddAndPopulateTransformationForTest : OneOffAddAndPopulateTransformation
		{
			public OneOffAddAndPopulateTransformationForTest(IUpgradeManager manager, string dbBeingUpgraded, string templateDb) : base(manager, dbBeingUpgraded, templateDb)
			{
				PopulatingColumns = (
					"dbo",
					TargetTable,
					null,
					new[]
					{
						new PopulatedColumn
						{
							ColumnName = "TST_Value",
							IsSparse = false,
							PopulateExpression = "CONCAT(TST_OldValue, '_populated')",
							PopulatePreAddWhereClause = "OR 1 = 1"
						},
						new PopulatedColumn
						{
							ColumnName = "TST_TempValue",
							ColumnType = "int",
							IsSparse = false,
							ColumnDefaultConstraint = "DEFAULT 0",
							PopulateExpression = "TST_PK"
						}
					});
			}

			protected override string UserDescription { get; } = nameof(UserDescription);
			protected override string StatusName { get; } = nameof(StatusName);
			protected override string WatermarkName { get; } = nameof(WatermarkName);

			public (string SqlSchemaName, string TableName, string TableOrderBy, IEnumerable<PopulatedColumn> PopulatingColumns) PopulatingColumns { get; set; }

			protected override void RunTransformation()
			{
				DoPopulate(PopulatingColumns);
			}

			public override bool ShouldRun()
			{
				return true;
			}
		}

		#endregion // Helper classes
	}
}
