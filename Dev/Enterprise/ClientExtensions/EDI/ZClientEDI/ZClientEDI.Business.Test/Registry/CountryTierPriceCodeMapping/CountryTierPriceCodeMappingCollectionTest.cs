using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(CountryTierPriceCodeMappingCollection))]
	internal class CountryTierPriceCodeMappingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CountryTierPriceCodeMappingCollection>
	{
		public void TestValidation_DuplicateMappings()
		{
			var collection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = collection.AddNew();
			mapping1.PriceCode = "P01";
			mapping1.SystemCode = "X01";
			AssertNoErrors("Precondition", mapping1);
			AssertNoRowErrors("Precondition", mapping1);

			var mapping2 = collection.AddNew();
			mapping2.PriceCode = "P01";
			mapping2.SystemCode = "X01";
			collection.RunPreSaveValidation();
			AssertHasRowError(mapping2, "Duplicate Price Code + System combinations are not permitted.");

			mapping2.SystemCode = "X02";
			collection.RunPreSaveValidation();
			AssertNoRowErrors(mapping2);

			mapping2.SystemCode = "X01";
			collection.RunPreSaveValidation();
			AssertHasRowError(mapping2, "Duplicate Price Code + System combinations are not permitted.");
			mapping2.PriceCode = "P02";
			collection.RunPreSaveValidation();
			AssertNoRowErrors(mapping2);
		}

		#region Overrides

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override CountryTierPriceCodeMappingCollection GetCollectionToTest() => new CountryTierPriceCodeMappingCollection(NewFallbackLevel(), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CountryTierPriceCodeMapping();

		#endregion
	}
}
