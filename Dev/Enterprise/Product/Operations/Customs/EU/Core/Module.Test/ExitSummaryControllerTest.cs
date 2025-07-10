using System;
using System.Reflection;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(ExitSummaryController))]
	sealed class ExitSummaryControllerTest : ZControllerBasherTest
	{
		public void TestPluginTabPageCaption()
		{
			var controller = new ExitSummaryController();
			AssertEquals("Exit Control", controller.PluginTabPageCaption.Caption);
		}

		public void TestGetPlugIn_Default()
		{
			var controller = new ExitSummaryController();
			var delcartion = Factory.New<JobDeclaration>();
			var getPlugin = typeof(ExitSummaryController).GetMethod("GetPlugIn", BindingFlags.Instance | BindingFlags.NonPublic);
			var plugin = getPlugin.Invoke(controller, new object[] { delcartion });
			AssertEquals("PlugIn type", typeof(ExitSummaryPlugIn), plugin.GetType());
			((ExitSummaryPlugIn)plugin).Dispose();
		}

		public void TestGetPlugIn_ExitControlPlugIn()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Ireland))
			{
				var controller = new ExitSummaryController();
				var delcartion = Factory.New<JobDeclaration>();
				var getPlugin = typeof(ExitSummaryController).GetMethod("GetPlugIn", BindingFlags.Instance | BindingFlags.NonPublic);
				var plugin = getPlugin.Invoke(controller, new object[] { delcartion });
				AssertEquals("PlugIn type", "Enterprise.Customs.EU.ExitControl.GUI.PlugIn.ExitControlPlugIn", plugin.GetType().ToString());
				((ZPlugIn)plugin).Dispose();
			}
		}

		public override Type ControllerToBashType => typeof(ExitSummaryController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.ExitSummaryController;
	}
}
