using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARCreditNoteLine))]
	public class ARCreditNoteLineTest : InvoicingLineBaseTest
	{
		public override void TestAL_JHValidationOnAL_RevRecognitionType()
		{
			Assert("Test is not applicable as revenue recognition validation is not run for this line type", true);
		}

		public override void TestIsStampDutyChargeLine()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Italy);

			var taxRate = AccTaxRate.FindExistingTaxRate(Factory, "ART7", AccTaxRate.Types.Rated, Core.Constants.CountryCodes.Italy);

			var taxRateExempt = AccTaxRate.FindExistingTaxRate(Factory, "EXEMPT", AccTaxRate.Types.Exempt, Core.Constants.CountryCodes.Italy);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.TaxIDsAttractingStampDuty.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, taxRate.PK.ToString());
			AccountingConfigurationRegistry.Instance.StampDutyFixedAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 1.81m);
			AccountingConfigurationRegistry.Instance.StampDutyThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 77.47m);

			var stampDutyChargeCode = TestObjectCreator.CreateChargeCode("BOLLO", "Stamp Duty", "MRG", 1m, taxRateExempt, TestObjectCreator.WHTFREE1);
			AccountingConfigurationRegistry.Instance.StampDutyChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, stampDutyChargeCode.PK.ToGuid());

			var italyOrg = TestObjectCreator.CreateOrgHeader("ITORG", false, true, "ITROM");

			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote.AH_OH = italyOrg.PK;
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(creditNote, TestObjectCreator.EUR, 1M, 100M, 0M, 0M, 100M, 0M, 0M, stampDutyChargeCode.PK);
			invoiceLine.AL_AT = taxRate.PK;

			AssertEquals(1, creditNote.Lines.Count);
			AssertEquals("IsStampDutyChargeLine flag should be false for the this line", false, creditNote.Lines[0].IsStampDutyChargeLine());

			Assert("Precondition: Italian stamp duty should be applicable", creditNote.ShouldAddStampDuty());

			Factory.Save();

			AssertEquals("stamp duty line created.", 2, creditNote.Lines.Count);

			AssertEquals("IsStampDutyChargeLine flag should remain false for the first line", false, creditNote.Lines[0].IsStampDutyChargeLine());
			AssertEquals("IsStampDutyChargeLine flag should be set to true for the stamp duty line", true, creditNote.Lines[1].IsStampDutyChargeLine());
		}

		public void TestPropertiesReadOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var relatedTransaction = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
				var line = creditNote.Lines.AddNew() as ARCreditNoteLine;
				creditNote.OriginalTransactionReference = relatedTransaction.PK;
				Assert(line.GenericCharge_ReadOnly);
				Assert(line.AL_JH_ReadOnly_ForTestOnly);
				Assert(line.AL_AT_ReadOnly_ForTestOnly);

				var newFactory = new BusinessObjectFactory();
				var creditNote1 = newFactory.NewWithValidTestData<ARCreditNote>();
				var line1 = creditNote.Lines.AddNew() as ARCreditNoteLine;
				creditNote.AH_TransactionBelongsToGroup = relatedTransaction.PK;
				Assert(line1.GenericCharge_ReadOnly);
				Assert(line1.AL_JH_ReadOnly_ForTestOnly);
				Assert(line1.AL_AT_ReadOnly_ForTestOnly);
			}
		}

		public override void TestTaxRateOverrideWorksForAPInvoices()
		{
			Assert("Test is not applicable for job related AR invoices because tax calculation and invoice posting is done from Job's Billing form", true);
		}

		public override void TestSettingJobSetsFallbackTaxRate()
		{
			Assert("Test is not applicable for job related AR invoices because tax calculation and invoice posting is done from Job's Billing form", true);
		}

		public override void TestSetSupplyTypeTriggerSetTaxRate()
		{
			Assert("Test is not applicable for job related AR invoices because tax calculation and invoice posting is done from Job's Billing form", true);
		}

		public override void TestSetBranchTriggerSetTaxRate()
		{
			Assert("Test is not applicable for job related AR invoices because tax calculation and invoice posting is done from Job's Billing form", true);
		}

		public override void TestChargeCodeTaxRateOverrideWithCustomsStatus()
		{
			Assert("Test is not applicable for job related AR invoices because tax calculation and invoice posting is done from Job's Billing form", true);
		}

		public override void TestChargeCodeTaxRateOverrideAndChargeCodeTaxOverride()
		{
			Assert("Test is not applicable for job related AR invoices because tax calculation and invoice posting is done from Job's Billing form", true);
		}

		public override void TestChargeCodeTaxRateOverride_PlaceOfSupply()
		{
			Assert("Test is not applicable for job related AR invoices because tax calculation and invoice posting is done from Job's Billing form", true);
		}

		public override void TestFallbackTaxRate()
		{
			Assert("Test is not applicable for job related AR invoices because tax calculation and invoice posting is done from Job's Billing form", true);
		}

		protected override Type MasterHeaderType
		{
			get { return typeof(ARCreditNote); }
		}

		[ExpectNoExceptions]
		public void TestCopyFromInvoiceLine()
		{
			ARInvoiceLine aRInvoiceLine = Factory.NewWithValidTestData<ARInvoiceLine>();
			ARCreditNoteLine aRCreditNoteLine = Factory.NewWithValidTestData<ARCreditNoteLine>();
			aRCreditNoteLine.CopyFromInvoiceLine(aRInvoiceLine);
		}
	}
}
