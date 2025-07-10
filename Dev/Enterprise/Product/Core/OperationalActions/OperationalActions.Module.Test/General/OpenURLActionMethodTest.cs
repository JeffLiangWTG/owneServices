using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	[TestedType(typeof(OpenURLActionMethod))]
	sealed class OpenURLActionMethodTest : OperationalActionMethodTest<OpenURLActionMethod>
	{
		public void TestHasSettings()
		{
			Assert(NewMethod().HasSettings);
		}

		public void TestNewSettingsType()
		{
			AssertEquals(typeof(OpenURLActionMethodSettings), NewMethod().NewSetting(Factory).GetType());
		}

		public void TestNewSettingsControlType()
		{
			using (var control = NewMethod().NewSettingsControl())
			{
				AssertEquals(typeof(OpenURLActionMethodSettingsControl), control.GetType());
			}
		}

		public void TestNewApplicatorType()
		{
			var method = NewMethod();
			AssertEquals(typeof(OpenURLActionMethodApplicator), method.NewApplicator(Factory, method.NewSetting(Factory)).GetType());
		}

		public void TestRunWithoutUI()
		{
			Assert(NewMethod().RunWithoutUI);
		}

		#region Implementation

		protected override OpenURLActionMethod NewMethod()
		{
			return new OpenURLActionMethod();
		}

		#endregion
	}
}
