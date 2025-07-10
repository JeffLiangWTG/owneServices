using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class AECustomsRegistryTest : TransactionedTestCase
	{
		public void TestCourierID()
		{
			Registry.RawRegistry.AECourierID.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "123456");
			AssertEquals("CourierID", "123456", Registry.AECustoms.CourierID);
			AssertExceptionThrown<RegistryValidationException>("Exception expected", () => Registry.RawRegistry.AECourierID.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "1234567"));
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
