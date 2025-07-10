using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Module.Testing
{
	[TestedType(typeof(EDICommunicationsModeOperationalActionMethod))]
	sealed class EDICommunicationsModeOperationalActionMethodTest : OperationalActionMethodTest<EDICommunicationsModeOperationalActionMethod>
	{
		public void TestProperties()
		{
			var method = new EDICommunicationsModeOperationalActionMethod();
			CombineAssertions(() =>
			{
				AssertEquals("TestMethodName", "Update EDI Client Details", method.Name);
				AssertEquals("TestMethodDescription", "Update EDI Client Details for EDI Communication Modes", method.Description);
				AssertEquals("TestHasControl", false, method.HasControl);
				AssertEquals("TestHasSettings", false, method.HasSettings);
			});
		}

		protected override EDICommunicationsModeOperationalActionMethod NewMethod() => new();
	}
}
