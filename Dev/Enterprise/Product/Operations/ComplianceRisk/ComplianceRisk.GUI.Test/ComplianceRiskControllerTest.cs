using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business.Test;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	[TestedType(typeof(ComplianceRiskController))]
	public class ComplianceRiskControllerTest : ZControllerBasherTest
	{
		public void TestGetPlugIn()
		{
			using (var plugIn = new ComplianceRiskControllerForTest().GetPlugInForTest(new DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(Factory)))
			{
				AssertType<ComplianceRiskPlugIn>(plugIn);
			}
		}

		public void TestCheckPoints()
		{
			var controller = new ComplianceRiskControllerForTest();

			AssertEquals(Env.Security.None, controller.CheckPointForViewForTest);
			AssertEquals(Env.Security.None, controller.CheckPointForNewForTest);
			AssertEquals(Env.Security.None, controller.CheckPointForEditForTest);
			AssertEquals(Env.Security.None, controller.CheckPointForDeleteForTest);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.ComplianceRiskPlugin;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase() => new DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(Factory);
	}

	class ComplianceRiskControllerForTest : ComplianceRiskController
	{
		public ZPlugIn GetPlugInForTest(IBusiness businessEntity) => GetPlugIn(businessEntity);

		public SecurityCheckpoint CheckPointForViewForTest => CheckPointForView;

		public SecurityCheckpoint CheckPointForNewForTest => CheckPointForNew;

		public SecurityCheckpoint CheckPointForEditForTest => CheckPointForEdit;

		public SecurityCheckpoint CheckPointForDeleteForTest => CheckPointForDelete;
	}
}
