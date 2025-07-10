using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusGoodsCatalogCollection))]
	sealed class CusGoodsCatalogCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusGoodsCatalogCollection(Factory.New<OrgSupplierPart>().PivotsForBinding.AddNew());
		}

		public void TestDefaultsForNewChild()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var classification = Factory.New<CusClassification>();

			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			var collection = new CusGoodsCatalogCollection(pivot);

			CombineAssertions(() =>
			{
				var catalog = collection.AddNew() as CusGoodsCatalog;
				AssertEquals(ZString.Empty, catalog.CGC_Type);
				AssertEquals(ZString.Empty, catalog.CGC_Tariff);
				AssertEquals(ZGuid.Empty, catalog.CGC_OH_Owner);
				AssertEquals(0, catalog.LocalPartNumbers.Count);
			});

			pivot.CI_TariffNum = "01010202";
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CI_OH = orgHeader.PK;
			part.OP_PartNum = "PART123";
			pivot.CI_CC = classification.PK;
			pivot.Classification.CC_TariffNum = "02020101";

			CombineAssertions(() =>
			{
				var catalog = collection.AddNew() as CusGoodsCatalog;
				AssertEquals(GoodsCatalogTypeList.Codes.Export, catalog.CGC_Type);
				AssertEquals("01010202", catalog.CGC_Tariff);
				AssertEquals(orgHeader.PK, catalog.CGC_OH_Owner);
				AssertEquals(1, catalog.LocalPartNumbers.Count);
				AssertEquals("PART123", catalog.LocalPartNumbers[0].CGI_Reference);
			});

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = ZString.Empty;
			CombineAssertions(() =>
			{
				var catalog = collection.AddNew() as CusGoodsCatalog;
				AssertEquals(GoodsCatalogTypeList.Codes.Import, catalog.CGC_Type);
				AssertEquals("02020101", catalog.CGC_Tariff);
				AssertEquals(orgHeader.PK, catalog.CGC_OH_Owner);
				AssertEquals(1, catalog.LocalPartNumbers.Count);
				AssertEquals("PART123", catalog.LocalPartNumbers[0].CGI_Reference);
			});

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			CombineAssertions(() =>
			{
				var catalog = collection.AddNew() as CusGoodsCatalog;
				AssertEquals(ZString.Empty, catalog.CGC_Type);
				AssertEquals("02020101", catalog.CGC_Tariff);
				AssertEquals(orgHeader.PK, catalog.CGC_OH_Owner);
				AssertEquals(1, catalog.LocalPartNumbers.Count);
				AssertEquals("PART123", catalog.LocalPartNumbers[0].CGI_Reference);
			});
		}
	}
}
