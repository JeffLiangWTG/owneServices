using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Meursing;

namespace Enterprise.Customs.EU.Business.Testing
{
	public class InvoiceLinesMeursingTargetTest : TestCaseWithFactory
	{
		public void TestGetFactory()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			IMeursingTarget targets = new InvoiceLinesMeursingTarget(lines);

			AssertNotNull(targets.Factory);
		}

		public void TestSetMeursingResult()
		{
			JobComInvoiceLine line1 = Factory.New<JobComInvoiceLine>();
			JobComInvoiceLine line2 = Factory.New<JobComInvoiceLine>();
			List<JobComInvoiceLine> lines = new List<JobComInvoiceLine> { line1, line2 };
			IMeursingTarget targets = new InvoiceLinesMeursingTarget(lines);
			targets.SetMeursingResult("7001");

			foreach (var line in lines)
			{
				AssertEquals(line.JI_SupplementaryCode1, "7001");
			}
		}
	}
}
