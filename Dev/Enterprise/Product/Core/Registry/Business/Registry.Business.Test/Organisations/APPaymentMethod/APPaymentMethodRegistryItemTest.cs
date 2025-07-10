using System;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(APPaymentMethodRegistryItemImplForTest))]
	sealed class APPaymentMethodRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
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
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(EnvProxy.Instance.CurrentCompany.PK, isOFXEPaymentEnabled))
			{
				var implForTest = new APPaymentMethodRegistryItemImplForTest(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, null, new APPaymentMethodListProvider());
				if (isCompanyLevel)
				{
					implForTest.GetRegistryItemPKCoreForTest(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				}
				else
				{
					implForTest.GetRegistryItemPKCoreForTest(Guid.Empty, Guid.Empty, Guid.Empty);
				}

				var list = implForTest.LookUp;
				if (isCompanyLevel && isOFXEPaymentEnabled)
				{
					AssertEquals(4, list.Count);
					AssertContainsExactElementsInAnyOrder(new[] { ReceiptTypes.Cheque, ReceiptTypes.DirectDebit, ReceiptTypes.eNettDirectDebit, EPaymentMethods.EPaymentViaOFX }, list.GetAllCodes());
				}
				else
				{
					AssertEquals(3, list.Count);
					AssertContainsExactElementsInAnyOrder(new[] { ReceiptTypes.Cheque, ReceiptTypes.DirectDebit, ReceiptTypes.eNettDirectDebit }, list.GetAllCodes());
				}
			}
		}

		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new APPaymentMethodRegistryItem(string.Empty, null, null, null, new APPaymentMethodListProvider(), RegistryStorageFlags.System, RegistryOptions.Default, string.Empty);
		}
	}
}
