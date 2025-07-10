using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class JobDeclarationPerformanceTest : TestCaseWithFactory
	{
		public void TestSetJE_RL_NKFinalDestination_QuarantineExDocLineDbHit()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			Factory.Save();
			var dbHitCount = Factory.GetTableHitCount("QuarantineExDocLine");
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			var newdbHitCount = Factory.GetTableHitCount("QuarantineExDocLine");
			AssertEquals(dbHitCount, newdbHitCount);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			dbHitCount = Factory.GetTableHitCount("QuarantineExDocLine");
			declaration.JE_RL_NKFinalDestination = "AUMEL";
			newdbHitCount = Factory.GetTableHitCount("QuarantineExDocLine");
			AssertEquals(dbHitCount + 1, newdbHitCount);
		}
	}
}
