using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CalculateDeliveryDateWithExceptionsOptionsRegistryItem : StronglyTypedRegistryItem<CalculateDeliveryDateWithExceptionsOptions>
	{
		public CalculateDeliveryDateWithExceptionsOptionsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions registryOptions, CalculateDeliveryDateWithExceptionsOptions defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CalculateDeliveryDateWithExceptionsOptionsRegistryItemDataType(defaultValue, 0, CalculateDeliveryDateWithExceptionsOptions.MaximumDurationHoursUpperBound), storage, registryOptions))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.CalculateDeliveryDateWithExceptionsOptionsRegistryItemEditor, Enterprise.Registry.GUI")]
	public class CalculateDeliveryDateWithExceptionsOptionsRegistryItemDataType : NonPersistentBusinessObjectRegistryDataType<CalculateDeliveryDateWithExceptionsOptions>
	{
		readonly int lowerBound;
		readonly int upperBound;

		public CalculateDeliveryDateWithExceptionsOptionsRegistryItemDataType(CalculateDeliveryDateWithExceptionsOptions defaultValue) : this(defaultValue, int.MinValue, int.MaxValue)
		{
		}

		public CalculateDeliveryDateWithExceptionsOptionsRegistryItemDataType(CalculateDeliveryDateWithExceptionsOptions defaultValue, int lowerBound, int upperBound)
			: base(defaultValue)
		{
			this.lowerBound = lowerBound;
			this.upperBound = upperBound;
		}

		protected override void ValidateCore(IRegistryItem registryItem, CalculateDeliveryDateWithExceptionsOptions proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			int value = Convert.ToInt32(proposedValue.MaximumDurationHours);

			if (value < lowerBound)
			{
				throw new RegistryValidationException(Res.GetString("da170abe-4845-4e1c-a4e0-f3244c743a22", "Value must be greater than or equal to the minimum ({0})", lowerBound));
			}

			if (value > upperBound)
			{
				throw new RegistryValidationException(Res.GetString("e002f4f0-4ec4-4aee-8d13-a78f4e071411", "Value must be less than or equal to the maximum ({0})", upperBound));
			}
		}
	}
}
