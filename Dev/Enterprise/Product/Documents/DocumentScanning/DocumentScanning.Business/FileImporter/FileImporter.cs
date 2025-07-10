using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.Environment;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
	public class FileImporter : NonPersistentBusinessObject, IObsoleteValidation
	{
		/// <summary>
		/// Accepts a Factory and a boolean. Pass in true if this object is actually used for
		/// importing, or false if it is just used in the Config Form to set the registry settings.
		/// </summary>
		public FileImporter(DocumentFactory factory, bool isForImport)
			: base(factory)
		{
			ReloadRegistrySettings();
			fIsForImporting = isForImport;
			HasChanges = false;
		}

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDefaultImportDirectory();
		}

		#endregion

		#region MasterFactory

		public DocumentFactory MasterFactory
		{
			get
			{
				if (fMasterFactory == null)
				{
					fMasterFactory = (DocumentFactory)Factory;
				}
				return fMasterFactory;
			}
		}

		DocumentFactory fMasterFactory;

		#endregion

		#region Properties

		#region IsForImporting
		public bool IsForImporting
		{
			get { return fIsForImporting; }
		}
		readonly bool fIsForImporting;
		#endregion

		#region IsAutoAllocate

		public ZBool IsAutoAllocate
		{
			get
			{
				return ImportConfigRegistrySettings.AutoAllocate;
			}
			set
			{
				if (ImportConfigRegistrySettings.AutoAllocate != value)
				{
					ImportConfigRegistrySettings.AutoAllocate = value;
					HasChanges = true;
					IsAutoAllocateInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IsAutoAllocateInfo
		{
			get { return GetZPropertyInfo(nameof(IsAutoAllocate)); }
		}

		#endregion

		#region IsUsingCoverSheet

		public ZBool IsUsingCoverSheet
		{
			get
			{
				return ImportConfigRegistrySettings.UseCoverSheet;
			}
			set
			{
				if (ImportConfigRegistrySettings.UseCoverSheet != value)
				{
					ImportConfigRegistrySettings.UseCoverSheet = value;
					HasChanges = true;
					IsUsingCoverSheetInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IsUsingCoverSheetInfo
		{
			get { return GetZPropertyInfo(nameof(IsUsingCoverSheet)); }
		}

		#endregion

		#region DefaultImportDirectory

		[CargoWise.ComponentModel.MaxLength(1000)]
		public ZString DefaultImportDirectory
		{
			get
			{
				return ImportConfigRegistrySettings.DefaultDirectory;
			}
			set
			{
				if (ImportConfigRegistrySettings.DefaultDirectory != value)
				{
					CheckMaximumLength(DefaultImportDirectoryInfo, value);
					ImportConfigRegistrySettings.DefaultDirectory = value;
					HasChanges = true;
					if (!IsValidationSuspended)
					{
						ValidateDefaultImportDirectory();
					}
					DefaultImportDirectoryInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo DefaultImportDirectoryInfo
		{
			get { return GetZPropertyInfo(nameof(DefaultImportDirectory)); }
		}

		#endregion

		#region IsOutputAutomatic

		public ZBool IsOutputAutomatic
		{
			get
			{
				return fIsOutputAutomatic;
			}
			set
			{
				if (fIsOutputAutomatic != value)
				{
					fIsOutputAutomatic = value;
					HasChanges = true;
					IsOutputAutomaticInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IsOutputAutomaticInfo
		{
			get { return GetZPropertyInfo(nameof(IsOutputAutomatic)); }
		}

		ZBool fIsOutputAutomatic;

		#endregion

		#region IsOutputAutomaticSingle

		public ZBool IsOutputAutomaticSingle
		{
			get
			{
				return fIsOutputAutomaticSingle;
			}
			set
			{
				if (fIsOutputAutomaticSingle != value)
				{
					fIsOutputAutomaticSingle = value;
					HasChanges = true;
					IsOutputAutomaticSingleInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IsOutputAutomaticSingleInfo
		{
			get { return GetZPropertyInfo(nameof(IsOutputAutomaticSingle)); }
		}

		ZBool fIsOutputAutomaticSingle;

		#endregion

		#region IsOutputNewFileBatch

		public ZBool IsOutputNewFileBatch
		{
			get
			{
				return fIsOutputNewFileBatch;
			}
			set
			{
				if (fIsOutputNewFileBatch != value)
				{
					fIsOutputNewFileBatch = value;
					HasChanges = true;
					IsOutputNewFileBatchInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IsOutputNewFileBatchInfo
		{
			get { return GetZPropertyInfo(nameof(IsOutputNewFileBatch)); }
		}

		ZBool fIsOutputNewFileBatch;

		#endregion

		#region IsOutputNewFileSingle
		public ZBool IsOutputNewFileSingle
		{
			get
			{
				return fIsOutputNewFileSingle;
			}
			set
			{
				if (fIsOutputNewFileSingle != value)
				{
					fIsOutputNewFileSingle = value;
					HasChanges = true;
					IsOutputNewFileSingleInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IsOutputNewFileSingleInfo
		{
			get { return GetZPropertyInfo(nameof(IsOutputNewFileSingle)); }
		}

		ZBool fIsOutputNewFileSingle;

		#endregion

		#region DeleteSourceFilesAfterImport

		public ZBool DeleteSourceFilesAfterImport
		{
			get
			{
				return ImportConfigRegistrySettings.DeleteSourceFilesAfterImport;
			}
			set
			{
				if (ImportConfigRegistrySettings.DeleteSourceFilesAfterImport != value)
				{
					ImportConfigRegistrySettings.DeleteSourceFilesAfterImport = value;
					HasChanges = true;
					DeleteSourceFilesAfterImportInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo DeleteSourceFilesAfterImportInfo
		{
			get { return GetZPropertyInfo(nameof(DeleteSourceFilesAfterImport)); }
		}

		#endregion

		#region IncludeSubdirectories

		public ZBool IncludeSubdirectories
		{
			get
			{
				return ImportConfigRegistrySettings.IncludeSubdirectories;
			}
			set
			{
				if (ImportConfigRegistrySettings.IncludeSubdirectories != value)
				{
					ImportConfigRegistrySettings.IncludeSubdirectories = value;
					HasChanges = true;
					IncludeSubdirectoriesInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IncludeSubdirectoriesInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeSubdirectories)); }
		}

		#endregion

		#region OutputOption
		public ZString OutputOption
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsOutputAutomatic)
				{
					result = Constants.Automatic;
				}
				else if (IsOutputAutomaticSingle)
				{
					result = Constants.AutomaticSingle;
				}
				else if (IsOutputNewFileSingle)
				{
					result = Constants.NewFileSingle;
				}
				else if (IsOutputNewFileBatch)
				{
					result = Constants.NewFileBatch;
				}

				return result;
			}
			set
			{
				IsOutputNewFileSingle = false;
				IsOutputNewFileBatch = false;
				IsOutputAutomaticSingle = false;
				IsOutputAutomatic = false;

				switch (value)
				{
					case Constants.NewFileSingle:
						IsOutputNewFileSingle = true;
						break;
					case Constants.NewFileBatch:
						IsOutputNewFileBatch = true;
						break;
					case Constants.AutomaticSingle:
						IsOutputAutomaticSingle = true;
						break;
					case Constants.Automatic:
					default:
						IsOutputAutomatic = true;
						break;
				}
			}
		}
		#endregion

		#region FileCount

		public virtual int FileCount
		{
			get { return GetFileList(DefaultImportDirectory, IncludeSubdirectories).Count; }
		}

		#endregion

		#region JobType

		public ZString JobType { get; set; }

		#endregion

		#region JobTypeList

		public CodeDescriptionPairList JobTypeList
		{
			get
			{
				return AssemblyDataLookup.DocManagerCodesForAllocation;
			}
		}

		#endregion

		#region DocType

		[CargoWise.ComponentModel.MaxLength(AutoStorageDocs.Schema.SC_DocTypeMaxLength)]
		public ZString DocType { get; set; }

		#endregion

		#region DocTypeList

		public CodeDescriptionPairList DocTypeList
		{
			get
			{
				CodeDescriptionPairList docTypeList;
				if (!docTypeListCache.TryGetValue(JobType, out docTypeList))
				{
					docTypeList = new CodeDescriptionPairList();
					docTypeList.AddPair(string.Empty, Res.GetString("4E96BEC1-38B3-4B85-A883-6B8325E796D8", "Use doc type from barcode."));
					docTypeList.AddRange(DocScanningHelper.GetCategoryDocTypesFromJobType(JobType, MasterFactory, false));
					docTypeListCache.Add(JobType, docTypeList);
				}
				return docTypeList;
			}
		}

		readonly Dictionary<string, CodeDescriptionPairList> docTypeListCache = new Dictionary<string, CodeDescriptionPairList>();

		#endregion

		#region ImportCancelled

		public ZBool ImportCancelled
		{
			get { return fImportCancelled; }
			set { fImportCancelled = value; }
		}

		ZBool fImportCancelled;

		#endregion

		#region ImportingUserPK

		public ZGuid ImportingUserPK
		{
			get { return fImportingUserPK; }
			set
			{
				fImportingUserPK = value;
				ReloadRegistrySettings();
			}
		}

		ZGuid fImportingUserPK;

		#endregion

		#endregion

		#region Validation

		#region ValidateDefaultImportDirectory

		public void ValidateDefaultImportDirectory()
		{
			DefaultImportDirectoryInfo.ClearAllNotifications();

			if (IsDirectoryPathInLocalClient(DefaultImportDirectory))
			{
				// We should only check Directory.Exists on a local path and avoid it on UNC path for security reason (NTLM hash leaks)
				if (!Directory.Exists(DefaultImportDirectory))
				{
					DefaultImportDirectoryInfo.AddError(Res.GetString("ca6b5940-95f8-4da5-9bde-d220eb01f6d3", "Please select a valid directory to import"));
				}
			}
			else
			{
				DefaultImportDirectoryInfo.AddError(Res.GetString("C0F4D6E0-567A-49F6-A926-2187643C333A", "Please select a local directory to import"));
			}
		}

		static bool IsDirectoryPathInLocalClient(string path)
		{
			if (ObjectFactory.Get<TerminalService>().IsRemoteAppSession)
			{
				return FileSystem.IsDirectoryPathForClientMachine(path);
			}

			return FileSystem.IsLocalDirectory(path);
		}

		#endregion

		#endregion

#if DEBUG
		public
#else
		internal
#endif
		void ReloadRegistrySettings()
		{
			ImportConfigRegistrySettings = (ImportingUserPK.IsEmpty) ? Env.Registry.DMImportConfigurationSettings : Env.Registry.GetDMImportConfigurationSettings(ImportingUserPK.ToGuid());
			OutputOption = ImportConfigRegistrySettings.OutputOption;
		}

		public void SaveRegistrySettings()
		{
			//RunPreSaveValidation();
			ImportConfigRegistrySettings.OutputOption = OutputOption;
			Env.Registry.DMImportConfigurationSettings = ImportConfigRegistrySettings;
		}

		/// <summary>
		/// Parses the file as per the scanning options, and returns the list of DocumentResults which 
		/// will eventually be turned into StorageDocs documents.
		/// </summary>
		public ScanningFinishedEventArgs ExecuteSort(string sourceFileName)
		{
			var finishedEventArgs = new ScanningFinishedEventArgs();

			using (var document = PreviewableDocumentHelper.GetPreviewableDocument(sourceFileName))
			{
				if (document.NumberOfPages > 0)
				{
					var documentResults = SplitDocument(GetSplitter(), document);

					foreach (DocumentResult result in documentResults)
					{
						result.SourceFileName = sourceFileName;
						finishedEventArgs.AddFileDetail(result);
					}
				}
			}

			return finishedEventArgs;
		}

		IFileSplitter GetSplitter()
		{
			switch (OutputOption)
			{
				case Constants.NewFileSingle:
					return new FilePageSplitter(IsUsingCoverSheet, JobType, DocType);

				case Constants.NewFileBatch:
					return new ImportWholeFileSplitter(IsUsingCoverSheet, JobType, DocType);

				case Constants.AutomaticSingle:
					return new BarcodeFileSplitter(MasterFactory, true, IsUsingCoverSheet, JobType, DocType);

				case Constants.Automatic:
				default:
					return new BarcodeFileSplitter(MasterFactory, false, IsUsingCoverSheet, JobType, DocType);
			}
		}

#if DEBUG
		public int ImportFromDirectoryCallCounter;
#endif

		public virtual int ImportFromDirectory(string sourceDirectory, List<string> failedImports)
		{
			return ImportFiles(GetFileList(sourceDirectory, IncludeSubdirectories), failedImports);
		}

		public static bool IsSupported(string extension) => PreviewableDocumentHelper.IsSupported(extension);

		public static bool IsSupportedImageFile(string extension)
		{
			extension = extension.Trim('.').ToUpperInvariant();
			return extension != Core.Constants.FileFormats.PDF && IsSupported(extension);
		}

		public static IEnumerable<string> SupportedFileFormats => PreviewableDocumentHelper.SupportedFileFormats;

		#region Implementation

		DMImportConfigurationSettingsStruct ImportConfigRegistrySettings;

		/// <summary>
		/// Recursive function, goes into subdirectores and retrieves the list of files to import
		/// </summary>
		protected ICollection<string> GetFileList(string sourceDirectory, bool includeSubdirectories)
		{
			bool IsCausedByFilesystem(Exception ex) => ex is IOException || ex is UnauthorizedAccessException || ex is OperationCanceledException;

			var result = new List<string>();
			foreach (var extensionType in SupportedFileFormats)
			{
				try
				{
					result.AddRange(Directory.GetFiles(sourceDirectory, "*." + extensionType));
				}
				catch (Exception ex) when (IsCausedByFilesystem(ex))
				{
				}
			}

			if (includeSubdirectories)
			{
				try
				{
					result.AddRange(Directory.GetDirectories(sourceDirectory).SelectMany(dir => GetFileList(dir, includeSubdirectories)));
				}
				catch (Exception ex) when (IsCausedByFilesystem(ex))
				{
				}
			}

			return result;
		}

		public int ImportFiles(ICollection<string> importFiles, ICollection<string> failedImports)
		{
			int filesProcessed = 0;

			foreach (string filePath in importFiles)
			{
				if (ImportCancelled)
				{
					break;
				}

				try
				{
					ImportFile(filePath);
					filesProcessed++;
				}
				catch (Exception ex)
				{
					if (!failedImports.Any())
					{
						failedImports.Add(ResString.GetMultilingualString("55813ecc-6a2f-4622-90f1-9e5db0bb1eb8", "The following files could not be imported:"));
					}

					Exception outOfMemoryException;
					Exception unsupportedDocumentException;
					var fileThatCausedException = "- " + Path.GetFileName(filePath) + ": ";
					if ((outOfMemoryException = ex.Find<OutOfMemoryException>()) != null)
					{
						failedImports.Add(fileThatCausedException + outOfMemoryException.Message);
					}
					else if ((unsupportedDocumentException = ex.Find<UnsupportedDocumentException>()) != null)
					{
						failedImports.Add(fileThatCausedException + unsupportedDocumentException.Message);
					}
					else
					{
						throw;
					}
				}
				finally
				{
					if (DeleteSourceFilesAfterImport)
					{
						DeleteFileAfterImport(filePath);
						FilesNotDeletedOnImport.Remove(filePath);
					}
				}
				SingleFileImported?.Invoke(this, new FileImportedEventArgs(filesProcessed, importFiles.Count));
			}
			return filesProcessed;
		}

		void DeleteFileAfterImport(string filePath)
		{
			try
			{
				File.Delete(filePath);
			}
			catch (Exception)
			{
				FilesNotDeletedOnImport.Add(filePath);
			}
		}

		public StringCollection FilesNotDeletedOnImport
		{
			get { return filesNotDeletedOnImport ?? (filesNotDeletedOnImport = new StringCollection()); }
		}

		StringCollection filesNotDeletedOnImport;

		/// <summary>
		/// Imports a single file from the filesystem 
		/// Returns whether file was imported successfully
		/// </summary>
		internal void ImportFile(string importFile)
		{
			ScanningFinishedEventArgs ea = ExecuteSort(importFile);

			if (ea.FileDetailsCount > 0 && ImportSuccessful != null)
			{
				ImportSuccessful(this, ea);
			}
		}

#if DEBUG
		public virtual
#else
		internal
#endif
		List<string> ReadBarcodesFromFile(string filePath)
		{
			var scannedBarcodes = new List<string>();
			using (var doc = PreviewableDocumentHelper.GetPreviewableDocument(filePath))
			using (var pageFileReader = new PreviewableDocumentImageReader(doc))
			{
				for (int i = 0; i < pageFileReader.PageSelector.TotalPages; i++)
				{
					var activePage = (Bitmap)pageFileReader.PageSelector.CurrentImage;
					if (activePage.Width <= 0 || activePage.Height <= 0)
					{
						throw new InvalidOperationException($"Height or Width not valid at page {i} in {filePath}. Height:{activePage.Height}, Width:{activePage.Width}");
					}
					var barcodeSorter = new BarcodeSorter(MasterFactory);
					var barcode = barcodeSorter.ProcessPageForBarcodes(activePage, JobType);
					scannedBarcodes.AddRange(barcodeSorter.ScannedBarcodesOnPage.Cast<string>());
				}
			}
			return scannedBarcodes;
		}

		int[] GetRange(FileSplitParameters split)
		{
			return Enumerable.Range(split.StartPageInclusive, split.EndPageExclusive - split.StartPageInclusive).ToArray();
		}

		string SplitAndSave(FileSplitParameters split, IPreviewableDocument document)
		{
			var splitRepresentsWholeDocument = split.StartPageInclusive == 0 && split.EndPageExclusive == document.NumberOfPages;
			var savePath = DocumentUtilities.GetTempFilename(document.PreferredExtension);

			if (splitRepresentsWholeDocument)
			{
				document.Save(savePath);
			}
			else
			{
				using (var subDocument = document.ExtractPages(GetRange(split)))
				{
					subDocument.Save(savePath);
				}
			}

			return savePath;
		}

		DocumentResultCollection SplitDocument(IFileSplitter splitter, IPreviewableDocument document)
		{
			var documentResults = new DocumentResultCollection();

			foreach (var split in splitter.SplitFile(document))
			{
				string filename = SplitAndSave(split, document);

				DocumentResult result;
				if (split.UseBarcodeType)
				{
					var hasConsignmentId = split.ReferenceType == Enterprise.Core.Constants.DocManagerCodes.DomesticTransportConsignment;
					result = new BaseBarcode(MasterFactory, split.InitialBarcode?.FullBarcodeText ?? ZString.Empty)
					{
						FilePath = filename,
						DocManagerCode = split.ReferenceType,
						RefCode = hasConsignmentId ? FindConsignmentIDByReference(split.ReferenceCode) : (ZString)split.ReferenceCode,
						DocType = split.DocumentType,
						CompanyCode = split.CompanyCode
					};
				}
				else
				{
					result = new DocumentResult(MasterFactory, filename);
				}

				result.ScannedBarcodeValue = BarcodeHelper.JoinValidBarcodes(split.ScannedBarcodes, StorageDocsBarcode.Schema.SCB_BarcodeMaxLength);
				documentResults.Add(result);
			}

			return documentResults;
		}

		ZString FindConsignmentIDByReference(ZString referenceNumber)
		{
			var query = new ZDBOnlyQuery(typeof(Enterprise.Integration.TransportConsignment.IDtbBookingConsignment));
			query.AddToFilter(DtbBookingSchema.KM_TransportReference, referenceNumber);

			var subQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.TransportConsignment.IDtbConsignmentConsolidation), DtbBookingConsolidationSchema.PK);
			subQuery.AddToFilter(DtbBookingConsolidationSchema.KB_JobType, "CSN");
			query.AddSubQuery(DtbBookingSchema.KM_KB_Booking, subQuery, JoinCondition.And);

			var consignments = Factory.Load(ObjectFactory.GetType(typeof(Enterprise.Integration.TransportConsignment.IDtbBookingConsignment)), query);
			return consignments.Length == 1 ? consignments[0][DtbBookingSchema.Constants.KM_JobID].ToString() : string.Empty;
		}

		#endregion

		public event FileImportedEventHandler SingleFileImported;
		public event ScanningFinishedEventHandler ImportSuccessful;
	}
}
