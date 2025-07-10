using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class ChargeCodeInvoiceTaxMessageOverrideRegistryItem : StronglyTypedRegistryItem<ChargeCodeInvoiceTaxMessageOverrideCollection>
	{
		public ChargeCodeInvoiceTaxMessageOverrideRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new ChargeCodeInvoiceTaxMessageOverrideRegistryItemImpl(name, category, caption, hint, storage))
		{
		}

		public class ChargeCodeInvoiceTaxMessageOverrideRegistryItemImpl : RegistryItemImpl
		{
			public ChargeCodeInvoiceTaxMessageOverrideRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
				: base(name, category, caption, hint, new ChargeCodeInvoiceTaxMessageOverrideRegistryDataType(), storage)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				return new ChargeCodeInvoiceTaxMessageOverrideCollection(new FallbackLevel(companyPK, branchPK, departmentPK), new BusinessObjectFactory());
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ChargeCodeInvoiceTaxMessageOverrideRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class ChargeCodeInvoiceTaxMessageOverrideRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ChargeCodeInvoiceTaxMessageOverrideCollection>
	{
	}
}
