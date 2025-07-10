using System;
using System.Collections.Generic;
using System.Text;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business.AST
{
	/// <summary>
	/// An operand representing the value of a constraint.
	/// THIS CLASS MUST NOT BE ACCESSED DIRECTLY EXCEPT BY THE PARSER
	/// </summary>
	sealed class FilterConstraintRef : IFilterOperand
	{
		public FilterConstraintRef(string constraintName)
		{
			if (constraintName == null)
			{
				throw new ArgumentNullException(nameof(constraintName));
			}

			this.constraintName = constraintName;
		}

		public string ConstraintName
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return constraintName; }
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			Append(builder);
			return builder.ToString();
		}

		#region IOperand Members

		public object Evaluate(IFilterValueProvider valueProvider, bool returnString = false)
		{
			if (valueProvider == null)
			{
				throw new ArgumentNullException(nameof(valueProvider));
			}

			IFilterConstraint constraint = valueProvider.GetConstraint(constraintName);
			if (constraint != null)
			{
				return returnString ? constraint.GetDefaultStringValue() : constraint.GetValue();
			}
			else
			{
				return null;
			}
		}

		public void Append(StringBuilder builder)
		{
			builder.Append(ConstraintName);
		}

		public void CollectConstraints(IList<string> list)
		{
			if (!list.Contains(constraintName))
			{
				list.Add(constraintName);
			}
		}

		public IFilterOperand AddConstraintPrefix(string prefix)
		{
			return new FilterConstraintRef(prefix + "." + ConstraintName);
		}

		public IFilterOperand RemoveConstraintPrefix(string prefix)
		{
			if (MatchesPrefix(prefix))
			{
				return new FilterConstraintRef(ConstraintName.Substring(prefix.Length + 1));
			}
			else
			{
				throw new InvalidOperationException(string.Format("'{0}' does not start with '{1}.'.", ConstraintName, prefix));
			}
		}

		public FilterRequirement GetValuesForRequirement(FilterRequirement requirement)
		{
			if (StringComparer.OrdinalIgnoreCase.Compare(requirement.ConstraintName, ConstraintName) == 0)
			{
				return requirement;
			}
			else
			{
				return new FilterRequirement(requirement.ConstraintName);
			}
		}

		#endregion

		#region Implementation

		bool MatchesPrefix(string prefix)
		{
			if (prefix.Length >= constraintName.Length)
			{
				return false;
			}
			else
			{
				for (int i = 0; i < prefix.Length; i++)
				{
					char c1 = prefix[i];
					char c2 = constraintName[i];

					if (c1 == '+')
					{
						c1 = '.';
					}

					if (c2 == '+')
					{
						c2 = '.';
					}

					if (c1 != c2)
					{
						return false;
					}
				}

				char c = constraintName[prefix.Length];
				return c == '.' || c == '+';
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly string constraintName;

		#endregion
	}
}
