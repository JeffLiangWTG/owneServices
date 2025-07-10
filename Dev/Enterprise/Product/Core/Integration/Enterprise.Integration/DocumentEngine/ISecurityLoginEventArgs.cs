using CargoWise.Types;

namespace Enterprise.Integration.DocumentEngine
{
	public interface ISecurityLoginEventArgs
	{
		bool IsAllowedToProceed { get; set; }

		ZString AuthorisingStaffLogin { get; set; }
	}
}
