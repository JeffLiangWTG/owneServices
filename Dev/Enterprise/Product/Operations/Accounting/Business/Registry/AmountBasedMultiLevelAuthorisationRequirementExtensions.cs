using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Registry.Business;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Registry.Business
{
	public static class AmountBasedMultiLevelAuthorisationRequirementExtensions
	{
		public static T GetAuthorisationRequired<T>(this IEnumerable<T> items, ZDecimal amount)
			where T : AmountBasedMultiLevelAuthorisationRequirement
		{
			T result = null;

			if (amount != 0)
			{
				foreach (var item in items)
				{
					if (item.Range == RangeCodes.Above && amount > item.Amount)
					{
						return item;
					}
				}

				foreach (var item in items)
				{
					if (item.Range == RangeCodes.UpTo &&
						(result == null || item.Amount < result.Amount))
					{
						if (item.Amount >= amount)
						{
							result = item;
						}
					}
				}
			}

			return result;
		}
	}
}