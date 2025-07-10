using System;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(CountryTierPriceCodeMappingLine))]
	internal class CountryTierPriceCodeMappingLineTest : RegistryBusinessObjectTemplateTestCase<CountryTierPriceCodeMappingLine>
	{
		public void TestGetClone()
		{
			var mappingLine = GetBusinessObjectToClone();
			var countryCode = "AU";
			var countryTierCode = "C1";
			mappingLine.CountryCode = countryCode;
			mappingLine.CountryTierCode = countryTierCode;

			var clone = (CountryTierPriceCodeMappingLine)mappingLine.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			AssertEquals(countryCode, clone.CountryCode);
			AssertEquals(countryTierCode, clone.CountryTierCode);
		}

		public void TestValidateCountryCode()
		{
			var mappingLine = NewPopulatedBusinessObject();
			mappingLine.CountryCode = "XX";
			mappingLine.CountryTierCode = "C1";
			AssertHasError(mappingLine.CountryCodeInfo, "Enter a valid Country Code.");
			mappingLine.CountryCode = "US";
			AssertNoErrors(mappingLine);
		}

		public void TestValidation()
		{
			var mappingLine = NewPopulatedBusinessObject();
			mappingLine.RunPreSaveValidation();
			AssertHasError(mappingLine.CountryCodeInfo, "Please enter a Country Code.");
			mappingLine.CountryCode = "AU";
			AssertHasError(mappingLine.CountryTierCodeInfo, "Please enter a Country Tier Code.");
			mappingLine.CountryTierCode = "C1";
			AssertNoErrors(mappingLine);
		}

		public void TestValidateCountryCode_DuplicateCodes()
		{
			var mappingLineCollection = new CountryTierPriceCodeMappingLineCollection();
			var mappingLine1 = mappingLineCollection.AddNew("AU", "C1");
			mappingLineCollection.RunPreSaveValidation();
			AssertNoErrors(mappingLine1);
			AssertNoRowErrors(mappingLine1);

			var mappingLine2 = mappingLineCollection.AddNew("AU", "C2");
			mappingLine1.ValidateCountryCode();
			AssertHasError(mappingLine1.CountryCodeInfo, "The Country Code has been duplicated and must be unique.");
			AssertHasError(mappingLine2.CountryCodeInfo, "The Country Code has been duplicated and must be unique.");

			mappingLine2.CountryCode = "NZ";
			mappingLineCollection.RunPreSaveValidation();
			AssertNoErrors(mappingLine1);
			AssertNoErrors(mappingLine2);
		}

		#region Overrides

		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => true;

		protected override CountryTierPriceCodeMappingLine GetBusinessObjectToClone() => NewPopulatedBusinessObject();

		protected override CountryTierPriceCodeMappingLine GetBusinessObjectToSerialise() => NewPopulatedBusinessObject();

		CountryTierPriceCodeMappingLine NewPopulatedBusinessObject() => new CountryTierPriceCodeMappingLine(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);

		#endregion
	}
}
