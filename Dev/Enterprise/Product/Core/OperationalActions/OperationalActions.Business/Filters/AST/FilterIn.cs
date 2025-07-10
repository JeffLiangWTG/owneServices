using System;
using System.Collections.Generic;
using System.Text;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business.AST
{
	/// <summary>
	/// An expression representing the combination of 2 sub expressions as an AND operation.
	/// THIS CLASS MUST NOT BE ACCESSED DIRECTLY EXCEPT BY THE PARSER
	/// </summary>
	sealed class FilterIn : IFilterExpression
	{
		public FilterIn(IFilterOperand left, IFilterOperand[] right)
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

		public IEnumerable<IFilterOperand> Right
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
			get { return FilterPrecedence.Comparison; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		bool IFilterExpression.IsEmpty
		{
			get { return false; }
		}

		public bool Evaluate(IFilterValueProvider valueProvider)
		{
			var lValue = Left.Evaluate(valueProvider);

			if (lValue != null)
			{
				foreach (var operand in Right)
				{
					var rValue = operand.Evaluate(valueProvider);

					if (rValue != null)
					{
						var regex = FilterRegexProvider.GetFilterWithWildcardsRegex(rValue.ToString());

						if (regex.Match(lValue.ToString()).Success)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		public void Append(StringBuilder builder)
		{
			left.Append(builder);
			builder.Append((NoResString)" in (");

			using (IEnumerator<IFilterOperand> enumerator = Right.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					enumerator.Current.Append(builder);

					while (enumerator.MoveNext())
					{
						builder.Append(", ");
						enumerator.Current.Append(builder);
					}
				}
			}

			builder.Append(')');
		}

		public void CollectConstraints(IList<string> list)
		{
			left.CollectConstraints(list);

			foreach (IFilterOperand operand in Right)
			{
				operand.CollectConstraints(list);
			}
		}

		public bool IsRequirementEnforced(FilterRequirement requirement)
		{
			FilterRequirement leftRequirement = left.GetValuesForRequirement(requirement);

			foreach (IFilterOperand operand in Right)
			{
				FilterRequirement rightRequirement = operand.GetValuesForRequirement(requirement);

				if (!leftRequirement.IsSuperSetOf(rightRequirement))
				{
					return false;
				}
			}

			return true;
		}

		public IFilterExpression[] GetSubExpressions()
		{
			return Array.Empty<IFilterExpression>();
		}

		public IFilterExpression AddConstraintPrefix(string prefix)
		{
			IFilterOperand[] newRight = new IFilterOperand[right.Length];

			for (int i = 0; i < right.Length; i++)
			{
				newRight[i] = right[i].AddConstraintPrefix(prefix);
			}

			return new FilterIn(Left.AddConstraintPrefix(prefix), newRight);
		}

		public IFilterExpression RemoveConstraintPrefix(string prefix)
		{
			IFilterOperand[] newRight = new IFilterOperand[right.Length];

			for (int i = 0; i < right.Length; i++)
			{
				newRight[i] = right[i].RemoveConstraintPrefix(prefix);
			}

			return new FilterIn(Left.RemoveConstraintPrefix(prefix), newRight);
		}

		#endregion

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly IFilterOperand left;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly IFilterOperand[] right;
	}
}
