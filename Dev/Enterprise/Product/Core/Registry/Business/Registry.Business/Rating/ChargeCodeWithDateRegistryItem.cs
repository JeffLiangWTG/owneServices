using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ChargeCodeWithDateRegistryItem : StronglyTypedRegistryItem<ChargeCodeWithDate, ChargeCodeWithDate>
	{
		public ChargeCodeWithDateRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new ChargeCodeWithDateRegistryDataType(), storage, options))
		{
		}
	}

	public class ChargeCodeWithDateRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ChargeCodeWithDate>
	{
		public ChargeCodeWithDateRegistryDataType()
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, ChargeCodeWithDate proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			if (proposedValue.ChargeCode == Guid.Empty && IsValueMandatory(registryItem))
			{
				throw new RegistryValidationException(Res.GetString("CD4A9804-B9F5-4BBC-A9D5-5F9DF1B60FC0", "Please select a valid selection."));
			}
		}

		bool IsValueMandatory(IRegistryItem registryItem)
		{
			return registryItem?.IsValueMandatory ?? false;
		}
	}
}
