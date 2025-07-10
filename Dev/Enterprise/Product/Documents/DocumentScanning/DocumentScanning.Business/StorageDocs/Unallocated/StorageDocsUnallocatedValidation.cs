using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDocsUnallocatedValidation : StorageDocsBaseValidation
	{
		public StorageDocsUnallocatedValidation(StorageDocsUnallocated parent)
			: base(parent)
		{
		}

		public new StorageDocsUnallocated Parent
		{
			get { return (StorageDocsUnallocated)base.Parent; }
		}

		protected override void CheckSC_ImageDataIsValidZBlobSize()
		{
			//Override to avoid ZBlob size - check validation(ie no call to base), as we need to add any blob size to an unallocated document without warnings appearing.
		}

		protected override void CheckSC_DataType()
		{
			base.CheckSC_DataType();
			if (!Parent.SC_IsDeleted && (!AssemblyDataLookup.IsDocManagerCodeValid(Parent.SM_Type, false) || !Parent.SC_FormCategory_List.ContainsCode(Parent.SM_Type)))
			{
				Parent.SM_TypeInfo.AddError(Res.GetString("390b27a9-7c6a-4db0-8408-16a9958e2a11", "Enter a valid Type."));
				Parent.enteredTypeValid = false;
			}
			else
			{
				Parent.enteredTypeValid = true;
			}
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
			}
		}

		protected override void CheckSC_FileName()
		{
			base.CheckSC_FileName();

			if (!Parent.IsObsolete && !Parent.SC_IsDeleted && !Parent.IsDeleting)
			{
				MandatoryValidation.CheckEntered(Parent.SC_FileNameInfo);
			}
		}

		protected override void CheckSC_SMIsNotEmpty()
		{
			// not mandatory for unallocated document
		}
	}
}
