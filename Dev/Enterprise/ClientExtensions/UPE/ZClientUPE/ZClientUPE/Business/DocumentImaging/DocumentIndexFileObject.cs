using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.UPE.DocumentImaging
{
	/// <summary>
	/// An object wrapper for the Document Index File only.
	/// </summary>
	/// <remarks>
	/// The file is only read (safely) when the contents are required.
	/// </remarks>
	public class DocumentIndexFileObject
	{
		public DocumentIndexFileObject(FileInfo documentIndexFile, INotifications notifications, INotifications emailedNotifications)
		{
			this.notifications = notifications;
			this.emailedNotifications = emailedNotifications;

			// Let base (ErrorReporter) handle these exceptions.
			if (documentIndexFile == null)
			{
				throw new ArgumentNullException(nameof(documentIndexFile));
			}
			else if (!documentIndexFile.Exists)
			{
				throw new ArgumentException(string.Format(documentIndexFileMustExistErrorMessageFormat, documentIndexFile.FullName), nameof(documentIndexFile));
			}
			IndexFile = documentIndexFile;
		}
		const string documentIndexFileMustExistErrorMessageFormat = "The DocumentIndexFile '{0}' must exist.";

		public ReadOnlyCollection<FileInfo> ImageFiles
		{
			get
			{
				if (imageFiles == null)
				{
					ReadDocumentIndexFile();
				}
				return imageFiles.AsReadOnly();
			}
		}
		List<FileInfo> imageFiles;

		public ZString HouseBill
		{
			get { return GetProperty(houseBillPropertyName); }
		}
		const string houseBillPropertyName = "ShipNum";

		public ZString UPSDocTypeCode
		{
			get { return GetProperty(upsDocTypeCodePropertyName); }
		}
		const string upsDocTypeCodePropertyName = "DocType";

		public DocumentImageType DocumentType
		{
			get
			{
				if (documentType == null)
				{
					documentType = UPEDataRegistry.Instance.DocumentImagingImageTypes.Value.FindByUPSCode(UPSDocTypeCode);
					if (documentType == null)
					{
						emailedNotifications.Notify(new InfoNotification(string.Format(unknownImageTypeMessageFormat, UPSDocTypeCode, IndexFile.Name)));
					}
				}
				return documentType;
			}
		}
		DocumentImageType documentType;
		const string unknownImageTypeMessageFormat = "Unknown image type aka DocType '{0}' for index file '{1}'";

		public ZBool IsIndexFileOld
		{
			get
			{
				IndexFile.Refresh();
				return IndexFile.Exists && IndexFile.CreationTime < ZDateTime.Now.AddMonths(-1) && IndexFile.LastWriteTime < ZDateTime.Now.AddMonths(-1);
			}
		}

		public ReadOnlyMemory<byte> FirstImageBinary
		{
			get
			{
				if (firstImageBinary == null && ImageFiles != null && ImageFiles.Count != 0)
				{
					ProcessFirstImageFile();
				}
				return firstImageBinary;
			}
		}
		byte[] firstImageBinary;

		public ZString FirstTiffImageFilename
		{
			get
			{
				if (string.IsNullOrEmpty(firstTiffImageFilename) && ImageFiles != null && ImageFiles.Count != 0)
				{
					ProcessFirstImageFile();
				}
				return firstTiffImageFilename;
			}
		}
		ZString firstTiffImageFilename;

		void ReadDocumentIndexFile()
		{
			if (!IOExecuter.ExecuteIOTask(ReadIndexDelegate, IndexFile.FullName))
			{
				imageFiles = null;
				properties = null;
			}
		}

		void ProcessFirstImageFile()
		{
			if (!IOExecuter.ExecuteIOTask(ProcessFirstImageDelegate, ImageFiles[0].FullName))
			{
				firstImageBinary = null;
				firstTiffImageFilename = ZString.Empty;
			}
		}

		#region Read Index File
		/// <summary>
		/// Read the contents of the index file and populate the Image Filenames list and Properties list.
		/// </summary>
		void ReadDocumentIndexFileCore()
		{
			imageFiles = new List<FileInfo>();
			properties = new Dictionary<string, string>();
			using (FileStream fileStream = IndexFile.Open(FileMode.Open, FileAccess.Read, FileShare.None))  // Force an exclusive read lock.
			{
				using (TextReader reader = new StreamReader(fileStream))
				{
					string line = reader.ReadLine();
					string currentSection = string.Empty;
					while (line != null)
					{
						// Section Name eg: [SectionName]
						if (line == sectionNameFiles || line == sectionNameIndexingInfo)
						{
							currentSection = line;
							line = reader.ReadLine();
						}

						// Section Key Value pairs eg: Key=Value
						int valueDelimiterPosition = line.IndexOf(valueDelimiter);
						if (valueDelimiterPosition != -1)
						{
							KeyValuePair<string, string> keyValuePair = GetKeyValuePair(line, valueDelimiterPosition);
							if (currentSection == sectionNameFiles)
							{
								imageFiles.Add(new FileInfo(Path.Combine(IndexFile.Directory.FullName, keyValuePair.Value)));
							}
							else if (currentSection == sectionNameIndexingInfo)
							{
								properties.Add(keyValuePair.Key, keyValuePair.Value);
							}
						}
						line = reader.ReadLine();
					}
				}
			}
		}
		const string sectionNameFiles = "[Files]";
		const string sectionNameIndexingInfo = "[IndexingInfo]";
		const char valueDelimiter = '=';

		KeyValuePair<string, string> GetKeyValuePair(string line, int valueDelimiterPosition)
		{
			return new KeyValuePair<string, string>(line.Substring(0, valueDelimiterPosition), line.Substring(valueDelimiterPosition + 1));
		}
		#endregion

		void ProcessFirstImageFileCore()
		{
			firstImageBinary = File.ReadAllBytes(ImageFiles[0].FullName);
			if (firstImageBinary != null && firstImageBinary.Length != 0)
			{
				firstTiffImageFilename = Path.ChangeExtension(ImageFiles[0].FullName, tiffFilenameExtension);
			}
		}
		const string tiffFilenameExtension = ".tif";

		string GetProperty(string propertyName)
		{
			return Properties != null ? (Properties.ContainsKey(propertyName) ? Properties[propertyName] : string.Empty) : string.Empty;
		}

		Dictionary<string, string> Properties
		{
			get
			{
				if (properties == null)
				{
					ReadDocumentIndexFile();
				}
				return properties;
			}
		}
		Dictionary<string, string> properties;

		IOTaskDelegate ReadIndexDelegate
		{
			get { return readIndexFileDelegate ?? (readIndexFileDelegate = new IOTaskDelegate(ReadDocumentIndexFileCore)); }
		}
		IOTaskDelegate readIndexFileDelegate;

		IOTaskDelegate ProcessFirstImageDelegate
		{
			get { return processFirstImageFileDelegate ?? (processFirstImageFileDelegate = new IOTaskDelegate(ProcessFirstImageFileCore)); }
		}
		IOTaskDelegate processFirstImageFileDelegate;

		NonPersistentIOExecuter IOExecuter
		{
			get { return ioExecuter ?? (ioExecuter = new NonPersistentIOExecuter(notifications, emailedNotifications)); }
		}
		NonPersistentIOExecuter ioExecuter;

		public readonly FileInfo IndexFile;
		public static readonly ZString IndexFilePattern = "*.000";
		readonly INotifications notifications;
		readonly INotifications emailedNotifications;
	}
}
