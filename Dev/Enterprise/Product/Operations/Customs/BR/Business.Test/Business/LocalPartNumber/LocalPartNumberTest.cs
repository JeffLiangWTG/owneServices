using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(LocalPartNumber))]
	sealed class LocalPartNumberTest : EnterpriseBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var cusGoodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var cusGoodsCatalogProductionInfo = cusGoodsCatalog.LocalPartNumbers.AddNew();
			cusGoodsCatalogProductionInfo.CGI_Reference = "123";

			CombineAssertions(() =>
			{
				AssertEquals("CGI_Reference", "123", cusGoodsCatalogProductionInfo.CGI_Reference);
				AssertEquals("CGI_Type", "LPN", cusGoodsCatalogProductionInfo.CGI_Type);
				AssertEquals("CGI_CGC_Catalog", cusGoodsCatalog.PK, cusGoodsCatalogProductionInfo.CGI_CGC_Catalog);
			});
		}

		public void TestResetMessageStatusAndCustomsStatusOnCatalog()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "123";
			var pivot = product.PivotsForBinding.AddNew();

			var catalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog.CGC_MessageStatus = BRMessageStatusList.Codes.Accepted;
			catalog.CGC_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;
			Factory.Save();

			pivot.CI_CGC_Catalog = catalog.PK;
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { "123" }, catalog.LocalPartNumbers.Select(x => x.CGI_Reference));
			AssertEquals("CGC_MessageStatus should be set to NotSent (empty) on adding a new Local Part Number", BRMessageStatusList.Codes.NotSent, catalog.CGC_MessageStatus);
			AssertEquals("CGC_CustomsStatus should be set to Active (ACT) on adding a new Local Part Number", CustomsPostedStatusList.Codes.Active, catalog.CGC_CustomsStatus);

			catalog.CGC_MessageStatus = BRMessageStatusList.Codes.Accepted;
			catalog.CGC_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;
			var localPartNumber = catalog.LocalPartNumbers[0];
			localPartNumber.CGI_SystemLastEditTimeUtc = ZDateTime.Now;
			AssertEquals("CGC_MessageStatus should NOT be set to NotSent (empty)", BRMessageStatusList.Codes.Accepted, catalog.CGC_MessageStatus);
			AssertEquals("CGC_CustomsStatus should NOT be set to Active (ACT)", CustomsPostedStatusList.Codes.Accepted, catalog.CGC_CustomsStatus);

			Factory.Save();
			
			pivot.CI_CGC_Catalog = ZGuid.Empty;
			Factory.Save();
			AssertEquals("LocalPartNumber deleted", 0, catalog.LocalPartNumbers.Count);
			AssertEquals("CGC_MessageStatus should be set to NotSent (empty) on deleting a saved Local Part Number", BRMessageStatusList.Codes.NotSent, catalog.CGC_MessageStatus);
			AssertEquals("CGC_CustomsStatus should be set to Active (ACT) on deleting a saved Local Part Number", CustomsPostedStatusList.Codes.Active, catalog.CGC_CustomsStatus);

			catalog.CGC_MessageStatus = BRMessageStatusList.Codes.Accepted;
			catalog.CGC_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;
			pivot.CI_CGC_Catalog = catalog.PK;
			pivot.CI_CGC_Catalog = ZGuid.Empty;
			Factory.Save();
			AssertEquals("CGC_MessageStatus should NOT be set to NotSent (empty)", BRMessageStatusList.Codes.Accepted, catalog.CGC_MessageStatus);
			AssertEquals("CGC_CustomsStatus should NOT be set to Active (ACT)", BRMessageStatusList.Codes.Accepted, catalog.CGC_CustomsStatus);
		}

		public void TestPivotFinder()
		{
			var cusGoodsCatalog = Factory.New<CusGoodsCatalog>();
			var cusGoodsCatalogProductionInfo = cusGoodsCatalog.LocalPartNumbers.AddNew();
			var pivotFinder = cusGoodsCatalogProductionInfo.PivotFinder;
			AssertSame(pivotFinder, cusGoodsCatalogProductionInfo.PivotFinder);

			cusGoodsCatalogProductionInfo.CGI_Reference = "234";
			AssertNotSame(pivotFinder, cusGoodsCatalogProductionInfo.PivotFinder);
		}

		public void TestReadOnly()
		{
			var cusGoodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var localPartNumber = cusGoodsCatalog.LocalPartNumbers.AddNew();
			localPartNumber.CGI_Reference = "123";

			Assert("LocalPartNumber should always be ReadOnly", localPartNumber.ReadOnly);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewLocalPartNumberForTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewLocalPartNumberForTest(factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewLocalPartNumberForTest(Factory);

		LocalPartNumber GetNewLocalPartNumberForTest(BusinessObjectFactory factory)
		{
			var cusGoodsCatalog = factory.NewWithValidTestData<CusGoodsCatalog>();
			var cusGoodsCatalogProductionInfo = cusGoodsCatalog.LocalPartNumbers.AddNew();
			cusGoodsCatalogProductionInfo.CGI_Reference = "123";
			return cusGoodsCatalogProductionInfo;
		}
	}
}
