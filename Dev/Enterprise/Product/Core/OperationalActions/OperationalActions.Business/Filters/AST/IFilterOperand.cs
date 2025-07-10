using System.Collections.Generic;
using System.Text;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business.AST
{
	/// <summary>
	/// An operand representing a value use in a comparison.
	/// </summary>
	/// <remarks>
	/// This interface (and classes implementing) it are not really useful outside the parser
	/// and abstract syntax tree. This interface is only public for the purpose of mocking.
	/// </remarks>
	public interface IFilterOperand
	{
		/// <summary>
		/// Evaluate the value of the operand represented by this object.
		/// </summary>
		/// <param name="valueProvider">The value provider for getting constraint values.</param>
		/// <param name="returnString">Decides whether to return as its orgiinal type or as a string.</param>
		/// <returns>The value of the operand.</returns>
		object Evaluate(IFilterValueProvider valueProvider, bool returnString = false);

		/// <summary>
		/// Appends a string representation of the operand to the given string builder.
		/// </summary>
		/// <param name="builder">The string builder to append to.</param>
		void Append(StringBuilder builder);

		/// <summary>
		/// Adds to the given list all constraints used in the operand that are not already in the list.
		/// </summary>
		/// <param name="list">The list of constraints to add to.</param>
		void CollectConstraints(IList<string> list);

		/// <summary>
		/// Returns an equivilent operand where all constraint reference are prefixed with the given value.
		/// </summary>
		/// <param name="prefix">The prefix to apply to all constraint references</param>
		/// <returns>An equivilent operand where the prefix has been applied to all constraint references.</returns>
		IFilterOperand AddConstraintPrefix(string prefix);

		/// <summary>
		/// Returns an equivilent operand where the given prefix is removed from all constraint references.
		/// </summary>
		/// <param name="prefix">The prefix to remove from all constraint references.</param>
		/// <returns>An equivilent operand where the prefix has been removed from all constraint references.</returns>
		IFilterOperand RemoveConstraintPrefix(string prefix);

		/// <summary>
		/// Get the set of possible/acceptable values relating to the constraint.
		/// </summary>
		FilterRequirement GetValuesForRequirement(FilterRequirement requirement);
	}
}
