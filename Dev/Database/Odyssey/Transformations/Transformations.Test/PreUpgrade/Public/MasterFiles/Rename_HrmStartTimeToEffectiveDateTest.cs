using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.PreUpgrade.Testing
{
	public abstract class Rename_HrmStartTimeToEffectiveDateTest : TransactionedTestCase
	{
		protected abstract string SchemaName { get; }
		protected abstract string TableName { get; }
		protected abstract string StartTimeColumnName { get; }
		protected abstract string EffectiveDateColumnName { get; }
		protected abstract string StartTimeColumnNameType { get; }
		protected string EffectiveDateColumnType = "DATETIMEOFFSET(0)";

		#region TestTransformation

		public void TestTransformation()
		{
			RunTransformation();
			AssertTransformationResults();

			RunTransformation();
			AssertTransformationResults();
		}

		void AssertTransformationResults()
		{
			AssertEquals($"Column [{TableName}] -> [{StartTimeColumnName}] should not exist.", false, DbObjectCreator.ColumnExists(TestConnection, TableName, StartTimeColumnName));

			AssertEquals($"Column [{TableName}] -> [{EffectiveDateColumnName}] should exist.", true, DbObjectCreator.ColumnExists(TestConnection, TableName, EffectiveDateColumnName));
		}

		#region TestDataInNewColumnIsPopulatedFromOldColumn

		protected abstract void RunTransformation();
		protected abstract void InsertOriginalData(Guid departmentPk, Guid branchPk);
		protected abstract void AssertDataPopulationResult();

		[UseSnapshotProtection]
		public void TestDataInNewColumnIsPopulatedFromOldColumn()
		{
			var departmentPk = Guid.NewGuid();
			var comanyPk = Guid.NewGuid();
			var branchPk = Guid.NewGuid();

			var sqlInsert = $@"
INSERT INTO dbo.GlbDepartment(GE_PK, GE_Code) VALUES('{departmentPk}', 'KST');
INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES('{comanyPk}', 'CT1', 'AU company', 'AU', 'AUD');
INSERT INTO dbo.GlbBranch(GB_PK, GB_GC) VALUES('{branchPk}', '{comanyPk}');
			";

			TestConnection.ExecuteNonQuery(sqlInsert);

			InsertOriginalData(departmentPk, branchPk);

			RunTransformation();

			AssertDataPopulationResult();
		}

		#endregion

		#endregion

		#region Implementations

		protected override void SetUp()
		{
			base.SetUp();
			disposableAdminConnection = ((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade();

			if (!DbObjectCreator.ColumnExists(Db.Connection, Db.Connection.CurrentDatabase, SchemaName, TableName, StartTimeColumnName))
			{
				new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, TableName, EffectiveDateColumnName).DropRelateObjects(Db.Connection);
				DbObjectCreator.RenameColumn(Db.Connection, Db.SqlDbOwnerSchema, TableName, EffectiveDateColumnName, StartTimeColumnName);
				TestConnection.ExecuteNonQuery(FormattableString.Invariant($"ALTER TABLE {SchemaName}.{TableName} ALTER COLUMN {StartTimeColumnName} {StartTimeColumnNameType}"));
			}
		}

		protected override sealed void TearDown()
		{
			disposableAdminConnection.Dispose();

			base.TearDown();
		}

		IDisposable disposableAdminConnection;

		#endregion
	}
}
