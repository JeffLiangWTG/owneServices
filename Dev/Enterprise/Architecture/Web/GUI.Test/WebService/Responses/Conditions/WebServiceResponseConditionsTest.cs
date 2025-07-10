using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	sealed class WebServiceResponseConditionsTest : TestCaseWithFactory
	{
		#region Test Cases

		public void TestConstants()
		{
			AssertEquals("Equal", WebServiceResponseConditions.Equal);
			AssertEquals("NotEqual", WebServiceResponseConditions.NotEqual);
		}

		#endregion
	}
}
