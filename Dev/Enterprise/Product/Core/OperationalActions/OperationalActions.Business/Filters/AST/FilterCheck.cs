using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business.AST
{
	/// <summary>
	/// An expression representing the actual constraint check.
	/// THIS CLASS MUST NOT BE ACCESSED DIRECTLY EXCEPT BY THE PARSER
	/// </summary>
	abstract class FilterCheck : IFilterExpression
	{
		public FilterCheck(IFilterOperand left, IFilterOperand right)
		{
			if (left == null)
			{
				throw new ArgumentNullException(nameof(left));
			}

			if (right == null)
			{
				throw new ArgumentNullException(nameof(right));
			}

			this.left = left;
			this.right = right;
		}

		public IFilterOperand Left
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return left; }
		}

		public IFilterOperand Right
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return right; }
		}

		public sealed override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			Append(builder);
			return builder.ToString();
		}

		#region IFilterExpression Members

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		FilterPrecedence IFilterExpression.Precidence
		{
			get { return FilterPrecedence.Comparison; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		bool IFilterExpression.IsEmpty
		{
			get { return false; }
		}

		public abstract void Append(StringBuilder builder);

		public abstract bool Evaluate(IFilterValueProvider valueProvider);

		public void CollectConstraints(IList<string> list)
		{
			if (list == null)
			{
				throw new ArgumentNullException(nameof(list));
			}

			Left.CollectConstraints(list);
			Right.CollectConstraints(list);
		}

		public object[] CastToType(IFilterValueProvider valueProvider)
		{
			var leftValue = Left.Evaluate(valueProvider);
			var rightValue = Right.Evaluate(valueProvider);
			if (leftValue != null && rightValue != null && rightValue is IComparable)
			{
				var typeLeft = leftValue.GetType();
				var typeRight = rightValue.GetType();
				try
				{
					if (typeRight == typeof(string) && typeLeft != typeof(string)
						&& (rightValue.ToString().Contains("?") || rightValue.ToString().Contains("*")))
					{
						return new object[2] { leftValue, rightValue };
					}
					else if (typeLeft == typeof(int))
					{
						rightValue = Convert.ToDecimal(rightValue, CultureInfo.CurrentCulture);
						leftValue = Convert.ToDecimal(leftValue, CultureInfo.CurrentCulture);
					}
					else
					{
						rightValue = Convert.ChangeType(rightValue, typeLeft, CultureInfo.CurrentCulture);
					}
				}
				catch (Exception ex)
				{
					if (ex is InvalidCastException || ex is FormatException || ex is OverflowException || ex is ArgumentNullException)
					{
						leftValue = Left.Evaluate(valueProvider, returnString: true);
						rightValue = Right.Evaluate(valueProvider, returnString: true);
					}
					else
					{
						throw;
					}
				}
			}
			return new object[2] { leftValue, rightValue };
		}

		public IFilterExpression[] GetSubExpressions()
		{
			return Array.Empty<IFilterExpression>();
		}

		public IFilterExpression AddConstraintPrefix(string prefix)
		{
			return NewFilter(Left.AddConstraintPrefix(prefix), Right.AddConstraintPrefix(prefix));
		}

		public IFilterExpression RemoveConstraintPrefix(string prefix)
		{
			return NewFilter(Left.RemoveConstraintPrefix(prefix), Right.RemoveConstraintPrefix(prefix));
		}

		public abstract bool IsRequirementEnforced(FilterRequirement requirement);

		#endregion

		protected abstract IFilterExpression NewFilter(IFilterOperand lhs, IFilterOperand rhs);

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly IFilterOperand left;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly IFilterOperand right;
	}
}
