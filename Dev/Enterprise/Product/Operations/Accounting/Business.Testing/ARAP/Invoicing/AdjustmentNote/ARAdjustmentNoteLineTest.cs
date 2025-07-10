using System;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARAdjustmentNoteLine))]
	public class ARAdjustmentNoteLineTest : InvoicingLineBaseTest
	{
		protected override Type MasterHeaderType
		{
			get
			{
				return typeof(ARAdjustmentNote);
			}
		}

		public override void TestAL_JHValidationOnAL_RevRecognitionType()
		{
			Assert("Test is not applicable as revenue recognition validation is not run for this line type", true);
		}

		public override void TestTaxRateOverrideWorksForAPInvoices()
		{
			Assert("Test is not applicable for AR Adjustment note line", true);
		}

		public override void TestSettingJobSetsFallbackTaxRate()
		{
			Assert("Test is not applicable for AR Adjustment note line", true);
		}

		public override void TestSetSupplyTypeTriggerSetTaxRate()
		{
			Assert("Test is not applicable for AR Adjustment note line", true);
		}

		public override void TestSetBranchTriggerSetTaxRate()
		{
			Assert("Test is not applicable for AR Adjustment note line", true);
		}

		public override void TestChargeCodeTaxRateOverrideWithCustomsStatus()
		{
			Assert("Test is not applicable for AR Adjustment note line", true);
		}

		public override void TestChargeCodeTaxRateOverrideAndChargeCodeTaxOverride()
		{
			Assert("Test is not applicable for AR Adjustment note line", true);
		}

		public override void TestChargeCodeTaxRateOverride_PlaceOfSupply()
		{
			Assert("Test is not applicable for AR Adjustment note line", true);
		}

		public override void TestFallbackTaxRate()
		{
			Assert("Test is not applicable for AR Adjustment note line", true);
		}
	}
}
