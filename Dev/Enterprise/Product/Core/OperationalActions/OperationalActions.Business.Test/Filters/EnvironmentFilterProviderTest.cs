using CargoWise.EntityFramework.Testing;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class EnvironmentFilterProviderTest : TestCaseWithFactory
	{
		public void TestInvalidCode()
		{
			AssertNull("should return null if constraint name not known.", EnvironmentFilterProvider.Instance.GetConstraint("XXX"));
		}
	}
}
