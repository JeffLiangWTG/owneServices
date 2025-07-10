namespace Enterprise.Integration
{
	/// <summary>
	/// Macros have been able to access public properties on business objects.
	/// This design decision means we have many strings in the database with property names from any number of business object public proterties.
	/// Any refractors/renames/just coding/changing a property from a concrete type to an interface can mean these strings may no longer match a public property.
	/// This interface allows for backward compatability when changing a type to an inteface.
	/// If the choosen interface does not have a property the macro evaluator will look for the property on the concrete type.
	/// These properties won't be exposed in the Data Field Map but will still work when evaluating for those who used them in the past.
	/// </summary>
	public interface IAllowMacroAccessToAllPublicProperties
	{
	}
}
