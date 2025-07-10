using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.OperationalActions.Testing
{
	[TestedType(typeof(CARNSQueryOperationalActionMethod))]
	sealed class CARNSQueryOperationalActionMethodTest : OperationalActionMethodTest<CARNSQueryOperationalActionMethod>
	{
		public void TestProperties()
		{
			var method = new CARNSQueryOperationalActionMethod();
			CombineAssertions(() =>
			{
				AssertEquals("TestMethodDescription", "Send Release Status Query for Import Declarations", method.Description);
				AssertEquals("TestMethodName", "Send Release Status Query", method.Name);
				AssertEquals("TestHasControl", true, method.HasControl);
				AssertEquals("TestHasSettings", false, method.HasSettings);
			});
		}

		protected override CARNSQueryOperationalActionMethod NewMethod() => new CARNSQueryOperationalActionMethod();
	}
}
