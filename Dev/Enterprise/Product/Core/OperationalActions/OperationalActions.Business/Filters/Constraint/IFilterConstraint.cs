namespace Enterprise.Services.OperationalActions.Business
{
	public interface IFilterConstraint
	{
		/// <summary>
		/// Name of the constraint as used in the filter expression.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The noun to describe a single value.
		/// </summary>
		string SingularValueName { get; }

		/// <summary>
		/// The noun to describe multiple values.
		/// </summary>
		string PluralValueName { get; }

		/// <summary>
		/// Description of the constraint.
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Get the current value of the constraint in its original type.
		/// </summary>
		/// <returns>The value of the constraint.</returns>
		object GetValue();

		/// <summary>
		/// Get the string value of the constraint for if there is a casting problem. 
		/// </summary>
		/// <returns></returns>
		string GetDefaultStringValue();
	}
}
