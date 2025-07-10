using CargoWise.ActiveDirectory;
using CargoWise.Loader.Common.Native;

namespace CargoWise.Loader.Common
{
	public interface IServiceContainer
	{
		IDirectoryProxy Directory { get; }
		IEnvironmentProxy Environment { get; }
		IEventLogProxy EventLog { get; }
		IFileProxy File { get; }
		IMessageBoxProxy MessageBox { get; }
		INativeMethods NativeMethods { get; }
		IRegistryProxy Registry { get; }
		IFileVersionInfoProxy GetFileVersionInfo(string fileName);
		IServiceControllerProxy ServiceController { get; }
		ICargoWiseOneInstanceClass CargoWiseOneInstanceClass { get; }
	}
}
