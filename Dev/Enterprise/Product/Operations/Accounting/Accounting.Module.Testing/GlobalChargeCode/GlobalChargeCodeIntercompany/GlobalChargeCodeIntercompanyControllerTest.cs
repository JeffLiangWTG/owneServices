using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GlobalChargeCodeIntercompanyController))]
	public class GlobalChargeCodeIntercompanyControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlobalChargeCodeIntercompany;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			Factory.Save();
			return globalChargeCode;
		}
	}
}
