using System.Collections.Generic;
using System.Text;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business
{
	public interface IFilterExpression
	{
		FilterPrecedence Precidence { get; }

		/// <summary>
		/// Returns true if this represents the empty filter or false otherwise.
		/// </summary>
		bool IsEmpty { get; }

		/// <summary>
		/// Evaluate the expression represented by this object and it's children.
		/// </summary>
		/// <param name="valueProvider">The value provider against which all constraint checks for the evaluation are made.</param>
		/// <returns>The result of the evaluated expression.</returns>
		bool Evaluate(IFilterValueProvider valueProvider);

		/// <summary>
		/// Appends a string representation of the expression to the given string builder.
		/// </summary>
		/// <param name="builder">The string builder to append to.</param>
		void Append(StringBuilder builder);

		/// <summary>
		/// Adds to the given list all constraints used in the expression that are not already in the list.
		/// </summary>
		/// <param name="list">The list of constraints to add to.</param>
		void CollectConstraints(IList<string> list);

		/// <summary>
		/// Determines if this expression enforces the given requirement.
		/// </summary>
		/// <param name="requirement">The requirement to check against.</param>
		/// <returns>Returns true if this expression is atleast as strict as the passed in requirement or false otherwise.</returns>
		bool IsRequirementEnforced(FilterRequirement requirement);

		/// <summary>
		/// Returns an equivilent filter where all constraint reference are prefixed with the given value.
		/// </summary>
		/// <param name="prefix">The prefix to apply to all constraint references</param>
		/// <returns>An equivilent filter where the prefix has been applied to all constraint references.</returns>
		IFilterExpression AddConstraintPrefix(string prefix);

		/// <summary>
		/// Returns an equivilent filter where the given prefix is removed from all constraint references.
		/// </summary>
		/// <param name="prefix">The prefix to remove from all constraint references.</param>
		/// <returns>An equivilent filter where the prefix has been removed from all constraint references.</returns>
		IFilterExpression RemoveConstraintPrefix(string prefix);

		/// <summary>
		/// Returns an array of all the sub-expressions used directly by this expression.
		/// </summary>
		IFilterExpression[] GetSubExpressions();
	}
}
