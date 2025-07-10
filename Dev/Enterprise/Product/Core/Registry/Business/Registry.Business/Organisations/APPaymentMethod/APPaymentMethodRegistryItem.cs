using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class APPaymentMethodRegistryItem : CodePairRegistryItem
	{
		public APPaymentMethodRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, ICodeDescriptionPairListProvider lookUpList, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
			: base(new APPaymentMethodRegistryItemImpl(name, category, caption, hint, storage, options, defaultValue, lookUpList))
		{
		}
	}

	class APPaymentMethodRegistryItemImpl : RegistryItemImpl
	{
		public APPaymentMethodRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, object defaultValue, ICodeDescriptionPairListProvider lookUpList)
			: base(name, category, caption, hint, new CodePairRegistryDataType(lookUpList, false, false), storage, options, defaultValue)
		{
			paymentMethodListProvider = lookUpList as APPaymentMethodListProvider;
		}

		protected APPaymentMethodListProvider paymentMethodListProvider;

		protected override Guid GetRegistryItemPKCore(Guid companyPk, Guid branchPk, Guid departmentPk)
		{
			paymentMethodListProvider.SetCompanyPk(companyPk);
			return base.GetRegistryItemPKCore(companyPk, branchPk, departmentPk);
		}
	}
}
