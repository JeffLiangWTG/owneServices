using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_CompanyAccountingTaxConfigurationProfileTest : ScriptTest
	{
		public void TestTaxConfigurationProfileDetailsFilterByCompany()
		{
			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("TA");

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem1 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS1", registrationLevel: TaxSystemRegistrationLevels.Company.Code);
			var taxSystem2 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS2", registrationLevel: TaxSystemRegistrationLevels.Branch.Code);

			taxSystemsConfigCollection.Add(taxSystem1);
			taxSystemsConfigCollection.Add(taxSystem2);

			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			var companyA = TestObjectCreator.CreateNewCompany("AAA");
			var companyB = TestObjectCreator.CreateNewCompany("BBB");
			var companyC = TestObjectCreator.CreateNewCompany("CCC");
			var companyD = TestObjectCreator.CreateNewCompany("DDD");
			var branchB = TestObjectCreator.CreateNewBranch(companyB, "AAB");
			var branchD = TestObjectCreator.CreateNewBranch(companyD, "DDB");

			Factory.Save();

			var taxConfiguration1 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(companyA, taxAuthority, taxSystem1);
			var taxConfiguration2 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branchB, taxAuthority, taxSystem2);
			var taxConfiguration3 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(companyD, taxAuthority, taxSystem1);
			var taxConfiguration4 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branchD, taxAuthority, taxSystem2);
			var glHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			var glHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			var glHeader3 = Factory.NewWithValidTestData<AccGLHeader>();

			glHeader1.AG_AccountNum = "1111.11.11";
			glHeader2.AG_AccountNum = "4444.44.44";
			glHeader3.AG_AccountNum = "5555.55.55";

			taxConfiguration1.ETC_Code = "TAXCONFIG1";
			taxConfiguration1.ETC_Description = "Test configuration companyA";
			taxConfiguration1.ETC_AG_LedgerControlAccount = glHeader1.PK;
			taxConfiguration1.ETC_AG_TaxControlAccount = glHeader2.PK;
			taxConfiguration1.ETC_AG_TaxExpenseAccount = glHeader3.PK;

			taxConfiguration2.ETC_Code = "TAXCONFIG2";
			taxConfiguration2.ETC_Description = "Test configuration branchB";
			taxConfiguration2.ETC_AG_LedgerControlAccount = glHeader2.PK;
			taxConfiguration2.ETC_AG_TaxControlAccount = glHeader3.PK;
			taxConfiguration2.ETC_AG_TaxPendingControlAccount = glHeader3.PK;

			taxConfiguration3.ETC_Code = "TAXCONFIG3";
			taxConfiguration3.ETC_Description = "Test configuration companyD";
			taxConfiguration3.ETC_AG_LedgerControlAccount = glHeader1.PK;
			taxConfiguration3.ETC_AG_TaxExpenseAccount = glHeader2.PK;

			taxConfiguration4.ETC_Code = "TAXCONFIG4";
			taxConfiguration4.ETC_Description = "Test configuration branchD";
			taxConfiguration4.ETC_AG_LedgerControlAccount = glHeader2.PK;
			taxConfiguration4.ETC_AG_TaxControlAccount = glHeader3.PK;

			Factory.Save();

			var dataTable = RunScript();
			var expectedResult = @"
GC_Code GC_Name                                                                                              GB_Code GB_BranchName                                      ETC_RN_NKCountry ETC_Code                       ETC_Description                                                                  ETC_TaxAuthorityCode ETC_TaxSystemCode ETC_Ledger ETC_TaxRecordCreationTrigger ETC_TaxRealisationMethod ETC_IsActive LedgerControlAccount TaxControlAccount TaxPendingControlAccount TaxExpenseAccount
------- ---------------------------------------------------------------------------------------------------- ------- -------------------------------------------------- ---------------- ------------------------------ -------------------------------------------------------------------------------- -------------------- ----------------- ---------- ---------------------------- ------------------------ ------------ -------------------- ----------------- ------------------------ -----------------
AAA    Company                                                                                               NULL    NULL                                               AU               TAXCONFIG1                     Test configuration companyA                                                      TA                   TS1               AR         PDT                          PDT                      1            1111.11.11           4444.44.44        NULL                     5555.55.55
BBB    Company                                                                                               AAB     Test Branch                                        AU               TAXCONFIG2                     Test configuration branchB                                                       TA                   TS2               AR         PDT                          PDT                      1            4444.44.44           5555.55.55        5555.55.55               NULL
DDD    Company                                                                                               NULL    NULL                                               AU               TAXCONFIG3                     Test configuration companyD                                                      TA                   TS1               AR         PDT                          PDT                      1            1111.11.11           NULL              NULL                     4444.44.44
DDD    Company                                                                                               DDB     Test Branch                                        AU               TAXCONFIG4                     Test configuration branchD                                                       TA                   TS2               AR         PDT                          PDT                      1            4444.44.44           5555.55.55        NULL                     NULL
";
			AssertTableAsTextFromSQLServerManagenentStudio("Tax Configuration details for all companies: ", dataTable, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());

			dataTable = RunScript(new[] { companyA.PK });
			expectedResult = @"
GC_Code GC_Name                                                                                              GB_Code GB_BranchName                                      ETC_RN_NKCountry ETC_Code                       ETC_Description                                                                  ETC_TaxAuthorityCode ETC_TaxSystemCode ETC_Ledger ETC_TaxRecordCreationTrigger ETC_TaxRealisationMethod ETC_IsActive LedgerControlAccount TaxControlAccount TaxPendingControlAccount TaxExpenseAccount
------- ---------------------------------------------------------------------------------------------------- ------- -------------------------------------------------- ---------------- ------------------------------ -------------------------------------------------------------------------------- -------------------- ----------------- ---------- ---------------------------- ------------------------ ------------ -------------------- ----------------- ------------------------ -----------------
AAA    Company                                                                                               NULL    NULL                                               AU               TAXCONFIG1                     Test configuration companyA                                                      TA                   TS1               AR         PDT                          PDT                      1            1111.11.11           4444.44.44        NULL                     5555.55.55
";
			AssertTableAsTextFromSQLServerManagenentStudio("companyA have Tax Configuration at company level: ", dataTable, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());

			dataTable = RunScript(new[] { companyB.PK });
			expectedResult = @"
GC_Code GC_Name                                                                                              GB_Code GB_BranchName                                      ETC_RN_NKCountry ETC_Code                       ETC_Description                                                                  ETC_TaxAuthorityCode ETC_TaxSystemCode ETC_Ledger ETC_TaxRecordCreationTrigger ETC_TaxRealisationMethod ETC_IsActive LedgerControlAccount TaxControlAccount TaxPendingControlAccount TaxExpenseAccount
------- ---------------------------------------------------------------------------------------------------- ------- -------------------------------------------------- ---------------- ------------------------------ -------------------------------------------------------------------------------- -------------------- ----------------- ---------- ---------------------------- ------------------------ ------------ -------------------- ----------------- ------------------------ -----------------
BBB    Company                                                                                               AAB     Test Branch                                        AU               TAXCONFIG2                     Test configuration branchB                                                       TA                   TS2               AR         PDT                          PDT                      1            4444.44.44           5555.55.55        5555.55.55               NULL
";
			AssertTableAsTextFromSQLServerManagenentStudio("companyB have Tax Configuration at branch level: ", dataTable, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());

			dataTable = RunScript(new[] { companyD.PK });
			expectedResult = @"
GC_Code GC_Name                                                                                              GB_Code GB_BranchName                                      ETC_RN_NKCountry ETC_Code                       ETC_Description                                                                  ETC_TaxAuthorityCode ETC_TaxSystemCode ETC_Ledger ETC_TaxRecordCreationTrigger ETC_TaxRealisationMethod ETC_IsActive LedgerControlAccount TaxControlAccount TaxPendingControlAccount TaxExpenseAccount
------- ---------------------------------------------------------------------------------------------------- ------- -------------------------------------------------- ---------------- ------------------------------ -------------------------------------------------------------------------------- -------------------- ----------------- ---------- ---------------------------- ------------------------ ------------ -------------------- ----------------- ------------------------ -----------------
DDD    Company                                                                                               NULL    NULL                                               AU               TAXCONFIG3                     Test configuration companyD                                                      TA                   TS1               AR         PDT                          PDT                      1            1111.11.11           NULL              NULL                     4444.44.44
DDD    Company                                                                                               DDB     Test Branch                                        AU               TAXCONFIG4                     Test configuration branchD                                                       TA                   TS2               AR         PDT                          PDT                      1            4444.44.44           5555.55.55        NULL                     NULL
";
			AssertTableAsTextFromSQLServerManagenentStudio("companyD have Tax Configurations at both company and branch levels: ", dataTable, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());

			dataTable = RunScript(new[] { companyC.PK });
			expectedResult = @"
GC_Code GC_Name                                                                                              GB_Code GB_BranchName                                      ETC_RN_NKCountry ETC_Code                       ETC_Description                                                                  ETC_TaxAuthorityCode ETC_TaxSystemCode ETC_Ledger ETC_TaxRecordCreationTrigger ETC_TaxRealisationMethod ETC_IsActive LedgerControlAccount TaxControlAccount TaxPendingControlAccount TaxExpenseAccount
------- ---------------------------------------------------------------------------------------------------- ------- -------------------------------------------------- ---------------- ------------------------------ -------------------------------------------------------------------------------- -------------------- ----------------- ---------- ---------------------------- ------------------------ ------------ -------------------- ----------------- ------------------------ -----------------
";
			AssertTableAsTextFromSQLServerManagenentStudio("companyC have no Tax Configuration: ", dataTable, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());
		}

		DataTable RunScript(ZGuid[] companyPKs = null)
		{
			var sqlQuery = "SELECT * FROM Report_CompanyAccountingTaxConfigurationProfile(@CompanyPKs, @CompanyPKsIsEmpty) ORDER BY GC_Code,GB_Code";
			var command = Db.Connection.Command(sqlQuery);
			AddTVP_uniqueidentifierAndIsEmptyParameters(command, "@CompanyPKs", companyPKs);
			return DataUtils.GetDataTableFromCommand(command);
		}

		void AddTVP_uniqueidentifierAndIsEmptyParameters(DbCommand command, string paramName, ZGuid[] values)
		{
			var table = new DataTable();
			table.Columns.Add("Value", typeof(Guid));

			if (values != null)
			{
				foreach (var value in values)
				{
					table.Rows.Add(value.ToGuid());
				}
			}

			command.AddTableValuedParameter(paramName, "dbo.TVP_uniqueidentifier", table);
			command.AddParameter(paramName + "IsEmpty", SqlDbType.Bit, table.Rows.Count == 0);
		}

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator => taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;
	}
}
