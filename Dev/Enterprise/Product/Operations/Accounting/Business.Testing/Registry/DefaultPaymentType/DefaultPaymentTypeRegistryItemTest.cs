using System;
using Enterprise.Accounting.Business.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(DefaultPaymentTypeRegistryItem))]
	class DefaultPaymentTypeRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		public void TestGetCodeDescriptionPairList_SystemLevel()
		{
			AssertCodeDescriptionPairList(false, true);
			AssertCodeDescriptionPairList(false, false);
		}

		public void TestGetCodeDescriptionPairList_CompanyLevel()
		{
			AssertCodeDescriptionPairList(true, true);
			AssertCodeDescriptionPairList(true, false);
		}

		void AssertCodeDescriptionPairList(bool isCompanyLevel, bool isOFXEPaymentEnabled)
		{
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.Instance.CurrentCompany.PK, isOFXEPaymentEnabled))
			{
				var implForTest = new DefaultPaymentTypeRegistryItemImplForTest(string.Empty, null, null, null, RegistryStorageFlags.System, null, new DefaultPaymentTypeListProvider());
				if (isCompanyLevel)
				{
					implForTest.GetRegistryItemPKCoreForTest(Env.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				}
				else
				{
					implForTest.GetRegistryItemPKCoreForTest(Guid.Empty, Guid.Empty, Guid.Empty);
				}

				var list = implForTest.LookUp;
				if (isCompanyLevel && isOFXEPaymentEnabled)
				{
					AssertEquals(19, list.Count);
				}
				else
				{
					AssertEquals(18, list.Count);
				}
			}
		}

		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new DefaultPaymentTypeRegistryItem(string.Empty, null, null, null, new DefaultPaymentTypeListProvider(), RegistryStorageFlags.System, string.Empty);
		}

		public class DefaultPaymentTypeRegistryItemImplForTest : DefaultPaymentTypeRegistryItemImpl
		{
			public DefaultPaymentTypeRegistryItemImplForTest(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, object defaultValue, ICodeDescriptionPairListProvider lookUpList)
			: base(name, category, caption, hint, storage, defaultValue, lookUpList)
			{
			}

			public Guid GetRegistryItemPKCoreForTest(Guid companyPk, Guid branchPk, Guid departmentPk)
			{
				return GetRegistryItemPKCore(companyPk, branchPk, departmentPk);
			}

			public CodeDescriptionPairList LookUp => paymentTypeListProvider.CodeDescriptionPairList;
		}
	}
}
