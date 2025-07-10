using System;
using System.Collections.Generic;
using System.Text;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business.AST
{
	/// <summary>
	/// An expression representing the combination of 2 sub expressions as an OR operation.
	/// THIS CLASS MUST NOT BE ACCESSED DIRECTLY EXCEPT BY THE PARSER
	/// </summary>
	sealed class FilterOr : IFilterExpression
	{
		public FilterOr(IFilterExpression left, IFilterExpression right)
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

		public IFilterExpression Left
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return left; }
		}

		public IFilterExpression Right
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return right; }
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			Append(builder);
			return builder.ToString();
		}

		#region IFilterExpression Members

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		FilterPrecedence IFilterExpression.Precidence
		{
			get { return FilterPrecedence.Or; }
		}

		bool IFilterExpression.IsEmpty
		{
			get { return false; }
		}

		public void Append(StringBuilder builder)
		{
			{
				if (left.Precidence > FilterPrecedence.Or)
				{
					builder.Append("(");
					left.Append(builder);
					builder.Append(")");
				}
				else
				{
					left.Append(builder);
				}

				builder.Append(" || ");

				if (right.Precidence > FilterPrecedence.Or)
				{
					builder.Append("(");
					right.Append(builder);
					builder.Append(")");
				}
				else
				{
					right.Append(builder);
				}
			}
		}

		public bool Evaluate(IFilterValueProvider valueProvider)
		{
			return left.Evaluate(valueProvider) || right.Evaluate(valueProvider);
		}

		public void CollectConstraints(IList<string> list)
		{
			left.CollectConstraints(list);
			right.CollectConstraints(list);
		}

		public IFilterExpression[] GetSubExpressions()
		{
			return new IFilterExpression[] { Left, Right };
		}

		public IFilterExpression AddConstraintPrefix(string prefix)
		{
			return new FilterOr(Left.AddConstraintPrefix(prefix), Right.AddConstraintPrefix(prefix));
		}

		public IFilterExpression RemoveConstraintPrefix(string prefix)
		{
			return new FilterOr(Left.RemoveConstraintPrefix(prefix), Right.RemoveConstraintPrefix(prefix));
		}

		public bool IsRequirementEnforced(FilterRequirement requirement)
		{
			return left.IsRequirementEnforced(requirement)
				&& right.IsRequirementEnforced(requirement);
		}

		#endregion

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly IFilterExpression left;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly IFilterExpression right;
	}
}
