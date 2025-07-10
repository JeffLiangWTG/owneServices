using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AddressValidationDisabledCountryItemCollection))]
	sealed class AddressValidationDisabledCountryItemCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<AddressValidationDisabledCountryItemCollection>
	{
		public void TestCountryCollection()
		{
			var collection = new AddressValidationDisabledCountryItemCollection();
			AssertNotNull(collection.CountryCollection);
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override AddressValidationDisabledCountryItemCollection GetCollectionToTest()
		{
			return new AddressValidationDisabledCountryItemCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AddressValidationDisabledCountryItem();
		}

		#endregion
	}
}
