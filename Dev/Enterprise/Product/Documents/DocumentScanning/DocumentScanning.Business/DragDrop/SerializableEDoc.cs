using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentScanning.Business
{
	/// <summary>
	/// A serializable version of the StorageDocsBase business object. 
	/// The serializable doc can be put onto the windows clipboard for cut and paste operations.
	/// </summary>
	[Serializable]
	public class SerializableEDoc : WrappedBusinessObject
	{
		public readonly Guid PK;
		public readonly string DocType;
		public readonly string DocSource;
		public readonly string Language;
		public readonly string DataType;
		public readonly string Desc;
		public readonly string FileName;
		public readonly bool IsDeleted;
		public readonly bool IsPublished;
		public readonly bool IsSystemGenerated;
		public readonly bool SaveVersions;
		public readonly string FileNameWithExtension;
		public readonly Guid SM;
		public readonly Type Type;

		public readonly string RefType;
		public readonly Guid RefPK;

		readonly object tempFileNameLock = new object();

		public SerializableEDoc(StorageDocsBase baseDoc)
			: base(baseDoc)
		{
			PK = baseDoc.PK.ToGuid();
			DocType = baseDoc.SC_DocType;
			DocSource = baseDoc.SC_RDS_NKDocSource;
			Language = baseDoc.SC_Language;
			DataType = baseDoc.EDocFormat;
			Desc = baseDoc.SC_DescMultilingual;
			FileName = baseDoc.SC_FileName;
			IsDeleted = baseDoc.SC_IsDeleted;
			IsPublished = baseDoc.SC_IsPublished;
			IsSystemGenerated = baseDoc.SC_IsSystemGenerated;
			SaveVersions = baseDoc.SC_SaveVersions;
			Type = baseDoc.GetType();
			SM = (baseDoc.SC_SM.IsValid) ? baseDoc.SC_SM.ToGuid() : Guid.Empty;

			lock (tempFileNameLock)
			{
				FileNameWithExtension = TempFileName;
			}

			if (baseDoc.TempFileName.IsEmpty || !File.Exists(baseDoc.TempFileName))
			{
				try
				{
					using (FileStream stream = File.Create(FileNameWithExtension))
					{
						baseDoc.SaveToStream(stream);
					}
				}
				catch (Exception)
				{
					if (File.Exists(FileNameWithExtension))
					{
						File.Delete(FileNameWithExtension);
					}
					throw;
				}
			}
			else
			{
				File.Copy(baseDoc.TempFileName, FileNameWithExtension);
			}
			if (baseDoc.IsImageFile)
			{
				StorageDocs eDoc = baseDoc as StorageDocs;

				if (eDoc.ParentMain != null)
				{
					RefType = eDoc.ParentMain.SM_Type.IsEmpty ? new ZString(Core.Constants.DocManagerCodes.Unallocated) : eDoc.ParentMain.SM_Type;

					if (eDoc.ParentMain.SM_ParentFK.IsValid) // Check parent for values first.
					{
						RefPK = eDoc.ParentMain.SM_ParentFK.ToGuid();
					}
					else
					{
						RefPK = Guid.Empty;
					}
				}
			}
#if DEBUG
			fileNames.Add(FileNameWithExtension);
#endif
		}

		public SerializableEDoc(StorageDocsBase baseDoc, byte[] replacementImageData)
			: this(baseDoc)
		{
			using (FileStream stream = File.Open(FileNameWithExtension, FileMode.Create, FileAccess.Write))
			{
				stream.Write(replacementImageData, 0, replacementImageData.Length);
			}
		}

		/// <summary>
		/// Converts the serialized eDoc to a business object. 
		/// </summary>
		public StorageDocsBase ToBusinessObject(StorageMain parent)
		{
			return ToBusinessObject(parent, false, false);
		}

		/// <summary>
		/// Converts the serialized eDoc to a business object. 
		/// </summary>
		/// <param name="conditionalDescriptionReplacement">Whether or not the description should be set to the original value.
		/// <param name="conditionalFileNameReplacement">Whether or not the file name should be set to the original value. 
		/// This is conditional on the document not being system generated, or being a misc document.</param>
		public StorageDocsBase ToBusinessObject(StorageMain parent, bool conditionalDescriptionReplacement, bool conditionalFileNameReplacement)
		{
			var filenameOnlyWithExtension = FileName + "." + DataType.ToLower();
			var addedElement = parent.AddFileOrDocument(DocumentUtilities.GetFileAsBytes(FileNameWithExtension), new AddFileOrDocumentDto
			{
				FileName = filenameOnlyWithExtension,
				DocumentType = DocType,
			});
			PopulateFieldsExcludingSC_SM(parent, addedElement, conditionalDescriptionReplacement, conditionalFileNameReplacement);
			return addedElement;
		}

		/// <summary>
		/// Populate the fields for a business object that has been added to an existing parent. 
		/// </summary>
		void PopulateFieldsExcludingSC_SM(StorageMain parent, StorageDocsBase element, bool conditionalDescriptionReplacement, bool conditionalFileNameReplacement)
		{
			if (element.IsImageFile && parent.SM_ParentFK.IsEmpty)
			{
				parent.SM_Type = RefType;
				parent.SM_ParentFK = RefPK;
			}

			if (!conditionalFileNameReplacement)
			{
				element.SC_FileName = FileName;
			}

			element.SC_DataType = DataType;
			element.SC_DocType = DocType;
			element.SC_RDS_NKDocSource = DocSource;
			element.SC_Language = Language;
			element.SC_IsPublished = IsPublished;
			element.SC_IsDeleted = IsDeleted;
			element.SC_IsSystemGenerated = false; // any copied/pasted docs will not be system generated
			element.SC_SaveVersions = SaveVersions;
			if (!conditionalDescriptionReplacement || !IsSystemGenerated || DocType == Core.Constants.RefDocTypes.MiscellaneousDocument)
			{
				element.SC_Desc = Desc;
			}
		}

		public bool IsImageFile => SerializableEDocsTools.IsImage(DataType);

		/// <summary>
		/// Unique name and file path with file extension
		/// </summary>
		string TempFileName
		{
			get
			{
				var name = IsImageFile ? Desc : FileName;

				if (string.IsNullOrEmpty(name))
				{
					name = Guid.NewGuid().ToString();
				}

				name = name + '.' + DataType.ToLower();
				var tempPath = Temp.TempPath;

				// Added in WI00616433 and can be removed if the root cause is identified and fixed.
				try
				{
					if (!Directory.Exists(tempPath))
					{
						Directory.CreateDirectory(tempPath);
					}
				}
				catch (IOException ex)
				{
					throw new IOException($"The network path was not found: '{tempPath}'. HResult is {ex.HResult}.", ex);
				}

				return new UniqueFilenameGenerator().GetNewUniqueFilePath(Temp.TempPath, name);
			}
		}

#if DEBUG
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Test only code")]
		public static List<string> fileNames = new List<string>();
#endif

		internal int DelayInMillisecondsBeforeDelete
		{
			get
			{
#if DEBUG
				if (Globals.IsTest)
				{
					return 1000;
				}
				else
#endif
				{
					return 60000;
				}
			}
		}
	}

	public static class SerializableEDocsTools
	{
		public static bool IsImage(string fileExtension)
		{
			var fileExtensionUpcase = fileExtension.ToUpperInvariant();
			return fileExtensionUpcase == Core.Constants.FileFormats.TIF || fileExtensionUpcase == Core.Constants.FileFormats.JPG
				|| fileExtensionUpcase == Core.Constants.FileFormats.JPEG || fileExtensionUpcase == Core.Constants.FileFormats.GIF
				|| fileExtensionUpcase == Core.Constants.FileFormats.PNG || fileExtensionUpcase == Core.Constants.FileFormats.BMP
				|| fileExtensionUpcase == Core.Constants.FileFormats.TIFF;
		}
	}
}
