using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.OperationalActions.Testing
{
	[TestedType(typeof(CAB3OperationalActionMethod))]
	sealed class CAB3OperationalActionMethodTest : OperationalActionMethodTest<CAB3OperationalActionMethod>
	{
		public void TestProperties()
		{
			var method = new CAB3OperationalActionMethod();
			CombineAssertions(() =>
			{
				AssertEquals("TestMethodName", "Send CAD Message for Import Declarations", method.Description);
				AssertEquals("TestMethodDescription", "Send CAD Message", method.Name);
				AssertEquals("TestHasControl", true, method.HasControl);
				AssertEquals("TestHasSettings", false, method.HasSettings);
				using (var control = method.NewGuiControl())
				{
					AssertEquals(typeof(CAB3OperationalActionControl), control.GetType());
				}
			});
		}

		protected override CAB3OperationalActionMethod NewMethod() => new CAB3OperationalActionMethod();
	}
}
