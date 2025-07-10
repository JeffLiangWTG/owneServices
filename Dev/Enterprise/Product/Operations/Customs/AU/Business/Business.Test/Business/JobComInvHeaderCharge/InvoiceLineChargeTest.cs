using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(InvoiceLineCharge))]
	public class InvoiceLineChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertEquals("Lookups type", typeof(JobComInvHeaderChargeLookups), lineCharge.Lookups.GetType());
		}

		public void TestValidation()
		{
			AssertEquals("Validation type", typeof(InvoiceLineChargeValidation), lineCharge.Validation.GetType());
		}

		#region Implementation

		JobDeclaration testDec;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		InvoiceLineCharge lineCharge;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<JobDeclaration>();
			invoice = testDec.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			lineCharge = invoiceLine.Charges.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			SetUp();
			return lineCharge;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		#endregion
	}
}
