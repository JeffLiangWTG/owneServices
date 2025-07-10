using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class PGAHeaderExtensionsTest : TestCaseWithFactory
	{
		[StressTest]
		public void TestGetCachedUNDGSubstanceCollection()
		{
			var collection1 = PGAHeaderExtensions.GetCachedUNDGSubstanceCollection(Factory);
			AssertEquals(typeof(UNDGSubstanceCollection), collection1.GetType());

			var collection2 = PGAHeaderExtensions.GetCachedUNDGSubstanceCollection(Factory);
			AssertSame(collection1, collection2);

			AssertEquals(true, collection2.All(x => x.DG_Standard == UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO));
			AssertEquals("Standard", collection2.FilterBusinessObjectDefaults["Standard:Property"].FilterName);
		}

		public void TestGetEnabledProgramCodes()
		{
			var pgaheader = Factory.New<HCPGAHeader>();
			AssertEquals(0, pgaheader.GetEnabledProgramCodes().Count());

			pgaheader.CA_OCSProgramInd = "Y";

			var enabledPrograms = pgaheader.GetEnabledProgramCodes().ToArray();
			AssertEquals(1, enabledPrograms.Length);
			AssertEquals("OCS", enabledPrograms[0]);
		}

		public void TestIsProgramEnabled()
		{
			var pgaheader = Factory.New<HCPGAHeader>();
			pgaheader.CA_OCSProgramInd = "Y";
			pgaheader.CA_REDProgramInd = "N";

			Assert(pgaheader.IsProgramEnabled("OCS"));
			Assert(!pgaheader.IsProgramEnabled("RED"));
		}

		public void TestGetDefaultLPCOTypesForAllEnalbedPrograms()
		{
			var pgaheader = Factory.New<ECCCPGAHeader>();
			pgaheader.CA_ProcessCode = ProcessCodes.Codes.XE02;
			pgaheader.CA_AOSConformity = AffirmationOfStatementCodes.Codes.ME03;
			pgaheader.CA_VEEProgramInd = "Y";
			AssertContainsExactElementsInAnyOrder(new[] { "8030", "8031" }, pgaheader.GetDefaultLPCOTypesForAllEnalbedPrograms().Select(x => x.Code));

			pgaheader.CA_WRMProgramInd = "Y";
			pgaheader.CA_ODSProgramInd = "Y";
			pgaheader.CA_AOSConformity = AffirmationOfStatementCodes.Codes.ME02;
			AssertContainsExactElementsInAnyOrder(new[] { "8000", "8001", "8010", "8011", "8012" }, pgaheader.GetDefaultLPCOTypesForAllEnalbedPrograms().Select(x => x.Code));

			var phacPGAHeader = Factory.New<PHACPGAHeader>();
			phacPGAHeader.CA_HAPProgramInd = YesNoList.Codes.Yes;
			AssertContainsExactElementsInAnyOrder(new string[] { "5503" }, phacPGAHeader.GetDefaultLPCOTypesForAllEnalbedPrograms().Select(x => x.Code));

			var gacPGAHeader = Factory.New<GACPGAHeader>();
			gacPGAHeader.CA_AllProgramInd = YesNoList.Codes.Yes;
			AssertContainsExactElementsInAnyOrder(new[] { "2001", "2003", "2004", "2005", "2006", "2007", "80", "81", "83" }, gacPGAHeader.GetDefaultLPCOTypesForAllEnalbedPrograms().Select(x => x.Code));
		}

		public void GetDefaultDocumentTypes_HC_HDR_HC31()
		{
			var pgaheader = Factory.New<HCPGAHeader>();
			pgaheader.CA_HDRProgramInd = YesNoList.Codes.Yes;
			pgaheader.CA_IntendedUseCodeHDR = HCIntendedUseCode.Codes.HC31;
			pgaheader.CA_CategoryHDR = HCCategories.Codes.HC05;
			AssertContainsExactElementsInAnyOrder(new string[] { LPCODocumentTypeQualifier.Codes._5010 }, pgaheader.GetDefaultDocumentTypes(PGACodes.Codes.HC).Select(x => x.Code));

			pgaheader.CA_CategoryHDR = HCCategories.Codes.HC01;
			AssertEquals(false, pgaheader.GetDefaultDocumentTypes(PGACodes.Codes.HC).Any());

			pgaheader.CA_CategoryHDR = HCCategories.Codes.HC06;
			AssertContainsExactElementsInAnyOrder(new string[] { LPCODocumentTypeQualifier.Codes._5010 }, pgaheader.GetDefaultDocumentTypes(PGACodes.Codes.HC).Select(x => x.Code));

			pgaheader.CA_IntendedUseCodeHDR = "@!@";
			AssertEquals(false, pgaheader.GetDefaultDocumentTypes(PGACodes.Codes.HC).Any());
		}

		public void TestGetMandatoryURNDocumentTypeHC()
		{
			var pgaheader = Factory.New<HCPGAHeader>();
			var madatoryDocTypes = pgaheader.GetURNMandatoryDocumentTypes();
			AssertEquals(2, madatoryDocTypes.Count);

			AssertEquals("5009", true, madatoryDocTypes.Contains(LPCODocumentTypeQualifier.Codes._5009));
			AssertEquals("5008", true, madatoryDocTypes.Contains(LPCODocumentTypeQualifier.Codes._5008));
		}

		public void TestGetMandatoryURNDocumentTypeTC()
		{
			var pgaheader = Factory.New<TCPGAHeader>();
			var madatoryDocTypes = pgaheader.GetURNMandatoryDocumentTypes();
			AssertEquals(6, madatoryDocTypes.Count);

			AssertEquals("4002", true, madatoryDocTypes.Contains(LPCODocumentTypeQualifier.Codes._4002));
			AssertEquals("4003", true, madatoryDocTypes.Contains(LPCODocumentTypeQualifier.Codes._4003));
			AssertEquals("4005", true, madatoryDocTypes.Contains(LPCODocumentTypeQualifier.Codes._4005));
			AssertEquals("4006", true, madatoryDocTypes.Contains(LPCODocumentTypeQualifier.Codes._4006));
			AssertEquals("4001", true, madatoryDocTypes.Contains(LPCODocumentTypeQualifier.Codes._4001));
			AssertEquals("4004", true, madatoryDocTypes.Contains(LPCODocumentTypeQualifier.Codes._4004));
		}

		public void TestGetMandatoryURNDocumentTypeECC()
		{
			var pgaheader = Factory.New<ECCCPGAHeader>();
			var madatoryDocTypes = pgaheader.GetURNMandatoryDocumentTypes();
			AssertEquals(11, madatoryDocTypes.Count);

			AssertEquals("8020", true, madatoryDocTypes.Contains(LPCODocumentTypeQualifier.Codes._8020));
			AssertEquals("8021", true, madatoryDocTypes.Contains(LPCODocumentTypeQualifier.Codes._8021));
			AssertEquals("8022", true, madatoryDocTypes.Contains(LPCODocumentTypeQualifier.Codes._8022));
			AssertEquals("8023", true, madatoryDocTypes.Contains(LPCODocumentTypeQualifier.Codes._8023));

			AssertEquals("8020", true, madatoryDocTypes.Contains(LPCODocumentTypeQualifier.Codes._8000));
			AssertEquals("8001", true, madatoryDocTypes.Contains(LPCODocumentTypeQualifier.Codes._8001));
			AssertEquals("8010", true, madatoryDocTypes.Contains(LPCODocumentTypeQualifier.Codes._8010));
			AssertEquals("8011", true, madatoryDocTypes.Contains(LPCODocumentTypeQualifier.Codes._8011));
			AssertEquals("8012", true, madatoryDocTypes.Contains(LPCODocumentTypeQualifier.Codes._8012));

			// ecccPGAHeader.CA_ProcessCode = ProcessCodes.Codes.XE02
			// ecccPGAHeader.CA_AOSConformity = AffirmationOfStatementCodes.Codes.ME03
			AssertEquals("8030", true, madatoryDocTypes.Contains(LPCODocumentTypeQualifier.Codes._8030));
			AssertEquals("8031", true, madatoryDocTypes.Contains(LPCODocumentTypeQualifier.Codes._8031));

			var pgaheaderDFO = Factory.New<DFOPGAHeader>();
			var madatoryDocTypesDFO = pgaheaderDFO.GetURNMandatoryDocumentTypes();
			AssertEquals("nothing", 0, madatoryDocTypesDFO.Count);
			var pgaheaderGAC = Factory.New<GACPGAHeader>();
			var madatoryDocTypesGAC = pgaheaderDFO.GetURNMandatoryDocumentTypes();
			AssertEquals("nothing", 0, madatoryDocTypesGAC.Count);
		}
	}
}
