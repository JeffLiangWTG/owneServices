using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public class EmptyActionResult : INetworkActionResult
	{
		bool INetworkActionResult.IsHandledByVisualiser
		{
			get { return false; }
		}
	}
}
