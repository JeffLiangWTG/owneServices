using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class InvoiceLineChargeValidationTest : TestCaseWithFactory
	{
		public void TestMessageValidationType()
		{
			InvoiceLineChargeValidation validation = new InvoiceLineChargeValidation(lineCharge);
			AssertEquals("Message validation type", typeof(ExternalMessageValidation), validation.MessageValidation.GetType());
		}

		public void TestCheckJ7_ChargeType()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("IsImport CMR", false, testDec.IsImportCMR);
			lineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals("No error", false, lineCharge.J7_ChargeTypeInfo.HasMessageErrors());

			lineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			AssertEquals("No error", false, lineCharge.J7_ChargeTypeInfo.HasMessageErrors());

			lineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			AssertEquals("Error expected", true, lineCharge.J7_ChargeTypeInfo.HasMessageErrors());

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("IsImportCMR", true, testDec.IsImportCMR);
			lineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			AssertEquals("Error expected", false, lineCharge.J7_ChargeTypeInfo.HasMessageErrors());
		}

		public void TestValidateDistributeByForEdifice()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			lineCharge.J7_DistributeBy = Customs.Common.ChargeDistributeByList.Codes.Volume;
			AssertHasError(lineCharge.J7_DistributeByInfo, ExternalMessageValidation.EdificeDistributionByOtherThanValueError);
			AssertNoWarning(lineCharge.J7_DistributeByInfo, ExternalMessageValidation.DistributionByOtherThanValueWarningForCMR);

			lineCharge.J7_DistributeBy = Customs.Common.ChargeDistributeByList.Codes.Value;
			AssertNoError(lineCharge.J7_DistributeByInfo, ExternalMessageValidation.EdificeDistributionByOtherThanValueError);
			AssertNoWarning(lineCharge.J7_DistributeByInfo, ExternalMessageValidation.DistributionByOtherThanValueWarningForCMR);

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("IsImportCMR", true, testDec.IsImportCMR);

			lineCharge.J7_DistributeBy = Customs.Common.ChargeDistributeByList.Codes.Volume;
			AssertNoError(lineCharge.J7_DistributeByInfo, ExternalMessageValidation.EdificeDistributionByOtherThanValueError);
			AssertHasWarning(lineCharge.J7_DistributeByInfo, ExternalMessageValidation.DistributionByOtherThanValueWarningForCMR);

			lineCharge.J7_DistributeBy = Customs.Common.ChargeDistributeByList.Codes.Value;
			AssertNoError(lineCharge.J7_DistributeByInfo, ExternalMessageValidation.EdificeDistributionByOtherThanValueError);
			AssertNoWarning(lineCharge.J7_DistributeByInfo, ExternalMessageValidation.DistributionByOtherThanValueWarningForCMR);
		}

		public void TestCheckJ7_Percentage()
		{
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var charge1 = invoiceLine.Charges.AddNew();
			charge1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge1.J7_Percentage = 11;

			AssertEquals("Default TotalUnknownFreightandInsurancePercentage should be 10", AUCustomsDataRegistry.Instance.TotalUnknownFreightandInsurancePercentage.Value, new decimal(10));
			AssertHasErrorContaining(charge1.J7_PercentageInfo, "The entered value exceeds the allowable percentage as stored in the Registry Item:  Customs -> Australia -> Import Declaration -> Total Unknown Freight and Insurance Percentage");

			charge1.J7_Percentage = 10;
			AssertNoErrorContaining(charge1.J7_PercentageInfo, "The entered value exceeds the allowable percentage as stored in the Registry Item:  Customs -> Australia -> Import Declaration -> Total Unknown Freight and Insurance Percentage");

			var charge2 = invoiceLine.Charges.AddNew();
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

		JobDeclaration testDec;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		InvoiceLineCharge lineCharge;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoice = testDec.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			lineCharge = invoiceLine.Charges.AddNew();
		}
	}
}
