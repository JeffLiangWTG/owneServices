using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class HCPGAHeaderAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUNDGCodeList()
		{
			AssertEquals(typeof(UNDGSubstanceCollection), header.AddInfoLookups.UNDGCodeList.GetType());
		}

		public void TestIntededUseCodesLookups()
		{
			header.CA_APIProgramInd = YesNoList.Codes.Yes;
			var listAPI = header.AddInfoLookups.IntendedUseCodesAPI;
			AssertEquals("HC13", listAPI.CodesAsString);
			header.CA_BBCProgramInd = YesNoList.Codes.Yes;
			var listBBC = header.AddInfoLookups.IntendedUseCodesBBC;
			AssertEquals("HC01", listBBC.CodesAsString);
			header.CA_CTOProgramInd = YesNoList.Codes.Yes;
			var listCTO = header.AddInfoLookups.IntendedUseCodesCTO;
			AssertEquals("HC01", listCTO.CodesAsString);
			header.CA_CPRProgramInd = YesNoList.Codes.Yes;
			var listCPR = header.AddInfoLookups.IntendedUseCodesCPR;
			AssertEquals("HC21, HC22, HC23, HC24, HC26, HC27", listCPR.CodesAsString);
			header.CA_DSEProgramInd = YesNoList.Codes.Yes;
			var listDSE = header.AddInfoLookups.IntendedUseCodesDSE;
			AssertEquals("HC01, HC02", listDSE.CodesAsString);
			header.CA_HDRProgramInd = YesNoList.Codes.Yes;
			var listHDR = header.AddInfoLookups.IntendedUseCodesHDR;
			AssertEquals("HC01, HC02, HC05, HC07, HC29, HC31", listHDR.CodesAsString);
			header.CA_NHPProgramInd = YesNoList.Codes.Yes;
			var listNHP = header.AddInfoLookups.IntendedUseCodesNHP;
			AssertEquals("HC01, HC02, HC05, HC07, HC29", listNHP.CodesAsString);
			header.CA_OCSProgramInd = YesNoList.Codes.Yes;
			var listOCS = header.AddInfoLookups.IntendedUseCodesOCS;
			AssertEquals("HC01, HC02, HC05, HC10, HC13, HC15, HC16, HC17, HC18, HC19, HC20, HC28", listOCS.CodesAsString);
			header.CA_MDEProgramInd = YesNoList.Codes.Yes;
			var listMDE = header.AddInfoLookups.IntendedUseCodesMDE;
			AssertEquals("HC01, HC02, HC03, HC04, HC07, HC29, HC30", listMDE.CodesAsString);
			header.CA_PESProgramInd = YesNoList.Codes.Yes;
			var listPES = header.AddInfoLookups.IntendedUseCodesPES;
			AssertEquals("HC06, HC07, HC08, HC09", listPES.CodesAsString);
			header.CA_VETProgramInd = YesNoList.Codes.Yes;
			var listVET = header.AddInfoLookups.IntendedUseCodesVET;
			AssertEquals("HC07, HC10, HC11, HC12, HC14, HC29", listVET.CodesAsString);

			var headerAPI = Factory.New<HCPGAHeader>();
			headerAPI.CA_CPRProgramInd = YesNoList.Codes.Yes;
			AssertSame("IntendedUseCodeListAPI is cached", listAPI, headerAPI.AddInfoLookups.IntendedUseCodesAPI);

			var headerBBC = Factory.New<HCPGAHeader>();
			headerBBC.CA_CPRProgramInd = YesNoList.Codes.Yes;
			AssertSame("IntendedUseCodeListBBC is cached", listBBC, headerBBC.AddInfoLookups.IntendedUseCodesBBC);

			var headerCPR = Factory.New<HCPGAHeader>();
			headerCPR.CA_CPRProgramInd = YesNoList.Codes.Yes;
			AssertSame("IntendedUseCodeListCPR is cached", listCPR, headerCPR.AddInfoLookups.IntendedUseCodesCPR);

			var headerCTO = Factory.New<HCPGAHeader>();
			headerCTO.CA_CTOProgramInd = YesNoList.Codes.Yes;
			AssertSame("IntendedUseCodeListCTO is cached", listCTO, headerCTO.AddInfoLookups.IntendedUseCodesCTO);

			var headerDSE = Factory.New<HCPGAHeader>();
			headerDSE.CA_DSEProgramInd = YesNoList.Codes.Yes;
			AssertSame("IntendedUseCodeListDSE is cached", listDSE, headerDSE.AddInfoLookups.IntendedUseCodesDSE);

			var headerHDR = Factory.New<HCPGAHeader>();
			headerHDR.CA_HDRProgramInd = YesNoList.Codes.Yes;
			AssertSame("IntendedUseCodeListHDR is cached", listHDR, headerHDR.AddInfoLookups.IntendedUseCodesHDR);

			var headerMDE = Factory.New<HCPGAHeader>();
			headerMDE.CA_MDEProgramInd = YesNoList.Codes.Yes;
			AssertSame("IntendedUseCodeListMDE is cached", listMDE, headerMDE.AddInfoLookups.IntendedUseCodesMDE);

			var headerNHP = Factory.New<HCPGAHeader>();
			headerNHP.CA_NHPProgramInd = YesNoList.Codes.Yes;
			AssertSame("IntendedUseCodeListNHP is cached", listNHP, headerNHP.AddInfoLookups.IntendedUseCodesNHP);

			var headerOCS = Factory.New<HCPGAHeader>();
			headerOCS.CA_OCSProgramInd = YesNoList.Codes.Yes;
			AssertSame("IntendedUseCodeListOCS is cached", listOCS, headerOCS.AddInfoLookups.IntendedUseCodesOCS);

			var headerPES = Factory.New<HCPGAHeader>();
			headerPES.CA_PESProgramInd = YesNoList.Codes.Yes;
			AssertSame("IntendedUseCodeListPES is cached", listPES, headerPES.AddInfoLookups.IntendedUseCodesPES);

			var headerVET = Factory.New<HCPGAHeader>();
			headerVET.CA_VETProgramInd = YesNoList.Codes.Yes;
			AssertSame("IntendedUseCodeListVET is cached", listVET, headerVET.AddInfoLookups.IntendedUseCodesVET);
		}

		public void TestProgramCodeForRED()
		{
			header.CA_REDProgramInd = YesNoList.Codes.Yes;
			AssertEquals("HC40, HC41, HC42, HC43, HC44, HC45", header.AddInfoLookups.CategoryCodesRED.CodesAsString);
		}

		public void TestCategoryLookupAPI()
		{
			header.CA_IntendedUseCodeAPI = HCIntendedUseCode.Codes.HC13;
			var list = header.AddInfoLookups.CategoryCodesAPI;
			AssertEquals("HC01", list.CodesAsString);
			var header2 = Factory.New<HCPGAHeader>();
			header2.CA_IntendedUseCodeAPI = HCIntendedUseCode.Codes.HC13;
			AssertSame("List is cached", list, header2.AddInfoLookups.CategoryCodesAPI);
		}

		public void TestCategoryLookupBBC()
		{
			header.CA_IntendedUseCodeBBC = HCIntendedUseCode.Codes.HC01;
			var list = header.AddInfoLookups.CategoryCodesBBC;
			AssertEquals("HC02", list.CodesAsString);
			var header2 = Factory.New<HCPGAHeader>();
			header2.CA_IntendedUseCodeBBC = HCIntendedUseCode.Codes.HC01;
			AssertSame("List is cached", list, header2.AddInfoLookups.CategoryCodesBBC);
		}
		public void TestCategoryLookupCTO()
		{
			header.CA_IntendedUseCodeCTO = HCIntendedUseCode.Codes.HC01;
			var list = header.AddInfoLookups.CategoryCodesCTO;
			AssertEquals("HC26, HC27, HC28", list.CodesAsString);
			var header2 = Factory.New<HCPGAHeader>();
			header2.CA_IntendedUseCodeCTO = HCIntendedUseCode.Codes.HC01;
			AssertEquals("list is cached", list, header2.AddInfoLookups.CategoryCodesCTO);
		}
		public void TestCategoryLookupCPR()
		{
			header.CA_IntendedUseCodeCPR = HCIntendedUseCode.Codes.HC21;
			var list = header.AddInfoLookups.CategoryCodesCPR;
			AssertEquals("HC29, HC30, HC31, HC32, HC33, HC34, HC35, HC36, HC37", list.CodesAsString);
			header.CA_IntendedUseCodeCPR = HCIntendedUseCode.Codes.HC22;
			AssertEquals("HC37", header.AddInfoLookups.CategoryCodesCPR.CodesAsString);
			var header2 = Factory.New<HCPGAHeader>();
			header2.CA_IntendedUseCodeCPR = HCIntendedUseCode.Codes.HC21;
			AssertSame("List is cached", list, header2.AddInfoLookups.CategoryCodesCPR);
		}
		public void TestCategoryLookupDSE()
		{
			header.CA_IntendedUseCodeDSE = HCIntendedUseCode.Codes.HC01;
			var list = header.AddInfoLookups.CategoryCodesDSE;
			AssertEquals("HC04", list.CodesAsString);
			header.CA_IntendedUseCodeDSE = HCIntendedUseCode.Codes.HC02;
			AssertEquals("HC04", header.AddInfoLookups.CategoryCodesDSE.CodesAsString);
			var header2 = Factory.New<HCPGAHeader>();
			header2.CA_IntendedUseCodeDSE = HCIntendedUseCode.Codes.HC01;
			AssertSame("List is cached", list, header2.AddInfoLookups.CategoryCodesDSE);
		}
		public void TestCategoryLookupHDR()
		{
			header.CA_IntendedUseCodeHDR = HCIntendedUseCode.Codes.HC01;
			var list = header.AddInfoLookups.CategoryCodesHDR;
			AssertEquals("HC05, HC06", list.CodesAsString);
			header.CA_IntendedUseCodeHDR = HCIntendedUseCode.Codes.HC05;
			AssertEquals("HC07, HC08, HC09, HC10", header.AddInfoLookups.CategoryCodesHDR.CodesAsString);
			var header2 = Factory.New<HCPGAHeader>();
			header2.CA_IntendedUseCodeHDR = HCIntendedUseCode.Codes.HC01;
			AssertSame("List is cached", list, header2.AddInfoLookups.CategoryCodesHDR);
		}
		public void TestCategoryLookupOCS()
		{
			header.CA_OCSProgramInd = YesNoList.Codes.Yes;
			header.CA_IntendedUseCodeOCS = HCIntendedUseCode.Codes.HC02;
			var list = header.AddInfoLookups.CategoryCodesOCS;
			AssertEquals("HC19, HC20, HC22", list.CodesAsString);
			header.CA_IntendedUseCodeOCS = HCIntendedUseCode.Codes.HC10;
			AssertEquals("HC19, HC20, HC22, HC24", header.AddInfoLookups.CategoryCodesOCS.CodesAsString);
			var header2 = Factory.New<HCPGAHeader>();
			header2.CA_IntendedUseCodeOCS = HCIntendedUseCode.Codes.HC02;
			AssertSame("List is cached", list, header2.AddInfoLookups.CategoryCodesOCS);
		}
		public void TestCategoryLookupMDE()
		{
			header.CA_IntendedUseCodeMDE = HCIntendedUseCode.Codes.HC01;
			var list = header.AddInfoLookups.CategoryCodesMDE;
			AssertEquals("HC11, HC12, HC13, HC14", list.CodesAsString);
			header.CA_IntendedUseCodeMDE = HCIntendedUseCode.Codes.HC02;
			AssertEquals("HC11, HC12, HC13, HC14", list.CodesAsString);
			var header2 = Factory.New<HCPGAHeader>();
			header2.CA_IntendedUseCodeMDE = HCIntendedUseCode.Codes.HC01;
			AssertSame("List is cached", list, header2.AddInfoLookups.CategoryCodesMDE);
		}
		public void TestCategoryLookupNHP()
		{
			header.CA_IntendedUseCodeNHP = HCIntendedUseCode.Codes.HC01;
			var list = header.AddInfoLookups.CategoryCodesNHP;
			AssertEquals("HC15", list.CodesAsString);
			var header2 = Factory.New<HCPGAHeader>();
			header2.CA_IntendedUseCodeNHP = HCIntendedUseCode.Codes.HC01;
			AssertSame("List is cached", list, header2.AddInfoLookups.CategoryCodesNHP);
		}
		public void TestCategoryLookupPES()
		{
			header.CA_IntendedUseCodePES = HCIntendedUseCode.Codes.HC06;
			var list = header.AddInfoLookups.CategoryCodesPES;
			AssertEquals("HC38, HC39", list.CodesAsString);
			var header2 = Factory.New<HCPGAHeader>();
			header2.CA_IntendedUseCodePES = HCIntendedUseCode.Codes.HC06;
			AssertSame("List is cached", list, header2.AddInfoLookups.CategoryCodesPES);
		}
		public void TestCategoryLookupVET()
		{
			header.CA_IntendedUseCodeVET = HCIntendedUseCode.Codes.HC10;
			var list = header.AddInfoLookups.CategoryCodesVET;
			AssertEquals("HC16, HC17", list.CodesAsString);
			var header2 = Factory.New<HCPGAHeader>();
			header2.CA_IntendedUseCodeVET = HCIntendedUseCode.Codes.HC10;
			AssertSame("List is cached", list, header2.AddInfoLookups.CategoryCodesVET);
		}
		public void TestProgramCodesList()
		{
			AssertEquals(typeof(HCPGADepartmentCodes), header.AddInfoLookups.ProgramCodesList.GetType());
		}

		public void TestProperties()
		{
			AssertEquals(typeof(CodeDescriptionPairList), header.AddInfoLookups.IntendedUseCodesAPI.GetType());
			AssertEquals(typeof(CodeDescriptionPairList), header.AddInfoLookups.CategoryCodesAPI.GetType());
			AssertEquals(typeof(HCExceptProcessingCodes), header.AddInfoLookups.ExceptProcessingCodes.GetType());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<HCPGAHeader>();
		}
		HCPGAHeader header;

		#endregion
	}
}
