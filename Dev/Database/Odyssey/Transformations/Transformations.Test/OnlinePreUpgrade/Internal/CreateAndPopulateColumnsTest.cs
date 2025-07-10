using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformations.PreUpgrade;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing
{
	abstract class CreateAndPopulateColumnsTest : TestCase
	{
		protected readonly IEnumerable<string> PK = new string[] { "ZD1_PK uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED" };
		protected abstract IEnumerable<string> NewColumnList { get; }

		public delegate IColumnCreatorWithValue ColumnCreatorDelegateForTest(IUpgradeManager manager, ColumnChangeMetadata columnMetadata, string dbBeingUpgraded);
		protected abstract ColumnCreatorDelegateForTest GetColumnCreatorDelegateForTest();

		protected virtual void PrerequisiteSetup() { }

		[SnailTest]
		public abstract void TestCreateAndPopulateColumns();

		[UseSnapshotProtection]
		public void TestHighWatermarkIsRecorded()
		{
			// Arrange
			PrerequisiteSetup();

			// Act
			PreSynchroniser.AddAndPopulateAuditAndNaturalKeyColumns();

			// Assert
			AssertHighWatermark(ExtProperty.Table.Select(
				Db.Connection,
				TargetTableSchema.SqlSchemaName,
				TargetTable,
				HighWatermarks.PopulateAuditTimeAndUserHighWatermark));
		}

		protected virtual void AssertHighWatermark(string highWatermark)
		{
			AssertNullOrEmpty(highWatermark);
		}

		public int GetEmptyValueCount()
		{
			var whereClause = String.Join("\r\n\t", NewColumnList.Select(c => String.Format("OR {0} is NULL", c.Split().First())));

			var sql = String.Format(CultureInfo.InvariantCulture, @"
SELECT
	COUNT(*)
FROM
	[dbo].[{0}]
WHERE 1=2
	{1}
",
				TargetTable,
				whereClause
				);

			return (int)Db.Connection.ExecuteScalar(sql);
		}

		public bool NewColumnsExist()
		{
			var newColumns = NewColumnList.Select(c => String.Format("OR name = '{0}'", c.Split().First()));

			var sql = String.Format(CultureInfo.InvariantCulture, @"
SELECT
	COUNT(*)
FROM
	sys.columns
WHERE
	object_id = OBJECT_ID(N'[dbo].[{0}]', N'U')
	AND
	(1=2
		{1}
	)
",
				TargetTable,
				String.Join("\r\n\t\t", newColumns)
				);

			return newColumns.Count() == (int)Db.Connection.ExecuteScalar(sql);
		}

		public IEnumerable<string> GetColumnsDeclaration()
		{
			var result = new List<string>();
			var newColumns = NewColumnList.Select(c => $"OR col.name = '{c.Split().First()}'");

			Db.Connection.ExecuteReader($@"
SELECT
	col_definition = CONCAT(col.name
		, N' '
		, CASE
			WHEN typ.name = N'decimal' THEN CONCAT(typ.name, N'(', col.precision, N', ', col.scale, N')')
			WHEN typ.name in (N'varchar', N'nvarchar', N'varbinary') AND col.max_length = -1 THEN CONCAT(typ.name, N'(max)')
			WHEN typ.name in (N'char', N'varchar', N'varbinary') THEN CONCAT(typ.name, N'(', col.max_length, N')')
			WHEN typ.name in (N'nchar', N'nvarchar') THEN CONCAT(typ.name, N'(', col.max_length / 2, N')')
			ELSE typ.name
		END
		, IIF(col.is_nullable = 1, N' NULL', N' NOT NULL')
		, IIF(def.definition is NULL, N'', CONCAT(N' DEFAULT ', def.definition))
		)
FROM
	sys.columns                       AS col
	JOIN sys.types                    AS typ ON typ.user_type_id = col.user_type_id
	LEFT JOIN sys.default_constraints AS def ON def.object_id = col.default_object_id
WHERE 1=1
	AND col.object_id = OBJECT_ID(N'[dbo].[{TargetTable}]', N'U')
	AND
	(1=2
		{string.Join("\r\n\t\t", newColumns)}
	)
"
				, record =>
				{
					result.Add((string)record["col_definition"]);
				});

			return result;
		}

		protected string templateDb;
		public string TargetTable { get; private set; }

		public ITableSchema TargetTableSchema = DummyDependentBizoSchema.Instance;
		public TablePreSynchroniser PreSynchroniser { get; private set; }

		public bool TriggerExists(string triggerName)
		{
			var sql = String.Format("SELECT CONVERT(bit, CASE WHEN OBJECT_ID(N'{0}', N'TR') is NOT NULL THEN 1 ELSE 0 END);", triggerName);
			return (bool)Db.Connection.ExecuteScalar(sql);
		}

		#region Implementation

		IAuxiliaryDbCreator templateDbCreator;
		IUpgradeManager manager;

		protected override void SetUp()
		{
			base.SetUp();

			templateDb = String.Format("_testTemplateDb_{0}", Guid.NewGuid());
			TargetTable = DummyDependentBizoSchema.Constants.TableName;

			manager = new DummyUpgradeManager();

			var columnCreatorFactoryForTest = new ColumnCreatorWithValueFactoryForTest(manager, GetColumnCreatorDelegateForTest());
			PreSynchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, templateDb, columnCreatorFactoryForTest);

			TablePreSynchroniser.CreatePreAddDb_ForTest();
			CreateTemplateDb();
		}

		protected override void TearDown()
		{
			DropTemplateDb();
			TablePreSynchroniser.DropPreAddDb_ForTest();

			base.TearDown();
		}

		void CreateTemplateDb()
		{
			templateDbCreator = new TablePreSynchroniserTestTemplateDbCreator(templateDb, TargetTable, PK.Union(NewColumnList));
			templateDbCreator.CreateDropExisting();
		}

		void DropTemplateDb()
		{
			templateDbCreator.Drop();
		}

		class ColumnCreatorWithValueFactoryForTest : ColumnCreatorWithValueFactory
		{
			public ColumnCreatorWithValueFactoryForTest(IUpgradeManager manager, ColumnCreatorDelegateForTest columnCreatorForTest)
				: base(manager)
			{
				this.columnCreatorForTest = columnCreatorForTest;
			}

			readonly ColumnCreatorDelegateForTest columnCreatorForTest;

			public override IColumnCreatorWithValue GetColumnCreator(IUpgradeManager manager, ColumnChangeMetadata columnMetadata, string dbBeingUpgraded)
			{
				return columnCreatorForTest(manager, columnMetadata, dbBeingUpgraded);
			}
		}

		#endregion //Implementation
	}
}
