namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Implement this on a FindBox Lookup List. 
	/// Allows you to set default filter criteria displayed by the findbox module.
	/// Eg, Tick consignor box on Organisation findbox.
	/// </summary>
	public interface IFilterBusinessObjectDefaultsProvider
	{
		FilterBusinessObjectDefaults FilterBusinessObjectDefaults { get; }
	}
}
