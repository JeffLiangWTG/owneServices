using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class ExternalMessageValidationTest : Customs.Business.Testing.MandatoryValidationAbstractTest
	{
		public void TestDoesEachWorkingDaysHaveAnExchangeRate()
		{
			ExternalMessageValidationForTesting validation = new ExternalMessageValidationForTesting(Factory.New<JobDeclaration>());
			AssertEquals("DoesEachWorkingDayHaveAnExchangeRate", true, validation.ValidateEachWorkingDayHasAnExchangeRate_Exposed);
		}

		public void TestGetAdviceHowToFixNoValidExchangeRates()
		{
			ExternalMessageValidationForTesting validation = new ExternalMessageValidationForTesting(JobDeclaration.New(Factory));
			ZString advice = validation.GetAdviceHowToFixNoValidExchangeRates_Exposed();
			AssertEquals("Exchange rates -how to fix", true, advice.Contains(ExternalMessageValidation.ManualUpgradeOfReference));
		}

		public void TestValidateDistributeByForEdifice()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

			AssertEquals("IsImportEdifice", true, testDec.IsImportEdifice);

			JobComInvoiceGroupHeader group = testDec.JobComInvoiceGroupHeaders[0];
			GroupInvoiceCharge charge = group.Charges.AddNew();
			charge.J7_DistributeBy = Customs.Common.ChargeDistributeByList.Codes.Volume;
			AssertHasError(charge.J7_DistributeByInfo, ExternalMessageValidation.EdificeDistributionByOtherThanValueError);
			AssertNoWarning(charge.J7_DistributeByInfo, ExternalMessageValidation.DistributionByOtherThanValueWarningForCMR);

			charge.J7_DistributeBy = Customs.Common.ChargeDistributeByList.Codes.Value;
			AssertNoError(charge.J7_DistributeByInfo, ExternalMessageValidation.EdificeDistributionByOtherThanValueError);
			AssertNoWarning(charge.J7_DistributeByInfo, ExternalMessageValidation.DistributionByOtherThanValueWarningForCMR);

			charge.J7_DistributeBy = "";
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

			charge.J7_DistributeBy = "";
			AssertNoError(charge.J7_DistributeByInfo, ExternalMessageValidation.EdificeDistributionByOtherThanValueError);
			AssertNoWarning(charge.J7_DistributeByInfo, ExternalMessageValidation.DistributionByOtherThanValueWarningForCMR);
		}
	}
}
