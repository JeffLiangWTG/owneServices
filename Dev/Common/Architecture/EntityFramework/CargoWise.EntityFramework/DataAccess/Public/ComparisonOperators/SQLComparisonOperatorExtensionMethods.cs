using System.Collections.Generic;
using System.Linq;

namespace CargoWise.EntityFramework
{
	public partial class SQLComparisonOperator
	{
		public bool IsNegativeSQLOperator()
		{
			return this == DoesNotStartWith
				|| this == DoesNotEndWith
				|| this == NotContains
				|| this == NotEqual;
		}

		public SQLComparisonOperator GetNegatingSQLOperatorIfNotInSubquery()
		{
			var result = this;
			if (result == DoesNotStartWith)
			{
				result = StartsWith;
			}
			else if (result == DoesNotEndWith)
			{
				result = EndsWith;
			}
			else if (result == NotContains)
			{
				result = Contains;
			}
			else if (result == NotEqual)
			{
				result = Equal;
			}
			return result;
		}
	}

	public static class SQLComparisonOperatorExtensions
	{
		public static bool In(this SQLComparisonOperator comparisonOperator, params SQLComparisonOperator[] operators)
		{
			return operators.Contains(comparisonOperator);
		}

		public static bool In(this SQLComparisonOperator comparisonOperator, IEnumerable<SQLComparisonOperator> operators)
		{
			return operators.Contains(comparisonOperator);
		}
	}
}
