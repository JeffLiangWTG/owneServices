using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	[TestedType(typeof(ADPasswordSettingsRegistryBusinessObject))]
	class ADPasswordSettingsRegistryBusinessObjectTest : RegistryBusinessObjectTemplateTestCase<ADPasswordSettingsRegistryBusinessObject>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override ADPasswordSettingsRegistryBusinessObject GetBusinessObjectToClone() => new ADPasswordSettingsRegistryBusinessObject();

		protected override ADPasswordSettingsRegistryBusinessObject GetBusinessObjectToSerialise() => new ADPasswordSettingsRegistryBusinessObject();
	}
}
