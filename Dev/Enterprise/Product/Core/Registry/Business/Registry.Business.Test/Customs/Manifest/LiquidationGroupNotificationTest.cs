using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.Testing
{
	[TestedType(typeof(ManifestGroupNotification))]
	sealed class LiquidationGroupNotificationTest : RegistryBusinessObjectTemplateTestCase<ManifestGroupNotification>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override ManifestGroupNotification GetBusinessObjectToClone()
		{
			return new ManifestGroupNotification();
		}

		protected override ManifestGroupNotification GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
