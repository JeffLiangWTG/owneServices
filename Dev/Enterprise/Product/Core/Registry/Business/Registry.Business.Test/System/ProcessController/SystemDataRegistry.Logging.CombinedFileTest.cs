using Enterprise.Integration;

namespace Enterprise.Registry.Business.Testing
{
	sealed partial class SystemDataRegistryTest
	{
		public void TestProcessControllerCombinedFileRetentionPeriod()
		{
			TestGenericRegistryItem(
				ItemSet.ProcessControllerCombinedFileRetentionPeriod,
				"ProcessControllerCombinedFileRetentionPeriod",
				SystemDataRegistry.Categories.System_ProcessController_Logging_CombinedFileSystem,
				"Retention period",
				"Number of days to keep combined log files, older files are deleted from the file system.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				15);
		}
	}
}
