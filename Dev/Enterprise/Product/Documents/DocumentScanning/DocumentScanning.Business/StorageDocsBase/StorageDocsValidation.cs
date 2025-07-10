using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDocsValidation : AutoStorageDocsValidation
	{
		public StorageDocsValidation(AutoStorageDocs parent)
			: base(parent)
		{
		}

		protected override void CheckSC_ParentIDIsValidZGuid()
		{
			if (!Parent.SC_IsDeleted)
			{
				base.CheckSC_ParentIDIsValidZGuid();
			}
		}

		protected override void CheckSC_DateIsValidZDateTime()
		{
			if (!Parent.SC_IsDeleted)
			{
				TypeValidation.CheckValidZDateTimeWithoutRange(Parent.SC_DateInfo);
			}
		}

		protected override void CheckSC_DateIsNotEmpty()
		{
			if (!Parent.SC_IsDeleted)
			{
				MandatoryValidation.CheckEntered(Parent.SC_DateInfo);
			}
		}

		protected override void CheckSC_DocType()
		{
			if (!Parent.SC_IsDeleted)
			{
				base.CheckSC_DocType();
				var doc = Parent as StorageDocsBase;
				if (doc != null)
				{
					if (!doc.SC_DocTypeInfo.HasErrors() && !doc.SC_DocTypeInfo.HasWarnings() && doc.DocType != null)
					{
						if (doc.SC_IsPublished != doc.DocType.RT_IsPublished)
						{
							doc.SC_DocTypeInfo.AddWarning(Res.GetString("1D261727-D78F-475D-883C-4BC6C8555A8C", "The Published status for the document has been changed from the {0} Document Type's published value.", doc.SC_DocType));
						}
					}

					if (doc.IsSupersededByNewVersion)
					{
						doc.SC_DocTypeInfo.AddWarning(Res.GetString("1AAB8C8F-4788-4EEA-A609-92454056A101", "This document has been superseded by a newer version and has been unpublished."));
					}
				}
			}

			if (!Parent.SC_IsSystemGenerated
				&& Parent.SC_DocType == Core.Constants.RefDocTypes.SystemQuotation)
			{
				Parent.SC_DocTypeInfo.AddError(Res.GetString("069d3265-e8c4-4cb2-b6f1-e8ae8203d764", "The Doc Type is only for System Use"));
			}

			if (Parent.SC_IsSystemGenerated
				&& (ZString)Parent.SC_DocTypeInfo.OriginalValue == Core.Constants.RefDocTypes.SystemQuotation
				&& Parent.SC_DocType != Core.Constants.RefDocTypes.SystemQuotation)
			{
				Parent.SC_DocTypeInfo.AddError(Res.GetString("2ec119ff-93ea-4612-a7b5-60dc495381af", "The SQTE Document Type cannot be modified"));
			}
		}

		protected override void CheckSC_DateIsValidZDateTimeRange()
		{
			if (!Parent.SC_IsDeleted)
			{
				TypeValidation.CheckValidZDateTimeRange(Parent.SC_DateInfo);
			}
		}

		public virtual void ValidateSC_FileNameForRenaming()
		{
		}

		protected BusinessObjectFactory Factory
		{
			get
			{
				if (Parent.Factory is NumberedBusinessObjectFactory numberedBusinessObjectFactory)
				{
					return numberedBusinessObjectFactory.MasterFactory;
				}
				return Parent.Factory;
			}
		}
	}
}
