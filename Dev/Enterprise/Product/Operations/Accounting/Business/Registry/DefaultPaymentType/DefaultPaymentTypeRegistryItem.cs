using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.Registry
{
	public class DefaultPaymentTypeRegistryItem : CodePairRegistryItem
	{
		public DefaultPaymentTypeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, ICodeDescriptionPairListProvider lookUpListProvider, RegistryStorageFlags storage, string defaultValue)
			: base(new DefaultPaymentTypeRegistryItemImpl(name, category, caption, hint, storage, defaultValue, lookUpListProvider))
		{
		}
	}

	class DefaultPaymentTypeRegistryItemImpl : RegistryItemImpl
	{
		public DefaultPaymentTypeRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, object defaultValue, ICodeDescriptionPairListProvider lookUpListProvider)
			: base(name, category, caption, hint, new CodePairRegistryDataType(lookUpListProvider, false, false), storage, defaultValue)
		{
			paymentTypeListProvider = lookUpListProvider as DefaultPaymentTypeListProvider;
		}

		protected DefaultPaymentTypeListProvider paymentTypeListProvider;

		protected override Guid GetRegistryItemPKCore(Guid companyPk, Guid branchPk, Guid departmentPk)
		{
			paymentTypeListProvider.SetCompanyPk(companyPk);
			return base.GetRegistryItemPKCore(companyPk, branchPk, departmentPk);
		}
	}
}
