using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Moq;

namespace Enterprise.DbUpgrader.Transformations.Testing
{
	public abstract class HrmEffectiveDateToDateTimeOffsetTest : DataCopyTestCase
	{
		protected abstract ITableSchema TableSchema { get; }

		protected string SchemaName => TableSchema.SqlSchemaName;

		protected string TableName => TableSchema.TableName;

		protected string EffectiveDateColumnName => GetColumnName("EffectiveDate");

		protected abstract string EffectiveDateColumnType { get; }

		protected abstract string EffectiveDateColumnOriginalType { get; }

		protected string StaffFKColumnName => GetColumnName("GS_Staff");

		protected TimeSpan LocalOffset = DateTimeOffset.Now.Offset;

		string ColumnPrefix => TableSchema.PK.ColumnPrefix;

		protected string GetColumnName(string columnSuffix) => $"{ColumnPrefix}_{columnSuffix}";

		public abstract string TriggerName { get; }

		protected abstract SchemaColumn EffectiveDateColumn { get; }
		protected abstract string FKConstraintName { get; }

		string AutoEffectiveEndDateColumnName => GetColumnName("AutoEffectiveEndDate");

		[UseSnapshotProtection]
		public void TestIsRequiredWhenSourceColumnExistsAndIsNotTransformedColumn()
		{
			RecreateEffectiveDateColumn();

			var transform = GetNewIsRequiredTestTransformationInstance();
			Assert("Transform.IsRequired should be true if source column exists and is not a transformed column", transform.IsRequired);
		}

		[UseSnapshotProtection]
		public void TestIsNotRequiredWhenSourceColumnExistsButIsTransformedColumn()
		{
			RecreateEffectiveDateColumn();
			TestConnection.ExecuteNonQuery(FormattableString.Invariant($"ALTER TABLE {SchemaName}.{TableName}  ALTER COLUMN {EffectiveDateColumnName} {EffectiveDateColumnType}"));

			var transform = GetNewIsRequiredTestTransformationInstance();
			Assert("Transform.IsRequired should be false if source column exists and is transformed", !transform.IsRequired);
		}

		[UseSnapshotProtection]
		public void TestIsNotRequiredWhenSourceColumnDoesNotExist()
		{
			if (DbObjectCreator.ColumnExists(TestConnection, TestConnection.CurrentDatabase, SchemaName, TableName, EffectiveDateColumnName))
			{
				new DbColumnDependencyRemover(SchemaName, TableName, EffectiveDateColumnName).DropRelateObjects(TestConnection);
				TestConnection.ExecuteNonQuery(FormattableString.Invariant($"ALTER TABLE {SchemaName}.{TableName} DROP COLUMN {EffectiveDateColumnName}"));
			}

			var transform = GetNewIsRequiredTestTransformationInstance();
			Assert("Transform.IsRequired should be false if source column does not exist", !transform.IsRequired);
		}

		[UseSnapshotProtection]
		public void TestRunAndAssertResults_DisabledTrigger()
		{
			CreateOldTablesAndOrColumnsIfNotExist();

			PrepareOriginalData();
			TransformationToTest.Run();

			DropCreatedOldTablesAndColumns();

			TestConnection.ExecuteNonQuery($"DISABLE TRIGGER {TriggerName} ON [{TableSchema.SqlSchemaName}].[{TableSchema.TableName}]");

			TransformationToTest.Run();

			AssertDataCopyResults();
		}

		[UseSnapshotProtection]
		public void TestPopulateAutoEffectiveEndDate()
		{
			TestConnection.ExecuteNonQuery($"DISABLE TRIGGER {TriggerName} ON [{TableSchema.SqlSchemaName}].[{TableSchema.TableName}]");

			PrepareOriginalData();

			GetAutoTriggerTransformationForTest().PopulateAutoEffectiveEndDate_Exposed();

			AssertPopulateAutoEffectiveEndDateResults();
		}

		[UseSnapshotProtection]
		public void TestConstraintsAndTriggerRestored()
		{
			var convert = new Mock<HrmEffectiveDateTypeChangeTransformationWithAutoTriggerForTest>();
			convert.SetupProperty(c => c.EffectiveDateColumn_ForTest, EffectiveDateColumn);
			convert.SetupProperty(c => c.FKConstraintName_ForTest, FKConstraintName);
			convert.SetupProperty(c => c.TriggerName_ForTest, TriggerName);

			var isKeyConstraintExistAndEnabled_BeforeRun = TestConnection.ExecuteScalar<bool>(SQLIsFKConstraintEnabled);
			var isTriggerExistAndEnabled_BeforeRun = TestConnection.ExecuteScalar<bool>(SQLIsTriggerEnabled);

			AssertExceptionThrown<Exception>(() => { convert.Object.PasteToTarget_Exposed(); });

			var isKeyConstraintExistAndEnabled_AfterRun = TestConnection.ExecuteScalar<bool>(SQLIsFKConstraintEnabled);
			var isTriggerExistAndEnabled_AfterRun = TestConnection.ExecuteScalar<bool>(SQLIsTriggerEnabled);

			Assert("The self foreignkey constraint is enabled before transform.", isKeyConstraintExistAndEnabled_BeforeRun);
			Assert("The auto end date trigger is enabled before transform.", isTriggerExistAndEnabled_BeforeRun);

			Assert("The self foreignkey constraint is enabled after transform.", isKeyConstraintExistAndEnabled_AfterRun);
			Assert("The auto end date trigger is enabled after transform.", isTriggerExistAndEnabled_AfterRun);
		}

		string SQLIsFKConstraintEnabled => GetSQLSysObjectIsEnabled("sys.foreign_keys", FKConstraintName);

		string SQLIsTriggerEnabled => GetSQLSysObjectIsEnabled("sys.triggers", TriggerName);

		string GetSQLSysObjectIsEnabled(string systable, string name) => $@"
SELECT ObjectExists = CONVERT(bit,
	CASE
		WHEN EXISTS (SELECT NULL FROM {systable} WHERE name = '{name}' AND is_disabled = '0') THEN 1
		ELSE 0
	END
	)
";

		protected abstract void AssertPopulateAutoEffectiveEndDateResults();

		protected abstract void RestoreConstraints();

		protected void RestoreTrigger()
		{
			TestConnection.ExecuteNonQuery(triggerDefination);
		}

		protected void AssertStaffEffectiveDates(Guid staffPk, List<(DateTimeOffset EffectiveDate, DateTimeOffset? EffectiveEndDate)> expectedResults)
		{
			var actualResults = new List<(DateTimeOffset EffectiveDate, DateTimeOffset? EffectiveEndDate)>();

			TestConnection.ExecuteReader($"SELECT [{EffectiveDateColumnName}], [{AutoEffectiveEndDateColumnName}] FROM [{SchemaName}].[{TableName}] WHERE [{StaffFKColumnName}] = '{staffPk}'", reader =>
			{
				var effectiveDate = (DateTimeOffset)reader.GetValue(0);
				var effectiveEndDate = reader.GetValue(1);

				actualResults.Add((effectiveDate, effectiveEndDate == DBNull.Value ? null : (DateTimeOffset?)effectiveEndDate));
			});

			AssertContainsExactElementsInAnyOrder(expectedResults, actualResults);
		}

		protected void AssertAutoStaffEffectiveEndDates(Guid staffPk, DateTimeOffset?[] expectedResults)
		{
			var actualResults = new List<DateTimeOffset?>();

			TestConnection.ExecuteReader($"SELECT [{AutoEffectiveEndDateColumnName}] FROM [{SchemaName}].[{TableName}] WHERE [{StaffFKColumnName}] = '{staffPk}' ORDER BY [{EffectiveDateColumnName}]", reader =>
			{
				var effectiveEndDate = reader.GetValue(0);

				actualResults.Add(effectiveEndDate == DBNull.Value ? null : (DateTimeOffset?)effectiveEndDate);
			});

			AssertArrayEqualsByElements(expectedResults, actualResults.ToArray());
		}

		void RecreateEffectiveDateColumn()
		{
			RecreateColumn(EffectiveDateColumnName, EffectiveDateColumnOriginalType);
			RecreateColumn(AutoEffectiveEndDateColumnName, "[datetimeoffset](0)");
		}

		protected void RecreateColumn(string columnName, string type)
		{
			if (DbObjectCreator.ColumnExists(TestConnection, TestConnection.CurrentDatabase, SchemaName, TableName, columnName))
			{
				new DbColumnDependencyRemover(SchemaName, TableName, columnName).DropRelateObjects(TestConnection);
			}

			TestConnection.ExecuteNonQuery(FormattableString.Invariant($"ALTER TABLE {SchemaName}.{TableName} ALTER COLUMN {columnName} {type}"));
		}

		protected abstract SchemaChangeDataTransformation GetNewIsRequiredTestTransformationInstance();

		protected override sealed void PrepareOriginalData()
		{
			PrepareSchema();

			var departmentPk = Guid.NewGuid();
			var comanyPk = Guid.NewGuid();
			var branchPk = Guid.NewGuid();

			var sql = $@"
INSERT INTO dbo.GlbDepartment (GE_PK) VALUES ('{departmentPk}');

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES ('{comanyPk}', 'CT1', 'AU company', 'AU', 'AUD');
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES ('{branchPk}', '{comanyPk}');
";
			TestConnection.ExecuteNonQuery(sql);

			InsertOriginalData(departmentPk, branchPk);
		}

		protected virtual void PrepareSchema()
			=> RecreateEffectiveDateColumn();

		protected abstract void InsertOriginalData(Guid departmentPk, Guid branchPk);

		protected override void RevertDatabaseColumnTypeChanges()
		{
			RecreateColumn(EffectiveDateColumnName, EffectiveDateColumnType);

			RestoreConstraints();

			RestoreTrigger();
		}

		protected override sealed List<string> IAcknowledgeTheseSourceColumnsHaveNoReplacementValue
		{
			get
			{
				var list = base.IAcknowledgeTheseSourceColumnsHaveNoReplacementValue;
				list.Add($"{TableName}.{StaffFKColumnName}");
				list.Add($"{TableName}.{EffectiveDateColumnName}");

				return list;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			disposableAdminConnection = ((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade();

			triggerDefination = TestConnection.ExecuteScalar<string>($@"
SELECT 
    definition   
FROM 
    sys.sql_modules  
WHERE 
    object_id = OBJECT_ID('[{TableSchema.SqlSchemaName}].[{TriggerName}]'); ");
		}

		protected override sealed void TearDown()
		{
			disposableAdminConnection.Dispose();

			base.TearDown();
		}

		IDisposable disposableAdminConnection;

		string triggerDefination;

		protected abstract IAutoTriggerTransformationForTest GetAutoTriggerTransformationForTest();

		protected interface IAutoTriggerTransformationForTest
		{
			void PopulateAutoEffectiveEndDate_Exposed();
		}

		public abstract class HrmEffectiveDateTypeChangeTransformationWithAutoTriggerForTest : HrmEffectiveDateToDateTimeOffsetChangeTransformation
		{
			public HrmEffectiveDateTypeChangeTransformationWithAutoTriggerForTest()
				: base()
			{
			}

			public void PasteToTarget_Exposed() => PasteToTarget();

			public virtual SchemaColumn EffectiveDateColumn_ForTest { get; set; }
			public virtual string FKConstraintName_ForTest { get; set; }
			public virtual string TriggerName_ForTest { get; set; }

			protected sealed override SchemaColumn EffectiveDateColumn => EffectiveDateColumn_ForTest;
			protected sealed override string FKConstraintName => FKConstraintName_ForTest;
			protected sealed override string TriggerName => TriggerName_ForTest;
		}
	}
}
