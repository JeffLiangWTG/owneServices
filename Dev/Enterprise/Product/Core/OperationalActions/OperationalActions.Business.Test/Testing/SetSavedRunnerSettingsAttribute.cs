using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class SetSavedRunnerSettingsAttribute : TestSetupAttribute
	{
		public SetSavedRunnerSettingsAttribute(string value)
		{
			this.value = value;
		}

		public override void SetUp(TestCase testCase)
		{
			Env.Registry.SetFilterCriteria(OperationalActionRunner.SettingCacheName, value);
		}

		public override void TearDown(TestCase testCase)
		{
		}

		readonly string value;
	}
}
