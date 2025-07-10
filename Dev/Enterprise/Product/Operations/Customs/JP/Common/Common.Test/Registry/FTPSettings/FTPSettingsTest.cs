using System;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(FTPSettings))]
	sealed class FTPSettingsTest : RegistryBusinessObjectTemplateTestCase<FTPSettings>
	{
		public void TestPassive()
		{
			var fTPSettings = new FTPSettings(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);

			CombineAssertions(() =>
			{
				fTPSettings.PassiveNoSelection = true;
				Assert("PassiveYesSelection should be false when PassiveNoSelection is true.", !fTPSettings.PassiveYesSelection);
				Assert("Passive should be false when PassiveNoSelection is true.", !fTPSettings.Passive);

				fTPSettings.PassiveYesSelection = true;
				Assert("PassiveNoSelection should be false when PassiveYesSelection is true.", !fTPSettings.PassiveNoSelection);
				Assert("Passive should be true when PassiveYesSelection is true.", fTPSettings.Passive);
			});
		}

		public void TestIsEmpty()
		{
			var settings = new FTPSettings();
			Assert("Default to true.", settings.IsEmpty);

			settings.Server = "127.0.0.1";
			settings.InFolder = "InTest";
			settings.OutFolder = "OutTest";
			settings.UserName = "Test";
			settings.Password = "2023";

			Assert(!settings.IsEmpty);
		}

		protected override FTPSettings GetBusinessObjectToClone() => (FTPSettings)GetNewBusinessObject();

		protected override FTPSettings GetBusinessObjectToSerialise() => (FTPSettings)GetNewBusinessObject();

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;
	}
}
