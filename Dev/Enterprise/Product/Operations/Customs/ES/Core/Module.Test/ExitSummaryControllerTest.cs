using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Registry;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Module.Testing
{
	[TestedType(typeof(ExitSummaryController))]
	sealed class ExitSummaryControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(ExitSummaryController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.ExitSummaryController;

		public void TestGetPlugIn()
		{
			var dummyBusinessObject = Factory.New<DummyBusinessObject>();

			var controller = new ExitSummaryControllerForTest();
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			using (var plugin = controller.GetPluginExposed(dummyBusinessObject))
			{
				AssertEquals("PlugIn Type", "Enterprise.Customs.EU.ExitControl.GUI.PlugIn.ExitControlPlugIn", plugin.GetType().FullName);
			}

			controller = new ExitSummaryControllerForTest();
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			using (var plugin = controller.GetPluginExposed(dummyBusinessObject))
			{
				AssertEquals("When messageVersion AES, PlugIn Type", "Enterprise.Customs.EU.ExitControl.GUI.PlugIn.ExitControlPlugIn", plugin.GetType().FullName);
			}
		}
	}

	sealed class ExitSummaryControllerForTest : ExitSummaryController
	{
		public ZPlugIn GetPluginExposed(IBusiness businessEntity) => GetPlugIn(businessEntity);
	}
}
