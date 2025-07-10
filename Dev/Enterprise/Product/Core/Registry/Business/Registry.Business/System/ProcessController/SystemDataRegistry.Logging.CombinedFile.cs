using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	partial class SystemDataRegistry
	{
		public IntRegistryItem ProcessControllerCombinedFileRetentionPeriod =>
			GetItem("ProcessControllerCombinedFileRetentionPeriod", () => new IntRegistryItem(
				"ProcessControllerCombinedFileRetentionPeriod",
				Categories.System_ProcessController_Logging_CombinedFileSystem,
				ResString.GetMultilingualString("ABDF8021-AB66-474A-A1F1-7857DE9E627B", "Retention period"),
				ResString.GetMultilingualString("E8A46517-2E65-4326-8834-7BCF2D14F836", "Number of days to keep combined log files, older files are deleted from the file system."),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				15,
				1,
				60));
	}
}
