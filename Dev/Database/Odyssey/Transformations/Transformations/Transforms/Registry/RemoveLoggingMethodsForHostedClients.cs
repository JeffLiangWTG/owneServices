using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Registry
{
	public class RemoveLoggingMethodsForHostedClients : RegistryDataTransformation
	{
		public override string UserDescription => "Remove LoggingMethods registry item for hosted clients";

		protected override void OfflinePostUpgradeTransform()
		{
			if (manager.IsHosted || (manager.IsInternalSystem ?? false))
			{
				DeleteRegistryItemRows("LoggingMethods");
			}
			else
			{
				ShowInfo("Client is not hosted, skipping.");
			}
		}
	}
}
