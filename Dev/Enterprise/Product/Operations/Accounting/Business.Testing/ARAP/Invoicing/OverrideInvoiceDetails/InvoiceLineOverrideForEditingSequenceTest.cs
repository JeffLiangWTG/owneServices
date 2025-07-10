using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoiceLineOverrideForEditingSequence))]
	public class InvoiceLineOverrideForEditingSequenceTest : InvoiceLineOverrideTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new InvoiceLineOverrideForEditingSequence(Factory.NewWithValidTestData<ARCreditNoteLine>());
		}

		TestObjectCreator testObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}

		public void TestPassThroughProperties_Sequence()
		{
			var job = TestObjectCreator.CreateJob("Job1", TestObjectCreator.LocalClient, 1, TestObjectCreator.Agent, 1);
			job.JH_JobLocalReference = "123";
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 100, 10, 0);
			line.AL_JH = job.PK;
			line.AL_AC = TestObjectCreator.CC1.PK;
			var charge = TestObjectCreator.CreateCharge(line);
			charge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;

			line.AL_GB = TestObjectCreator.CreateBranch("BR1", GlbCompany.CurrentCompany).PK;
			line.AL_GE = TestObjectCreator.FIADepartment.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_RX_NKTransactionCurrency = testObjectCreator.USD.RX_Code;
			line.AL_ExchangeRate = 1m;
			line.AL_Desc = "123";
			line.AL_Sequence = 5;
			Factory.Save();

			var invoiceOverride = new InvoiceLineOverrideForEditingSequence(line);

			AssertEquals((short)5, invoiceOverride.AL_Sequence);

			line.AL_Sequence = 2;

			AssertEquals((short)2, invoiceOverride.AL_Sequence);
		}

		public void TestSetAL_SequenceValidation_DuplicateSequence()
		{
			var expectedError = "The Line Sequence Number must be unique.";
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1999", TestObjectCreator.AUD, 1.0M, TestObjectCreator.ABIGAS);
			var line1 = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.CC4, TestObjectCreator.AUD, 1.0M, "hello", 100);
			var line2 = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.CC4, TestObjectCreator.AUD, 1.0M, "hello", 100);

			line2.AL_Sequence = 2;

			var invoiceOverride = new InvoiceLineOverrideForEditingSequence(line1);

			invoiceOverride.AL_Sequence = 3;
			AssertNoError(invoiceOverride.AL_SequenceInfo, expectedError);

			invoiceOverride.AL_Sequence = 2;
			AssertHasError(invoiceOverride.AL_SequenceInfo, expectedError);
		}

		public void TestSetAL_SequenceValidation_Warning()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingChina(DateTime.Now.AddDays(-30)))
			{
				var expectedWarning = "The 'discount line' should be recorded immediately after the discounted line in the same invoice group.";
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1999", TestObjectCreator.AUD, 1.0M, TestObjectCreator.ABIGAS);
				var line1 = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.CC4, TestObjectCreator.AUD, 1.0M, "hello", -100);
				line1.AL_AT = TestObjectCreator.ServiceTax.PK;
				line1.AL_Sequence = 1;
				var line2 = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.CC4, TestObjectCreator.AUD, 1.0M, "hello", 200);
				line2.AL_AT = TestObjectCreator.ServiceTax.PK;
				line2.AL_Sequence = 2;

				AccountingMasterFilesRegistry.Instance.AlwaysTransmitNegativeChargesAsDiscount.SetValue(invoice.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var invoiceOverride1 = new InvoiceLineOverrideForEditingSequence(line1);
				invoiceOverride1.AL_Sequence = 3;
				AssertHasWarning(invoiceOverride1.AL_SequenceInfo, expectedWarning);

				AccountingMasterFilesRegistry.Instance.AlwaysTransmitNegativeChargesAsDiscount.SetValue(invoice.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				invoiceOverride1 = new InvoiceLineOverrideForEditingSequence(line1);
				invoiceOverride1.AL_Sequence = 3;
				AssertNoWarning(invoiceOverride1.AL_SequenceInfo, expectedWarning);

				AccountingMasterFilesRegistry.Instance.AlwaysTransmitNegativeChargesAsDiscount.SetValue(invoice.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var invoiceOverride2 = new InvoiceLineOverrideForEditingSequence(line2);
				invoiceOverride2.AL_Sequence = 4;
				AssertNoWarning(invoiceOverride2.AL_SequenceInfo, expectedWarning);
			}
		}

		protected override InvoiceLineOverride GetInvoiceOverride(DependentTransactionLine line)
		{
			return new InvoiceLineOverrideForEditingSequence(line);
		}
	}
}
