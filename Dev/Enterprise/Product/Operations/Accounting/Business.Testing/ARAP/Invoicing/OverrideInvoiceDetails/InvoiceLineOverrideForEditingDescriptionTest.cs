using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoiceLineOverrideForEditingDescription))]
	public class InvoiceLineOverrideForEditingDescriptionTest : InvoiceLineOverrideTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new InvoiceLineOverrideForEditingDescription(Factory.NewWithValidTestData<ARCreditNoteLine>());
		}

		TestObjectCreator testObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}

		public void TestPassThroughProperties_Description()
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
			Factory.Save();

			var invoiceOverride = new InvoiceLineOverrideForEditingDescription(line);

			AssertEquals("123", invoiceOverride.AL_Desc);
			AssertEquals("123", invoiceOverride.OriginalDescription);

			line.AL_Desc = "Something else";

			AssertEquals("Something else", invoiceOverride.AL_Desc);
			AssertEquals("123", invoiceOverride.OriginalDescription);
		}

		public void TestSetAL_DescValidation_SisterCompany()
		{
			var alternateBranch = TestObjectCreator.CreateBranch("TMP", TestObjectCreator.CreateNewCompany("TMP"));
			Factory.Save();

			AccountingConfigurationRegistry.Instance.AllowChargeDescriptionOverrideOnPostedARInvoice.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestObjectCreator.CC1.PK.ToString());

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 100, 10, 0);
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_Desc = "Original";
			Factory.Save();
			var invoiceOverride = new InvoiceLineOverrideForEditingDescription(line);
			invoiceOverride.AL_Desc = "Hello Again";
			AssertNoError(invoiceOverride.AL_DescInfo, @"Overriding the Transaction Description is not allowed on lines belonging to a sister company invoice.");
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, alternateBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				invoiceOverride.AL_Desc = "Hello Again 2";
				AssertHasError(invoiceOverride.AL_DescInfo, @"Overriding the Transaction Description is not allowed on lines belonging to a sister company invoice.");
			}
		}

		public void TestSetAL_DescValidation_Registry()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 100, 10, 0);
			line.AL_AC = normalChargeCodeLinked.PK;
			line.AL_Desc = "Original";
			Factory.Save();
			var invoiceOverride = new InvoiceLineOverrideForEditingDescription(line);

			// Tests with nothing set up in registry
			AssertNoError(invoiceOverride.AL_DescInfo, @"The charge code 'CC2' is not in the list of allowed charge codes for overriding the line description. You can configure the list by changing the registry setting 'Allow Description Override on AR Invoice Lines'");
			invoiceOverride.AL_Desc = "Hello";
			AssertHasError(invoiceOverride.AL_DescInfo, @"The charge code 'CC2' is not in the list of allowed charge codes for overriding the line description. You can configure the list by changing the registry setting 'Allow Description Override on AR Invoice Lines'");

			// Tests with local charge code in registry
			AccountingConfigurationRegistry.Instance.AllowChargeDescriptionOverrideOnPostedARInvoice.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, normalChargeCodeLinked.PK.ToString());

			invoiceOverride.AL_Desc = "Hello Again";
			AssertNoError(invoiceOverride.AL_DescInfo, @"The charge code 'CC2' is not in the list of allowed charge codes for overriding the line description. You can configure the list by changing the registry setting 'Allow Description Override on AR Invoice Lines'");
			line.AL_AC = TestObjectCreator.CC3.PK;
			Factory.Save();

			invoiceOverride.AL_Desc = "Hello Again 2";
			AssertHasError(invoiceOverride.AL_DescInfo, @"The charge code 'ZZCC3' is not in the list of allowed charge codes for overriding the line description. You can configure the list by changing the registry setting 'Allow Description Override on AR Invoice Lines'");
			line.AL_AC = normalChargeCodeLinked.PK;
			Factory.Save();

			// Tests with global charge code in registry
			AccountingConfigurationRegistry.Instance.AllowChargeDescriptionOverrideOnPostedARInvoice.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToString());

			invoiceOverride.AL_Desc = "Hello Again 3";
			AssertNoError(invoiceOverride.AL_DescInfo, @"The charge code 'CC2' is not in the list of allowed charge codes for overriding the line description. You can configure the list by changing the registry setting 'Allow Description Override on AR Invoice Lines'");
		}

		public void TestSetAL_Desc()
		{
			// No charge
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 100, 10, 0);
			var invoiceOverride = new InvoiceLineOverrideForEditingDescription(line);
			invoiceOverride.AL_Desc = "Hello";

			AssertEquals("Description successfully updated in case where there is no charge attached", "Hello", line.AL_Desc);
			AssertEquals("Description successfully updated in case where there is no charge attached", "Hello", invoiceOverride.AL_Desc);

			// Posted charge
			var job = TestObjectCreator.CreateJob("Job1", TestObjectCreator.LocalClient, 1, TestObjectCreator.Agent, 1);
			job.JH_JobLocalReference = "123";
			line.AL_JH = job.PK;
			line.AL_AC = TestObjectCreator.CC1.PK;
			var charge = TestObjectCreator.CreateCharge(line);
			Factory.Save();

			invoiceOverride.AL_Desc = "Goodbye";

			AssertEquals("Description successfully updated in case where there is a charge attached", "Goodbye", line.AL_Desc);
			AssertEquals("Description successfully updated in case where there is a charge attached", "Goodbye", invoiceOverride.AL_Desc);
			AssertEquals("From the charge point of view we keep the description in sync with the AR line. So it should be updated too.", "Goodbye", charge.JR_Desc);

			// Doesn't set charge if AP
			var invoiceAP = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1);
			var lineAP = TestObjectCreator.CreateInvoiceLine(invoiceAP, TestObjectCreator.AUD, 1, 100, 10, 0);
			lineAP.AL_JH = job.PK;
			lineAP.AL_AC = TestObjectCreator.CC1.PK;
			lineAP.AL_Desc = "Don't change me";
			var chargeForAP = TestObjectCreator.CreateCharge(lineAP);
			chargeForAP.JR_Desc = "Don't change me";
			Factory.Save();
			var invoiceOverrideAP = new InvoiceLineOverrideForEditingDescription(lineAP);

			invoiceOverrideAP.AL_Desc = "Hello Again";

			AssertEquals("In the AP case the line description should be set (when editing an AP invoice for example)", "Hello Again", lineAP.AL_Desc);
			AssertEquals("In the AP case the line description should be set (when editing an AP invoice for example)", "Hello Again", invoiceOverrideAP.AL_Desc);
			AssertEquals("From the charge point of view we keep the description in sync with the AR line. Not the AP line.", "Don't change me", chargeForAP.JR_Desc);
		}

		public void TestAL_Desc_ReadOnly()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 100, 10, 0);
			var invoiceOverride = new InvoiceLineOverrideForEditingDescription(line);
			AssertEquals("AL_Desc is not readonly.", false, invoiceOverride.AL_DescInfo.ReadOnly);
		}

		protected override InvoiceLineOverride GetInvoiceOverride(DependentTransactionLine line)
		{
			return new InvoiceLineOverrideForEditingDescription(line);
		}
	}
}
