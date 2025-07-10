using System;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(CountryTierPriceCodeMapping))]
	internal class CountryTierPriceCodeMappingTest : RegistryBusinessObjectTemplateTestCase<CountryTierPriceCodeMapping>
	{
		public void TestGetClone()
		{
			var mapping = NewPopulatedBusinessObject();
			var priceCode = "P01";
			var systemCode = "X01";
			var countryCode = "AU";
			var countryTierCode = "C1";
			mapping.PriceCode = priceCode;
			mapping.SystemCode = systemCode;
			var mappingLine = mapping.MappingLines.AddNew();
			mappingLine.CountryCode = countryCode;
			mappingLine.CountryTierCode = countryTierCode;

			var clone = (CountryTierPriceCodeMapping)mapping.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			AssertEquals(priceCode, clone.PriceCode);
			AssertEquals(systemCode, clone.SystemCode);
			AssertEquals(1, clone.MappingLines.Count);
			AssertEquals(countryCode, clone.MappingLines[0].CountryCode);
			AssertEquals(countryTierCode, clone.MappingLines[0].CountryTierCode);
		}

		public void TestValidation()
		{
			var mapping1 = NewPopulatedBusinessObject();
			mapping1.RunPreSaveValidation();
			AssertHasError(mapping1.PriceCodeInfo, "Please enter a Price Code.");
			mapping1.PriceCode = "P01";
			AssertHasError(mapping1.SystemCodeInfo, "Please enter a System.");
			mapping1.SystemCode = "X01";
			AssertNoErrors(mapping1);
		}

		#region Overrides

		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => true;

		protected override CountryTierPriceCodeMapping GetBusinessObjectToClone() => NewPopulatedBusinessObject();

		protected override CountryTierPriceCodeMapping GetBusinessObjectToSerialise() => NewPopulatedBusinessObject();

		CountryTierPriceCodeMapping NewPopulatedBusinessObject() => new CountryTierPriceCodeMapping(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);

		#endregion
	}
}
