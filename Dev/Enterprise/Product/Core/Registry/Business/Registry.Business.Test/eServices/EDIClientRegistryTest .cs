using System;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.eServices.Testing
{
	[TestedType(typeof(EDIClientRegistry))]
	sealed class EDIClientRegistryTest : RegistryItemSetTestCaseWithFactory<EDIClientRegistry>
	{
		public void TesteServiceRegistry_ClientCertificateExpiryNotificationGroup()
		{
			EDIClientRegistry.Instance.ClientCertificateExpiryNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Parse("e9ac15d3-d9fb-4526-bcd0-b5be24741d64"));
			AssertEquals("e9ac15d3-d9fb-4526-bcd0-b5be24741d64", EDIClientRegistry.Instance.ClientCertificateExpiryNotificationGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString());
		}

		public void TesteServiceRegistry_ClientCertificateExpiryNotificationDuration()
		{
			AssertEquals("Default value should be 30.", 30, EDIClientRegistry.Instance.ClientCertificateExpiryNotificationDuration.DefaultValue);
			EDIClientRegistry.Instance.ClientCertificateExpiryNotificationDuration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 15);
			AssertEquals(15, EDIClientRegistry.Instance.ClientCertificateExpiryNotificationDuration.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}
	}
}
