using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class ImportGroupInvoiceChargeValidationTest : TestCaseWithFactory
	{
		public void TestMessageValidationType()
		{
			GroupInvoiceCharge charge = groupHeader.Charges.AddNew();
			GroupInvoiceChargeValidation validation = new GroupInvoiceChargeValidation(charge);
			AssertEquals("Message validation type with better message for exchange rates", typeof(ExternalMessageValidation), validation.MessageValidation.GetType());
		}

		public void TestValidateDistributeByForEdifice()
		{
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

			AssertEquals("IsImportEdifice", true, testDec.IsImportEdifice);

			GroupInvoiceCharge charge = groupHeader.Charges.AddNew();
			charge.J7_DistributeBy = Customs.Common.ChargeDistributeByList.Codes.Volume;
			AssertHasError(charge.J7_DistributeByInfo, ExternalMessageValidation.EdificeDistributionByOtherThanValueError);
			AssertNoWarning(charge.J7_DistributeByInfo, ExternalMessageValidation.DistributionByOtherThanValueWarningForCMR);

			charge.J7_DistributeBy = Customs.Common.ChargeDistributeByList.Codes.Value;
			AssertNoError(charge.J7_DistributeByInfo, ExternalMessageValidation.EdificeDistributionByOtherThanValueError);
			AssertNoWarning(charge.J7_DistributeByInfo, ExternalMessageValidation.DistributionByOtherThanValueWarningForCMR);

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("IsImportCMR", true, testDec.IsImportCMR);

			charge.J7_DistributeBy = Customs.Common.ChargeDistributeByList.Codes.Volume;
			AssertNoError(charge.J7_DistributeByInfo, ExternalMessageValidation.EdificeDistributionByOtherThanValueError);
			AssertHasWarning(charge.J7_DistributeByInfo, ExternalMessageValidation.DistributionByOtherThanValueWarningForCMR);

			charge.J7_DistributeBy = Customs.Common.ChargeDistributeByList.Codes.Value;
			AssertNoError(charge.J7_DistributeByInfo, ExternalMessageValidation.EdificeDistributionByOtherThanValueError);
			AssertNoWarning(charge.J7_DistributeByInfo, ExternalMessageValidation.DistributionByOtherThanValueWarningForCMR);
		}

		public void TestCheckJ7_Percentage()
		{
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			GroupInvoiceCharge charge1 = groupHeader.Charges.AddNew();
			charge1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge1.J7_Percentage = 11;

			AssertEquals("Default TotalUnknownFreightandInsurancePercentage should be 10", AUCustomsDataRegistry.Instance.TotalUnknownFreightandInsurancePercentage.Value, new Decimal(10));
			AssertHasErrorContaining(charge1.J7_PercentageInfo, "The entered value exceeds the allowable percentage as stored in the Registry Item:  Customs -> Australia -> Import Declaration -> Total Unknown Freight and Insurance Percentage");

			charge1.J7_Percentage = 10;
			AssertNoErrorContaining(charge1.J7_PercentageInfo, "The entered value exceeds the allowable percentage as stored in the Registry Item:  Customs -> Australia -> Import Declaration -> Total Unknown Freight and Insurance Percentage");

			GroupInvoiceCharge charge2 = groupHeader.Charges.AddNew();
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
		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			groupHeader = testDec.JobComInvoiceGroupHeaders[0];
		}

		#endregion
	}
}
