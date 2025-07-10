using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ContainerPenaltyFreeDaysOptionsRegistryItem : StronglyTypedRegistryItem<ContainerPenaltyFreeDaysOptions>
	{
		public ContainerPenaltyFreeDaysOptionsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ContainerPenaltyFreeDaysOptions defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ContainerPenaltyFreeDaysOptionsRegistryItemDataType(defaultValue, 0, 255), storage, RegistryOptions.PreserveTestValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.ContainerPenaltyFreeDaysOptionsRegistryItemEditor, Enterprise.Registry.GUI")]
	public class ContainerPenaltyFreeDaysOptionsRegistryItemDataType : NonPersistentBusinessObjectRegistryDataType<ContainerPenaltyFreeDaysOptions>
	{
		readonly int lowerBound;
		readonly int upperBound;

		public ContainerPenaltyFreeDaysOptionsRegistryItemDataType(ContainerPenaltyFreeDaysOptions defaultValue) : this(defaultValue, int.MinValue, int.MaxValue)
		{
		}

		public ContainerPenaltyFreeDaysOptionsRegistryItemDataType(ContainerPenaltyFreeDaysOptions defaultValue, int lowerBound, int upperBound)
			: base(defaultValue)
		{
			this.lowerBound = lowerBound;
			this.upperBound = upperBound;
		}

		protected override void ValidateCore(IRegistryItem registryItem, ContainerPenaltyFreeDaysOptions proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			double value = Convert.ToDouble(proposedValue.FreeDays);

			if (value < lowerBound)
			{
				throw new RegistryValidationException(Res.GetString("da170abe-4845-4e1c-a4e0-f3244c743e57", "Value must be greater than or equal to the minimum ({0})", lowerBound));
			}

			if (value > upperBound)
			{
				throw new RegistryValidationException(Res.GetString("e002f4f0-4ec4-4aee-8d13-a78f4e0712c8", "Value must be less than or equal to the maximum ({0})", upperBound));
			}
		}
	}
}
