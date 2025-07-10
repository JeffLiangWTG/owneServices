using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

public sealed partial class NctsPreviousDocumentLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestProcedureList()
	{
		CombineAssertions("Checking ProcedureList Lookups", () =>
		{
			AssertEquals("ProcedureList Count", 27, lookups.ProcedureList.Count);
			AssertContainsExactElementsInAnyOrder("ProcedureList", new PreviousDocumentProcedureList(), lookups.ProcedureList);
		});
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

	public void TestCustomsOfficeList()
	{
		PreviousDocumentLookupsHelperTest.TestCustomsOfficeList(Factory, lookups);
	}

	public void TestUnitOfQuantityList()
	{
		AssertEquals(1, lookups.UnitOfQuantity2List.Count);
		Assert(lookups.UnitOfQuantityList.ContainsCode(Core.Constants.Weight.Kilograms));
	}

	public void TestUnitOfQuantity2List()
	{
		AssertEquals(1, lookups.UnitOfQuantity2List.Count);
		Assert(lookups.UnitOfQuantity2List.ContainsCode(Core.Constants.Weight.Kilograms));
	}

	public void TestUnitOfQuantity3List()
	{
		AssertEquals(1, lookups.UnitOfQuantity3List.Count);
		Assert(lookups.UnitOfQuantity3List.ContainsCode(Core.Constants.Weight.Kilograms));
	}

	IPreviousDocumentLookupsForTesting GetNewLookups(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		previousDocument = factory.New<NctsPreviousDocumentForTest>();
		goodsItem.PreviousDocuments.Add(previousDocument);
		return new NctsPreviousDocumentLookupsForTest(previousDocument);
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

		previousDocument.FormattedTariff = "";
		var lookups = previousDocument.Lookups as NctsPreviousDocumentLookups;
		var unitOfMeasures = lookups.SupplementaryQuantityUOMs;
		AssertEquals("No UOMs available", 0, unitOfMeasures.Count());

		previousDocument.FormattedTariff = "1";
		unitOfMeasures = lookups.SupplementaryQuantityUOMs;
		AssertEquals("No UOMs available", 0, unitOfMeasures.Count());

		previousDocument.FormattedTariff = "3333333333";
		unitOfMeasures = lookups.SupplementaryQuantityUOMs;
		AssertEquals("No UOMs available", 0, unitOfMeasures.Count());

		previousDocument.FormattedTariff = "2222222222";
		unitOfMeasures = lookups.SupplementaryQuantityUOMs;
		AssertEquals("1 UOM available", 1, unitOfMeasures.Count());
		Assert("SSS is available", unitOfMeasures.Contains("SSS"));

		previousDocument.FormattedTariff = "1111111111";
		unitOfMeasures = lookups.SupplementaryQuantityUOMs;
		AssertEquals("2 UOMs available", 2, unitOfMeasures.Count());
		Assert("NAR is available", unitOfMeasures.Contains("NAR"));
		Assert("XXX is available", unitOfMeasures.Contains("XXX"));
	}

	protected override void SetUp()
	{
		base.SetUp();
		lookups = (NctsPreviousDocumentLookupsForTest)GetNewLookups(Factory);
	}

	NctsPreviousDocumentForTest previousDocument;
	NctsPreviousDocumentLookupsForTest lookups;
	NctsDepartureCargoDesc goodsItem;
}

public class NctsPreviousDocumentLookupsForTest : NctsPreviousDocumentLookups, IPreviousDocumentLookupsForTesting
{
	public NctsPreviousDocumentLookupsForTest(NctsPreviousDocumentForTest parent)
		: base(parent)
	{
	}

	public EU.Business.Declaration.MultiLineAddInfos.PreviousDocument PreviousDocument => Parent;

	CodeDescriptionPairList IPreviousDocumentLookupsForTesting.CodeList => (CodeDescriptionPairList)base.CodeList;
}
