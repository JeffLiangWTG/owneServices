using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class IUnitConverterDataProviderExtensionTest : TestCaseWithFactory
	{
		public void TestGetBestMatchingForPhysicalUnit()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals(CustomsUnitOfMeasurementListHelper.Codes.Kilograms, invoiceLine.GetBestMatchingCustomsUnit(Core.Constants.Weight.Kilograms));
			AssertEquals(CustomsUnitOfMeasurementListHelper.Codes.Kilograms, invoiceLine.GetBestMatchingCustomsUnit(Core.Constants.Weight.Hectograms));
			AssertEquals(CustomsUnitOfMeasurementListHelper.Codes.Grams, invoiceLine.GetBestMatchingCustomsUnit(Core.Constants.Weight.Grams));
			AssertEquals(CustomsUnitOfMeasurementListHelper.Codes.Metres, invoiceLine.GetBestMatchingCustomsUnit(Core.Constants.Length.Metres));
			AssertEquals(CustomsUnitOfMeasurementListHelper.Codes.Metres, invoiceLine.GetBestMatchingCustomsUnit(Core.Constants.Length.Centimetres));
			AssertEquals("147", invoiceLine.GetBestMatchingCustomsUnit(Core.Constants.Length.Kilometres));
			AssertEquals(CustomsUnitOfMeasurementListHelper.Codes.SquareMetres, invoiceLine.GetBestMatchingCustomsUnit(Core.Constants.Area.SquareMetre));
			AssertEquals(CustomsUnitOfMeasurementListHelper.Codes.SquareMetres, invoiceLine.GetBestMatchingCustomsUnit(Core.Constants.Area.SquareMile));
			AssertEquals("110", invoiceLine.GetBestMatchingCustomsUnit(Core.Constants.Area.SquareYard));
			AssertEquals(CustomsUnitOfMeasurementListHelper.Codes.CubicMetres, invoiceLine.GetBestMatchingCustomsUnit(Core.Constants.Volume.CubicMetres));
			AssertEquals(CustomsUnitOfMeasurementListHelper.Codes.CubicMetres, invoiceLine.GetBestMatchingCustomsUnit(Core.Constants.Volume.CubicDecimetres));
			AssertEquals("099", invoiceLine.GetBestMatchingCustomsUnit(Core.Constants.Volume.CubicFeet));
		}

		public void TestGetBestMatchingCustomsUnitFromProduct()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Unit Quantity");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "148", "Unit 1", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			Factory.Save();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.FillWithValidTestData();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PART1";
			product.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			Factory.Save();
			CreateOrgPartUnit(product, "CRT", "147", 1m);
			CreateOrgPartUnit(product, "CRT", "148", 2m);
			Factory.Save();
			var testItem = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { });
			testItem.InvoiceHeader.JZ_OH_Supplier = supplier.PK;
			var invoiceline = testItem.InvoiceLine;
			invoiceline.JI_PartNo = "PART1";
			Factory.Save();
			AssertEquals("Unit conversion of higher Factor should be the priority.", "148", invoiceline.GetBestMatchingCustomsUnit("CRT"));
		}

		public void TestGetBestMatchingCustomsUnitFromRefPacks()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Unit Quantity");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "111", "Unit 1", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			Factory.Save();
			var testItem = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { });
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.FillWithValidTestData();
			CreateCusRefPacks("CRT", "001", 1m, supplier.PK);
			CreateCusRefPacks("CRT", "115", 1m, supplier.PK);
			CreateCusRefPacks("CRT", "115", 10m, ZGuid.Empty);
			CreateCusRefPacks("CRT", "111", 2m, ZGuid.Empty);
			testItem.InvoiceHeader.JZ_OH_Supplier = supplier.PK;
			Factory.Save();
			AssertEquals("111", testItem.InvoiceLine.GetBestMatchingCustomsUnit("CRT"));
		}

		public void TestGetBestMatchingCustomsUnitFromConverters()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Unit Quantity");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "123", "Unit 1", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "111", "Unit 2", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "088", "Unit 3", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			Factory.Save();
			var testItem = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { });
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.FillWithValidTestData();
			var invoiceline = testItem.InvoiceLine;
			AssertNoExceptionThrown("Should not throw exceptions even for an empty InvoiceLine", () => invoiceline.GetBestMatchingCustomsUnit(""));
			CreateCusRefPacks("CRT", "001", 1m, supplier.PK);
			CreateCusRefPacks("CRT", "115", 1m, supplier.PK);
			CreateCusRefPacks("CRT", "115", 10m, ZGuid.Empty);
			CreateCusRefPacks("CRT", "111", 3m, ZGuid.Empty);
			testItem.InvoiceHeader.JZ_OH_Supplier = supplier.PK;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PART1";
			product.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			CreateOrgPartUnit(product, "CRT", "122", 1m);
			CreateOrgPartUnit(product, "CRT", "123", 2m);
			Factory.Save();
			invoiceline.JI_PartNo = "PART1";
			Factory.Save();
			AssertEquals("123", invoiceline.GetBestMatchingCustomsUnit("CRT"));
			invoiceline.JI_PartNo = "";
			AssertEquals("111", invoiceline.GetBestMatchingCustomsUnit("CRT"));
			CreateOrgPartUnit(product, "086", "CRT", 100m);
			CreateOrgPartUnit(product, "087", "CRT", 0.001m);
			CreateOrgPartUnit(product, "088", "CRT", 0.01m);
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			testItem = CNCusEntryHeaderHelper.SetupCusEntryHeader(factory2, () => { });
			testItem.InvoiceHeader.JZ_OH_Supplier = supplier.PK;
			invoiceline = testItem.InvoiceLine;
			invoiceline.JI_PartNo = "PART1";
			Factory.Save();
			AssertEquals("088", invoiceline.GetBestMatchingCustomsUnit("CRT"));
		}

		static void CreateOrgPartUnit(OrgSupplierPart product, ZString parentType, ZString type, ZDecimal quantityInParent)
		{
			var unit = product.PartUnits.AddNew();
			unit.OF_ParentPackType = parentType;
			unit.OF_PackType = type;
			unit.OF_QuantityInParent = quantityInParent;
		}

		void CreateCusRefPacks(ZString commercialPack, ZString customsPack, ZDecimal factor, ZGuid supplier)
		{
			var result = Factory.New<CusRefPacks>();
			result.RP_CustomsCountry = "CN";
			result.RP_CommercialPack = commercialPack;
			result.RP_CustomsPack = customsPack;
			result.RP_ConversionFactor = factor;
			result.RP_Type = "CIP";
			result.RP_OH_Supplier = supplier;
		}
	}
}
