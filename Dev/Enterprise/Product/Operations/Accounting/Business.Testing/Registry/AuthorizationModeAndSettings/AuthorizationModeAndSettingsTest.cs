using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using static Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(AuthorizationModeAndSettings))]
	public class AuthorizationModeAndSettingsTest : RegistryBusinessObjectTemplateTestCase<AuthorizationModeAndSettings>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		public void TestAuthorizationModesList()
		{
			var setting = new AuthorizationModeAndSettings();
			AssertEquals(3, setting.AuthorizationModesList.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { Constants.AuthorizationMode.Codes.Default,
					Constants.AuthorizationMode.Codes.TwoApprovers,
					Constants.AuthorizationMode.Codes.SequentialApprovers }, setting.AuthorizationModesList.GetAllCodes());
		}

		protected override AuthorizationModeAndSettings GetBusinessObjectToClone()
		{
			var result = new AuthorizationModeAndSettings();
			result.AuthorizationMode = "DEF";

			var setting1 = result.AuthorisationSettings.AddNew();
			setting1.Amount = 100;
			setting1.AuthorisationRequirement = AuthorisationRequirementCodes.NoApprovalRequired;
			setting1.Range = "Up to";

			var setting2 = result.AuthorisationSettings.AddNew();
			setting2.Amount = 500;
			setting2.Range = "Up to";
			setting2.AuthorisationRequirement = AuthorisationRequirementCodes.FirstApprovalRequiredOnly;

			var setting3 = result.AuthorisationSettings.AddNew();
			setting3.Amount = 1000;
			setting3.Range = "Up to";
			setting3.AuthorisationRequirement = AuthorisationRequirementCodes.SecondApprovalRequiredOnly;

			var setting4 = result.AuthorisationSettings.AddNew();
			setting4.Amount = 1000;
			setting4.Range = "Above";
			setting4.AuthorisationRequirement = AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;

			return result;
		}

		protected override AuthorizationModeAndSettings GetBusinessObjectToSerialise() => GetBusinessObjectToClone();
	}
}
