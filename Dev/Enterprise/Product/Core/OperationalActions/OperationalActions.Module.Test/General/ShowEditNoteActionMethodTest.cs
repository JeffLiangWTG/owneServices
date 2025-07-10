using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	[TestedType(typeof(ShowEditNoteActionMethod))]
	sealed class ShowEditNoteActionMethodTest : OperationalActionMethodTest<ShowEditNoteActionMethod>
	{
		public void TestHasSettings()
		{
			Assert(NewMethod().HasSettings);
		}

		public void TestNewSettingsType()
		{
			AssertEquals(typeof(ShowEditNoteActionMethodSettings), NewMethod().NewSetting(Factory).GetType());
		}

		public void TestNewSettingsControlType()
		{
			using (var control = NewMethod().NewSettingsControl())
			{
				AssertEquals(typeof(ShowEditNoteActionMethodSettingsControl), control.GetType());
			}
		}

		public void TestNewApplicatorType()
		{
			var method = NewMethod();
			AssertEquals(typeof(ShowEditNoteActionMethodApplicator), method.NewApplicator(Factory, method.NewSetting(Factory)).GetType());
		}

		public void TestRunWithoutUI()
		{
			Assert(NewMethod().RunWithoutUI);
		}

		#region Implementation

		protected override ShowEditNoteActionMethod NewMethod()
		{
			return new ShowEditNoteActionMethod();
		}

		#endregion
	}
}
