using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.AFR.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Module.Testing
{
	[TestedType(typeof(JPAFRModule))]
	class JPAFRModuleTest : ZModuleBasherTest
	{
		public void TestModuleButtonsAndNewForm()
		{
			using (var module = new JPAFRModuleForTest())
			{
				AssertEquals(7, module.ToolBarButtons.Length);
				var voccButton = module.ToolBarButtons.FirstOrDefault(button => button.Text == "New VOCC") as ZToolBarButton;
				var nvoccButton = module.ToolBarButtons.FirstOrDefault(button => button.Text == "New NVOCC") as ZToolBarButton;
				AssertNotNull(voccButton);
				AssertNotNull(nvoccButton);

				nvoccButton.PerformClick();
				var controller = module.ControllerForTest as JPAFRController;
				AssertNotNull(controller);
				Assert(!controller.CreateVOCCAFR);
				var lastForm = module.ControllerForTest.LastShownForm as JPAFRForm;
				AssertNotNull(lastForm);
				Assert(!lastForm.BusinessEntity.JPH_IsShippingLineEntry);
				module.ControllerForTest.LastShownForm.Dispose();

				voccButton.PerformClick();
				controller = module.ControllerForTest as JPAFRController;
				AssertNotNull(controller);
				Assert(controller.CreateVOCCAFR);
				lastForm = module.ControllerForTest.LastShownForm as JPAFRForm;
				AssertNotNull(lastForm);
				Assert(lastForm.BusinessEntity.JPH_IsShippingLineEntry);
				module.ControllerForTest.LastShownForm.Dispose();
			}
		}

		public void TestAllowNew()
		{
			using (var module = new JPAFRModule())
			{
				Assert(!module.AllowNew);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.JP.AFR;

		protected override bool HasController() => true;

		class JPAFRModuleForTest : JPAFRModule
		{
			protected override ZController GetNewController(BusinessObject selectedBusinessObject)
			{
				ControllerForTest = base.GetNewController(selectedBusinessObject);
				return ControllerForTest;
			}
			internal ZController ControllerForTest;
		}
	}
}
