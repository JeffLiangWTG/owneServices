using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Moq;
using NUnit.Framework;

using static Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CreditTemporaryIncreaseAuthorisationSettingsRegistryDataType))]
	sealed class CreditTemporaryIncreaseAuthorisationSettingsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CreditTemporaryIncreaseAuthorisationSettingsRegistryDataType>
	{
		#region Implementation

		protected override CreditTemporaryIncreaseAuthorisationSettingsRegistryDataType GetNewDataType()
		{
			return new CreditTemporaryIncreaseAuthorisationSettingsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "CreditTemporaryIncreaseAuthorisationSettingsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new CreditTemporaryIncreaseAuthorisationSettingsCollection();
			var settings1 = collection.AddNew();
			settings1.Amount = 0;
			settings1.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			settings1.AuthorisationRequirement = settings1.AuthorisationRequirementList[0].Code;
			settings1.Percentage = 0.01;
			settings1.DaysToExpiry = 30;
			var settings2 = collection.AddNew();
			settings2.Amount = 0;
			settings2.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			settings2.AuthorisationRequirement = settings2.AuthorisationRequirementList[1].Code;
			settings2.Percentage = 0.01;
			settings2.DaysToExpiry = 20;
			settings1.ClearAllNotifications();
			settings2.ClearAllNotifications();

			var byteArrayValue = System.Text.Encoding.Unicode.GetBytes(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfCreditTemporaryIncreaseAuthorisationSettings xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <CreditTemporaryIncreaseAuthorisationSettings>
    <Amount>0</Amount>
    <Range>Up to</Range>
    <AuthorisationRequirement>None</AuthorisationRequirement>
    <Percentage>0.01</Percentage>
    <DaysToExpiry>30</DaysToExpiry>
  </CreditTemporaryIncreaseAuthorisationSettings>
  <CreditTemporaryIncreaseAuthorisationSettings>
    <Amount>0</Amount>
    <Range>Above</Range>
    <AuthorisationRequirement>1st Level Only</AuthorisationRequirement>
    <Percentage>0.01</Percentage>
    <DaysToExpiry>20</DaysToExpiry>
  </CreditTemporaryIncreaseAuthorisationSettings>
</ArrayOfCreditTemporaryIncreaseAuthorisationSettings>");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		public void TestValidation()
		{
			// Arrange
			var proposedValue = new CreditTemporaryIncreaseAuthorisationSettingsCollection
			{
				new CreditTemporaryIncreaseAuthorisationSettings
				{
					Amount = 0,
					Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo,
					AuthorisationRequirement = AuthorisationRequirementDescriptions.NoApprovalRequired,
					Percentage = 0.01,
					DaysToExpiry = 30
				},
			};

			var dataType = new CreditTemporaryIncreaseAuthorisationSettingsRegistryDataType();
			var registryItemMock = new Mock<IRegistryItem>();

			// Act
			// Assert
			var result = AssertExceptionThrown<RegistryValidationException>(
				() => dataType.Validate(registryItemMock.Object, proposedValue, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Range: There must be at least one 'Up to' and one 'Above' line.", result.Message);
		}

		#endregion
	}
}
