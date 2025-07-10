using System;
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
	[TestedType(typeof(ECCCPGAHeader))]
	sealed class ECCCPGAHeaderTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<ECCCPGAHeader>
	{
		public void TestSupportedFields()
		{
			var header = Factory.New<ECCCPGAHeader>();
			var supportedFields = header.SupportedFields;
			Assert(supportedFields.Contains(ECCCPGAHeader.Schema.CA_IntendedUseCode));
			Assert(supportedFields.Contains(ECCCPGAHeader.Schema.CA_CASNumber));
			Assert(supportedFields.Contains(ECCCPGAHeader.Schema.CA_ODSProgramInd));
			Assert(supportedFields.Contains(ECCCPGAHeader.Schema.CA_VEEProgramInd));
			Assert(supportedFields.Contains(ECCCPGAHeader.Schema.CA_WENProgramInd));
			Assert(supportedFields.Contains(ECCCPGAHeader.Schema.CA_WRMProgramInd));
		}

		public void TestSetterSuspenderForProgramInds()
		{
			var header = Factory.New<ECCCPGAHeader>();
			var suspender = header.SetterSuspender;
			suspender.SuspendSetting(ECCCPGAHeader.Schema.CA_WRMProgramInd);
			suspender.SuspendSetting(ECCCPGAHeader.Schema.CA_ODSProgramInd);
			suspender.SuspendSetting(ECCCPGAHeader.Schema.CA_WENProgramInd);
			suspender.SuspendSetting(ECCCPGAHeader.Schema.CA_VEEProgramInd);

			header.CA_WRMProgramInd = "Y";
			header.CA_ODSProgramInd = "Y";
			header.CA_WENProgramInd = "Y";
			header.CA_VEEProgramInd = "Y";

			CombineAssertions(() =>
			{
				AssertEquals("WRM", ZString.Empty, header.CA_WRMProgramInd);
				AssertEquals("ODS", ZString.Empty, header.CA_ODSProgramInd);
				AssertEquals("WEN", ZString.Empty, header.CA_WENProgramInd);
				AssertEquals("VEE", ZString.Empty, header.CA_VEEProgramInd);
			});

			suspender.ResumeSetting(ECCCPGAHeader.Schema.CA_WRMProgramInd);
			suspender.ResumeSetting(ECCCPGAHeader.Schema.CA_ODSProgramInd);
			suspender.ResumeSetting(ECCCPGAHeader.Schema.CA_WENProgramInd);
			suspender.ResumeSetting(ECCCPGAHeader.Schema.CA_VEEProgramInd);

			header.CA_WRMProgramInd = "Y";
			header.CA_ODSProgramInd = "Y";
			header.CA_WENProgramInd = "Y";
			header.CA_VEEProgramInd = "Y";

			CombineAssertions(() =>
			{
				AssertEquals("WRM", "Y", header.CA_WRMProgramInd);
				AssertEquals("ODS", "Y", header.CA_ODSProgramInd);
				AssertEquals("WEN", "Y", header.CA_WENProgramInd);
				AssertEquals("VEE", "Y", header.CA_VEEProgramInd);
			});
		}

		public void TestLPCODefaulter()
		{
			var header = Factory.New<ECCCPGAHeader>();
			var lpcoDefaulter = header as ILPCODefaulter;
			AssertNotNull(lpcoDefaulter);
			Assert(!lpcoDefaulter.ShouldDefaultLPCOFields);
			header.CA_WRMProgramInd = YesNoList.Codes.Yes;
			Assert(lpcoDefaulter.ShouldDefaultLPCOFields);
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_Type));
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_RefNo));
			Assert(!lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_EndDate));
			Assert(!lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_HolderType));
			header.CA_WENProgramInd = YesNoList.Codes.Yes;
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_EndDate));
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_HolderType));
		}

		public void TestPurgeValuesWhenCA_ProcessCodeIsChanged()
		{
			var header = Factory.New<ECCCPGAHeader>();
			header.CA_VEEProgramInd = "Y";
			SetUpECCCPGAHeader(header);
			header.CA_ProcessCode = ProcessCodes.Codes.XE01;
			AssertECCCPGAHeader(header, true, true, false, true, true, true, true);

			SetUpECCCPGAHeader(header);
			header.CA_ProcessCode = ProcessCodes.Codes.XE02;
			AssertECCCPGAHeader(header, false, false, true, false, false, false, false);

			SetUpECCCPGAHeader(header);
			header.CA_ProcessCode = ProcessCodes.Codes.XE03;
			AssertECCCPGAHeader(header, true, true, false, false, false, false, false);

			SetUpECCCPGAHeader(header);
			header.CA_ProcessCode = ProcessCodes.Codes.XE04;
			AssertECCCPGAHeader(header, true, true, false, true, true, true, true);

			void SetUpECCCPGAHeader(ECCCPGAHeader ecccHeader)
			{
				ecccHeader.CA_ProcessCode = ZString.Empty;
				ecccHeader.CA_Transition = true;
				ecccHeader.CA_Incomplete = true;
				ecccHeader.CA_ReplacementEngines = true;
				ecccHeader.CA_ENGNationalMark = true;
				ecccHeader.CA_ENGEPACertified = true;
				ecccHeader.CA_ENGCanadaUnique = true;
				ecccHeader.CA_ENGIncomplete = true;
			}

			void AssertECCCPGAHeader(ECCCPGAHeader ecccHeader, bool isTranstion, bool isIncomplete, bool isReplacementEngines, bool isENGNationalMark, bool isENGEPACertified, bool isENGCanadaUnique, bool isENGIncomplete)
			{
				AssertEquals(isTranstion, ecccHeader.CA_Transition);
				AssertEquals(isIncomplete, ecccHeader.CA_Incomplete);
				AssertEquals(isReplacementEngines, ecccHeader.CA_ReplacementEngines);
				AssertEquals(isENGNationalMark, ecccHeader.CA_ENGNationalMark);
				AssertEquals(isENGEPACertified, ecccHeader.CA_ENGEPACertified);
				AssertEquals(isENGCanadaUnique, ecccHeader.CA_ENGCanadaUnique);
				AssertEquals(isENGIncomplete, ecccHeader.CA_ENGIncomplete);
			}
		}

		public void TestCA_BulkReporting()
		{
			var header = Factory.New<ECCCPGAHeader>();
			header.CA_VEEProgramInd = YesNoList.Codes.Yes;
			header.CA_ProcessCode = ProcessCodes.Codes.XE01;
			AssertEquals(false, header.CA_BulkReporting);
			header.CA_BulkReporting = true;
			header.CA_ProcessCode = ProcessCodes.Codes.XE02;
			AssertEquals(true, header.CA_BulkReporting);
			header.CA_ProcessCode = ProcessCodes.Codes.XE04;
			AssertEquals(true, header.CA_BulkReporting);
			header.CA_ProcessCode = ProcessCodes.Codes.XE01;
			AssertEquals(true, header.CA_BulkReporting);
			header.CA_ProcessCode = ProcessCodes.Codes.XE03;
			AssertEquals(false, header.CA_BulkReporting);
		}

		public void TestAvailableLPCOFields()
		{
			AssertEquals(15, ECCCPGAHeader.AvailableLPCOFields.Count);
			Assert("CLP_HolderContactEmail", ECCCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderContactEmail));
			Assert("CLP_HolderContactName", ECCCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderContactName));
			Assert("LPCOHolderPhone", ECCCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderContactPhone));
			Assert("CLP_DIFRefNumberOrLocation", ECCCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation));
			Assert("CLP_EndDate", ECCCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_EndDate));
			Assert("CLP_HolderType", ECCCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderType));
			Assert("LPCOHolderOrgPK", ECCCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.LPCOHolderOrgPK));
			Assert("LPCOHolderAddressPK", ECCCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_OA_Holder));
			Assert("LPCOHolderCompanyName", ECCCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderName));
			Assert("CLP_IssueDate", ECCCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_IssueDate));
			Assert("CLP_AlternativeQuotaQuantity", ECCCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_AlternativeQuotaQuantity));
			Assert("CLP_RefNo", ECCCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RefNo));
			Assert("CLP_Type", ECCCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_Type));
			Assert("CLP_AlternativeQuotaUQ", ECCCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_AlternativeQuotaUQ));
			Assert("CLP_IsHolderOverridden", ECCCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_IsHolderOverridden));
		}

		public void TestPurgeValues_LPCOs()
		{
			var header = (ECCCPGAHeader)GetNewBusinessObject();
			header.LPCOViews.RemoveAndDeleteAll();
			AssertEquals("Empty", 0, header.LPCOViews.Count);

			var line = header.InvoiceLine;
			line.JI_Description = "PurgeTest";

			var lpco2 = header.LPCOViews.AddNew();
			lpco2.CLP_Type = "11";
			lpco2.CLP_DIFRefNumberOrLocation = "Test";
			lpco2.CLP_RefNo = "Test";

			AssertEquals("LPCO 11 added", 1, header.LPCOViews.Count);

			header.CA_VEEProgramInd = YesNoList.Codes.Yes;
			header.CA_ProcessCode = ProcessCodes.Codes.XE02;
			AssertEquals("LPCOs are not purged.", false, lpco2.IsDeleted);

			header.CA_ProcessCode = ProcessCodes.Codes.XE01;
			AssertEquals("LPCOs are not purged.", true, lpco2.IsDeleted);
			AssertEquals("LPCOs all purged ProcessCode is not XE02", 0, header.LPCOViews.Count);

			var lpco3 = header.LPCOViews.AddNew();
			lpco3.CLP_Type = "99";
			AssertEquals("LPCO 99 added", 1, header.LPCOViews.Count);

			header.CA_VEEProgramInd = YesNoList.Codes.No;
			AssertEquals("LPCOs are not purged.", true, lpco3.IsDeleted);
			AssertEquals("LPCOs all purged VEEProgramInd is N", 0, header.LPCOViews.Count);

			var lpco = header.LPCOViews.AddNew();
			lpco.CLP_Type = "10";
			lpco.CLP_DIFRefNumberOrLocation = "Test";
			lpco.CLP_RefNo = "Test";
			AssertEquals("LPCO added", 1, header.LPCOViews.Count);

			header.CA_ODSProgramInd = YesNoList.Codes.Yes;
			AssertEquals("LPCO defaulted", 1, header.LPCOViews.Count);
			header.CA_WRMProgramInd = YesNoList.Codes.Yes;
			AssertEquals("LPCOs WRM", 3, header.LPCOViews.Count);
			header.CA_WENProgramInd = YesNoList.Codes.Yes;
			AssertEquals("LPCOs WEN", 3, header.LPCOViews.Count);
			header.CA_VEEProgramInd = YesNoList.Codes.Yes;
			AssertEquals("LPCOs VEE", 3, header.LPCOViews.Count);

			header.CA_VEEProgramInd = YesNoList.Codes.No;
			AssertEquals("LPCOs are not purged.", false, lpco.IsDeleted);
			header.CA_WENProgramInd = YesNoList.Codes.No;
			AssertEquals("LPCOs are not purged.", false, lpco.IsDeleted);
			header.CA_WRMProgramInd = YesNoList.Codes.No;
			AssertEquals("LPCOs are not purged.", false, lpco.IsDeleted);
			header.CA_ODSProgramInd = YesNoList.Codes.No;

			AssertEquals("LPCOs are purged.", true, lpco.IsDeleted);
			AssertEquals("LPCOs all purged", 0, header.LPCOViews.Count);
		}

		public void TestPurgeValues()
		{
			var address = Factory.New<OrgAddress>();
			header.CA_WRMProgramInd = "Y";

			AssertEquals("Default LPCOs For WRM", 2, header.LPCOViews.Count);

			header.CA_IntendedUseCode = "050.001";
			invoiceLine.JI_OA_ManufacturerAddress = address.PK;
			invoiceLine.JI_OA_ConsigneeAddress = address.PK;
			var lpco = header.LPCOViews.AddNew();

			header.CA_ODSProgramInd = "Y";
			header.CA_WRMProgramInd = "N";
			CombineAssertions(() =>
			{
				AssertEquals("Purged - CA_IntendedUseCode", ZString.Empty, header.CA_IntendedUseCode);
				AssertEquals("Shouln't be purged - JI_OA_ManufacturerAddress", address.PK, invoiceLine.JI_OA_ManufacturerAddress);
				AssertEquals("Shouln't be purged - JI_OA_ConsigneeAddress", address.PK, invoiceLine.JI_OA_ConsigneeAddress);
				AssertEquals("LPCOs shouldn't be purged", 3, header.LPCOViews.Count);
			});

			header.CA_CASNumber = "1234";
			var component = header.Components.AddNew();

			header.CA_WENProgramInd = "Y";
			header.CA_ODSProgramInd = "N";
			CombineAssertions(() =>
			{
				AssertEquals("Purged - CA_CASNumber", ZString.Empty, header.CA_CASNumber);
				AssertEquals("Shouln't be purged - LPCOs", 3, header.LPCOViews.Count);
				AssertEquals("Shouln't be purged - Components", 1, header.Components.Count);
			});

			header.CA_IntendedUseCode = "EC01";
			invoiceLine.JI_Model = "AAA";
			header.CA_ScientificName = "BBB";
			header.CA_AphiaID = "1234";
			header.CA_ComplianceDeclaration = true;
			header.CA_TSN = "abc123";
			header.CA_SourceOfSpecimen = "EC35";
			invoiceLine.CA_RN_NKSource = "US";
			header.CA_Age = 12;
			header.CA_LifeStage = "Propagate";
			header.CA_Sex = "Male";

			header.CA_VEEProgramInd = "Y";
			header.CA_ProcessCode = ProcessCodes.Codes.XE01;
			header.CA_WENProgramInd = "N";
			CombineAssertions(() =>
			{
				AssertEquals("Purged - CA_IntendedUseCode", ZString.Empty, header.CA_IntendedUseCode);
				AssertEquals("Purged - CA_ScientificName", ZString.Empty, header.CA_ScientificName);
				AssertEquals("Purged - CA_AphiaID", ZString.Empty, header.CA_AphiaID);
				AssertEquals("Purged - CA_ComplianceDeclaration", false, header.CA_ComplianceDeclaration);
				AssertEquals("Purged - CA_TSN", ZString.Empty, header.CA_TSN);
				AssertEquals("Purged - CA_SourceOfSpecimen", ZString.Empty, header.CA_SourceOfSpecimen);
				AssertEquals("Purged - CA_Age", 0, header.CA_Age);
				AssertEquals("Purged - CA_LifeStage", ZString.Empty, header.CA_LifeStage);
				AssertEquals("Purged - CA_Sex", ZString.Empty, header.CA_Sex);
				AssertEquals("Purged - LPCOs", 0, header.LPCOViews.Count);
				AssertEquals("Purged - Components", 0, header.Components.Count);
				AssertEquals("Shouln't be purged - JI_Model", "AAA", invoiceLine.JI_Model);
				AssertEquals("Shouln't be purged - CA_RN_NKSource", "US", invoiceLine.CA_RN_NKSource);
			}
			);
			header.CA_EPACertified = true;
			header.CA_NationalMark = true;
			header.CA_Incomplete = true;
			header.CA_Transition = true;
			header.CA_CanadaUnique = true;
			header.CA_VehicleClass = "EC05";
			header.CA_MakeOfEngine = "make of engine";
			header.CA_ModelOfEngine = "model of engine";
			header.CA_EngineManufacturer = "manufacturer name";
			header.CA_EngineModelYear = "2017";
			header.CA_EngineIDNumber = "1234";
			header.CA_EngineClass = "EC14";
			header.CA_EngineFamilyName = "family name";
			header.CA_TestGroupName = "group";

			header.CA_ProcessCode = ProcessCodes.Codes.XE02;
			CombineAssertions(() =>
			{
				AssertEquals("Shouln't be purged - CA_EPACertified", true, header.CA_EPACertified);
				AssertEquals("Shouln't be purged - CA_NationalMark", true, header.CA_NationalMark);
				AssertEquals("Shouln't be purged - CA_CanadaUnique", true, header.CA_CanadaUnique);
				AssertEquals("Shouln't be purged - CA_MakeOfEngine", "make of engine", header.CA_MakeOfEngine);
				AssertEquals("Shouln't be purged - CA_ModelOfEngine", "model of engine", header.CA_ModelOfEngine);
				AssertEquals("Shouln't be purged - CA_EngineManufacturer", "manufacturer name", header.CA_EngineManufacturer);
				AssertEquals("Shouln't be purged - CA_EngineModelYear", "2017", header.CA_EngineModelYear);
				AssertEquals("Shouln't be purged - CA_EngineIDNumber", "1234", header.CA_EngineIDNumber);
				AssertEquals("Shouln't be purged - CA_EngineClass", "EC14", header.CA_EngineClass);
				AssertEquals("Shouln't be purged - CA_EngineFamilyName", "family name", header.CA_EngineFamilyName);
				AssertEquals("Shouln't be purged - JI_OA_ManufacturerAddress", address.PK, invoiceLine.JI_OA_ManufacturerAddress);
				AssertEquals("Purged - CA_Incomplete", false, header.CA_Incomplete);
				AssertEquals("Purged - CA_Transition", false, header.CA_Transition);
				AssertEquals("Purged - CA_VehicleClass", ZString.Empty, header.CA_VehicleClass);
				AssertEquals("Purged - CA_TestGroupName", ZString.Empty, header.CA_TestGroupName);
			}
			);

			header.CA_Incomplete = true;
			header.CA_Transition = true;
			header.CA_EnginePowerRating = 10m;
			header.CA_PowerRatingUQ = "HP";
			header.CA_MakeOfMachine = "make of machine";
			header.CA_ModelOfMachine = "model of machine";
			var machineManufacturer = ZGuid.NewZGuid();
			header.CA_MachineManufacturer = machineManufacturer;
			header.CA_MachineModelYear = "2018";

			header.CA_ProcessCode = ProcessCodes.Codes.XE03;
			CombineAssertions(() =>
			{
				AssertEquals("Shouln't be purged - CA_EPACertified", true, header.CA_EPACertified);
				AssertEquals("Shouln't be purged - CA_NationalMark", true, header.CA_NationalMark);
				AssertEquals("Shouln't be purged - CA_Incomplete", true, header.CA_Incomplete);
				AssertEquals("Shouln't be purged - CA_Transition", true, header.CA_Transition);
				AssertEquals("Shouln't be purged - CA_CanadaUnique", true, header.CA_CanadaUnique);
				AssertEquals("Shouln't be purged - CA_MakeOfEngine", "make of engine", header.CA_MakeOfEngine);
				AssertEquals("Shouln't be purged - CA_ModelOfEngine", "model of engine", header.CA_ModelOfEngine);
				AssertEquals("Shouln't be purged - CA_EngineManufacturer", "manufacturer name", header.CA_EngineManufacturer);
				AssertEquals("Shouln't be purged - CA_EngineModelYear", "2017", header.CA_EngineModelYear);
				AssertEquals("Shouln't be purged - CA_EngineIDNumber", "1234", header.CA_EngineIDNumber);
				AssertEquals("Shouln't be purged - CA_EngineClass", "EC14", header.CA_EngineClass);
				AssertEquals("Shouln't be purged - CA_EngineFamilyName", "family name", header.CA_EngineFamilyName);
				AssertEquals("Shouln't be purged - CA_EnginePowerRating", 10m, header.CA_EnginePowerRating);
				AssertEquals("Shouln't be purged - CA_PowerRatingUQ", "HP", header.CA_PowerRatingUQ);
				AssertEquals("Shouln't be purged - CA_MakeOfMachine", "make of machine", header.CA_MakeOfMachine);
				AssertEquals("Shouln't be purged - CA_ModelOfMachine", "model of machine", header.CA_ModelOfMachine);
				AssertEquals("Shouln't be purged - CA_MachineManufacturer", machineManufacturer, header.CA_MachineManufacturer);
				AssertEquals("Purged - CA_MachineModelYear", ZString.Empty, header.CA_MachineModelYear);
			});

			header.CA_ProcessCode = ProcessCodes.Codes.XE04;
			CombineAssertions(() =>
			{
				AssertEquals("Shouln't be purged - CA_EPACertified", true, header.CA_EPACertified);
				AssertEquals("Shouln't be purged - CA_NationalMark", true, header.CA_NationalMark);
				AssertEquals("Shouln't be purged - CA_Incomplete", true, header.CA_Incomplete);
				AssertEquals("Shouln't be purged - CA_Transition", true, header.CA_Transition);
				AssertEquals("Shouln't be purged - CA_CanadaUnique", true, header.CA_CanadaUnique);
				AssertEquals("Shouln't be purged - CA_MakeOfEngine", "make of engine", header.CA_MakeOfEngine);
				AssertEquals("Shouln't be purged - CA_ModelOfEngine", "model of engine", header.CA_ModelOfEngine);
				AssertEquals("Shouln't be purged - CA_EngineManufacturer", "manufacturer name", header.CA_EngineManufacturer);
				AssertEquals("Shouln't be purged - CA_EngineModelYear", "2017", header.CA_EngineModelYear);
				AssertEquals("Shouln't be purged - CA_EngineIDNumber", "1234", header.CA_EngineIDNumber);
				AssertEquals("Shouln't be purged - CA_EngineClass", "EC14", header.CA_EngineClass);
				AssertEquals("Shouln't be purged - CA_EngineFamilyName", "family name", header.CA_EngineFamilyName);
				AssertEquals("Shouln't be purged - CA_MachineManufacturer", machineManufacturer, header.CA_MachineManufacturer);
				AssertEquals("Purged - CA_EnginePowerRating", 0m, header.CA_EnginePowerRating);
				AssertEquals("Purged - CA_PowerRatingUQ", ZString.Empty, header.CA_PowerRatingUQ);
				AssertEquals("Purged - CA_MakeOfMachine", ZString.Empty, header.CA_MakeOfMachine);
				AssertEquals("Purged - CA_ModelOfMachine", ZString.Empty, header.CA_ModelOfMachine);
			});

			header.CA_VehicleClass = "EC25";
			header.CA_EvaporativeFamily = "family";

			header.CA_VEEProgramInd = "N";
			AssertEquals("Purged - LPCOs", 0, header.LPCOViews.Count);

			header.CA_WRMProgramInd = "Y";
			CombineAssertions(() =>
			{
				AssertEquals("Purged - CA_ProcessCode", ZString.Empty, header.CA_ProcessCode);
				AssertEquals("Purged - CA_EPACertified", false, header.CA_EPACertified);
				AssertEquals("Purged - CA_NationalMark", false, header.CA_NationalMark);
				AssertEquals("Purged - CA_Incomplete", false, header.CA_Incomplete);
				AssertEquals("Purged - CA_Transition", false, header.CA_Transition);
				AssertEquals("Purged - CA_CanadaUnique", false, header.CA_CanadaUnique);
				AssertEquals("Shouldn't be purged - JI_OA_ManufacturerAddress", address.PK, invoiceLine.JI_OA_ManufacturerAddress);
				AssertEquals("Purged - CA_VehicleClass", ZString.Empty, header.CA_VehicleClass);
				AssertEquals("Purged - CA_MakeOfEngine", ZString.Empty, header.CA_MakeOfEngine);
				AssertEquals("Purged - CA_ModelOfEngine", ZString.Empty, header.CA_ModelOfEngine);
				AssertEquals("Purged - CA_EngineManufacturer", ZString.Empty, header.CA_EngineManufacturer);
				AssertEquals("Purged - CA_EngineModelYear", ZString.Empty, header.CA_EngineModelYear);
				AssertEquals("Purged - CA_EngineIDNumber", ZString.Empty, header.CA_EngineIDNumber);
				AssertEquals("Purged - CA_EngineClass", ZString.Empty, header.CA_EngineClass);
				AssertEquals("Purged - CA_EngineFamilyName", ZString.Empty, header.CA_EngineFamilyName);
				AssertEquals("Purged - CA_EvaporativeFamily", ZString.Empty, header.CA_EvaporativeFamily);
			});
		}

		public void TestECCCVEEMachineManufacturerDefaultValue()
		{
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var orgHeader01 = Factory.New<OrgHeader>();
			var orgAddress01 = orgHeader01.Addresses.AddNew();
			invoiceLine.JI_OA_ManufacturerAddress = orgAddress01.PK;
			invoiceLine.CA_ECCCInd = "Y";
			header.CA_VEEProgramInd = "Y";
			header.CA_ProcessCode = ProcessCodes.Codes.XE02;
			AssertEquals("Should be equal to invoice's manufacturer.", invoiceLine.JI_OA_ManufacturerAddress, header.CA_MachineManufacturer);
		}

		public void TestCA_MachineManufacturer()
		{
			AssertType(typeof(ZGuid), header.CA_MachineManufacturer);
		}

		public void TestSupportsNotes()
		{
			var bo = (ECCCPGAHeader)GetNewBusinessObject();

			AssertEquals(false, bo.SupportsNotes);
		}

		public void TestGetCusAddInfoType()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			var iCusAddInfoTypeSupporter = (ICusAddInfoTypeSupporter)pgaHeader;
			iCusAddInfoTypeSupporter.AssertType(typeof(Component), CusAddInfoTypeAttribute.Codes.CAComponent);
		}

		protected override IEnumerable<ECCCPGAHeader> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = "Y";
			yield return invoiceLine.ECCCPGAHeader;

			var product = factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = factory.New<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			pivot.CI_TariffNum = "1111.11.11";
			pivot.CCA_ECCCIndicator = Customs.Business.YesNoList.Codes.Yes;
			yield return pivot.ECCCPGAHeader;
		}

		public void TestEngineGroupBoxVisible()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			CombineAssertions(() =>
			{
				AssertVisibilityOfProcessCodeChanges(pgaHeader, ProcessCodes.Codes.EC01, ProcessCodes.Codes.XE01, () => pgaHeader.EngineGroupBoxVisible);
				AssertVisibilityOfProcessCodeChanges(pgaHeader, ProcessCodes.Codes.EC02, ProcessCodes.Codes.XE02, () => pgaHeader.EngineGroupBoxVisible);
				AssertVisibilityOfProcessCodeChanges(pgaHeader, ProcessCodes.Codes.EC03, ProcessCodes.Codes.XE03, () => pgaHeader.EngineGroupBoxVisible);
				AssertVisibilityOfProcessCodeChanges(pgaHeader, ProcessCodes.Codes.EC04, ProcessCodes.Codes.XE04, () => pgaHeader.EngineGroupBoxVisible);
			});
		}

		public void TestVehicleGroupBoxVisible()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			CombineAssertions(() =>
			{
				AssertVisibilityOfProcessCode(pgaHeader, Array.Empty<ZString>(), new ZString[] { ProcessCodes.Codes.XE02, ProcessCodes.Codes.XE03 }, () => pgaHeader.VehicleGroupBoxVisible);
				AssertVisibilityOfProcessCodeChanges(pgaHeader, ProcessCodes.Codes.EC01, ProcessCodes.Codes.XE01, () => pgaHeader.VehicleGroupBoxVisible);
				AssertVisibilityOfProcessCodeChanges(pgaHeader, ProcessCodes.Codes.EC04, ProcessCodes.Codes.XE04, () => pgaHeader.VehicleGroupBoxVisible);
			});
		}

		public void TestMachineGroupBoxVisible()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			CombineAssertions(() =>
			{
				AssertVisibilityOfProcessCode(pgaHeader, Array.Empty<ZString>(), new ZString[] { ProcessCodes.Codes.XE01, ProcessCodes.Codes.XE04 }, () => pgaHeader.MachineGroupBoxVisible);
				AssertVisibilityOfProcessCodeChanges(pgaHeader, ProcessCodes.Codes.EC02, ProcessCodes.Codes.XE02, () => pgaHeader.MachineGroupBoxVisible);
				AssertVisibilityOfProcessCodeChanges(pgaHeader, ProcessCodes.Codes.EC03, ProcessCodes.Codes.XE03, () => pgaHeader.MachineGroupBoxVisible);
			});
		}

		public void TestGroupTextBoxVisible()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			CombineAssertions(() =>
			{
				AssertVisibilityOfProcessCode(pgaHeader, Array.Empty<ZString>(), new ZString[] { ProcessCodes.Codes.XE02, ProcessCodes.Codes.XE03, ProcessCodes.Codes.XE04 }, () => pgaHeader.TestGroupTextBoxVisible);
				AssertVisibilityOfProcessCodeChanges(pgaHeader, ProcessCodes.Codes.EC01, ProcessCodes.Codes.XE01, () => pgaHeader.TestGroupTextBoxVisible);
			});
		}

		public void TestEvaporativeFamilyTextBoxVisible()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			CombineAssertions(() =>
			{
				AssertVisibilityOfProcessCode(pgaHeader, Array.Empty<ZString>(), new ZString[] { ProcessCodes.Codes.XE01 }, () => pgaHeader.EvaporativeFamilyTextBoxVisible);
				AssertVisibilityOfProcessCodeChanges(pgaHeader, ProcessCodes.Codes.EC02, ProcessCodes.Codes.XE02, () => pgaHeader.EvaporativeFamilyTextBoxVisible);
				AssertVisibilityOfProcessCodeChanges(pgaHeader, ProcessCodes.Codes.EC04, ProcessCodes.Codes.XE04, () => pgaHeader.EvaporativeFamilyTextBoxVisible);
				AssertVisibilityOfProcessCodeChanges(pgaHeader, ProcessCodes.Codes.EC03, ProcessCodes.Codes.XE03, () => pgaHeader.EvaporativeFamilyTextBoxVisible);
			});
		}

		public void TestMachineModelYearDropEditVisible()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			CombineAssertions(() =>
			{
				AssertVisibilityOfProcessCode(pgaHeader, Array.Empty<ZString>(), new ZString[] { ProcessCodes.Codes.XE01, ProcessCodes.Codes.XE03, ProcessCodes.Codes.XE04 }, () => pgaHeader.MachineModelYearDropEditVisible);
				AssertVisibilityOfProcessCodeChanges(pgaHeader, ProcessCodes.Codes.EC02, ProcessCodes.Codes.XE02, () => pgaHeader.MachineModelYearDropEditVisible);
			});
		}

		public void TestEngineComplianceStatementGroupBoxVisible()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			CombineAssertions(() =>
			{
				var invisibleProcessCodes = new ProcessCodes();
				invisibleProcessCodes.RemoveCode(ProcessCodes.Codes.XE01);
				invisibleProcessCodes.RemoveCode(ProcessCodes.Codes.XE04);
				AssertVisibilityOfProcessCode(pgaHeader, new ZString[] { ProcessCodes.Codes.XE01, ProcessCodes.Codes.XE04 }, invisibleProcessCodes.GetAllCodesZString(), () => pgaHeader.EngineComplianceStatementGroupBoxVisible);
			});
		}

		public void TestPowerCalcDropEditVisible()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			CombineAssertions(() =>
			{
				AssertVisibilityOfProcessCode(pgaHeader, Array.Empty<ZString>(), new ZString[] { ProcessCodes.Codes.XE01, ProcessCodes.Codes.XE04 }, () => pgaHeader.PowerCalcDropEditVisible);
				AssertVisibilityOfProcessCodeChanges(pgaHeader, ProcessCodes.Codes.EC02, ProcessCodes.Codes.XE02, () => pgaHeader.PowerCalcDropEditVisible);
				AssertVisibilityOfProcessCodeChanges(pgaHeader, ProcessCodes.Codes.EC03, ProcessCodes.Codes.XE03, () => pgaHeader.PowerCalcDropEditVisible);
			});
		}

		public void TestTransitionCheckBoxVisibleVisible()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			AssertVisibilityOfProcessCode(pgaHeader, new ZString[] { ProcessCodes.Codes.XE01, ProcessCodes.Codes.XE03, ProcessCodes.Codes.XE04 }, new ZString[] { ProcessCodes.Codes.XE02 }, () => pgaHeader.TransitionCheckBoxIncompleteCheckBoxVisible);
			AssertVisibilityOfProcessCodeChanges(pgaHeader, ProcessCodes.Codes.XE02, ProcessCodes.Codes.XE03, () => pgaHeader.TransitionCheckBoxIncompleteCheckBoxVisible);
		}

		public void TestInvalidProcessCodeControlsInvisible()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			pgaHeader.CA_ProcessCode = "XXX";
			CombineAssertions(() =>
			{
				AssertEquals("EngineGroupBoxVisible", false, pgaHeader.EngineGroupBoxVisible);
				AssertEquals("VehicleGroupBoxVisible", false, pgaHeader.VehicleGroupBoxVisible);
				AssertEquals("MachineGroupBoxVisible", false, pgaHeader.MachineGroupBoxVisible);
				AssertEquals("TestGroupTextBoxVisible", false, pgaHeader.TestGroupTextBoxVisible);
				AssertEquals("PowerCalcDropEditVisible", false, pgaHeader.PowerCalcDropEditVisible);
				AssertEquals("EvaporativeFamilyTextBoxVisible", false, pgaHeader.EvaporativeFamilyTextBoxVisible);
				AssertEquals("MachineModelYearDropEditVisible", false, pgaHeader.MachineModelYearDropEditVisible);
				AssertEquals("TransitionCheckBoxVisible", true, pgaHeader.TransitionCheckBoxIncompleteCheckBoxVisible);
				AssertEquals("EngineComplianceStatementGroupBoxVisible", false, pgaHeader.EngineComplianceStatementGroupBoxVisible);
			});
		}

		OrgHeader CreateOrganisation(ZString name, ZString code)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = name;
			org.OH_Code = code;
			org.MainAddress.OA_Address1 = name + " ADDRESS 1";
			return org;
		}

		public void TestXE02EngineLocationAndEvidenceOfConfirmityLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = Customs.Business.YesNoList.Codes.Yes;
			var eccPGAHeader = invoiceLine.ECCCPGAHeader;
			eccPGAHeader.CA_ProcessCode = ProcessCodes.Codes.XE02;
			var org1 = CreateOrganisation("Engine1", "E$G1");
			var org2 = CreateOrganisation("Confirmity2", "E$C2");
			eccPGAHeader.CA_OA_EngineLocation = org1.MainAddress.PK;
			eccPGAHeader.CA_OA_EvidenceOfConformityLocation = org2.MainAddress.PK;
			Factory.Save();

			AssertEquals(org1.MainAddress.PK, eccPGAHeader.EngineLocation.PK);
			AssertEquals(org2.MainAddress.PK, eccPGAHeader.EvidenceOfConformityLocation.PK);
		}

		public void TestXE02TransitionCheckBoxIncompleteCheckBoxVisible()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			pgaHeader.CA_ProcessCode = ProcessCodes.Codes.XE01;
			AssertEquals(true, pgaHeader.TransitionCheckBoxIncompleteCheckBoxVisible);

			pgaHeader.CA_ProcessCode = ProcessCodes.Codes.XE02;
			AssertEquals(false, pgaHeader.TransitionCheckBoxIncompleteCheckBoxVisible);
		}

		public void TestXE02SpecificControlsVisible()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			pgaHeader.CA_ProcessCode = ProcessCodes.Codes.XE01;
			AssertEquals(false, pgaHeader.XE02SpecificControlsVisible);

			pgaHeader.CA_ProcessCode = ProcessCodes.Codes.XE02;
			AssertEquals(true, pgaHeader.XE02SpecificControlsVisible);
		}

		public void TestIsVehicleDetailsRequiredForXE01()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			pgaHeader.CA_VEEProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_ProcessCode = ProcessCodes.Codes.XE01;
			pgaHeader.CA_Incomplete = true;
			pgaHeader.CA_VehicleClass = ECCCProductCategories.Codes.EC11;
			AssertEquals(true, pgaHeader.IsVehicleDetailsRequiredForXE01);

			pgaHeader.CA_VehicleClass = ZString.Empty;
			AssertEquals(false, pgaHeader.IsVehicleDetailsRequiredForXE01);

			pgaHeader.CA_VehicleClass = ECCCProductCategories.Codes.EC11;
			pgaHeader.CA_Incomplete = false;
			AssertEquals(false, pgaHeader.IsVehicleDetailsRequiredForXE01);

			pgaHeader.CA_Incomplete = true;
			pgaHeader.CA_VehicleClass = ECCCProductCategories.Codes.EC11;
			pgaHeader.CA_ProcessCode = ProcessCodes.Codes.XE02;
			AssertEquals(false, pgaHeader.IsVehicleDetailsRequiredForXE01);

			pgaHeader.CA_ProcessCode = ProcessCodes.Codes.XE01;
			pgaHeader.CA_VEEProgramInd = YesNoList.Codes.No;
			AssertEquals(false, pgaHeader.IsVehicleDetailsRequiredForXE01);
		}

		public void TestIsEngineDetailsRequiredForXE01()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			pgaHeader.CA_VEEProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_ProcessCode = ProcessCodes.Codes.XE01;
			pgaHeader.CA_Incomplete = true;
			pgaHeader.CA_EngineClass = ECCCProductCategories.Codes.EC15;
			AssertEquals(true, pgaHeader.IsEngineDetailsRequiredForXE01);

			pgaHeader.CA_EngineClass = ZString.Empty;
			AssertEquals(false, pgaHeader.IsEngineDetailsRequiredForXE01);

			pgaHeader.CA_EngineClass = ECCCProductCategories.Codes.EC15;
			pgaHeader.CA_Incomplete = false;
			AssertEquals(false, pgaHeader.IsEngineDetailsRequiredForXE01);

			pgaHeader.CA_Incomplete = true;
			pgaHeader.CA_EngineClass = ECCCProductCategories.Codes.EC15;
			pgaHeader.CA_ProcessCode = ProcessCodes.Codes.XE02;
			AssertEquals(false, pgaHeader.IsEngineDetailsRequiredForXE01);

			pgaHeader.CA_ProcessCode = ProcessCodes.Codes.XE01;
			pgaHeader.CA_VEEProgramInd = YesNoList.Codes.No;
			AssertEquals(false, pgaHeader.IsEngineDetailsRequiredForXE01);
		}

		public void TestIsXE04Process()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			pgaHeader.CA_ProcessCode = ProcessCodes.Codes.XE01;
			AssertEquals(false, pgaHeader.IsXE04Process);
			pgaHeader.CA_ProcessCode = ProcessCodes.Codes.XE04;
			AssertEquals(true, pgaHeader.IsXE04Process);
		}

		public void TestDelete()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			var lpcoView = pgaHeader.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;
			var compoment1 = pgaHeader.Components.AddNew();

			pgaHeader.Delete();
			Assert("LPCO should have been deleted as well", lpco.IsDeleted);
			Assert("Component should have been deleted as well", compoment1.IsDeleted);
		}

		public void TestAddDefaultLPCOsForWRM()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			pgaHeader.CA_WRMProgramInd = YesNoList.Codes.Yes;
			AssertContainsExactElementsInAnyOrder(new[] { "8000", "8001" }, pgaHeader.LPCOViews.Cast<LPCOView>().Select(x => x.CLP_Type));
		}

		public void TestGetContactDetails()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "test@mail.com";

			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "TestOrgName";

			var contact = importer.Contacts.AddNew();
			contact.OC_Email = "xxx@yyy.com";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = OrgConstants.ContactAllocationType.CAPGA;

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_OH_Importer = importer.PK;
			jobDeclaration.JE_GS_NKCusAgent = staff.GS_Code;

			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var jobComInvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			jobComInvoiceLine.CA_ECCCInd = "Y";
			var ecccPgaHeader = jobComInvoiceLine.ECCCPGAHeader;

			AssertEquals("Should get value from the importer.", contact.OC_Email, ((ILPCOContactParent)ecccPgaHeader).GetContactDetails(LPCOHolderPartyTypeCodes.Codes.Importer).EmailAddress);
			AssertEquals("Should get value from the broker.", staff.GS_EmailAddress, ((ILPCOContactParent)ecccPgaHeader).GetContactDetails(LPCOHolderPartyTypeCodes.Codes.Broker).EmailAddress);
		}

		public void TestLPCOViewCollection()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "8021", "8021DESC", startDate, endDate);
			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributePGA = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.ECCC);
			newFactory.Save();

			AssertEquals("LpcoViews on ECCC", 0, header.LPCOViews.Count);
			AssertEquals(typeof(LPCOViewCollection), header.LPCOViews.GetType());

			var headerLPCO = declaration.LPCOViews.AddNew();
			headerLPCO.CLP_Type = "8021";
			header.LPCOViews.AddNew();
			AssertEquals("LpcoViews on ECCC", 2, header.LPCOViews.Count);

			var headerLPCO2 = declaration.LPCOViews.AddNew();
			headerLPCO2.CLP_Type = "8021";
			AssertEquals("LpcoViews on ECCC", 3, header.LPCOViews.Count);
		}

		public void TestThereIsNoExceptionWhenRequirementsParentIsNULL()
		{
			AssertNoExceptionThrown(() =>
			{
				var pgaHeader = Factory.New<ECCCPGAHeader>();
				_ = pgaHeader.OA_Manufacturer;
				pgaHeader.OA_Manufacturer = ZGuid.Empty;
				_ = pgaHeader.OA_ManufacturerInfo.SupportsMaxLength;
				_ = pgaHeader.OA_Manufacturer_ZAddress;
				_ = pgaHeader.JI_BrandName;
				pgaHeader.JI_BrandName = ZString.Empty;
				_ = pgaHeader.JI_BrandNameInfo.SupportsMaxLength;
				_ = pgaHeader.JI_Model;
				pgaHeader.JI_Model = ZString.Empty;
				_ = pgaHeader.JI_ModelInfo.SupportsMaxLength;
			});
		}

		protected override BusinessObject GetNewBusinessObject() => header;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = "Y";
			header = invoiceLine.ECCCPGAHeader;
		}

		ECCCPGAHeader header;
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		void AssertVisibilityOfProcessCodeChanges(ECCCPGAHeader pgaHeader, ZString oldCode, ZString newCode, Func<ZBool> visiblityMethod)
		{
			pgaHeader.CA_ProcessCode = oldCode;
			AssertEquals(oldCode, false, visiblityMethod.Invoke());
			pgaHeader.CA_ProcessCode = newCode;
			AssertEquals(newCode, true, visiblityMethod.Invoke());
		}

		void AssertVisibilityOfProcessCode(ECCCPGAHeader pgaHeader, ZString[] trueCodes, ZString[] falseCodes, Func<ZBool> visiblityMethod)
		{
			foreach (var code in trueCodes)
			{
				pgaHeader.CA_ProcessCode = code;
				AssertEquals(code, true, visiblityMethod.Invoke());
			}
			foreach (var code in falseCodes)
			{
				pgaHeader.CA_ProcessCode = code;
				AssertEquals(code, false, visiblityMethod.Invoke());
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = "Y";
			return invoiceLine.ECCCPGAHeader;
		}

		[TestedType(typeof(ECCCPGAHeader))]
		class ECCCPGAHeaderProgramRequirementProviderTest : InvoiceLinePGAProgramRequirementProviderTest<ECCCPGAHeader>
		{
			protected override ZString AgencyCode
			{
				get { return PGACodes.Codes.ECCC; }
			}
		}
	}
}
