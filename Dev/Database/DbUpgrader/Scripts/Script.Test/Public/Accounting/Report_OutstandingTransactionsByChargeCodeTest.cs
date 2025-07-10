using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_OutstandingTransactionsByChargeCode))]
	class Report_OutstandingTransactionsByChargeCodeTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '200605', 'May 01 2006  3:44:00:000PM','May 31 2006  3:44:00:000PM','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_GB, AH_GC, AH_GE, AH_TransactionCount, AH_OH) 
				values('01FE98E0-E893-463C-BE75-01ACD2E72E0D', 'AP', 'INV', '00001001', 'Test_Desc', 'May 25 2006  3:44:00:000PM', 'May 25 2006  3:44:00:000PM', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '4E5A97E8-85F4-41EC-95C6-5D14A676157C', 1, '4F1F6B5D-F65F-4B9F-A769-8C170A7A8642')");

			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccTransactionLines(AL_PK, AL_LineType, AL_LineAmount, AL_AH, AL_AG, AL_GC, AL_GB, AL_GE)
									VALUES(NEWID(), 'CST', -10.00, '01FE98E0-E893-463C-BE75-01ACD2E72E0D', 'ac129d82-b88d-45ee-bce5-25592f734023', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '2B67864D-42E9-4A43-A9C8-09D2083C4227')");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "select * from Report_OutstandingTransactionsByChargeCode ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC','AP', null,null,null,null,null,null,null,null,null,null,null,null, 200605,'INV','',1,2,3,4,'2008-07-01 00:00:00','','')");
			AssertEquals("Result should have one row", 1, result.Rows.Count);
		}
	}
}

