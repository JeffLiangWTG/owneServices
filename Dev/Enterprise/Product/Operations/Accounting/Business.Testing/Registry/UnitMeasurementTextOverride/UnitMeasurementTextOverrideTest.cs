using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(UnitMeasurementTextOverride))]
	public class UnitMeasurementTextOverrideTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new UnitMeasurementTextOverride();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new UnitMeasurementTextOverride();
		}
	}
}
