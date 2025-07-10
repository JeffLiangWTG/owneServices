using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class PreviousDocumentLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestProcedureList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var previousDocument = declaration.PreviousDocuments.AddNew();

		declaration.JE_MessageType = "IMP";
		AssertEquals("When Parent is import, ProcedureList", typeof(ImportPreviousDocumentProcedureList), previousDocument.Lookups.ProcedureList.GetType());

		declaration.JE_MessageType = "EXP";
		AssertEquals("When Parent is not import, ProcedureList", typeof(PreviousDocumentProcedureList), previousDocument.Lookups.ProcedureList.GetType());

		var orphanPreviousDocument = Factory.New<PreviousDocumentForTest>();
		AssertEquals("When Parent is not found, ProcedureList", typeof(PreviousDocumentProcedureList), orphanPreviousDocument.Lookups.ProcedureList.GetType());
	}

	public void TestCodeList()
	{
		previousDocument.CSI_Procedure = "";
		AssertEquals("When procedure is empty, CodeList", "270, 720, 740, 750, 785, 820, 821, 822, 952, CO, EU, EX, IM, T2F, ZZZ", (previousDocument.Lookups.CodeList as CodeDescriptionPairList).CodesAsString);

		previousDocument.CSI_Procedure = "LC";
		AssertEquals("When procedure is 'LC', CodeList", "270", (previousDocument.Lookups.CodeList as CodeDescriptionPairList).CodesAsString);

		previousDocument.CSI_Procedure = "MRN";
		AssertEquals("When procedure is 'MRN', CodeList", "820, 821, 822", (previousDocument.Lookups.CodeList as CodeDescriptionPairList).CodesAsString);
	}

	public void TestCodeListUcc6Export()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var previousDocument = declaration.PreviousDocuments.AddNew();
		var lookups = previousDocument.Lookups;

		AssertEquals("For Non Ucc6 Export", "270, 720, 740, 750, 785, 820, 821, 822, 952, CO, EU, EX, IM, T2F, ZZZ", (lookups.CodeList as CodeDescriptionPairList).CodesAsString);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			CombineAssertions("For Ucc6 Export", () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: euGrouping);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "Previous Document Of Export", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "PEU1", "Prev Doc for EU 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "PEU2", "Prev Doc for EU 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
				Factory.Save();

				AssertEquals("When only EUN CusCodes present", "PEU1, PEU2", (lookups.CodeList as CodeDescriptionPairList).CodesAsString);

				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "PIT1", "Prev Doc for IT 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "PIT2", "Prev Doc for IT 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
				Factory.Save();
				previousDocument = Factory.CreateNewFactory().Load<PreviousDocument>(previousDocument.PK);
				lookups = previousDocument.Lookups;

				AssertEquals("When both IT and EUN CusCodes present", "PIT1, PIT2", (lookups.CodeList as CodeDescriptionPairList).CodesAsString);
			});
		}
	}

	public void TestCustomsOfficeList()
	{
		PreviousDocumentLookupsHelperTest.TestCustomsOfficeList(Factory, lookups);
	}

	public void TestUnitOfQuantityList()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var previousDocument = declaration.PreviousDocuments.AddNew();
		var lookups = previousDocument.Lookups;

		Assert("For non Ucc6 Export, UnitOfQuantityList has elements", lookups.UnitOfQuantityList.Cast<CodeDescriptionPair>().Any());
		AssertContainsExactElementsInAnyOrder("For non Ucc6 Export, UnitOfQuantityList", new CodeDescriptionPairList(OLookUpEditType.Weight), lookups.UnitOfQuantityList);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGMG", "Kilogram Gross", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "MTQ", "Cubic meter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "TNE", "Tonne", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
			Factory.Save();

			AssertEquals("For Ucc6 Export", 3, lookups.UnitOfQuantityList.Count);
			CombineAssertions("For Ucc6 Export, UnitOfQuantityList should contains these UOMs", () =>
			{
				Assert(lookups.UnitOfQuantityList.ContainsCode("KGMG"));
				Assert(lookups.UnitOfQuantityList.ContainsCode("MTQ"));
				Assert(lookups.UnitOfQuantityList.ContainsCode("TNE"));
			});
		}

		declaration.JE_MessageType = "IMP";
		Assert("For Import, UnitOfQuantityList has elements", lookups.UnitOfQuantityList.Cast<CodeDescriptionPair>().Any());
		AssertContainsExactElementsInAnyOrder("For Import, UnitOfQuantityList", new CodeDescriptionPairList(OLookUpEditType.Weight), lookups.UnitOfQuantityList);
	}

	public void TestSubTypeList()
	{
		previousDocument.CSI_Procedure = "LC";
		AssertEquals("For Procedure 'LC', CodesAsString", "X", previousDocument.Lookups.SubTypeList.CodesAsString);

		previousDocument.CSI_Procedure = "";
		AssertEquals("For Procedure '', CodesAsString", "X, Z", previousDocument.Lookups.SubTypeList.CodesAsString);
	}

	public void TestUnitOfQuantity2List()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGMG", "Kilogram Gross", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "MTQ", "Cubic meter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "TNE", "Tonne", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
		Factory.Save();

		AssertEquals(3, lookups.UnitOfQuantity2List.Count);
		CombineAssertions("UnitOfQuantityList should contains these UOMs", () =>
		{
			Assert(lookups.UnitOfQuantity2List.ContainsCode("KGMG"));
			Assert(lookups.UnitOfQuantity2List.ContainsCode("MTQ"));
			Assert(lookups.UnitOfQuantity2List.ContainsCode("TNE"));
		});
	}

	public void TestUnitOfQuantity3List()
	{
		Assert("UnitOfQuantity3List has elements", lookups.UnitOfQuantity3List.Cast<CodeDescriptionPair>().Any());
		AssertContainsExactElementsInAnyOrder("UnitOfQuantity3List", new CodeDescriptionPairList(OLookUpEditType.Weight), lookups.UnitOfQuantity3List);
	}

	IPreviousDocumentLookupsForTesting GetNewLookups(BusinessObjectFactory factory)
	{
		previousDocument = factory.New<PreviousDocumentForTest>();
		factory.New<JobDeclaration>().Invoices.AddNew().PreviousDocuments.Add(previousDocument);
		return new PreviousDocumentLookupsForTest(previousDocument);
	}

	public void TestSupplementaryQuantityUOMs()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Italy, Universal.Constants.TariffTypes.Import);
		Factory.Save();

		var tariffWithMultipleUnits = helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffType.PK, "1111111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(tariffWithMultipleUnits, UnitOfMeasureTypes.AdditionalUOMType, "NAR");
		helper.CreateTariffUOM(tariffWithMultipleUnits, UnitOfMeasureTypes.AdditionalUOMType, "XXX");
		helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffType.PK, "3333333333", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var tariffWithOneUnit = helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(tariffWithOneUnit, UnitOfMeasureTypes.AdditionalUOMType, "SSS");
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var previousDocument = declaration.PreviousDocuments.AddNew();

		previousDocument.FormattedTariff = "";
		var unitOfMeasures = previousDocument.Lookups.SupplementaryQuantityUOMs;
		AssertEquals("No UOMs available", 0, unitOfMeasures.Count());

		previousDocument.FormattedTariff = "1";
		unitOfMeasures = previousDocument.Lookups.SupplementaryQuantityUOMs;
		AssertEquals("No UOMs available", 0, unitOfMeasures.Count());

		previousDocument.FormattedTariff = "3333333333";
		unitOfMeasures = previousDocument.Lookups.SupplementaryQuantityUOMs;
		AssertEquals("No UOMs available", 0, unitOfMeasures.Count());

		previousDocument.FormattedTariff = "2222222222";
		unitOfMeasures = previousDocument.Lookups.SupplementaryQuantityUOMs;
		AssertEquals("1 UOM available", 1, unitOfMeasures.Count());
		Assert("SSS is available", unitOfMeasures.Contains("SSS"));

		previousDocument.FormattedTariff = "1111111111";
		unitOfMeasures = previousDocument.Lookups.SupplementaryQuantityUOMs;
		AssertEquals("2 UOMs available", 2, unitOfMeasures.Count());
		Assert("NAR is available", unitOfMeasures.Contains("NAR"));
		Assert("XXX is available", unitOfMeasures.Contains("XXX"));
	}

	public void TestPackageTypeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType("UNPKG", "Packagings", "UNE");
		helper.CreateCusCodeList("UNE", "UNPKG", "BOX", "Box", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeList("UNE", "UNPKG", "CNT", "Container", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		CombineAssertions(() =>
		{
			var packageTypeList = previousDocument.Lookups.PackageTypeList;
			AssertEquals("PackageTypeList Count", 2, packageTypeList.Count);
			AssertEquals("PackageTypeList ContainsCode 'BOX'", true, packageTypeList.ContainsCode("BOX"));
			AssertEquals("PackageTypeList ContainsCode 'CNT'", true, packageTypeList.ContainsCode("CNT"));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		lookups = (PreviousDocumentLookupsForTest)GetNewLookups(Factory);
	}

	PreviousDocumentForTest previousDocument;
	PreviousDocumentLookupsForTest lookups;
}

class PreviousDocumentLookupsForTest : PreviousDocumentLookups, IPreviousDocumentLookupsForTesting
{
	public PreviousDocumentLookupsForTest(PreviousDocumentForTest parent)
		: base(parent)
	{
	}

	public EU.Business.Declaration.MultiLineAddInfos.PreviousDocument PreviousDocument => Parent;

	CodeDescriptionPairList IPreviousDocumentLookupsForTesting.CodeList => (CodeDescriptionPairList)base.CodeList;
}
