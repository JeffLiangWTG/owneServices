using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing
{
	public class TaxIdAndTaxMessageMappingLimitTaxMessagesValidatorTest : TaxIdAndTaxMessageMappingValidatorTest
	{
		protected override void AssertLineTypes(string[] lineTypes)
		{
			AssertEquals(false, MappingValidator.IsValidateMapping(lineTypes, taxRate1, taxMsg2));
			AssertEquals(true, MappingValidator.IsValidateMapping(lineTypes, taxRate1, taxMsg11));
			AssertEquals(true, MappingValidator.IsValidateMapping(lineTypes, taxRate1, taxMsg12));

			AssertEquals(false, MappingValidator.IsValidateMapping(lineTypes, taxRate2, taxMsg11));
			AssertEquals(false, MappingValidator.IsValidateMapping(lineTypes, taxRate2, taxMsg12));
			AssertEquals(true, MappingValidator.IsValidateMapping(lineTypes, taxRate2, taxMsg2));

			AssertEquals(true, MappingValidator.IsValidateMapping(lineTypes, taxRate3, taxMsg11));
			AssertEquals(true, MappingValidator.IsValidateMapping(lineTypes, taxRate3, taxMsg2));
		}

		protected override void AssertMixLineTypes(string[] lineTypes)
		{
			AssertEquals(true, MappingValidator.IsValidateMapping(lineTypes, taxRate1, taxMsg11));
			AssertEquals(false, MappingValidator.IsValidateMapping(lineTypes, taxRate1, taxMsg12));
			AssertEquals(false, MappingValidator.IsValidateMapping(lineTypes, taxRate1, taxMsg12));
			AssertEquals(true, MappingValidator.IsValidateMapping(lineTypes, taxRate2, taxMsg11));
			AssertEquals(true, MappingValidator.IsValidateMapping(lineTypes, taxRate3, taxMsg2));
		}
		protected override void AssertOtherLineTypes(string[] otherLineTypes)
		{
			foreach (var lineType in otherLineTypes)
			{
				var lineTypes = new[] { lineType };
				AssertEquals(true, MappingValidator.IsValidateMapping(lineTypes, taxRate1, taxMsg2));
				AssertEquals(true, MappingValidator.IsValidateMapping(lineTypes, taxRate2, taxMsg11));
				AssertEquals(true, MappingValidator.IsValidateMapping(lineTypes, taxRate1, taxMsg11));
				AssertEquals(true, MappingValidator.IsValidateMapping(lineTypes, taxRate2, taxMsg2));
			}
		}

		protected override void AssertEmptyValues()
		{
			AssertEquals(true, MappingValidator.IsValidateMapping(System.Array.Empty<string>(), taxRate1, taxMsg2));
			AssertEquals(true, MappingValidator.IsValidateMapping(new[] { TransactionLineTypes.Cost }, null, taxMsg2));
			AssertEquals(false, MappingValidator.IsValidateMapping(new[] { TransactionLineTypes.Cost }, taxRate1, null));
		}

		protected override TaxIdAndTaxMessageMappingValidator CreateMappingValidator(TaxIdAndTaxMessageCombinationRulesCollection rules)
			=> new TaxIdAndTaxMessageMappingLimitTaxMessagesValidator(rules);
	}
}
