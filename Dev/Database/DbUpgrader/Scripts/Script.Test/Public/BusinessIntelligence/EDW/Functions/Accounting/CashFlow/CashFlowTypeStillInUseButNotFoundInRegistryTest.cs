using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.CashFlow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting.CashFlow.Testing
{
	[TestedType(typeof(CashFlowTypeStillInUseButNotFoundInRegistry))]
	class CashFlowTypeStillInUseButNotFoundInRegistryTest : BiCreateScriptTest
	{
		protected override string ScriptDbName => Db.EdwDatabaseName;

		public void TestCashFlowTypeStillInUseButNotFoundInRegistry()
		{
			var companyPKStr = "";
			var branchPKStr = "";
			PrepareData();

			var result = Execute(companyPKStr, 201512, branchPKStr);
			AssertEquals(1, result.Select("CashFlowTypeCode = '111'").Length);
			AssertEquals(1, result.Select("CashFlowTypeCode = 'tst'").Length);
			AssertEquals(1, result.Select("CashFlowTypeCode = 'OEQ'").Length);

			void PrepareData()
			{
				var helper = new PrepareDataHelper(TestConnection, ScriptDbName);
				var companyPK = helper.InsertCompany(1, "ABC", "ABC Compay", "CNY", "CN", true, true);
				companyPKStr = companyPK.ToString();
				var branch2PK = helper.InsertBranch(1, "BR2", 1, "Branch 2");
				branchPKStr = branch2PK.ToString();
				helper.CreateOrganisation(1, "1stCarDiv", "1st Carrier Division");
				helper.InsertGLAccount(1, "4444.22.22", accountTypeCode: "P&L", cashFlowType: "111");
				var orgCreditorGroupPK = Guid.NewGuid();
				var orgCreditorGroupPKstr = orgCreditorGroupPK.ToString();
				var ahPK = Guid.NewGuid().ToString();

				helper.InsertAccPeriodScriptWithCompanyId(2015, 12, companyPK);
				helper.InsertAccPeriodScriptWithCompanyId(2016, 01, companyPK);
				helper.InsertAccPeriodScriptWithCompanyId(2016, 02, companyPK);

				TestConnection.ExecuteNonQuery($@"INSERT {ScriptDbName}.Organization.BAS__OrgCreditorGroup(OrgCreditorGroupID, OrgCreditorGroupKey) VALUES ('{orgCreditorGroupPKstr}', 1)");

				TestConnection.ExecuteNonQuery($@"INSERT INTO {ScriptDbName}.Organization.BAS__OrganizationCompanyData(OrganizationCompanyDataKey, OrganizationCompanyDataID, OrganizationHeaderKey, CompanyKey, APCreditorGroupKey) VALUES (1, newid(), 1, 1, 1)");

				helper.InsertAccTransactionHeader(1, "7BE5FA30-4618-497C-8E83-182104925E4F", "AR", "REC", "00001001", new DateTime(2015, 12, 5, 15, 44, 0), new DateTime(2015, 12, 25, 15, 44, 0), 'Y', 100.0000m, 100.0000m, "CNY", 1, new DateTime(2015, 12, 5, 15, 44, 0), 1, 1, 1, 1, "tst");

				helper.InsertAccTransactionHeader(2, "344319BA-BBFE-4E80-8B1F-0E587B3240C6", "AP", "PAY", "123456", new DateTime(2015, 1, 5, 15, 44, 0), new DateTime(2015, 1, 5, 15, 44, 0), 'Y', -30.0000m, -30.0000m, "CNY", 1, new DateTime(2015, 12, 5, 15, 44, 0), 1, 1, 1, 1, "");

				helper.InsertAccTransactionHeader(3, ahPK, "CB", "PAY", "123456", new DateTime(2015, 1, 5, 15, 44, 0), new DateTime(2015, 1, 5, 15, 44, 0), 'Y', -30.0000m, -30.0000m, "CNY", 1, new DateTime(2015, 1, 5, 15, 44, 0), 1, 1, 1, 1, "");

				TestConnection.ExecuteNonQuery($@"INSERT INTO {ScriptDbName}.Finance.BAS__AccGLTransactionLine (AccGLTransactionLineKey, AccGLTransactionLineID, GLTransactionHeaderKey, GLAccountKey, CompanyKey, BranchKey, DepartmentKey, LineType, PostDateTime) VALUES(1, NEWID(), 3, 1, 1, 1, 1, 'DRC', CAST('2015-12-2' AS smalldatetime));");

				var insertRegistryValues = $@"
DECLARE @registryRawValue nvarchar(MAX);
SET @registryRawValue = '<ArrayOfCashFlowCategoryBasedOnCreditorGroup xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><CashFlowCategoryBasedOnCreditorGroup><OrgGroupPK>{orgCreditorGroupPK}</OrgGroupPK><CashFlowCategory>O04</CashFlowCategory></CashFlowCategoryBasedOnCreditorGroup><CashFlowCategoryBasedOnCreditorGroup><OrgGroupPK>{orgCreditorGroupPK}</OrgGroupPK><CashFlowCategory>OEQ</CashFlowCategory></CashFlowCategoryBasedOnCreditorGroup></ArrayOfCashFlowCategoryBasedOnCreditorGroup>'
INSERT INTO {ScriptDbName}.Finance.BAS__CashFlowCategoryBasedOnGroup 
    (CashFlowCategoryBasedOnGroupKey, CashFlowCategoryBasedOnGroupID, Name, Value) 
VALUES 
    (1, NEWID(), 'CashFlowCategoryBasedOnCreditorGroup', CONVERT(varbinary(MAX), @registryRawValue));
";
				TestConnection.ExecuteNonQuery(insertRegistryValues);
				TestConnection.ExecuteNonQuery($@"INSERT INTO {ScriptDbName}.Finance.GRP__CashFlowCategoryBasedOnCreditorGroup(CashFlowCategory, CreditorGroupPK) VALUES ('OEQ', '{orgCreditorGroupPKstr}');");
				TestConnection.ExecuteNonQuery($@"INSERT INTO {ScriptDbName}.Finance.BAS__StmDataDate(StmDataDateKey, StmDataDateID, Name, CompanyKey, Value) VALUES(1, NEWID(), 'JournalEntriesLastProcessedDate', 1, CONVERT(datetime, '2010-01-01 00:00:00.000'))");
			}
		}

		DataTable Execute(string companyPK, int period, string branchID)
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, string.Format($"SELECT * FROM [{ScriptDbName}].[dbo].CashFlowTypeStillInUseButNotFoundInRegistry({period}, '{companyPK}', '{branchID}')"));
		}
	}
}
