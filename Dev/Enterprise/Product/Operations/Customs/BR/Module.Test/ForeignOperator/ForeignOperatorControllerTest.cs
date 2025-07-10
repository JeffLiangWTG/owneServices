using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(ForeignOperatorController))]
	class ForeignOperatorControllerTest : ZControllerBasherTest
	{
		public void TestSecurityCheckpoints()
		{
			var controller = new ForeignOperatorController();
			CombineAssertions(() =>
			{
				AssertEquals("For View", Env.Security.BRForeignOperatorView, controller.CheckPointForViewExposedForTest);
				AssertEquals("For Edit", Env.Security.BRForeignOperatorEdit, controller.CheckPointForEditExposedForTest);
				AssertEquals("For New", Env.Security.BRForeignOperatorNew, controller.CheckPointForNewExposedForTest);
				AssertEquals("For Delete", Env.Security.BRForeignOperatorDelete, controller.CheckPointForDeleteExposedForTest);
			});
		}

		public void TestTopLevelBusinessObject()
		{
			var controller = new ForeignOperatorController();
			AssertEquals(typeof(CusBRForeignOperator), controller.TypeOfTopLevelBusinessObject);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.BR.ForeignOperator;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var result = Factory.NewWithValidTestData(GetBusinessObjectType()) as CusBRForeignOperator;
			result.BFR_AuthorityIdentifier = string.Empty;
			result.BFR_AuthorityVersion = string.Empty;
			Factory.Save();
			return result;
		}
	}
}
