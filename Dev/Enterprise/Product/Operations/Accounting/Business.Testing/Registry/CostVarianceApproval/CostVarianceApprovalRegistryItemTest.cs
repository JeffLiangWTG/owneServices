using System;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CostVarianceApprovalRegistryItem))]
	class CostVarianceApprovalRegistryItemTest : StronglyTypedRegistryItemTestCase<CostVarianceApproval>
	{
		[ExpectNoExceptions]
		public void TestGetCostVarianceApprovalRegistryItemShouldWorkUnderOtherCulture()
		{
			var costVarianceApproval = new CostVarianceApproval();
			costVarianceApproval.VarianceCalculationStyle = Enterprise.Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			costVarianceApproval.VarianceComparisonOption = Enterprise.Core.Constants.CostVarianceComparisonOption.Job;
			var upToAuthorisationRequirement = costVarianceApproval.AuthorisationRequirements.AddNew();
			upToAuthorisationRequirement.VarianceSign = CostVarianceApprovalAuthorisationRequirement.VarianceSigns.Plus;
			upToAuthorisationRequirement.AuthorisationRequirementLocalized = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			upToAuthorisationRequirement.RangeLocalized = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upToAuthorisationRequirement.LocalCostAmount = 33.3m;
			var authorisationRequirement = costVarianceApproval.AuthorisationRequirements.AddNew();
			authorisationRequirement.VarianceSign = CostVarianceApprovalAuthorisationRequirement.VarianceSigns.Plus;
			authorisationRequirement.AuthorisationRequirementLocalized = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			authorisationRequirement.RangeLocalized = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			authorisationRequirement.LocalCostAmount = 33.3m;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, costVarianceApproval);

			using (Culture.SetTemporarily(Culture.GetCulture("SE")))
			{
				var amount = AccountingConfigurationRegistry.Instance.CostVarianceApproval.Value.AuthorisationRequirements[0].LocalCostAmount;
				AssertEquals(33.3m, amount);
			}
		}

		protected override StronglyTypedRegistryItem<CostVarianceApproval, CostVarianceApproval> GetNewRegistryItem()
		{
			return new CostVarianceApprovalRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
