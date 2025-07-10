using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusClassPartPivot))]
	class CusClassPartPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCloneCusClassPartPivot()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "123";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.ComplementaryDescription = "ComplementaryDescription";
			var clonedPivot = (CusClassPartPivot)pivot.Clone();
			AssertEquals("ComplementaryDescription", clonedPivot.ComplementaryDescription);
		}

		public void TestNveCusCodeDataCollection()
		{
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);

			var catalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog.CGC_Tariff = "56049000";
			catalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			catalog.CGC_Description = "Test Descr";

			var classification = Factory.NewWithValidTestData<CusClassification>();
			classification.CC_TariffNum = "56049000";

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "123";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals("NveCusCodeDataCollection must contain 0", 0, pivot.NveCusCodeDataCollection.Count);

			pivot.CI_CGC_Catalog = catalog.PK;
			AssertEquals("NveCusCodeDataCollection must contain 1", 1, pivot.NveCusCodeDataCollection.Count);
			pivot.CI_CGC_Catalog = ZGuid.Empty;
			AssertEquals("NveCusCodeDataCollection must contain 0", 0, pivot.NveCusCodeDataCollection.Count);

			pivot.CI_CC = classification.PK;
			AssertEquals("NveCusCodeDataCollection must contain 1", 1, pivot.NveCusCodeDataCollection.Count);
			pivot.CI_CC = ZGuid.Empty;
			AssertEquals("NveCusCodeDataCollection must contain 0", 0, pivot.NveCusCodeDataCollection.Count);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			pivot.CI_TariffNum = "56049000";
			AssertEquals("NveCusCodeDataCollection must contain 1", 1, pivot.NveCusCodeDataCollection.Count);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "56049000";
			Factory.Save();
			AssertEquals("NveCusCodeDataCollection must contain 1", 1, pivot.NveCusCodeDataCollection.Count);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			Factory.Save();
			AssertEquals("NveCusCodeDataCollection must contain 0", 0, pivot.NveCusCodeDataCollection.Count);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals("NveCusCodeDataCollection must contain 1", 1, pivot.NveCusCodeDataCollection.Count);
		}

		public void TestAttributeCusCodeDataCollection()
		{
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "123";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			pivot.CI_TariffNum = "56049000";
			AssertEquals("AttributeCusCodeDataCollection must contain 1", 1, pivot.Attributes.Count);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CI_TariffNum = "56049000";
			Factory.Save();
			AssertEquals("AttributeCusCodeDataCollection must contain 1", 1, pivot.Attributes.Count);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Factory.Save();
			AssertEquals("AttributeCusCodeDataCollection must contain 1", 1, pivot.Attributes.Count);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("AttributeCusCodeDataCollection must contain 1", 1, pivot.Attributes.Count);
		}

		public void TestCloneCusClassPartPivot_Attributes()
		{
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "123";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CI_TariffNum = "56049000";
			AssertEquals("AttributeCusCodeDataCollection must contain 1", 1, pivot.Attributes.Count);
			pivot.Attributes[0].CY_Data = "234";

			var clonedPivot = (CusClassPartPivot)pivot.Clone();
			AssertEquals("clonedPivot must contain 1", 1, clonedPivot.Attributes.Count);
			AssertEquals("CY_Data cloned", "234", clonedPivot.Attributes[0].CY_Data);
			AssertEquals("TariffCharacteristic", pivot.Attributes[0].TariffProfileQuestion, clonedPivot.Attributes[0].TariffProfileQuestion);
		}

		public void TestCloneCusClassPartPivot_TariffDetails()
		{
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "123";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "56049000";
			pivot.TariffDetachs.AddNew("000");
			AssertEquals("TariffDetachCollection must contain", 1, pivot.TariffDetachs.Count);
			AssertEquals("NveCusCodeDataCollection must contain", 1, pivot.NveCusCodeDataCollection.Count);
			pivot.NveCusCodeDataCollection[0].CY_Data = "9999";

			var clonedPivot = (CusClassPartPivot)pivot.Clone();

			AssertEquals("clonedPivot.NveCusCodeDataCollection must contain", 1, clonedPivot.NveCusCodeDataCollection.Count);
			CombineAssertions(() =>
			{
				AssertEquals("clonedPivot CY_Order should be", (ZShort)3, clonedPivot.NveCusCodeDataCollection[0].CY_Order);
				AssertEquals("clonedPivot CY_Data should be", "9999", clonedPivot.NveCusCodeDataCollection[0].CY_Data);
				AssertEquals("clonedPivot TariffCharacteristic should be", pivot.NveCusCodeDataCollection[0].TariffCharacteristic, clonedPivot.NveCusCodeDataCollection[0].TariffCharacteristic);
			});

			AssertEquals("clonedPivot.TariffDetachCollection must contain", 1, clonedPivot.TariffDetachs.Count);
			AssertEquals("clonedPivot CY_Code should be", "000", clonedPivot.TariffDetachs[0].CY_Code);
		}

		public void TestTariffDetachCollection()
		{
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "123";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "56049000";

			var tariffDetach = pivot.TariffDetachs.AddNew();
			tariffDetach.CY_Code = "000";
			Assert(pivot.TariffDetachs.Any());

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			Assert(pivot.TariffDetachs.Any());

			pivot.CI_TariffNum = "99999999";
			Assert(pivot.TariffDetachs.Any());

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			Assert(pivot.TariffDetachs.Any());
		}

		public void TestAdditionalTariffs()
		{
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "123";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "56049000";

			pivot.AdditionalTariffs.AddNew().LegalActSubject = AdditionalTaxTypeList.Codes.ExDutyTariff;
			Assert(pivot.AdditionalTariffs.Any());

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			Assert(pivot.AdditionalTariffs.Any());

			pivot.CI_TariffNum = "99999999";
			Assert(!pivot.AdditionalTariffs.Any());

			pivot.AdditionalTariffs.AddNew().LegalActSubject = AdditionalTaxTypeList.Codes.ExDutyTariff;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			Assert(pivot.AdditionalTariffs.Any());

			Factory.Save();
			Assert("AdditionalTariffs should deleted", !pivot.AdditionalTariffs.Any());
		}

		public void TestIAdditionalTariffParent()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "123";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			pivot.CI_TariffNum = "12345678";
			var additionalTariff = pivot.AdditionalTariffs.AddNew();
			AssertSame(pivot, additionalTariff.Parent);
			AssertEquals("12345678", additionalTariff.Parent.Tariff);
		}

		public void TestTariffDetachCollectionOnSaving()
		{
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "123";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "56049000";

			var tariffDetach = pivot.TariffDetachs.AddNew();
			tariffDetach.CY_Code = "000";
			Factory.Save();
			Assert(pivot.TariffDetachs.Any());

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			tariffDetach = pivot.TariffDetachs.AddNew();
			tariffDetach.CY_Code = "000";
			Factory.Save();
			Assert(pivot.TariffDetachs.Any());

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			tariffDetach = pivot.TariffDetachs.AddNew();
			tariffDetach.CY_Code = "000";
			Factory.Save();
			Assert(!pivot.TariffDetachs.Any());
		}

		public void TestTariffDetachConcatenated()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "56049000";

			var tariffDetach = pivot.TariffDetachs.AddNew();
			tariffDetach.CY_Code = "001";
			tariffDetach = pivot.TariffDetachs.AddNew();
			tariffDetach.CY_Code = "999";
			tariffDetach = pivot.TariffDetachs.AddNew();
			tariffDetach.CY_Code = "555";

			AssertEquals("TariffDetachConcatenated should be", "001,555,999", pivot.TariffDetachConcatenated);
		}

		public void TestClearTariffAndClassificationWhenGoodsCatalogIsEntered()
		{
			var newPKforCatalog = Factory.New<CusGoodsCatalog>().PK;
			var newPKforClassification = Factory.New<CusGoodsCatalog>().PK;

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "12345678";
			pivot.CI_CC = newPKforClassification;

			pivot.CI_CGC_Catalog = newPKforCatalog;

			AssertEquals("Tariff Should be Empty when Goods Catalog is entered", ZString.Empty, pivot.CI_TariffNum);
			AssertEquals("Classification should be empty when Goods catalog is entered", ZGuid.Empty, pivot.CI_CC);

			pivot.CI_TariffNum = "12345678";
			pivot.CI_CC = newPKforClassification;
			pivot.CI_CGC_Catalog = newPKforCatalog;

			AssertEquals("Tariff Should not be Clear when Goods Catalog do not change", "12345678", pivot.CI_TariffNum);
			AssertEquals("Classification should not be Clear when Goods Catalog do not change", newPKforClassification, pivot.CI_CC);
		}

		public void TestReadOnly()
		{
			var catalog = Factory.New<CusGoodsCatalog>();
			catalog.CGC_CatalogCode = "C1";

			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "123";

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "123";

			var pivot = product.PivotsForBinding.AddNew();
			CombineAssertions(() =>
			{
				Assert("CI_CC should NOT be ReadOnly", !pivot.CI_CCInfo.ReadOnly);
				Assert("CI_TariffNum should NOT be ReadOnly", !pivot.CI_TariffNumInfo.ReadOnly);
				Assert("CI_CGC_Catalog should NOT be ReadOnly", !pivot.CI_CGC_CatalogInfo.ReadOnly);
			});

			pivot.CI_CGC_Catalog = catalog.PK;
			CombineAssertions(() =>
			{
				Assert("CI_CC should be ReadOnly", pivot.CI_CCInfo.ReadOnly);
				Assert("CI_TariffNum should be ReadOnly", pivot.CI_TariffNumInfo.ReadOnly);
				Assert("CI_CGC_Catalog should NOT be ReadOnly", !pivot.CI_CGC_CatalogInfo.ReadOnly);
			});

			pivot.CI_CGC_Catalog = ZGuid.Empty;
			pivot.CI_CC = classification.PK;
			CombineAssertions(() =>
			{
				Assert("CI_CC should NOT be ReadOnly", !pivot.CI_CCInfo.ReadOnly);
				Assert("CI_TariffNum should be ReadOnly", pivot.CI_TariffNumInfo.ReadOnly);
				Assert("CI_CGC_Catalog should NOT be ReadOnly", !pivot.CI_CGC_CatalogInfo.ReadOnly);
			});

			pivot.CI_CGC_Catalog = ZGuid.Empty;
			pivot.CI_CC = ZGuid.Empty;
			pivot.CI_TariffNum = "12345678";
			CombineAssertions(() =>
			{
				Assert("CI_CC should be ReadOnly", pivot.CI_CCInfo.ReadOnly);
				Assert("CI_TariffNum should NOT be ReadOnly", !pivot.CI_TariffNumInfo.ReadOnly);
				Assert("CI_CGC_Catalog should NOT be ReadOnly", !pivot.CI_CGC_CatalogInfo.ReadOnly);
			});
		}

		public void TestUpdateLocalPartNumber()
		{
			var catalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog.LocalPartNumbers.AddNew().CGI_Reference = "123";

			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product1.OP_PartNum = "123";
			var product2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product2.OP_PartNum = "234";

			CombineAssertions(() =>
			{
				var pivot1 = product1.PivotsForBinding.AddNew();
				pivot1.CI_CGC_Catalog = catalog.PK;
				AssertLocalPartNumbers("pivot1: New Local Part Number added", catalog, "123");

				var pivot2 = product2.PivotsForBinding.AddNew();
				pivot2.CI_CGC_Catalog = catalog.PK;
				AssertLocalPartNumbers("pivot2: No new Local Part Number added", catalog, "123", "234");

				var pivot3 = product2.PivotsForBinding.AddNew();
				pivot3.CI_CGC_Catalog = catalog.PK;
				AssertLocalPartNumbers("pivot3: No new Local Part Number added", catalog, "123", "234");

				catalog.LocalPartNumbers.AddNew().CGI_Reference = "234";
				catalog.LocalPartNumbers.AddNew().CGI_Reference = "345";

				AssertLocalPartNumbers("Local Part Number added manually", catalog, "123", "234", "234", "345");

				pivot1.CI_CGC_Catalog = ZGuid.Empty;
				AssertLocalPartNumbers("Local Part Number 123 removed", catalog, "234", "234", "345");

				pivot2.Delete();
				AssertLocalPartNumbers("Local Part Number 234 removed", catalog, "345");

				var pivot4 = Factory.New<CusClassPartPivot>();
				pivot4.CI_CGC_Catalog = catalog.PK;
				product2.PivotsForBinding.Add(pivot4);
				AssertLocalPartNumbers("Local Part Number 234 added by adding Pivot on Part", catalog, "234", "345");

				product2.PivotsForBinding.RemoveAndDelete(pivot4);
				AssertLocalPartNumbers("Local Part Number 234 removed by removing Pivot on Part", catalog, "345");
			});

			void AssertLocalPartNumbers(string message, CusGoodsCatalog catalog, params string[] localPartNumbers)
			{
				Factory.Save();
				AssertContainsExactElementsInAnyOrder(message, localPartNumbers, catalog.LocalPartNumbers.Select(x => x.CGI_Reference));
			}
		}

		public void TestComplementaryDescriptionMaxLength()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "123";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals(4000, pivot.ComplementaryDescriptionInfo.MaxLength);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(2000, pivot.ComplementaryDescriptionInfo.MaxLength);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			AssertEquals(4000, pivot.ComplementaryDescriptionInfo.MaxLength);
		}

		public void TestComplementaryDescription()
		{
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			product.OP_PartNum = "123";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.ComplementaryDescription = new ZString('X', 4000);
			Factory.Save();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			Factory.Save();
			AssertEquals("ComplementaryDescription Length should", 4000, pivot.ComplementaryDescription.Length);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			Factory.Save();
			AssertEquals("ComplementaryDescription Length should", 2000, pivot.ComplementaryDescription.Length);
		}

		public void TestGetAttributes()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "123";

			var pivot = product.PivotsForBinding.AddNew();
			CombineAssertions(() =>
			{
				AssertSame("When Type is ATT, should return Attributes", pivot.Attributes, pivot.GetAttributes(CusCodeDataTypeList.Codes.Attribute));
				AssertNull("When Type is not ATT, should return null", pivot.GetAttributes(CusCodeDataTypeList.Codes.NVE));
			});
		}

		public void TestTariffNumberAndUniversalTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCodes.Brazil, "HSN");
			helper.LoadOrCreateNewTariff(CountryCodes.Brazil, tariffType.PK, "56049000", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			helper.LoadOrCreateNewTariff(CountryCodes.Brazil, tariffType.PK, "56049001", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			helper.LoadOrCreateNewTariff(CountryCodes.Brazil, tariffType.PK, "56049002", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));

			var catalog = Factory.New<CusGoodsCatalog>();
			catalog.CGC_Tariff = "56049000";
			catalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			catalog.CGC_Description = "Test Descr";

			var classification = Factory.New<CusClassification>();
			classification.CC_TariffNum = "56049001";

			var pivot = Factory.New<CusClassPartPivot>();
			AssertEquals("TariffNumber should be empty", ZString.Empty, pivot.TariffNumber);
			AssertNull("UniversalTariff should be null", pivot.UniversalTariff);

			pivot.CI_CGC_Catalog = catalog.PK;
			AssertEquals("TariffNumber should be '56049000'", "56049000", pivot.TariffNumber);
			AssertNotNull("UniversalTariff should not be null", pivot.UniversalTariff);

			pivot.CI_CGC_Catalog = ZGuid.Empty;
			pivot.CI_TariffNum = ZString.Empty;
			pivot.CI_CC = classification.PK;
			AssertEquals("TariffNumber should be '56049001'", "56049001", pivot.TariffNumber);
			AssertNotNull("UniversalTariff should not be null", pivot.UniversalTariff);

			pivot.CI_CGC_Catalog = ZGuid.Empty;
			pivot.CI_CC = ZGuid.Empty;
			pivot.CI_TariffNum = "56049002";
			AssertEquals("TariffNumber should be '56049002'", "56049002", pivot.TariffNumber);
			AssertNotNull("UniversalTariff should not be null", pivot.UniversalTariff);
		}
	}
}
