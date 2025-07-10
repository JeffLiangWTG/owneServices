using CargoWise.Common.MemoryManagement;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Startup
{
	public class StartupInitEnableMemoryManager : InitializingApplicationStartupTask
	{
		public override string TaskDescription => (NoResString)"Enable Memory Manager";

		public override int FailureExitCode => ExitCodes.StartupInitEnableMemoryManagerError;

		protected override bool DoExecute(CommandLineArguments arguments)
		{
			MemoryManager.Enable();
			return true;
		}
	}

	class StartupInitDisableMemoryManager : IPostLoginTask
	{
		public string TaskDescription
		{
			get { return (NoResString)"Disable Memory Manager"; }
		}

		public bool ShouldExecute()
		{
			return DataRegistry.Instance.MegabytesOfManagedMemoryBeforeAutomaticCollection == 0;
		}

		public void Execute()
		{
			MemoryManager.Disable();
		}
	}
}
