using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(vw_OrgCollectionCall))]
	class vw_OrgCollectionCallTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			string sqlToInsertFirstHeader = @"INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger,AH_TransactionType,AH_TransactionNum, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_GB, AH_GC, AH_GE, AH_TransactionCount,AH_OH) values (NEWID(), 'AR','INV', '00001001','Test_Desc','Jan 25 2008  3:44:00:000PM','Jan 25 2009  3:44:00:000PM','FDD429D2-648C-4895-8F9F-06E90DED2BE5','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC','4E5A97E8-85F4-41EC-95C6-5D14A676157C', 1, '4F1F6B5D-F65F-4B9F-A769-8C170A7A8642')";
			string sqlToInsertSecondHeader = @"INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger,AH_TransactionType,AH_TransactionNum, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_GB, AH_GC, AH_GE, AH_TransactionCount, AH_OH) values (NEWID(), 'AR','INV', '00001002','Test_Desc','Jan 28 2008  3:44:00:000PM','Jan 28 2009  3:44:00:000PM','FDD429D2-648C-4895-8F9F-06E90DED2BE5','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC','4E5A97E8-85F4-41EC-95C6-5D14A676157C', 1, '4F1F6B5D-F65F-4B9F-A769-8C170A7A8642')";

			TestConnection.ExecuteNonQuery(sqlToInsertFirstHeader);
			TestConnection.ExecuteNonQuery(sqlToInsertSecondHeader);

			string sqlText = string.Format(@"
				SELECT TOP 1 CC_OH_FullName, CC_OB_ARInvoiceTerms, CC_LastInvoiced, CC_OldestDueDate
				FROM {0} WHERE CC_DebtorCode = 'EDICUS' AND CC_GC = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'", ScriptToTest.Name);
			DataTable table = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);

			AssertEquals("Organisation Full Name", "EDI CUSTOMS BROKERS", table.Rows[0][0].ToString());
			AssertEquals("AR Invoice Terms", "COD", table.Rows[0][1].ToString());

			var culture = CultureInfo.GetCultureInfo("en-AU");
#if NETFRAMEWORK
			AssertEquals("Oldest Due Date", "28/01/2008 3:44:00 PM", ((IFormattable)table.Rows[0][2]).ToString("G", culture));
			AssertEquals("Latest Invoice Date", "25/01/2009 3:44:00 PM", ((IFormattable)table.Rows[0][3]).ToString("G", culture));
#else
			AssertEquals("Oldest Due Date", "28/1/2008 3:44:00 pm", ((IFormattable)table.Rows[0][2]).ToString("G", culture));
			AssertEquals("Latest Invoice Date", "25/1/2009 3:44:00 pm", ((IFormattable)table.Rows[0][3]).ToString("G", culture));
#endif
		}

		[UseSnapshotProtection]
		public void TestIndex()
		{
			var sqlToInsertFirstHeader = @"INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger,AH_TransactionType,AH_TransactionNum, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_GB, AH_GC, AH_GE, AH_TransactionCount,AH_OH) values (NEWID(), 'AR','INV', '00001001','Test_Desc','Jan 25 2008  3:44:00:000PM','Jan 25 2009  3:44:00:000PM','FDD429D2-648C-4895-8F9F-06E90DED2BE5','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC','4E5A97E8-85F4-41EC-95C6-5D14A676157C', 1, '4F1F6B5D-F65F-4B9F-A769-8C170A7A8642')";
			var sqlToInsertSecondHeader = @"INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger,AH_TransactionType,AH_TransactionNum, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_GB, AH_GC, AH_GE, AH_TransactionCount, AH_OH) values (NEWID(), 'AR','INV', '00001002','Test_Desc','Jan 28 2008  3:44:00:000PM','Jan 28 2009  3:44:00:000PM','FDD429D2-648C-4895-8F9F-06E90DED2BE5','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC','4E5A97E8-85F4-41EC-95C6-5D14A676157C', 1, '4F1F6B5D-F65F-4B9F-A769-8C170A7A8642')";
			var sqlToInsertOrgStaffAssignments = @"INSERT INTO dbo.OrgStaffAssignments (O8_PK, O8_Role, O8_Department, O8_OH, O8_GC, O8_GS_NKPersonResponsible) VALUES(NEWID(), 'ACT', 'ALL', '4F1F6B5D-F65F-4B9F-A769-8C170A7A8642', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'AK')";

			TestConnection.ExecuteNonQuery(sqlToInsertFirstHeader);
			TestConnection.ExecuteNonQuery(sqlToInsertSecondHeader);
			TestConnection.ExecuteNonQuery(sqlToInsertOrgStaffAssignments);

			var sqlText = string.Format(@"
				SELECT *
				FROM {0}
				WHERE 
					CC_GC = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
					and
					(
						(
							CC_TotalOverdueAmount >= 0
							and
							(
								CC_OH IN (SELECT O8_OH FROM dbo.OrgStaffAssignments WHERE O8_GS_NKPersonResponsible = 'AK' and O8_Role = 'ACT' and O8_GC = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')
							)
						))", ScriptToTest.Name);

			using (var adminConnection = Db.NewAdminConnection())
			{
				adminConnection.ExecuteNonQuery($"UPDATE STATISTICS {GlbDepartmentSchema.Constants.TableName} WITH ROWCOUNT=3000");

				try
				{
					// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
					using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
					{
						var totalRows = 0;
						TestConnection.ExecuteReader(sqlText, _ => totalRows++);
						AssertEquals("Total Rows = 1", 1, totalRows);
						var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains(ScriptToTest.Name));
						var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());
						CombineAssertions(() =>
						{
							Assert("Clustered index FK_RC__OC_OH must be used.", queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexName == "FK_RC__OC_OH"));
							Assert("None clustered index PK_UX__OC_PK must be used.", queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexName == "PK_UX__OC_PK"));
							Assert("None clustered index NR_RX__O8_Role_O8_GC_O8_GS_NKPersonResponsible must be used.", queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexName == "NR_RX__O8_Role_O8_GC_O8_GS_NKPersonResponsible"));
							Assert("There must have no Table Scan been used.", !queryPlanAnalyzer.TableScans.Any());
						});
					}
				}
				finally
				{
					adminConnection.ExecuteNonQuery($"DBCC UPDATEUSAGE ({Db.Connection.CurrentDatabase}, {GlbDepartmentSchema.Constants.TableName}) WITH COUNT_ROWS");
				}
			}
		}
	}
}
