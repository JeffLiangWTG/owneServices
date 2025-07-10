using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Build.Database.Script.TestFramework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.DbCreateScriptTests.CashFlow
{
	class CashFlowTypeStillInUseButNotFoundInRegistryTest : ScriptTest
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main })]
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
				var companyPK = Helper.InsertCompany("ABC", "ABC Compay", "CNY", "CN", true, true);
				companyPKStr = companyPK.ToString();
				var branch2PK = Helper.InsertBranch("BR2", companyPK, "Branch 2");
				branchPKStr = branch2PK.ToString();
				var organisationPK = TestDataCreator.CreateOrganisation("1stCarDiv", "1st Carrier Division");
				var agPK = Helper.InsertGLAccount("4444.22.22", "TestGLAccount 1", "P&L", cashFlowType: "111");
				var orgCreditorGroupPK = Guid.NewGuid().ToString();
				var ahPK = Guid.NewGuid().ToString();

				TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company)
								values(NEWID(), 201512, 2015, '12/01/2015', '12/30/2015 23:59', 1, 1, '{companyPK}')");

				TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company)
								values(NEWID(), 201601, 2016, '01/01/2016', '01/31/2016 23:59', 1, 1, '{companyPK}')");

				TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company)
								values(NEWID(), 201602, 2016, '02/01/2016', '02/29/2016 23:59', 1, 1, '{companyPK}')");

				TestConnection.ExecuteNonQuery($@"INSERT dbo.OrgCreditorGroup(OG_PK, OG_Code, OG_Desc) VALUES('{orgCreditorGroupPK}', '~C1', '~CreditorGroup')");

				TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.OrgCompanyData(OB_PK, OB_OH, OB_GC, OB_IsCreditor, OB_OG_APCreditorGroup)
								VALUES (newid(), '{organisationPK}', '{companyPK}', 1, '{orgCreditorGroupPK}')");

				TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_TransactionCategory)
								VALUES('7BE5FA30-4618-497C-8E83-182104925E4F', 'AR', 'REC', '00001001', 'DEC 05 2015  3:44:00:000PM', 'DEC 25 2015  3:44:00:000PM', 'Y', 100.0000, 100.0000, 'CNY', 1, 'DEC 05 2015  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '{companyPK}', '{branch2PK}', 'f5c72696-19ad-4759-879f-89c8532ff238', 'tst')");

				TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE)
								VALUES('344319BA-BBFE-4E80-8B1F-0E587B3240C6', 'AP', 'PAY', '123456', 'JAN 05 2015  3:44:00:000PM', 'JAN 05 2015  3:44:00:000PM', 'Y', -30.0000, -30.0000, 'CNY', 1, 'DEC 05 2015  3:44:00:000PM', '{organisationPK}', '{companyPK}', '{branch2PK}', 'f5c72696-19ad-4759-879f-89c8532ff238')");

				TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE)
								VALUES('{ahPK}', 'CB', 'PAY', '123456', 'JAN 05 2015  3:44:00:000PM', 'JAN 05 2015  3:44:00:000PM', 'Y', -30.0000, -30.0000, 'CNY', 1, 'DEC 05 2015  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '{companyPK}', '{branch2PK}', 'f5c72696-19ad-4759-879f-89c8532ff238')");

				TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_AG, AL_GC, AL_GB, AL_GE, AL_LineType, AL_PostDate)
								VALUES(NEWID(), '{ahPK}', '{agPK}', '{companyPK}', '{branch2PK}', 'f5c72696-19ad-4759-879f-89c8532ff238', 'DRC', '2015-12-02')");

				var insertRegistryValues = $@"DECLARE @registryRawValue nvarchar(MAX)
Set @registryRawValue = '<ArrayOfCashFlowCategoryBasedOnCreditorGroup xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><CashFlowCategoryBasedOnCreditorGroup><OrgGroupPK>{orgCreditorGroupPK}</OrgGroupPK><CashFlowCategory>O04</CashFlowCategory></CashFlowCategoryBasedOnCreditorGroup><CashFlowCategoryBasedOnCreditorGroup><OrgGroupPK>{orgCreditorGroupPK}</OrgGroupPK><CashFlowCategory>OEQ</CashFlowCategory></CashFlowCategoryBasedOnCreditorGroup></ArrayOfCashFlowCategoryBasedOnCreditorGroup>'
INSERT INTO dbo.StmData
(SD_PK, SD_Name, SD_Type, SD_BinaryValue, SD_IsLogged, SD_IsCancelled)
Values
(newID(), 'CashFlowCategoryBasedOnCreditorGroup', 'BIN', CONVERT(varbinary(MAX), @registryRawValue), 1, 0)";
				TestConnection.ExecuteNonQuery(insertRegistryValues);
				TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue) VALUES(NEWID(), 'JournalEntriesLastProcessedDate', '{companyPK}', NULL, 'DT', convert(varbinary(8000), N'2010-01-01 00:00:00.000'), NULL)");
			}
		}

		string ScriptDbName
		{
			get { return Db.DatabaseName; }
		}

		TestDbHelper Helper => helper ?? (helper = new TestDbHelper(TestConnection));

		TestDbHelper helper;

		DataTable Execute(string companyPK, int period, string branchID)
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, string.Format($"SELECT * FROM [{ScriptDbName}].[dbo].CashFlowTypeStillInUseButNotFoundInRegistry({period}, '{companyPK}', '{branchID}')"));
		}
	}
}

