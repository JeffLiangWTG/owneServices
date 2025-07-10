using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	public class ProfitShareShipmentValidatorTest : TestCaseWithFactory
	{
		public void TestIsChargeValidToBePosted()
		{
			Assert("null charge is not valid", !new ProfitShareShipmentValidator().IsChargeValidToBePosted(null));

			var charge = Factory.NewWithValidTestData<Charge>();
			Assert("PreCond: charge has no error", !charge.HasErrors);
			Assert(new ProfitShareShipmentValidator().IsChargeValidToBePosted(charge));

			charge.JR_AB = ZGuid.BrettsGuid;
			Assert("PreCond: charge has error", charge.HasErrors);
			Assert(!new ProfitShareShipmentValidator().IsChargeValidToBePosted(charge));
		}
	}
}
