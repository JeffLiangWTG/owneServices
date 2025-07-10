using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(APPaymentMethodRegistryItemImplForTest))]
	sealed class APPaymentMethodRegistryItemImplForTest : APPaymentMethodRegistryItemImpl
	{
		public APPaymentMethodRegistryItemImplForTest(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, object defaultValue, ICodeDescriptionPairListProvider lookUpList)
		: base(name, category, caption, hint, storage, options, defaultValue, lookUpList)
		{
		}

		public Guid GetRegistryItemPKCoreForTest(Guid companyPk, Guid branchPk, Guid departmentPk)
		{
			return GetRegistryItemPKCore(companyPk, branchPk, departmentPk);
		}

		public CodeDescriptionPairList LookUp => paymentMethodListProvider.CodeDescriptionPairList;
	}
}
