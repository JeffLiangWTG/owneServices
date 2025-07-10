using System.Collections.Generic;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class ValidatorPack
	{
		public ValidatorPack()
		{
			RequiredFilter = new RequiredFilterValidator();
			RequiredBranchFilter = new RequiredBranchFilterValidator();
			AtLeastOneAgentFilterContainsOrgProxy = new AtLeastOneAgentFilterContainsOrgProxyValidator();
			AtLeastOneFilterNotEmptyValidators = new Dictionary<string, AtLeastOneFilterNotEmptyValidator>();
			OnlyOneFilterNotEmptyValidators = new Dictionary<string, OnlyOneFilterNotEmptyValidator>();
			AnyOneFilterNotEmptyValidators = new Dictionary<string, AnyOneFilterNotEmptyValidator>();
			OnlyCurrentPeriodIfPayByWebService = new OnlyCurrentPeriodIfPayByWebServiceValidator();
			OnlyCurrentCompanyIfSetInCommissionRegistry = new OnlyCurrentCompanyIfSetInCommissionRegistryValidator();
		}

		public readonly RequiredFilterValidator RequiredFilter;
		public readonly RequiredBranchFilterValidator RequiredBranchFilter;
		public readonly OnlyCurrentPeriodIfPayByWebServiceValidator OnlyCurrentPeriodIfPayByWebService;
		public readonly OnlyCurrentCompanyIfSetInCommissionRegistryValidator OnlyCurrentCompanyIfSetInCommissionRegistry;
		public readonly AtLeastOneAgentFilterContainsOrgProxyValidator AtLeastOneAgentFilterContainsOrgProxy;
		readonly Dictionary<string, AtLeastOneFilterNotEmptyValidator> AtLeastOneFilterNotEmptyValidators;
		readonly Dictionary<string, OnlyOneFilterNotEmptyValidator> OnlyOneFilterNotEmptyValidators;
		readonly Dictionary<string, AnyOneFilterNotEmptyValidator> AnyOneFilterNotEmptyValidators;

		public AtLeastOneFilterNotEmptyValidator GetAtLeastOneFilterNotEmptyValidatorForGroup(string groupName)
		{
			AtLeastOneFilterNotEmptyValidator result = null;
			if (AtLeastOneFilterNotEmptyValidators.ContainsKey(groupName))
			{
				result = AtLeastOneFilterNotEmptyValidators[groupName];
			}
			else
			{
				result = new AtLeastOneFilterNotEmptyValidator();
				AtLeastOneFilterNotEmptyValidators.Add(groupName, result);
			}
			return result;
		}

		public OnlyOneFilterNotEmptyValidator GetOnlyOneFilterNotEmptyValidatorForGroup(string groupName)
		{
			OnlyOneFilterNotEmptyValidator result = null;
			if (OnlyOneFilterNotEmptyValidators.ContainsKey(groupName))
			{
				result = OnlyOneFilterNotEmptyValidators[groupName];
			}
			else
			{
				result = new OnlyOneFilterNotEmptyValidator();
				OnlyOneFilterNotEmptyValidators.Add(groupName, result);
			}
			return result;
		}

		public AnyOneFilterNotEmptyValidator GetAnyOneFilterNotEmptyValidatorForGroup(string groupName)
		{
			AnyOneFilterNotEmptyValidator result = null;
			if (AnyOneFilterNotEmptyValidators.ContainsKey(groupName))
			{
				result = AnyOneFilterNotEmptyValidators[groupName];
			}
			else
			{
				result = new AnyOneFilterNotEmptyValidator();
				AnyOneFilterNotEmptyValidators.Add(groupName, result);
			}
			return result;
		}
	}
}
