using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public abstract class AmountBasedThreeLevelAuthorisationRequirement : AmountBasedTwoLevelAuthorisationRequirement
	{
		protected override int GetAuthorisationRequirementWeightCore(string authorisationRequirement)
		{
			int weight = -1;
			switch (authorisationRequirement)
			{
				case AuthorisationRequirementCodes.NoApprovalRequired:
					weight = 0;
					break;
				case AuthorisationRequirementCodes.FirstApprovalRequiredOnly:
					weight = 1;
					break;
				case AuthorisationRequirementCodes.SecondApprovalRequiredOnly:
					weight = 2;
					break;
				case AuthorisationRequirementCodes.ThirdApprovalRequiredOnly:
					weight = 3;
					break;
			}
			return weight;
		}

		#region Lookups

		#region Authorisation Requirement List

		protected override CodeDescriptionPairList GetAuthorisationRequirementList()
		{
			CodeDescriptionPairList resultPairList = new CodeDescriptionPairList();
			resultPairList.AddPair(AuthorisationRequirementDescriptions.NoApprovalRequired);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.FirstApprovalRequiredOnly);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.SecondApprovalRequiredOnly);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.ThirdApprovalRequiredOnly);
			return resultPairList;
		}

		#endregion

		#endregion
	}
}
