using System;
using CargoWise.Common;
using Enterprise.Registry.Business;

namespace Enterprise.DataTransfer.Native.Common
{
	public static class OptionalEntityConditionMapping
	{
		public static bool IsEntityOptionalConditionSatisfied(string optionalCondition)
		{
			Argument.NotNullOrEmpty(optionalCondition, nameof(optionalCondition));

			var result = false;
			switch (optionalCondition)
			{
				case "Organization_OrgPatternMatchOverride":
					result = OrganisationsDataRegistry.Instance.EnableOrgPatternMatchOverride.Value;
					break;
				default:
					throw new ArgumentException("Invalid optional condition which is not covered in OptionalEntityConditionMapping.", nameof(optionalCondition));
			}

			return result;
		}
	}
}
