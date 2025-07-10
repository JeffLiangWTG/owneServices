using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	[TestedType(typeof(RunProgramActionMethod))]
	sealed class RunProgramActionMethodTest : OperationalActionMethodTest<RunProgramActionMethod>
	{
		public void TestHasSettings()
		{
			Assert(NewMethod().HasSettings);
		}

		public void TestNewSettingsType()
		{
			AssertEquals(typeof(RunProgramActionMethodSettings), NewMethod().NewSetting(Factory).GetType());
		}

		public void TestNewSettingsControlType()
		{
			using (var control = NewMethod().NewSettingsControl())
			{
				AssertEquals(typeof(RunProgramActionMethodSettingsControl), control.GetType());
			}
		}

		public void TestNewApplicatorType()
		{
			var method = NewMethod();
			AssertEquals(typeof(RunProgramActionMethodApplicator), method.NewApplicator(Factory, method.NewSetting(Factory)).GetType());
		}

		public void TestRunWithoutUI()
		{
			Assert(NewMethod().RunWithoutUI);
		}

		public void TestRequiredSecurity()
		{
			AssertContainsExactElementsInAnyOrder(
				"Should require the right 'Allow Run External Program from Operational Actions'",
				(c) => c.DisplayText,
				new SecurityCheckpoint[] { Env.Security.RunExternalProgramFromOperationalActions },
				Method.GetRequiredSecurityCheckpoints());
		}

		#region Implementation

		protected override RunProgramActionMethod NewMethod()
		{
			return new RunProgramActionMethod();
		}

		#endregion
	}
}
