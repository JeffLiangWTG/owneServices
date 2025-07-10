using System;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.ElectronicMessaging.Registry
{
	class EnableEInvoicingFunctionalityDateTimeRegistryItem : DateTimeRegistryItem
	{
		public EnableEInvoicingFunctionalityDateTimeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new EnableEInvoicingFunctionalityDateTimeRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}
	}

	class EnableEInvoicingFunctionalityDateTimeRegistryItemImpl : RegistryItemImpl
	{
		public EnableEInvoicingFunctionalityDateTimeRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(name, category, caption, hint, RegistryDataTypes.DateTimeType, new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Short), storage, options, DateTime.MinValue)
		{
		}

		public override bool IsVisible(Guid companyPk, Guid branchPk, Guid departmentPk, IGlbDepartment department, IRegistryItemVisibility registryItemVisibility)
		{
			return base.IsVisible(companyPk, branchPk, departmentPk, department, registryItemVisibility) && AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetValueWithoutFallback(companyPk, Guid.Empty, Guid.Empty);
		}
	}
}
