using System;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Core.Environment.Testing
{
	sealed class ZACustomsRegistryTest : TransactionedTestCase
	{
		public void TestIsTestMode()
		{
			Registry.RawRegistry.ZAIsTestMode.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("IsTestMode", true, Registry.ZACustoms.GetIsTestMode(EnvProxy.Instance.CurrentBranch));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Registry = new DataRegistry();
		}

		DataRegistry Registry;

		#endregion
	}
}
