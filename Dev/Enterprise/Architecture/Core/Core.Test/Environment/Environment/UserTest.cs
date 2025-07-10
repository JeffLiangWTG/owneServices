using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class UserTest : TransactionedTestCase
	{
		public void TestIsBatchProcessorOverride()
		{
			using (new User.IsBatchProcessorOverride(EnvProxy.Instance.CurrentUser))
			{
				Assert(EnvProxy.Instance.CurrentUser.IsBatchProcessor);
			}

			Assert(!EnvProxy.Instance.CurrentUser.IsBatchProcessor);
		}

		public void TestUserInitialsAndDateTime()
		{
			AssertNotNull("UserInitialsAndDateTime", EnvProxy.Instance.CurrentUser.InitialsAndDateTime);
		}

		public void TestIsUserCanLogin()
		{
			Assert(EnvProxy.Instance.CurrentUser.CanLogin);
		}

		public void TestLoadUserFromStaffPKDoesntLoadFactoryIfGivenCurrentUserPK()
		{
			User.Factory = null;
			User.LoadUserFromStaffPK(EnvProxy.Instance.CurrentUser.PK);
			AssertNull("Shouldn't have initialised a factory.", User.Factory);
		}

		public void TestGetOrCreateFactory()
		{
			User.Factory = null;

			User.LoadUserFromStaffPK(Guid.Empty);
			var existingFactory = User.Factory;
			AssertNotNull("Should have created a Factory", User.Factory);

			User.LoadUserFromStaffPK(Guid.Empty);
			AssertEquals("Should have reused existing Factory", existingFactory, User.Factory);
		}

		public void TestDisposeFactorySetsFactoryToNull()
		{
			User.Factory = new BusinessObjectFactory();
			AssertNotNull("Should have set User.Factory.", User.Factory);
			User.DisposeFactory();
			AssertNull("Should have set User.Factory to null.", User.Factory);
		}
	}
}
