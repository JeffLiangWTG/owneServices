using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_ARProfileCFXTest : ScriptTest
	{
		public void TestARProfileCFX()
		{
			var org1 = TestObjectCreator.ABIGAS;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORG2";
			var debtorGroupPK = TestObjectCreator.CreateDebtorGroup().PK;

			org1.OH_IsActive = true;
			org1.OH_IsTempAccount = false;
			org1.CompanyData.OB_OJ_ARDebtorGroup = debtorGroupPK;
			org1.CompanyData.OB_ARCategory = "STD";
			org1.CompanyData.OB_ARConsolidatedAccountingCategory = "UNR";
			org1.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "IMP", "SEA", 5.76m, 5.00m);
			org1.CompanyData.AccCFXConfigurations.SetUplifts("SHP", "DOM", "COU", 6.81m, 15.00m);

			org2.OH_IsActive = true;
			org2.OH_IsTempAccount = false;
			org2.OH_IsDebtor = true;
			org2.CompanyData.OB_OJ_ARDebtorGroup = debtorGroupPK;
			org2.CompanyData.OB_ARCategory = "STD";
			org2.CompanyData.OB_ARConsolidatedAccountingCategory = "UNR";
			org2.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "EXP", "AIR", 15.7m, 20.01m);
			org2.CompanyData.AccCFXConfigurations.SetUplifts("SHP", "IMP", "AIR", 25.0m, 7.00m);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				org1.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "EXP", "AIR", 15.7m, 20.01m);
				org1.CompanyData.AccCFXConfigurations.SetUplifts("SHP", "IMP", "AIR", 25.0m, 7.00m);

				org2.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "IMP", "SEA", 5.76m, 5.00m);
				org2.CompanyData.AccCFXConfigurations.SetUplifts("SHP", "DOM", "COU", 6.81m, 15.00m);

				Factory.Save();
			}

			DataTable resultOrgs = RunScript(debtorGroupPK);
			AssertEquals("Result Rows", 4, resultOrgs.Rows.Count);

			var headers = new[] { "AccountCode", "AccountName", "AccountPK", "IsTemporary", "AccountsRelationship", "ConsolidationCategory",
									"JobType", "ServiceDirection", "TransportMode", "CFXPercentage", "CFXMinimum"
						};

			var org1Rows = resultOrgs.Select("AccountCode = 'ABIGAS      '");
			AssertEquals("exportedRows.Count", 2, org1Rows.Length);
			AssertDataRow(org1Rows[0], headers, new object[] {  "ABIGAS      ", TestObjectCreator.ABIGAS.OH_FullName, TestObjectCreator.ABIGAS.PK, "N", "STD", "UNR",
											"ALL", "IMP", "SEA", 5.76m, 5.00m });

			AssertDataRow(org1Rows[1], headers, new object[] {  "ABIGAS      ", TestObjectCreator.ABIGAS.OH_FullName, TestObjectCreator.ABIGAS.PK, "N", "STD", "UNR",
											"SHP", "DOM", "COU", 6.81m, 15.00m });

			var org2Rows = resultOrgs.Select("AccountCode = 'ORG2      '");
			AssertEquals("exportedRows.Count", 2, org2Rows.Length);

			AssertDataRow(org2Rows[0], headers, new object[] {  "ORG2", string.Empty, org2.PK, "N", "STD", "UNR",
											"ALL", "EXP", "AIR", 15.7m, 20.01m });

			AssertDataRow(org2Rows[1], headers, new object[] {  "ORG2", string.Empty, org2.PK, "N", "STD", "UNR",
											"SHP", "IMP", "AIR", 25.0m, 7.00m });

			Assert(true);
		}
		DataTable RunScript(ZGuid debtorGroupPK)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
EXEC Report_ARProfileCFX
'{0}',	--@Company
'{1}',		--@AccountTypeActive
'',		--@IsTemporary
'',		--@Debtors 
'{2}',	--@DebtorGroupPK
NULL,		--@BranchPK
NULL,	--@UNLOCO
NULL,	--@CountryCode
NULL,	--@SalesID
''		--@StaffRole
",
			GlbCompany.CurrentCompany.PK,
			"Active",
			debtorGroupPK
			));
		}
	}
}
