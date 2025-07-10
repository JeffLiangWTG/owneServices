using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentScanning.Business
{
	public class DocumentScanningModuleInitialiser : IModuleInitialisedAtDATStartup
	{
		public void Initialise()
		{
			if (AssemblyDataLookup.AllAssemblyData == null)
			{
				Globals.Message.ShowDeveloperErrorAlways("Error Loading DocumentScanning Assembly Data.", "DocumentScanning");
			}
		}
	}
}
