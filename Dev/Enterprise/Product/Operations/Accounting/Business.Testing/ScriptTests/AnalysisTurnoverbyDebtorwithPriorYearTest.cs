

using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class AnalysisTurnoverbyDebtorwithPriorYearTest : ScriptTest
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

			DataTable resultForSettlementGroup = RunScript();

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);
			AssertEquals("All result has ABIGAS as settlemet code", 3, resultForSettlementGroup.Select("SettlementCode = 'ABIGAS'").Length);
		}

		DataTable RunScript()
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
SELECT * 
FROM AnalysisTurnoverbyDebtorwithPriorYear(
'{0}',	--@Company
'',		--@BranchPKList			     
'',		--@SalesRepList			    
'',		--@SalesRepRoll			    
'INV',	--@TransactionTypeList	    
'',		--@ExcludeGSV				
'{1}',	--@PeriodFromDate				
'{2}',	--@PeriodToDate				
NULL,	--@LastYearPeriodFromDate				
NULL,	--@LastYearPeriodToDate				
NULL,	--@ThisYearFromDate				
NULL,	--@LastYearFromDate				
NULL	--@LastYearToDate				
)  
",
			GlbCompany.CurrentCompany.PK,
			ZDateTime.Today.AddDays(-1).ToISO8601String(),
			ZDateTime.Today.AddDays(1).ToISO8601String()
			));
		}
	}
}


