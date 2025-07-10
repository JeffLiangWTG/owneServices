using System;
using System.Text;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business.AST
{
	sealed class FilterCheckGreaterThan : FilterCheck
	{
		public FilterCheckGreaterThan(IFilterOperand lhs, IFilterOperand rhs)
			: base(lhs, rhs) { }

		public override void Append(StringBuilder builder)
		{
			Left.Append(builder);
			builder.Append(" > ");
			Right.Append(builder);
		}

		public override bool Evaluate(IFilterValueProvider valueProvider)
		{
			if (valueProvider == null)
			{
				throw new ArgumentNullException(nameof(valueProvider));
			}

			var values = CastToType(valueProvider);

			if (values[0] is string && values[1] is string)
			{
				var leftValue = values[0] as string;
				var rightValue = values[1] as string;

				if (leftValue == null || rightValue == null)
				{
					return false;
				}

				return string.Compare(leftValue, rightValue, StringComparison.OrdinalIgnoreCase) > 0;
			}
			else
			{
				var leftValue = values[0] as IComparable;
				var rightValue = values[1] as IComparable;

				if (leftValue == null || rightValue == null)
				{
					return false;
				}

				return leftValue.CompareTo(rightValue) > 0;
			}
		}

		public override bool IsRequirementEnforced(FilterRequirement requirement)
		{
			return Left.GetValuesForRequirement(requirement).IsSuperSetOf(Right.GetValuesForRequirement(requirement));
		}

		#region Implementation

		protected override IFilterExpression NewFilter(IFilterOperand lhs, IFilterOperand rhs)
		{
			return new FilterCheckGreaterThan(lhs, rhs);
		}

		#endregion
	}
}
