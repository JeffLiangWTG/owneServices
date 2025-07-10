namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Implement this on a FindBox Lookup List. Any New/Edit forms from the FindBox
	/// will call ValidateEntityOnSaving as part of their save process, allowing 
	/// you to provide validation on the top level object of the New/Edit form.
	/// </summary>
	public interface IValidateForController
	{
		void ValidateEntityOnSaving(IBusiness entity);
	}
}
