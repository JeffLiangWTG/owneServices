using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.DocumentScanning.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.Records
{
	public partial class OfflineStorage : AutoOfflineStorage, IStmALogParent, IArchiveLogger
	{
		public OfflineStorage()
			: base(new BusinessObjectFactory())
		{ }

		public IArchiveLogger Logger { get; set; }
		public event EventHandler ArchivedOneUnit;
		public bool IsArchiving { get; private set; }
		public bool RequiredStorageCapacityCalculated { get; private set; }

		public override ZDateTime ArchiveDateTo
		{
			get
			{
				return base.ArchiveDateTo;
			}
			set
			{
				if (base.ArchiveDateTo != value)
				{
					RequiredStorageCapacityCalculated = false;
					base.ArchiveDateTo = value;

					if (value.IsValid)
					{
						CalculateRequiredStorageCapacity();
						UpdateConfirmationLabel();
					}
				}
			}
		}

		internal static class NativeMethods
		{
			[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);
		}

		ulong GetNetFolderFreeSpace(string path)
		{
			var success = NativeMethods.GetDiskFreeSpaceEx(path,
								  out _,
								  out _,
								  out var totalNumberOfFreeBytes);

			return success ? totalNumberOfFreeBytes : 0;
		}

		[MaxLength(Schema.OfflineLocationMaxLength)]
		public override ZString OfflineLocation
		{
			get
			{
				return base.OfflineLocation;
			}
			set
			{
				if (value != base.OfflineLocation)
				{
					base.OfflineLocation = value;
					base.FreeSpaceInMB = 0;

					if (!string.IsNullOrEmpty(value) && Directory.Exists(value) && new VersionHelper().IsWindowsVistaOrGreater())
					{
						DriveInfo drive = null;
						try
						{
							drive = new DriveInfo(value);
						}
						catch (ArgumentException)
						{
							// dont worry, let the validation show an error.
						}

						var availableFreeSpace = drive != null
							? drive.AvailableFreeSpace
							: (long)GetNetFolderFreeSpace(value);

						if (availableFreeSpace >= 0)
						{
							base.FreeSpaceInMB = Convert.ToInt32(Math.Ceiling(availableFreeSpace / 1024 / 1024 * 0.9));
						}
					}

					Validation.ValidateFreeSpaceInMB();
				}

				UpdateConfirmationLabel();
			}
		}

		public override ZInt FreeSpaceInMB
		{
			get
			{
				return base.FreeSpaceInMB;
			}
			set
			{
				base.FreeSpaceInMB = value;
				UpdateConfirmationLabel();
			}
		}

		static long BytesToMB(long bytes)
			=> (long)Utilities.Round(bytes / (ZDecimal)(1024 * 1024), 0);

		public void CalculateRequiredStorageCapacity()
		{
			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);

			var storageMainQuery = new ZQuery(StorageMainSchema.SM_Archived, SQLComparisonOperator.LessThanOrEqualTo, ArchiveDateTo);
			_ = storageMainQuery.AddToFilter(StorageMainSchema.SM_OffLine, null);

			var storageMains = documentFactory.Load<StorageMain>(storageMainQuery);
			var storageMainsCount = storageMains.Length;
			long totalBytes = 0;

			foreach (var storageMain in storageMains)
			{
				var storageDocs = storageMain.eDocs;
				var counts = storageDocs.Select(doc => doc.FileSizeInBytes);
				totalBytes += counts.Sum();
			}

			StorageMainRecordsForArchiving = storageMainsCount;
			StorageCapacityRequiredInMB = (ZInt)BytesToMB(totalBytes);
			RequiredStorageCapacityCalculated = true;
		}

		public void Archive()
		{
			archiveAbortRaised = false;

			if (StorageCapacityRequiredInMB <= 0)
			{
				LogError(descriptor.Code, Res.GetString("a92ea9d4-0610-449a-91e8-1d09bb551cd5", "There are no Archived Records to archive offline. Try choosing a later date."));
			}
			else
			{
				LogInfo(descriptor.Code, Res.GetString("0507f418-9069-4d9e-b2db-87bf1ae7c60d", "Starting to archive offline..."));
				IsArchiving = true;

				try
				{
					ProduceArchiveVolumes();

					foreach (var volumeZipPath in Directory.GetFiles(OfflineLocation, ArchiveVolume.VolumeFilePrefix + "*"))
					{
						LogFinalized(volumeZipPath);
					}

					if (archiveAbortRaised)
					{
						foreach (var volumePath in Directory.GetDirectories(OfflineLocation, ArchiveVolume.VolumeFilePrefix + "*"))
						{
							Directory.Delete(volumePath, recursive: true);
						}

						foreach (var volumeZipPaths in Directory.GetFiles(OfflineLocation, ArchiveVolume.VolumeFilePrefix + "*"))
						{
							File.Delete(volumeZipPaths);
						}

						LogInfo(descriptor.Code, Res.GetString("24206635-740c-4ddd-aeab-21843ca8396c", "Archiving aborted by user."));
					}
					else
					{
						LogInfo(descriptor.Code, Res.GetString("28f9fa72-efb5-48f6-8d5b-62d6708921ca", "Archiving completed."));
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					LogAndReportError("OfflineStorageError", descriptor.Code, Res.GetString("393780b1-a68d-443c-ac1a-0c78fb429790", "Error encountered. Archive process aborted.{0}{1}", System.Environment.NewLine, e.ToString()), e);
				}
				finally
				{
					IsArchiving = false;
				}
			}
		}

		public void AbortArchiving()
		{
			if (IsArchiving)
			{
				archiveAbortRaised = true;
			}
		}

		protected internal bool FreeSpaceInMB_ReadOnly
			=> true;

		bool archiveAbortRaised;

		void LogFinalized(string destinationFilePath)
		{
			LogInfo(descriptor.Code, Res.GetString("e7a15a86-4ba2-49d4-9bc9-4d7123e4b85f", "{0} finalized.", destinationFilePath));
		}

		readonly OfflineStorageSystemDescriptor descriptor = new();

		void ProduceArchiveVolumes()
		{
			var volumeZipPath = string.Empty;
			descriptor.VolumeManager = new ArchiveVolumeManager(OfflineLocation, FreeSpaceInMB);
			var system = new ArchiveSystem(descriptor);
			var config = new ArchiveConfiguration(ArchiveDateTo.Date, -1, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var stageEnumerator = system.GetArchiveStages(config).GetEnumerator();
			_ = stageEnumerator.MoveNext();

			var stage = stageEnumerator.Current;

			try
			{
				stage.BeginRun(config, null, this);
				LogInfo(descriptor.Code, Res.GetString("e7a5f67a-b37b-4b9d-b003-6a69ac1caa16", "Producing archive volume files to file path."));

				UnmarkStorageMains();
				int count;

				do
				{
					var archiveSets = stage.GetNextArchiveSet(null, null, stage);
					count = 0;

					foreach (var set in archiveSets)
					{
						var result = stage.ArchiveToImages(set);

						if (result.ErrorsEncountered.Count > 0)
						{
							LogError(descriptor.Code, Res.GetString("962105b6-6bc4-4f6d-9966-36ebb75f0ba9", "ERROR encountered while adding a record to the archive volume file. This record will be skipped."));
						}

						ArchivedOneUnit?.Invoke(this, EventArgs.Empty);

						if (archiveAbortRaised)
						{
							break;
						}

						if (string.IsNullOrEmpty(volumeZipPath))
						{
							volumeZipPath = descriptor.VolumeManager.CurrentVolume?.VolumeZipPath;
						}

						count++;
					}
				}
				while (count == stage.BatchSize);

				if (descriptor.VolumeManager.CurrentVolume != null && descriptor.VolumeManager.CurrentVolume.VolumeState == VolumeState.Open)
				{
					descriptor.VolumeManager.CurrentVolume.Close();
				}
			}
			finally
			{
				stage.EndRun();
				if (!archiveAbortRaised)
				{
					LogInfo(descriptor.Code, Res.GetString("499534a7-9594-4d15-a022-368c10eb2825", "Completed producing archive volume files in file path."));
				}
			}

			return;
		}

		void UnmarkStorageMains()
		{
			const string sql = @"UPDATE dbo.StorageMain SET SM_CD2 = 0 WHERE SM_CD2 = {0}";
			var formattedSql = string.Format(sql, int.MaxValue);
			_ = Db.Connection.ExecuteNonQuery(formattedSql);
		}

		void UpdateConfirmationLabel()
		{
			if (StorageCapacityRequiredInMB <= 0)
			{
				ConfirmationLabel = Res.GetString("1ad36e0a-9595-4203-9b04-88a5dcf0507c", "There are no archived records to archive offline.");
			}
			else if (string.IsNullOrEmpty(OfflineLocation) || FreeSpaceInMB <= 0)
			{
				ConfirmationLabel = Res.GetString("fd6d9a54-ad83-4bea-ab0e-52f33a844b5b", "File Path not specified or determined.");
			}
			else
			{
				var archiveDateToString = ArchiveDateTo.ToShortDateString();
				ConfirmationLabel = Res.GetString("778fe1b8-33db-49c5-9325-c1531c8c7352", "This archive process will copy all archived images for records archived on or before {0} to '{1}'. Once copied and verified, the process will remove the archived records from the SQL database and then repeat this process for the next archive volume file (if any), until all archive volume files are copied to the specified path.", archiveDateToString, OfflineLocation);
			}
		}

		#region IStmALogParent Members

		public BusinessObject[] BusinessObjectsWithRelatedEvents
			=> Array.Empty<BusinessObject>();

		public Logs Logs
			=> logs ?? (logs = new Logs(this));

		Logs logs;

		public BusinessObjectFactory LogsFactory
			=> Factory;

		public ZGuid LogsParentPK
			=> new("810FAC0E-0618-442E-9717-C3DC8EF9AC3F");

		public string LogsParentTableName
			=> (NoResString)"Archive";

		void IStmALogParent.ProcessLog(IStmALog log)
		{ }

		bool IStmALogParent.DeferFiringWorkflow
			=> false;

		#endregion

		#region IArchiveLogger Members

		public void LogAndReportError(string key, string systemCode, string message, Exception exception)
			=> Logger?.LogAndReportError(key, systemCode, TimeStampMessage(message), exception);

		public void LogError(string code, string message)
			=> Logger?.LogError(code, TimeStampMessage(message));

		public void LogError(string message)
			=> Logger?.LogError(TimeStampMessage(message));

		public void LogInfo(string code, string message)
			=> Logger?.LogInfo(code, TimeStampMessage(message));

		public void LogInfo(string message)
			=> Logger?.LogInfo(TimeStampMessage(message));

		public void LogWarning(string code, string message)
			=> Logger?.LogWarning(code, TimeStampMessage(message));

		public void LogWarning(string message)
			=> Logger?.LogWarning(TimeStampMessage(message));

		string TimeStampMessage(string message)
			=> ZDateTime.Now.ToLongTimeString() + ": " + message;

		#endregion
	}
}
