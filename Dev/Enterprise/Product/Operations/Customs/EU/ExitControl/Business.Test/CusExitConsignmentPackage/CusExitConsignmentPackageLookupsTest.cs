using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;
using static Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	class CusExitConsignmentPackageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackTypeList()
		{
			var helperCustomsOffice = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helperCustomsOffice.CreateNewOrGetExistingCusCodeType(UnitedNationsPackageTypes, "Package Units");
			helperCustomsOffice.CreateNewOrGetExistingCusCodeType("Inv", "Invalid Type");
			helperCustomsOffice.CreateCusCodeList(UnitedNationsRecommendations, UnitedNationsPackageTypes, "01", "Test1", new ZDateTime(2011, 12, 20), ZDateTime.Now.AddDays(5));
			helperCustomsOffice.CreateCusCodeList(UnitedNationsRecommendations, UnitedNationsPackageTypes, "02", "Test2", new ZDateTime(2011, 12, 20), ZDateTime.Now.AddDays(5));
			helperCustomsOffice.CreateCusCodeList(UnitedNationsRecommendations, UnitedNationsPackageTypes, "03", "Invalid startDate", new ZDateTime(2012, 02, 20), ZDateTime.Now.AddDays(5));
			helperCustomsOffice.CreateCusCodeList(UnitedNationsRecommendations, UnitedNationsPackageTypes, "04", "Invalid endDate", new ZDateTime(2011, 12, 20), ZDateTime.Now.AddDays(-5));
			helperCustomsOffice.CreateCusCodeList(UnitedNationsRecommendations, "Inv", "05", "Invalid codeType", new ZDateTime(2011, 12, 20), ZDateTime.Now.AddDays(5));
			helperCustomsOffice.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, UnitedNationsPackageTypes, "06", "Invalid dataGroupingCode", new ZDateTime(2011, 12, 20), ZDateTime.Now.AddDays(5));
			Factory.Save();

			(var exitConsignmentPackage, _, _, _, _) = CusExitConsignmentPackageTest.GetNewBusinessObject(Factory);
			AssertEquals("01, 02", exitConsignmentPackage.Lookups.PackTypeList.CodesAsString);
		}

		public void TestBulkPackageUnitTypeList()
		{
			Factory.SetupBulkCusCode();
			AssertEquals("VG", lookups.BulkPackageUnitTypeList.CodesAsString);
		}

		public void TestBreakBulkPackageUnitTypeList()
		{
			Factory.SetupBreakBulkCusCode();
			AssertEquals("NE", lookups.BreakBulkPackageUnitTypeList.CodesAsString);
		}

		public void TestStatusList()
		{
			var lookups = new CusExitConsignmentPackageLookups(Factory.New<CusExitConsignmentPackage>());
			AssertType<CodeDescriptionPairList>("Empty Value", lookups.StatusList);
		}

		protected override void SetUp()
		{
			base.SetUp();

			(var exitConsignmentPackage, _, _, _, _) = CusExitConsignmentPackageTest.GetNewBusinessObject(Factory);
			lookups = exitConsignmentPackage.Lookups;
		}

		CusExitConsignmentPackageLookups lookups;
	}
}
