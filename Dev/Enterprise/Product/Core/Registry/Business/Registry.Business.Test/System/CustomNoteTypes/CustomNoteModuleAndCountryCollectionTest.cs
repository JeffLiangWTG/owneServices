using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CustomNoteModuleAndCountryCollection))]
	sealed class CustomNoteModuleAndCountryCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CustomNoteModuleAndCountryCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CustomNoteModuleAndCountryCollection GetCollectionToTest()
		{
			return new CustomNoteModuleAndCountryCollection(null);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CustomNoteModuleAndCountry();
		}
	}
}
