using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class DocumentPreviewFormLayoutRegistryItem : StronglyTypedRegistryItem<DocumentPreviewFormLayout>
	{
		public DocumentPreviewFormLayoutRegistryItem(string name)
			: base(new RegistryItemImpl(name, null, null, null, new DocumentPreviewFormLayoutRegistryDataType(), RegistryStorageFlags.Company, RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue))
		{
		}

		public DocumentPreviewFormLayout UserValue
		{
			get { return GetValueWithoutFallback(Env.CurrentUser.PK, Guid.Empty, Guid.Empty); }
			set { SetValue(Env.CurrentUser.PK, Guid.Empty, Guid.Empty, value); }
		}

		public bool IsSetByCurrentUser
		{
			get { return Inner.HasActualValue(Env.CurrentUser.PK, Guid.Empty, Guid.Empty); }
		}
	}

	class DocumentPreviewFormLayoutRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DocumentPreviewFormLayout>
	{
		public DocumentPreviewFormLayoutRegistryDataType()
		{
		}
	}
}
