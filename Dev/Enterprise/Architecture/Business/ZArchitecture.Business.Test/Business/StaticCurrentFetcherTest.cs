using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class StaticCurrentFetcherTest : TestCase
	{
		public void TestLoggedInUserCode()
		{
			AssertEquals("Correct user code", StaticCurrentFetcher.Instance.CurrentUser.GS_Code, StaticCurrentFetcher.Instance.CurrentUserCode);

			using (EnvProxy.Instance.SetTemporaryUserContext(null))
			{
				AssertEquals("Empty user code", "", StaticCurrentFetcher.Instance.CurrentUserCode);
			}
		}

		public void TestCurrentCompany_DoesntReturnNull()
		{
			AssertNotNull(StaticCurrentFetcher.Instance.CurrentCompany);
		}

		public void TestCurrentBranch_DoesntReturnNull()
		{
			AssertNotNull(StaticCurrentFetcher.Instance.CurrentBranch);
		}

		public void TestCurrentUser_DoesntReturnNull()
		{
			AssertNotNull(StaticCurrentFetcher.Instance.CurrentUser);
		}
	}
}
