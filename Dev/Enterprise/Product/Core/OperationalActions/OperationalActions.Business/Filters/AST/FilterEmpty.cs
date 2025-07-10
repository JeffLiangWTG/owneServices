using System.Collections.Generic;
using System.Text;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business.AST
{
	/// <summary>
	/// An expression representing an empty expression. This is used to represent the behaviour when parsed expression string is empty.
	/// THIS CLASS MUST NOT BE ACCESSED DIRECTLY EXCEPT BY THE PARSER
	/// </summary>
	sealed class FilterEmpty : IFilterExpression
	{
		public override string ToString()
		{
			return "";
		}

		#region IFilterExpression Members

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public FilterPrecedence Precidence
		{
			get { return FilterPrecedence.Comparison; }
		}

		bool IFilterExpression.IsEmpty
		{
			get { return true; }
		}

		public bool Evaluate(IFilterValueProvider valueProvider)
		{
			return true;
		}

		public void Append(StringBuilder builder)
		{
		}

		public void CollectConstraints(IList<string> list)
		{
		}

		public IFilterExpression[] GetSubExpressions()
		{
			return System.Array.Empty<IFilterExpression>();
		}

		IFilterExpression IFilterExpression.AddConstraintPrefix(string prefix)
		{
			return this;
		}

		IFilterExpression IFilterExpression.RemoveConstraintPrefix(string prefix)
		{
			return this;
		}

		public bool IsRequirementEnforced(FilterRequirement requirement)
		{
			return false;
		}

		#endregion
	}
}
