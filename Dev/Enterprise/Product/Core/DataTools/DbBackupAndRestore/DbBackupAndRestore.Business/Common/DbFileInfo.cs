using System;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	interface ISetFileSequence
	{
		void SetFileSequencePerType(int seqNumber);
	}

	public class DbFileInfo : ISetFileSequence
	{
		public DbFileInfo(string logicalName, string folderPath, string fileType, string filePath = "", string dbType = DbFileInfo.DbTypeMain, string restoreDbName = "")
		{
			ValidateFileType(fileType);
			ValidateDbType(dbType);

			LogicalName = logicalName;
			FolderPath = folderPath;
			FilePath = filePath;
			FolderPathView = folderPath;
			FileType = fileType;
			DbType = dbType;
			DbName = restoreDbName;
			Visible = true;
		}

		void ValidateFileType(string fileType)
		{
			if (fileType != FileTypeData && fileType != FileTypeLog)
			{
				throw new ArgumentException(String.Format("Wrong file type ({0}). It must be (D) data or (L) log.", fileType), nameof(fileType));
			}
		}

		public const string FileTypeData = "D";
		public const string FileTypeLog = "L";

		public readonly string LogicalName;
		public readonly string DbType;
		public readonly string DbName;
		public readonly string FileType;
		public readonly string FilePath;

		public string FolderPath
		{
			get;
			set;
		}

		public string FolderPathView
		{
			get;
			set;
		}

		public bool Visible
		{
			get;
			set;
		}

		public bool SharedRefDb
		{
			get;
			set;
		}

		public int FileSequencePerType
		{
			get;
			set;
		}

		void ValidateDbType(string dbType)
		{
			if (
				dbType != DbTypeMain &&
				dbType != DbTypeEDocs &&
				dbType != DbTypeUserRepository &&
				dbType != DbTypeRefDB &&
				dbType != DbTypeAuditDB &&
				dbType != DbTypeEdwDB &&
				dbType != DbTypeSingleSharedRefDB)
			{
				throw new ArgumentException(String.Format("Wrong db type ({0}). It must be ({1}), ({2}), ({3}), ({4}), ({5}), or ({6}).",
					dbType, DbTypeMain, DbTypeEDocs, DbTypeRefDB, DbTypeUserRepository, DbTypeAuditDB, DbTypeEdwDB), nameof(dbType));
			}
		}

		public const string DbTypeMain = "Main DB";
		public const string DbTypeEDocs = "eDocs DB";
		public const string DbTypeRefDB = "Reference DB";
		public const string DbTypeUserRepository = "User Repository DB";
		public const string DbTypeAuditDB = "Audit DB";
		public const string DbTypeEdwDB = "EDW DB";
		public const string DbTypeSingleSharedRefDB = "Single Shared Ref DB";

		void ISetFileSequence.SetFileSequencePerType(int seqNumber)
		{
			FileSequencePerType = seqNumber;
		}

		public string FilePathSuffix
		{
			get
			{
				string result;

				if (FileSequencePerType <= 0)
				{
					throw new InvalidOperationException("Attempt to access FileSequencePerType before it was initialised.");
				}
				else if (FileSequencePerType == 1)
				{
					result = (FileType == FileTypeData) ? "_Data.mdf" : "_Log.ldf";
				}
				else
				{
					var mask = (FileType == FileTypeData) ? "_Data{0}.mdf" : "_Log{0}.ldf";
					result = string.Format(System.Globalization.CultureInfo.InvariantCulture, mask, FileSequencePerType);
				}

				return result;
			}
		}
	}
}
