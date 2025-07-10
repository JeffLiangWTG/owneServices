using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsPackageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUnitTypeList()
		{
			var unpackPackType = Factory.SetupUnpackCusCode();
			var bulkPackType = Factory.SetupBulkCusCode();
			AssertEquals($"{unpackPackType}, {bulkPackType}", lookups.UnitTypeList.CodesAsString);
		}

		public void TestUnpackedPackageUnitTypeList()
		{
			var unpackPackType = Factory.SetupUnpackCusCode();
			AssertEquals(unpackPackType, lookups.UnpackedPackageUnitTypeList.CodesAsString);
		}

		public void TestUnpackedPackageUnitTypeListLanguage()
		{
			var unpackPackDesc = Factory.SetupUnpackCusCodeWithLanguage();
			var codes = NctsPackageLookups.GetPackageUnitTypeList(Factory, TranslationHelper.GetLanguageCode(Enterprise.Core.SharedConstants.Languages.EnglishBritish));
			AssertEquals(unpackPackDesc, codes.GetMultilingualDescriptionFromCode(codes.CodesAsString));
		}

		public void TestBulkPackageUnitTypeList()
		{
			var bulkPackType = Factory.SetupBulkCusCode();
			AssertEquals(bulkPackType, lookups.BulkPackageUnitTypeList.CodesAsString);
		}

		public void TestTypeOfDifferenceList()
		{
			AssertEquals("UnloadedStates", package.Lookups.UnloadedStates, package.Lookups.TypeOfDifferenceList);
			package.B5_B5_ParentPackage = Factory.New<NctsPackage>().PK;
			AssertEquals("Constraint", "DEC, DIF, MIS, NEW", package.Lookups.TypeOfDifferenceList.CodesAsString);
		}

		public void TestUnloadedStatesList_IsNew()
		{
			var list = package.Lookups.UnloadedStates;
			Factory.TryGetValueFromCacheOnly("UnloadedStatesCore_True_N", out CodeDescriptionPairList cachedList);
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "DEC, MIS, NEW", list.CodesAsString);
				AssertSame("Cached", list, cachedList);
			});
		}

		public void TestUnloadedStatesList()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
			Factory.TryGetValueFromCacheOnly("UnloadedStatesCore_False_N", out CodeDescriptionPairList cachedList);
			var list = package.Lookups.UnloadedStates;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "DEC, MIS", list.CodesAsString);
				AssertSame("Cached", list, cachedList);
			});
		}

		public void TestUnloadedStatesList_ShouldIncludeDIFInUnloadedStatesList_IsNew()
		{
			var lookups = new NctsPackageLookupsForUnloadedStatesTest(package);
			var list = lookups.UnloadedStates;
			Factory.TryGetValueFromCacheOnly("UnloadedStatesCore_True_Y", out CodeDescriptionPairList cachedList);
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "DEC, DIF, MIS, NEW", list.CodesAsString);
				AssertSame("Cached", list, cachedList);
			});
		}

		public void TestUnloadedStatesList_ShouldIncludeDIFInUnloadedStatesList()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
			var lookups = new NctsPackageLookupsForUnloadedStatesTest(package);
			var list = lookups.UnloadedStates;
			Factory.TryGetValueFromCacheOnly("UnloadedStatesCore_False_Y", out CodeDescriptionPairList cachedList);
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "DEC, DIF, MIS", list.CodesAsString);
				AssertSame("Cached", list, cachedList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			package = goodsItem.Packages.AddNew();
			lookups = new NctsPackageLookups(package);
		}

		class NctsPackageLookupsForUnloadedStatesTest : NctsPackageLookups
		{
			public NctsPackageLookupsForUnloadedStatesTest(NctsPackage parent) : base(parent)
			{
			}

			protected override ZBool ShouldIncludeDIFInUnloadedStatesList => true;
		}

		NctsHeader nctsHeader;
		NctsPackage package;
		NctsPackageLookups lookups;
	}
}
