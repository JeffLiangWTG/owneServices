

using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class AnalysisTurnoverbyDebtorwithTypeByBranchTest : ScriptTest
	{
		public void TestSettlementGroup()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.ABIGAS.ARSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.LocalClient.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.LocalClient2.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.LocalClient, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.LocalClient2, glAccount.PK);
			Factory.Save();

			DataTable resultForSettlementGroup = RunScript(ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);
			AssertEquals("All result has ABIGAS as settlemet code", 3, resultForSettlementGroup.Select("SettlementCode = 'ABIGAS'").Length);
		}

		public void TestNoOverlapOfPeriods()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2011, 07, 01));

			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();

			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.LocalClient, glAccount.PK);
			var invoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.LocalClient2, glAccount.PK);
			var invoice4 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV4", TestObjectCreator.AUD, 1M, 40M, 0M, 40M, 0M, TestObjectCreator.AALSHI, glAccount.PK);
			var invoice5 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV5", TestObjectCreator.AUD, 1M, 50M, 0M, 50M, 0M, TestObjectCreator.XLINDU, glAccount.PK);
			invoice1.AH_PostDate = new ZDateTime(2011, 05, 31, 23, 59, 59);
			invoice2.AH_PostDate = new ZDateTime(2011, 06, 01, 00, 00, 00);
			invoice3.AH_PostDate = new ZDateTime(2011, 06, 15, 12, 00, 00);
			invoice4.AH_PostDate = new ZDateTime(2011, 06, 30, 23, 59, 00);
			invoice5.AH_PostDate = new ZDateTime(2011, 07, 01, 00, 00, 00);

			Factory.Save();

			DataTable resultTable1 = RunScript(new ZDateTime(2011, 06, 01, 00, 00, 00), new ZDateTime(2011, 06, 30, 23, 59, 00));

			AssertEquals("Resulting table should NOT include transactions from other periods", 3, resultTable1.Rows.Count);
		}

		DataTable RunScript(ZDateTime fromDate, ZDateTime toDate)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
SELECT * 
FROM AnalysisTurnoverbyDebtorwithTypeByBranch(
'{1}',	--@FromDate				
'{2}',	--@ToDate				
'{0}',	--@Company
'',		--@BranchPKList			     
'',		--@SalesRepList			    
'',		--@SalesRepRoll			    
'INV',	--@TransactionTypeList	    
''		--@ExcludeGSV				
)  
",
			GlbCompany.CurrentCompany.PK,
			fromDate.ToISO8601String(),
			toDate.ToISO8601String()
			));
		}
	}
}


