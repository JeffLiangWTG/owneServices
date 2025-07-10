using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing
{
	[TestedType(typeof(InvoicingLineTaxSummaryCollection))]
	public class InvoicingLineTaxSummaryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<InvoicingLineTaxSummaryCollection>
	{
		public void TestLoadData()
		{
			var taxSummaryCollection = new InvoicingLineTaxSummaryCollection();

			var branch = GlbBranch.CurrentBranch;
			var invoice = ObjectCreator.CreateAPInvoice<APInvoice>("TestINV1", ObjectCreator.AUD, 1.0m, 10m, 20m, 0, 10, 20, 0);
			invoice.AH_OH = ObjectCreator.Creditor1.PK;

			var msg1 = Factory.NewWithValidTestData<AccInvMsg>();
			var msg2 = Factory.NewWithValidTestData<AccInvMsg>();
			ObjectCreator.GST1.AT_A9_DefaultVatClass = msg1.PK;
			msg1.A9_Code = "TEST";
			msg2.A9_Code = "NEW";

			var invoiceLine1 = invoice.Lines[0];
			var invoiceLine2 = CreateAPInvoiceLine(invoice, branch.PK, null, 20, 30);
			var invoiceLine3 = CreateAPInvoiceLine(invoice, branch.PK, ObjectCreator.GSTFREE1, 30, 40);
			var invoiceLine4 = CreateAPInvoiceLine(invoice, branch.PK, ObjectCreator.GST1, 40, 50);
			var invoiceLine5 = CreateAPInvoiceLine(invoice, branch.PK, ObjectCreator.GST1, 70, 80);
			var invoiceLine6 = CreateAPInvoiceLine(invoice, branch.PK, ObjectCreator.GST1, 100, 110);
			invoiceLine6.AL_A9_VATClass = msg2.PK;

			taxSummaryCollection.Load(invoice.Lines);
			AssertEquals("Should group by 3 items", taxSummaryCollection.Count, 3);
			TaxSummaryTestHelper.AssertTaxSummaryResult(taxSummaryCollection[0], ObjectCreator.GSTFREE1.AT_Code, null, 30, 40, invoiceLine3.AL_LocalTotalAmount, ObjectCreator.GSTFREE1.AT_Description);
			TaxSummaryTestHelper.AssertTaxSummaryResult(taxSummaryCollection[1], ObjectCreator.GST1.AT_Code, "TEST", 110, 130, invoiceLine4.AL_LocalTotalAmount + invoiceLine5.AL_LocalTotalAmount, ObjectCreator.GST1.AT_Description);
			TaxSummaryTestHelper.AssertTaxSummaryResult(taxSummaryCollection[2], ObjectCreator.GST1.AT_Code, "NEW", 100, 110, invoiceLine6.AL_LocalTotalAmount, ObjectCreator.GST1.AT_Description);

			invoice.Lines.Remove(invoiceLine1);
			invoice.Lines.Remove(invoiceLine2);

			taxSummaryCollection.Load(invoice.Lines);
			AssertEquals("Should group by 3 items", taxSummaryCollection.Count, 3);
			TaxSummaryTestHelper.AssertTaxSummaryResult(taxSummaryCollection[0], ObjectCreator.GSTFREE1.AT_Code, null, 30, 40, invoiceLine3.AL_LocalTotalAmount, ObjectCreator.GSTFREE1.AT_Description);
			TaxSummaryTestHelper.AssertTaxSummaryResult(taxSummaryCollection[1], ObjectCreator.GST1.AT_Code, "TEST", 110, 130, invoiceLine4.AL_LocalTotalAmount + invoiceLine5.AL_LocalTotalAmount, ObjectCreator.GST1.AT_Description);
			TaxSummaryTestHelper.AssertTaxSummaryResult(taxSummaryCollection[2], ObjectCreator.GST1.AT_Code, "NEW", 100, 110, invoiceLine6.AL_LocalTotalAmount, ObjectCreator.GST1.AT_Description);

			var invoiceLine7 = CreateAPInvoiceLine(invoice, branch.PK, null, 10, 20);
			taxSummaryCollection.Load(invoice.Lines);
			AssertEquals("Should group by 3 items", taxSummaryCollection.Count, 3);
			TaxSummaryTestHelper.AssertTaxSummaryResult(taxSummaryCollection[0], ObjectCreator.GSTFREE1.AT_Code, null, 30, 40, invoiceLine3.AL_LocalTotalAmount, ObjectCreator.GSTFREE1.AT_Description);
			TaxSummaryTestHelper.AssertTaxSummaryResult(taxSummaryCollection[1], ObjectCreator.GST1.AT_Code, "TEST", 110, 130, invoiceLine4.AL_LocalTotalAmount + invoiceLine5.AL_LocalTotalAmount, ObjectCreator.GST1.AT_Description);
			TaxSummaryTestHelper.AssertTaxSummaryResult(taxSummaryCollection[2], ObjectCreator.GST1.AT_Code, "NEW", 100, 110, invoiceLine6.AL_LocalTotalAmount, ObjectCreator.GST1.AT_Description);
		}

		protected override InvoicingLineTaxSummaryCollection GetCollectionToTest()
		{
			return new InvoicingLineTaxSummaryCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new InvoicingLineTaxSummary();
		}

		#region implementation

		APInvoiceLine CreateAPInvoiceLine(APInvoice apInvoice, ZGuid branch, AccTaxRate taxRate, ZDecimal exTaxAmount, ZDecimal taxAmount)
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();

			var invoiceLine = (APInvoiceLine)apInvoice.Lines.AddNew();
			invoiceLine.AL_JH = job.PK;
			invoiceLine.AL_GE = Env.CurrentDepartment.PK;
			invoiceLine.AL_GB = branch;
			invoiceLine.AL_GC = Env.CurrentCompany.PK;
			if (taxRate != null)
			{
				invoiceLine.AL_AT = taxRate.PK;
			}
			invoiceLine.AL_LocalExTaxAmount = exTaxAmount;
			invoiceLine.AL_LocalTaxAmount = taxAmount;

			ObjectCreator.CreateJobCharge(invoiceLine, job, ObjectCreator.CC1, ObjectCreator.AUD);

			return invoiceLine;
		}

		TestObjectCreator ObjectCreator
		{
			get
			{
				if (fObjectCreator == null)
				{
					fObjectCreator = new TestObjectCreator(Factory);
				}
				return fObjectCreator;
			}
		}
		TestObjectCreator fObjectCreator;

		#endregion
	}
}
