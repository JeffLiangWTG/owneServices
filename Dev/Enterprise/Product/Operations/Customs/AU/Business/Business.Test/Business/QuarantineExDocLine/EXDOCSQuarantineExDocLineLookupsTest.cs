using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDOCSQuarantineExDocLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSupplementaryCodes()
		{
			AssertEquals(ZQuery.NoResultQuery, ((ILegacyBusinessObjectCollectionInternals)orphanedQuarantineLine.Lookups.SupplementaryCodes).AdditionalFilter);

			var newFactory = new BusinessObjectFactory();
			var refHelper = new UniversalReferenceTestDataHelper(newFactory);
			refHelper.CreateNewOrGetExistingCusCodeType("SUPP", "SUPP");
			var beef = refHelper.CreateNewOrGetExistingCusCodeList("AU", "SUPP", "G", "GRAIN FED", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateCusCodeListAttribute(beef.PK, "IsMeat", "");
			var trout = refHelper.CreateNewOrGetExistingCusCodeList("AU", "SUPP", "WO", "WILD ORIGIN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateCusCodeListAttribute(trout.PK, "IsFish", "");
			newFactory.Save();

			var filter = (quarantineLine.Lookups.SupplementaryCodes as BusinessObjectCollection).CompleteFilter;
			AssertEquals(true, Factory.Load<ZZRefCusCodeListCombined>(beef.PK).MatchesFilter(filter));
			AssertEquals(true, Factory.Load<ZZRefCusCodeListCombined>(trout.PK).MatchesFilter(filter));
		}

		public void TestPackType()
		{
			var packType = quarantineLine.Lookups.PackType;
			AssertEquals("BB, BG, BL, BI, BK, CS, BX, VR, VL, BP, BT, BE, CA, CT, CR, CB, DZ, DM, EN, ES, FL, FE, GP, IW, IV, JA, LP, LV, MA, MW, MV, OC, PA, PS, PL, PF, PP, PC, PM, PB, PR, PX, PJ, RO, SC, SO, SS, TP, TV, TB, TU, UP, VP, VI, WA", packType.CodesAsString);
			AssertSame("Should be Cached.", packType, Factory.New<QuarantineExDocLine>().Lookups.PackType);
		}

		public void TestNatureOfCommodity()
		{
			var natureOfCommodity = quarantineLine.Lookups.NatureOfCommodity;
			AssertEquals("AL, BP, AQ, BA, BAR, BE, BM, BV, BN, CQ, CS, CW, CHF, CI, CC, CL, CU, CT, DA, DF, DN, EG, EGP, GAF, FIP, FI, FGP, FP, FPC, FPN, FIF, HAF, PIF, FM, OK, GA, GE, BG, HAP, HA, HPP, HO, JE, GAL, MB, MPR, MP, MI, MIC, MM, MN, PL, PIP, PI, PPP, MIR, RP, ALR, PIR, SES, GAS, GAB, TO, VL, WAP, GAW, WO, BYR", natureOfCommodity.CodesAsString);
			AssertSame("Should be Cached.", natureOfCommodity, Factory.New<QuarantineExDocLine>().Lookups.NatureOfCommodity);
		}

		public void TestPreservation()
		{
			var preservation = quarantineLine.Lookups.Preservation;
			AssertEquals("C, F, X, U", preservation.CodesAsString);
			AssertSame("Should be Cached.", preservation, Factory.New<QuarantineExDocLine>().Lookups.Preservation);
		}

		[TestDate(2022, 1, 18)]
		public void TestNetQuantityUnits()
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);

			var date1 = new ZDateTime(2022, 1, 1);
			var date2 = new ZDateTime(2022, 1, 31);
			var date3 = new ZDateTime(2022, 1, 3);

			helper.CreateNewOrGetExistingCusCodeList("AU", "EUOM", "ONZ", "OUNCE", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("US", "EUOM", "DZN", "DOZEN", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "EUOM", "DMT", "DECIMETRE", date1, date3);
			newFactory.Save();

			var netQuantityUnits = quarantineLine.Lookups.NetQuantityUnits;

			AssertEquals("NetQuantityUnits Lookup count", 1, netQuantityUnits.Count);
			AssertContains("ONZ", netQuantityUnits.CodesAsString);
			AssertSame("Should be Cached.", netQuantityUnits, Factory.New<QuarantineExDocLine>().Lookups.NetQuantityUnits);
		}

		public void TestPackageTypes()
		{
			var packageTypes = quarantineLine.Lookups.PackageTypes;
			AssertContains("Package types when final destination is not set", "BG, BL, BK, BO, BX, VR, BI, VL, BP, BE, CA, CW, CT, CK, CF, CN, CR, CB, DZ, DR, EN, FL, FE, JA, MX, OC, PA, PS, PL, PF, PW, PP, PC, PM, QR, PR, PB, PJ, CQ, RO, SC, CD, SO, SS, TK, TP, TB, TU, UP, VI", packageTypes.CodesAsString);
			AssertSame("Should be Cached.", packageTypes, quarantineHeader.InvoiceHeader.JobComInvoiceLines.AddNew().QuarantineExDocLine.Lookups.PackageTypes);

			quarantineHeader.Declaration.JE_RL_NKFinalDestination = "FRMRS";
			packageTypes = quarantineLine.Lookups.PackageTypes;
			AssertContains("Package types for EU destination", "BG, BL, BK, BO, BX, VR, BI, VL, BP, BE, CA, CW, CT, CK, CF, CN, CR, CB, DZ, DR, EN, FL, FE, JA, MX, OC, PA, PS, PL, PF, PW, PP, PM, QR, PR, PJ, CQ, RO, SC, CD, SO, SS, TK, TP, TB, TU, UP, VI", packageTypes.CodesAsString);
			AssertSame("Should be Cached.", packageTypes, quarantineHeader.InvoiceHeader.JobComInvoiceLines.AddNew().QuarantineExDocLine.Lookups.PackageTypes);

			quarantineHeader.Declaration.JE_RL_NKFinalDestination = "AUBNE";
			packageTypes = quarantineLine.Lookups.PackageTypes;
			AssertContains("Package types for non EU and Turkey destination", "BG, BL, BK, BO, BX, VR, BI, VL, BP, BE, CA, CW, CT, CK, CF, CN, CR, CB, DZ, DR, EN, FL, FE, JA, MX, OC, PA, PS, PL, PF, PW, PC, PM, PR, PB, PJ, CQ, RO, SC, CD, SO, SS, TK, TP, TB, TU, UP, VI", packageTypes.CodesAsString);
			AssertSame("Should be Cached.", packageTypes, quarantineHeader.InvoiceHeader.JobComInvoiceLines.AddNew().QuarantineExDocLine.Lookups.PackageTypes);

			quarantineHeader.Declaration.JE_RL_NKFinalDestination = "TRIST";
			packageTypes = quarantineLine.Lookups.PackageTypes;
			AssertContains("Package types for Turkey destination", "BG, BL, BK, BO, BX, VR, BI, VL, BP, BE, CA, CW, CT, CK, CF, CN, CR, CB, DZ, DR, EN, FL, FE, JA, MX, OC, PA, PS, PL, PF, PW, PP, PM, QR, PR, PJ, CQ, RO, SC, CD, SO, SS, TK, TP, TB, TU, UP, VI", packageTypes.CodesAsString);
			AssertSame("Should be Cached.", packageTypes, quarantineHeader.InvoiceHeader.JobComInvoiceLines.AddNew().QuarantineExDocLine.Lookups.PackageTypes);
		}

		[TestDate(2022, 1, 18)]
		public void TestWeight()
		{
			var newFactory = new BusinessObjectFactory();
			var refDataHelper = new UniversalReferenceTestDataHelper(newFactory);

			var date1 = new ZDateTime(2022, 1, 1);
			var date2 = new ZDateTime(2022, 1, 3);
			var date3 = new ZDateTime(2022, 1, 31);

			refDataHelper.CreateNewOrGetExistingCusCodeList("AU", "EUOM", "ONZ", "OUNCE", date1, date2);
			refDataHelper.CreateNewOrGetExistingCusCodeList("US", "EUOM", "DZN", "DOZEN", date1, date2);
			refDataHelper.CreateNewOrGetExistingCusCodeList("AU", "EUOM", "DMT", "DECIMETRE", date1, date3);
			newFactory.Save();

			AssertEquals("NetQuantityUnits Lookup count", 1, orphanedQuarantineLine.Lookups.Weight.Count);
			AssertContains("DMT", orphanedQuarantineLine.Lookups.Weight.CodesAsString);

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			var weight = quarantineLine.Lookups.Weight;
			AssertEquals("CGM, CU, DTN, GRN, GRM, HGM, JCM, KGM, KTN, MTO, TNE, MTK, MTN, MGM, MT, MTS, NO, SM, SS, TN", weight.CodesAsString);

			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			helper.Header1.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			AssertSame("Should be Cached.", weight, helper.Line1.QuarantineExDocLine.Lookups.Weight);

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			weight = quarantineLine.Lookups.Weight;
			AssertContains("DMT", weight.CodesAsString);
			helper.Header1.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			AssertSame("Should be Cached.", weight, helper.Line1.QuarantineExDocLine.Lookups.Weight);
		}

		public void TestMetricWeight()
		{
			var metricWeight = quarantineLine.Lookups.MetricWeight;
			AssertEquals("CGM, CU, DTN, GRN, GRM, HGM, JCM, KGM, KTN, MTO, TNE, MTK, MTN, MGM, MT, MTS, NO, SM, SS, TN", metricWeight.CodesAsString);
			AssertSame("Should be Cached.", metricWeight, Factory.New<QuarantineExDocLine>().Lookups.MetricWeight);
		}

		public void TestProductTypes()
		{
			AssertEquals(ZQuery.NoResultQuery, ((ILegacyBusinessObjectCollectionInternals)orphanedQuarantineLine.Lookups.ProductTypes).AdditionalFilter);

			var newFactory = new BusinessObjectFactory();
			var refHelper = new UniversalReferenceTestDataHelper(newFactory);
			refHelper.CreateNewOrGetExistingCusCodeType("PRODM", "Meat");
			var tbone = refHelper.CreateNewOrGetExistingCusCodeList("AU", "PRODM", "TBO", "TBone", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeType("PRODE", "Egg");
			var egg = refHelper.CreateNewOrGetExistingCusCodeList("AU", "PRODE", "EGG", "Egg", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();

			var filter = quarantineLine.Lookups.ProductTypes.CompleteFilter;
			AssertEquals(true, Factory.Load<ZZRefCusCodeListCombined>(tbone.PK).MatchesFilter(filter));
			AssertEquals(false, Factory.Load<ZZRefCusCodeListCombined>(egg.PK).MatchesFilter(filter));
		}

		public void TestCategoryCodes()
		{
			AssertNull(orphanedQuarantineLine.Lookups.CategoryCodes);
			AssertNull(quarantineLine.Lookups.CategoryCodes);
		}

		public void TestTreatmentType()
		{
			var newFactory = new BusinessObjectFactory();
			var refHelper = new UniversalReferenceTestDataHelper(newFactory);
			refHelper.CreateNewOrGetExistingCusCodeType("EXE34", "EXDOCS Code Set - E34 Treatment Type", "AU");
			refHelper.CreateNewOrGetExistingCusCodeList("AU", "EXE34", "CHIL", "CHILLED", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			refHelper.CreateNewOrGetExistingCusCodeList("AU", "EXE34", "CHT", "COMBINED HEAT TREATMENT", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			newFactory.Save();

			var treatmentType = quarantineLine.Lookups.TreatmentType;
			AssertEquals(2, treatmentType.Count);
			AssertEquals("CHIL, CHT", treatmentType.CodesAsString);
			AssertSame("Should be Cached.", treatmentType, Factory.New<QuarantineExDocLine>().Lookups.TreatmentType);
		}

		public void TestCutCodes()
		{
			AssertEquals(ZQuery.NoResultQuery, ((ILegacyBusinessObjectCollectionInternals)orphanedQuarantineLine.Lookups.CutCodes).AdditionalFilter);

			var newFactory = new BusinessObjectFactory();
			var refHelper = new UniversalReferenceTestDataHelper(newFactory);
			refHelper.CreateNewOrGetExistingCusCodeType("CUTCE", "Egg");
			var butter = refHelper.CreateNewOrGetExistingCusCodeList("AU", "CUTCE", "BUT", "Butter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeType("CUTCM", "Meat");
			var veal = refHelper.CreateNewOrGetExistingCusCodeList("AU", "CUTCM", "3398", "BONLESS VEAL ROSTBIFF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();

			var filter = quarantineLine.Lookups.CutCodes.CompleteFilter;
			AssertEquals(true, Factory.Load<ZZRefCusCodeListCombined>(veal.PK).MatchesFilter(filter));
			AssertEquals(false, Factory.Load<ZZRefCusCodeListCombined>(butter.PK).MatchesFilter(filter));
		}

		public void TestLocationQualifier()
		{
			var locationQualifier = quarantineLine.Lookups.LocationQualifier;
			AssertEquals("Australian, Tasmanian", locationQualifier.CodesAsString);
			AssertSame("Should be Cached.", locationQualifier, Factory.New<QuarantineExDocLine>().Lookups.LocationQualifier);
		}

		public void TestDominantProducts()
		{
			AssertEquals(ZQuery.NoResultQuery, ((ILegacyBusinessObjectCollectionInternals)orphanedQuarantineLine.Lookups.DominantProducts).AdditionalFilter);

			var newFactory = new BusinessObjectFactory();
			var refHelper = new UniversalReferenceTestDataHelper(newFactory);
			refHelper.CreateNewOrGetExistingCusCodeType("DOMP", "DOMP");
			var emu = refHelper.CreateNewOrGetExistingCusCodeList("AU", "DOMP", "EMU", "EMU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var goat = refHelper.CreateNewOrGetExistingCusCodeList("GR", "DOMP", "GOAT", "GOAT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();

			var collectionFilter = quarantineLine.Lookups.DominantProducts.CompleteFilter;
			AssertEquals(true, Factory.Load<ZZRefCusCodeListCombined>(emu.PK).MatchesFilter(collectionFilter));
			AssertEquals(false, Factory.Load<ZZRefCusCodeListCombined>(goat.PK).MatchesFilter(collectionFilter));
		}

		public void TestNetImperialWeightUnit()
		{
			var netImperialWeightUnit = quarantineLine.Lookups.NetImperialWeightUnit;
			AssertEquals("CWI, ONZ, LBR, STI, LTN, STN", netImperialWeightUnit.CodesAsString);
			AssertSame("Should be Cached.", netImperialWeightUnit, Factory.New<QuarantineExDocLine>().Lookups.NetImperialWeightUnit);
		}

		public void TestAqisCustomsWeightUqList()
		{
			var aqisCustomsWeightUqList = quarantineLine.Lookups.AqisCustomsWeightUqList;
			AssertEquals("CT, CU, KG, KGM, LTR, NO, NR, SM, TNE", aqisCustomsWeightUqList.CodesAsString);
			AssertSame("Should be Cached.", aqisCustomsWeightUqList, Factory.New<QuarantineExDocLine>().Lookups.AqisCustomsWeightUqList);
		}

		public void TestPackAccuracy()
		{
			var packAccuracy = quarantineLine.Lookups.PackAccuracy;
			AssertEquals("3, 4", packAccuracy.CodesAsString);
			AssertSame("Should be Cached.", packAccuracy, Factory.New<QuarantineExDocLine>().Lookups.PackAccuracy);
		}

		public void TestProductPartList()
		{
			var newFactory = new BusinessObjectFactory();
			var refHelper = new UniversalReferenceTestDataHelper(newFactory);
			refHelper.CreateNewOrGetExistingCusCodeType(Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EXDOCSProductPart, "EXDOCS Code Set - E44 Product Part");
			refHelper.CreateNewOrGetExistingCusCodeList("AU", Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EXDOCSProductPart, "1", "Seeds", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeList("AU", Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EXDOCSProductPart, "2", "Plants", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();

			var productPartsList = quarantineLine.Lookups.ProductPart;
			AssertEquals("1, 2", productPartsList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			quarantineHeader = helper.Header1.QuarantineExDocHeader;
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			quarantineLine = helper.Line1.QuarantineExDocLine;
			orphanedQuarantineLine = Factory.New<QuarantineExDocLine>();
		}
		QuarantineExDocHeader quarantineHeader;
		QuarantineExDocLine quarantineLine;
		QuarantineExDocLine orphanedQuarantineLine;
	}
}
