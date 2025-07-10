using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.ExitControl.Module
{
	public static class FilterBusinessObjectHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public static ZQuery GetJobNumberQuery(SQLComparisonOperator @operator, ZString oldValue)
		{
			var result = new ZQuery();
			if (@operator == SQLComparisonOperator.Equal)
			{
				var multiSearchSeparator = EnvProxy.Instance.Registry.MultiSearchSeparator;
				ZString[] values = null;
				if (oldValue.Contains(multiSearchSeparator))
				{
					values = oldValue.Split(multiSearchSeparator).Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
				}
				else
				{
					values = new ZString[] { oldValue };
				}

				var innerQuery = new ZQuery();

				foreach (var value in values)
				{
					if (value.Length <= 8)
					{
						if (value.StartsWith("S") || value.StartsWith("C") || value.StartsWith("B") || value.StartsWith("E"))
						{
							var jobPrefix = value.Left(1);
							var jobNumber = value.SubstringSafe(1);
							if (jobNumber.IsEmpty)
							{
								innerQuery.AddToFilter(JoinCondition.Or, CusExitHeaderSchema.CXH_JobReference, SQLComparisonOperator.StartsWith, jobPrefix);
							}
							else if (jobNumber.IsNumbersOnlyOrEmpty)
							{
								var jobNumberZeroPadded = jobNumber.PadLeft(8, '0');
								var subQuery = new ZQuery(new ZQuery(CusExitHeaderSchema.CXH_JobReference, SQLComparisonOperator.Equal, jobPrefix + jobNumber));
								subQuery.AddToFilter(JoinCondition.Or, CusExitHeaderSchema.CXH_JobReference, SQLComparisonOperator.Equal, jobPrefix + jobNumberZeroPadded);
								innerQuery.AddToFilter(subQuery, JoinCondition.Or);
							}
							else
							{
								innerQuery.AddToFilter(JoinCondition.Or, CusExitHeaderSchema.CXH_JobReference, SQLComparisonOperator.Equal, value);
							}
						}
						else if (value.IsNumbersOnlyOrEmpty)
						{
							var jobNumberZeroPadded = value.PadLeft(8, '0');
							var subQuery = new ZQuery(new ZQuery(CusExitHeaderSchema.CXH_JobReference, SQLComparisonOperator.Equal, value));
							subQuery.AddToFilter(JoinCondition.Or, CusExitHeaderSchema.CXH_JobReference, SQLComparisonOperator.Equal, 'S' + jobNumberZeroPadded);
							subQuery.AddToFilter(JoinCondition.Or, CusExitHeaderSchema.CXH_JobReference, SQLComparisonOperator.Equal, 'C' + jobNumberZeroPadded);
							subQuery.AddToFilter(JoinCondition.Or, CusExitHeaderSchema.CXH_JobReference, SQLComparisonOperator.Equal, 'B' + jobNumberZeroPadded);
							subQuery.AddToFilter(JoinCondition.Or, CusExitHeaderSchema.CXH_JobReference, SQLComparisonOperator.Equal, 'E' + jobNumberZeroPadded);
							innerQuery.AddToFilter(subQuery, JoinCondition.Or);
						}
						else
						{
							innerQuery.AddToFilter(JoinCondition.Or, CusExitHeaderSchema.CXH_JobReference, SQLComparisonOperator.Equal, value);
						}
					}
					else
					{
						innerQuery.AddToFilter(JoinCondition.Or, CusExitHeaderSchema.CXH_JobReference, SQLComparisonOperator.Equal, value);
					}
				}

				result.AddToFilter(innerQuery);
			}
			else
			{
				result.AddToFilter_PossiblyCommaSeparated(CusExitHeaderSchema.CXH_JobReference, @operator, oldValue);
			}
			return result;
		}
	}
}
