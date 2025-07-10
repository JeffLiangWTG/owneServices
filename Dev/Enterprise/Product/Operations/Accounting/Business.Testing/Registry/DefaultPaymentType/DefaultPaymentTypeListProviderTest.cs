using Enterprise.Accounting.Business.Registry;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	class DefaultPaymentTypeListProviderTest : TransactionedTestCase
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
			var provider = new DefaultPaymentTypeListProvider();
			if (isCompanyLevel)
			{
				provider.SetCompanyPk(Env.Instance.CurrentCompany.PK);
			}

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.Instance.CurrentCompany.PK, isOFXEPaymentEnabled))
			{
				var list = provider.CodeDescriptionPairList;
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
	}
}
