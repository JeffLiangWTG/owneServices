using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA
{
	class UpdateRegistryKeyItemsFromB3ToEntry : RegistryDataTransformation
	{
		public override string UserDescription => "Update Customs Registry Key Items From B3 to Entry";

		protected override void OfflinePostUpgradeTransform()
		{
			UpdateRegistryItemName("B3StatementDateThreshold", "EntryStatementDateThreshold");
			UpdateRegistryItemName("B3AutomaticSendingDelayThresholds", "EntryAutomaticSendingDelayThresholds");
			UpdateRegistryItemName("B3LateSendingFailsafeWarningThresholds", "EntryLateSendingFailsafeWarningThresholds");
			UpdateRegistryItemName("DefaultDeferredLowValueB3SendAction", "DefaultDeferredLowValueEntrySendAction");
			UpdateRegistryItemName("DefaultDeferredNormalB3SendAction", "DefaultDeferredNormalEntrySendAction");
			UpdateRegistryItemName("ACROSSHighValueProductAudit", "ReleaseHighValueProductAudit");
			UpdateRegistryItemName("ACROSSLowValueProductAudit", "ReleaseLowValueProductAudit");
			UpdateRegistryItemName("B3HighValueProductAudit", "EntryHighValueProductAudit");
			UpdateRegistryItemName("B3LowValueProductAudit", "EntryLowValueProductAudit");
		}
	}
}
