using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AddressMatchingLevelBusinessObject))]
	sealed class AddressMatchingLevelBusinessObjectTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => new AddressMatchingLevelBusinessObject();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		public void TestSchema()
		{
			AssertEquals("MatchingLevel", AddressMatchingLevelBusinessObject.Schema.MatchingLevel);
		}
	}
}
