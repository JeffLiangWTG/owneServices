using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CalculateDeliveryDueDateOptionsRegistryItem : StronglyTypedRegistryItem<CalculateDeliveryDueDateOptions>
	{
		public CalculateDeliveryDueDateOptionsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions registryOptions, CalculateDeliveryDueDateOptions defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CalculateDeliveryDueDateOptionsRegistryItemDataType(defaultValue), storage, registryOptions))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.CalculateDeliveryDueDateOptionsRegistryItemEditor, Enterprise.Registry.GUI")]
	public class CalculateDeliveryDueDateOptionsRegistryItemDataType : NonPersistentBusinessObjectRegistryDataType<CalculateDeliveryDueDateOptions>
	{
		public CalculateDeliveryDueDateOptionsRegistryItemDataType(CalculateDeliveryDueDateOptions defaultValue)
			: base(defaultValue)
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, CalculateDeliveryDueDateOptions proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (proposedValue.IsActive)
			{
				var isValid = false;
				for (int i = 0; i < proposedValue.TransportModes.Count; i++)
				{
					if (proposedValue.TransportModes[i].Enabled)
					{
						isValid = true;
					}
				}

				if (!isValid)
				{
					throw new RegistryValidationException(Res.GetString("B001f4f0-4ec4-4ae2-8d13-a78f4e071421", "When the Registry for calculation of Delivery Due Date is enabled, at least one Transport Mode must be selected."));
				}
			}
		}
	}
}




