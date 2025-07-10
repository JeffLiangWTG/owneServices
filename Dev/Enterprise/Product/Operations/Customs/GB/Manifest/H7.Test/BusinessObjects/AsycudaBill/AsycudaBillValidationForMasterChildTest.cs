using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	sealed class AsycudaBillValidationForMasterChildTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_RL_NKPortOfDischarge()
		{
			var bill = Factory.New<AsycudaBill>();
			var validation = new AsycudaBillValidationForMasterChild(bill);

			CombineAssertions(() =>
			{
				bill.ABL_RL_NKPortOfDischarge = "";
				validation.ValidateABL_RL_NKPortOfDischarge();
				AssertHasMessageErrorContaining("Discharge Port is empty", bill.ABL_RL_NKPortOfDischargeInfo, "You have not entered");

				bill.ABL_RL_NKPortOfDischarge = "AUSYD";
				validation.ValidateABL_RL_NKPortOfDischarge();
				AssertHasMessageError("Discharge Port does not start with GB", bill.ABL_RL_NKPortOfDischargeInfo, "The port code entered is a non GB port code. A GB port code is required to send a valid declaration.");

				bill.ABL_RL_NKPortOfDischarge = "GBLHR";
				validation.ValidateABL_RL_NKPortOfDischarge();
				AssertNoMessageErrors("Discharge Port starts with GB", bill.ABL_RL_NKPortOfDischargeInfo);
			});
		}
	}
}
