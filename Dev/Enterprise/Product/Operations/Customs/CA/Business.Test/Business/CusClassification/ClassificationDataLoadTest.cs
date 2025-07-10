using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ClassificationDataLoad))]
	sealed class ClassificationDataLoadTest : DataLoadTestCase<ClassificationDataLoad>
	{
		public void TestPGAHasNoDataWhenClassificationIsNotHTS()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(headerFields + ",PGA_ECCC_ODSInd");
					sw.WriteLine("C1,EXP,Desc0,9999999999,13,04,9901,Auth,TRS,Y");
				}

				Runner.ImportData(testFileName.Filename, "row");
				var classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C1")).First();
				Assert(!classification.IsHTS);
				AssertEquals("CCA_ECCCIndicator", ZString.Empty, classification.CCA_ECCCIndicator);
			}
		}

		public void TestImportCFIA()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					var lpcos1 = "AAAA/11111111111111/22222222222222;BBBB/44444444444444;//";
					var regs1 = "CCC/55555555555555;6666666666;/";
					sw.WriteLine(headerFields + CFIAIndicators + CFIAFields);
					sw.WriteLine(string.Format("{0}", headerValues("C1")));
					sw.WriteLine(string.Format("{0},N", headerValues("C2")));
					sw.WriteLine(string.Format("{0},N,ABCDEF,,,987, 654,US,AA", headerValues("C3")));
					sw.WriteLine(string.Format("{0},Y,ABCDEF,,,987, 654,US,AA", headerValues("C4")));
					sw.WriteLine(string.Format("{0},Y,HIJKLM,{1},{2},123, 456,CA,BB", headerValues("C5"), lpcos1, regs1));
				}

				Runner.ImportData(testFileName.Filename, "row");
				var classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C1")).First();
				AssertEquals("PGA_CFIA_Indicator", ZString.Empty, classification.CCA_CFIAIndicator);
				var cfia = classification.CFIAPGAHeader;
				AssertNull(cfia);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C2")).First();
				AssertEquals("PGA_CFIA_Indicator", YesNoList.Codes.No, classification.CCA_CFIAIndicator);
				cfia = classification.CFIAPGAHeader;
				AssertEquals("CA_AllProgramInd", YesNoList.Codes.No, cfia.CA_AllProgramInd);
				AssertEquals("PGA_CFIA_AIRSExtensionCode", ZString.Empty, cfia.CA_AIRSExtensionCode);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C3")).First();
				AssertEquals("PGA_CFIA_Indicator", YesNoList.Codes.No, classification.CCA_CFIAIndicator);
				cfia = classification.CFIAPGAHeader;
				AssertEquals("CA_AllProgramInd", YesNoList.Codes.No, cfia.CA_AllProgramInd);
				AssertEquals("PGA_CFIA_AIRSExtensionCode", ZString.Empty, cfia.CA_AIRSExtensionCode);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C4")).First();
				AssertEquals("PGA_CFIA_Indicator", YesNoList.Codes.Yes, classification.CCA_CFIAIndicator);
				cfia = classification.CFIAPGAHeader;
				AssertEquals("CA_AllProgramInd", YesNoList.Codes.Yes, cfia.CA_AllProgramInd);
				AssertEquals("PGA_CFIA_AIRSExtensionCode", "ABCDEF", cfia.CA_AIRSExtensionCode);
				AssertEquals("PGA_CFIA_AIRSEndUse", "987", cfia.CA_AIRSEndUse);
				AssertEquals("PGA_CFIA_AIRSMiscellaneous", "654", cfia.CA_AIRSMiscellaneous);
				AssertEquals("PGA_CFIA_SourceCountry", "US", cfia.RN_NKCountryOfSource);
				AssertEquals("PGA_CFIA_SourceState", "AA", cfia.RW_NKSourceState);
				AssertEquals("PGA_CFIA_LPCOs", 0, cfia.LPCOViews.Count);
				AssertEquals("PGA_CFIA_AIRSRegistrations", 0, cfia.AIRSRegistrationNumbers.Count);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C5")).First();
				AssertEquals("PGA_CFIA_Indicator", YesNoList.Codes.Yes, classification.CCA_CFIAIndicator);
				cfia = classification.CFIAPGAHeader;
				AssertEquals("CA_AllProgramInd", YesNoList.Codes.Yes, cfia.CA_AllProgramInd);
				AssertEquals("PGA_CFIA_AIRSExtensionCode", "HIJKLM", cfia.CA_AIRSExtensionCode);
				AssertEquals("PGA_CFIA_AIRSEndUse", "123", cfia.CA_AIRSEndUse);
				AssertEquals("PGA_CFIA_AIRSMiscellaneous", "456", cfia.CA_AIRSMiscellaneous);
				AssertEquals("PGA_CFIA_SourceCountry", "CA", cfia.RN_NKCountryOfSource);
				AssertEquals("PGA_CFIA_SourceState", ZString.Empty, cfia.RW_NKSourceState);
				var lpcos = cfia.LPCOViews.Cast<LPCOView>().OrderBy(x => x.CLP_Type).ToList();
				AssertEquals("PGA_CFIA_LPCOs", 3, lpcos.Count);
				AssertEquals("LPCO1 CLP_Type", ZString.Empty, lpcos[0].CLP_Type);
				AssertEquals("LPCO1 CLP_RefNo", ZString.Empty, lpcos[0].CLP_RefNo);
				AssertEquals("LPCO1 CLP_DIFRefNumberOrLocation", ZString.Empty, lpcos[0].CLP_DIFRefNumberOrLocation);
				AssertEquals("LPCO2 CLP_Type", "AAAA", lpcos[1].CLP_Type);
				AssertEquals("LPCO2 CLP_RefNo", "11111111111111", lpcos[1].CLP_RefNo);
				AssertEquals("LPCO2 CLP_DIFRefNumberOrLocation", "22222222222222", lpcos[1].CLP_DIFRefNumberOrLocation);
				AssertEquals("LPCO3 CLP_Type", "BBBB", lpcos[2].CLP_Type);
				AssertEquals("LPCO3 CLP_RefNo", "44444444444444", lpcos[2].CLP_RefNo);
				AssertEquals("LPCO3 CLP_DIFRefNumberOrLocation", ZString.Empty, lpcos[2].CLP_DIFRefNumberOrLocation);
				var regs = cfia.AIRSRegistrationNumbers.Cast<AIRSRegistrationNumber>().OrderBy(x => x.CY_Code).ToList();
				AssertEquals("PGA_CFIA_AIRSRegistrations", 2, regs.Count);
				AssertEquals("Reg1 CY_Code", ZString.Empty, regs[0].CY_Code);
				AssertEquals("Reg1 CY_Data.", ZString.Empty, regs[0].CY_Data);
				AssertEquals("Reg2 CY_Code", "CCC", regs[1].CY_Code);
				AssertEquals("Reg2 CY_Data.", "55555555555555", regs[1].CY_Data);
			}
		}
		const string CFIAIndicators = ",PGA_CFIA_Indicator";
		const string CFIAFields = ",PGA_CFIA_AIRSExtensionCode,PGA_CFIA_LPCOs,PGA_CFIA_AIRSRegistrations,PGA_CFIA_AIRSEndUse,PGA_CFIA_AIRSMiscellaneous,PGA_CFIA_SourceCountry,PGA_CFIA_SourceState";

		public void TestImportGAC()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(headerFields + GACIndicators);
					sw.WriteLine(string.Format("{0}", headerValues("C1")));
					sw.WriteLine(string.Format("{0},N", headerValues("C2")));
					sw.WriteLine(string.Format("{0},Y", headerValues("C3")));
				}

				Runner.ImportData(testFileName.Filename, "row");
				var classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C1")).First();
				AssertEquals("PGA_GAC_Indicator", ZString.Empty, classification.CCA_GACIndicator);
				var gac = classification.GACPGAHeader;
				AssertNull(gac);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C2")).First();
				AssertEquals("PGA_GAC_Indicator", YesNoList.Codes.No, classification.CCA_GACIndicator);
				gac = classification.GACPGAHeader;
				AssertEquals("CA_AllProgramInd", YesNoList.Codes.No, gac.CA_AllProgramInd);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C3")).First();
				AssertEquals("PGA_GAC_Indicator", YesNoList.Codes.Yes, classification.CCA_GACIndicator);
				gac = classification.GACPGAHeader;
				AssertEquals("CA_AllProgramInd", YesNoList.Codes.Yes, gac.CA_AllProgramInd);
			}
		}
		const string GACIndicators = ",PGA_GAC_Indicator";

		public void TestImportDFO()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(headerFields + DFOIndicators);
					sw.WriteLine(string.Format("{0}", headerValues("C1")));
					sw.WriteLine(string.Format("{0},N,N,N", headerValues("C2")));
					sw.WriteLine(string.Format("{0},Y,N,N", headerValues("C3")));
					sw.WriteLine(string.Format("{0},N,Y,N", headerValues("C4")));
					sw.WriteLine(string.Format("{0},N,N,Y", headerValues("C5")));
					sw.WriteLine(string.Format("{0},Y,Y,Y", headerValues("C6")));
				}

				Runner.ImportData(testFileName.Filename, "row");
				var classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C1")).First();
				AssertEquals("CCA_DFOIndicator", ZString.Empty, classification.CCA_DFOIndicator);
				var dfo = classification.DFOPGAHeader;
				AssertNull(dfo);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C2")).First();
				AssertEquals("CCA_DFOIndicator", YesNoList.Codes.No, classification.CCA_DFOIndicator);
				dfo = classification.DFOPGAHeader;
				AssertEquals("PGA_DFO_ABIInd", YesNoList.Codes.No, dfo.CA_ABIProgramInd);
				AssertEquals("PGA_DFO_AISInd", YesNoList.Codes.No, dfo.CA_AISProgramInd);
				AssertEquals("PGA_DFO_TTPInd", YesNoList.Codes.No, dfo.CA_TTPProgramInd);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C3")).First();
				AssertEquals("CCA_DFOIndicator", YesNoList.Codes.Yes, classification.CCA_DFOIndicator);
				dfo = classification.DFOPGAHeader;
				AssertEquals("PGA_DFO_ABIInd", YesNoList.Codes.Yes, dfo.CA_ABIProgramInd);
				AssertEquals("PGA_DFO_AISInd", YesNoList.Codes.No, dfo.CA_AISProgramInd);
				AssertEquals("PGA_DFO_TTPInd", YesNoList.Codes.No, dfo.CA_TTPProgramInd);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C4")).First();
				AssertEquals("CCA_DFOIndicator", YesNoList.Codes.Yes, classification.CCA_DFOIndicator);
				dfo = classification.DFOPGAHeader;
				AssertEquals("PGA_DFO_ABIInd", YesNoList.Codes.No, dfo.CA_ABIProgramInd);
				AssertEquals("PGA_DFO_AISInd", YesNoList.Codes.Yes, dfo.CA_AISProgramInd);
				AssertEquals("PGA_DFO_TTPInd", YesNoList.Codes.No, dfo.CA_TTPProgramInd);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C5")).First();
				AssertEquals("CCA_DFOIndicator", YesNoList.Codes.Yes, classification.CCA_DFOIndicator);
				dfo = classification.DFOPGAHeader;
				AssertEquals("PGA_DFO_ABIInd", YesNoList.Codes.No, dfo.CA_ABIProgramInd);
				AssertEquals("PGA_DFO_AISInd", YesNoList.Codes.No, dfo.CA_AISProgramInd);
				AssertEquals("PGA_DFO_TTPInd", YesNoList.Codes.Yes, dfo.CA_TTPProgramInd);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C6")).First();
				AssertEquals("CCA_DFOIndicator", YesNoList.Codes.Yes, classification.CCA_DFOIndicator);
				dfo = classification.DFOPGAHeader;
				AssertEquals("PGA_DFO_ABIInd", YesNoList.Codes.Yes, dfo.CA_ABIProgramInd);
				AssertEquals("PGA_DFO_AISInd", YesNoList.Codes.Yes, dfo.CA_AISProgramInd);
				AssertEquals("PGA_DFO_TTPInd", YesNoList.Codes.Yes, dfo.CA_TTPProgramInd);
			}
		}
		const string DFOIndicators = ",PGA_DFO_ABIInd,PGA_DFO_AISInd,PGA_DFO_TTPInd";

		public void TestImportECCCIndicators()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(headerFields + ECCCIndicators);
					sw.WriteLine(string.Format("{0}", headerValues("C1")));
					sw.WriteLine(string.Format("{0},N,N,N,N", headerValues("C2")));
					sw.WriteLine(string.Format("{0},Y,N,N,N", headerValues("C3")));
					sw.WriteLine(string.Format("{0},N,Y,N,N", headerValues("C4")));
					sw.WriteLine(string.Format("{0},N,N,Y,N", headerValues("C5")));
					sw.WriteLine(string.Format("{0},N,N,N,Y", headerValues("C6")));
					sw.WriteLine(string.Format("{0},Y,Y,Y,Y", headerValues("C7")));
				}

				Runner.ImportData(testFileName.Filename, "row");
				var classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C1")).First();
				AssertEquals("CCA_ECCCIndicator", ZString.Empty, classification.CCA_ECCCIndicator);
				var eccc = classification.ECCCPGAHeader;
				AssertNull(eccc);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C2")).First();
				AssertEquals("CCA_ECCCIndicator", YesNoList.Codes.No, classification.CCA_ECCCIndicator);
				eccc = classification.ECCCPGAHeader;
				AssertEquals("PGA_ECCC_WRMInd", YesNoList.Codes.No, eccc.CA_WRMProgramInd);
				AssertEquals("PGA_ECCC_ODSInd", YesNoList.Codes.No, eccc.CA_ODSProgramInd);
				AssertEquals("PGA_ECCC_WENInd", YesNoList.Codes.No, eccc.CA_WENProgramInd);
				AssertEquals("PGA_ECCC_VEEInd", YesNoList.Codes.No, eccc.CA_VEEProgramInd);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C3")).First();
				AssertEquals("CCA_ECCCIndicator", YesNoList.Codes.Yes, classification.CCA_ECCCIndicator);
				eccc = classification.ECCCPGAHeader;
				AssertEquals("PGA_ECCC_WRMInd", YesNoList.Codes.Yes, eccc.CA_WRMProgramInd);
				AssertEquals("PGA_ECCC_ODSInd", YesNoList.Codes.No, eccc.CA_ODSProgramInd);
				AssertEquals("PGA_ECCC_WENInd", YesNoList.Codes.No, eccc.CA_WENProgramInd);
				AssertEquals("PGA_ECCC_VEEInd", YesNoList.Codes.No, eccc.CA_VEEProgramInd);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C4")).First();
				AssertEquals("CCA_ECCCIndicator", YesNoList.Codes.Yes, classification.CCA_ECCCIndicator);
				eccc = classification.ECCCPGAHeader;
				AssertEquals("PGA_ECCC_WRMInd", YesNoList.Codes.No, eccc.CA_WRMProgramInd);
				AssertEquals("PGA_ECCC_ODSInd", YesNoList.Codes.Yes, eccc.CA_ODSProgramInd);
				AssertEquals("PGA_ECCC_WENInd", YesNoList.Codes.No, eccc.CA_WENProgramInd);
				AssertEquals("PGA_ECCC_VEEInd", YesNoList.Codes.No, eccc.CA_VEEProgramInd);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C5")).First();
				AssertEquals("CCA_ECCCIndicator", YesNoList.Codes.Yes, classification.CCA_ECCCIndicator);
				eccc = classification.ECCCPGAHeader;
				AssertEquals("PGA_ECCC_WRMInd", YesNoList.Codes.No, eccc.CA_WRMProgramInd);
				AssertEquals("PGA_ECCC_ODSInd", YesNoList.Codes.No, eccc.CA_ODSProgramInd);
				AssertEquals("PGA_ECCC_WENInd", YesNoList.Codes.Yes, eccc.CA_WENProgramInd);
				AssertEquals("PGA_ECCC_VEEInd", YesNoList.Codes.No, eccc.CA_VEEProgramInd);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C6")).First();
				AssertEquals("CCA_ECCCIndicator", YesNoList.Codes.Yes, classification.CCA_ECCCIndicator);
				eccc = classification.ECCCPGAHeader;
				AssertEquals("PGA_ECCC_WRMInd", YesNoList.Codes.No, eccc.CA_WRMProgramInd);
				AssertEquals("PGA_ECCC_ODSInd", YesNoList.Codes.No, eccc.CA_ODSProgramInd);
				AssertEquals("PGA_ECCC_WENInd", YesNoList.Codes.No, eccc.CA_WENProgramInd);
				AssertEquals("PGA_ECCC_VEEInd", YesNoList.Codes.Yes, eccc.CA_VEEProgramInd);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C7")).First();
				AssertEquals("CCA_ECCCIndicator", YesNoList.Codes.Yes, classification.CCA_ECCCIndicator);
				eccc = classification.ECCCPGAHeader;
				AssertEquals("PGA_ECCC_WRMInd", YesNoList.Codes.Yes, eccc.CA_WRMProgramInd);
				AssertEquals("PGA_ECCC_ODSInd", YesNoList.Codes.Yes, eccc.CA_ODSProgramInd);
				AssertEquals("PGA_ECCC_WENInd", YesNoList.Codes.Yes, eccc.CA_WENProgramInd);
				AssertEquals("PGA_ECCC_VEEInd", YesNoList.Codes.Yes, eccc.CA_VEEProgramInd);
			}
		}
		const string ECCCIndicators = ",PGA_ECCC_WRMInd,PGA_ECCC_ODSInd,PGA_ECCC_WENInd,PGA_ECCC_VEEInd";

		public void TestImportECCCSharedFields()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG01";
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					var lpcos1 = "AAA/111/222/333/M;BBB/444//6/M;DDD";
					sw.WriteLine(headerFields + ",Manufacturer,PGA_ECCC_WRMInd" + ECCCSharedFields);
					sw.WriteLine(string.Format("{0},ORG01,Y,IUC01,{1}", headerValues("C1"), lpcos1));
				}

				Runner.ImportData(testFileName.Filename, "row");
				var classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C1")).First();
				AssertEquals("CCA_ECCCIndicator", YesNoList.Codes.Yes, classification.CCA_ECCCIndicator);
				AssertEquals("Manufacturer", org.MainAddress.PK, classification.CCA_OA_Manufacturer);
				var eccc = classification.ECCCPGAHeader;
				AssertEquals("PGA_ECCC_WRMInd", YesNoList.Codes.Yes, eccc.CA_WRMProgramInd);
				AssertEquals("PGA_ECCC_IntendedUseCode", "IUC01", eccc.CA_IntendedUseCode);
				var lpcos = eccc.LPCOViews.Cast<LPCOView>().OrderBy(x => x.CLP_Type).ToList();
				AssertEquals("PGA_ECCC_LPCOs", 4, lpcos.Count);
				AssertEquals("LPCO1 CLP_Type", "8000", lpcos[0].CLP_Type);
				AssertEquals("LPCO2 CLP_Type", "8001", lpcos[1].CLP_Type);
				AssertEquals("LPCO3 CLP_Type", "AAA", lpcos[2].CLP_Type);
				AssertEquals("LPCO3 CLP_RefNo", "111", lpcos[2].CLP_RefNo);
				AssertEquals("LPCO3 CLP_DIFRefNumberOrLocation", "222", lpcos[2].CLP_DIFRefNumberOrLocation);
				AssertEquals("LPCO3 CLP_AlternativeQuotaQuantity", 333m, lpcos[2].CLP_AlternativeQuotaQuantity);
				AssertEquals("LPCO3 CLP_AlternativeQuotaUQ", "M", lpcos[2].CLP_AlternativeQuotaUQ);
				AssertEquals("LPCO4 CLP_Type", "BBB", lpcos[3].CLP_Type);
				AssertEquals("LPCO4 CLP_RefNo", "444", lpcos[3].CLP_RefNo);
				AssertEquals("LPCO4 CLP_DIFRefNumberOrLocation", ZString.Empty, lpcos[3].CLP_DIFRefNumberOrLocation);
				AssertEquals("LPCO4 CLP_AlternativeQuotaQuantity", 6m, lpcos[3].CLP_AlternativeQuotaQuantity);
				AssertEquals("LPCO4 CLP_AlternativeQuotaUQ", "M", lpcos[3].CLP_AlternativeQuotaUQ);
			}
		}
		const string ECCCSharedFields = ",PGA_ECCC_IntendedUseCode,PGA_ECCC_LPCOs";

		public void TestImportECCC_ODS()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(headerFields + ",PGA_ECCC_ODSInd" + ECCC_ODSField);
					sw.WriteLine(string.Format("{0},Y,123", headerValues("C1")));
				}

				Runner.ImportData(testFileName.Filename, "row");
				var classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C1")).First();
				AssertEquals("CCA_ECCCIndicator", YesNoList.Codes.Yes, classification.CCA_ECCCIndicator);
				var eccc = classification.ECCCPGAHeader;
				AssertEquals("PGA_ECCC_ODSInd", YesNoList.Codes.Yes, eccc.CA_ODSProgramInd);
				AssertEquals("PGA_ECCC_CASNumber", "123", eccc.CA_CASNumber);
			}
		}
		const string ECCC_ODSField = ",PGA_ECCC_CASNumber";

		public void TestImportECCC_WEN()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(headerFields + ",PGA_ECCC_WENInd" + ECCC_WENField);
					sw.WriteLine(string.Format("{0},Y,SPEC,LS01,12,MALE,Y,SCN01,TSN01,APID01,AA/BB;CC/DD;EE", headerValues("C1")));
				}

				Runner.ImportData(testFileName.Filename, "row");
				var classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C1")).First();
				AssertEquals("CCA_ECCCIndicator", YesNoList.Codes.Yes, classification.CCA_ECCCIndicator);
				var eccc = classification.ECCCPGAHeader;
				AssertEquals("PGA_ECCC_WENInd", YesNoList.Codes.Yes, eccc.CA_WENProgramInd);
				AssertEquals("PGA_ECCC_SourceOfSpecimen", "SPEC", eccc.CA_SourceOfSpecimen);
				AssertEquals("PGA_ECCC_LifeStage", "LS01", eccc.CA_LifeStage);
				AssertEquals("PGA_ECCC_Age", new ZInt(12), eccc.CA_Age);
				AssertEquals("PGA_ECCC_Sex", "MALE", eccc.CA_Sex);
				AssertEquals("PGA_ECCC_Regulated", true, eccc.CA_ComplianceDeclaration);
				AssertEquals("PGA_ECCC_ScientificName", "SCN01", eccc.CA_ScientificName);
				AssertEquals("PGA_ECCC_TSN", "TSN01", eccc.CA_TSN);
				AssertEquals("PGA_ECCC_AphiaID", "APID01", eccc.CA_AphiaID);
				var components = eccc.Components.Cast<Component>().OrderBy(x => x.CA_Type).ToList();
				AssertEquals("Components Count", 2, components.Count);
				AssertEquals("Component1 CA_Type", "AA", components[0].CA_Type);
				AssertEquals("Component1 CA_Name", "BB", components[0].CA_Name);
				AssertEquals("Component2 CA_Type", "CC", components[1].CA_Type);
				AssertEquals("Component2 CA_Name", "DD", components[1].CA_Name);
			}
		}
		const string ECCC_WENField = ",PGA_ECCC_SourceOfSpecimen,PGA_ECCC_LifeStage,PGA_ECCC_Age,PGA_ECCC_Sex,PGA_ECCC_Regulated,PGA_ECCC_ScientificName,PGA_ECCC_TSN,PGA_ECCC_AphiaID,PGA_ECCC_Identities";

		public void TestImportECCC_VEE()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG02";
			org1.MainAddress.Address1 = "TEST Address 2";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "EGL03";
			org2.MainAddress.Address1 = "TEST Address 3";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "ECL04";
			org3.MainAddress.Address1 = "TEST Address 4";
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(headerFields + ",PGA_ECCC_VEEInd" + ECCC_VEEField);
					sw.WriteLine(string.Format("{0},Y,EC01,Y,Y,Y,Y,Y,Y,VC01,ECL1,EM01,EMD01,2020,EIDN01,EMF01,EFN01,ETG01,EEF01,12,M,MCM01,MCMD01,2019,ORG02,EGL03,ECL04", headerValues("C1")));
					sw.WriteLine(string.Format("{0},Y,EC02,Y,Y,Y,Y,Y,Y,VC01,ECL1,EM01,EMD01,2020,EIDN01,EMF01,EFN01,ETG01,EEF01,12,M,MCM01,MCMD01,2019,ORG02,EGL03,ECL04", headerValues("C2")));
					sw.WriteLine(string.Format("{0},Y,EC03,Y,Y,Y,Y,Y,Y,VC01,ECL1,EM01,EMD01,2020,EIDN01,EMF01,EFN01,ETG01,EEF01,12,M,MCM01,MCMD01,2019,ORG02,EGL03,ECL04", headerValues("C3")));
					sw.WriteLine(string.Format("{0},Y,EC04,Y,Y,Y,Y,Y,Y,VC01,ECL1,EM01,EMD01,2020,EIDN01,EMF01,EFN01,ETG01,EEF01,12,M,MCM01,MCMD01,2019,ORG02,EGL03,ECL04", headerValues("C4")));
				}

				Runner.ImportData(testFileName.Filename, "row");
				var classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C1")).First();
				AssertEquals("CCA_ECCCIndicator", YesNoList.Codes.Yes, classification.CCA_ECCCIndicator);
				var eccc = classification.ECCCPGAHeader;
				AssertEquals("PGA_ECCC_VEEInd", YesNoList.Codes.Yes, eccc.CA_VEEProgramInd);
				AssertECCC(eccc, "EC01", true, true, false, true, true, true, "VC01", "ECL1", "EM01", "EMD01", "2020", "EIDN01", "EMF01", "EFN01", "ETG01", ZString.Empty, 0m, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C2")).First();
				AssertEquals("CCA_ECCCIndicator", YesNoList.Codes.Yes, classification.CCA_ECCCIndicator);
				eccc = classification.ECCCPGAHeader;
				AssertEquals("PGA_ECCC_VEEInd", YesNoList.Codes.Yes, eccc.CA_VEEProgramInd);
				AssertECCC(eccc, "EC02", true, true, true, true, true, true, ZString.Empty, "ECL1", "EM01", "EMD01", "2020", "EIDN01", "EMF01", "EFN01", ZString.Empty, ZString.Empty, 12m, "M", "MCM01", "MCMD01", "2019", org1.MainAddress.PK, org2.MainAddress.PK, org3.MainAddress.PK);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C3")).First();
				AssertEquals("CCA_ECCCIndicator", YesNoList.Codes.Yes, classification.CCA_ECCCIndicator);
				eccc = classification.ECCCPGAHeader;
				AssertEquals("PGA_ECCC_VEEInd", YesNoList.Codes.Yes, eccc.CA_VEEProgramInd);
				AssertECCC(eccc, "EC03", true, true, false, true, true, true, ZString.Empty, "ECL1", "EM01", "EMD01", "2020", "EIDN01", "EMF01", "EFN01", ZString.Empty, ZString.Empty, 12m, "M", "MCM01", "MCMD01", ZString.Empty, org1.MainAddress.PK, ZGuid.Empty, ZGuid.Empty);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C4")).First();
				AssertEquals("CCA_ECCCIndicator", YesNoList.Codes.Yes, classification.CCA_ECCCIndicator);
				eccc = classification.ECCCPGAHeader;
				AssertEquals("PGA_ECCC_VEEInd", YesNoList.Codes.Yes, eccc.CA_VEEProgramInd);
				AssertECCC(eccc, "EC04", true, true, false, true, true, true, "VC01", "ECL1", "EM01", "EMD01", "2020", "EIDN01", "EMF01", "EFN01", ZString.Empty, "EEF01", 0m, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, org1.MainAddress.PK, ZGuid.Empty, ZGuid.Empty);
			}
		}
		const string ECCC_VEEField = ",PGA_ECCC_ProcessCode,PGA_ECCC_NationalMark,PGA_ECCC_EPACertified,PGA_ECCC_Transition,PGA_ECCC_Incomplete,PGA_ECCC_CanadaUnique,PGA_ECCC_BulkReporting"
			+ ",PGA_ECCC_VehicleClass"
			+ ",PGA_ECCC_EngineClass,PGA_ECCC_EngineMake,PGA_ECCC_EngineModel,PGA_ECCC_EngineModelYear,PGA_ECCC_EngineIDNumber,PGA_ECCC_EngineManufacturer,PGA_ECCC_EngineFamilyName,PGA_ECCC_EngineTestGroup"
			+ ",PGA_ECCC_EngineEvaporativeFamily,PGA_ECCC_EnginePowerRating,PGA_ECCC_EnginePowerRatingUQ"
			+ ",PGA_ECCC_MachineMake,PGA_ECCC_MachineModel,PGA_ECCC_MachineModelYear,PGA_ECCC_MachineManufacturer,PGA_ECCC_EngineLocation,PGA_ECCC_EvidenceOfConfirmityLocation";

		void AssertECCC(ECCCPGAHeader eccc, ZString processCode, ZBool nationalMark, ZBool epaCertified, ZBool transition, ZBool incomplete, ZBool canadaUnique, ZBool bulkReporting, ZString vehicleClass,
			ZString engineClass, ZString makeOfEngine, ZString modelOfEngine, ZString engineModelYear, ZString engineIDNumber, ZString engineManufacturer, ZString engineFamilyName, ZString testGroupName,
			ZString evaporativeFamily, ZDecimal enginePowerRating, ZString powerRatingUQ, ZString makeOfMachine, ZString modelOfMachine, ZString machineModelYear, ZGuid machineManufacturer, ZGuid engineLocation,
			ZGuid evidenceOfConformityLocation)
		{
			AssertEquals("PGA_ECCC_ProcessCode", processCode, eccc.CA_ProcessCode);
			AssertEquals("PGA_ECCC_NationalMark", nationalMark, eccc.CA_NationalMark);
			AssertEquals("PGA_ECCC_EPACertified", epaCertified, eccc.CA_EPACertified);
			AssertEquals("PGA_ECCC_Transition", transition, eccc.CA_Transition);
			AssertEquals("PGA_ECCC_Incomplete", incomplete, eccc.CA_Incomplete);
			AssertEquals("PGA_ECCC_CanadaUnique", canadaUnique, eccc.CA_CanadaUnique);
			AssertEquals("PGA_ECCC_BulkReporting", bulkReporting, eccc.CA_BulkReporting);
			AssertEquals("PGA_ECCC_VehicleClass", vehicleClass, eccc.CA_VehicleClass);

			AssertEquals("PGA_ECCC_EngineClass", engineClass, eccc.CA_EngineClass);
			AssertEquals("PGA_ECCC_EngineMake", makeOfEngine, eccc.CA_MakeOfEngine);
			AssertEquals("PGA_ECCC_EngineModel", modelOfEngine, eccc.CA_ModelOfEngine);
			AssertEquals("PGA_ECCC_EngineModelYear", engineModelYear, eccc.CA_EngineModelYear);
			AssertEquals("PGA_ECCC_EngineIDNumber", engineIDNumber, eccc.CA_EngineIDNumber);
			AssertEquals("PGA_ECCC_EngineManufacturer", engineManufacturer, eccc.CA_EngineManufacturer);
			AssertEquals("PGA_ECCC_EngineFamilyName", engineFamilyName, eccc.CA_EngineFamilyName);
			AssertEquals("PGA_ECCC_EngineTestGroup", testGroupName, eccc.CA_TestGroupName);

			AssertEquals("PGA_ECCC_EngineEvaporativeFamily", evaporativeFamily, eccc.CA_EvaporativeFamily);
			AssertEquals("PGA_ECCC_EnginePowerRating", enginePowerRating, eccc.CA_EnginePowerRating);
			AssertEquals("PGA_ECCC_EnginePowerRatingUQ", powerRatingUQ, eccc.CA_PowerRatingUQ);
			AssertEquals("PGA_ECCC_MachineMake", makeOfMachine, eccc.CA_MakeOfMachine);
			AssertEquals("PGA_ECCC_MachineModel", modelOfMachine, eccc.CA_ModelOfMachine);
			AssertEquals("PGA_ECCC_MachineModelYear", machineModelYear, eccc.CA_MachineModelYear);
			AssertEquals("PGA_ECCC_MachineManufacturer", machineManufacturer, eccc.CA_MachineManufacturer);
			AssertEquals("PGA_ECCC_EngineLocation", engineLocation, eccc.CA_OA_EngineLocation);

			AssertEquals("PGA_ECCC_EvidenceOfConfirmityLocation", evidenceOfConformityLocation, eccc.CA_OA_EvidenceOfConformityLocation);
		}

		public void TestImportHC()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(headerFields + HCAPIFields + HCBBCFields + HCCPRFields + HCCTOFields + HCDSEFields + HCHDRFields + HCMDEFields + HCNHPFields + HCOCSFields + HCPESFields + HCREDFields + HCVETFields + HCSharedFields);
					sw.WriteLine(string.Format("{0},,API1,API2,,BBC1,BBC2,,CPR1,CPR2,,CTO1,CTO2,,DSE1,DSE2,,HDR1,HDR2,,MDE1,MDE2,,NHP1,NHP2,,OCS1,OCS2,,PES1,PES2,,RED1,RED2,,VET1,VET2,1,2,Y,Y,5,Y,7,Y,Y,10", headerValues("C1")));
					sw.WriteLine(string.Format("{0},N,API1,API2,N,BBC1,BBC2,N,CPR1,CPR2,N,CTO1,CTO2,N,DSE1,DSE2,N,HDR1,HDR2,N,MDE1,MDE2,N,NHP1,NHP2,N,OCS1,OCS2,N,PES1,PES2,N,RED1,RED2,N,VET1,VET2,1,2,Y,Y,5,Y,7,Y,Y,10,AAA/111/222;BBB/333/444", headerValues("C2")));
					sw.WriteLine(string.Format("{0},Y,API1,API2,N,BBC1,BBC2,N,CPR1,CPR2,N,CTO1,CTO2,N,DSE1,DSE2,N,HDR1,HDR2,N,MDE1,MDE2,N,NHP1,NHP2,N,OCS1,OCS2,N,PES1,PES2,N,RED1,RED2,N,VET1,VET2,1,2,Y,Y,5,Y,7,Y,Y,10,AAA/111/222;BBB/333/444", headerValues("C3")));
					sw.WriteLine(string.Format("{0},N,API1,API2,Y,BBC1,BBC2,N,CPR1,CPR2,N,CTO1,CTO2,N,DSE1,DSE2,N,HDR1,HDR2,N,MDE1,MDE2,N,NHP1,NHP2,N,OCS1,OCS2,N,PES1,PES2,N,RED1,RED2,N,VET1,VET2,1,2,Y,Y,5,Y,7,Y,Y,10,AAA/111/222;BBB/333/444", headerValues("C4")));
					sw.WriteLine(string.Format("{0},N,API1,API2,N,BBC1,BBC2,Y,CPR1,CPR2,N,CTO1,CTO2,N,DSE1,DSE2,N,HDR1,HDR2,N,MDE1,MDE2,N,NHP1,NHP2,N,OCS1,OCS2,N,PES1,PES2,N,RED1,RED2,N,VET1,VET2,1,2,Y,Y,5,Y,7,Y,Y,10,AAA/111/222;BBB/333/444", headerValues("C5")));
					sw.WriteLine(string.Format("{0},N,API1,API2,N,BBC1,BBC2,N,CPR1,CPR2,Y,CTO1,CTO2,N,DSE1,DSE2,N,HDR1,HDR2,N,MDE1,MDE2,N,NHP1,NHP2,N,OCS1,OCS2,N,PES1,PES2,N,RED1,RED2,N,VET1,VET2,1,2,Y,Y,5,Y,7,Y,Y,10,AAA/111/222;BBB/333/444", headerValues("C6")));
					sw.WriteLine(string.Format("{0},N,API1,API2,N,BBC1,BBC2,N,CPR1,CPR2,N,CTO1,CTO2,Y,DSE1,DSE2,N,HDR1,HDR2,N,MDE1,MDE2,N,NHP1,NHP2,N,OCS1,OCS2,N,PES1,PES2,N,RED1,RED2,N,VET1,VET2,1,2,Y,Y,5,Y,7,Y,Y,10,AAA/111/222;BBB/333/444", headerValues("C7")));
					sw.WriteLine(string.Format("{0},N,API1,API2,N,BBC1,BBC2,N,CPR1,CPR2,N,CTO1,CTO2,N,DSE1,DSE2,Y,HDR1,HDR2,N,MDE1,MDE2,N,NHP1,NHP2,N,OCS1,OCS2,N,PES1,PES2,N,RED1,RED2,N,VET1,VET2,1,2,Y,Y,5,Y,7,Y,Y,10,AAA/111/222;BBB/333/444", headerValues("C8")));
					sw.WriteLine(string.Format("{0},N,API1,API2,N,BBC1,BBC2,N,CPR1,CPR2,N,CTO1,CTO2,N,DSE1,DSE2,N,HDR1,HDR2,Y,MDE1,MDE2,N,NHP1,NHP2,N,OCS1,OCS2,N,PES1,PES2,N,RED1,RED2,N,VET1,VET2,1,2,Y,Y,5,Y,7,Y,Y,10,AAA/111/222;BBB/333/444", headerValues("C9")));
					sw.WriteLine(string.Format("{0},N,API1,API2,N,BBC1,BBC2,N,CPR1,CPR2,N,CTO1,CTO2,N,DSE1,DSE2,N,HDR1,HDR2,N,MDE1,MDE2,Y,NHP1,NHP2,N,OCS1,OCS2,N,PES1,PES2,N,RED1,RED2,N,VET1,VET2,1,2,Y,Y,5,Y,7,Y,Y,10,AAA/111/222;BBB/333/444", headerValues("C10")));
					sw.WriteLine(string.Format("{0},N,API1,API2,N,BBC1,BBC2,N,CPR1,CPR2,N,CTO1,CTO2,N,DSE1,DSE2,N,HDR1,HDR2,N,MDE1,MDE2,N,NHP1,NHP2,Y,OCS1,OCS2,N,PES1,PES2,N,RED1,RED2,N,VET1,VET2,1,2,Y,Y,5,Y,7,Y,Y,10,AAA/111/222;BBB/333/444", headerValues("C11")));
					sw.WriteLine(string.Format("{0},N,API1,API2,N,BBC1,BBC2,N,CPR1,CPR2,N,CTO1,CTO2,N,DSE1,DSE2,N,HDR1,HDR2,N,MDE1,MDE2,N,NHP1,NHP2,N,OCS1,OCS2,Y,PES1,PES2,N,RED1,RED2,N,VET1,VET2,1,2,Y,Y,5,Y,7,Y,Y,10,AAA/111/222;BBB/333/444", headerValues("C12")));
					sw.WriteLine(string.Format("{0},N,API1,API2,N,BBC1,BBC2,N,CPR1,CPR2,N,CTO1,CTO2,N,DSE1,DSE2,N,HDR1,HDR2,N,MDE1,MDE2,N,NHP1,NHP2,N,OCS1,OCS2,N,PES1,PES2,Y,RED1,RED2,N,VET1,VET2,1,2,Y,Y,5,Y,7,Y,Y,10,AAA/111/222;BBB/333/444", headerValues("C13")));
					sw.WriteLine(string.Format("{0},N,API1,API2,N,BBC1,BBC2,N,CPR1,CPR2,N,CTO1,CTO2,N,DSE1,DSE2,N,HDR1,HDR2,N,MDE1,MDE2,N,NHP1,NHP2,N,OCS1,OCS2,N,PES1,PES2,N,RED1,RED2,Y,VET1,VET2,1,2,Y,Y,5,Y,7,Y,Y,10,AAA/111/222;BBB/333/444", headerValues("C14")));
					sw.WriteLine(string.Format("{0},Y,API1,API2,Y,BBC1,BBC2,Y,CPR1,CPR2,Y,CTO1,CTO2,Y,DSE1,DSE2,Y,HDR1,HDR2,Y,MDE1,MDE2,Y,NHP1,NHP2,Y,OCS1,OCS2,Y,PES1,PES2,Y,RED1,RED2,Y,VET1,VET2,1,2,Y,Y,5,Y,7,Y,Y,10,AAA/111/222;BBB/333/444", headerValues("C15")));
				}

				Runner.ImportData(testFileName.Filename, "row");
				AssertHC("C1", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, false, false, ZString.Empty, false, ZString.Empty, false, false, ZString.Empty, false, false);
				AssertHC("C2", "N", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, false, false, ZString.Empty, false, ZString.Empty, false, false, ZString.Empty, false, false);
				AssertHC("C3", "Y", "Y", "API1", "API2", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "1", "2", false, false, ZString.Empty, false, ZString.Empty, false, false, ZString.Empty, true, false);
				AssertHC("C4", "Y", "N", ZString.Empty, ZString.Empty, "Y", "BBC1", "BBC2", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "1", ZString.Empty, false, false, ZString.Empty, false, ZString.Empty, false, false, ZString.Empty, true, true);
				AssertHC("C5", "Y", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "Y", "CPR1", "CPR2", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "1", "2", false, false, ZString.Empty, false, ZString.Empty, false, false, ZString.Empty, true, true);
				AssertHC("C6", "Y", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "Y", "CTO1", "CTO2", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "1", ZString.Empty, true, false, ZString.Empty, false, ZString.Empty, false, false, ZString.Empty, true, true);
				AssertHC("C7", "Y", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "Y", "DSE1", "DSE2", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, false, true, ZString.Empty, false, ZString.Empty, false, false, ZString.Empty, true, true);
				AssertHC("C8", "Y", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "Y", "HDR1", "HDR2", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "1", "2", false, false, ZString.Empty, false, ZString.Empty, false, false, ZString.Empty, true, true);
				AssertHC("C9", "Y", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "Y", "MDE1", "MDE2", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "1", "2", false, false, "5", true, ZString.Empty, false, false, ZString.Empty, true, true);
				AssertHC("C10", "Y", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "Y", "NHP1", "NHP2", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "1", "2", false, false, ZString.Empty, false, ZString.Empty, false, false, ZString.Empty, true, true);
				AssertHC("C11", "Y", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "Y", "OCS1", "OCS2", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, ZString.Empty, "2", false, false, ZString.Empty, false, ZString.Empty, false, false, ZString.Empty, true, true);
				AssertHC("C12", "Y", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "Y", "PES1", "PES2", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, ZString.Empty, "2", false, false, ZString.Empty, false, "7", true, true, ZString.Empty, true, true);
				AssertHC("C13", "Y", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "Y", "RED1", "RED2", "N", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, false, false, ZString.Empty, false, ZString.Empty, false, false, "10", true, true);
				AssertHC("C14", "Y", "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty, "Y", "VET1", "VET2", "1", "2", false, false, ZString.Empty, false, ZString.Empty, false, false, ZString.Empty, true, true);
				AssertHC("C15", "Y", "Y", "API1", "API2", "Y", "BBC1", "BBC2", "Y", "CPR1", "CPR2", "Y", "CTO1", "CTO2", "Y", "DSE1", "DSE2", "Y", "HDR1", "HDR2", "Y", "MDE1", "MDE2", "Y", "NHP1", "NHP2", "Y", "OCS1", "OCS2", "Y", "PES1", "PES2", "Y", "RED1", "RED2", "Y", "VET1", "VET2", "1", "2", false, true, "5", false, "7", true, true, "10", true, true);
			}
		}
		const string HCAPIFields = ",PGA_HC_APIInd,PGA_HC_IntendedUseCodeAPI,PGA_HC_CommodityTypeAPI";
		const string HCBBCFields = ",PGA_HC_BBCInd,PGA_HC_IntendedUseCodeBBC,PGA_HC_CommodityTypeBBC";
		const string HCCPRFields = ",PGA_HC_CPRInd,PGA_HC_IntendedUseCodeCPR,PGA_HC_CommodityTypeCPR";
		const string HCCTOFields = ",PGA_HC_CTOInd,PGA_HC_IntendedUseCodeCTO,PGA_HC_CommodityTypeCTO";
		const string HCDSEFields = ",PGA_HC_DSEInd,PGA_HC_IntendedUseCodeDSE,PGA_HC_CommodityTypeDSE";
		const string HCHDRFields = ",PGA_HC_HDRInd,PGA_HC_IntendedUseCodeHDR,PGA_HC_CommodityTypeHDR";
		const string HCMDEFields = ",PGA_HC_MDEInd,PGA_HC_IntendedUseCodeMDE,PGA_HC_CommodityTypeMDE";
		const string HCNHPFields = ",PGA_HC_NHPInd,PGA_HC_IntendedUseCodeNHP,PGA_HC_CommodityTypeNHP";
		const string HCOCSFields = ",PGA_HC_OCSInd,PGA_HC_IntendedUseCodeOCS,PGA_HC_CommodityTypeOCS";
		const string HCPESFields = ",PGA_HC_PESInd,PGA_HC_IntendedUseCodePES,PGA_HC_CommodityTypePES";
		const string HCREDFields = ",PGA_HC_REDInd,PGA_HC_IntendedUseCodeRED,PGA_HC_CommodityTypeRED";
		const string HCVETFields = ",PGA_HC_VETInd,PGA_HC_IntendedUseCodeVET,PGA_HC_CommodityTypeVET";
		const string HCSharedFields = ",PGA_HC_GTINNumber,PGA_HC_BatchLotNumber,PGA_HC_LymphoCellOrgan,PGA_HC_SemenCertification,PGA_HC_MedUniqueDeviceIDNumber,PGA_HC_MedDevEstablishLicenceExemption,PGA_HC_CASNumber,PGA_HC_PMRAScheduledPestControlProducts,PGA_HC_PMRAExemptPestControlProducts,PGA_HC_FDANumber,PGA_HC_LPCOs";

		void AssertHC(ZString lookupCode, ZString hcInd, ZString hcAPIInd, ZString intendedUseCodeAPI, ZString commodityTypeAPI,
			ZString hcBBCInd, ZString intendedUseCodeBBC, ZString commodityTypeBBC, ZString hcCPRInd, ZString intendedUseCodeCPR, ZString commodityTypeCPR,
			ZString hcCTOInd, ZString intendedUseCodeCTO, ZString commodityTypeCTO, ZString hcDSEInd, ZString intendedUseCodeDSE, ZString commodityTypeDSE,
			ZString hcHDRInd, ZString intendedUseCodeHDR, ZString commodityTypeHDR, ZString hcMDEInd, ZString intendedUseCodeMDE, ZString commodityTypeMDE,
			ZString hcNHPInd, ZString intendedUseCodeNHP, ZString commodityTypeNHP, ZString hcOCSInd, ZString intendedUseCodeOCS, ZString commodityTypeOCS,
			ZString hcPESInd, ZString intendedUseCodePES, ZString commodityTypePES, ZString hcREDInd, ZString intendedUseCodeRED, ZString commodityTypeRED,
			ZString hcVETInd, ZString intendedUseCodeVET, ZString commodityTypeVET, ZString gtinNo, ZString batchLoNo, ZBool lymphoCellOrgan, ZBool semenCertification,
			ZString medUniqueDeviceIDNo, ZBool medDevEstablishLicenceExemption, ZString casNumber, ZBool pmraScheduledPestControlProducts, ZBool pmraExemptPestControlProducts,
			ZString fdaNumber, ZBool haveLPCO, ZBool haveDIFURN)
		{
			var classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, lookupCode)).First();
			AssertEquals("CCA_HCIndicator", hcInd, classification.CCA_HCIndicator);
			var hc = classification.HCPGAHeader;
			if (YesNoList.IsYesOrNo(hcInd))
			{
				AssertEquals("PGA_HC_APIInd", hcAPIInd, hc.CA_APIProgramInd);
				AssertEquals("PGA_HC_IntendedUseCodeAPI", intendedUseCodeAPI, hc.CA_IntendedUseCodeAPI);
				AssertEquals("PGA_HC_CommodityTypeAPI", commodityTypeAPI, hc.CA_CategoryAPI);
				AssertEquals("PGA_HC_BBCInd", hcBBCInd, hc.CA_BBCProgramInd);
				AssertEquals("PGA_HC_IntendedUseCodeBBC", intendedUseCodeBBC, hc.CA_IntendedUseCodeBBC);
				AssertEquals("PGA_HC_CommodityTypeBBC", commodityTypeBBC, hc.CA_CategoryBBC);
				AssertEquals("PGA_HC_CPRInd", hcCPRInd, hc.CA_CPRProgramInd);
				AssertEquals("PGA_HC_IntendedUseCodeCPR", intendedUseCodeCPR, hc.CA_IntendedUseCodeCPR);
				AssertEquals("PGA_HC_CommodityTypeCPR", commodityTypeCPR, hc.CA_CategoryCPR);
				AssertEquals("PGA_HC_CTOInd", hcCTOInd, hc.CA_CTOProgramInd);
				AssertEquals("PGA_HC_IntendedUseCodeCTO", intendedUseCodeCTO, hc.CA_IntendedUseCodeCTO);
				AssertEquals("PGA_HC_CommodityTypeCTO", commodityTypeCTO, hc.CA_CategoryCTO);
				AssertEquals("PGA_HC_DSEInd", hcDSEInd, hc.CA_DSEProgramInd);
				AssertEquals("PGA_HC_IntendedUseCodeDSE", intendedUseCodeDSE, hc.CA_IntendedUseCodeDSE);
				AssertEquals("PGA_HC_CommodityTypeDSE", commodityTypeDSE, hc.CA_CategoryDSE);
				AssertEquals("PGA_HC_HDRInd", hcHDRInd, hc.CA_HDRProgramInd);
				AssertEquals("PGA_HC_IntendedUseCodeHDR", intendedUseCodeHDR, hc.CA_IntendedUseCodeHDR);
				AssertEquals("PGA_HC_CommodityTypeHDR", commodityTypeHDR, hc.CA_CategoryHDR);
				AssertEquals("PGA_HC_MDEInd", hcMDEInd, hc.CA_MDEProgramInd);
				AssertEquals("PGA_HC_IntendedUseCodeMDE", intendedUseCodeMDE, hc.CA_IntendedUseCodeMDE);
				AssertEquals("PGA_HC_CommodityTypeMDE", commodityTypeMDE, hc.CA_CategoryMDE);
				AssertEquals("PGA_HC_NHPInd", hcNHPInd, hc.CA_NHPProgramInd);
				AssertEquals("PGA_HC_IntendedUseCodeNHP", intendedUseCodeNHP, hc.CA_IntendedUseCodeNHP);
				AssertEquals("PGA_HC_CommodityTypeNHP", commodityTypeNHP, hc.CA_CategoryNHP);
				AssertEquals("PGA_HC_OCSInd", hcOCSInd, hc.CA_OCSProgramInd);
				AssertEquals("PGA_HC_IntendedUseCodeOCS", intendedUseCodeOCS, hc.CA_IntendedUseCodeOCS);
				AssertEquals("PGA_HC_CommodityTypeOCS", commodityTypeOCS, hc.CA_CategoryOCS);
				AssertEquals("PGA_HC_PESInd", hcPESInd, hc.CA_PESProgramInd);
				AssertEquals("PGA_HC_IntendedUseCodePES", intendedUseCodePES, hc.CA_IntendedUseCodePES);
				AssertEquals("PGA_HC_CommodityTypePES", commodityTypePES, hc.CA_CategoryPES);
				AssertEquals("PGA_HC_REDInd", hcREDInd, hc.CA_REDProgramInd);
				AssertEquals("PGA_HC_IntendedUseCodeRED", intendedUseCodeRED, hc.CA_IntendedUseCodeRED);
				AssertEquals("PGA_HC_CommodityTypeRED", commodityTypeRED, hc.CA_CategoryRED);
				AssertEquals("PGA_HC_VETInd", hcVETInd, hc.CA_VETProgramInd);
				AssertEquals("PGA_HC_IntendedUseCodeVET", intendedUseCodeVET, hc.CA_IntendedUseCodeVET);
				AssertEquals("PGA_HC_CommodityTypeVET", commodityTypeVET, hc.CA_CategoryVET);

				AssertEquals("PGA_HC_GTINNumber", gtinNo, hc.CA_GTINNumber);
				AssertEquals("PGA_HC_BatchLotNumber", batchLoNo, hc.CA_BatchLotNumber);
				AssertEquals("PGA_HC_LymphoCellOrgan", lymphoCellOrgan, hc.CA_CTO_LCO);
				AssertEquals("PGA_HC_SemenCertification", semenCertification, hc.CA_ComplianceStatement);
				AssertEquals("PGA_HC_MedUniqueDeviceIDNumber", medUniqueDeviceIDNo, hc.CA_UniqueDeviceIDNumber);
				AssertEquals("PGA_HC_MedDevEstablishLicenceExemption", medDevEstablishLicenceExemption, hc.CA_MDE_LEX);
				AssertEquals("PGA_HC_CASNumber", casNumber, hc.CA_CASNumber);
				AssertEquals("PGA_HC_PMRAExemptPestControlProducts", pmraScheduledPestControlProducts, hc.CA_PES_SPCP);
				AssertEquals("PGA_HC_PMRAScheduledPestControlProducts", pmraExemptPestControlProducts, hc.CA_PES_EPCP);
				AssertEquals("PGA_HC_FDANumber", fdaNumber, hc.CA_FDANumber);

				if (haveLPCO)
				{
					var lpcos = hc.LPCOViews.Cast<LPCOView>().Where(x => x.CLP_Type == "AAA" || x.CLP_Type == "BBB").OrderBy(x => x.CLP_Type).ToList();
					AssertEquals("PGA_HC_LPCOs", 2, lpcos.Count);
					AssertEquals("LPCO1 CLP_Type", "AAA", lpcos[0].CLP_Type);
					AssertEquals("LPCO1 CLP_RefNo", "111", lpcos[0].CLP_RefNo);
					AssertEquals("LPCO2 CLP_Type", "BBB", lpcos[1].CLP_Type);
					AssertEquals("LPCO2 CLP_RefNo", "333", lpcos[1].CLP_RefNo);
					if (haveDIFURN)
					{
						AssertEquals("LPCO1 CLP_DIFRefNumberOrLocation", "222", lpcos[0].CLP_DIFRefNumberOrLocation);
						AssertEquals("LPCO2 CLP_DIFRefNumberOrLocation", "444", lpcos[1].CLP_DIFRefNumberOrLocation);
					}
					else
					{
						AssertEquals("LPCO1 CLP_DIFRefNumberOrLocation", ZString.Empty, lpcos[0].CLP_DIFRefNumberOrLocation);
						AssertEquals("LPCO2 CLP_DIFRefNumberOrLocation", ZString.Empty, lpcos[1].CLP_DIFRefNumberOrLocation);
					}
				}
				else
				{
					var lpcos = hc.LPCOViews.Cast<LPCOView>().OrderBy(x => x.CLP_Type).ToList();
					AssertEquals("PGA_HC_LPCOs", 0, lpcos.Count);
				}
			}
			else
			{
				AssertNull(hc);
			}
		}

		public void TestImportNRCan()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(headerFields + NRCanIndicators);
					sw.WriteLine(string.Format("{0}", headerValues("C1")));
					sw.WriteLine(string.Format("{0},N,N,N", headerValues("C2")));
					sw.WriteLine(string.Format("{0},Y,N,N", headerValues("C3")));
					sw.WriteLine(string.Format("{0},N,Y,N", headerValues("C4")));
					sw.WriteLine(string.Format("{0},N,N,Y", headerValues("C5")));
					sw.WriteLine(string.Format("{0},Y,Y,Y", headerValues("C6")));
				}

				Runner.ImportData(testFileName.Filename, "row");
				var classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C1")).First();
				AssertEquals("CCA_NRCanIndicator", ZString.Empty, classification.CCA_NRCanIndicator);
				var nrcan = classification.NRCanPGAHeader;
				AssertNull(nrcan);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C2")).First();
				AssertEquals("CCA_NRCanIndicator", YesNoList.Codes.No, classification.CCA_NRCanIndicator);
				nrcan = classification.NRCanPGAHeader;
				AssertEquals("PGA_NRCan_EEFInd", YesNoList.Codes.No, nrcan.CA_EEFProgramInd);
				AssertEquals("PGA_NRCan_EXPInd", YesNoList.Codes.No, nrcan.CA_EXPProgramInd);
				AssertEquals("PGA_NRCan_RDAInd", YesNoList.Codes.No, nrcan.CA_RDAProgramInd);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C3")).First();
				AssertEquals("CCA_NRCanIndicator", YesNoList.Codes.Yes, classification.CCA_NRCanIndicator);
				nrcan = classification.NRCanPGAHeader;
				AssertEquals("PGA_NRCan_EEFInd", YesNoList.Codes.Yes, nrcan.CA_EEFProgramInd);
				AssertEquals("PGA_NRCan_EXPInd", YesNoList.Codes.No, nrcan.CA_EXPProgramInd);
				AssertEquals("PGA_NRCan_RDAInd", YesNoList.Codes.No, nrcan.CA_RDAProgramInd);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C4")).First();
				AssertEquals("CCA_NRCanIndicator", YesNoList.Codes.Yes, classification.CCA_NRCanIndicator);
				nrcan = classification.NRCanPGAHeader;
				AssertEquals("PGA_NRCan_EEFInd", YesNoList.Codes.No, nrcan.CA_EEFProgramInd);
				AssertEquals("PGA_NRCan_EXPInd", YesNoList.Codes.Yes, nrcan.CA_EXPProgramInd);
				AssertEquals("PGA_NRCan_RDAInd", YesNoList.Codes.No, nrcan.CA_RDAProgramInd);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C5")).First();
				AssertEquals("CCA_NRCanIndicator", YesNoList.Codes.Yes, classification.CCA_NRCanIndicator);
				nrcan = classification.NRCanPGAHeader;
				AssertEquals("PGA_NRCan_EEFInd", YesNoList.Codes.No, nrcan.CA_EEFProgramInd);
				AssertEquals("PGA_NRCan_EXPInd", YesNoList.Codes.No, nrcan.CA_EXPProgramInd);
				AssertEquals("PGA_NRCan_RDAInd", YesNoList.Codes.Yes, nrcan.CA_RDAProgramInd);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C6")).First();
				AssertEquals("CCA_NRCanIndicator", YesNoList.Codes.Yes, classification.CCA_NRCanIndicator);
				nrcan = classification.NRCanPGAHeader;
				AssertEquals("PGA_NRCan_EEFInd", YesNoList.Codes.Yes, nrcan.CA_EEFProgramInd);
				AssertEquals("PGA_NRCan_EXPInd", YesNoList.Codes.Yes, nrcan.CA_EXPProgramInd);
				AssertEquals("PGA_NRCan_RDAInd", YesNoList.Codes.Yes, nrcan.CA_RDAProgramInd);
			}
		}
		const string NRCanIndicators = ",PGA_NRCan_EEFInd,PGA_NRCan_EXPInd,PGA_NRCan_RDAInd";

		public void TestImportPHAC()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(headerFields + PHACIndicators);
					sw.WriteLine(string.Format("{0}", headerValues("C1")));
					sw.WriteLine(string.Format("{0},N", headerValues("C2")));
					sw.WriteLine(string.Format("{0},Y", headerValues("C3")));
				}

				Runner.ImportData(testFileName.Filename, "row");
				var classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C1")).First();
				AssertEquals("PGA_PHAC_HAPInd", ZString.Empty, classification.CCA_PHACIndicator);
				var phac = classification.PHACPGAHeader;
				AssertNull(phac);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C2")).First();
				AssertEquals("PGA_PHAC_HAPInd", YesNoList.Codes.No, classification.CCA_PHACIndicator);
				phac = classification.PHACPGAHeader;
				AssertEquals("CA_HAPProgramInd", YesNoList.Codes.No, phac.CA_HAPProgramInd);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C3")).First();
				AssertEquals("PGA_PHAC_HAPInd", YesNoList.Codes.Yes, classification.CCA_PHACIndicator);
				phac = classification.PHACPGAHeader;
				AssertEquals("CA_HAPProgramInd", YesNoList.Codes.Yes, phac.CA_HAPProgramInd);
			}
		}
		const string PHACIndicators = ",PGA_PHAC_HAPInd";

		public void TestImportTC()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(headerFields + TCCanIndicators);
					sw.WriteLine(string.Format("{0}", headerValues("C1")));
					sw.WriteLine(string.Format("{0},N,N", headerValues("C2")));
					sw.WriteLine(string.Format("{0},Y,N", headerValues("C3")));
					sw.WriteLine(string.Format("{0},N,Y", headerValues("C4")));
					sw.WriteLine(string.Format("{0},Y,Y", headerValues("C5")));
				}

				Runner.ImportData(testFileName.Filename, "row");
				var classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C1")).First();
				AssertEquals("CCA_TCIndicator", ZString.Empty, classification.CCA_TCIndicator);
				var tc = classification.TCPGAHeader;
				AssertNull(tc);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C2")).First();
				AssertEquals("CCA_TCIndicator", YesNoList.Codes.No, classification.CCA_TCIndicator);
				tc = classification.TCPGAHeader;
				AssertEquals("PGA_TC_TPRInd", YesNoList.Codes.No, tc.CA_TPRProgramInd);
				AssertEquals("PGA_TC_VPRInd", YesNoList.Codes.No, tc.CA_VPRProgramInd);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C3")).First();
				AssertEquals("CCA_TCIndicator", YesNoList.Codes.Yes, classification.CCA_TCIndicator);
				tc = classification.TCPGAHeader;
				AssertEquals("PGA_TC_TPRInd", YesNoList.Codes.Yes, tc.CA_TPRProgramInd);
				AssertEquals("PGA_TC_VPRInd", YesNoList.Codes.No, tc.CA_VPRProgramInd);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C4")).First();
				AssertEquals("CCA_TCIndicator", YesNoList.Codes.Yes, classification.CCA_TCIndicator);
				tc = classification.TCPGAHeader;
				AssertEquals("PGA_TC_TPRInd", YesNoList.Codes.No, tc.CA_TPRProgramInd);
				AssertEquals("PGA_TC_VPRInd", YesNoList.Codes.Yes, tc.CA_VPRProgramInd);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C5")).First();
				AssertEquals("CCA_TCIndicator", YesNoList.Codes.Yes, classification.CCA_TCIndicator);
				tc = classification.TCPGAHeader;
				AssertEquals("PGA_TC_TPRInd", YesNoList.Codes.Yes, tc.CA_TPRProgramInd);
				AssertEquals("PGA_TC_VPRInd", YesNoList.Codes.Yes, tc.CA_VPRProgramInd);
			}
		}
		const string TCCanIndicators = ",PGA_TC_TPRInd,PGA_TC_VPRInd";

		public void TestImportCNSC()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					var lpcos1 = "7000/NO123;7001/NO456";
					sw.WriteLine(headerFields + CNSCIndicators + CNSCFields);
					sw.WriteLine(string.Format("{0}", headerValues("C1")));
					sw.WriteLine(string.Format("{0},N", headerValues("C2")));
					sw.WriteLine(string.Format("{0},N,NE,TEST,TEST1", headerValues("C3")));
					sw.WriteLine(string.Format("{0},Y,NE,TEST,TEST1", headerValues("C4")));
					sw.WriteLine(string.Format("{0},Y,NE,TEST,TEST1,{1}", headerValues("C5"), lpcos1));
				}

				Runner.ImportData(testFileName.Filename, "row");
				var classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C1")).First();
				AssertEquals("PGA_CNSC_Indicator", ZString.Empty, classification.CCA_CNSCIndicator);
				var cnsc = classification.CNSCPGAHeader;
				AssertNull(cnsc);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C2")).First();
				AssertEquals("PGA_CNSC_Indicator", YesNoList.Codes.No, classification.CCA_CNSCIndicator);
				cnsc = classification.CNSCPGAHeader;
				AssertEquals("CA_AllProgramInd", YesNoList.Codes.No, cnsc.CA_AllProgramInd);
				AssertEquals("PGA_CNSC_Category", ZString.Empty, cnsc.CA_Category);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C3")).First();
				AssertEquals("PGA_CNSC_Indicator", YesNoList.Codes.No, classification.CCA_CNSCIndicator);
				cnsc = classification.CNSCPGAHeader;
				AssertEquals("CA_AllProgramInd", YesNoList.Codes.No, cnsc.CA_AllProgramInd);
				AssertEquals("PGA_CNSC_Category", ZString.Empty, cnsc.CA_Category);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C4")).First();
				AssertEquals("PGA_CNSC_Indicator", YesNoList.Codes.Yes, classification.CCA_CNSCIndicator);
				cnsc = classification.CNSCPGAHeader;
				AssertEquals("CA_AllProgramInd", YesNoList.Codes.Yes, cnsc.CA_AllProgramInd);
				AssertEquals("PGA_CNSC_Category", "NE", cnsc.CA_Category);
				AssertEquals("PGA_CNSC_NNIECRSchedulePartNo", "TEST", cnsc.CA_NNIECRSchePartNo);
				AssertEquals("PGA_CNSC_PackMarks", "TEST1", cnsc.CA_PackMarks);
				AssertEquals("PGA_CNSC_LPCOs", 0, cnsc.LPCOViews.Count);

				classification = Factory.Load<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, "C5")).First();
				AssertEquals("PGA_CNSC_Indicator", YesNoList.Codes.Yes, classification.CCA_CNSCIndicator);
				cnsc = classification.CNSCPGAHeader;
				AssertEquals("CA_AllProgramInd", YesNoList.Codes.Yes, cnsc.CA_AllProgramInd);
				AssertEquals("PGA_CNSC_Category", "NE", cnsc.CA_Category);
				AssertEquals("PGA_CNSC_NNIECRSchedulePartNo", "TEST", cnsc.CA_NNIECRSchePartNo);
				AssertEquals("PGA_CNSC_PackMarks", "TEST1", cnsc.CA_PackMarks);
				var lpcos = cnsc.LPCOViews.Cast<LPCOView>().OrderBy(x => x.CLP_Type).ToList();
				AssertEquals("PGA_CNSC_LPCOs", 2, lpcos.Count);
				AssertEquals("LPCO1 CLP_Type", "7000", lpcos[0].CLP_Type);
				AssertEquals("LPCO1 CLP_RefNo", "NO123", lpcos[0].CLP_RefNo);
				AssertEquals("LPCO2 CLP_Type", "7001", lpcos[1].CLP_Type);
				AssertEquals("LPCO2 CLP_RefNo", "NO456", lpcos[1].CLP_RefNo);
			}
		}
		const string CNSCIndicators = ",PGA_CNSC_Indicator";
		const string CNSCFields = ",PGA_CNSC_Category,PGA_CNSC_NNIECRSchedulePartNo,PGA_CNSC_PackMarks,PGA_CNSC_LPCOs";

		public void TestCountrySpecificColumns()
		{
			var cols = new List<string>() {
			ClassificationDataLoad.CAFieldNames.VFDCode,
			ClassificationDataLoad.CAFieldNames.TariffTreatment,
			ClassificationDataLoad.CAFieldNames.Tariff99Code,
			ClassificationDataLoad.CAFieldNames.AuthorityNum,
			ClassificationDataLoad.CAFieldNames.TRSNum,
			ClassificationDataLoad.CAFieldNames.Manufacturer,
			ClassificationDataLoad.CAFieldNames.GSTCode,
			ClassificationDataLoad.CAFieldNames.ExciseExemptCode,
			ClassificationDataLoad.CAFieldNames.ExciseRateCode,
			ClassificationDataLoad.CAFieldNames.SIMACode,
			ClassificationDataLoad.CAFieldNames.SIMADumpingNumber,
			ClassificationDataLoad.CAFieldNames.OriginCountry
			};
			cols.AddRange(typeof(IPGADataToLoad).GetProperties().Select(x => x.Name));
			AssertContainsExactElementsInAnyOrder(cols, Runner.ExtraColumns);
		}

		public void TestGetPGAPropertiesToSuspendSetting()
		{
			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					sw.WriteLine(headerFields + ",PGA_ECCC_WRMInd,PGA_CFIA_Indicator,PGA_CNSC_Indicator,PGA_GAC_Indicator,PGA_DFO_ABIInd,PGA_HC_APIInd,PGA_NRCan_EEFInd,PGA_PHAC_HAPInd,PGA_TC_TPRInd");
					sw.WriteLine(headerValues("C1") + ",Y,N,Y,N,Y,N,Y,N,Y");
				}
				Runner.ImportData(tempFile.Filename, "row");
				var suspendedPGAProperties = CAOrgSupplierPartAndClassificationDataLoad_PGAHelper.PGAFieldNames.GetPGAPropertiesToSuspendSetting(Runner).ToArray();
				var expectPGAProperties = new ZString[] {
					CusClassPartPivot.Schema.CCA_ECCCIndicator,
					CusClassPartPivot.Schema.CCA_CFIAIndicator,
					CusClassPartPivot.Schema.CCA_CNSCIndicator,
					CusClassPartPivot.Schema.CCA_GACIndicator,
					CusClassPartPivot.Schema.CCA_DFOIndicator,
					CusClassPartPivot.Schema.CCA_HCIndicator,
					CusClassPartPivot.Schema.CCA_NRCanIndicator,
					CusClassPartPivot.Schema.CCA_PHACIndicator,
					CusClassPartPivot.Schema.CCA_TCIndicator
				};
				AssertContainsExactElementsInAnyOrder(expectPGAProperties, suspendedPGAProperties);
			}
		}

		public void TestImportCsvClassificationsWithCPC()
		{
			#region Universal Tariff Setup

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var chinaTradeGroup = universalHelper.CreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddCountry(chinaTradeGroup, Core.Constants.CountryCodes.China, ZDate.Today.AddYears(-1), ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var simaTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, DutyAndTaxManager.SIMATariffType);
			var antiDumpingRateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.AntiDumping);
			var antiDumpingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.ADD, antiDumpingRateType.PK);
			var surTaxRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.SUR, antiDumpingRateType.PK);
			var countervailingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.CVD, antiDumpingRateType.PK);
			Factory.Save();
			var surtaxTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, "Desc1 from dbo.TariffView");
			universalHelper.CreateTariffUOM(surtaxTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM");
			var surTaxRate = universalHelper.CreateRate(surtaxTariff, surTaxRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "0.1 * VFD");
			var surTaxApplicability = universalHelper.CreateCusApplicability(surTaxRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var antiDumpingRelTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0123456789", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(antiDumpingRelTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NMB");
			var antiDumpingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1407", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "0123456789");
			var antiDumpingRelationShip = universalHelper.CreateTariffRelationship(antiDumpingTariff.PK, harmonizedTariffType.PK, "0123456789");
			var antiDumpingRate = universalHelper.CreateRate(antiDumpingTariff, antiDumpingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "100.5 * [NMB]");
			var antiDumpingApplicability = universalHelper.CreateCusApplicability(antiDumpingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			Factory.Save();

			#endregion

			var tariff2 = Factory.New<CACExportTariff>();
			tariff2.CE_Code = "12345678";
			tariff2.CE_Description = "Desc2";

			var cACClassHeader1 = Factory.New<CACClassHeader>();
			cACClassHeader1.ZA_ClassificationNumber = "1234567890";
			cACClassHeader1.ZA_AreaCode = "AAA";
			cACClassHeader1.ZA_EffectiveDate = ZDateTime.Today.AddDays(-1);
			cACClassHeader1.ZA_ExpiryDate = ZDateTime.Today.AddDays(1);

			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("CODE,TYPE,DESCRIPTION,TARIFF,VFDCode,TariffTreatment,Tariff99Code,AuthorityNum,TRSNum,GSTCode,ExciseExemptCode,ExciseRateCode,SIMACode,SIMADumpingNumber,OriginCountry");
					sw.WriteLine("C1,IMP,,1234567890");
					sw.WriteLine("C2,EXP,Desc2,12345678");
					sw.WriteLine("C3,IMP,Desc3,0123456789,13,04,9901,Auth,TRS,ASD,ZXC,QWE,ASS,11111,CN");
				}

				Runner.ImportData(testFileName.Filename, "row");
				var checkFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, SQLComparisonOperator.GreaterThan, "");
				checkFilter.OrderBy = CusClassificationSchema.CC_Description.Name;
				var addedRows = Factory.Load<CusClassification>(checkFilter);
				AssertEquals("There should have been 1 Classification records created.", 3, addedRows.Length);
				AssertEquals("Desc1 from dbo.TariffView", addedRows[0].CC_Description);
				AssertEquals("Desc2", addedRows[1].CC_Description);
				AssertEquals("C3", addedRows[2].CC_LookupCode);
				AssertEquals("IMP", addedRows[2].CC_ClassificationType);
				AssertEquals("Desc3", addedRows[2].CC_Description);
				AssertEquals("13", addedRows[2].CCA_ValueForDutyCode);
				AssertEquals("04", addedRows[2].CCA_TreatmentCode);
				AssertEquals("9901", addedRows[2].CCA_99TariffCode);
				AssertEquals("Auth", addedRows[2].CCA_AuthorityNumber);
				AssertEquals("TRS", addedRows[2].CCA_TRSNumber);
				AssertEquals("AS", addedRows[2].CCA_GSTStatusCode);
				AssertEquals("ZX", addedRows[2].CCA_ETExemption);
				AssertEquals("QWE", addedRows[2].CCA_ETRateCode);
				AssertEquals("11111", addedRows[2].CCA_SIMADumpingNumber);
				AssertEquals("ASS", addedRows[2].DutiesAndTaxes.Cast<DutyAndTax>().FirstOrDefault(x => DutyAndTaxTypes.IsSIMATaxCode(x.C1_TaxType)).C1_ExemptCode);
			}
		}

		public void TestImportCsvClassificationsWithLongCode()
		{
			var tariff2 = Factory.New<CACExportTariff>();
			tariff2.CE_Code = "12345678";
			tariff2.CE_Description = "Desc2";
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("CODE,TYPE,DESCRIPTION,TARIFF,VFDCode,TariffTreatment,Tariff99Code,AuthorityNum,TRSNum");
					sw.WriteLine("SomeCodeWithExtremelyRidiculousLongCharacters,EXP,,12345678");
				}

				Runner.ImportData(testFileName.Filename, "row");
				var checkFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, SQLComparisonOperator.GreaterThan, "");
				checkFilter.OrderBy = CusClassificationSchema.CC_Description.Name;
				var addedRows = Factory.Load<CusClassification>(checkFilter);
				AssertEquals("There should have been records created.", 1, addedRows.Length);
				AssertEquals("SomeCodeWithExtremelyRidiculousLong", addedRows[0].CC_LookupCode);
			}
		}

		public void TestImportCsvClassificationsWithInvalidType()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("CODE,TYPE,DESCRIPTION,TARIFF,VFDCode,TariffTreatment,Tariff99Code,AuthorityNum,TRSNum");
					sw.WriteLine("C0,IMP,Desc0,9999999999,13,04,9901,Auth,TRS");
					sw.WriteLine("C1,EXP,Desc1,9999999999,13,04,9901,Auth,TRS");
					sw.WriteLine("C2,imp,Desc2,9999999999,13,04,9901,Auth,TRS");
					sw.WriteLine("C3,exp,Desc3,9999999999,13,04,9901,Auth,TRS");
					sw.WriteLine("C4,TST,Desc4,9999999999,13,04,9901,Auth,TRS");
					sw.WriteLine("C5,TEST,Desc5,9999999999,13,04,9901,Auth,TRS");
					sw.WriteLine("C6,,Desc6,9999999999,13,04,9901,Auth,TRS");
				}

				Runner.ImportData(testFileName.Filename, "row");
				var checkFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, SQLComparisonOperator.GreaterThan, "");
				checkFilter.OrderBy = CusClassificationSchema.CC_Description.Name;
				var addedRows = Factory.Load<CusClassification>(checkFilter);
				AssertEquals("There should have been 4 Classification records created.", 4, addedRows.Length);
				AssertEquals("IMP", addedRows[0].CC_ClassificationType.ToUpper());
				AssertEquals("EXP", addedRows[1].CC_ClassificationType.ToUpper());
				AssertEquals("IMP", addedRows[2].CC_ClassificationType.ToUpper());
				AssertEquals("EXP", addedRows[3].CC_ClassificationType.ToUpper());
				Assert(Runner.Log.Contains("Row 6: Record Excluded - Classification type should only be 'IMP' or 'EXP'"));
				Assert(Runner.Log.Contains("Row 7: Record Excluded - Classification type should only be 'IMP' or 'EXP'"));
				Assert(Runner.Log.Contains("Row 8: Record Excluded - Classification type should only be 'IMP' or 'EXP'"));
			}
		}

		const string headerFields = "CODE,TYPE,DESCRIPTION,TARIFF,VFDCode,TariffTreatment,Tariff99Code,AuthorityNum,TRSNum,GSTCode,ExciseExemptCode,ExciseRateCode,SIMACode,SIMADumpingNumber,OriginCountry";
		string headerValues(string code)
		{
			return code + ",IMP,Desc0,9999999999,13,04,9901,Auth,TRS,GST,ETE,ETR,VBN,111111,CN";
		}

		#region Implementation

		protected override ClassificationDataLoad GetNewDataLoader()
		{
			return new ClassificationDataLoad();
		}

		ClassificationDataLoadForTest runner;
		ClassificationDataLoadForTest Runner
		{
			get
			{
				if (runner == null)
				{
					runner = new ClassificationDataLoadForTest();
				}
				return runner;
			}
		}

		#endregion
	}
}
