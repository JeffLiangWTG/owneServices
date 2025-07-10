using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class NEXDOCSQuarantineExDocLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestFarmType()
		{
			var newFactory = new BusinessObjectFactory();
			var refHelper = new UniversalReferenceTestDataHelper(newFactory);
			refHelper.CreateNewOrGetExistingCusCodeType("NXEFT", "EGG Farm Type");
			refHelper.CreateNewOrGetExistingCusCodeList("AU", "NXEFT", "Code1", "Desc1", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			refHelper.CreateNewOrGetExistingCusCodeList("AU", "NXEFT", "Code2", "Desc2", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			newFactory.Save();

			AssertEquals("FarmType", "Code1, Code2", quarantineLine.Lookups.FarmType.CodesAsString);
		}

		[TestDate(2019, 3, 6)]
		public void TestSupplementaryCodes()
		{
			quarantineHeader.QH_ProduceType = "DAI";
			quarantineLine.QL_ProductType = "AMF";
			var supplementaryCodes = quarantineLine.Lookups.SupplementaryCodes as CodeDescriptionPairList;
			AssertEquals("DM, EK", supplementaryCodes.CodesAsString);
			AssertNotEquals(supplementaryCodes.CodesAsString, (quarantineLine2.Lookups.SupplementaryCodes as CodeDescriptionPairList).CodesAsString);
			quarantineLine2.QL_ProductType = "AMF";
			AssertSame("Should be Cached.", supplementaryCodes, (quarantineLine2.Lookups.SupplementaryCodes as CodeDescriptionPairList));
		}

		[TestDate(2019, 3, 6)]
		public void TestPackType()
		{
			quarantineHeader.QH_ProduceType = "DAI";
			quarantineLine.QL_ProductType = "AMF";
			var codes = quarantineLine.Lookups.PackType;
			AssertEquals("CS, PO", codes.CodesAsString);
			AssertNotEquals(codes.CodesAsString, quarantineLine2.Lookups.PackType.CodesAsString);
			quarantineLine2.QL_ProductType = "AMF";
			AssertSame("Should be Cached.", codes, quarantineLine2.Lookups.PackType);
		}

		public void TestCutCodesCollection()
		{
			var newFactory = new BusinessObjectFactory();
			var refHelper = new UniversalReferenceTestDataHelper(newFactory);
			refHelper.CreateNewOrGetExistingCusCodeType("NCUTC", "CutCode");
			var test1 = refHelper.CreateNewOrGetExistingCusCodeList("AU", "NCUTC", "NC1471", "TEST1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var test2 = refHelper.CreateNewOrGetExistingCusCodeList("US", "NCUTC", "NC1472", "TEST2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();

			AssertEquals(ZQuery.NoResultQuery, ((ILegacyBusinessObjectCollectionInternals)orphanedQuarantineLineLookups.CategoryCodes).AdditionalFilter);

			var filter = quarantineLine.Lookups.CutCodes.CompleteFilter;
			AssertEquals(true, Factory.Load<ZZRefCusCodeListCombined>(test1.PK).MatchesFilter(filter));
			AssertEquals(false, Factory.Load<ZZRefCusCodeListCombined>(test2.PK).MatchesFilter(filter));
		}

		[TestDate(2019, 3, 6)]
		public void TestNatureOfCommodity()
		{
			var codes = quarantineLine.Lookups.NatureOfCommodity;
			AssertEquals("AQ", codes.CodesAsString);
			AssertSame("Should be Cached.", codes, quarantineLine2.Lookups.NatureOfCommodity);
		}

		[TestDate(2019, 3, 6)]
		public void TestPreservation()
		{
			quarantineHeader.QH_ProduceType = "DAI";
			quarantineLine.QL_ProductType = "AMF";
			var codes = quarantineLine.Lookups.Preservation;
			AssertEquals("H, UR", codes.CodesAsString);
			AssertNotEquals(codes.CodesAsString, quarantineLine2.Lookups.Preservation.CodesAsString);
			quarantineLine2.QL_ProductType = "AMF";
			AssertSame("Should be Cached.", codes, quarantineLine2.Lookups.Preservation);
		}

		[TestDate(2019, 3, 6)]
		public void TestNetQuantityUnits()
		{
			var codes = quarantineLine.Lookups.NetQuantityUnits;
			AssertEquals("BIL", codes.CodesAsString);
			AssertSame("Should be Cached.", codes, quarantineLine2.Lookups.NetQuantityUnits);
		}

		[TestDate(2019, 3, 6)]
		public void TestPackageTypes()
		{
			var codes = quarantineLine.Lookups.PackageTypes;
			AssertEquals("PT, VI", codes.CodesAsString);
			AssertSame("Should be Cached.", codes, quarantineLine2.Lookups.PackageTypes);
		}

		[TestDate(2019, 3, 6)]
		public void TestWeight()
		{
			var codes = quarantineLine.Lookups.Weight;
			AssertEquals("BIL", codes.CodesAsString);
			AssertSame("Should be Cached.", codes, quarantineLine2.Lookups.Weight);
		}

		[TestDate(2019, 3, 6)]
		public void TestMetricWeight()
		{
			var codes = quarantineLine.Lookups.MetricWeight;
			AssertEquals("BIL", codes.CodesAsString);
			AssertSame("Should be Cached.", codes, quarantineLine2.Lookups.MetricWeight);
		}

		public void TestProductTypes()
		{
			var newFactory = new BusinessObjectFactory();
			var refHelper = new UniversalReferenceTestDataHelper(newFactory);
			refHelper.CreateNewOrGetExistingCusCodeType("NPRDD", "NexDoc Product Type Dairy");
			var butter = refHelper.CreateNewOrGetExistingCusCodeList("AU", "NPRDD", "BUT", "Butter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var milk = refHelper.CreateNewOrGetExistingCusCodeList("ZA", "NPRDD", "MLK", "Milk", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();

			AssertEquals(ZQuery.NoResultQuery, ((ILegacyBusinessObjectCollectionInternals)orphanedQuarantineLineLookups.ProductTypes).AdditionalFilter);

			var filter = quarantineLine.Lookups.ProductTypes.CompleteFilter;
			AssertEquals(true, Factory.Load<ZZRefCusCodeListCombined>(butter.PK).MatchesFilter(filter));
			AssertEquals(false, Factory.Load<ZZRefCusCodeListCombined>(milk.PK).MatchesFilter(filter));
		}

		public void TestCategoryCodes()
		{
			var newFactory = new BusinessObjectFactory();
			var refHelper = new UniversalReferenceTestDataHelper(newFactory);
			refHelper.CreateNewOrGetExistingCusCodeType("NPRCD", "Dairy");
			var butter = refHelper.CreateNewOrGetExistingCusCodeList("AU", "NPRCD", "BUT", "Butter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var iceCream = refHelper.CreateNewOrGetExistingCusCodeList("US", "NPRCD", "ICE", "Ice Cream", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();

			AssertEquals(ZQuery.NoResultQuery, ((ILegacyBusinessObjectCollectionInternals)orphanedQuarantineLineLookups.CategoryCodes).AdditionalFilter);

			var filter = quarantineLine.Lookups.CategoryCodes.CompleteFilter;
			AssertEquals(true, Factory.Load<ZZRefCusCodeListCombined>(butter.PK).MatchesFilter(filter));
			AssertEquals(false, Factory.Load<ZZRefCusCodeListCombined>(iceCream.PK).MatchesFilter(filter));
		}

		public void TestTreatmentType()
		{
			var codes = quarantineLine.Lookups.TreatmentType;
			AssertEquals("BI, CA, CHIL, CHT, COO, BO, DR, EV, FRE, FR, HOM, IS, LI, MT, MI, NT, PA, PL, PRE, PR, PC, RM, CH, RO, SA, SH, SHT, SK, SM, TR, UN, UNP, US", codes.CodesAsString);
			AssertSame("Should be Cached.", codes, quarantineLine2.Lookups.TreatmentType);
		}

		public void TestLocationQualifier()
		{
			var codes = quarantineLine.Lookups.LocationQualifier;
			AssertEquals("Australian, Tasmanian", codes.CodesAsString);
			AssertSame("Should be Cached.", codes, Factory.New<QuarantineExDocLine>().Lookups.LocationQualifier);
		}

		public void TestDominantProducts()
		{
			var newFactory = new BusinessObjectFactory();
			var refHelper = new UniversalReferenceTestDataHelper(newFactory);
			refHelper.CreateNewOrGetExistingCusCodeType("DOMP", "DOMP");
			var emu = refHelper.CreateNewOrGetExistingCusCodeList("AU", "DOMP", "EMU", "EMU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var goat = refHelper.CreateNewOrGetExistingCusCodeList("GR", "DOMP", "GOAT", "GOAT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();

			AssertEquals(ZQuery.NoResultQuery, ((ILegacyBusinessObjectCollectionInternals)orphanedQuarantineLineLookups.DominantProducts).AdditionalFilter);

			var collectionFilter = quarantineLine.Lookups.DominantProducts.CompleteFilter;
			AssertEquals(true, Factory.Load<ZZRefCusCodeListCombined>(emu.PK).MatchesFilter(collectionFilter));
			AssertEquals(false, Factory.Load<ZZRefCusCodeListCombined>(goat.PK).MatchesFilter(collectionFilter));
		}

		public void TestNetImperialWeightUnit()
		{
			var codes = quarantineLine.Lookups.NetImperialWeightUnit;
			AssertEquals("CWI, ONZ, LBR, STI, LTN, STN", codes.CodesAsString);
			AssertSame("Should be Cached.", codes, Factory.New<QuarantineExDocLine>().Lookups.NetImperialWeightUnit);
		}

		public void TestAqisCustomsWeightUqList()
		{
			var codes = quarantineLine.Lookups.AqisCustomsWeightUqList;
			AssertEquals("CT, CU, KG, KGM, LTR, NO, NR, SM, TNE", codes.CodesAsString);
			AssertSame("Should be Cached.", codes, Factory.New<QuarantineExDocLine>().Lookups.AqisCustomsWeightUqList);
		}

		public void TestPackAccuracy()
		{
			var codes = quarantineLine.Lookups.PackAccuracy;
			AssertEquals("3, 4", codes.CodesAsString);
			AssertSame("Should be Cached.", codes, Factory.New<QuarantineExDocLine>().Lookups.PackAccuracy);
		}

		protected override void SetUp()
		{
			base.SetUp();

			NEXDOCSUniversalTestHelper.SetUp();
			var helper1 = new ZTestHelper(Factory);
			helper1.PopulateSimpleQuarantineDeclaration();
			quarantineHeader = helper1.Header1.QuarantineExDocHeader;
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			quarantineLine = helper1.Line1.QuarantineExDocLine;
			orphanedQuarantineLineLookups = new NEXDOCSQuarantineExDocLineLookups(Factory.New<QuarantineExDocLine>());

			var helper2 = new ZTestHelper(Factory);
			helper2.PopulateSimpleQuarantineDeclaration();
			helper2.Header1.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			quarantineLine2 = helper2.Line1.QuarantineExDocLine;
		}
		QuarantineExDocHeader quarantineHeader;
		QuarantineExDocLine quarantineLine;
		NEXDOCSQuarantineExDocLineLookups orphanedQuarantineLineLookups;
		QuarantineExDocLine quarantineLine2;
	}
}
