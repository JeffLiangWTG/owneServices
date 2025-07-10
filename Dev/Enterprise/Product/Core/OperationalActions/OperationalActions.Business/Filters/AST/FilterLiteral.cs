using System;
using System.Collections.Generic;
using System.Text;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business.AST
{
	/// <summary>
	/// An operand representing a literal string value.
	/// THIS CLASS MUST NOT BE ACCESSED DIRECTLY EXCEPT BY THE PARSER
	/// </summary>
	sealed class FilterLiteral : IFilterOperand
	{
		public FilterLiteral(string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			this.value = value;
		}

		public string Value
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return value; }
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

			return value;
		}

		public void Append(StringBuilder builder)
		{
			builder.AppendFormat("\"{0}\"", Value.Replace("\"", "\"\""));
		}

		public void CollectConstraints(IList<string> list)
		{
		}

		IFilterOperand IFilterOperand.AddConstraintPrefix(string prefix)
		{
			return this;
		}

		IFilterOperand IFilterOperand.RemoveConstraintPrefix(string prefix)
		{
			return this;
		}

		public FilterRequirement GetValuesForRequirement(FilterRequirement requirement)
		{
			FilterRequirement result = new FilterRequirement(requirement.ConstraintName);
			result.Add(value);
			return result;
		}

		#endregion

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly string value;
	}
}
