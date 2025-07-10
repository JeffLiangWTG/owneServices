using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageFileCollectionView : StorageDocsCollectionViewBase
	{
		public StorageFileCollectionView(StorageMain parent, BusinessObjectCollection collection)
			: base(parent, collection)
		{
		}
#if DEBUG
		public bool IsThisPartOfTheCollectionExposed(BusinessObject element) => IsThisPartOfTheCollection(element);
#endif
		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			StorageDocsBase bizO = element as StorageDocsBase;
			return base.IsThisPartOfTheCollection(element) && !bizO.IsImageFile && (SC_DataTypeFilter.IsEmpty || bizO.SC_DataType.EqualsIgnoringCase(SC_DataTypeFilter));
		}

		public ZString SC_DataTypeFilter { get; set; }

		public new StorageFile this[int index]
		{
			get { return (StorageFile)Elements[index]; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public new StorageFile AddNew()
		{
			return (StorageFile)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(StorageFile);
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || CollectionToFilter.ReadOnly; }
		}

		/// <summary>
		/// This method is only used in test cases as a workaround to test AddOrUpdateFromFilename.
		/// </summary>
#if DEBUG
		public
#else
		private
#endif
		StorageFile[] AddOrUpdateFromFilenames(FileAction fileActionForDuplicateNames, params string[] filenames)
		{
			var count = 0;
			var returnValue = new StorageFile[filenames.Length];

			foreach (var name in filenames)
			{
				var filename = name.Trim();
				var fileContents = DocumentUtilities.GetFileAsBytes(filename);
				var action = FilenameExists(filename) ? fileActionForDuplicateNames : FileAction.CreateNew;
				var file = AddOrUpdateFromFilename(fileContents, Path.GetFileName(filename), action);
				if (file != null)
				{
					returnValue[count++] = file;
				}
			}

			return returnValue;
		}

		public void DisposeAll()
		{
			foreach (StorageFile file in Elements)
			{
				file.Dispose();
			}
		}

		public string[] FilesOpen
		{
			get
			{
				return Elements
					.Where(x => x is StorageFile file && file.IsTempFileOpen)
					.Cast<StorageFile>()
					.Select(x => x.UseRemoteFile ? x.SC_FileNameWithExtension.ToString() : x.TempFileName.ToString())
					.ToArray();
			}
		}

		public bool FilenameExists(string filename)
		{
			return FindDocByName(Path.GetFileNameWithoutExtension(filename), Path.GetExtension(filename)) != null;
		}

		internal StorageFile AddOrUpdateFromFilename(byte[] contents, ZString filenameOnlyWithExtension, FileAction actionToPerformForDuplicateNames)
		{
			Action<StorageFile> updateExistingFile = (storageFile) => UpdateExistingFile(storageFile, contents);
			Func<FileAction, StorageFile> addOrUpdateFromFilename = (fileAction) => AddOrUpdateFromFilename(contents, filenameOnlyWithExtension, fileAction);
			Func<string, string, StorageFile> createNewDoc = (nameOnly, extension) => (StorageFile)CreateNewDoc(contents, nameOnly, extension);
			return AddOrUpdateFromFilename(filenameOnlyWithExtension, actionToPerformForDuplicateNames, updateExistingFile, addOrUpdateFromFilename, createNewDoc);
		}

		internal StorageFile AddOrUpdateFromFilename(SubStreamableStream contents, ZString filenameOnlyWithExtension, FileAction actionToPerformForDuplicateNames)
		{
			Action<StorageFile> updateExistingFile = (storageFile) => UpdateExistingFile(storageFile, contents);
			Func<FileAction, StorageFile> addOrUpdateFromFilename = (fileAction) => AddOrUpdateFromFilename(contents, filenameOnlyWithExtension, fileAction);
			Func<string, string, StorageFile> createNewDoc = (nameOnly, extension) => (StorageFile)CreateNewDoc(contents, nameOnly, extension);
			return AddOrUpdateFromFilename(filenameOnlyWithExtension, actionToPerformForDuplicateNames, updateExistingFile, addOrUpdateFromFilename, createNewDoc);
		}

		StorageFile AddOrUpdateFromFilename(ZString filenameOnlyWithExtension, FileAction actionToPerformForDuplicateNames, Action<StorageFile> updateExistingFile, Func<FileAction, StorageFile> addOrUpdateFromFilename, Func<string, string, StorageFile> createNewDoc)
		{
			StorageFile result = null;
			string extension = Path.GetExtension(filenameOnlyWithExtension);
			string nameOnly = TruncateAndTrimDotRemoveExtension(filenameOnlyWithExtension, StorageDocsSchema.SC_FileName.MaxLength);

			switch (actionToPerformForDuplicateNames)
			{
				case FileAction.Overwrite:
					result = (StorageFile)FindDocByName(nameOnly, extension, false);
					if (result != null)
					{
						updateExistingFile(result);
					}
					else
					{
						result = addOrUpdateFromFilename(FileAction.CreateNew);
					}
					break;

				case FileAction.CreateNew:
					result = createNewDoc(nameOnly, extension);
					break;

				case FileAction.None:
					break;

				case FileAction.AskForUserInput:
				default:
					if (AskForOverwriteOrCreateNew != null)
					{
						FilenameEventArgs args = new FilenameEventArgs(filenameOnlyWithExtension);
						AskForOverwriteOrCreateNew(this, args);
						result = addOrUpdateFromFilename(args.Action);
					}
					break;
			}
			return result;
		}

		void UpdateExistingFile(StorageFile existingFile, byte[] contents)
		{
			if (existingFile != null)
			{
				existingFile.SC_ImageData = contents;
				existingFile.SC_Date = ZDateTime.Now;
			}
		}

		void UpdateExistingFile(StorageFile existingFile, SubStreamableStream contents)
		{
			if (existingFile != null)
			{
				((IeDocBase)existingFile).SetImageDataStream(contents);
				existingFile.SC_Date = ZDateTime.Now;
			}
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(StorageDocsSchema.SC_FileName, SQLComparisonOperator.NotEqual, ZString.Empty);
			return query;
		}

		public event OverwriteOrCreateNewEventHandler AskForOverwriteOrCreateNew;
	}
}
