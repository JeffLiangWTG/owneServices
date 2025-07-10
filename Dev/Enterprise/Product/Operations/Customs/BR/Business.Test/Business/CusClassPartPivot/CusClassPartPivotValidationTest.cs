using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class CusClassPartPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestFormatTariffForSaving()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "1234.56.78";
			AssertEquals("Keep Numeric Only", "12345678", pivot.CI_TariffNum);
		}

		public void TestTariffOrClassificationOrGoodsCatalogIsEntered()
		{
			var pivot = Factory.New<CusClassPartPivot>();

			using (Registry.BRCustomsDataRegistry.Instance.EnableCatalogModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				pivot.CI_TariffNum = ZString.Empty;
				pivot.CI_CC = ZGuid.Empty;

				AssertHasError(pivot.CI_TariffNumInfo, "One of Tariff or Classification is Mandatory");
				AssertNoNotifications(pivot.CI_CGC_CatalogInfo);

				pivot.CI_TariffNum = "1234.56.78";
				pivot.CI_CC = ZGuid.Empty;

				AssertNoError(pivot.CI_TariffNumInfo, "One of Tariff or Classification is Mandatory");
				AssertNoNotifications(pivot.CI_CGC_CatalogInfo);

				pivot.CI_TariffNum = ZString.Empty;
				pivot.CI_CC = Factory.New<CusClassification>().PK;
				pivot.CI_CGC_Catalog = ZGuid.Empty;

				AssertNoError(pivot.CI_TariffNumInfo, "One of Tariff or Classification is Mandatory");
				AssertNoNotifications(pivot.CI_CGC_CatalogInfo);
			}

			using (Registry.BRCustomsDataRegistry.Instance.EnableCatalogModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				pivot.CI_TariffNum = ZString.Empty;
				pivot.CI_CC = ZGuid.Empty;
				pivot.CI_CGC_Catalog = ZGuid.Empty;

				AssertHasError(pivot.CI_CGC_CatalogInfo, "One of Tariff or Classification or Goods Catalog is Mandatory.");
				AssertHasError(pivot.CI_TariffNumInfo, "One of Tariff or Classification or Goods Catalog is Mandatory.");
				AssertHasError(pivot.CI_CCInfo, "One of Tariff or Classification or Goods Catalog is Mandatory.");

				pivot.CI_TariffNum = "1234.56.78";
				pivot.CI_CC = ZGuid.Empty;
				pivot.CI_CGC_Catalog = ZGuid.Empty;

				AssertNoError(pivot.CI_CGC_CatalogInfo, "One of Tariff or Classification or Goods Catalog is Mandatory.");
				AssertNoError(pivot.CI_TariffNumInfo, "One of Tariff or Classification or Goods Catalog is Mandatory.");
				AssertNoError(pivot.CI_CCInfo, "One of Tariff or Classification or Goods Catalog is Mandatory.");

				pivot.CI_TariffNum = ZString.Empty;
				pivot.CI_CC = Factory.New<CusClassification>().PK;
				pivot.CI_CGC_Catalog = ZGuid.Empty;

				AssertNoError(pivot.CI_CGC_CatalogInfo, "One of Tariff or Classification or Goods Catalog is Mandatory.");
				AssertNoError(pivot.CI_TariffNumInfo, "One of Tariff or Classification or Goods Catalog is Mandatory.");
				AssertNoError(pivot.CI_CCInfo, "One of Tariff or Classification or Goods Catalog is Mandatory.");

				pivot.CI_TariffNum = ZString.Empty;
				pivot.CI_CC = ZGuid.Empty;
				pivot.CI_CGC_Catalog = Factory.New<CusGoodsCatalog>().PK;

				AssertNoError(pivot.CI_CGC_CatalogInfo, "One of Tariff or Classification or Goods Catalog is Mandatory.");
				AssertNoError(pivot.CI_TariffNumInfo, "One of Tariff or Classification or Goods Catalog is Mandatory.");
				AssertNoError(pivot.CI_CCInfo, "One of Tariff or Classification or Goods Catalog is Mandatory.");
			}
		}

		public void TestGoodsCatalogHasAuthorityCode()
		{
			var pivot = Factory.New<CusClassPartPivot>();

			var newCatalog = Factory.New<CusGoodsCatalog>();
			newCatalog.CGC_AuthorityIdentifier = ZString.Empty;

			pivot.CI_CGC_Catalog = newCatalog.PK;

			AssertHasWarning(pivot.CI_CGC_CatalogInfo, "Catalog has no Authority Code.");

			newCatalog.CGC_AuthorityIdentifier = "012345";

			pivot.CI_CGC_Catalog = newCatalog.PK;
			AssertNoWarning(pivot.CI_CGC_CatalogInfo, "Catalog has no Authority Code.");
		}

		public void TestGoodsCatalogWarningOrErrorWhenStatusMatched()
		{
			var pivot = Factory.New<CusClassPartPivot>();

			var newCatalog = Factory.New<CusGoodsCatalog>();
			newCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.NotSent;

			pivot.CI_CGC_Catalog = newCatalog.PK;

			AssertHasWarning(pivot.CI_CGC_CatalogInfo, "This Goods Catalog should not be used because there might be messages that need to be sent (its Latest Message Status is currently NST - Not Sent).");

			newCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;

			pivot.CI_CGC_Catalog = newCatalog.PK;
			AssertHasErrorContaining(pivot.CI_CGC_CatalogInfo, "This Goods Catalog should not be used because there is a message waiting for response (its Latest Message Status is currently AWA - Awaiting Response).");

			newCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.Rejected;

			pivot.CI_CGC_Catalog = newCatalog.PK;
			AssertHasWarning(pivot.CI_CGC_CatalogInfo, "This Goods Catalog should not be used due to its last message being rejected (its Latest Message Status is currently REJ - Rejected).");

			newCatalog.CGC_AuthorityIdentifier = "012345";
			newCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.Accepted;
			pivot.CI_CGC_Catalog = newCatalog.PK;
			AssertNoWarnings(pivot.CI_CGC_CatalogInfo);
			AssertNoErrors(pivot.CI_CGC_CatalogInfo);
		}
	}
}
