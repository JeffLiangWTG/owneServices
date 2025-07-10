using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DpsMatchingConfigurationBusinessObject))]
	sealed class DpsMatchingConfigurationBusinessObjectTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => new DpsMatchingConfigurationBusinessObject();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		public void TestSchema()
		{
			AssertEquals("MatchingConfiguration", DpsMatchingConfigurationBusinessObject.Schema.MatchingConfiguration);
		}
	}
}
