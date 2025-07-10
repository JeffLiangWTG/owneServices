using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Module.Testing
{
	[TestedType(typeof(CustomsAndExciseReportsModule))]
	class CustomsAndExciseReportsModuleTest : ZModuleBasherTest
	{
		public void TestFilterBusinessObject()
		{
			using (var module = new CustomsAndExciseReportsModule())
			{
				AssertType<CustomsAndExciseReportsFilterStripBusinessObject>(module.FilterBusinessObject);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.IE.CustomsAndExciseReports;

		protected override string CountryCode => Core.Constants.CountryCodes.Ireland;

		protected override bool HasController() => true;
	}
}
