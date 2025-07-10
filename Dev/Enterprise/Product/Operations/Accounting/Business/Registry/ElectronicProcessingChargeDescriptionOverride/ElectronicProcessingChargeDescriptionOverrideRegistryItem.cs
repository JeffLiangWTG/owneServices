using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class ElectronicProcessingChargeDescriptionOverrideRegistryItem : StronglyTypedRegistryItem<ElectronicProcessingChargeDescriptionOverrideCollection>
	{
		public ElectronicProcessingChargeDescriptionOverrideRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
		: base(new ElectronicProcessingChargeDescriptionOverrideRegistryItemImpl(name, category, caption, hint, storage, option))
		{
		}

		class ElectronicProcessingChargeDescriptionOverrideRegistryItemImpl : RegistryItemImpl
		{
			public ElectronicProcessingChargeDescriptionOverrideRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new ElectronicProcessingChargeDescriptionOverrideRegistryDataType(), storage, options)
			{
			}

			public override bool IsVisible(Guid companyPK, Guid branchPK, Guid departmentPK, IGlbDepartment department, IRegistryItemVisibility registryItemVisibility)
			{
				return AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ElectronicProcessingChargeDescriptionOverrideRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class ElectronicProcessingChargeDescriptionOverrideRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ElectronicProcessingChargeDescriptionOverrideCollection>
	{
	}
}
