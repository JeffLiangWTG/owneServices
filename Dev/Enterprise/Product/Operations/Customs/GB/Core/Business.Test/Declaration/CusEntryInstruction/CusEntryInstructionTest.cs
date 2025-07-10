using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Customs.GB.Business.GBCommonConstants;
using CusAuthorizationHeaderTypeList = Enterprise.Customs.GB.Business.CodeDescriptionPairLists.CusAuthorizationHeaderTypeList;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryInstruction))]
	class CusEntryInstructionTest : CusEntryInstructionAbstractTest
	{
		public void TestCusEntryInstructionAddInfosInUniversalShipment()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_PackageCount = 30;
			instruction.CEI_SplitReference = "1";
			var usxmlData = (UniversalShipment)UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			var cusEntryInstructionAddInfoCollection = usxmlData.EntryInstructionCollection.ElementAt(0).AddInfoCollection;
			AssertContains("PackageCount", "30", cusEntryInstructionAddInfoCollection.FirstOrDefault(x => x.Key.Value == "PackageCount").Value);
			AssertContains("HouseSplitReference", "1", cusEntryInstructionAddInfoCollection.FirstOrDefault(x => x.Key.Value == "HouseSplitReference").Value);
		}

		public void TestLevel()
		{
			var cusEntryInstruction = (EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport)Factory.New<CusEntryInstruction>();
			AssertEquals(EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header, cusEntryInstruction.Level);
		}

		public void TestWarehouseIDFor27()
		{
			OrgHeader warehouseOUTOF;
			JobDeclaration dec;
			CusEntryInstruction cei4071, cei4000, cei7100, cei7171, cei7200;
			TestDataHelper.CreateInstructionsForWarehouseAndGoodsLocationTest(Factory, out warehouseOUTOF, out dec, out cei4071, out cei4000, out cei7100, out cei7171, out cei7200);

			// From warehouse to free circ. Current location is warehouse. 
			AssertEquals("U7654321OUT", cei4071.WarehouseIDFor27);

			// From frontier to free circ. Current location is frontier
			AssertEquals("", cei4000.WarehouseIDFor27);

			// From frontier to warehouse. Current location is frontier
			AssertEquals("U1234567INN", cei7100.WarehouseIDFor27);

			// Between warehouses. Current location is current warehouse, warehouse 2/7 is target whs
			AssertEquals("U1234567INN", cei7171.WarehouseIDFor27);

			//assert rules for step 10
			AssertEquals("", cei7200.WarehouseIDFor27);

			var newCeIWithoutInvoiceLine = (CusEntryInstruction)dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			AssertEquals("", newCeIWithoutInvoiceLine.WarehouseIDFor27);

			// What about Froggie warehouses?
			warehouseOUTOF.MainAddress.OA_RN_NKCountryCode = "FR";
			warehouseOUTOF.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U99999FROGGIEOUT", "FR");
			AssertEquals("U1234567INN", cei7171.WarehouseIDFor27);
		}

		public void TestHasAnyChangeOfOwnershipProcedure()
		{
			var instructionForTest = Factory.NewWithValidTestData<CusEntryInstructionForTest>();
			declaration.CustomsEntryInstructions.Add(instructionForTest);

			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When not into warehousing and not out of warehousing HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: true, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When into warehousing and not out of warehousing HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: true
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When not into warehousing and out of warehousing HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: true, isOutOfWarehouse: true
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: true
				, assertionMessage: "When into warehousing and out of warehousing HasAnyChangeOfOwnershipProcedure should be true");

			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When not into outward processing and not out of outward processing HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: true, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When into outward processing and not out of outward processing HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: true
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When not into outward processing and out of outward processing HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: true, isOutOfOutwardProcessing: true
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: true
				, assertionMessage: "When into outward processing and into out of outward processing HasAnyChangeOfOwnershipProcedure should be true");

			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When not into inward processing and not out of inward processing HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: true, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When not into inward processing and not out of inward processing HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: true
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When not into inward processing and not out of inward processing HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: true, isOutOfInwardProcessing: true
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: true
				, assertionMessage: "When into inward processing and out of inward processing HasAnyChangeOfOwnershipProcedure should be true");

			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When not into temporary import procedure and not out of temporary import procedure HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: true, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When into temporary import procedure and not out of temporary import procedure HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: true
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false
				, assertionExpected: false
				, assertionMessage: "When not into temporary import procedure and out of temporary import procedure HasAnyChangeOfOwnershipProcedure should be false");
			SetupAndAssertHasAnyChangeOfOwnershipProcedure(instructionForTest
				, isIntoWarehouse: false, isOutOfWarehouse: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: true, isOutOfTemporaryImportProcedure: true
				, assertionExpected: true
				, assertionMessage: "When into temporary import procedure and out of temporary import procedure HasAnyChangeOfOwnershipProcedure should be true");
		}

		void SetupAndAssertHasAnyChangeOfOwnershipProcedure(CusEntryInstructionForTest instructionForTest
			, bool isIntoWarehouse, bool isOutOfWarehouse
			, bool isIntoOutwardProcessing, bool isOutOfOutwardProcessing
			, bool isIntoInwardProcessing, bool isOutOfInwardProcessing
			, bool isIntoTemporaryImportProcedure, bool isOutOfTemporaryImportProcedure
			, ZBool assertionExpected
			, string assertionMessage)
		{
			instructionForTest.SetPropertiesForTest(isIntoTemporaryImportProcedure, isOutOfTemporaryImportProcedure
				, isIntoTemporaryExportProcedure: false
				, isIntoWarehouse, isOutOfWarehouse
				, isIntoOutwardProcessing, isOutOfOutwardProcessing
				, isIntoInwardProcessing, isOutOfInwardProcessing);

			AssertEquals("pre-req HasIntoTemporaryImportProcedure", isIntoTemporaryImportProcedure, instructionForTest.HasIntoTemporaryImportProcedure);
			AssertEquals("pre-req HasOutOfTemporaryImportProcedure", isOutOfTemporaryImportProcedure, instructionForTest.HasOutOfTemporaryImportProcedure);
			AssertEquals("pre-req HasIntoWarehouseProcedure", isIntoWarehouse, instructionForTest.HasIntoWarehouseProcedure);
			AssertEquals("pre-req HasOutOfWarehouseProcedure", isOutOfWarehouse, instructionForTest.HasOutOfWarehouseProcedure);
			AssertEquals("pre-req HasIntoOutwardProcessingProcedure", isIntoOutwardProcessing, instructionForTest.HasIntoOutwardProcessingProcedure);
			AssertEquals("pre-req HasOutOfOutwardProcessingProcedure", isOutOfOutwardProcessing, instructionForTest.HasOutOfOutwardProcessingProcedure);
			AssertEquals("pre-req HasIntoInwardProcessingProcedure", isIntoInwardProcessing, instructionForTest.HasIntoInwardProcessingProcedure);
			AssertEquals("pre-req HasOutOfInwardProcessingProcedure", isOutOfInwardProcessing, instructionForTest.HasOutOfInwardProcessingProcedure);

			AssertEquals(assertionMessage, assertionExpected, instructionForTest.HasAnyChangeOfOwnershipProcedure);
		}

		public void TestCusAuthorizationUsagesExceptOldOwnerForChangeOfOwner()
		{
			var instruction = Factory.NewWithValidTestData<CusEntryInstructionForTest>();
			declaration.CustomsEntryInstructions.Add(instruction);

			var oldOwner = Factory.NewWithValidTestData<OrgHeader>();
			var newOwner = Factory.NewWithValidTestData<OrgHeader>();
			var anotherOwner = Factory.NewWithValidTestData<OrgHeader>();
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();

			var usageIPO1 = instruction.CusAuthorizationUsages.AddNew();
			usageIPO1.AGC_OH_Owner = oldOwner.PK;
			usageIPO1.AGC_Code = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			usageIPO1.AGC_Number = "IPO1 OLD";
			var usageIPO2 = instruction.CusAuthorizationUsages.AddNew();
			usageIPO2.AGC_OH_Owner = newOwner.PK;
			usageIPO2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			usageIPO2.AGC_Number = "IPO2 NEW";
			var usageIPO3 = instruction.CusAuthorizationUsages.AddNew();

			instruction.SetPropertiesForTest(isIntoWarehouse: false, isOutOfWarehouse: false, isIntoTemporaryExportProcedure: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false);
			instruction.CEI_OH_Owner = ZGuid.Empty;
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals("pre-req", false, instruction.HasAnyChangeOfOwnershipProcedure);
			AssertEquals("pre-req", ZGuid.Empty, instruction.CEI_OH_Owner);
			AssertEquals("pre-req", null, instruction.OldOwner);
			AssertEquals("pre-req", 3, instruction.CusAuthorizationUsages.Count);

			var usages = instruction.CusAuthorizationUsagesExceptOldOwnerForChangeOfOwner;
			var usageList = usages.ToList();
			AssertEquals("Return all usages - no HasAnyChangeOfOwnershipProcedure, no owner, no old owner", 3, usageList.Count);
			instruction.CEI_OH_Owner = oldOwner.PK;
			usages = instruction.CusAuthorizationUsagesExceptOldOwnerForChangeOfOwner;
			usageList = usages.ToList();
			AssertEquals("Return all usages - no HasAnyChangeOfOwnershipProcedure, owner, no old owner", 3, usageList.Count);
			declaration.JE_OH_Importer = oldOwner.PK;
			usages = instruction.CusAuthorizationUsagesExceptOldOwnerForChangeOfOwner;
			usageList = usages.ToList();
			AssertEquals("Return all usages - no HasAnyChangeOfOwnershipProcedure, owner, old owner", 3, usageList.Count);

			instruction.SetPropertiesForTest(isIntoWarehouse: false, isOutOfWarehouse: false, isIntoTemporaryExportProcedure: false
				, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
				, isIntoInwardProcessing: true, isOutOfInwardProcessing: true
				, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false);
			instruction.CEI_OH_Owner = ZGuid.Empty;
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals("pre-req", true, instruction.HasAnyChangeOfOwnershipProcedure);
			AssertEquals("pre-req", ZGuid.Empty, instruction.CEI_OH_Owner);
			AssertEquals("pre-req", null, instruction.OldOwner);

			usages = instruction.CusAuthorizationUsagesExceptOldOwnerForChangeOfOwner;
			usageList = usages.ToList();
			AssertEquals("Return all usages - HasAnyChangeOfOwnershipProcedure, no owner, no old owner", 3, usageList.Count);
			instruction.CEI_OH_Owner = oldOwner.PK;
			usages = instruction.CusAuthorizationUsagesExceptOldOwnerForChangeOfOwner;
			usageList = usages.ToList();
			AssertEquals("Return all usages - HasAnyChangeOfOwnershipProcedure, owner, no old owner", 3, usageList.Count);
			declaration.JE_OH_Importer = oldOwner.PK;
			usages = instruction.CusAuthorizationUsagesExceptOldOwnerForChangeOfOwner;
			usageList = usages.ToList();
			CombineAssertions("Return usages that don't belong to old owner", () =>
			{
				AssertEquals("Has HasAnyChangeOfOwnershipProcedure, has owner, has old owner", 2, usageList.Count);
				AssertEquals(usageIPO2.PK, usageList[0].PK);
				AssertEquals(usageIPO3.PK, usageList[1].PK);
			});

			declaration.JE_OH_Importer = anotherOwner.PK;
			usages = instruction.CusAuthorizationUsagesExceptOldOwnerForChangeOfOwner;
			usageList = usages.ToList();
			CombineAssertions("Return all usages - old owner not found", () =>
			{
				AssertEquals(3, usageList.Count);
			});
		}

		public void TestGetAuthorisationNumberOfTypeConsideringOwnersAndRegime()
		{
			var mockedInstruction = Factory.NewWithValidTestData<CusEntryInstructionForTest>();
			declaration.CustomsEntryInstructions.Add(mockedInstruction);

			mockedInstruction.SetPropertiesForTest(isIntoWarehouse: false, isOutOfWarehouse: false, isIntoTemporaryExportProcedure: false
			, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
			, isIntoInwardProcessing: false, isOutOfInwardProcessing: false
			, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false);
			AssertEquals("pre-req", false, mockedInstruction.HasAnyChangeOfOwnershipProcedure);
			AssertEquals("pre-req", ZGuid.Empty, mockedInstruction.CEI_OH_Owner);
			AssertEquals("pre-req", null, mockedInstruction.OldOwner);
			AssertEquals("HasAnyChangeOfOwnershipProcedure should be false", false, mockedInstruction.HasAnyChangeOfOwnershipProcedure);

			var owner1 = Factory.NewWithValidTestData<OrgHeader>();
			var owner2 = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = owner1.PK;
			mockedInstruction.CEI_OH_Owner = owner2.PK;

			mockedInstruction.SetPropertiesForTest(isIntoWarehouse: false, isOutOfWarehouse: false, isIntoTemporaryExportProcedure: false
			, isIntoOutwardProcessing: false, isOutOfOutwardProcessing: false
			, isIntoInwardProcessing: true, isOutOfInwardProcessing: true
			, isIntoTemporaryImportProcedure: false, isOutOfTemporaryImportProcedure: false);
			AssertEquals("pre-req", true, mockedInstruction.HasAnyChangeOfOwnershipProcedure);
			AssertEquals("pre-req", owner2.PK, mockedInstruction.CEI_OH_Owner);
			AssertEquals("pre-req", owner1.PK, mockedInstruction.OldOwner.PK);
			AssertEquals("HasAnyChangeOfOwnershipProcedure should be true", true, mockedInstruction.HasAnyChangeOfOwnershipProcedure);
		}

		public void TestCurrentLocationFor523ForNonFSDs()
		{
			var (instructionForTest, owner) = SetupOwnerAndAuthorisations();

			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: false
				, isOutOfInwardProcessing: false
				, isOutOfOutwardProcessing: false
				, isOutOfTemporaryImportProcedure: false
				, "AABBCCDD", "not out of warehouse, not out of IP, not out of OP, not out of TA");

			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: true
				, isOutOfInwardProcessing: false
				, isOutOfOutwardProcessing: false
				, isOutOfTemporaryImportProcedure: false
				, "", "No Warehouse: out of warehouse, not out of IP, not out of OP, not out of TA");

			var fromWarehouse = Factory.NewWithValidTestData<OrgHeader>();
			var fromWarehouseAddress = fromWarehouse.Addresses.AddNew();
			fromWarehouseAddress.Address1 = "Address1";
			instructionForTest.CEI_OA_Warehouse = fromWarehouseAddress.PK;

			var toWarehouse = Factory.NewWithValidTestData<OrgHeader>();
			var toWarehouseAddress = toWarehouse.Addresses.AddNew();
			toWarehouseAddress.Address1 = "Address2";
			instructionForTest.CEI_OA_Warehouse2 = toWarehouseAddress.PK;

			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: true
				, isOutOfInwardProcessing: false
				, isOutOfOutwardProcessing: false
				, isOutOfTemporaryImportProcedure: false
				, "GBBYCW", "No warehouse code: out of warehouse, not out of IP, not out of OP, not out of TA");

			var cusCode1 = fromWarehouse.CustomsCodes.AddNew();
			cusCode1.OK_CustomsRegNo = "WAREHOUSE1";
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
			cusCode1.OK_CodeType = "CCP";
			cusCode1.OK_OA_PremisesAddress = fromWarehouseAddress.PK;

			var cusCode2 = toWarehouse.CustomsCodes.AddNew();
			cusCode2.OK_CustomsRegNo = "WAREHOUSE2";
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
			cusCode2.OK_CodeType = "CCP";
			cusCode2.OK_OA_PremisesAddress = toWarehouseAddress.PK;

			var warehouseAuthorization = instructionForTest.CusAuthorizationUsages.AddNew();
			warehouseAuthorization.AGC_OH_Owner = owner.PK;
			warehouseAuthorization.AGC_Number = "12345678";

			warehouseAuthorization.AGC_Code = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: true
				, isOutOfInwardProcessing: false
				, isOutOfOutwardProcessing: false
				, isOutOfTemporaryImportProcedure: false
				, "GBBYCWWAREHOUSE1", "Warehouse code set: out of warehouse (TST), not out of IP, not out of OP, not out of TA");

			warehouseAuthorization.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: true
				, isOutOfInwardProcessing: false
				, isOutOfOutwardProcessing: false
				, isOutOfTemporaryImportProcedure: false
				, "GBBYCWWAREHOUSE1", "Warehouse code set: out of warehouse (CW1), not out of IP, not out of OP, not out of TA");

			warehouseAuthorization.AGC_Code = CusAuthorizationHeaderTypeList.Codes.ExciseWarehouse;
			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: true
				, isOutOfInwardProcessing: false
				, isOutOfOutwardProcessing: false
				, isOutOfTemporaryImportProcedure: false
				, "GBBYEXWWAREHOUSE1", "Warehouse code set: out of warehouse (EXW), not out of IP, not out of OP, not out of TA");

			warehouseAuthorization.AGC_Code = CusAuthorizationHeaderTypeList.Codes.ExciseWarehouseHydrocarbonOils;
			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: true
				, isOutOfInwardProcessing: false
				, isOutOfOutwardProcessing: false
				, isOutOfTemporaryImportProcedure: false
				, "GBBYEXWHWAREHOUSE1", "Warehouse code set: out of warehouse (EXWH), not out of IP, not out of OP, not out of TA");

			warehouseAuthorization.AGC_Code = "FZ";
			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: true
				, isOutOfInwardProcessing: false
				, isOutOfOutwardProcessing: false
				, isOutOfTemporaryImportProcedure: false
				, "GBBYFZWAREHOUSE1", "Warehouse code set: out of warehouse (FX), not out of IP, not out of OP, not out of TA");

			var toWarehouseAuthorization = instructionForTest.CusAuthorizationUsages.AddNew();
			toWarehouseAuthorization.AGC_OH_Owner = toWarehouse.PK;
			toWarehouseAuthorization.AGC_Code = CusAuthorizationHeaderTypeList.Codes.ExciseWarehouse;
			toWarehouseAuthorization.AGC_Number = "34567890";

			warehouseAuthorization.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: true
				, isOutOfInwardProcessing: false
				, isOutOfOutwardProcessing: false
				, isOutOfTemporaryImportProcedure: false
				, "GBBYCWWAREHOUSE1", "Warehouse code set: out of warehouse (CW1), not out of IP, not out of OP, not out of TA");

			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: false
				, isOutOfInwardProcessing: true
				, isOutOfOutwardProcessing: false
				, isOutOfTemporaryImportProcedure: false
				, "AABBCCDD", "No IPO authorisation: not out of warehouse, out of IP, not out of OP, not out of TA");

			CreateCusAuthorisationHeader("AUTHIPO", CusAuthorizationHeaderTypeList.Codes.InwardProcessing, owner);
			var usageIPO = instructionForTest.CusAuthorizationUsages[0];
			AssertEquals("pre-req usageIPO.RelatedAuthorisationHeaderIgnoringReferenceNumber.CPH_Number", "AUTHIPO", usageIPO.RelatedAuthorisationHeaderIgnoringReferenceNumber.CPH_Number);

			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: false
				, isOutOfInwardProcessing: true
				, isOutOfOutwardProcessing: false
				, isOutOfTemporaryImportProcedure: false
				, "GBBYIPAUTHIPO", "Has IPO authorisation: not out of warehouse, out of IP, not out of OP, not out of TA");

			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: false
				, isOutOfInwardProcessing: false
				, isOutOfOutwardProcessing: true
				, isOutOfTemporaryImportProcedure: false
				, "AABBCCDD", "No OPO authorisation: not out of warehouse, not out of IP, out of OP, not out of TA");

			CreateCusAuthorisationHeader("AUTHOPO", CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, owner);
			var usageOPO = instructionForTest.CusAuthorizationUsages[1];
			AssertEquals("pre-req usageOPO.RelatedAuthorisationHeaderIgnoringReferenceNumber.CPH_Number", "AUTHOPO", usageOPO.RelatedAuthorisationHeaderIgnoringReferenceNumber.CPH_Number);

			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: false
				, isOutOfInwardProcessing: false
				, isOutOfOutwardProcessing: true
				, isOutOfTemporaryImportProcedure: false
				, "GBBYOPAUTHOPO", "Has OPO authorisation: not out of warehouse, not out of IP, out of OP, not out of TA");

			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: false
				, isOutOfInwardProcessing: false
				, isOutOfOutwardProcessing: false
				, isOutOfTemporaryImportProcedure: true
				, "AABBCCDD", "No TEA authorisation: not out of warehouse, not out of IP, not out of OP, out of TA");

			CreateCusAuthorisationHeader("AUTHTEA", CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission, owner);
			var usageTEA = instructionForTest.CusAuthorizationUsages[2];
			AssertEquals("pre-req usageTEA.RelatedAuthorisationHeaderIgnoringReferenceNumber.CPH_Number", "AUTHTEA", usageTEA.RelatedAuthorisationHeaderIgnoringReferenceNumber.CPH_Number);

			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: false
				, isOutOfInwardProcessing: false
				, isOutOfOutwardProcessing: false
				, isOutOfTemporaryImportProcedure: true
				, "GBBYTAAUTHTEA", "Has TEA authorisation: not out of warehouse, not out of IP, not out of OP, out of TA");
		}

		public void TestCurrentLocationFor523ForNonFSDs_Arrived()
		{
			var (instructionForTest, owner) = SetupOwnerAndAuthorisations();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instructionForTest.CEI_SubStyle = EntrySubStyleListImport.Codes.NormalFullAndWrdDeclarationGoodsArrived;

			CreateCusAuthorisationHeader("AUTHIPO", CusAuthorizationHeaderTypeList.Codes.InwardProcessing, owner);
			CreateCusAuthorisationHeader("AUTHOPO", CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, owner);
			CreateCusAuthorisationHeader("AUTHTEA", CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission, owner);

			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: false
				, isOutOfInwardProcessing: true
				, isOutOfOutwardProcessing: false
				, isOutOfTemporaryImportProcedure: false
				, "GBBYIPAUTHIPO", "Arrived import xx51xxxxx");
			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: false
				, isOutOfInwardProcessing: false
				, isOutOfOutwardProcessing: true
				, isOutOfTemporaryImportProcedure: false
				, "GBBYOPAUTHOPO", "Arrived import xx21xxxxx");
			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: false
				, isOutOfInwardProcessing: false
				, isOutOfOutwardProcessing: false
				, isOutOfTemporaryImportProcedure: true
				, "GBBYTAAUTHTEA", "Arrived import xx53xxxxx");

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			instructionForTest.CEI_Style = EntryStyleListExport.Codes.ExportNormal;
			instructionForTest.CEI_SubStyle = EntrySubStyleListExport.Codes.FullDeclarationGoodsArrived_IEFD;

			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: false
				, isOutOfInwardProcessing: true
				, isOutOfOutwardProcessing: false
				, isOutOfTemporaryImportProcedure: false
				, "AABBCCDD", "Arrived export xx51xxxxx");
			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: false
				, isOutOfInwardProcessing: false
				, isOutOfOutwardProcessing: true
				, isOutOfTemporaryImportProcedure: false
				, "AABBCCDD", "Arrived export xx21xxxxx");
			SetupAndAssertCurrentLocationFor523(instructionForTest
				, isOutOfWarehouse: false
				, isOutOfInwardProcessing: false
				, isOutOfOutwardProcessing: false
				, isOutOfTemporaryImportProcedure: true
				, "AABBCCDD", "Arrived export xx53xxxxx");
		}

		(CusEntryInstructionForTest, OrgHeader) SetupOwnerAndAuthorisations()
		{
			var instructionForTest = Factory.NewWithValidTestData<CusEntryInstructionForTest>();
			declaration.CustomsEntryInstructions.Add(instructionForTest);

			var owner = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_MessageType = "IMP";
			instructionForTest.CEI_Style = EntryStyleListImport.Codes.ImportNormal;
			instructionForTest.CEI_SubStyle = EntrySubStyleListImport.Codes.NormalFullDeclarationGoodsNotArrived; //Not FSD

			var usageIPO = instructionForTest.CusAuthorizationUsages.AddNew();
			usageIPO.AGC_OH_Owner = owner.PK;
			usageIPO.AGC_Code = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			usageIPO.AGC_Number = "IPO12345";
			var usageOPO = instructionForTest.CusAuthorizationUsages.AddNew();
			usageOPO.AGC_OH_Owner = owner.PK;
			usageOPO.AGC_Code = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			usageOPO.AGC_Number = "OPO12345";
			var usageTEA = instructionForTest.CusAuthorizationUsages.AddNew();
			usageTEA.AGC_OH_Owner = owner.PK;
			usageTEA.AGC_Code = CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission;
			usageTEA.AGC_Number = "TEA12345";

			SetupDefaultFullLocationOfGoods(declaration);

			AssertEquals("pre-req FullLocationOfGoods", "AABBCCDD", declaration.FullLocationOfGoods);

			return (instructionForTest, owner);
		}

		void CreateCusAuthorisationHeader(ZString number, ZString type, OrgHeader owner)
		{
			var authHeaderOPO = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authHeaderOPO.CPH_Number = number;
			authHeaderOPO.CPH_Type = type;
			authHeaderOPO.CPH_OH_PermitHolder = owner.PK;
		}

		void SetupAndAssertCurrentLocationFor523(CusEntryInstructionForTest instructionForTest
			, bool isOutOfWarehouse
			, bool isOutOfInwardProcessing
			, bool isOutOfOutwardProcessing
			, bool isOutOfTemporaryImportProcedure
			, string assertionExpected, string assertionMessage)
		{
			instructionForTest.SetPropertiesForTest(isOutOfWarehouse, isOutOfInwardProcessing, isOutOfOutwardProcessing, isOutOfTemporaryImportProcedure);
			AssertEquals("pre-req HasOutOfWarehouseProcedure", isOutOfWarehouse, instructionForTest.HasOutOfWarehouseProcedure);
			AssertEquals("pre-req HasOutOfInwardProcessingProcedure", isOutOfInwardProcessing, instructionForTest.HasOutOfInwardProcessingProcedure);
			AssertEquals("pre-req HasOutOfOutwardProcessingProcedure", isOutOfOutwardProcessing, instructionForTest.HasOutOfOutwardProcessingProcedure);
			AssertEquals("pre-req HasOutOfTemporaryImportProcedure", isOutOfTemporaryImportProcedure, instructionForTest.HasOutOfTemporaryImportProcedure);

			AssertEquals(assertionMessage, assertionExpected, instructionForTest.CurrentLocationFor523);
		}

		void SetupDefaultFullLocationOfGoods(JobDeclaration dec)
		{
			dec.JE_Calc_LocationOtherInformationCountry = "AA";
			dec.JE_Calc_LocationOtherInformationType = "BB";
			dec.JE_LocationQualifier = "CC";
			dec.JE_GoodsLocation = "DD";
		}

		public void TestCurrentLocationFor523_ExportFromWarehouse()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var instructionForTest = Factory.NewWithValidTestData<CusEntryInstructionForTest>();
			instructionForTest.SetPropertiesForTest(isOutOfWarehouse: true);
			declaration.CustomsEntryInstructions.Add(instructionForTest);
			SetupDefaultFullLocationOfGoods(declaration);

			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			var warehouseAddress1 = warehouse.Addresses.AddNew();
			warehouseAddress1.Address1 = "Address1";
			var cusCode1 = warehouse.CustomsCodes.AddNew();
			cusCode1.OK_CustomsRegNo = "WAREHOUSE1";
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			cusCode1.OK_OA_PremisesAddress = warehouseAddress1.PK;
			instructionForTest.CEI_OA_Warehouse = warehouseAddress1.PK;

			instructionForTest.CEI_SubStyle = EntrySubStyleListExport.Codes.FullDeclarationGoodsNotArrived_IEFD;
			AssertEquals("Prelodged", "GBBYCWWAREHOUSE1", instructionForTest.CurrentLocationFor523);

			instructionForTest.CEI_SubStyle = EntrySubStyleListExport.Codes.FullDeclarationGoodsArrived_IEFD;
			AssertEquals("Arrived", "AABBCCDD", instructionForTest.CurrentLocationFor523);
		}

		public void TestCurrentLocationFor523_FSD()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_MessageType = "IMP";
			var line1 = declaration.InvoiceLines.AddNew();
			line1.JI_Procedure = JobComInvoiceLine.CfspFsdCPCCode;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = EntryStyleListImport.Codes.ImportNormal;
			instruction.CEI_SubStyle = EntrySubStyleListImport.Codes.FinalSupplementaryDeclaration;
			SetupDefaultFullLocationOfGoods(declaration);
			AssertEquals("AABBCCDD", instruction.CurrentLocationFor523);

			var authHeaderEIR = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authHeaderEIR.CPH_Number = "AUTHEIR";
			authHeaderEIR.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			authHeaderEIR.CPH_OH_PermitHolder = owner.PK;
			var authHeaderSDE = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authHeaderSDE.CPH_Number = "AUTHSDE";
			authHeaderSDE.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			authHeaderSDE.CPH_OH_PermitHolder = owner.PK;

			var usageEIR = instruction.CusAuthorizationUsages.AddNew();
			usageEIR.AGC_OH_Owner = owner.PK;
			usageEIR.AGC_Code = AuthorisationTypeCodes.EIR;
			usageEIR.AGC_Number = "EIR12345";
			AssertEquals("GBBYFSDAUTHEIR", instruction.CurrentLocationFor523);

			var usageSDE = instruction.CusAuthorizationUsages.AddNew();
			usageSDE.AGC_OH_Owner = owner.PK;
			usageSDE.AGC_Code = AuthorisationTypeCodes.SDE;
			usageSDE.AGC_Number = "SDE12345";
			AssertEquals("GBBYFSDAUTHSDE", instruction.CurrentLocationFor523);
		}

		public void TestOutFromWarehouseCode()
		{
			var cei = Factory.New<CusEntryInstruction>();
			WarehouseCodeRunner(cei, () => cei.FromWarehouseCode, (ZGuid oa) => cei.CEI_OA_Warehouse = oa);
		}

		public void TestInToWarehouseCode()
		{
			var cei = Factory.New<CusEntryInstruction>();
			WarehouseCodeRunner(cei, () => cei.ToWarehouseCode, (ZGuid oa) => cei.CEI_OA_Warehouse2 = oa);
		}

		void WarehouseCodeRunner(CusEntryInstruction cei, Func<ZString> getWarehouseCodeResult, Action<ZGuid> assignWarehouse)
		{
			AssertEquals("", cei.ToWarehouseCode);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = org.Addresses.AddNew();
			var address2 = org.Addresses.AddNew();
			var address3 = org.Addresses.AddNew();
			var okOnOrg = org.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U0000");
			var okOnAddress1 = address1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U1111");
			var okOnAddress2 = address2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U2222");
			okOnAddress1.OK_OA_PremisesAddress = address1.PK;
			okOnAddress2.OK_OA_PremisesAddress = address2.PK;
			assignWarehouse(org.MainAddress.PK);
			AssertEquals("U0000", getWarehouseCodeResult());
			assignWarehouse(address1.PK);
			AssertEquals("U1111", getWarehouseCodeResult());
			assignWarehouse(address2.PK);
			AssertEquals("U2222", getWarehouseCodeResult());
			assignWarehouse(address3.PK);
			AssertEquals("no warehouse defined on this address.", "", getWarehouseCodeResult());
		}

		public void TestGoodsOfLocationsWhenSubStyleChanged()
		{
			GBCustomsDataRegistry.Instance.AllowLocationOfGoodsCalculationForNotArrivedGoods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			declaration.JE_MessageType = "EXP";
			declaration.JE_RL_NKPortOfLoading = "GBLHR";
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_SubStyle = "A";
			declaration.JE_GoodsLocation = "HESLHRELX";

			instruction.CEI_SubStyle = "B";
			AssertEquals("HESLHRELX", declaration.JE_GoodsLocation);

			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			instruction.CEI_SubStyle = "A";
			declaration.JE_CHIEF_GoodsLocation = "XXX";

			instruction.CEI_SubStyle = "B";
			AssertEquals("HESLHRELX", declaration.JE_GoodsLocation);
		}

		public void TestDefaultEntrySubStyleWhenStyleChanged()
		{
			declaration.JE_DateOfArrival = ZDateTime.Now.AddHours(-100);
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			declaration.JE_MessageType = "IMP";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "H1";
			AssertEquals("A", instruction.CEI_SubStyle);

			instruction.CEI_Style = "H2";
			AssertEquals("A", instruction.CEI_SubStyle);

			instruction.CEI_Style = "H3";
			AssertEquals("A", instruction.CEI_SubStyle);

			instruction.CEI_Style = "H4";
			AssertEquals("A", instruction.CEI_SubStyle);

			instruction.CEI_Style = "H5";
			AssertEquals("A", instruction.CEI_SubStyle);

			instruction.CEI_Style = "I1";
			AssertEquals("C", instruction.CEI_SubStyle);
		}

		public void TestPackageCount_House()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.ZG_ShipmentType = EU.Business.ShipmentTypeList.Codes.HouseConsignment;
			dec.JE_TotalNoOfPacks = 50;
			var ceh = dec.CustomsEntryHeaders.AddNew();
			dec.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_PackageCount = 30;

			var cei2 = dec.CustomsEntryInstructions.AddNew();
			cei2.CEI_PackageCount = 20;
			var cel = ceh.AllEntryLines.AddNew();
			var cel2 = ceh.AllEntryLines.AddNew();

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_JE_CustomsFormalEntry = dec.PK;
			var split1 = Factory.New<CusPartShip>();
			split1.CG_CS = hawb.PK;
			split1.CG_MessageReference = "01";
			split1.CG_PiecesManifested = 50;
			var split2 = Factory.New<CusPartShip>();
			split2.CG_CS = hawb.PK;
			split2.CG_MessageReference = "02";
			split2.CG_PiecesManifested = 40;
			var invoice = dec.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			invLine.JI_CL = cel.PK;
			invLine.JI_CEI = cei.PK;
			var invLine2 = invoice.InvoiceLines.AddNew();
			invLine2.JI_CL = cel2.PK;
			invLine2.JI_CEI = cei2.PK;

			AssertEquals(30, cei.CEI_PackageCount);
			AssertEquals(20, cei2.CEI_PackageCount);

			cei.CEI_SplitReference = "01";
			cei2.CEI_SplitReference = "02";

			AssertEquals(50, cei.CEI_PackageCount);
			AssertEquals(40, cei2.CEI_PackageCount);
		}

		public void TestPackageCount_Basic()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.ZG_ShipmentType = EU.Business.ShipmentTypeList.Codes.BasicDirect;
			dec.JE_TotalNoOfPacks = 90;
			var ceh = dec.CustomsEntryHeaders.AddNew();
			var ceh2 = dec.CustomsEntryHeaders.AddNew();
			dec.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_PackageCount = 30;
			var cei2 = dec.CustomsEntryInstructions.AddNew();
			cei2.CEI_PackageCount = 20;

			var cel = ceh.AllEntryLines.AddNew();
			var cel2 = ceh2.AllEntryLines.AddNew();

			var basic = Factory.New<CusMAWB>();
			basic.CM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			var workerHawb = basic.ChildBills.AddNew();
			workerHawb.CS_IsMasterHouse = true;
			workerHawb.CS_JE_CustomsFormalEntry = dec.PK;
			var split1 = Factory.New<CusPartShip>();
			split1.CG_CM_LinkToPartMaster = basic.PK;
			split1.CG_MessageReference = "01";
			split1.CG_PiecesManifested = 50;
			var split2 = Factory.New<CusPartShip>();
			split2.CG_CM_LinkToPartMaster = basic.PK;
			split2.CG_MessageReference = "02";
			split2.CG_PiecesManifested = 40;
			var invoice = dec.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			invLine.JI_CL = cel.PK;
			invLine.JI_CEI = cei.PK;
			var invLine2 = invoice.InvoiceLines.AddNew();
			invLine2.JI_CL = cel.PK;
			invLine2.JI_CEI = cei2.PK;

			AssertEquals(30, cei.CEI_PackageCount);
			AssertEquals(20, cei2.CEI_PackageCount);

			cei.CEI_SplitReference = "01";
			cei2.CEI_SplitReference = "02";

			AssertEquals(50, cei.CEI_PackageCount);
			AssertEquals(40, cei2.CEI_PackageCount);
		}

		public void TestCusEntryInstructionSchemaMaxLength()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			AssertEquals(50, CusEntryInstruction.Schema.CEI_DescriptionMaxLength);
			AssertEquals(3, CusEntryInstruction.Schema.CEI_StyleMaxLength);
			AssertEquals(2, CusEntryInstruction.Schema.CEI_SplitReferenceMaxLength);
			AssertEquals(1, CusEntryInstruction.Schema.CEI_SubStyleMaxLength);

			AssertEquals(CusEntryInstruction.Schema.CEI_DescriptionMaxLength, instruction.CEI_DescriptionInfo.MaxLength);
			AssertEquals(CusEntryInstruction.Schema.CEI_StyleMaxLength, instruction.CEI_StyleInfo.MaxLength);
			AssertEquals(CusEntryInstruction.Schema.CEI_SubStyleMaxLength, instruction.CEI_SubStyleInfo.MaxLength);
		}

		public void TestGBLookupsWithoutDeclaration()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			AssertType<CusEntryInstructionLookups>(instruction.Lookups);
		}

		public void TestIsDescriptionDefaultedFromStyle()
		{
			Assert(Factory.New<CEI_WithDescriptionDefaultedFromStyle>().IsDescriptionDefaultedFromStyle_Exposed);
		}

		public void TestCDSIsClearanceRequest()
		{
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			instruction.CEI_Style = "21i";
			Assert("IsClearanceRequest should be true for '21i'", instruction.IsClearanceRequest);

			instruction.CEI_Style = "21I";
			Assert("IsClearanceRequest should be true for '21I'", instruction.IsClearanceRequest);

			instruction.CEI_Style = "21n";
			Assert("IsClearanceRequest should be true for '21n'", instruction.IsClearanceRequest);

			instruction.CEI_Style = "21N";
			Assert("IsClearanceRequest should be true for '21N'", instruction.IsClearanceRequest);

			instruction.CEI_Style = "21e";
			Assert("IsClearanceRequest should be true for '21e'", instruction.IsClearanceRequest);

			instruction.CEI_Style = "21E";
			Assert("IsClearanceRequest should be true for '21E'", instruction.IsClearanceRequest);

			instruction.CEI_Style = "H1";
			Assert("IsClearanceRequest should be false for 'H1'", !instruction.IsClearanceRequest);

			instruction.CEI_Style = "CEN";
			Assert("IsClearanceRequest should be true for 'CEN'", instruction.IsClearanceRequest);
		}

		public void TestDefaultPackageCount()
		{
			declaration.JE_TotalNoOfPacks = 10;
			var cei = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(10, cei.CEI_PackageCount);
			cei.CEI_PackageCount = 8;
			var cei2 = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(2, cei2.CEI_PackageCount);
			declaration.JE_TotalNoOfPacks = 7;
			var cei3 = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(0, cei3.CEI_PackageCount);
		}

		protected class CEI_WithDescriptionDefaultedFromStyle : CusEntryInstruction
		{
			public CEI_WithDescriptionDefaultedFromStyle(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool IsDescriptionDefaultedFromStyle_Exposed => base.IsDescriptionDefaultedFromStyle;
		}

		public void TestCEI_StyleAndSubStyleReadOnly()
		{
			declaration.JE_ApplicationCode = "CDS";
			var cei1 = declaration.CustomsEntryInstructions.AddNew();
			cei1.CEI_Style = "H1";
			cei1.CEI_SubStyle = "A";
			var cei2 = declaration.CustomsEntryInstructions.AddNew();
			cei2.CEI_Style = "H2";
			cei2.CEI_SubStyle = "D";
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = cei1.PK;
			entryHeader1.CH_EntryStatus = "CLE";
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_CEI_Instruction = cei2.PK;

			AssertStyleAndSubStyle(true, cei1);
			AssertStyleAndSubStyle(false, cei2);

			entryHeader2.CH_EntryStatus = "ACC";
			AssertStyleAndSubStyle(true, cei1);
			AssertStyleAndSubStyle(true, cei2);

			entryHeader2.CH_EntryStatus = "REJ";
			AssertStyleAndSubStyle(true, cei1);
			AssertStyleAndSubStyle(false, cei2);

			entryHeader1.CH_EntryStatus = "REJ";
			AssertStyleAndSubStyle(false, cei1);
			AssertStyleAndSubStyle(false, cei2);

			declaration.JE_ApplicationCode = "CHF";
			AssertStyleAndSubStyle(false, cei1);
			AssertStyleAndSubStyle(false, cei2);
		}

		static void AssertStyleAndSubStyle(bool expectedReadOnly, CusEntryInstruction entryInstruction)
		{
			AssertEquals(expectedReadOnly, entryInstruction.CEI_StyleInfo.ReadOnly);
			AssertEquals(expectedReadOnly, entryInstruction.CEI_SubStyleInfo.ReadOnly);
		}

		public void TestCEI_DisplaySequence_Caption()
		{
			AssertEquals("Display Sequence", DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), nameof(CusEntryInstruction.CEI_DisplaySequence)).Caption);
		}

		public void TestCodePropertyAttribute()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "21E";
			AssertEquals("21E", CodePropertyAttribute.CodeFromBusinessObject(instruction));
		}

		public void TestDescriptionPropertyAttribute()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Description = "desc";
			AssertEquals("desc (1)", DescriptionPropertyAttribute.DescriptionFromBusinessObject(instruction));
		}

		public void TestCalculatedDescription()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				instruction.CEI_Description = "desc";
				AssertEquals("CalculatedDescription", "desc (1)", instruction.CalculatedDescription);

				instruction.CEI_Description = ZString.Empty;
				AssertEquals("CEI_Description is empty", "(1)", instruction.CalculatedDescription);
			});
		}

		public void TestReAssigningLineNumbers()
		{
			CombineAssertions(() =>
			{
				var instruction1 = declaration.CustomsEntryInstructions.AddNew();
				var instruction2 = declaration.CustomsEntryInstructions.AddNew();
				var instruction3 = declaration.CustomsEntryInstructions.AddNew();

				AssertEquals("Instruction1 Added", (short)1, instruction1.CEI_DisplaySequence);
				AssertEquals("Instruction2 Added", (short)2, instruction2.CEI_DisplaySequence);
				AssertEquals("Instruction3 Added", (short)3, instruction3.CEI_DisplaySequence);

				instruction3.CEI_DisplaySequence = 2;
				AssertEquals("Assign smaller value to Instruction3|Instruction1", (short)1, instruction1.CEI_DisplaySequence);
				AssertEquals("Assign smaller value to Instruction3|Instruction2", (short)3, instruction2.CEI_DisplaySequence);
				AssertEquals("Assign smaller value to Instruction3|Instruction3", (short)2, instruction3.CEI_DisplaySequence);

				instruction3.CEI_DisplaySequence = 8;
				AssertEquals("Assign bigger value to Instruction3|Instruction1", (short)1, instruction1.CEI_DisplaySequence);
				AssertEquals("Assign bigger value to Instruction3|Instruction2", (short)2, instruction2.CEI_DisplaySequence);
				AssertEquals("Assign bigger value to Instruction3|Instruction3", (short)3, instruction3.CEI_DisplaySequence);

				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				instruction2.CEI_JE = declaration2.PK;
				AssertEquals("Link Instruction 2 to another job|Instruction1", (short)1, instruction1.CEI_DisplaySequence);
				AssertEquals("Link Instruction 2 to another job|Instruction3", (short)2, instruction3.CEI_DisplaySequence);

				instruction1.Delete();
				AssertEquals("Instruction 1 removed and Instruction 3 remains only", (short)1, instruction3.CEI_DisplaySequence);
			});
		}

		public void TestIsPostponedVatViaFiscalReference()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var reference = instruction.FiscalReferences.AddNew();
			AssertEquals(false, instruction.IsPostponedVatViaFiscalReference);
			reference.CFR_Code = EU.Business.FiscalReferenceCodeList.Codes.FR1_Importer;
			reference.CFR_Reference = "AB123";
			AssertEquals(true, instruction.IsPostponedVatViaFiscalReference);
		}

		public void TestIsPostponedVatViaFiscalReference_Caption()
		{
			var data = DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryInstruction), CusEntryInstruction.Schema.IsPostponedVatViaFiscalReference);
			AssertEquals("PVA", data.ShortCaption);
			AssertEquals("Postponed VAT Accounting", data.Caption);
			AssertEquals("The presence of an FR1 fiscal reference for this instruction indicates that Postponed VAT Accounting (PVA) is in use", data.FullDescription);
		}

		public void TestPVAFiscalReference()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var reference = instruction.FiscalReferences.AddNew();
			reference.CFR_Code = EU.Business.FiscalReferenceCodeList.Codes.FR2_Customer;
			reference.CFR_Reference = "AB123";
			AssertEquals("NO PVA - should be blank", ZString.Empty, instruction.PVAFiscalReference);
			var reference2 = instruction.FiscalReferences.AddNew();
			reference2.CFR_Code = EU.Business.FiscalReferenceCodeList.Codes.FR1_Importer;
			reference2.CFR_Reference = "ZZ999";
			AssertEquals("PVA Found - ZZ999", "ZZ999", instruction.PVAFiscalReference);
		}

		public void TestCusAuthorisationChangeCreatesSupportingDocs()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("GB");
			helper.CreateNewOrGetExistingCusCodeType("DC44I", "Supporting Docs", "GB");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("AuthCode", "AuthCode", "DC44I", "GB");
			var doc1 = helper.CreateNewOrGetExistingCusCodeList("GB", "DC44I", "DEF", "Sup doc1 with AuthCode", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var doc2 = helper.CreateNewOrGetExistingCusCodeList("GB", "DC44I", "EFG", "Sup doc2 with AuthCode", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("GB", "DC44I", "ZZZ", "Sup doc with no AuthCode", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(doc1.PK, "AuthCode", "ABC");
			helper.CreateNewOrGetExistingCusCodeListAttribute(doc2.PK, "AuthCode", "ABC");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var permitHolder = ZGuid.NewZGuid();
				var header1 = Factory.New<CusAuthorisationHeader>();
				header1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
				header1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
				header1.CPH_IsActive = true;
				header1.CPH_Type = "ABC";
				header1.CPH_Number = "ABC1234567";
				header1.CPH_OH_PermitHolder = permitHolder;
				header1.CPH_StartDate = ZDate.Today;

				var header2 = Factory.New<CusAuthorisationHeader>();
				header2.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
				header2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
				header2.CPH_IsActive = true;
				header2.CPH_Type = "XYZ";
				header2.CPH_Number = "XYZ1234567";
				header2.CPH_OH_PermitHolder = permitHolder;
				header2.CPH_StartDate = ZDate.Today;

				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = "IMP";
				var cei = dec.CustomsEntryInstructions.AddNew();
				var cei2 = dec.CustomsEntryInstructions.AddNew();
				var inv = dec.Invoices.AddNew();
				var line1 = inv.InvoiceLines.AddNew();
				var line2 = inv.InvoiceLines.AddNew();
				var line3 = inv.InvoiceLines.AddNew();
				var line4 = inv.InvoiceLines.AddNew();

				line1.JI_CEI = cei.PK;
				line2.JI_CEI = cei.PK;
				line3.JI_CEI = cei.PK;
				line4.JI_CEI = cei2.PK;

				var existingDoc = line2.SupportingDocuments.AddNew();
				existingDoc.SetPropertiesFromCusAuthorisationHeader(header2, "123");

				var usage = cei.CusAuthorizationUsages.AddNew();

				usage.AGC_OH_Owner = permitHolder;
				usage.AGC_Code = "XYZ";

				CombineAssertions("No doc with AuthCode XYZ", () =>
				{
					AssertEquals("Line 1 not created", 0, line1.SupportingDocuments.Count);
					AssertEquals("Line 2 not changed", 1, line2.SupportingDocuments.Count);
					AssertEquals("Line 2 CSI_Code", "123", line2.SupportingDocuments[0].CSI_Code);
					AssertEquals("Line 2 CSI_Ref", "GBXYZXYZ1234567", line2.SupportingDocuments[0].CSI_ReferenceNumber);
					AssertEquals("Line 3 not created", 0, line3.SupportingDocuments.Count);
					AssertEquals("Line 4 not created", 0, line4.SupportingDocuments.Count);
				});
			}
		}

		public void TestCEI_DateForDuty()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			AssertEquals(0, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Allow to change without entry header", false, instruction.CEI_DateForDutyInfo.ReadOnly);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			AssertEquals(true, entryHeader.DateOfLegalAcceptance.IsEmpty);
			AssertEquals("Allow to change before getting ACC response", false, instruction.CEI_DateForDutyInfo.ReadOnly);

			entryHeader.Logs.AddNew(Events.CustomsEntryStatus, CDS.Constants.ThreeCharFunctionCodes.DeclarationAccepted);
			AssertEquals(false, entryHeader.DateOfLegalAcceptance.IsEmpty);
			AssertEquals("Not allow to change after getting ACC response", true, instruction.CEI_DateForDutyInfo.ReadOnly);

			declaration.MarkApportionmentDirty();
			AssertEquals("Not yet run apportionment", true, declaration.ApportionmentDirty);
			declaration.CusEntryInstruction.CEI_DateForDuty = ZDateTime.Now;
			AssertEquals("Have run apportionment", false, declaration.ApportionmentDirty);
		}

		public override void TestIsChangeOfOwnershipWarehousing()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			owner.OH_Code = "AAAA";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;

			invoiceLine1.JI_Procedure = GetInvoiceProcedure();

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;

			var helper = new WhsDataTestHelper(Factory);
			entryInstruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;

			AssertEquals("Prerequisite: IsChangeOfOwnershipWarehousingEnabled should be false.", false, entryInstruction.IsChangeOfOwnershipWarehousingEnabled);
			AssertEquals("Prerequisite: there should not be any invoice line with IsChangeOfOwnershipWarehousing.", false, entryInstruction.InvoiceLines.Any(x => x.IsChangeOfOwnershipWarehousing));
			AssertEquals("IsChangeOfOwnershipWarehousing should be false because IsChangeOfOwnershipWarehousingEnabled is false and no invoice line has IsChangeOfOwnershipWarehousing set to true.", false, entryInstruction.IsChangeOfOwnershipWarehousing);

			entryInstruction.CEI_OH_Owner = ZGuid.BrettsGuid;
			AssertEquals("Prerequisite: IsChangeOfOwnershipWarehousingEnabled should be true.", true, entryInstruction.IsChangeOfOwnershipWarehousingEnabled);
			AssertEquals("Prerequisite: there should not be any invoice line with IsChangeOfOwnershipWarehousing.", false, entryInstruction.InvoiceLines.Any(x => x.IsChangeOfOwnershipWarehousing));
			AssertEquals("IsChangeOfOwnershipWarehousing should be false because no invoice line has IsChangeOfOwnershipWarehousing set to true.", false, entryInstruction.IsChangeOfOwnershipWarehousing);

			entryInstruction.CEI_OH_Owner = ZGuid.Invalid;
			helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals("Prerequisite: IsChangeOfOwnershipWarehousingEnabled should be false.", false, entryInstruction.IsChangeOfOwnershipWarehousingEnabled);
			AssertEquals("Prerequisite: there should be at least one invoice line with IsChangeOfOwnershipWarehousing.", true, entryInstruction.InvoiceLines.Any(x => x.IsChangeOfOwnershipWarehousing));
			AssertEquals("IsChangeOfOwnershipWarehousing should be false because IsChangeOfOwnershipWarehousingEnabled is false.", false, entryInstruction.IsChangeOfOwnershipWarehousing);

			entryInstruction.CEI_OH_Owner = ZGuid.BrettsGuid;
			AssertEquals("Prerequisite: IsChangeOfOwnershipWarehousingEnabled should be true.", true, entryInstruction.IsChangeOfOwnershipWarehousingEnabled);
			AssertEquals("Prerequisite: there should be at least one invoice line with IsChangeOfOwnershipWarehousing.", true, entryInstruction.InvoiceLines.Any(x => x.IsChangeOfOwnershipWarehousing));
			AssertEquals("IsChangeOfOwnershipWarehousing should be true because IsChangeOfOwnershipWarehousing is true and at least one invoice line has IsChangeOfOwnershipWarehousing set to true.", true, entryInstruction.IsChangeOfOwnershipWarehousing);
		}

		public void TestIsGoodsNotArrivedSubStyle()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_SubStyle = EntrySubStyleListExport.Codes.C21_GoodsArrived_IECR;
			Assert(!instruction.IsGoodsNotArrivedSubStyle);
			instruction.CEI_SubStyle = EntrySubStyleListExport.Codes.C21_GoodsNotArrived_IECR;
			Assert(instruction.IsGoodsNotArrivedSubStyle);
			instruction.CEI_SubStyle = EntrySubStyleListExport.Codes.FullDeclarationGoodsArrived_IEFD;
			Assert(!instruction.IsGoodsNotArrivedSubStyle);
			instruction.CEI_SubStyle = EntrySubStyleListExport.Codes.FullDeclarationGoodsNotArrived_IEFD;
			Assert(instruction.IsGoodsNotArrivedSubStyle);
			instruction.CEI_SubStyle = EntrySubStyleCodeList.Codes.B;
			Assert(!instruction.IsGoodsNotArrivedSubStyle);
			instruction.CEI_SubStyle = EntrySubStyleCodeList.Codes.E;
			Assert(instruction.IsGoodsNotArrivedSubStyle);
			instruction.CEI_SubStyle = EntrySubStyleListExport.Codes.SDP_PSA_GoodsArrived_IESP;
			Assert(!instruction.IsGoodsNotArrivedSubStyle);
			instruction.CEI_SubStyle = EntrySubStyleListExport.Codes.SDP_and_LCP_PSA_GoodsNotArrived_IELP_IESP;
			Assert(instruction.IsGoodsNotArrivedSubStyle);
			instruction.CEI_SubStyle = EntrySubStyleListImport.Codes.TransitSfdGoodsArrived;
			Assert(!instruction.IsGoodsNotArrivedSubStyle);
			instruction.CEI_SubStyle = EntrySubStyleListImport.Codes.TransitSfdGoodsNotArrived;
			Assert(instruction.IsGoodsNotArrivedSubStyle);
		}

		public void TestConvertNotArrivedSubStyleToArrived()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_SubStyle = EntrySubStyleListExport.Codes.C21_GoodsNotArrived_IECR;
			instruction.ChangeSubStyleFromNotArrivedToArrived();
			AssertEquals(EntrySubStyleListExport.Codes.C21_GoodsArrived_IECR, instruction.CEI_SubStyle);

			instruction.CEI_SubStyle = EntrySubStyleListExport.Codes.FullDeclarationGoodsNotArrived_IEFD;
			instruction.ChangeSubStyleFromNotArrivedToArrived();
			AssertEquals(EntrySubStyleListExport.Codes.FullDeclarationGoodsArrived_IEFD, instruction.CEI_SubStyle);

			instruction.CEI_SubStyle = EntrySubStyleCodeList.Codes.E;
			instruction.ChangeSubStyleFromNotArrivedToArrived();
			AssertEquals(EntrySubStyleCodeList.Codes.B, instruction.CEI_SubStyle);

			instruction.CEI_SubStyle = EntrySubStyleListExport.Codes.SDP_and_LCP_PSA_GoodsNotArrived_IELP_IESP;
			instruction.ChangeSubStyleFromNotArrivedToArrived();
			AssertEquals(EntrySubStyleListExport.Codes.SDP_PSA_GoodsArrived_IESP, instruction.CEI_SubStyle);

			instruction.CEI_SubStyle = EntrySubStyleListImport.Codes.TransitSfdGoodsNotArrived;
			instruction.ChangeSubStyleFromNotArrivedToArrived();
			AssertEquals(EntrySubStyleListImport.Codes.TransitSfdGoodsArrived, instruction.CEI_SubStyle);

			instruction.CEI_SubStyle = EntrySubStyleListExport.Codes.FullDeclarationGoodsArrived_IEFD;
			instruction.ChangeSubStyleFromNotArrivedToArrived();
			AssertEquals(EntrySubStyleListExport.Codes.FullDeclarationGoodsArrived_IEFD, instruction.CEI_SubStyle);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		}

		JobDeclaration declaration;
	}

	class CusEntryInstructionForTest : CusEntryInstruction
	{
		public CusEntryInstructionForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public void SetPropertiesForTest(bool isOutOfWarehouse = false, bool isOutOfInwardProcessing = false, bool isOutOfOutwardProcessing = false, bool isOutOfTemporaryImportProcedure = false)
		{
			this.isOutOfWarehouse = isOutOfWarehouse;
			this.isOutOfInwardProcessing = isOutOfInwardProcessing;
			this.isOutOfOutwardProcessing = isOutOfOutwardProcessing;
			this.isOutOfTemporaryImportProcedure = isOutOfTemporaryImportProcedure;
		}

		public void SetPropertiesForTest(bool isIntoTemporaryImportProcedure, bool isOutOfTemporaryImportProcedure
			, bool isIntoTemporaryExportProcedure
			, bool isIntoWarehouse, bool isOutOfWarehouse
			, bool isIntoOutwardProcessing, bool isOutOfOutwardProcessing
			, bool isIntoInwardProcessing, bool isOutOfInwardProcessing)
		{
			this.isIntoTemporaryImportProcedure = isIntoTemporaryImportProcedure;
			this.isOutOfTemporaryImportProcedure = isOutOfTemporaryImportProcedure;
			this.isIntoTemporaryExportProcedure = isIntoTemporaryExportProcedure;
			this.isIntoWarehouse = isIntoWarehouse;
			this.isOutOfWarehouse = isOutOfWarehouse;
			this.isIntoOutwardProcessing = isIntoOutwardProcessing;
			this.isOutOfOutwardProcessing = isOutOfOutwardProcessing;
			this.isIntoInwardProcessing = isIntoInwardProcessing;
			this.isOutOfInwardProcessing = isOutOfInwardProcessing;
		}

		protected override bool HasIntoTemporaryImportProcedureCore => isIntoTemporaryImportProcedure;
		protected override bool HasOutOfTemporaryImportProcedureCore => isOutOfTemporaryImportProcedure;
		protected override bool HasIntoTemporaryExportProcedureCore => isIntoTemporaryExportProcedure;
		protected override bool HasIntoWarehouseProcedureCore => isIntoWarehouse;
		protected override bool HasOutOfWarehouseProcedureCore => isOutOfWarehouse;
		protected override bool HasIntoOutwardProcessingProcedureCore => isIntoOutwardProcessing;
		protected override bool HasOutOfOutwardProcessingProcedureCore => isOutOfOutwardProcessing;
		protected override bool HasIntoInwardProcessingProcedureCore => isIntoInwardProcessing;
		protected override bool HasOutOfInwardProcessingProcedureCore => isOutOfInwardProcessing;

		bool isIntoTemporaryImportProcedure;
		bool isOutOfTemporaryImportProcedure;
		bool isIntoTemporaryExportProcedure;
		bool isIntoWarehouse;
		bool isOutOfWarehouse;
		bool isIntoOutwardProcessing;
		bool isOutOfOutwardProcessing;
		bool isIntoInwardProcessing;
		bool isOutOfInwardProcessing;
	}
}
