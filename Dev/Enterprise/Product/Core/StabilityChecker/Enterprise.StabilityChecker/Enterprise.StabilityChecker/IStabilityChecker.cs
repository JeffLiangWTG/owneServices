
namespace Enterprise.StabilityChecker
{
	/// <summary>
	/// This interface is implemented by classes that test the stability of certain aspects of the system.
	/// </summary>
	public interface IStabilityChecker
	{
		/// <summary>
		/// If the check is non applicable to a system, return null.
		/// </summary>
		/// <returns>Returns zero or more StabilityResult objects showing the stability of an aspect of a system.</returns>
		StabilityResult[] Check();
	}
}
