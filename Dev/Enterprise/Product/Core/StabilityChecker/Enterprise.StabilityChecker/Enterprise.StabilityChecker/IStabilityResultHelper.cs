namespace Enterprise.StabilityChecker
{
	/// <summary>
	/// Provides the callback target from a stability result, to give the user additional help to resolve the issue. 
	/// For example, the appropriate configuration or diagnostics window can be opened.
	/// </summary>
	public interface IStabilityResultHelper
	{
		void HelpUser(StabilityResult result);
	}
}
