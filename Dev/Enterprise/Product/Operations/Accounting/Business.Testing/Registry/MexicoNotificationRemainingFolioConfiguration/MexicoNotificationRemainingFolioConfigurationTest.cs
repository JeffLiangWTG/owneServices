using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(MexicoNotificationRemainingFolioConfiguration))]
	class MexicoNotificationRemainingFolioConfigurationTest : RegistryBusinessObjectTemplateTestCase<MexicoNotificationRemainingFolioConfiguration>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		protected override MexicoNotificationRemainingFolioConfiguration GetBusinessObjectToClone()
		{
			return new MexicoNotificationRemainingFolioConfiguration();
		}

		protected override MexicoNotificationRemainingFolioConfiguration GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
