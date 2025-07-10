using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AutomaticContainerCreation))]
	sealed class AutomaticContainerCreationTest : RegistryBusinessObjectTemplateTestCase<AutomaticContainerCreation>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override AutomaticContainerCreation GetBusinessObjectToClone()
		{
			return new AutomaticContainerCreation();
		}

		protected override AutomaticContainerCreation GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
