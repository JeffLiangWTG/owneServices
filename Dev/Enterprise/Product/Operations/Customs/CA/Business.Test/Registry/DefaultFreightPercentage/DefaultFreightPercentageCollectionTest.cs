using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Registry.Testing
{
	[TestedType(typeof(DefaultFreightPercentageCollection))]
	sealed class DefaultFreightPercentageCollectionTest : Enterprise.Registry.Business.Testing.RegistryBusinessObjectCollectionTemplateTestCase<DefaultFreightPercentageCollection>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override DefaultFreightPercentageCollection GetCollectionToTest()
		{
			return new DefaultFreightPercentageCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DefaultFreightPercentage();
		}
	}
}
