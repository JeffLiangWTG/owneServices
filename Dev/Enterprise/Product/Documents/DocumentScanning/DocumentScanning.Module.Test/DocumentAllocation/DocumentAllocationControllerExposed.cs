using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentScanning.Module.Testing
{
	internal sealed class DocumentAllocationControllerExposed : DocumentAllocationController
	{
		public ODisplayMode GetDisplayModeForNewExposed()
		{
			return base.GetDisplayModeForNew();
		}
	}
}
