using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class CusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProcedureCodeList_Import()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "10", "11", "111", "One", "IMP", group: "IFD");
			helper.CreateRefCusProcedure(currentCountry, "B", "10", "22", "222", "Two", "IMP", group: "IFD");
			helper.CreateRefCusProcedure(currentCountry, "C", "10", "33", "333", "Three", "EXP", group: "ICR");
			helper.CreateRefCusProcedure(currentCountry, "D", "20", "44", "444", "Four", "IMP", group: "IFD");

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = "IMP";

			CombineAssertions(() =>
			{
				var values = entryInstruction.Lookups.ProcedureCodeList;

				AssertEquals(3, values.Count);
				Assert("contains 1011111, 1022222 and 2044444", values.ContainsCode("1011111") && values.ContainsCode("1022222") && values.ContainsCode("2044444"));
			});
		}

		public void TestAutonomyRegionTypeList()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateCusCodeType(code: "CFTRY", desc: "CFTRY", dataGrouping: Core.Constants.CountryCodes.Israel);
			helper.CreateCusCodeList(dataGroupingCode: Core.Constants.CountryCodes.Israel, codeType: "CFTRY", code: "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = "IMP";

			var values = entryInstruction.Lookups.AutonomyRegionTypeList as ZZRefCusCodeListCombinedCollection;
			values.Load();
			AssertEquals(1, values.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "1" }, values.Select(x => x.ZZD_Code));
		}

		public void TestFromWarehouseBondedWarehouseAddressList()
		{
			var (orgAddressBond, entryInstruction) = SetupForWarehouseBondedWarehouseAddressList();

			AssertType<OrgAddressDependentCollection>(entryInstruction.Lookups.FromWarehouseBondedWarehouseAddressList);
			AssertEquals("When FromWarehouse is empty", 0, entryInstruction.Lookups.FromWarehouseBondedWarehouseAddressList.Count);
			entryInstruction.CEI_OA_Warehouse = orgAddressBond.PK;
			AssertEquals("When FromWarehouse is not empty", 3, entryInstruction.Lookups.FromWarehouseBondedWarehouseAddressList.Count);
		}

		public void TestToWarehouseBondedWarehouseAddressList()
		{
			var (orgAddressBond, entryInstruction) = SetupForWarehouseBondedWarehouseAddressList();

			AssertType<OrgAddressDependentCollection>(entryInstruction.Lookups.ToWarehouseBondedWarehouseAddressList);
			AssertEquals("When ToWarehouse is not empty", 0, entryInstruction.Lookups.ToWarehouseBondedWarehouseAddressList.Count);

			entryInstruction.CEI_OA_Warehouse2 = orgAddressBond.PK;
			AssertEquals("When ToWarehouse is not empty", 3, entryInstruction.Lookups.ToWarehouseBondedWarehouseAddressList.Count);
		}

		public void TestPackageUQList()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			AssertType<ILCustomsPackTypeList>(entryInstruction.Lookups.PackageUQList);

			var packageUQList = entryInstruction.Lookups.PackageUQList;
			AssertSame("Cache", packageUQList, entryInstruction.Lookups.PackageUQList);
			AssertEquals("PackageUQList", 4, packageUQList.Count);
			AssertEquals("D5, NE, PP, VN", packageUQList.CodesAsString);
		}

		(OrgAddress, CusEntryInstruction) SetupForWarehouseBondedWarehouseAddressList()
		{
			var factory = Factory;
			var orgHeaderBond = factory.New<OrgHeader>();
			var orgAddressBond = orgHeaderBond.MainAddress;
			var helper = new WhsDataTestHelper(factory);
			helper.GetNewWhsWarehouse(orgAddressBond.PK, true, "B01", "PRW");
			orgHeaderBond.OH_Code = "Test Org";
			var orgAddress1 = orgHeaderBond.Addresses.AddNew();
			orgAddress1.OA_Code = "First Address";
			var orgAddress2 = orgHeaderBond.Addresses.AddNew();
			orgAddress2.OA_Code = "Second Address";

			var declaration = factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = "IMP";
			return (orgAddressBond, entryInstruction);
		}
	}
}
