using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(CountryTierPriceCodeMappingLineCollection))]
	internal class CountryTierPriceCodeMappingLineCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CountryTierPriceCodeMappingLineCollection>
	{
		#region Overrides

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override CountryTierPriceCodeMappingLineCollection GetCollectionToTest() => new CountryTierPriceCodeMappingLineCollection(NewFallbackLevel(), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CountryTierPriceCodeMappingLine();

		#endregion
	}
}
