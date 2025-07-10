using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	public class MessageHostedServiceRequirementTest : TestCaseWithFactory
	{
		public void TestCheckMXManifestEnabled_True()
		{
			using (MXCustomsDataRegistry.Instance.EnableMXManifests.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(ZString.Empty, MessageHostedServiceRequirement.CheckMXManifestEnabled());
			}
		}

		public void TestCheckMXmanifestEnabled_False()
		{
			using (MXCustomsDataRegistry.Instance.EnableMXManifests.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("MX Manifest is not enable for any Company", MessageHostedServiceRequirement.CheckMXManifestEnabled());
			}
		}
	}
}
