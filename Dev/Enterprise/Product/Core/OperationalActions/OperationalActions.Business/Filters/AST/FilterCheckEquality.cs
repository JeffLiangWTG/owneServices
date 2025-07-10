using System;
using System.Text;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business.AST
{
	sealed class FilterCheckEquality : FilterCheck
	{
		public FilterCheckEquality(IFilterOperand lhs, IFilterOperand rhs)
			: base(lhs, rhs) { }

		public override void Append(StringBuilder builder)
		{
			Left.Append(builder);
			builder.Append(" == ");
			Right.Append(builder);
		}

		public override bool Evaluate(IFilterValueProvider valueProvider)
		{
			if (valueProvider == null)
			{
				throw new ArgumentNullException(nameof(valueProvider));
			}

			var values = CastToType(valueProvider);

			if (values[1] is string rightStringValue && values[0] != null)
			{
				if (values[0] is DateTime leftDateTimeValue)
				{
					return FilterRegexProvider.GetDateTimeFilterWithWildcardsRegexMatch(rightStringValue, leftDateTimeValue).Success;
				}

				return FilterRegexProvider.GetFilterWithWildcardsRegex(rightStringValue).Match(values[0].ToString()).Success;
			}
			else
			{
				if (!(values[0] is IComparable leftValue) || !(values[1] is IComparable rightValue))
				{
					return false;
				}

				return leftValue.CompareTo(rightValue) == 0;
			}
		}

		public override bool IsRequirementEnforced(FilterRequirement requirement)
		{
			return Left.GetValuesForRequirement(requirement).IsSuperSetOf(Right.GetValuesForRequirement(requirement));
		}

		#region Implementation

		protected override IFilterExpression NewFilter(IFilterOperand lhs, IFilterOperand rhs)
		{
			return new FilterCheckEquality(lhs, rhs);
		}

		#endregion
	}
}
