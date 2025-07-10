using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DepotAddressColorSoundCollection))]
	sealed class TestDepotAddressColorSoundCollection : RegistryBusinessObjectCollectionTemplateTestCase<DepotAddressColorSoundCollection>
	{
		protected override DepotAddressColorSoundCollection GetCollectionToTest()
		{
			return new DepotAddressColorSoundCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DepotAddressColorSound(Factory);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
