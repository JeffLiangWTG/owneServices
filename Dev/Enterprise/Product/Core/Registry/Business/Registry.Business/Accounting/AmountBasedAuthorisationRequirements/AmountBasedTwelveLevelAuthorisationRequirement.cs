using System.Diagnostics.CodeAnalysis;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public abstract class AmountBasedTwelveLevelAuthorisationRequirement : AmountBasedTwoLevelAuthorisationRequirement
	{
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Don't see any complexity at all")]
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
				case AuthorisationRequirementCodes.FourthApprovalRequiredOnly:
					weight = 4;
					break;
				case AuthorisationRequirementCodes.FifthApprovalRequiredOnly:
					weight = 5;
					break;
				case AuthorisationRequirementCodes.SixthApprovalRequiredOnly:
					weight = 6;
					break;
				case AuthorisationRequirementCodes.SeventhApprovalRequiredOnly:
					weight = 7;
					break;
				case AuthorisationRequirementCodes.EighthApprovalRequiredOnly:
					weight = 8;
					break;
				case AuthorisationRequirementCodes.NinthApprovalRequiredOnly:
					weight = 9;
					break;
				case AuthorisationRequirementCodes.TenthApprovalRequiredOnly:
					weight = 10;
					break;
				case AuthorisationRequirementCodes.EleventhApprovalRequiredOnly:
					weight = 11;
					break;
				case AuthorisationRequirementCodes.TwelfthApprovalRequiredOnly:
					weight = 12;
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
			resultPairList.AddPair(AuthorisationRequirementDescriptions.FourthApprovalRequiredOnly);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.FifthApprovalRequiredOnly);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.SixthApprovalRequiredOnly);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.SeventhApprovalRequiredOnly);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.EighthApprovalRequiredOnly);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.NinthApprovalRequiredOnly);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.TenthApprovalRequiredOnly);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.EleventhApprovalRequiredOnly);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.TwelfthApprovalRequiredOnly);
			return resultPairList;
		}

		#endregion

		#endregion
	}
}
