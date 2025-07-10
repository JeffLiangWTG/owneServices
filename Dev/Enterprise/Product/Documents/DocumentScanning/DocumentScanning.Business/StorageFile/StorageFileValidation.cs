using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageFileValidation : StorageDocsBaseValidation
	{
		public StorageFileValidation(StorageFile parent)
			: base(parent)
		{
		}

		new StorageFile Parent
		{
			get { return (StorageFile)base.Parent; }
		}

		#region SC_ImageData
		protected override void CheckSC_ImageDataIsNotEmpty()
		{
		}

		#endregion

		#region SC_DocType

		protected override void CheckSC_DocType()
		{
			base.CheckSC_DocType();

			if (IsNewOrChangedRecord && !Parent.IsObsolete && !Parent.SC_IsDeleted && Parent.ParentMain != null && Parent.ParentMain.SM_DB != 0)
			{
				if (!Parent.ReadOnly)
				{
					var checkpoint = Env.Security.GetDocumentTypeUploadCheckPoint(Parent.SC_DocType);
					if (!checkpoint.IsAllowed)
					{
						Parent.SC_DocTypeInfo.AddError(checkpoint.ErrorMessageForNotAllowed);
					}
				}
				if (!Parent.SC_DocTypeInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidCode(Parent.SC_DocTypeInfo, Parent.SC_DocType_List);
				}

				if (!Parent.SC_DocTypeInfo.HasErrors())
				{
					MandatoryValidation.CheckEntered(Parent.SC_DocTypeInfo);
				}
			}
		}

		#endregion

		#region SC_RDS_NKDocSource

		protected override void CheckSC_RDS_NKDocSource()
		{
			base.CheckSC_RDS_NKDocSource();

			if (IsNewOrChangedRecord && !Parent.IsObsolete && !Parent.SC_IsDeleted && Parent.ParentMain != null && Parent.ParentMain.SM_DB != 0)
			{
				if (!Parent.SC_RDS_NKDocSourceInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidCode(Parent.SC_RDS_NKDocSourceInfo, Parent.SC_DocSource_List);
				}
			}
		}

		#endregion

		#region SC_Desc

		protected override void CheckSC_Desc()
		{
			base.CheckSC_Desc();

			if (!Parent.IsObsolete && !Parent.SC_IsDeleted && Parent.ParentMain != null && Parent.ParentMain.SM_DB != 0)
			{
				MandatoryValidation.CheckEntered(Parent.SC_DescInfo);
			}
		}

		#endregion

		#region SC_FileNameForRenaming

		protected void CheckSC_FileNameForRenaming()
		{
			if (!FileNameForRenamingValidationDisabled)
			{
				ValidateFileNameUniqueInCollection();

				if (Parent.SC_FileNameForRenaming.LastIndexOf('.') == -1)
				{
					Parent.SC_FileNameForRenamingInfo.AddError(Res.GetString("662a924d-f311-4f25-9841-dd987145f4bd", "You must specify a file extension e.g. '.doc'"));
				}
				else
				{
					if (!IsValidFileName(Parent.SC_FileNameForRenaming))
					{
						Parent.SC_FileNameForRenamingInfo.AddError(Res.GetString("beecb221-08f9-4d4d-be95-5546517fd88d", "Filename contains Invalid character(s). The name should be a Windows compatible file name."));
					}
					else if (CargoWise.IO.MakeFilenameSafe.IsWindowsReservedFileName(Parent.SC_FileNameForRenaming))
					{
						Parent.SC_FileNameForRenamingInfo.AddError(Res.GetString("9F57AC7B-036F-468A-8B8D-D233D1BEDB91", "Filename is Microsoft MS-DOS reserved. Please choose another filename."));
					}
					else
					{
						ZString newName = StorageFile.GetFileNameOnlyFromFilename(Parent.SC_FileNameForRenaming, false);
						ZString newExtension = StorageFile.GetExtensionFromFilename(Parent.SC_FileNameForRenaming, false);

						if (newName.IsEmpty || newExtension.IsEmpty)
						{
							Parent.SC_FileNameForRenamingInfo.AddError(Res.GetString("33b60436-c507-4cfa-9299-d54fd95ce7c8", "You must specify a valid file name with extension e.g. 'Quote.doc'"));
						}

						if (Parent.SC_FileNameForRenaming.Length > StorageDocs.Schema.SC_FileNameMaxLength - 1)
						{
							Parent.SC_FileNameForRenamingInfo.AddError(Res.GetString("6cd9b390-b531-4980-8307-48cf5cdddf6f", "This filename is longer than {0} characters", StorageDocs.Schema.SC_FileNameMaxLength - 1));
						}
						else if (new FileTypeValidation().IsDangerousFile(Parent.SC_FileNameForRenaming))
						{
							Parent.SC_FileNameForRenamingInfo.AddError(Res.GetString("2E7675D2-AF59-4123-80B3-6108BD34CDE8", "The file cannot be renamed to {0} as it is a potentially dangerous file type.", Parent.SC_FileNameForRenaming));
						}

						if (newExtension.Length > StorageDocs.Schema.SC_DataTypeMaxLength)
						{
							Parent.SC_FileNameForRenamingInfo.AddWarning(Res.GetString("20c3491c-e764-4bbf-8538-ec649db50fde", "The filename extension is longer than {0} characters", StorageDocs.Schema.SC_DataTypeMaxLength));
						}

						if (newExtension != Parent.SC_DataType)
						{
							Parent.SC_FileNameForRenamingInfo.AddWarning(Res.GetString("eec6f595-35fa-4979-a9b4-0d708294d929", "Changing the file extension from '{0}' may make the file unusable.", Parent.SC_DataType));
						}
					}
				}
			}
		}

		void ValidateFileNameUniqueInCollection()
		{
			if (Parent.ParentMain?.eDocsView != null && IsValidFileName(Parent.SC_FileNameForRenaming))
			{
				var filenameWithoutExtension = Path.GetFileNameWithoutExtension(Parent.SC_FileNameForRenaming);
				var extension = Path.GetExtension(Parent.SC_FileNameForRenaming);
				var file = Parent.ParentMain.eDocsView.FindDocByName(filenameWithoutExtension, extension);

				if (file != null && file.PK != Parent.PK)
				{
					Parent.SC_FileNameForRenamingInfo.AddError(Res.GetString("cef6609c-82f9-45a0-89dd-6bcdfbc47406", "This file name already exists. Please choose another filename. If you cannot see that file, then it's probably set to visible only to specific Company / Branch / Department or unpublished."));
				}
			}
		}

		bool IsValidFileName(ZString fileName)
		{
			return !(fileName.ContainsAnyChar(new string(Path.GetInvalidFileNameChars())));
		}

		bool FileNameForRenamingValidationDisabled;

		void ValidateSC_FileNameForRenaming_Empty()
		{
			// just to force a clear notifications on the property - but we don't want to validate it
			FileNameForRenamingValidationDisabled = true;
			try
			{
				ValidateSC_FileNameForRenaming();
			}
			finally
			{
				FileNameForRenamingValidationDisabled = false;
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateSC_FileNameForRenaming_Empty();
		}

		#endregion
	}
}
