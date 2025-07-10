using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SummarizeULDSLACConfigCollection))]
	sealed class SummarizeULDSLACConfigCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<SummarizeULDSLACConfigCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override SummarizeULDSLACConfigCollection GetCollectionToTest()
		{
			return new SummarizeULDSLACConfigCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SummarizeULDSLACConfig();
		}
	}
}
