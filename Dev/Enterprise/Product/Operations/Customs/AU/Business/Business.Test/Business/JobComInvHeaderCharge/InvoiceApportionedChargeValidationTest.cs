using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class InvoiceApportionedChargeValidationTest : TestCaseWithFactory
	{
		public void TestCheckJ7_Percentage()
		{
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var charge1 = invoice.GroupCharges.AddNew();
			charge1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge1.J7_Percentage = 11;

			AssertEquals("Default TotalUnknownFreightandInsurancePercentage should be 10", AUCustomsDataRegistry.Instance.TotalUnknownFreightandInsurancePercentage.Value, new Decimal(10));
			AssertHasErrorContaining(charge1.J7_PercentageInfo, "The entered value exceeds the allowable percentage as stored in the Registry Item:  Customs -> Australia -> Import Declaration -> Total Unknown Freight and Insurance Percentage");

			charge1.J7_Percentage = 10;
			AssertNoErrorContaining(charge1.J7_PercentageInfo, "The entered value exceeds the allowable percentage as stored in the Registry Item:  Customs -> Australia -> Import Declaration -> Total Unknown Freight and Insurance Percentage");

			var charge2 = invoice.GroupCharges.AddNew();
			charge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			charge2.J7_Percentage = 1;
			charge1.Validation.ValidateAll();
			AssertNoErrorContaining(charge1.J7_PercentageInfo, "The entered value exceeds the allowable percentage as stored in the Registry Item:  Customs -> Australia -> Import Declaration -> Total Unknown Freight and Insurance Percentage");
			AssertNoErrorContaining(charge2.J7_PercentageInfo, "The entered value exceeds the allowable percentage as stored in the Registry Item:  Customs -> Australia -> Import Declaration -> Total Unknown Freight and Insurance Percentage");

			var errorMsg = string.Format("Total combined percentages of OFT and ONS must be no greater than {0} percentage.", AUCustomsDataRegistry.Instance.TotalUnknownFreightandInsurancePercentage.Value);

			AssertHasMessageErrorContaining(charge1.J7_PercentageInfo, errorMsg);
			AssertHasMessageErrorContaining(charge2.J7_PercentageInfo, errorMsg);

			charge1.J7_Percentage = 9;
			charge2.Validation.ValidateAll();
			AssertNoMessageErrorContaining(charge1.J7_PercentageInfo, errorMsg);
			AssertNoMessageErrorContaining(charge2.J7_PercentageInfo, errorMsg);

			AUCustomsDataRegistry.Instance.TotalUnknownFreightandInsurancePercentage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			charge1.J7_Percentage = 15;
			AssertNoErrorContaining(charge1.J7_PercentageInfo, "The entered value exceeds the allowable percentage as stored in the Registry Item:  Customs -> Australia -> Import Declaration -> Total Unknown Freight and Insurance Percentage");
			AssertNoMessageErrorContaining(charge1.J7_PercentageInfo, errorMsg);
		}

		#region Implementation

		JobDeclaration testDec;
		JobComInvoiceGroupHeader groupHeader;
		JobComInvoiceHeader invoice;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			invoice = groupHeader.JobComInvoiceHeaders.AddNew();
		}

		#endregion

	}
}
