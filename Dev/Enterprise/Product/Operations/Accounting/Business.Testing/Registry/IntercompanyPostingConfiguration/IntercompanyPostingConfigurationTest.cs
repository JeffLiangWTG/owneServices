using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using AuthorisationRequirementCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(IntercompanyPostingConfiguration))]
	public class IntercompanyPostingConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestMaxExcessCostApprovalLevelList()
		{
			AssertEquals("MaxExcessCostApprovalLevelList.Count", 3, BizObj.MaxCostVarianceApprovalLevelList.Count);
			AssertEquals("The RangeList should contain NoApprovalRequired", true, BizObj.MaxCostVarianceApprovalLevelList.ContainsCode(AuthorisationRequirementCodes.NoApprovalRequired));
			AssertEquals("The RangeList should contain FirstApprovalRequiredOnly", true, BizObj.MaxCostVarianceApprovalLevelList.ContainsCode(AuthorisationRequirementCodes.FirstApprovalRequiredOnly));
			AssertEquals("The RangeList should contain SecondApprovalRequiredOnly", true, BizObj.MaxCostVarianceApprovalLevelList.ContainsCode(AuthorisationRequirementCodes.SecondApprovalRequiredOnly));
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			IntercompanyPostingConfiguration result = new IntercompanyPostingConfiguration();

			result.Company = "EDI";
			result.MaxCostVarianceApprovalLevel = result.MaxCostVarianceApprovalLevelList[0].ToString();

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new IntercompanyPostingConfiguration BizObj
		{
			get { return (IntercompanyPostingConfiguration)base.BizObj; }
		}

		protected virtual IntercompanyPostingConfigurationCollection GetAuthorisationSettingsCollection()
		{
			return new IntercompanyPostingConfigurationCollection();
		}

		#endregion
	}
}
