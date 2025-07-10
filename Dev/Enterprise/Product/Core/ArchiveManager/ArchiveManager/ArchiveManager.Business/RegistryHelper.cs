using System.Collections.Generic;
using System.Linq;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.ArchiveManager.Business
{
	public static class RegistryHelper
	{
		static Dictionary<string, string> Log
		{
			get => new()
			{
				{ SystemDataRegistry.Instance.BatchSizeControl.Name, $"Set Batch Size for Archiving and Purging Operational Jobs: {SystemDataRegistry.Instance.BatchSizeControl.Value}" },
				{ SystemDataRegistry.Instance.PurgeArchivedRecordsBatchSizeControl.Name, $"Set Batch Size: {SystemDataRegistry.Instance.PurgeArchivedRecordsBatchSizeControl.Value}" },
				{ SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum.Name, $"On or Before Minimum: {SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum.Value}" },
				{ SystemDataRegistry.Instance.InactiveOperationalJobsArchiveSystemOnOrBeforeMinimum.Name, $"On or Before Minimum: {SystemDataRegistry.Instance.InactiveOperationalJobsArchiveSystemOnOrBeforeMinimum.Value}" },
				{ SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum.Name, $"On or Before Minimum: {SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum.Value}" },
				{ SystemDataRegistry.Instance.PurgeArchivedRecordsOnOrBeforeMinimum.Name, $"On or Before Minimum: {SystemDataRegistry.Instance.PurgeArchivedRecordsOnOrBeforeMinimum.Value}" },
				{ SystemDataRegistry.Instance.StandaloneRecordsOnOrBeforeMinimum.Name, $"On or Before Minimum: {SystemDataRegistry.Instance.StandaloneRecordsOnOrBeforeMinimum.Value}" },
				{ SystemDataRegistry.Instance.StandaloneRecordsBatchSizeControl.Name, $"Set Batch Size: {SystemDataRegistry.Instance.StandaloneRecordsBatchSizeControl.Value}" },
				{ SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemOnOrBeforeMinimum.Name, $"On or Before Minimum: {SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemOnOrBeforeMinimum.Value}" },
				{ SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemBatchSizeControl.Name, $"Set Batch Size: {SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemBatchSizeControl.Value}" },
				{ SystemDataRegistry.Instance.HVLVArchiveSystemBatchSizeControl.Name, $"Set Batch Size: {SystemDataRegistry.Instance.HVLVArchiveSystemBatchSizeControl.Value}" },
				{ SystemDataRegistry.Instance.HVLVArchiveSystemOnOrBeforeMinimum.Name, $"On or Before Minimum: {SystemDataRegistry.Instance.HVLVArchiveSystemOnOrBeforeMinimum.Value}" },
				{ SystemDataRegistry.Instance.ExpiredRatesArchiveSystemOnOrBeforeMinimum.Name, $"On or Before Minimum: {SystemDataRegistry.Instance.ExpiredRatesArchiveSystemOnOrBeforeMinimum.Value}" },
				{ SystemDataRegistry.Instance.ExpiredRatesArchiveSystemBatchSizeControl.Name, $"Set Batch Size: {SystemDataRegistry.Instance.ExpiredRatesArchiveSystemBatchSizeControl.Value}" },
				{ SystemDataRegistry.Instance.ActivityLogsArchiveSystemOnOrBeforeMinimum.Name, $"On or Before Minimum: {SystemDataRegistry.Instance.ActivityLogsArchiveSystemOnOrBeforeMinimum.Value}" },
				{ SystemDataRegistry.Instance.ArchivingFileFormat.Name, $"Archiving File Format: {SystemDataRegistry.Instance.ArchivingFileFormat.Value}" },
			};
		}

		public static string ToLog(string name)
			=> Log.ContainsKey(name) ? Log[name] : $"Unhandled registry item: {name}";

		public static int GetOnOrBeforeMinimumValue(string code)
		{
			switch (code)
			{
				case ArchiveManagerConstants.Codes.OPS:
					return SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum.Value;

				case ArchiveManagerConstants.Codes.IPS:
					return SystemDataRegistry.Instance.InactiveOperationalJobsArchiveSystemOnOrBeforeMinimum.Value;

				case ArchiveManagerConstants.Codes.STA:
					return SystemDataRegistry.Instance.StandaloneRecordsOnOrBeforeMinimum.Value;

				case ArchiveManagerConstants.Codes.PDR:
					return SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum.Value;

				case ArchiveManagerConstants.Codes.PAR:
					return SystemDataRegistry.Instance.PurgeArchivedRecordsOnOrBeforeMinimum.Value;

				case ArchiveManagerConstants.Codes.PDO:
					return SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemOnOrBeforeMinimum.Value;

				case ArchiveManagerConstants.Codes.HAR:
					return SystemDataRegistry.Instance.HVLVArchiveSystemOnOrBeforeMinimum.Value;

				case ArchiveManagerConstants.Codes.RED:
					return SystemDataRegistry.Instance.ExpiredRatesArchiveSystemOnOrBeforeMinimum.Value;

				case ArchiveManagerConstants.Codes.PAL:
					return SystemDataRegistry.Instance.ActivityLogsArchiveSystemOnOrBeforeMinimum.Value;

				default:
					var systemDescriptor = new ArchiveSystemDescriptorLoader().Load()
						.SingleOrDefault(d => d.Code == code);

					return systemDescriptor is ISelfContainedArchiveSystemDescriptor selfContainedDescriptor
						? selfContainedDescriptor.OnOrBeforeMinimumValue
						: ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default;
			}
		}

		public static int GetBatchSizeValue(string code)
		{
			switch (code)
			{
				case ArchiveManagerConstants.Codes.OPS:
				case ArchiveManagerConstants.Codes.IPS:
				case ArchiveManagerConstants.Codes.PDR:
					return SystemDataRegistry.Instance.BatchSizeControl.Value;

				case ArchiveManagerConstants.Codes.STA:
					return SystemDataRegistry.Instance.StandaloneRecordsBatchSizeControl.Value;

				case ArchiveManagerConstants.Codes.PAR:
					return SystemDataRegistry.Instance.PurgeArchivedRecordsBatchSizeControl.Value;

				case ArchiveManagerConstants.Codes.PDO:
					return SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemBatchSizeControl.Value;

				case ArchiveManagerConstants.Codes.HAR:
					return SystemDataRegistry.Instance.HVLVArchiveSystemBatchSizeControl.Value;

				case ArchiveManagerConstants.Codes.RED:
					return SystemDataRegistry.Instance.ExpiredRatesArchiveSystemBatchSizeControl.Value;

				default:
					var systemDescriptor = new ArchiveSystemDescriptorLoader().Load()
						.SingleOrDefault(d => d.Code == code);

					return systemDescriptor is ISelfContainedArchiveSystemDescriptor selfContainedDescriptor
						? selfContainedDescriptor.BatchSizeControlValue
						: 50;
			}
		}
	}
}
