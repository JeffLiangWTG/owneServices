using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageFile : StorageDocsBase, IStorageFile
	{
		public StorageFile(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Testing Only
#if DEBUG

		public static StorageFile New_DEBUG(BusinessObjectFactory factory)
		{
			return (StorageFile)factory.New(typeof(StorageFile));
		}

		public static StorageFile NewWithParent_DEBUG(NumberedBusinessObjectFactory factory)
		{
			var doc = (StorageFile)factory.NewWithParent(typeof(StorageFile));
			doc.ParentMain.SM_DB = factory.DBNumber == 0 ? 1 : factory.DBNumber;
			return doc;
		}

#endif
		#endregion

		public new StorageMain ParentMain
		{
			get { return (StorageMain)MasterFactory.Load(typeof(StorageMain), SC_SM); }
		}

		#region Properties

		#region SC_FileName

		[ReadOnly(true)]
		public override ZString SC_FileName
		{
			get { return base.SC_FileName; }
			set { base.SC_FileName = value; }
		}

		#endregion

		#region SC_Date

		[ReadOnly(true)]
		public override ZDateTime SC_Date
		{
			get { return base.SC_Date; }
			set { base.SC_Date = value; }
		}

		#endregion

		#endregion

		#region Validation

		public new StorageFileValidation Validation
		{
			get { return (StorageFileValidation)base.Validation; }
		}

		protected override StorageDocsValidation GetNewValidation()
		{
			return new StorageFileValidation(this);
		}

		#endregion

		#region Opening on File System

		protected override string GetTempFileNameWithPath()
		{
			var validFileName = SC_FileNameWithExtension;
			if (CargoWise.IO.MakeFilenameSafe.IsWindowsReservedFileName(validFileName))
			{
				validFileName = Guid.NewGuid().ToString() + validFileName;
			}

			return new UniqueFilenameGenerator().GetNewUniqueFilePath(Temp.TempPath, validFileName);
		}

		#endregion

		const char CharToReplaceIllegalCharsInFileName = '-';

		public override void SetNewFileName(string fileNameWithExtension)
		{
			var fileName = GetFileNameOnlyFromFilename(fileNameWithExtension);
			var extension = GetExtensionFromFilename(fileNameWithExtension);
			SetNewFileName(fileName, extension);
		}

		public void SetNewFileName(string fileNameOnly, string extension)
		{
			SC_FileName = fileNameOnly;
			SC_DataType = extension.ToUpper();
			SetDefaultSC_FileNameForRenaming();
		}

		public static string ReplaceIllegalCharsInFileNameWithDash(string fileName)
		{
			return MakeFilenameSafe.MakeSafe(fileName, CharToReplaceIllegalCharsInFileName);
		}

		/// <summary>
		/// Truncates the extension to the StorageDocsSchema.SC_DataType.MaxLength and removes the '.'
		/// </summary>
		public static string GetExtensionFromFilename(string fileName, bool truncate = true)
		{
			fileName = ReplaceIllegalCharsInFileNameWithDash(fileName);
			var extension = Path.GetExtension(fileName);

			return extension.Length > 0 ? ((ZString)extension).SubstringSafe(1, truncate ? StorageDocsSchema.SC_DataType.MaxLength : extension.Length).ToUpper() : ZString.Empty;
		}

		/// <summary>
		/// Truncates the filename to the StorageDocsSchema.SC_FileName.MaxLength 
		/// </summary>
		public static string GetFileNameOnlyFromFilename(string fileName, bool truncate = true)
		{
			fileName = ReplaceIllegalCharsInFileNameWithDash(fileName);
			return ((ZString)Path.GetFileNameWithoutExtension(fileName)).SubstringSafe(0, truncate ? StorageDocsSchema.SC_FileName.MaxLength : fileName.Length).Trim();
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("813364e9-e447-4b0e-9cca-259d7f2aeefa", "File"); }
		}

		public override bool IsImageFile
		{
			get { return false; }
		}

		public override void OnSaving()
		{
			base.OnSaving();

			// We can't use SC_ImageDataInfo.HasChanges, because it compares the SC_ImageData to the original value in DB.
			// If a file is in S3 bucket, the original value is empty in DB, while SC_ImageData will return the file from S3,
			// then SC_ImageDataInfo.HasChanges is always true.
			if (!IsMovingToExternalStorage && IsInDatabase && hasSC_ImageDataBeenUpdated)
			{
				SC_Date = TimeFactory.CurrentUtcDateTime;
			}
		}

		#region Document Types
#if DEBUG
		public
#else
		protected internal
#endif
		override bool SC_IsPublishedReadonlyDefault
		{
			get { return true; }
		}

		protected override void UpdatePublishedAndSaveVersionFlags()
		{
			base.UpdatePublishedAndSaveVersionFlags();

			if (DocType == null)
			{
				SC_IsPublished = false;
				SC_SaveVersions = false;
			}
		}

		RefDocType refDocType
		{
			get
			{
				ZQuery query = new ZQuery(RefDocTypeSchema.RT_IsActive, true);
				query.AddToFilter(RefDocTypeSchema.RT_IsSystem, true);
				query.AddToFilter(RefDocTypeSchema.RT_DocType, SC_DocType);
				query.AddToFilter(RefDocTypeSchema.RT_ReferenceType, Core.Constants.ReferenceTypes.SupplyChainLogistics);
				return MasterFactory.LoadTop1<RefDocType>(query);
			}
		}

#endregion

		#region Lock

		public IDisposable Lock()
		{
			lock (this)
			{
				if (isLocked)
				{
					return null;
				}

				isLocked = true;
				return new DisposableAction(
					delegate
					{
						lock (this)
						{
							isLocked = false;
						}
					});
			}
		}

		bool isLocked;

		#region For Testing
#if DEBUG
		public bool IsLocked { get { return isLocked; } }
#endif
#endregion

		#endregion

		#region IDeliverable Members

		protected override DeliveryInfo.DeliveryFormats DeliveryFormats
		{
			get { return DeliveryInfo.DeliveryFormats.File; }
		}

		bool IsHPPClPrintFileType
		{
			get { return refDocType != null && refDocType.RT_HPPclPrintFile; }
		}

		protected override IEnumerable<string> GetSupportedDeliveryMethodsCore()
		{
			if (IsHPPClPrintFileType)
			{
				return new string[] { ContactNotifyModes.Print };
			}
			else if (EDocFormat.EqualsIgnoringCase(FileFormats.PDF))
			{
				return new string[] { ContactNotifyModes.Print, ContactNotifyModes.EPrint, ContactNotifyModes.Email };
			}
			else if (EDocFormat.EqualsIgnoringCase(FileFormats.XLS) || EDocFormat.EqualsIgnoringCase(FileFormats.XLSX))
			{
				return new string[] { ContactNotifyModes.Print, ContactNotifyModes.EPrint, ContactNotifyModes.Email };
			}
			else
			{
				return new string[] { ContactNotifyModes.Email, ContactNotifyModes.EPrint };
			}
		}

		protected override ZString HackedFileExtensionOnlyForIDeliverable => SC_DataType;

		protected override ZString NameCore
		{
			get { return SC_FileNameWithExtension; }
		}

		#endregion
	}
}
