using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ArchiveManager.Business;
using Enterprise.Registry.Business;

namespace Enterprise.ArchiveManager.Test
{
	public class RegistryHelperTest : TestCaseWithFactory
	{
		#region ToLog

		public void TestToLog_ForBatchSizeControl()
		{
			var registryLogLine = RegistryHelper.ToLog(SystemDataRegistry.Instance.BatchSizeControl.Name);

			AssertEquals($"Set Batch Size for Archiving and Purging Operational Jobs: {SystemDataRegistry.Instance.BatchSizeControl.Value}", registryLogLine);
		}

		public void TestToLog_ForPurgeArchivedRecordsBatchSizeControl()
		{
			var registryLogLine = RegistryHelper.ToLog(SystemDataRegistry.Instance.PurgeArchivedRecordsBatchSizeControl.Name);

			AssertEquals($"Set Batch Size: {SystemDataRegistry.Instance.PurgeArchivedRecordsBatchSizeControl.Value}", registryLogLine);
		}

		public void TestToLog_ForArchiveRecordsOnOrBeforeMinimum()
		{
			var registryLogLine = RegistryHelper.ToLog(SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum.Name);

			AssertEquals($"On or Before Minimum: {SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum.Value}", registryLogLine);
		}

		public void TestToLog_ForInactiveOperationalJobsArchiveSystemOnOrBeforeMinimum()
		{
			var registryLogLine = RegistryHelper.ToLog(SystemDataRegistry.Instance.InactiveOperationalJobsArchiveSystemOnOrBeforeMinimum.Name);

			AssertEquals($"On or Before Minimum: {SystemDataRegistry.Instance.InactiveOperationalJobsArchiveSystemOnOrBeforeMinimum.Value}", registryLogLine);
		}

		public void TestToLog_ForPurgeDocumentsAndRecordsOnOrBeforeMinimum()
		{
			var registryLogLine = RegistryHelper.ToLog(SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum.Name);

			AssertEquals($"On or Before Minimum: {SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum.Value}", registryLogLine);
		}

		public void TestToLog_ForPurgeArchivedRecordsOnOrBeforeMinimum()
		{
			var registryLogLine = RegistryHelper.ToLog(SystemDataRegistry.Instance.PurgeArchivedRecordsOnOrBeforeMinimum.Name);

			AssertEquals($"On or Before Minimum: {SystemDataRegistry.Instance.PurgeArchivedRecordsOnOrBeforeMinimum.Value}", registryLogLine);
		}

		public void TestToLog_ForStandaloneRecordsOnOrBeforeMinimum()
		{
			var registryLogLine = RegistryHelper.ToLog(SystemDataRegistry.Instance.StandaloneRecordsOnOrBeforeMinimum.Name);

			AssertEquals($"On or Before Minimum: {SystemDataRegistry.Instance.StandaloneRecordsOnOrBeforeMinimum.Value}", registryLogLine);
		}

		public void TestToLog_ForStandaloneRecordsBatchSizeControl()
		{
			var registryLogLine = RegistryHelper.ToLog(SystemDataRegistry.Instance.StandaloneRecordsBatchSizeControl.Name);

			AssertEquals($"Set Batch Size: {SystemDataRegistry.Instance.StandaloneRecordsBatchSizeControl.Value}", registryLogLine);
		}

		public void TestToLog_ForPurgeDocumentsOfOperationalRecordsOnOrBeforeMinimum()
		{
			var registryLogLine = RegistryHelper.ToLog(SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemOnOrBeforeMinimum.Name);

			AssertEquals("On or Before Minimum: 10", registryLogLine);
		}

		public void TestToLog_ForPurgeDocumentsOfOperationalRecordsBatchSizeControl()
		{
			var registryLogLine = RegistryHelper.ToLog(SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemBatchSizeControl.Name);

			AssertEquals("Set Batch Size: 50", registryLogLine);
		}

		public void TestToLog_ForHVLVArchiveSystemBatchSizeControl()
		{
			var registryLogLine = RegistryHelper.ToLog(SystemDataRegistry.Instance.HVLVArchiveSystemBatchSizeControl.Name);

			AssertEquals("Set Batch Size: 100", registryLogLine);
		}

		public void TestToLog_ForHVLVArchiveSystemOnOrBeforeMinimum()
		{
			var registryLogLine = RegistryHelper.ToLog(SystemDataRegistry.Instance.HVLVArchiveSystemOnOrBeforeMinimum.Name);

			AssertEquals("On or Before Minimum: 1", registryLogLine);
		}

		public void TestToLog_ForArchivingFileFormat()
		{
			var registryLogLine = RegistryHelper.ToLog(SystemDataRegistry.Instance.ArchivingFileFormat.Name);

			AssertEquals("Archiving File Format: CSV", registryLogLine);
		}

		public void TestToLog_WhenRegistyItemIsUnhandled()
		{
			var unhandledRegistryItem = "SetNumberOfCats";
			var registryLogLine = RegistryHelper.ToLog(unhandledRegistryItem);

			AssertEquals($"Unhandled registry item: {unhandledRegistryItem}", registryLogLine);
		}

		#endregion

		#region GetOnOrBeforeMinimum

		public void TestGetOnOrBeforeMinimumValue_Default()
		{
			var defaultValue = 7;
			var onOrBeforeMinimum = RegistryHelper.GetOnOrBeforeMinimumValue("CAT");

			AssertEquals($"Unhandled archive system code should default to: '{defaultValue}'.", defaultValue, onOrBeforeMinimum);
		}

		public void TestGetOnOrBeforeMinimumValue_OPS()
		{
			var expectedValue = 33;
			using (SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedValue))
			{
				var onOrBeforeMinimum = RegistryHelper.GetOnOrBeforeMinimumValue(ArchiveManagerConstants.Codes.OPS);
				AssertEquals($"OPS OnOrBeforeMinimum value should be: '{expectedValue}'.", expectedValue, onOrBeforeMinimum);
			}
		}

		public void TestGetOnOrBeforeMinimumValue_IPS()
		{
			var expectedValue = 44;
			using (SystemDataRegistry.Instance.InactiveOperationalJobsArchiveSystemOnOrBeforeMinimum.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedValue))
			{
				var onOrBeforeMinimum = RegistryHelper.GetOnOrBeforeMinimumValue(ArchiveManagerConstants.Codes.IPS);
				AssertEquals($"IPS OnOrBeforeMinimum value should be: '{expectedValue}'.", expectedValue, onOrBeforeMinimum);
			}
		}

		public void TestGetOnOrBeforeMinimumValue_STA()
		{
			var expectedValue = 55;
			using (SystemDataRegistry.Instance.StandaloneRecordsOnOrBeforeMinimum.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedValue))
			{
				var onOrBeforeMinimum = RegistryHelper.GetOnOrBeforeMinimumValue(ArchiveManagerConstants.Codes.STA);
				AssertEquals($"STA OnOrBeforeMinimum value should be: '{expectedValue}'.", expectedValue, onOrBeforeMinimum);
			}
		}

		public void TestGetOnOrBeforeMinimumValue_PDR()
		{
			var expectedValue = 66;
			using (SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedValue))
			{
				var onOrBeforeMinimum = RegistryHelper.GetOnOrBeforeMinimumValue(ArchiveManagerConstants.Codes.PDR);

				AssertEquals($"PDR OnOrBeforeMinimum value should be: '{expectedValue}'.", expectedValue, onOrBeforeMinimum);
			}
		}

		public void TestGetOnOrBeforeMinimumValue_PAR()
		{
			var expectedValue = 77;
			using (SystemDataRegistry.Instance.PurgeArchivedRecordsOnOrBeforeMinimum.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedValue))
			{
				var onOrBeforeMinimum = RegistryHelper.GetOnOrBeforeMinimumValue(ArchiveManagerConstants.Codes.PAR);

				AssertEquals($"PAR OnOrBeforeMinimum value should be: '{expectedValue}'.", expectedValue, onOrBeforeMinimum);
			}
		}

		public void TestGetOnOrBeforeMinimumValue_PDO()
		{
			const int expectedValue = 88;
			using (SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemOnOrBeforeMinimum.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedValue))
			{
				var onOrBeforeMinimum = RegistryHelper.GetOnOrBeforeMinimumValue(ArchiveManagerConstants.Codes.PDO);

				AssertEquals($"PDO OnOrBeforeMinimum value should be: '{expectedValue}'.", expectedValue, onOrBeforeMinimum);
			}
		}

		public void TestGetOnOrBeforeMinimumValue_HAR()
		{
			const int expectedValue = 99;
			using (SystemDataRegistry.Instance.HVLVArchiveSystemOnOrBeforeMinimum.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedValue))
			{
				var onOrBeforeMinimum = RegistryHelper.GetOnOrBeforeMinimumValue(ArchiveManagerConstants.Codes.HAR);

				AssertEquals($"HAR OnOrBeforeMinimum value should be: '{expectedValue}'.", expectedValue, onOrBeforeMinimum);
			}
		}

		#endregion

		#region GetBatchSize

		public void TestGetBatchSize_Default()
		{
			var defaultValue = 50;
			var batchSize = RegistryHelper.GetBatchSizeValue("CAT");

			AssertEquals($"Unhandled archive system code should default to: '{defaultValue}'.", defaultValue, batchSize);
		}

		public void TestGetBatchSize_OPS()
		{
			var expectedValue = 60;
			using (SystemDataRegistry.Instance.BatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedValue))
			{
				var batchSize = RegistryHelper.GetBatchSizeValue(ArchiveManagerConstants.Codes.OPS);
				AssertEquals($"OPS BatchSizeControl value should be: '{expectedValue}'.", expectedValue, batchSize);
			}
		}

		public void TestGetBatchSize_IPS()
		{
			var expectedValue = 44;
			using (SystemDataRegistry.Instance.BatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedValue))
			{
				var batchSize = RegistryHelper.GetBatchSizeValue(ArchiveManagerConstants.Codes.IPS);
				AssertEquals($"IPS BatchSizeControl value should be: '{expectedValue}'.", expectedValue, batchSize);
			}
		}

		public void TestGetBatchSize_STA()
		{
			var expectedValue = 55;
			using (SystemDataRegistry.Instance.StandaloneRecordsBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedValue))
			{
				var batchSize = RegistryHelper.GetBatchSizeValue(ArchiveManagerConstants.Codes.STA);
				AssertEquals($"STA BatchSizeControl value should be: '{expectedValue}'.", expectedValue, batchSize);
			}
		}

		public void TestGetBatchSize_PDR()
		{
			var expectedValue = 66;
			using (SystemDataRegistry.Instance.BatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedValue))
			{
				var batchSize = RegistryHelper.GetBatchSizeValue(ArchiveManagerConstants.Codes.PDR);

				AssertEquals($"PDR BatchSizeControl value should be: '{expectedValue}'.", expectedValue, batchSize);
			}
		}

		public void TestGetBatchSize_PAR()
		{
			var expectedValue = 77;
			using (SystemDataRegistry.Instance.PurgeArchivedRecordsBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedValue))
			{
				var batchSize = RegistryHelper.GetBatchSizeValue(ArchiveManagerConstants.Codes.PAR);

				AssertEquals($"PAR BatchSizeControl value should be: '{expectedValue}'.", expectedValue, batchSize);
			}
		}

		public void TestGetBatchSize_PDO()
		{
			const int expectedValue = 88;
			using (SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedValue))
			{
				var batchSize = RegistryHelper.GetBatchSizeValue(ArchiveManagerConstants.Codes.PDO);

				AssertEquals($"PDO BatchSizeControl value should be: '{expectedValue}'.", expectedValue, batchSize);
			}
		}

		public void TestGetBatchSize_HAR()
		{
			const int expectedValue = 99;
			using (SystemDataRegistry.Instance.HVLVArchiveSystemBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedValue))
			{
				var batchSize = RegistryHelper.GetBatchSizeValue(ArchiveManagerConstants.Codes.HAR);

				AssertEquals($"HAR BatchSizeControl value should be: '{expectedValue}'.", expectedValue, batchSize);
			}
		}

		#endregion
	}
}
