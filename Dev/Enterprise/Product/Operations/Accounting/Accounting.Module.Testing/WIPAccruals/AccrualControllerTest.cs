using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccrualController))]
	public class AccrualControllerTest : WIPAccrualControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Accrual;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			Accrual result = Factory.New<Accrual>();
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_APLine = result.PK;
			result.AL_AC = charge.JR_AC;
			result.AL_OH = charge.JR_OH_SellAccount;
			result.AL_JH = charge.JR_JH;
			result.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Factory.Save();
			return result;
		}
	}
}
