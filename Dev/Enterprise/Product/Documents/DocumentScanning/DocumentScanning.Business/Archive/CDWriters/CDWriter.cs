using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
	public abstract class CDWriter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public event StringEventHandler CurrentDriveNotSet;
		public event StringEventHandler CDWriterSoftwareNotInitialised;
		public event NumberEventHandler ProgressMade;
		public event EventHandler StatusUpdated;
		public event EventHandler BurnSuccessful;
		public event StringEventHandler BurnFailed;
		public event EventHandler CDDriveEmpty;
		public event EventHandler CDDriveInUse;

		public CDWriter(DocumentFactory factory, ArchiveEDocsManager archiver)
			: base(factory)
		{
			fArchiveManager = archiver;
		}

		#region ArchiveEDocsManager

		public ArchiveEDocsManager ArchiveManager
		{
			get { return fArchiveManager; }
		}

		readonly ArchiveEDocsManager fArchiveManager;

		#endregion

		#region MasterFactory

		public DocumentFactory MasterFactory
		{
			get { return (DocumentFactory)Factory; }
		}

		#endregion

		#region Percentage

		#region Percent

		public ZInt Percent
		{
			get { return fPercent; }
			set { fPercent = value; }
		}

		ZInt fPercent;

		#endregion

		protected void SetProgress(int percentageToSet)
		{
			Percent = percentageToSet;
			OnProgressMade(percentageToSet);
		}

		#endregion

		#region Phasing

		#region CurrentPhase

		[CargoWise.ComponentModel.MaxLength(100)]
		public ZString CurrentPhase
		{
			get { return fCurrentPhase; }
			set
			{
				if (fCurrentPhase != value)
				{
					CheckMaximumLength(CurrentPhaseInfo, value);
					fCurrentPhase = value;
					CurrentPhaseInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo CurrentPhaseInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentPhase)); }
		}

		ZString fCurrentPhase;

		#endregion

		protected void SetPhase(string phase)
		{
			CurrentPhase = phase;
			CurrentPhaseInfo.RefreshBinding();
			OnStatusUpdated();
		}

		#endregion

		#region Log

		#region SessionLog

		[MaxLength(1000)]
		[ReadOnly(true)]
		public ZString SessionLog
		{
			get { return fSessionLog; }
			set
			{
				if (fSessionLog != value)
				{
					CheckMaximumLength(SessionLogInfo, value);
					fSessionLog = value;
					SessionLogInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo SessionLogInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(SessionLog));
			}
		}

		ZString fSessionLog;

		#endregion

		protected void AddLogLine(string lineToAdd)
		{
			SessionLog += lineToAdd.Replace('\n'.ToString(), System.Environment.NewLine) + System.Environment.NewLine;
			SessionLogInfo.RefreshBinding();
			OnStatusUpdated();
		}

		#endregion

		#region CDVolumeLabel

		[CargoWise.ComponentModel.MaxLength(16)]
		[ReadOnlyMember(nameof(IsBurnt))]
		public ZString CDVolumeLabel
		{
			get
			{
				return fCDVolumeLabel;
			}
			set
			{
				CheckMaximumLength(CDVolumeLabelInfo, value);
				fCDVolumeLabel = value;
				CDVolumeLabelInfo.RefreshBinding();
			}
		}

		ZString fCDVolumeLabel;

		public ZPropertyInfo CDVolumeLabelInfo
		{
			get { return GetZPropertyInfo(nameof(CDVolumeLabel)); }
		}

		#endregion

		#region IsUserBurn

		public virtual ZBool IsUserControlled
		{
			get { return false; }
		}

		#endregion

		#region CDArchiveTempDirectory

		public ZString CDArchiveTempDirectory
		{
			get { return Path.Combine(Temp.TempPath, "CDArchive"); }
		}

		#endregion

		public virtual ZPropertyInfo CurrentWriteSpeedInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentWriteSpeed)); }
		}

		public virtual ZPropertyInfo CurrentDriveInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentDrive)); }
		}

		public virtual void Burn()
		{
			IsBurnt = true;
			SaveFilesToFilesystemAndCreateIndex();
			CreateAutorunFile();
		}

		public bool IsBurnt
		{
			get;
			private set;
		}

		public virtual void CancelBurn()
		{
			ClearTempDirectory();
		}

		[CargoWise.ComponentModel.MaxLength(30)]
		[ReadOnlyMember(nameof(IsBurnt))]
		public abstract ZString CurrentDrive { get; set; }
		public abstract CodeDescriptionPairList DriveList { get; }

		[CargoWise.ComponentModel.MaxLength(5)]
		[ReadOnlyMember(nameof(IsBurnt))]
		public abstract ZString CurrentWriteSpeed { get; set; }
		public abstract CodeDescriptionPairList WriteSpeedList { get; }

		#region IndexFile

		protected string IndexFileName
		{
			get { return "index.xls"; }
		}

		#endregion

		protected void CreateAutorunFile()
		{
			string autorunContents = (NoResString)@"[autorun]
ShellExecute=" + IndexFileName;
			StreamWriter writer = new StreamWriter(Path.Combine(CDArchiveTempDirectory, "autorun.inf"));

			try
			{
				writer.Write(autorunContents);
			}
			finally
			{
				writer.Close();
			}
		}

		#region Events

		protected void OnStatusUpdated()
		{
			if (StatusUpdated != null)
			{
				StatusUpdated(this, EventArgs.Empty);
			}
		}

		protected void OnCDDriveEmpty()
		{
			if (CDDriveEmpty != null)
			{
				CDDriveEmpty(this, EventArgs.Empty);
			}
		}

		protected void OnBurnSuccessful()
		{
			if (BurnSuccessful != null)
			{
				BurnSuccessful(this, EventArgs.Empty);
			}
		}

		protected void OnBurnFailed(string message)
		{
			if (BurnFailed != null)
			{
				BurnFailed(this, new StringEventArgs(message));
			}
		}

		protected void OnCurrentDriveNotSet(string errorMessage)
		{
			if (CurrentDriveNotSet != null)
			{
				CurrentDriveNotSet(this, new StringEventArgs(errorMessage));
			}
		}

		protected void OnCDWriterSoftwareNotInitialised(string errorMessage)
		{
			if (CDWriterSoftwareNotInitialised != null)
			{
				CDWriterSoftwareNotInitialised(this, new StringEventArgs(errorMessage));
			}
		}

		protected void OnProgressMade(int percentageToSet)
		{
			if (ProgressMade != null)
			{
				ProgressMade(this, new NumberEventArgs(percentageToSet));
			}
		}

		protected void OnCDDriveInUse()
		{
			if (CDDriveInUse != null)
			{
				CDDriveInUse(this, EventArgs.Empty);
			}
		}

		#endregion

		protected virtual void ClearTempDirectory()
		{
			if (Directory.Exists(CDArchiveTempDirectory))
			{
				try
				{
					Directory.Delete(CDArchiveTempDirectory, true);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					// don't mind if this dies
				}
			}
		}
#if DEBUG
		public
#else
		internal
#endif
		string CreateFileName(int prefix, string existingPrefix, string tempFileDirectory, StorageDocsBase document)
		{
			string description = document.SC_FileName.IsEmpty ? document.SC_DescMultilingual : document.SC_FileName;

			string tempFilenameBeforeFileType = existingPrefix + (char)prefix + "_[" + document.SC_DocType + "] " + DocumentUtilities.RemoveIllegalCharacters(description);
			tempFilenameBeforeFileType = tempFilenameBeforeFileType.Substring(0, Math.Min(tempFilenameBeforeFileType.Length, 250 - tempFileDirectory.Length));
			return tempFilenameBeforeFileType + "." + document.SC_DataType;
		}

		/// <summary>
		/// Copies all the documents to the filesystem to burn to CD.
		/// Assigns a unique prefix to all the filenames so that they don't clash on the CD.
		/// Prefix must start with a letter to satisfy file naming conventions
		/// Also builds up the index file for the CD at the same time
		/// </summary>
#if DEBUG
		public
#else
		internal
#endif
		void SaveFilesToFilesystemAndCreateIndex()
		{
			ClearTempDirectory();
			Directory.CreateDirectory(CDArchiveTempDirectory);

			ExcelIndexFile indexFile = new ExcelIndexFile(Path.Combine(CDArchiveTempDirectory, IndexFileName));
			try
			{
				indexFile.Start();

				foreach (StorageMain header in ArchiveManager.ListToArchive)
				{
					var documentsToBurn = header.PublishedEDocsAndFiles;

					if (documentsToBurn.Count > 0)
					{
						var documentOwnerDescription = PathValidation.GetSafeFilename(header.DocumentOwnerDescription);
						indexFile.AddDirectory(documentOwnerDescription);
						string relativeDirectoryPath = documentOwnerDescription + Path.DirectorySeparatorChar;
						string tempFileDirectory = Path.Combine(CDArchiveTempDirectory, relativeDirectoryPath);

						if (!Directory.Exists(tempFileDirectory))
						{
							Directory.CreateDirectory(tempFileDirectory);
						}

						int prefix = 65; // using capitals A-Z - systems may not let you save numbers as the start of filenames
						string existingPrefix = ZString.Empty;

						documentsToBurn.Sort(StorageDocsSchema.SC_DocType.Name, ListSortDirection.Ascending);
						for (int i = documentsToBurn.Count - 1; i >= 0; i--)
						{
							StorageDocsBase document = documentsToBurn[i];

							string tempFilename = PathValidation.GetSafeFilename(CreateFileName(prefix, existingPrefix, tempFileDirectory, document));

							(document.GetDocumentInNewFactory() ?? document).SaveToTempFile(tempFileDirectory + tempFilename);

							indexFile.AddFile(document, relativeDirectoryPath, tempFilename);

							if (prefix < 90) // haven't reached Z
							{
								prefix++;
							}
							else // reset back to A and add an extra letter on to the start of the prefix so you get X, Y, Z, AA, AB, etc.
							{
								existingPrefix = existingPrefix + "A";
								prefix = 65;
							}
						}

						indexFile.CloseDirectory();
					}
				}

				indexFile.Finish();
			}
			finally
			{
				GCWrapper.ReclaimMemory<ExcelIndexFile>(ref indexFile);
			}
		}

		protected Hashtable DriveMapping = new Hashtable();
#if DEBUG
		public bool IsTesting;
#endif
	}
}
