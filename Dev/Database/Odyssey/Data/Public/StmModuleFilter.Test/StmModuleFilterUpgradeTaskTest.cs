using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	public class StmModuleFilterUpgradeTaskTest : TransactionedTestCase
	{
		public void TestInsertingFilterLayouts()
		{
			var companyPk = DataHelpers.GetFirstKey(GlbCompanySchema.Instance);
			string filterData = "";

			string insertSql = string.Format(
				@"
					insert dbo.StmModuleFilter
					(
						S9_PK,
						S9_GC,
						S9_ModuleID,
						S9_IsPublished,
						S9_FilterName,
						S9_FilterData,
						S9_SaveColumnLayout,
						S9_RelatedEntityID
					)
					VALUES
					(
						'3CE880E6-74D2-41a1-990D-4C2994AA6A64',
						'{0}',
						'moduleID',
						1,
						'filterLayoutName',
						cast('{1}' as varbinary(max)),
						1,
						null
					)",
						companyPk, filterData);

			Db.Connection.ExecuteNonQuery(insertSql);

			insertSql = @"
				INSERT dbo.StmModuleFilterUserData (S0_PK, S0_RelatedEntityTableCode, S0_RelatedEntityID, S0_S9) VALUES ('86FA7854-ADC0-44b9-8B9B-F53AC1073A16', 'GS', '70EFA270-3F0F-479C-9AED-0009455622E2', '3CE880E6-74D2-41a1-990D-4C2994AA6A64')
				";

			Db.Connection.ExecuteNonQuery(insertSql);

			StmModuleFilterUpgradeTask task = new StmModuleFilterUpgradeTask(new StmModuleFilterDataFile());
			task.Run();

			StmModuleFilterDataFile tempFile = new StmModuleFilterDataFile();
			var data = tempFile.LoadDataFromDatabase();

			AssertEquals("Table Count", 2, data.Tables.Count);
		}
	}
}
