using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryDataType))]
	sealed class AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryDataType>
	{
		#region Implementation

		protected override AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryDataType GetNewDataType()
		{
			return new AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			var settings1 = collection.AddNew();
			settings1.Amount = 0;
			settings1.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			settings1.AuthorisationRequirement = settings1.AuthorisationRequirementList[0].Code;
			settings1.Percentage = 0.01;
			var settings2 = collection.AddNew();
			settings2.Amount = 0;
			settings2.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			settings2.AuthorisationRequirement = settings2.AuthorisationRequirementList[1].Code;
			settings2.Percentage = 0.01;
			settings1.ClearAllNotifications();
			settings2.ClearAllNotifications();

			var byteArrayValue = System.Text.Encoding.Unicode.GetBytes(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfAmountOrPercentageBasedThreeLevelAuthorisationRequirement xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <AmountOrPercentageBasedThreeLevelAuthorisationRequirement>
    <Amount>0</Amount>
    <Range>Up to</Range>
    <AuthorisationRequirement>None</AuthorisationRequirement>
    <Percentage>0.01</Percentage>
  </AmountOrPercentageBasedThreeLevelAuthorisationRequirement>
  <AmountOrPercentageBasedThreeLevelAuthorisationRequirement>
    <Amount>0</Amount>
    <Range>Above</Range>
    <AuthorisationRequirement>1st Level Only</AuthorisationRequirement>
    <Percentage>0.01</Percentage>
  </AmountOrPercentageBasedThreeLevelAuthorisationRequirement>
</ArrayOfAmountOrPercentageBasedThreeLevelAuthorisationRequirement>");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
