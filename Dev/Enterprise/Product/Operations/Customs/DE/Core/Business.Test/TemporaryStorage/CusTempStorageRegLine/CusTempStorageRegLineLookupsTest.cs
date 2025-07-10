using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	public class CusTempStorageRegLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOwnerReferenceTypeList()
		{
			var ownerReferenceTypeList = lookups.OwnerReferenceTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("List values", "AWB, ULD, ZZZ", ownerReferenceTypeList.CodesAsString);
				AssertSame("Cached", ownerReferenceTypeList, lookups.OwnerReferenceTypeList);
			});
		}

		public void TestCustomsStatusList()
		{
			var customsStatusList = lookups.CustomsStatusList;
			CombineAssertions(() =>
			{
				AssertEquals("List values", "DEL, FIN, LCK, PAC, PRE, TST", customsStatusList.CodesAsString);
				AssertSame("Cached", customsStatusList, lookups.CustomsStatusList);
			});
		}

		public void TestOrganizationsFindBoxList()
		{
			AssertType<OrganisationsFindBoxCollection>(lookups.OrganizationsFindBoxList);
		}

		public void TestPackageTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			helper.CreateNewOrGetExistingCusCodeType(Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package code List");
			helper.CreateNewOrGetExistingCusCodeList(Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "VQ", "VOLUME PACKAGE WCO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "VQ", "VOLUME PACKAGE UNE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "PC", "PARCEL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var packageTypeList = lookups.PackageTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("List Values", "PC, VQ", lookups.PackageTypeList.CodesAsString);
				AssertSame("Cached", packageTypeList, lookups.PackageTypeList);
			});
		}

		public void TestUnionStatusList()
		{
			var unionStatusList = lookups.UnionStatusList;
			AssertSame(unionStatusList, lookups.UnionStatusList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line = header.CusTempStorageRegLines.AddNew();
			line.SRL_LineNumber = 1;
			lookups = new CusTempStorageRegLineLookups(line);
		}
		CusTempStorageRegHeader header;
		CusTempStorageRegLineLookups lookups;
	}
}
