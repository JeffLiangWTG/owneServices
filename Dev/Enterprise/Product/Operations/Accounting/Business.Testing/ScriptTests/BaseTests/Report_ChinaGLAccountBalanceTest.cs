using System;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Accounting.Utility.Testing;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.BaseTests
{
	[UseSnapshotProtection(new[] { DatabaseType.Main })]
	class Report_ChinaGLAccountBalanceTest : ScriptTest
	{
		public void TestReport_ChinaGLAccountBalanceTest()
		{
			using (Connection = Db.NewAdminConnection())
			{
				PrepareReport_ChinaGLAccountBalanceTest();

				var result = Execute("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", 201512, "");
				AssertEquals(1, result.Select("AccountNum = '6210.00.00' and Period = 201512 and Amount =100 and OpeningBalance =0 and Currency ='CNY' and Account = 'WALHAT'").Length);

				result = Execute("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", 201601, "");
				AssertEquals(1, result.Select("AccountNum = '6210.00.00' and Period = 201601 and Amount =-30 and OpeningBalance =100 and Currency ='CNY' and Account = 'WALHAT'").Length);

				result = Execute("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", 201602, "");
				AssertEquals(1, result.Select("AccountNum = '6210.00.00' and Period = 201602 and Amount =0 and OpeningBalance =70 and Currency ='CNY' and Account = 'WALHAT'").Length);
			}
		}

		protected virtual void PrepareReport_ChinaGLAccountBalanceTest()
		{
			PrepareData();
		}

		protected virtual string ScriptDbName => Db.DatabaseName;

		DataTable Execute(string companyPK, int endPeriod, string branchList, string includePeriodEndCLosing = "")
		{
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendLine($"EXEC [{ScriptDbName}].[dbo].[Report_ChinaGLAccountBalance]");
			sqlBuilder.Append($"@CompanyPK = '{companyPK}'");
			sqlBuilder.Append(
				$@",@EndPeriod = ").Append(endPeriod == 0 ? "NULL" : endPeriod.ToString());
			sqlBuilder.Append(
				$@",@BranchPKList = ").Append(branchList == null ? "NULL" : $"'{branchList}'");
			sqlBuilder.Append(
				$@",@IncludePeriodEndCLosing = ").Append($"'{includePeriodEndCLosing}'");

			var sqlText = string.Format(CultureInfo.InvariantCulture,
				sqlBuilder.ToString()
			);

			return DataUtils.GetDataTableFromQuery(Connection, sqlText);
		}

		void PrepareData()
		{
			var bankAccountPK = Guid.NewGuid();
			Connection.ExecuteNonQuery(string.Format("INSERT INTO dbo.AccBankAccount (AB_PK, AB_Code, AB_RX_NKAccountCurrency, AB_AG, AB_GC) VALUES ('{0}', 'BANKCDE', 'CNY', 'A17ACD2B-4303-4F08-8E81-56C94353D270', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')", bankAccountPK));

			Connection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company)
								values(NEWID(), 201512, 2015, '12/01/2015', '12/30/2015 23:59', 1, 1, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

			Connection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company)
								values(NEWID(), 201601, 2016, '01/01/2016', '01/31/2016 23:59', 1, 1, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

			Connection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company)
								values(NEWID(), 201602, 2016, '02/01/2016', '02/29/2016 23:59', 1, 1, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

			Connection.ExecuteNonQuery($@"INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AG)
								VALUES('7BE5FA30-4618-497C-8E83-182104925E4F', 'AR', 'JNL', '00001001', 'DEC 05 2015  3:44:00:000PM', 'DEC 25 2015  3:44:00:000PM', 'Y', 100.0000, 100.0000, 'CNY', 1, 'DEC 05 2015  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', 'A17ACD2B-4303-4F08-8E81-56C94353D270')");

			Connection.ExecuteNonQuery($@"INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_AB)
								VALUES('344319BA-BBFE-4E80-8B1F-0E587B3240C6', 'AR', 'REC', '123456', 'JAN 05 2016  3:44:00:000PM', 'JAN 05 2016  3:44:00:000PM', 'Y', -30.0000, -30.0000, 'CNY', 1, 'JAN 05 2016  3:44:00:000PM', 'c72bfdd2-e264-4dd9-a5ab-3f75efd8569b', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '27a55065-ac88-4ec3-8bed-e575e79172cb', 'f5c72696-19ad-4759-879f-89c8532ff238', '{bankAccountPK}')");

			Connection.ExecuteNonQuery(@"INSERT INTO dbo.AccTransactionMatchLink(AP_PK, AP_Amount, AP_MatchGroupNum, AP_MatchDate, AP_AH)
								VALUES(NEWID(), 30, 'M10000', 'JAN 20 2016  3:44:00:000PM', '7BE5FA30-4618-497C-8E83-182104925E4F')");

			Connection.ExecuteNonQuery(@"INSERT INTO dbo.AccTransactionMatchLink(AP_PK, AP_Amount, AP_MatchGroupNum, AP_MatchDate, AP_AH)
								VALUES(NEWID(), -30, 'M10000', 'JAN 20 2016  3:44:00:000PM', '344319BA-BBFE-4E80-8B1F-0E587B3240C6')"
			);

			Connection.ExecuteNonQuery(@"INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue) VALUES(NEWID(), 'JournalEntriesLastProcessedDate', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', NULL, 'DT', convert(varbinary(8000), N'2010-01-01 00:00:00.000'), NULL)"
			);
		}

		protected AdminConnection Connection;
	}
}
