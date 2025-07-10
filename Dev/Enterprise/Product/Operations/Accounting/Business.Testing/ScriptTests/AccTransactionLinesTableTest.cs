using System.Linq;
using CargoWise.Data.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	public class AccTransactionLinesTableTest : ScriptTest
	{
		public void TestNR_RX__AL_RX_NKTransactionCurrencyIndexIsUsed_WhenNewCurrencyIsAdded()
		{
			for (var i = 0; i < 100; i++)
			{
				var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
				TestObjectCreator.CreateInvoiceLine(invoice, 100);
			}
			Factory.Save();
			AssertNotEquals("Precondition - vw_AccTransactionHeaderTax_Base", 0, TestConnection.ExecuteScalar("Select count(*) from dbo.vw_AccTransactionHeaderTax_Base"));
			TestConnection.ExecuteNonQuery($"UPDATE STATISTICS {AccTransactionLinesSchema.Constants.TableName} WITH FULLSCAN");
			TestConnection.ExecuteNonQuery($"UPDATE STATISTICS {AccTransactionHeaderSchema.Constants.TableName} WITH FULLSCAN");

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var sql = @"INSERT INTO dbo.RefCurrency VALUES (newid() ,'ABD' ,1 ,1 ,'ABC' ,'DESC' ,'UNIT' ,'SUBUNIT' ,100 ,100 ,1 ,getdate() ,'E' ,getdate() ,'E')";
				using (var reader = TestConnection.Command(sql).ExecuteReader())
				{
					while (reader.Read())
					{ }
				}
				var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains("RefCurrency"));
				var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());

				AssertEquals("TableScans", false, queryPlanAnalyzer.TableScans.Any());
				AssertEquals("Index Seek", true, queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexName == "NR_RX__AL_RX_NKTransactionCurrency"));
			}
		}
	}
}
