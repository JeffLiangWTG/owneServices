using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(HCPGAHeader))]
	sealed class HCPGAHeaderTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<HCPGAHeader>
	{
		public void TestDangerousGoodsDGSubs()
		{
			var undg = Factory.New<UNDGSubstance>();
			undg.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undg.DG_Code = "9999";
			var undgItem = Factory.New<UNDGDataItem>();
			undgItem.DI_DG = undg.PK;

			AssertNull(invoiceLine.DangerousGoods.UNDGSubstance);
			AssertEquals(ZGuid.Empty, header.DangerousGoodsDGSubs);
			header.DangerousGoodsDGSubs = undg.PK;
			AssertNotNull(invoiceLine.DangerousGoods);
		}

		public void TestSetterSuspenderForProgramInds()
		{
			var header = Factory.New<HCPGAHeader>();
			var suspender = header.SetterSuspender;
			suspender.SuspendSetting(AutoHCPGAHeader.Schema.CA_APIProgramInd);
			suspender.SuspendSetting(AutoHCPGAHeader.Schema.CA_BBCProgramInd);
			suspender.SuspendSetting(AutoHCPGAHeader.Schema.CA_CTOProgramInd);
			suspender.SuspendSetting(AutoHCPGAHeader.Schema.CA_CPRProgramInd);
			suspender.SuspendSetting(AutoHCPGAHeader.Schema.CA_DSEProgramInd);
			suspender.SuspendSetting(AutoHCPGAHeader.Schema.CA_HDRProgramInd);
			suspender.SuspendSetting(AutoHCPGAHeader.Schema.CA_MDEProgramInd);
			suspender.SuspendSetting(AutoHCPGAHeader.Schema.CA_NHPProgramInd);
			suspender.SuspendSetting(AutoHCPGAHeader.Schema.CA_OCSProgramInd);
			suspender.SuspendSetting(AutoHCPGAHeader.Schema.CA_PESProgramInd);
			suspender.SuspendSetting(AutoHCPGAHeader.Schema.CA_REDProgramInd);
			suspender.SuspendSetting(AutoHCPGAHeader.Schema.CA_VETProgramInd);

			header.CA_APIProgramInd = "Y";
			header.CA_BBCProgramInd = "Y";
			header.CA_CTOProgramInd = "Y";
			header.CA_CPRProgramInd = "Y";
			header.CA_DSEProgramInd = "Y";
			header.CA_HDRProgramInd = "Y";
			header.CA_MDEProgramInd = "Y";
			header.CA_NHPProgramInd = "Y";
			header.CA_OCSProgramInd = "Y";
			header.CA_PESProgramInd = "Y";
			header.CA_REDProgramInd = "Y";
			header.CA_VETProgramInd = "Y";

			CombineAssertions(() =>
			{
				AssertEquals("API", ZString.Empty, header.CA_APIProgramInd);
				AssertEquals("BBC", ZString.Empty, header.CA_BBCProgramInd);
				AssertEquals("CTO", ZString.Empty, header.CA_CTOProgramInd);
				AssertEquals("CPR", ZString.Empty, header.CA_CPRProgramInd);
				AssertEquals("DSE", ZString.Empty, header.CA_DSEProgramInd);
				AssertEquals("HDR", ZString.Empty, header.CA_HDRProgramInd);
				AssertEquals("MDE", ZString.Empty, header.CA_MDEProgramInd);
				AssertEquals("NHP", ZString.Empty, header.CA_NHPProgramInd);
				AssertEquals("OCS", ZString.Empty, header.CA_OCSProgramInd);
				AssertEquals("PES", ZString.Empty, header.CA_PESProgramInd);
				AssertEquals("RED", ZString.Empty, header.CA_REDProgramInd);
				AssertEquals("VET", ZString.Empty, header.CA_VETProgramInd);
			});

			suspender.ResumeSetting(AutoHCPGAHeader.Schema.CA_APIProgramInd);
			suspender.ResumeSetting(AutoHCPGAHeader.Schema.CA_BBCProgramInd);
			suspender.ResumeSetting(AutoHCPGAHeader.Schema.CA_CTOProgramInd);
			suspender.ResumeSetting(AutoHCPGAHeader.Schema.CA_CPRProgramInd);
			suspender.ResumeSetting(AutoHCPGAHeader.Schema.CA_DSEProgramInd);
			suspender.ResumeSetting(AutoHCPGAHeader.Schema.CA_HDRProgramInd);
			suspender.ResumeSetting(AutoHCPGAHeader.Schema.CA_MDEProgramInd);
			suspender.ResumeSetting(AutoHCPGAHeader.Schema.CA_NHPProgramInd);
			suspender.ResumeSetting(AutoHCPGAHeader.Schema.CA_OCSProgramInd);
			suspender.ResumeSetting(AutoHCPGAHeader.Schema.CA_PESProgramInd);
			suspender.ResumeSetting(AutoHCPGAHeader.Schema.CA_REDProgramInd);
			suspender.ResumeSetting(AutoHCPGAHeader.Schema.CA_VETProgramInd);

			header.CA_APIProgramInd = "Y";
			header.CA_BBCProgramInd = "Y";
			header.CA_CTOProgramInd = "Y";
			header.CA_CPRProgramInd = "Y";
			header.CA_DSEProgramInd = "Y";
			header.CA_HDRProgramInd = "Y";
			header.CA_MDEProgramInd = "Y";
			header.CA_NHPProgramInd = "Y";
			header.CA_OCSProgramInd = "Y";
			header.CA_PESProgramInd = "Y";
			header.CA_REDProgramInd = "Y";
			header.CA_VETProgramInd = "Y";

			CombineAssertions(() =>
			{
				AssertEquals("API", "Y", header.CA_APIProgramInd);
				AssertEquals("BBC", "Y", header.CA_BBCProgramInd);
				AssertEquals("CTO", "Y", header.CA_CTOProgramInd);
				AssertEquals("CPR", "Y", header.CA_CPRProgramInd);
				AssertEquals("DSE", "Y", header.CA_DSEProgramInd);
				AssertEquals("HDR", "Y", header.CA_HDRProgramInd);
				AssertEquals("MDE", "Y", header.CA_MDEProgramInd);
				AssertEquals("NHP", "Y", header.CA_NHPProgramInd);
				AssertEquals("OCS", "Y", header.CA_OCSProgramInd);
				AssertEquals("PES", "Y", header.CA_PESProgramInd);
				AssertEquals("RED", "Y", header.CA_REDProgramInd);
				AssertEquals("VET", "Y", header.CA_VETProgramInd);
			});
		}

		public void TestLPCODefaulter()
		{
			var header = Factory.New<HCPGAHeader>();
			var lpcoDefaulter = header as ILPCODefaulter;
			AssertNotNull(lpcoDefaulter);
			header.CA_APIProgramInd = "Y";
			Assert(!lpcoDefaulter.ShouldDefaultLPCOFields);
			header.CA_BBCProgramInd = "Y";
			Assert(lpcoDefaulter.ShouldDefaultLPCOFields);
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_Type));
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_RefNo));
		}

		public void TestAvailableLPCOFields()
		{
			AssertEquals(3, HCPGAHeader.AvailableLPCOFields.Count);
			Assert("CLP_DIFRefNumberOrLocation", HCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation));
			Assert("CLP_RefNo", HCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RefNo));
			Assert("CLP_Type", HCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_Type));
		}

		public void TestSupportsNotes()
		{
			var bo = (HCPGAHeader)GetNewBusinessObject();

			AssertEquals(false, bo.SupportsNotes);
		}

		public void TestGetCusAddInfoType()
		{
			var pgaHeader = Factory.New<HCPGAHeader>();
			var iCusAddInfoTypeSupporter = (ICusAddInfoTypeSupporter)pgaHeader;
			iCusAddInfoTypeSupporter.AssertType(typeof(Component), CusAddInfoTypeAttribute.Codes.CAComponent);
		}

		public void TestSetDefaultValue()
		{
			var pgaHeader = Factory.New<HCPGAHeader>();
			pgaHeader.CA_APIProgramInd = YesNoList.Codes.Yes;
			AssertEquals(HCIntendedUseCode.Codes.HC13, pgaHeader.CA_IntendedUseCodeAPI);
			AssertEquals(HCCategories.Codes.HC01, pgaHeader.CA_CategoryAPI);

			pgaHeader.CA_APIProgramInd = YesNoList.Codes.No;
			pgaHeader.CA_BBCProgramInd = YesNoList.Codes.Yes;
			AssertEquals(HCIntendedUseCode.Codes.HC01, pgaHeader.CA_IntendedUseCodeBBC);
			AssertEquals(HCCategories.Codes.HC02, pgaHeader.CA_CategoryBBC);

			pgaHeader.CA_BBCProgramInd = YesNoList.Codes.No;
			pgaHeader.CA_CTOProgramInd = YesNoList.Codes.Yes;
			AssertEquals(HCIntendedUseCode.Codes.HC01, pgaHeader.CA_IntendedUseCodeCTO);

			pgaHeader.CA_CTOProgramInd = YesNoList.Codes.No;
			pgaHeader.CA_DSEProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_IntendedUseCodeDSE = HCIntendedUseCode.Codes.HC02;
			AssertEquals(HCCategories.Codes.HC04, pgaHeader.CA_CategoryDSE);

			pgaHeader.CA_DSEProgramInd = YesNoList.Codes.No;
			pgaHeader.CA_NHPProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_IntendedUseCodeNHP = HCIntendedUseCode.Codes.HC01;
			AssertEquals(HCCategories.Codes.HC15, pgaHeader.CA_CategoryNHP);

			pgaHeader.CA_CategoryNHP = ZString.Empty;
			pgaHeader.CA_IntendedUseCodeNHP = ZString.Empty;
			pgaHeader.CA_CategoryDSE = ZString.Empty;
			pgaHeader.CA_IntendedUseCodeDSE = ZString.Empty;

			pgaHeader.CA_NHPProgramInd = YesNoList.Codes.No;
			pgaHeader.CA_DSEProgramInd = YesNoList.Codes.No;
			using (((IPGAProgramRequirementProvider)pgaHeader).SuspendSettingDefaultValues())
			{
				pgaHeader.CA_NHPProgramInd = YesNoList.Codes.Yes;
				pgaHeader.CA_DSEProgramInd = YesNoList.Codes.Yes;
			}
			AssertEquals(ZString.Empty, pgaHeader.CA_CategoryNHP);
			AssertEquals(ZString.Empty, pgaHeader.CA_IntendedUseCodeNHP);
			AssertEquals(ZString.Empty, pgaHeader.CA_CategoryDSE);
			AssertEquals(ZString.Empty, pgaHeader.CA_IntendedUseCodeDSE);

			pgaHeader.CA_CategoryAPI = ZString.Empty;
			pgaHeader.CA_IntendedUseCodeAPI = ZString.Empty;
			using (((IPGAProgramRequirementProvider)pgaHeader).SuspendSettingDefaultValues())
			{
				pgaHeader.CA_APIProgramInd = YesNoList.Codes.Yes;
			}
			AssertEquals(ZString.Empty, pgaHeader.CA_IntendedUseCodeAPI);
			AssertEquals(ZString.Empty, pgaHeader.CA_CategoryAPI);
		}

		public void TestSetDefaultCA_Category()
		{
			var pgaHeader = Factory.New<HCPGAHeader>();
			pgaHeader.CA_HDRProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_IntendedUseCodeHDR = HCIntendedUseCode.Codes.HC02;

			AssertEquals(HCCategories.Codes.HC05, pgaHeader.CA_CategoryHDR);
		}

		public void TestThereIsNoExceptionWhenRequirementsParentIsNULL()
		{
			AssertNoExceptionThrown(() =>
			{
				var pgaHeader = Factory.New<HCPGAHeader>();
				_ = pgaHeader.OA_Manufacturer;
				pgaHeader.OA_Manufacturer = ZGuid.Empty;
				_ = pgaHeader.OA_ManufacturerInfo.SupportsMaxLength;
				_ = pgaHeader.OA_Manufacturer_ZAddress;
			});
		}

		#region AddDefaultLPCOs

		public void TestAddDefaultLPCOs_API()
		{
			var pgaHeader = Factory.New<HCPGAHeader>();
			pgaHeader.CA_APIProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_IntendedUseCodeAPI = HCIntendedUseCode.Codes.HC13;
			pgaHeader.CA_CategoryAPI = HCCategories.Codes.HC01;

			AssertDefaultLPCOs(new[] { "5001" }, pgaHeader);
		}

		public void TestAddDefaultLPCOs_CTO()
		{
			var pgaHeader = Factory.New<HCPGAHeader>();
			pgaHeader.CA_CTOProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_IntendedUseCodeCTO = HCIntendedUseCode.Codes.HC01;

			pgaHeader.CA_CategoryCTO = HCCategories.Codes.HC26;
			AssertDefaultLPCOs(new[] { "5004", "5005" }, pgaHeader);
		}

		public void TestAddDefaultLPCOs_CPR()
		{
			var pgaHeader = Factory.New<HCPGAHeader>();
			pgaHeader.CA_CPRProgramInd = YesNoList.Codes.Yes;

			pgaHeader.CA_IntendedUseCodeCPR = HCIntendedUseCode.Codes.HC21;
			pgaHeader.CA_CategoryCPR = HCCategories.Codes.HC29;
			AssertEquals(0, pgaHeader.LPCOViews.Count);

			pgaHeader.CA_IntendedUseCodeCPR = HCIntendedUseCode.Codes.HC22;
			pgaHeader.CA_CategoryCPR = HCCategories.Codes.HC37;
			AssertEquals(0, pgaHeader.LPCOViews.Count);

			pgaHeader.CA_IntendedUseCodeCPR = HCIntendedUseCode.Codes.HC21;
			pgaHeader.CA_CategoryCPR = HCCategories.Codes.HC36;
			AssertDefaultLPCOs(new[] { "5006" }, pgaHeader);
		}

		public void TestAddDefaultLPCOs_DSE()
		{
			var pgaHeader = Factory.New<HCPGAHeader>();
			pgaHeader.CA_DSEProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_CategoryDSE = HCCategories.Codes.HC04;
			pgaHeader.CA_IntendedUseCodeDSE = HCIntendedUseCode.Codes.HC01;
			AssertDefaultLPCOs(new[] { "5009" }, pgaHeader);
		}

		public void TestAddDefaultLPCOs_HDR()
		{
			var pgaHeader = Factory.New<HCPGAHeader>();
			pgaHeader.CA_HDRProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_IntendedUseCodeHDR = HCIntendedUseCode.Codes.HC01;
			pgaHeader.CA_CategoryHDR = HCCategories.Codes.HC05;
			AssertDefaultLPCOs(new[] { "5010", "5011" }, pgaHeader);
		}

		public void TestAddDefaultLPCOs_OCS()
		{
			var pgaHeader = Factory.New<HCPGAHeader>();
			pgaHeader.CA_OCSProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_IntendedUseCodeOCS = HCIntendedUseCode.Codes.HC01;
			pgaHeader.CA_CategoryOCS = HCCategories.Codes.HC18;

			AssertDefaultLPCOs(new[] { "5018" }, pgaHeader);
		}

		public void TestAddDefaultLPCOs_MDE()
		{
			var pgaHeader = Factory.New<HCPGAHeader>();
			pgaHeader.CA_MDEProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_IntendedUseCodeMDE = HCIntendedUseCode.Codes.HC01;
			pgaHeader.CA_CategoryMDE = HCCategories.Codes.HC11;

			AssertDefaultLPCOs(new[] { "5020" }, pgaHeader);
		}

		public void TestAddDefaultLPCOs_NHP()
		{
			var pgaHeader = Factory.New<HCPGAHeader>();
			pgaHeader.CA_NHPProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_IntendedUseCodeNHP = HCIntendedUseCode.Codes.HC01;
			pgaHeader.CA_CategoryNHP = HCCategories.Codes.HC15;

			AssertDefaultLPCOs(new[] { "5022" }, pgaHeader);
		}

		public void TestAddDefaultLPCOs_PES()
		{
			var pgaHeader = Factory.New<HCPGAHeader>();
			pgaHeader.CA_PESProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_IntendedUseCodePES = HCIntendedUseCode.Codes.HC06;
			pgaHeader.CA_CategoryPES = HCCategories.Codes.HC38;

			AssertDefaultLPCOs(new[] { "5026" }, pgaHeader);
		}

		public void TestAddDefaultLPCOs_RED()
		{
			var pgaHeader = Factory.New<HCPGAHeader>();
			pgaHeader.CA_REDProgramInd = YesNoList.Codes.Yes;

			AssertDefaultLPCOs(new[] { "5031" }, pgaHeader);
		}

		public void TestAddDefaultLPCOs_VET()
		{
			var pgaHeader = Factory.New<HCPGAHeader>();
			pgaHeader.CA_VETProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_IntendedUseCodeVET = HCIntendedUseCode.Codes.HC10;
			pgaHeader.CA_CategoryVET = HCCategories.Codes.HC16;

			AssertDefaultLPCOs(new[] { "5032", "5039" }, pgaHeader);
		}

		void AssertDefaultLPCOs(string[] expectedLPCOs, HCPGAHeader pgaHeader)
		{
			AssertContainsExactElementsInAnyOrder(expectedLPCOs, pgaHeader.LPCOViews.Cast<LPCOView>().Select(x => x.CLP_Type));
		}

		#endregion

		#region Purge Values

		public void TestPurgeValueValues()
		{
			var header = SetupPGAHeader();
			header.CA_APIProgramInd = YesNoList.Codes.No;
			Assert("API Category cleared when Program is disabled", header.CA_CategoryAPI.IsEmpty);
			Assert("API LPCO not cleared", header.LPCOViews.Any());
			Assert("API Components not cleared.", header.Components.Any());
			Assert("API IntendedUseCode cleared when Program is disabled", header.CA_IntendedUseCodeAPI.IsEmpty);
			Assert("API BatchLotNumber not cleared", !header.CA_BatchLotNumber.IsEmpty);
			Assert("API GTINNumber not cleared", !header.CA_GTINNumber.IsEmpty);

			header.CA_BBCProgramInd = YesNoList.Codes.No;
			Assert("BBC Category not cleared", header.CA_CategoryBBC.IsEmpty);
			Assert("BBC LPCO not cleared", header.LPCOViews.Any());
			Assert("BBC IntendedUseCode cleared when Program is disabled", header.CA_IntendedUseCodeBBC.IsEmpty);
			Assert("BBC GTINNumber not cleared", !header.CA_GTINNumber.IsEmpty);

			header.CA_CPRProgramInd = YesNoList.Codes.No;
			Assert("CPR Category cleared when Program is disabled", header.CA_CategoryCPR.IsEmpty);
			Assert("CPR LPCO not cleared", header.LPCOViews.Any());
			Assert("CPR IntendedUseCode cleared when Program is disabled", header.CA_IntendedUseCodeCPR.IsEmpty);
			Assert("CPR BatchLotNumber not cleared", !header.CA_BatchLotNumber.IsEmpty);
			Assert("CPR GTINNumber not cleared", !header.CA_GTINNumber.IsEmpty);

			header.CA_CTOProgramInd = YesNoList.Codes.No;
			Assert("CTO Category cleared when Program is disabled", header.CA_CategoryCTO.IsEmpty);
			Assert("CTO LPCO not cleared", header.LPCOViews.Any());
			Assert("CTO IntendedUseCode cleared when Program is disabled", header.CA_IntendedUseCodeCTO.IsEmpty);
			Assert("CTO GTINNumber not cleared", !header.CA_GTINNumber.IsEmpty);
			Assert("CTO ComplianceStatement not cleared", header.CA_ComplianceStatement);

			header.CA_DSEProgramInd = YesNoList.Codes.No;
			Assert("DSE Category cleared when Program is disabled", header.CA_CategoryDSE.IsEmpty);
			Assert("DSE LPCO not cleared", header.LPCOViews.Any());
			Assert("DSE IntendedUseCode cleared when Program is disabled", header.CA_IntendedUseCodeDSE.IsEmpty);
			Assert("DSE ComplianceStatement cleared when DSE is disabled", !header.CA_ComplianceStatement);

			header.CA_HDRProgramInd = YesNoList.Codes.No;
			Assert("HDR Category cleared when Program is disabled", header.CA_CategoryHDR.IsEmpty);
			Assert("HDR LPCO not cleared", header.LPCOViews.Any());
			Assert("HDR IntendedUseCode cleared when Program is disabled", header.CA_IntendedUseCodeHDR.IsEmpty);
			Assert("HDR BatchLotNumber not cleared", !header.CA_BatchLotNumber.IsEmpty);
			Assert("HDR GTINNumber not cleared", !header.CA_GTINNumber.IsEmpty);
			Assert("HDR UniqueDeviceIDNumber not cleared", !header.CA_UniqueDeviceIDNumber.IsEmpty);

			header.CA_MDEProgramInd = YesNoList.Codes.No;
			Assert("MDE Category cleared when Program is disabled", header.CA_CategoryMDE.IsEmpty);
			Assert("MDE LPCO not cleared", header.LPCOViews.Any());
			Assert("MDE IntendedUseCode cleared when Program is disabled", header.CA_IntendedUseCodeMDE.IsEmpty);
			Assert("MDE BatchLotNumber not cleared", !header.CA_BatchLotNumber.IsEmpty);
			Assert("MDE GTINNumber not cleared", !header.CA_GTINNumber.IsEmpty);
			Assert("MDE UniqueDeviceIDNumber cleared when MDE is disabled", header.CA_UniqueDeviceIDNumber.IsEmpty);

			header.CA_NHPProgramInd = YesNoList.Codes.No;
			Assert("NHP Category cleared when Program is disabled", header.CA_CategoryNHP.IsEmpty);
			Assert("NHP LPCO not cleared", header.LPCOViews.Any());
			Assert("NHP IntendedUseCode cleared when Program is disabled", header.CA_IntendedUseCodeNHP.IsEmpty);
			Assert("NHP BatchLotNumber not cleared", !header.CA_BatchLotNumber.IsEmpty);
			Assert("NHP GTINNumber not cleared", !header.CA_GTINNumber.IsEmpty);

			header.CA_OCSProgramInd = YesNoList.Codes.No;
			Assert("OCS Category cleared when Program is disabled", header.CA_CategoryOCS.IsEmpty);
			Assert("OCS LPCO not cleared", header.LPCOViews.Any());
			Assert("OCS Components not cleared.", header.Components.Any());
			Assert("OCS IntendedUseCode cleared when Program is disabled", header.CA_IntendedUseCodeOCS.IsEmpty);
			Assert("OCS BatchLotNumber not cleared", !header.CA_BatchLotNumber.IsEmpty);
			Assert("OCS CASNumber not cleared", !header.CA_CASNumber.IsEmpty);

			header.CA_PESProgramInd = YesNoList.Codes.No;
			Assert("PES Category cleared when Program is disabled", header.CA_CategoryPES.IsEmpty);
			Assert("PES LPCO not cleared", header.LPCOViews.Any());
			Assert("PES Components cleared when API, OCS and PES are disabled.", !header.Components.Any());
			Assert("PES IntendedUseCode cleared when Program is disabled", header.CA_IntendedUseCodePES.IsEmpty);
			Assert("PES BatchLotNumber not cleared", !header.CA_BatchLotNumber.IsEmpty);
			Assert("PES CASNumber cleared when PES is disabled", header.CA_CASNumber.IsEmpty);

			header.CA_VETProgramInd = YesNoList.Codes.No;
			Assert("VET LPCO not cleared", header.LPCOViews.Any());
			Assert("VET IntendedUseCode cleared  when Program is disabled", header.CA_IntendedUseCodeVET.IsEmpty);
			Assert("VET Category cleared  when Program is disabled", header.CA_CategoryVET.IsEmpty);
			Assert("VET BatchLotNumber cleared when API, CPR, HDR, OCS, MDE, NHP, PES and VET are disabled", header.CA_BatchLotNumber.IsEmpty);
			Assert("VET GTINNumber cleared when API, BBC, CTO, CPR, HDR, MDE, NHP and VET are disabled", header.CA_GTINNumber.IsEmpty);
			Assert("VET FDANumber not cleared", !header.CA_FDANumber.IsEmpty);

			header.CA_REDProgramInd = YesNoList.Codes.No;
			Assert("RED FDANumber cleared when RED is disabled", header.CA_FDANumber.IsEmpty);
			Assert("RED Category cleared when all program are disabled", header.CA_CategoryRED.IsEmpty);
			Assert("RED LPCO cleared when all program are disabled", !header.LPCOViews.Any());
		}

		HCPGAHeader SetupPGAHeader()
		{
			var header = (HCPGAHeader)GetNewBusinessObject();
			SetAllProgram(header, YesNoList.Codes.Yes);

			header.CA_CategoryAPI = "HC01";
			header.CA_IntendedUseCodeAPI = "HC02";
			header.CA_CategoryBBC = "HC01";
			header.CA_IntendedUseCodeBBC = "HC02";
			header.CA_CategoryCTO = "HC01";
			header.CA_IntendedUseCodeCTO = "HC02";
			header.CA_CategoryCPR = "HC01";
			header.CA_IntendedUseCodeCPR = "HC02";
			header.CA_CategoryDSE = "HC01";
			header.CA_IntendedUseCodeDSE = "HC02";
			header.CA_CategoryHDR = "HC01";
			header.CA_IntendedUseCodeHDR = "HC02";
			header.CA_CategoryNHP = "HC01";
			header.CA_IntendedUseCodeNHP = "HC02";
			header.CA_CategoryOCS = "HC01";
			header.CA_IntendedUseCodeOCS = "HC02";
			header.CA_CategoryMDE = "HC01";
			header.CA_IntendedUseCodeMDE = "HC02";
			header.CA_CategoryPES = "HC01";
			header.CA_IntendedUseCodePES = "HC02";
			header.CA_CategoryVET = "HC01";
			header.CA_IntendedUseCodeVET = "HC02";

			var lpco = header.LPCOViews.Any() ? (LPCOView)header.LPCOViews.First() : header.LPCOViews.AddNew();
			lpco.CLP_Type = "05";

			var component = header.Components.Any() ? (Component)header.Components.First() : header.Components.AddNew();
			component.CA_Type = "06";

			header.CA_BatchLotNumber = "Test BLOT";
			header.CA_GTINNumber = "Test GTIN";
			header.CA_ComplianceStatement = ZBool.True;
			header.CA_UniqueDeviceIDNumber = "001";
			header.CA_CASNumber = "CAS01";
			header.CA_FDANumber = "FDA01";
			header.CA_CTO_LCO = ZBool.True;
			header.CA_MDE_LEX = ZBool.True;
			header.CA_PES_SPCP = ZBool.True;
			header.CA_PES_EPCP = ZBool.True;

			return header;
		}

		void SetAllProgram(HCPGAHeader header, string value)
		{
			header.CA_APIProgramInd = value;
			header.CA_BBCProgramInd = value;
			header.CA_CPRProgramInd = value;
			header.CA_CTOProgramInd = value;
			header.CA_DSEProgramInd = value;
			header.CA_HDRProgramInd = value;
			header.CA_MDEProgramInd = value;
			header.CA_NHPProgramInd = value;
			header.CA_OCSProgramInd = value;
			header.CA_PESProgramInd = value;
			header.CA_REDProgramInd = value;
			header.CA_VETProgramInd = value;
		}

		#endregion

		public void TestLPCOViewCollection()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "5032", "5032 DESC", startDate, endDate);
			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributePGA = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.HC);
			newFactory.Save();

			AssertEquals("LpcoViews on HC", 0, header.LPCOViews.Count);
			AssertEquals(typeof(LPCOViewCollection), header.LPCOViews.GetType());

			var headerLPCO = declaration.LPCOViews.AddNew();
			headerLPCO.CLP_Type = "5032";
			header.LPCOViews.AddNew();
			AssertEquals("LpcoViews on HC", 2, header.LPCOViews.Count);

			var headerLPCO2 = declaration.LPCOViews.AddNew();
			headerLPCO2.CLP_Type = "5032";
			AssertEquals("LpcoViews on HC", 3, header.LPCOViews.Count);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_HCInd = "Y";
			header = invoiceLine.HCPGAHeader;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return header;
		}
		HCPGAHeader header;
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		protected override IEnumerable<HCPGAHeader> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_HCInd = "Y";
			yield return invoiceLine.HCPGAHeader;

			var product = factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = factory.New<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			pivot.CI_TariffNum = "1111.11.11";
			pivot.CCA_HCIndicator = YesNoList.Codes.Yes;
			yield return pivot.HCPGAHeader;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_HCInd = "Y";
			return invoiceLine.HCPGAHeader;
		}

		#endregion
	}
}
