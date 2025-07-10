using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public abstract class AmountBasedTwoLevelAuthorisationRequirement : AmountBasedMultiLevelAuthorisationRequirement
	{
		#region Amount

		protected override void ValidateAmountCore()
		{
			base.ValidateAmountCore();
			HigherAmountsHigherAuthorisationLevelAndMandatoryAboveLevelValidation(AmountInfo);
		}

		protected void HigherAmountsHigherAuthorisationLevelAndMandatoryAboveLevelValidation(ZPropertyInfo amountProperty)
		{
			if (!Range.IsEmpty && !AuthorisationRequirement.IsEmpty)
			{
				if (OtherCollectionElements.Any())
				{
					int currentWeight = GetAuthorisationRequirementWeight(AuthorisationRequirement);

					if (Range == RangeCodes.UpTo)
					{
						foreach (AmountBasedMultiLevelAuthorisationRequirement setting in OtherCollectionElements.Where(x => x.Range == RangeCodes.UpTo))
						{
							int settingWeight = GetAuthorisationRequirementWeight(setting.AuthorisationRequirement);
							ZDecimal settingAmount = (ZDecimal)setting[amountProperty.Name];
							if ((settingAmount < (ZDecimal)amountProperty.Value && settingWeight > currentWeight) ||
								(settingAmount > (ZDecimal)amountProperty.Value && settingWeight < currentWeight))
							{
								amountProperty.AddError(Res.GetString("cb1ae0d5-24ac-405c-a3ca-781de2b65ad7", "Higher amounts must have Authorization level higher than lower amounts."));
								break;
							}
						}

						if (OtherCollectionElements.FirstOrDefault(x => x.Range == RangeCodes.Above) == null)
						{
							amountProperty.AddError(Res.GetString("6940b144-fdd7-4b22-90d4-183adb76abfb", "At least one item must be 'Above'."));
						}
					}
					else if (Range == RangeCodes.Above)
					{
						foreach (AmountBasedMultiLevelAuthorisationRequirement setting in OtherCollectionElements)
						{
							if (setting.Range == RangeCodes.UpTo && GetAuthorisationRequirementWeight(setting.AuthorisationRequirement) > currentWeight)
							{
								amountProperty.AddError(Res.GetString("cb1ae0d5-24ac-405c-a3ca-781de2b65ad7", "Higher amounts must have Authorization level higher than lower amounts."));
								break;
							}
						}
					}
				}
			}
		}

		public int GetAuthorisationRequirementWeight(string authorisationRequirement)
		{
			return GetAuthorisationRequirementWeightCore(authorisationRequirement);
		}

		protected virtual int GetAuthorisationRequirementWeightCore(string authorisationRequirement)
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
			}
			return weight;
		}

		#endregion

		#region Authorisation Requirement

		protected override void ValidateAuthorisationRequirementCore()
		{
			base.ValidateAuthorisationRequirementCore();
			if (!AuthorisationRequirementInfo.HasErrors())
			{
				if (ParentCollections.Count > 0 && !AuthorisationRequirement.IsEmpty)
				{
					PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(AuthorisationRequirementInfo, OtherCollectionElements,
						Res.GetString("b977d11a-f9ae-45ec-8bbf-8ea4dba9d244", "There must be only one '{0}' line.", AuthorisationRequirement));
				}
			}
		}

		#endregion

		#region Lookups

		#region Authorisation Requirement List

		protected override CodeDescriptionPairList GetAuthorisationRequirementList()
		{
			CodeDescriptionPairList resultPairList = new CodeDescriptionPairList();
			resultPairList.AddPair(AuthorisationRequirementDescriptions.NoApprovalRequired);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.FirstApprovalRequiredOnly);
			resultPairList.AddPair(AuthorisationRequirementDescriptions.SecondApprovalRequiredOnly);
			return resultPairList;
		}

		#endregion

		#endregion
	}
}
