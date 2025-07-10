using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DefaultPremiseIDCollection))]
	sealed class DefaultPremiseIDCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DefaultPremiseIDCollection>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override DefaultPremiseIDCollection GetCollectionToTest()
		{
			return new DefaultPremiseIDCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DefaultPremiseID();
		}
	}
}
