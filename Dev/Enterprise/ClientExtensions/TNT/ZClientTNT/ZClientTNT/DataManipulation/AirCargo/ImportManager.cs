using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.TNT.AirCargo
{
	public abstract class ImportManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ImportManager(BusinessObjectFactory factory, ZString filename)
			: base(factory)
		{
			RecordFactory = new IQDownRecordFactory();

			if (filename.IsEmpty)
			{
				throw new ArgumentException("Filename");
			}
			ImportFile = new FileInfo(filename);
			BranchCode = new ZString(ImportFile.Name.Trim().ToUpper()).Left(3).PadLeft(3);
		}

		#region MoveFileToProcessedDirectory

		public void MoveFileToProcessedDirectory()
		{
			FileMover.Move(new FileInfo(FileFullName));
		}

		#region FileMover

		protected ProcessedFileMover FileMover
		{
			get
			{
				if (fFileMover == null)
				{
					fFileMover = new ProcessedFileMover(ProcessedDirectory);
				}
				return fFileMover;
			}
		}

		ProcessedFileMover fFileMover;

		#endregion

		protected abstract string ProcessedDirectory
		{ get; }

		#endregion

		#region FileFullName

		public ZString FileFullName
		{
			get { return ImportFile.FullName; }
		}

		public ZPropertyInfo FileFullNameInfo
		{
			get { return GetZPropertyInfo(nameof(FileFullName)); }
		}

		#endregion

		#region RefUNLOCO_List

		public RefUNLOCOCollection RefUNLOCO_List
		{
			get { return BindingLists.RefUNLOCO_List; }
		}

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#endregion

		#region LoadFromFile

		public event TNTProgressEventHandler LoadProgress;

		public void LoadFromFile(INotifications notify)
		{
			NotificationBuffer buffer = new NotificationBuffer(notify);

			string[] fileLines = ExtractLinesFromFile(buffer);

			if (fileLines != null)
			{
				if (IsOkToLoadFromFileLines(fileLines, buffer))
				{
					LoadFromFileLines(fileLines, buffer);
				}
			}
		}

		string[] ExtractLinesFromFile(NotificationBuffer buffer)
		{
			string[] result = null;

			if (ImportFile.Exists)
			{
				string fileData = File.ReadAllText(ImportFile.FullName);
				result = fileData.Split(new char[] { '\n' });
				result = StringParser.TrimBlankLinesAndNewLines(result);
			}
			else
			{
				buffer.Notify(new ErrorNotification(TNTErrorType.IOError, string.Format("File '{0}' doesn't exists", ImportFile.FullName)));
			}

			return result;
		}

		bool IsOkToLoadFromFileLines(string[] fileLines, NotificationBuffer buffer)
		{
			bool result = false;

			if (fileLines.Length > 0)
			{
				result = IsExtraCheckOK(fileLines, buffer);
			}
			else
			{
				buffer.Notify(new ErrorNotification(TNTErrorType.Error, string.Format("File '{0}' is empty", ImportFile.FullName)));
			}

			return result;
		}

		internal virtual bool IsExtraCheckOK(string[] fileLines, NotificationBuffer buffer)
		{
			return true;
		}

		void LoadFromFileLines(string[] fileLines, NotificationBuffer buffer)
		{
			PreLoadSetup();
			int i = 0;

			while (!buffer.HasErrors && i < fileLines.Length)
			{
				ZString line = fileLines[i];
				IQDownBaseRecord record = RecordFactory.NewRecord(line, buffer);

				if (LoadProgress != null)
				{
					LoadProgress(this, new TNTProgressEventArgs(i, fileLines.Length, "Reading lines from file"));
				}

				if (record != null)
				{
					ProcessRecord(record, buffer);
				}
				i++;
			}
		}

		protected abstract void PreLoadSetup();
		protected abstract void ProcessRecord(IQDownBaseRecord record, INotifications notify);

		#endregion

		protected readonly IQDownRecordFactory RecordFactory;
		protected readonly FileInfo ImportFile;
		protected readonly ZString BranchCode;
	}
}
