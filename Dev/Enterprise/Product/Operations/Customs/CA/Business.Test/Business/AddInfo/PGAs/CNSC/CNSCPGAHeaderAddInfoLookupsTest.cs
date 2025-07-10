using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CNSCPGAHeaderAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUNDGCodeList()
		{
			AssertEquals(typeof(UNDGSubstanceCollection), header.AddInfoLookups.UNDGCodeList.GetType());
		}

		public void TestPGAIndicatorList()
		{
			AssertEquals(typeof(YesNoList), header.AddInfoLookups.PGAIndicatorList.GetType());
		}

		public void TestProgramCodesList()
		{
			AssertEquals(typeof(CNSCPGADepartmentCodes), header.AddInfoLookups.ProgramCodesList.GetType());
		}

		public void TestPackUQList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"AAA", "AAAAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BG", "BG DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BBB", "BBB DESC", new ZDateTime(1994, 3, 3), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			Factory.Save();

			// Test if new added code exists in list
			Assert("AAA", header.AddInfoLookups.PackUQList.ContainsCode("AAA"));
			Assert("BAG should not appear as it is not in the list", !header.AddInfoLookups.PackUQList.ContainsCode("BAG"));
			Assert("BBB should not appear as it is too new", !header.AddInfoLookups.PackUQList.ContainsCode("BBB"));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CNSCPGAHeader>();
			header.B7_ParentID = ZGuid.NewZGuid();
			header.B7_ParentTableCode = "B7";
		}
		CNSCPGAHeader header;

		#endregion
	}
}
