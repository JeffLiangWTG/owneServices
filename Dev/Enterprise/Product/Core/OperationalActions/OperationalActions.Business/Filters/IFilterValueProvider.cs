namespace Enterprise.Services.OperationalActions.Business
{
	public interface IFilterValueProvider
	{
		/// <summary>
		/// Get the constraint with the given name.
		/// </summary>
		/// <param name="constraint">The name of the desired constraint.</param>
		/// <returns>The requested constraint or null if no such constraint exists.</returns>
		IFilterConstraint GetConstraint(string constraint);
	}
}
