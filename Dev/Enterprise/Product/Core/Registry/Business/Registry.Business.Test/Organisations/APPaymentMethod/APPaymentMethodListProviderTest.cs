using CargoWise.Application;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class APPaymentMethodListProviderTest : TransactionedTestCase
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
			var provider = new APPaymentMethodListProvider();
			if (isCompanyLevel)
			{
				provider.SetCompanyPk(EnvProxy.Instance.CurrentCompany.PK);
			}

			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(EnvProxy.Instance.CurrentCompany.PK, isOFXEPaymentEnabled))
			{
				var list = provider.CodeDescriptionPairList;
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
	}
}
