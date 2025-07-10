using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.ChiefExportConsolIntegration.Testing
{
	[TestedType(typeof(ChiefExportConsolIntegrationController))]
	class ChiefExportConsolIntegrationControllerBasherTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType
		{
			get { return typeof(ChiefExportConsolIntegrationController); }
		}

		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.UnitedKingdom; }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.GB.ChiefExportConsolIntegrationController;
		}

		public void TestPluginTabPageCaption()
		{
			AssertEquals("Customs", Controller.PluginTabPageCaption.Caption);
		}
	}
}
