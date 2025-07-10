using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(WIPController))]
	public class WIPControllerTest : WIPAccrualControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WIP;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			WIP result = Factory.New<WIP>();
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_ARLine = result.PK;
			result.AL_AC = charge.JR_AC;
			result.AL_OH = charge.JR_OH_SellAccount;
			result.AL_JH = charge.JR_JH;
			result.AL_AG = new TestObjectCreator(Factory).GLHeader1.PK;
			Factory.Save();
			return result;
		}
	}
}
