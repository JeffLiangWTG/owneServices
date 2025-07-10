using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlobalTrackingShipmentVisibilityOptions))]
	sealed class GlobalTrackingShipmentVisibilityOptionsTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new GlobalTrackingShipmentVisibilityOptions();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new GlobalTrackingShipmentVisibilityOptions();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
