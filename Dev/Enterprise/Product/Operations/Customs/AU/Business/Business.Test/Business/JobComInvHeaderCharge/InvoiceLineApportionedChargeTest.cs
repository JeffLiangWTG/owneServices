using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(InvoiceLineApportionedCharge))]
	public class InvoiceLineApportionedChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertEquals("Lookups type", typeof(JobComInvHeaderChargeLookups), appCharge.Lookups.GetType());
		}

		#region Implementation

		JobDeclaration testDec;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		InvoiceLineApportionedCharge appCharge;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<JobDeclaration>();
			invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			testDec.ResumeApportionment();
			appCharge = invoiceLine.ApportionedCharges.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			SetUp();
			return appCharge;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		#endregion
	}
}
