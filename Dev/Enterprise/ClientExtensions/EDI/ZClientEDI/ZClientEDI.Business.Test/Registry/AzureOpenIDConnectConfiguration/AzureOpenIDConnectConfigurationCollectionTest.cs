using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business
{
	[TestedType(typeof(AzureOpenIDConnectConfigurationCollection))]
	public class AzureOpenIDConnectConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<AzureOpenIDConnectConfigurationCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override AzureOpenIDConnectConfigurationCollection GetCollectionToTest()
		{
			return new AzureOpenIDConnectConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AzureOpenIDConnectConfiguration();
		}

		public void TestAllowNew()
		{
			Assert(Collection.AllowNew);
		}
	}
}
