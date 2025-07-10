using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Environment;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDocsRealValidation : StorageDocsBaseValidation
	{
		public StorageDocsRealValidation(StorageDocs parent)
			: base(parent)
		{
		}

		new StorageDocs Parent
		{
			get { return (StorageDocs)base.Parent; }
		}

		protected override void CheckSC_DocType()
		{
			base.CheckSC_DocType();

			if (!Parent.IsObsolete && !Parent.SC_IsDeleted && !Parent.IsPrivateDocument)
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

				if (!Parent.SC_DocTypeInfo.HasErrors() && Parent.IsAllocated)
				{
					MandatoryValidation.CheckEntered(Parent.SC_DocTypeInfo);
				}
			}
		}

		protected override void CheckSC_ImageDataIsNotEmpty()
		{
			// Not mandatory - so don't call base
		}

		protected override void CheckSC_Desc()
		{
			base.CheckSC_Desc();

			if (!Parent.IsObsolete && !Parent.SC_IsDeleted && Parent.IsAllocated)
			{
				MandatoryValidation.CheckEntered(Parent.SC_DescInfo);
			}
		}

		protected override void CheckSC_FileName()
		{
			base.CheckSC_FileName();

			if (!Parent.IsObsolete && !Parent.SC_IsDeleted && !Parent.IsDeleting)
			{
				if (!Parent.IsInDatabase || !Equals(Parent.SC_FileName, Parent.SC_FileNameInfo.OriginalValue))
				{
					MandatoryValidation.CheckEntered(Parent.SC_FileNameInfo);
				}
				else
				{
					MandatoryValidation.WarnIfNotEntered(Parent.SC_FileNameInfo);
				}
			}
		}

		protected void CheckSC_FileNameForRenaming()
		{
			var fileName = Parent.SC_FileNameForRenaming;
			if (!MakeFilenameSafe.IsSafe(fileName))
			{
				Parent.SC_FileNameForRenamingInfo.AddError(Res.GetString("4E62DD6B-1299-4CE8-A822-024E5C9342EA", "Filename contains Invalid character(s). The name should be a Windows compatible file name."));
			}
			else if (MakeFilenameSafe.IsWindowsReservedFileName(fileName))
			{
				Parent.SC_FileNameForRenamingInfo.AddError(Res.GetString("FA27C466-7004-489F-926C-3913281C31C2", "Filename is Microsoft MS-DOS reserved. Please choose another filename."));
			}
			else
			{
				var extension = Path.GetExtension(Parent.SC_FileNameWithExtension);
				var document = Parent.ParentMain?.eDocsView.FindDocByName(Parent.SC_FileNameForRenaming, extension);
				if (document != null && Parent.PK != document.PK)
				{
					Parent.SC_FileNameForRenamingInfo.AddError(Res.GetString("B0E37F38-669B-4822-8F9F-D61AD96248B3",
						"This file name already exists. Please choose another filename. If you cannot see that file, then it's probably set to visible only to specific Company / Branch / Department or unpublished."));
				}
			}
		}
	}
}
