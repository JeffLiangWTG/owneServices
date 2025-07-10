using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(AuthorizationModeAndSettingsRegistryDataType))]
	public class AuthorizationModeAndSettingsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AuthorizationModeAndSettingsRegistryDataType>
	{
		protected override AuthorizationModeAndSettingsRegistryDataType GetNewDataType()
		{
			return new AuthorizationModeAndSettingsRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample1 = new AuthorizationModeAndSettings();
			sample1.AuthorizationMode = "DEF";
			var setting11 = sample1.AuthorisationSettings.AddNew();
			setting11.Amount = 100;
			setting11.AuthorisationRequirement = AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			setting11.Range = "Up to";

			var setting12 = sample1.AuthorisationSettings.AddNew();
			setting12.Amount = 100;
			setting12.Range = "Above";
			setting12.AuthorisationRequirement = AuthorisationRequirementCodes.SecondApprovalRequiredOnly;

			var sample2 = new AuthorizationModeAndSettings();
			sample2.AuthorizationMode = "TWO";
			var setting21 = sample2.AuthorisationSettings.AddNew();
			setting21.Amount = 100;
			setting21.AuthorisationRequirement = AuthorisationRequirementCodes.NoApprovalRequired;
			setting21.Range = "Up to";

			var setting22 = sample2.AuthorisationSettings.AddNew();
			setting22.Amount = 200;
			setting22.Range = "Up to";
			setting22.AuthorisationRequirement = AuthorisationRequirementCodes.FirstApprovalRequiredOnly;

			var setting23 = sample2.AuthorisationSettings.AddNew();
			setting23.Amount = 500;
			setting23.Range = "Up to";
			setting23.AuthorisationRequirement = AuthorisationRequirementCodes.SecondApprovalRequiredOnly;

			var setting24 = sample2.AuthorisationSettings.AddNew();
			setting24.Amount = 500;
			setting24.Range = "Above";
			setting24.AuthorisationRequirement = AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;

			var sample1XML = @"<?xml version=""1.0"" encoding=""utf-16""?>
<AuthorizationModeAndSettings>
	<AuthorizationMode>DEF</AuthorizationMode>
	<ArrayOfPaymentSixLevelAuthorisationSettings
		xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
		xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
		<PaymentSixLevelAuthorisationSettings>
			<Amount>100</Amount>
			<Range>Up to</Range>
			<AuthorisationRequirement>1st Level Only</AuthorisationRequirement>
		</PaymentSixLevelAuthorisationSettings>
		<PaymentSixLevelAuthorisationSettings>
			<Amount>100</Amount>
			<Range>Above</Range>
			<AuthorisationRequirement>2nd Level Only</AuthorisationRequirement>
		</PaymentSixLevelAuthorisationSettings>
	</ArrayOfPaymentSixLevelAuthorisationSettings>
</AuthorizationModeAndSettings>";

			var sample2XML = @"<?xml version=""1.0"" encoding=""utf-16""?>
<AuthorizationModeAndSettings>
	<AuthorizationMode>TWO</AuthorizationMode>
	<ArrayOfPaymentSixLevelAuthorisationSettings
		xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
		xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
		<PaymentSixLevelAuthorisationSettings>
			<Amount>100</Amount>
			<Range>Up to</Range>
			<AuthorisationRequirement>None</AuthorisationRequirement>
		</PaymentSixLevelAuthorisationSettings>
		<PaymentSixLevelAuthorisationSettings>
			<Amount>200</Amount>
			<Range>Up to</Range>
			<AuthorisationRequirement>1st Level Only</AuthorisationRequirement>
		</PaymentSixLevelAuthorisationSettings>
		<PaymentSixLevelAuthorisationSettings>
			<Amount>500</Amount>
			<Range>Up to</Range>
			<AuthorisationRequirement>2nd Level Only</AuthorisationRequirement>
		</PaymentSixLevelAuthorisationSettings>
		<PaymentSixLevelAuthorisationSettings>
			<Amount>500</Amount>
			<Range>Above</Range>
			<AuthorisationRequirement>1st Level Only</AuthorisationRequirement>
		</PaymentSixLevelAuthorisationSettings>
	</ArrayOfPaymentSixLevelAuthorisationSettings>
</AuthorizationModeAndSettings>";

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(sample1, sample1XML),
				new ValidSampleAndBinaryValueInDB(sample2, sample2XML),
			};
		}

		protected override string ExpectedEditorName => "AuthorizationModeAndSettingsEditor";
	}
}
