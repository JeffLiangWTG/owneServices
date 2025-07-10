using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using CusProcedure = Enterprise.Core.Constants.Customs.Universal.RefCusProcedure.Codes;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class CusEntryInstructionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCEI_Procedure()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "CPC1", "11", "111", "CPC1 Desc", "IMP", group: "H1,H2");

			helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "Procedure Code");
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "CPC1", "CPC1 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var dec = Factory.New<JobDeclarationForTestOnly_RequestedProcedure>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(dec, true))
			{
				dec.JE_MessageType = "IMP";
				var instruction = dec.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = "H1";
				instruction.CEI_Procedure = "";

				var validation = instruction.Validation;
				var info = instruction.CEI_ProcedureInfo;
				validation.ValidateCEI_Procedure();
				AssertHasMessageErrorContaining("CEI_Procedure is mandatory", info, "You have not entered");

				instruction.CEI_Procedure = "XXX";
				validation.ValidateCEI_Procedure();
				AssertListValidationInvalidCodeMessageError(info, true);

				instruction.CEI_Procedure = "CPC1";
				validation.ValidateCEI_Procedure();
				AssertListValidationInvalidCodeMessageError(info, false);
				AssertNoMessageErrorContaining("CEI_Procedure is mandatory", info, "You have not entered");
			}
		}

		public void TestValidateInvoiceLinesCountryOfSupply()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertEquals("Pre-Req: CountryOfSupplyMustBeTheSameForAllLinesOnInstruction is true for this test to correctly expect the validation behavior", true, declaration.Configuration.InvoiceLineConfiguration.CountryOfSupplyMustBeTheSameForAllLinesOnInstruction(declaration));

				var cei = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = cei.PK;
				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = cei.PK;
				Assert(cei.IsCountryOfSupplySameForAllInvoiceLines);
				AssertNoRowMessageError(cei, "All invoice lines on an Entry instruction must have the same Country of Supply");

				invoiceLine2.ZG_CountryOfSupply = Core.Constants.CountryCodes.Australia;
				AssertEquals(false, cei.IsCountryOfSupplySameForAllInvoiceLines);
				AssertHasRowMessageError("There should be an error against the entry instruction because its invoice lines country of supply are not all the same and FR require them to be (the same).", cei, "All invoice lines on an Entry instruction must have the same Country of Supply");
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertEquals("Pre-Req: CountryOfSupplyMustBeTheSameForAllLinesOnInstruction is false for this test to correctly expect the validation behavior", false, declaration.Configuration.InvoiceLineConfiguration.CountryOfSupplyMustBeTheSameForAllLinesOnInstruction(declaration));

				var cei = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = cei.PK;
				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = cei.PK;
				invoiceLine2.ZG_CountryOfSupply = Core.Constants.CountryCodes.Australia;
				AssertEquals(false, cei.IsCountryOfSupplySameForAllInvoiceLines);
				AssertEquals("There shouldn't be any error against the entry instruction even if its invoice lines country of supply are not all the same, because EU doesn't require them to be (the same)", 0, cei.Notifications.Count());
			}
		}

		public void TestCheckCEI_SubStyle()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var cei = dec.CustomsEntryInstructions.AddNew();
			var substyleCodeList = cei.Lookups.EntrySubStyleList.GetAllCodes();

			cei.CEI_SubStyle = "#";
			Assert(!substyleCodeList.Contains("#"));
			AssertListValidationInvalidCodeMessageError(cei.CEI_SubStyleInfo, true);

			cei.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
			Assert(substyleCodeList.Contains(EntrySubStyleList.Codes.IncompleteDeclaration));
			AssertListValidationInvalidCodeMessageError(cei.CEI_SubStyleInfo, false);
		}

		public void TestCheckCEI_OA_Warehouse2()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "BOB1";
				importer.OH_FullName = "BOB THE BUILDER";
				importer.CompanyData.OB_IMUsedBondedWhs = true;
				importer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
				var cusProcedureHelper = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Latvia, "", "40", "10", "", "AB DESC", "IMP", intoWarehouse: true, outOfWarehouse: true, group: "JC");
				cusProcedureHelper.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Latvia;

				var warehouse1 = Factory.New<OrgHeader>();
				warehouse1.OH_Code = "WAR1";
				warehouse1.OH_FullName = "WAREHOUSE 1";
				warehouse1.OH_RL_NKClosestPort = "LVTES";
				warehouse1.MainAddress.OA_Address1 = "2 TEST";
				warehouse1.MainAddress.OA_RL_NKRelatedPortCode = "LVTES";
				warehouse1.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;

				TestWarehouseWithGoodBoolean(importer, cusProcedureHelper, out var entryInstruction, true, "ENT4010", out string message);

				Assert(entryInstruction.IsWarehouse2RequiredForWarehouseValidation);
				Assert(entryInstruction.EntryHeader.IsBondedWarehousingFieldValidationRequired);
				entryInstruction.CEI_OA_Warehouse2 = warehouse1.MainAddress.PK;
				AssertHasMessageError(entryInstruction.CEI_OA_Warehouse2Info, message);

				var refUNLOCO = Factory.New<RefUNLOCO>();
				refUNLOCO.RL_Code = "LVTES";
				refUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
				Factory.Save();

				warehouse1 = Factory.New<OrgHeader>();
				warehouse1.OH_Code = "WAR1";
				warehouse1.OH_FullName = "WAREHOUSE 1";
				warehouse1.OH_RL_NKClosestPort = "LVTES";
				warehouse1.MainAddress.OA_Address1 = "2 TEST";
				warehouse1.MainAddress.OA_RL_NKRelatedPortCode = "LVTES";
				warehouse1.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
				var warehouse2 = Factory.New<OrgHeader>();
				warehouse2.OH_Code = "WAR2";
				warehouse2.OH_FullName = "WAREHOUSE 2";
				warehouse2.OH_RL_NKClosestPort = "AUSYD";
				warehouse2.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
				warehouse2.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				var warehouse3 = Factory.New<OrgHeader>();
				warehouse3.OH_Code = "WAR3";
				warehouse3.OH_FullName = "WAREHOUSE 3";
				warehouse3.OH_RL_NKClosestPort = "DEFRA";
				warehouse3.MainAddress.OA_RL_NKRelatedPortCode = "DEFRA";
				warehouse3.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
				Factory.Save();

				TestWarehouseWithGoodBoolean(importer, cusProcedureHelper, out entryInstruction, true, "ENT4010", out message);

				Assert(entryInstruction.IsWarehouse2RequiredForWarehouseValidation);
				Assert(entryInstruction.EntryHeader.IsBondedWarehousingFieldValidationRequired);
				entryInstruction.CEI_OA_Warehouse2 = warehouse1.MainAddress.PK;
				AssertNoMessageErrors(entryInstruction.CEI_OA_Warehouse2Info);

				TestWarehouseWithGoodBoolean(importer, cusProcedureHelper, out entryInstruction, true, "ENT4010", out message);

				Assert(entryInstruction.IsWarehouse2RequiredForWarehouseValidation);
				Assert(entryInstruction.EntryHeader.IsBondedWarehousingFieldValidationRequired);
				entryInstruction.CEI_OA_Warehouse2 = warehouse2.MainAddress.PK;
				AssertHasMessageError(entryInstruction.CEI_OA_Warehouse2Info, message);

				TestWarehouseWithGoodBoolean(importer, cusProcedureHelper, out entryInstruction, true, "ENT4010", out message);

				Assert(entryInstruction.IsWarehouse2RequiredForWarehouseValidation);
				Assert(entryInstruction.EntryHeader.IsBondedWarehousingFieldValidationRequired);
				entryInstruction.CEI_OA_Warehouse2 = warehouse3.MainAddress.PK;
				AssertNoMessageErrors(entryInstruction.CEI_OA_Warehouse2Info);
			}
		}

		public void TestNIWarehouseValidation()
		{
			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = Factory.NewWithValidTestData<RefCountryStates>();
				ni.RW_RegionName = "NORTHERN IRELAND";
				belfast.RL_RW = ni.PK;
			}

			var london = new RefUNLOCO.Loader(Factory).Load("GBLON");
			if (london.CountryStates == null)
			{
				var gb = Factory.NewWithValidTestData<RefCountryStates>();
				gb.RW_RegionName = "ENGLAND";
				london.RL_RW = gb.PK;
			}
			Factory.Save();

			var warehouseBelfast = Factory.New<OrgHeader>();
			warehouseBelfast.OH_Code = "WAR1";
			warehouseBelfast.OH_FullName = "WAREHOUSE 1";
			warehouseBelfast.OH_RL_NKClosestPort = "GBBEL";
			warehouseBelfast.MainAddress.OA_Address1 = "1 TEST";
			warehouseBelfast.MainAddress.OA_RL_NKRelatedPortCode = "GBBEL";
			warehouseBelfast.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;

			var warehouseLondon = Factory.New<OrgHeader>();
			warehouseLondon.OH_Code = "WAR1";
			warehouseLondon.OH_FullName = "WAREHOUSE 1";
			warehouseLondon.OH_RL_NKClosestPort = "GBLON";
			warehouseLondon.MainAddress.OA_Address1 = "1 TEST";
			warehouseLondon.MainAddress.OA_RL_NKRelatedPortCode = "GBLON";
			warehouseLondon.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;

			var warehouseSydney = Factory.New<OrgHeader>();
			warehouseSydney.OH_Code = "WAR2";
			warehouseSydney.OH_FullName = "WAREHOUSE 2";
			warehouseSydney.OH_RL_NKClosestPort = "AUSYD";
			warehouseSydney.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			warehouseSydney.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "WENDY1";
				importer.OH_FullName = "WENDY1 THE BUILDER";
				importer.CompanyData.OB_IMUsedBondedWhs = true;
				importer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
				var cusProcedureHelper = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Latvia, "", "40", "10", "", "AB DESC", "IMP", intoWarehouse: true, outOfWarehouse: true, group: "JC");
				cusProcedureHelper.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Latvia;

				TestWarehouseWithGoodBoolean(importer, cusProcedureHelper, out var entryInstruction, true, "ENT4010", out var message);

				entryInstruction.CEI_OA_Warehouse = warehouseBelfast.MainAddress.PK;
				AssertNoMessageErrors(entryInstruction.CEI_OA_WarehouseInfo);

				entryInstruction.CEI_OA_Warehouse = warehouseLondon.MainAddress.PK;
				AssertHasMessageError(entryInstruction.CEI_OA_WarehouseInfo, message);

				entryInstruction.CEI_OA_Warehouse = warehouseSydney.MainAddress.PK;
				AssertHasMessageError(entryInstruction.CEI_OA_WarehouseInfo, message);

				entryInstruction.CEI_OA_Warehouse2 = warehouseBelfast.MainAddress.PK;
				AssertNoMessageErrorContaining(entryInstruction.CEI_OA_Warehouse2Info, message);

				entryInstruction.CEI_OA_Warehouse2 = warehouseLondon.MainAddress.PK;
				AssertHasMessageError(entryInstruction.CEI_OA_Warehouse2Info, message);

				entryInstruction.CEI_OA_Warehouse2 = warehouseSydney.MainAddress.PK;
				AssertHasMessageError(entryInstruction.CEI_OA_Warehouse2Info, message);
			}
		}

		public void TestCheckCEI_OA_Warehouse()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "BOB1";
			importer.OH_FullName = "BOB THE BUILDER";
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			importer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			var cusProcedureHelper = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Latvia, "", "40", "10", "", "AB DESC", "IMP", intoWarehouse: true, outOfWarehouse: true, group: "JC");
			cusProcedureHelper.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Latvia;

			var warehouse1 = Factory.New<OrgHeader>();
			warehouse1.OH_Code = "WAR1";
			warehouse1.OH_FullName = "WAREHOUSE 1";
			warehouse1.OH_RL_NKClosestPort = "LVTES";
			warehouse1.MainAddress.OA_Address1 = "2 TEST";
			warehouse1.MainAddress.OA_RL_NKRelatedPortCode = "LVTES";
			warehouse1.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;

			TestWarehouseWithGoodBoolean(importer, cusProcedureHelper, out var entryInstruction, true, "ENT4010", out string message);

			Assert(entryInstruction.IsWarehouseRequiredForWarehouseValidation);
			Assert(entryInstruction.EntryHeader.IsBondedWarehousingFieldValidationRequired);
			entryInstruction.CEI_OA_Warehouse = warehouse1.MainAddress.PK;
			AssertHasMessageError(entryInstruction.CEI_OA_WarehouseInfo, message);

			var refUNLOCO = Factory.New<RefUNLOCO>();
			refUNLOCO.RL_Code = "LVTES";
			refUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			Factory.Save();

			warehouse1 = Factory.New<OrgHeader>();
			warehouse1.OH_Code = "WAR1";
			warehouse1.OH_FullName = "WAREHOUSE 1";
			warehouse1.OH_RL_NKClosestPort = "LVTES";
			warehouse1.MainAddress.OA_Address1 = "2 TEST";
			warehouse1.MainAddress.OA_RL_NKRelatedPortCode = "LVTES";
			warehouse1.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			var warehouse2 = Factory.New<OrgHeader>();
			warehouse2.OH_Code = "WAR2";
			warehouse2.OH_FullName = "WAREHOUSE 2";
			warehouse2.OH_RL_NKClosestPort = "AUSYD";
			warehouse2.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			warehouse2.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var warehouse3 = Factory.New<OrgHeader>();
			warehouse3.OH_Code = "WAR3";
			warehouse3.OH_FullName = "WAREHOUSE 3";
			warehouse3.OH_RL_NKClosestPort = "DEFRA";
			warehouse3.MainAddress.OA_RL_NKRelatedPortCode = "DEFRA";
			warehouse3.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			Factory.Save();

			TestWarehouseWithGoodBoolean(importer, cusProcedureHelper, out entryInstruction, true, "ENT4010", out message);

			Assert(entryInstruction.IsWarehouseRequiredForWarehouseValidation);
			Assert(entryInstruction.EntryHeader.IsBondedWarehousingFieldValidationRequired);
			entryInstruction.CEI_OA_Warehouse = warehouse1.MainAddress.PK;
			AssertNoMessageErrors(entryInstruction.CEI_OA_WarehouseInfo);

			TestWarehouseWithGoodBoolean(importer, cusProcedureHelper, out entryInstruction, true, "ENT4010", out message);

			Assert(entryInstruction.IsWarehouseRequiredForWarehouseValidation);
			Assert(entryInstruction.EntryHeader.IsBondedWarehousingFieldValidationRequired);
			entryInstruction.CEI_OA_Warehouse = warehouse2.MainAddress.PK;
			AssertHasMessageError(entryInstruction.CEI_OA_WarehouseInfo, message);

			TestWarehouseWithGoodBoolean(importer, cusProcedureHelper, out entryInstruction, true, "ENT4010", out message);

			Assert(entryInstruction.IsWarehouseRequiredForWarehouseValidation);
			Assert(entryInstruction.EntryHeader.IsBondedWarehousingFieldValidationRequired);
			entryInstruction.CEI_OA_Warehouse = warehouse3.MainAddress.PK;
			AssertNoMessageErrors(entryInstruction.CEI_OA_WarehouseInfo);
		}

		void TestWarehouseWithGoodBoolean(OrgHeader importer, RefCusProcedure cusProcedureHelper, out CusEntryInstruction entryInstruction, bool areMultipleEntryInstructionsAllowed, string entryNumber, out string message)
		{
			var declarationMock = Factory.NewMoq<JobDeclaration>();
			declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
			declarationMock.Setup(d => d.AreMultipleEntryInstructionsAllowed).Returns(areMultipleEntryInstructionsAllowed);
			declarationMock.Protected().Setup<bool>("SupportsBondedWarehousingCore").Returns(true);
			var declaration = declarationMock.Object;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Style = cusProcedureHelper.ZZ6_Group;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			invoiceLine.JI_Procedure = cusProcedureHelper.ZZ6_ProcedureCode + cusProcedureHelper.ZZ6_PreviousProcedureCode;
			invoiceLine.JI_CEI = entryInstruction.PK;
			entry.CH_CEI_Instruction = entryInstruction.PK;
			entry.EntryNumber = entryNumber;
			entry.CH_WarehouseTransactionStatus = "SOS";
			Assert(entry.IsBondedWarehousingFieldValidationRequired);
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.SetUseBondedWarehouseAutomationForTesting(true);
			message = CusEntryHeader.BondedWarehouseAddressShouldBeInsideDeclarationRegionEU(importer == null ? ZString.Empty : importer.OH_Code, entryInstruction.EntryHeader.EntryHeaderDescriptiveMenuItemText, declaration.Country == null ? ZString.Empty : declaration.Country.RN_DescMultilingual);
		}

		public void TestAddNotAllowDeleteEntryLinesError()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryHeader.CH_HighestLineNumber = 2;
			entryHeader.CH_EntryStatus = Common.EU.EntryStatusList.Codes.Clear;

			instruction.Validation.AddNotAllowDeleteEntryLinesError();
			AssertEquals(false, instruction.ShouldKeepNotAllowDeleteEntryLinesErrors);
			AssertEquals(false, instruction.RowErrors.Any(error => error.Message.Contains("Cannot delete Entry lines from a Declared or Canceled Entry")));
		}

		public void TestValidateNotAllowDeleteEntryLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "BGM001";
			entryHeader.CH_CEI_Instruction = instruction.PK;
			instruction.Validation.ValidateNotAllowDeleteEntryLines();
			AssertNoRowError(instruction, "Entry Declared (BGM001) : Cannot delete Entry lines from a Declared or Canceled Entry. Please, cancel and reopen the declaration. Data will revert to the last save.");

			instruction.ShouldKeepNotAllowDeleteEntryLinesErrors = true;
			instruction.Validation.ValidateNotAllowDeleteEntryLines();
			AssertHasRowError(instruction, "Entry Declared (BGM001) : Cannot delete Entry lines from a Declared or Canceled Entry. Please, cancel and reopen the declaration. Data will revert to the last save.");

			instruction.Validation.ValidateAll();
			AssertHasRowError(instruction, "Entry Declared (BGM001) : Cannot delete Entry lines from a Declared or Canceled Entry. Please, cancel and reopen the declaration. Data will revert to the last save.");
		}

		public void TestValidateFiscalReferences_CannotBeEntered()
		{
			const string fiscalReferencesCannotBeEntered = "Fiscal Representation should only be entered when there is at least one invoice line with CPC starts with 42 or 63.";

			var invoiceLineConfigurationMock = new Mock<InstructionConfiguration>();
			invoiceLineConfigurationMock.CallBase = true;
			invoiceLineConfigurationMock.Protected()
				.Setup<ZBool>("FiscalReferencesSupportCore", ItExpr.IsAny<CargoWise.EntityFramework.BusinessObject>())
				.Returns(true);

			using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "GetNewInstructionConfiguration", invoiceLineConfigurationMock.Object, null))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				CombineAssertions(() =>
				{
					AssertFiscalReference("No error message - CPC 40", instruction, false, invoiceLine, "4000000", false, false, fiscalReferencesCannotBeEntered);
					AssertFiscalReference("Error message - CPC 40 - fiscal reference on instruction", instruction, true, invoiceLine, "4000000", false, true, fiscalReferencesCannotBeEntered);
				});
			}
		}

		public void TestValidateFiscalReferences_MustBeEntered_FiscalReferencesSupportedOnInstructionOnly()
		{
			const string fiscalReferencesMustBeEntered = "If there is an invoice line with CPC starts with 42 or 63, at least one Fiscal Reference record must exist at the Instruction.";

			var invoiceLineConfigurationMock = new Mock<InstructionConfiguration>();
			invoiceLineConfigurationMock.CallBase = true;
			invoiceLineConfigurationMock.Protected()
				.Setup<ZBool>("FiscalReferencesSupportCore", ItExpr.IsAny<CargoWise.EntityFramework.BusinessObject>())
				.Returns(true);

			using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "GetNewInstructionConfiguration", invoiceLineConfigurationMock.Object, null))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;

				CombineAssertions(() =>
				{
					AssertFiscalReference("No error message - CPC 40", instruction, false, invoiceLine, "4000000", false, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("Error message - CPC 42 - no fiscal reference", instruction, false, invoiceLine, "4200000", false, true, fiscalReferencesMustBeEntered);
					AssertFiscalReference("Error message - CPC 63 - no fiscal reference", instruction, false, invoiceLine, "6300000", false, true, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 42 - fiscal reference on instruction", instruction, true, invoiceLine, "4200000", false, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 63 - fiscal reference on instruction", instruction, true, invoiceLine, "6300000", false, false, fiscalReferencesMustBeEntered);
				});
			}
		}

		public void TestValidateFiscalReferences_MustBeEntered_FiscalReferencesSupportedOnInstructionAndInvoiceLine()
		{
			const string fiscalReferencesMustBeEntered = "If there is an invoice line with CPC starts with 42 or 63, at least one Fiscal Reference record must exist at the Instruction or Line level.";

			var invoiceLineConfigurationMock1 = new Mock<InvoiceLineConfiguration>();
			invoiceLineConfigurationMock1.Protected()
				.Setup<ZBool>("FiscalReferencesSupportCore", ItExpr.IsAny<CargoWise.EntityFramework.BusinessObject>())
				.Returns(true);

			var instructionConfigurationMock1 = new Mock<InstructionConfiguration>();
			instructionConfigurationMock1.Protected()
				.Setup<ZBool>("FiscalReferencesSupportCore", ItExpr.IsAny<CargoWise.EntityFramework.BusinessObject>())
				.Returns(true);
			instructionConfigurationMock1.Protected()
				.Setup<ZBool>("FiscalReferencesSupportOnCPC42And63OnlyCore", ItExpr.IsAny<CargoWise.EntityFramework.BusinessObject>())
				.Returns(true);

			var declarationConfigurationMock = new Mock<DeclarationConfiguration>();
			declarationConfigurationMock.CallBase = true;
			declarationConfigurationMock.Protected().Setup<InvoiceLineConfiguration>("GetNewInvoiceLineConfiguration")
				.Returns(invoiceLineConfigurationMock1.Object);
			declarationConfigurationMock.Protected().Setup<InstructionConfiguration>("GetNewInstructionConfiguration")
				.Returns(instructionConfigurationMock1.Object);

			var countryOrGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.ClearCachedValue<DeclarationConfiguration>($"DeclarationConfiguration_{countryOrGrouping}");

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject()).Returns(declarationConfigurationMock.Object);

			var declarationConfiguration = new KeyObjectHandleDictionaryObject
			{
				{ countryOrGrouping, objectHandleMock.Object }
			};

			using (ObjectFactory.Substitute(nameof(DeclarationConfiguration), declarationConfiguration))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;

				CombineAssertions(() =>
				{
					AssertFiscalReference("No error message - CPC 40", instruction, false, invoiceLine, "4000000", false, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("Error message - CPC 42 - no fiscal reference", instruction, false, invoiceLine, "4200000", false, true, fiscalReferencesMustBeEntered);
					AssertFiscalReference("Error message - CPC 63 - no fiscal reference", instruction, false, invoiceLine, "6300000", false, true, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 42 - fiscal reference on instruction", instruction, true, invoiceLine, "4200000", false, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 63 - fiscal reference on instruction", instruction, true, invoiceLine, "6300000", false, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 42 - fiscal reference on instruction and invoice line", instruction, true, invoiceLine, "4200000", true, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 63 - fiscal reference on instruction and invoice line", instruction, true, invoiceLine, "6300000", true, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 42 - fiscal reference on invoice line", instruction, false, invoiceLine, "4200000", true, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 63 - fiscal reference on invoice line", instruction, false, invoiceLine, "6300000", true, false, fiscalReferencesMustBeEntered);

					var invoiceLine2 = invoice.InvoiceLines.AddNew();
					invoiceLine2.JI_CEI = instruction.PK;

					AssertFiscalReference("No error message - CPC 42 - fiscal reference on some invoice line", instruction, false, invoiceLine2, "4200000", false, true, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 63 - fiscal reference on some invoice line", instruction, false, invoiceLine2, "6300000", false, true, fiscalReferencesMustBeEntered);
				});
			}

			var invoiceLineConfigurationMock2 = new Mock<InvoiceLineConfiguration>();
			invoiceLineConfigurationMock2.Protected()
				.Setup<ZBool>("FiscalReferencesSupportCore", ItExpr.IsAny<CargoWise.EntityFramework.BusinessObject>())
				.Returns(true);

			var instructionConfigurationMock2 = new Mock<InstructionConfiguration>();
			instructionConfigurationMock2.Protected()
				.Setup<ZBool>("FiscalReferencesSupportCore", ItExpr.IsAny<CargoWise.EntityFramework.BusinessObject>())
				.Returns(true);
			instructionConfigurationMock2.Protected()
				.Setup<ZBool>("FiscalReferencesSupportOnCPC42And63OnlyCore", ItExpr.IsAny<CargoWise.EntityFramework.BusinessObject>())
				.Returns(false);

			var declarationConfigurationMock2 = new Mock<DeclarationConfiguration>();
			declarationConfigurationMock2.CallBase = true;
			declarationConfigurationMock2.Protected().Setup<InvoiceLineConfiguration>("GetNewInvoiceLineConfiguration")
				.Returns(invoiceLineConfigurationMock2.Object);
			declarationConfigurationMock2.Protected().Setup<InstructionConfiguration>("GetNewInstructionConfiguration")
				.Returns(instructionConfigurationMock2.Object);

			countryOrGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.ClearCachedValue<DeclarationConfiguration>($"DeclarationConfiguration_{countryOrGrouping}");

			var objectHandleMock2 = new Mock<ObjectHandle>();
			objectHandleMock2.Setup(m => m.GetObject()).Returns(declarationConfigurationMock2.Object);

			var declarationConfiguration2 = new KeyObjectHandleDictionaryObject
			{
				{ countryOrGrouping, objectHandleMock2.Object }
			};

			using (ObjectFactory.Substitute(nameof(DeclarationConfiguration), declarationConfiguration2))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;

				CombineAssertions(() =>
				{
					AssertFiscalReference("No error message - CPC 42 - no fiscal reference", instruction, false, invoiceLine, "4200000", false, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 63 - no fiscal reference", instruction, false, invoiceLine, "6300000", false, false, fiscalReferencesMustBeEntered);
				});
			}
		}

		public void TestValidateGuaranteeTypesCount()
		{
			var expectedMessage = "You have entered more than 9 different Types.";
			var entryInstruction = Factory.New<CusEntryInstruction>();
			for (int i = 1; i <= 9; i++)
			{
				var guarantee = entryInstruction.Guarantees.AddNew();
				guarantee.PW_BondType = i.ToString();
			}

			entryInstruction.Validation.ValidateAll();
			AssertEquals("Distinct types count in Guarantees collection", 9, GetDistinctGuaranteeTypesInEntryInstruction());
			AssertNoRowMessageError("When only 9 distinct types in Guarantees collection", entryInstruction, expectedMessage);

			var guarantee3 = entryInstruction.Guarantees.AddNew();
			guarantee3.PW_BondType = "3";

			entryInstruction.Validation.ValidateAll();
			AssertEquals("Distinct types count in Guarantees collection", 9, GetDistinctGuaranteeTypesInEntryInstruction());
			AssertNoRowMessageError("When only 9 distinct types in Guarantees collection", entryInstruction, expectedMessage);

			var guarantee0 = entryInstruction.Guarantees.AddNew();
			guarantee0.PW_BondType = "0";

			entryInstruction.Validation.ValidateAll();
			AssertEquals("Distinct types count in Guarantees collection", 10, GetDistinctGuaranteeTypesInEntryInstruction());
			AssertHasRowMessageError("When more than 9 distinct types in Guarantees collection", entryInstruction, expectedMessage);

			int GetDistinctGuaranteeTypesInEntryInstruction() => entryInstruction.Guarantees.Cast<GuaranteeForEntryInstruction>().Select(g => g.PW_BondType).Distinct().Count();
		}

		static void AssertFiscalReference(string scenarioName, CusEntryInstruction instruction, bool hasFiscalOnInstruction, JobComInvoiceLine invoiceLine, string cpc, bool hasFiscalOnItem, bool hasError, string errorMessage)
		{
			invoiceLine.JI_Procedure = cpc;

			instruction.FiscalReferences.RemoveAndDeleteAll();
			if (hasFiscalOnInstruction)
			{
				var fiscalReference = instruction.FiscalReferences.AddNew();
				fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
				fiscalReference.CFR_Reference = "refInstruction";
			}

			invoiceLine.FiscalReferences.RemoveAndDeleteAll();
			if (hasFiscalOnItem)
			{
				var fiscalReference = invoiceLine.FiscalReferences.AddNew();
				fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
				fiscalReference.CFR_Reference = "refInvoiceLine";
			}

			instruction.ClearRowNotifications();
			instruction.Validation.ValidateFiscalReferences();
			if (hasError)
			{
				AssertHasRowMessageError(scenarioName, instruction, errorMessage);
			}
			else
			{
				AssertNoRowMessageError(scenarioName, instruction, errorMessage);
			}
		}

		public void TestCheckRuleC0614()
		{
			var message = "[C0614] Previous Documents on Entry Instruction or Invoice Header required when Sub Style is 'A' or 'D' or 'Y'.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryinstruction = declaration.CustomsEntryInstructions.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				entryinstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
				entryinstruction.Validation.ValidateCEI_SubStyle();
				AssertNoMessageError("Not UCC6, there should be no message error.", entryinstruction.CEI_SubStyleInfo, message);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				entryinstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
				entryinstruction.Validation.ValidateCEI_SubStyle();
				AssertNoMessageError("Export, there should be no message error.", entryinstruction.CEI_SubStyleInfo, message);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				entryinstruction.Validation.ValidateCEI_SubStyle();
				AssertHasMessageError("Import, UCC6, SubStyle equal to A, D or Y, previous documents are empty, there should be a message error.", entryinstruction.CEI_SubStyleInfo, message);

				entryinstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
				entryinstruction.Validation.ValidateCEI_SubStyle();
				AssertNoMessageError("SubStyle not equal to A, D or Y, there should be no message error.", entryinstruction.CEI_SubStyleInfo, message);

				entryinstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
				var previousDoc = entryinstruction.PreviousDocuments.AddNew();
				entryinstruction.Validation.ValidateCEI_SubStyle();
				AssertNoMessageError("Previous document code added, there should be no message error.", entryinstruction.CEI_SubStyleInfo, message);

				previousDoc.Delete();
				entryinstruction.Validation.ValidateCEI_SubStyle();
				AssertHasMessageError("Previous document removed from entry instruction, there should be a message error.", entryinstruction.CEI_SubStyleInfo, message);

				var invoice = declaration.Invoices.AddNew();
				invoice.PreviousDocuments.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryinstruction.PK;
				entryinstruction.Validation.ValidateCEI_SubStyle();
				AssertNoMessageError("Previous document added to invoice header, there should be no message error.", entryinstruction.CEI_SubStyleInfo, message);
			}
		}

		public void TestCheckRuleG0128_Export()
			=> TestG0128ForEntryStyle(RefCusCodeListEntryStyle.Export, new[] { "10", "11", "21", "22", "23", "31" },
				"[G0128] Only requested procedure ‘10’ or ‘11’ or ‘21’ or ‘22’ or ‘23’ or ‘31’ is allowed for Declaration type ‘EX’.");

		public void TestCheckRuleG0128_ImportOfGoodsFromSpecialTerritoryOfTheCommunity()
			=> TestG0128ForEntryStyle(RefCusCodeListEntryStyle.ImportOfGoodsFromSpecialTerritoryOfTheCommunity, new[] { "10", "76", "77" },
				"[G0128] Only requested procedure '10' or '76' or '77' is allowed for Declaration type 'CO'.");

		void TestG0128ForEntryStyle(string entryStyle, string[] allowedProcedures, string errorMessage)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var procedureInfo = entryInstruction.CEI_ProcedureInfo;

			var allPossibleCusProcedures = typeof(CusProcedure).GetFields(BindingFlags.Public | BindingFlags.Static)
																.Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
																.Select(field => (string)field.GetValue(null))
																.ToArray();

			using var testContext = new EntryInstructionValidationDeciderTestContext(declaration, isUCC6: true, typeof(IRuleG0128ForCEI_ProcedureDecider));
			CombineAssertions(() =>
			{
				testContext.EnableRuleDecider<IRuleG0128ForCEI_ProcedureDecider>(x => x.IsActive);
				declaration.JE_EntryStyle = entryStyle;
				allPossibleCusProcedures.ForEach(procedure => TestWithEntryStyleAndCusProcedure(procedure, errorIsExpected: !allowedProcedures.Contains(procedure)));

				declaration.JE_EntryStyle = RefCusCodeListEntryStyle.Import;
				allPossibleCusProcedures.ForEach(procedure => TestWithEntryStyleAndCusProcedure(procedure, errorIsExpected: false));

				testContext.DisableRuleDecider<IRuleG0128ForCEI_ProcedureDecider>(x => x.IsActive);
				declaration.JE_EntryStyle = entryStyle;
				allPossibleCusProcedures.ForEach(procedure => TestWithEntryStyleAndCusProcedure(procedure, errorIsExpected: false));
			});

			void TestWithEntryStyleAndCusProcedure(string cusProcedure, bool errorIsExpected)
			{
				entryInstruction.CEI_Procedure = cusProcedure;
				if (errorIsExpected)
				{
					AssertHasMessageError(
						$"UCC6={declaration.IsUCC6}, JE_MessageType={declaration.JE_MessageType}, EntryStyle={declaration.JE_EntryStyle}, CusProcedure={cusProcedure}, message={errorMessage}",
						procedureInfo, errorMessage);
				}
				else
				{
					AssertNoMessageError(
						$"UCC6={declaration.IsUCC6}, JE_MessageType={declaration.JE_MessageType}, EntryStyle={declaration.JE_EntryStyle}, CusProcedure={cusProcedure}, message={errorMessage}",
						procedureInfo, errorMessage);
				}
			}
		}

		public void TestCheckRuleR0028EWhenAuthorizationCodeC512()
		{
			var message = "[R0028E] Only declaration Sub Style C or F or Y is allowed for an authorization code C512.";
			var (declaration, entryInstruction) = SetUpRuleR0028E(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
			CombineAssertions(() =>
			{
				CheckWhenR0028ENotActive(message, declaration, entryInstruction);
				var instructionConfigurationMock = MockInstructionConfigurationR0028EActive(entryInstruction);
				using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(declaration, "GetNewInstructionConfiguration", instructionConfigurationMock.Object), true))
				{
					entryInstruction.Validation.ValidateCEI_SubStyle();
					AssertHasMessageError("Export, UCC6, SubStyle not equal to C, F or Y, there should be a message error.", entryInstruction.CEI_SubStyleInfo, message);

					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
					AssertNoMessageError("Export, UCC6, SubStyle equal to C, F or Y, there should be no message error.", entryInstruction.CEI_SubStyleInfo, message);

					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;
					AssertNoMessageError("Export, UCC6, SubStyle equal to C, F or Y, there should be no message error.", entryInstruction.CEI_SubStyleInfo, message);
				}
			});
		}

		public void TestCheckRuleR0028EWhenAuthorizationCodeC513()
		{
			var message = "[R0028E] Only declaration Sub Style A or B or C or D or E or F or X or Y or Z is allowed for an authorization code C513.";
			var (declaration, entryInstruction) = SetUpRuleR0028E(CusAuthorizationHeaderTypeList.Codes.CentralizedClearance);
			var instructionConfigurationMock = MockInstructionConfigurationR0028EActive(entryInstruction);
			CombineAssertions(() =>
			{
				CheckWhenR0028ENotActive(message, declaration, entryInstruction);
				using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(declaration, "GetNewInstructionConfiguration", instructionConfigurationMock.Object), true))
				{
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationOfSimplifiedDeclarationsCoveredByCAndF;
					AssertHasMessageError("Export, UCC6, SubStyle not equal to A or B or C or D or E or F or X or Y or Z, there should be a message error.", entryInstruction.CEI_SubStyleInfo, message);

					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
					AssertNoMessageError("Export, UCC6, SubStyle equal to A or B or C or D or E or F or X or Y or Z, there should be no message error.", entryInstruction.CEI_SubStyleInfo, message);
				}
			});
		}

		public void TestCheckRuleR0028EWhenAuthorizationCodeC514()
		{
			var message = "[R0028E] Only declaration Sub Style A or C or D or F or Y or Z is allowed for an authorization code C514.";
			var (declaration, entryInstruction) = SetUpRuleR0028E(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords);
			var instructionConfigurationMock = MockInstructionConfigurationR0028EActive(entryInstruction);
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(declaration, "GetNewInstructionConfiguration", instructionConfigurationMock.Object), true))
				{
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
					AssertHasMessageError("Export, UCC6, SubStyle not equal to A or C or D or F or Y or Z, there should be a message error.", entryInstruction.CEI_SubStyleInfo, message);

					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
					AssertNoMessageError("Export, UCC6, SubStyle equal to A or C or D or F or Y or Z, there should be no message error.", entryInstruction.CEI_SubStyleInfo, message);
				}
			});
		}

		(JobDeclaration declaration, CusEntryInstruction entryInstruction) SetUpRuleR0028E(string authorizationCode)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, MapDirectionList.Codes.OUT, "Authorisation Codes to Document Type Code", true);
			helper.CreateCusMap(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, EUCommonConstants.CusAuthorizationUsageType.C512, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, Constants.DataGrouping.EuropeanUnion);
			helper.CreateCusMap(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.CentralizedClearance, EUCommonConstants.CusAuthorizationUsageType.C513, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, Constants.DataGrouping.EuropeanUnion);
			helper.CreateCusMap(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, EUCommonConstants.CusAuthorizationUsageType.C514, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, Constants.DataGrouping.EuropeanUnion);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = authorizationCode;
			return (declaration, entryInstruction);
		}

		Mock<InstructionConfiguration> MockInstructionConfigurationR0028EActive(CusEntryInstruction entryInstruction)
		{
			var validationDeciderMock = new Mock<IEntryInstructionValidationDecider> { CallBase = true };
			validationDeciderMock.As<IRuleR0028EForCEI_SubStyleDecider>().SetupGet(s => s.IsActive).Returns(true);
			var instructionConfigurationMock = new Mock<InstructionConfiguration>();
			instructionConfigurationMock.Protected()
					.Setup<IEntryInstructionValidationDecider>("GetValidationDeciderCore", entryInstruction)
					.Returns(validationDeciderMock.Object);
			return instructionConfigurationMock;
		}

		void CheckWhenR0028ENotActive(string message, JobDeclaration declaration, CusEntryInstruction entryInstruction)
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
				AssertNoMessageError("Not UCC6, there should be no message error.", entryInstruction.CEI_SubStyleInfo, message);
			}
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				entryInstruction.Validation.ValidateCEI_SubStyle();
				AssertNoMessageError("Rule R0028E not Active, there should be no message error.", entryInstruction.CEI_SubStyleInfo, message);
			}
		}

		public void TestCheckRuleC0619()
		{
			var message = "[C0619] Location Of Goods must be empty if CPC=71.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var entryinstruction = declaration.CustomsEntryInstructions.AddNew();

			entryinstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
			var goodsLocation = entryinstruction.GoodsLocation;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				entryinstruction.Validation.ValidateGoodsLocationDescription();
				AssertNoMessageError("Rule C0619: Declaration is Import, not UCC6, cei procedure is equal to 71, goodsDescription is not empty, there should be no message error.",
					entryinstruction.GoodsLocationDescriptionInfo, message);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				entryinstruction.Validation.ValidateGoodsLocationDescription();
				AssertNoMessageError("Rule C0619: Declaration is export, UCC6, there should be no message error.",
					entryinstruction.GoodsLocationDescriptionInfo, message);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				entryinstruction.Validation.ValidateGoodsLocationDescription();
				AssertHasMessageError(
					"Rule C0619: Declaration is Import, UCC6, cei procedure is equal to 71, goodsDescription is not empty, there should be a message error.",
					entryinstruction.GoodsLocationDescriptionInfo, message);

				goodsLocation.CGL_Qualifier = ZString.Empty;
				entryinstruction.Validation.ValidateGoodsLocationDescription();
				AssertNoMessageError(
					"Rule C0619: Declaration is Import, UCC6, cei procedure is equal to 71, goodsDescription is empty, there should be no message error.",
					entryinstruction.GoodsLocationDescriptionInfo, message);

				entryinstruction.CEI_Procedure = "10";
				AssertNoMessageError(
					"Rule C0619: Declaration is Import, UCC6, cei procedure is not equal to 71, goodsDescription is empty, there should be no message error.",
					entryinstruction.GoodsLocationDescriptionInfo, message);

				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				entryinstruction.Validation.ValidateGoodsLocationDescription();
				AssertNoMessageError(
					"Rule C0619: Declaration is Import, UCC6, cei procedure is not equal to 71, goodsDescription is not empty, there should be no message error.",
					entryinstruction.GoodsLocationDescriptionInfo, message);
			}
		}

		public void TestCheckRuleC0626()
		{
			var organisation = Factory.New<OrgHeader>();

			var message = "[C0626] To Warehouse is mandatory in case Sub Style equals A or D and CPC = 71.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryinstruction = declaration.CustomsEntryInstructions.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				entryinstruction.CEI_OA_Warehouse2 = organisation.PK;
				entryinstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				entryinstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
				entryinstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
				entryinstruction.CEI_OA_Warehouse2 = organisation.PK;
				entryinstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				AssertHasMessageError("procedure = 71, Substyle = A, to Warehouse is empty => error.", entryinstruction.CEI_OA_Warehouse2Info, message);

				entryinstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;
				entryinstruction.CEI_OA_Warehouse2 = organisation.PK;
				entryinstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				AssertHasMessageError("procedure = 71, Substyle = D, to Warehouse is empty => error.", entryinstruction.CEI_OA_Warehouse2Info, message);

				entryinstruction.CEI_SubStyle = "C";
				entryinstruction.CEI_OA_Warehouse2 = organisation.PK;
				entryinstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				AssertNoMessageError("procedure = 71, Substyle = C, to Warehouse is empty => no error.", entryinstruction.CEI_OA_Warehouse2Info, message);

				entryinstruction.CEI_Procedure = "10";
				entryinstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;
				entryinstruction.CEI_OA_Warehouse2 = organisation.PK;
				entryinstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				AssertNoMessageError("procedure = 10, Substyle = D, to Warehouse is empty => no error.", entryinstruction.CEI_OA_Warehouse2Info, message);

				entryinstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
				entryinstruction.CEI_OA_Warehouse2 = organisation.PK;
				entryinstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				AssertNoMessageError("procedure = 10, Substyle = A, to Warehouse is empty => no error.", entryinstruction.CEI_OA_Warehouse2Info, message);

				entryinstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
				entryinstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;
				entryinstruction.CEI_OA_Warehouse2 = organisation.PK;
				AssertNoMessageError("procedure = 71, Substyle = D, to Warehouse is not empty => no error.", entryinstruction.CEI_OA_Warehouse2Info, message);

				entryinstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
				entryinstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				entryinstruction.CEI_OA_Warehouse2 = organisation.PK;
				AssertNoMessageError("procedure = 71, Substyle = A, to Warehouse is not empty => no error.", entryinstruction.CEI_OA_Warehouse2Info, message);

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				entryinstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
				entryinstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
				entryinstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				AssertNoMessageError("export, UCC6, procedure = 71, Substyle = A, to Warehouse is empty => no error.", entryinstruction.CEI_OA_Warehouse2Info, message);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				entryinstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
				entryinstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
				entryinstruction.CEI_OA_Warehouse2 = organisation.PK;
				entryinstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				AssertNoMessageError("import, not UCC6, procedure = 71, Substyle = A, to Warehouse is empty => no error.", entryinstruction.CEI_OA_Warehouse2Info, message);
			}
		}

		public void TestCheckRuleC0628()
		{
			var message = "[C0628] Location Of Goods is mandatory for this declaration sub style.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryinstruction = declaration.CustomsEntryInstructions.AddNew();
			var goodsLocation = entryinstruction.GoodsLocation;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				entryinstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
				goodsLocation.CGL_Qualifier = ZString.Empty;
				entryinstruction.Validation.ValidateGoodsLocationDescription();
				AssertNoMessageError("Rule C0628: Import, not UCC6, SubStyle is not equal to D or F, goodsDescription is empty, there should be no message error.",
					entryinstruction.GoodsLocationDescriptionInfo, message);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				entryinstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
				goodsLocation.CGL_Qualifier = ZString.Empty;
				entryinstruction.Validation.ValidateGoodsLocationDescription();
				AssertNoMessageError("Rule C0628: Export, UCC6, there should be no message error.",
					entryinstruction.GoodsLocationDescriptionInfo, message);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				entryinstruction.Validation.ValidateGoodsLocationDescription();
				AssertHasMessageError(
					"Rule C0628: Import, UCC6, SubStyle is not equal to D or F, goodsDescription is empty, there should be a message error.",
					entryinstruction.GoodsLocationDescriptionInfo, message);

				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				entryinstruction.Validation.ValidateGoodsLocationDescription();
				AssertNoMessageError(
					"Rule C0628: Import, UCC6, SubStyle is not equal to D or F, goodsDescription is not empty, there should be no message error.",
					entryinstruction.GoodsLocationDescriptionInfo, message);

				entryinstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;
				goodsLocation.CGL_Qualifier = ZString.Empty;
				entryinstruction.Validation.ValidateGoodsLocationDescription();
				AssertNoMessageError(
					"Rule C0628: Import, UCC6, SubStyle is equal to D, goodsDescription is empty, there should be no message error.",
					entryinstruction.GoodsLocationDescriptionInfo, message);

				entryinstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;
				entryinstruction.Validation.ValidateGoodsLocationDescription();
				AssertNoMessageError(
					"Rule C0628: Import, UCC6, SubStyle is equal to F, goodsDescription is empty, there should be no message error.",
					entryinstruction.GoodsLocationDescriptionInfo, message);
			}
		}

		public void TestCheckRuleC0829()
		{
			var organisation = Factory.New<OrgHeader>();

			var message = "[C0829] To Warehouse is mandatory in case Requested Procedure =  07 / 45 / 68 / 95 / 96.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var listCPC = new ZString[]
			{
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._07,
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._45,
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._68,
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._95,
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._96
			};

			var entryinstruction = declaration.CustomsEntryInstructions.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				foreach (string cpc in listCPC)
				{
					AssertToWareHouseIsMandatory(cpc);
				}

				entryinstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
				entryinstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				AssertNoMessageError("procedure not in {07 / 45 / 68 / 95 / 96}, CEI_OA_Warehouse2 is empty => no error.", entryinstruction.CEI_OA_Warehouse2Info, message);

				entryinstruction.CEI_OA_Warehouse2 = organisation.PK;
				AssertNoMessageError("procedure not in {07 / 45 / 68 / 95 / 96}, CEI_OA_Warehouse2 is not empty => no error.", entryinstruction.CEI_OA_Warehouse2Info, message);

				void AssertToWareHouseIsMandatory(string cpc)
				{
					entryinstruction.CEI_Procedure = cpc;
					entryinstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
					AssertHasMessageError("procedure in {07 / 45 / 68 / 95 / 96}, CEI_OA_Warehouse2 is empty => error.", entryinstruction.CEI_OA_Warehouse2Info, message);

					entryinstruction.CEI_OA_Warehouse2 = organisation.PK;
					AssertNoMessageError("procedure in {07 / 45 / 68 / 95 / 96}, CEI_OA_Warehouse2 is not empty => no error.", entryinstruction.CEI_OA_Warehouse2Info, message);
				}

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				entryinstruction.CEI_Procedure = "07";
				entryinstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				AssertNoMessageError("export, ucc6, procedure in {07 / 45 / 68 / 95 / 96}, CEI_OA_Warehouse2 is empty => no error.", entryinstruction.CEI_OA_Warehouse2Info, message);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				entryinstruction.CEI_OA_Warehouse2 = organisation.PK;
				entryinstruction.CEI_Procedure = "07";
				entryinstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				AssertNoMessageError("not ucc6 procedure in {07 / 45 / 68 / 95 / 96}, CEI_OA_Warehouse2 is empty => no error.", entryinstruction.CEI_OA_Warehouse2Info, message);
			}
		}

		public void TestCheckRuleC0853_ConditionsAreMet()
		{
			var listCPC = new ZString[]
			{
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._01,
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._07,
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._40,
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._42,
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._43,
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._44,
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._45,
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._46,
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._48,
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._51,
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._53,
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._61,
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._63,
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._68
			};

			foreach (string cpc in listCPC)
			{
				AssertFromWareHouseIsMandatory(cpc);
			}

			var organisation2 = Factory.New<OrgHeader>();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = MessageTypeList.Codes.Import;
			var entryinstruction3 = declaration2.CustomsEntryInstructions.AddNew();
			var invoiceHeader3 = declaration2.Invoices.AddNew();
			var invoiceLine5 = invoiceHeader3.InvoiceLines.AddNew();
			invoiceLine5.JI_CEI = entryinstruction3.PK;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration2, true))
			{
				entryinstruction3.CEI_Procedure = "10";
				entryinstruction3.CEI_OA_Warehouse = ZGuid.Empty;
				AssertNoMessageErrorContaining("invoiceLine has a 71 CPC, procedure not in listCPC, CEI_OA_Warehouse is empty => no error.", entryinstruction3.CEI_OA_WarehouseInfo, "[C0853] From Warehouse is mandatory in case Requested Procedure");
				entryinstruction3.CEI_OA_Warehouse = organisation2.PK;
				invoiceLine5.JI_FormattedProcedure = "4071F47";
				AssertNoMessageErrorContaining("invoiceLine has a 71 CPC, procedure not in listCPC, CEI_OA_Warehouse is not empty => no error.", entryinstruction3.CEI_OA_WarehouseInfo, "[C0853] From Warehouse is mandatory in case Requested Procedure");

				entryinstruction3.CEI_Procedure = "07";
				entryinstruction3.CEI_OA_Warehouse = ZGuid.Empty;
				AssertHasMessageErrorContaining("invoiceLine has a 71 CPC, procedure is in listCPC, CEI_OA_Warehouse is empty => error.", entryinstruction3.CEI_OA_WarehouseInfo, "[C0853] From Warehouse is mandatory in case Requested Procedure");
			}

			void AssertFromWareHouseIsMandatory(string cpc)
			{
				var organisation = Factory.New<OrgHeader>();
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var entryinstruction = declaration.CustomsEntryInstructions.AddNew();
				var entryinstruction2 = declaration.CustomsEntryInstructions.AddNew();
				var message = $"[C0853] From Warehouse is mandatory in case Requested Procedure = {cpc} and Previous Procedure = 71.";

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					entryinstruction.CEI_Procedure = cpc;
					entryinstruction.CEI_OA_Warehouse = ZGuid.Empty;
					AssertNoMessageError("no invoiceLine, procedure in listCPC, CEI_OA_Warehouse is empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);
					entryinstruction.CEI_OA_Warehouse = organisation.PK;
					AssertNoMessageError("no invoiceLine, procedure in listCPC, CEI_OA_Warehouse is not empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);

					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceHeader2 = declaration.Invoices.AddNew();

					var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
					var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

					var invoiceLine3 = invoiceHeader2.InvoiceLines.AddNew();
					var invoiceLine4 = invoiceHeader2.InvoiceLines.AddNew();

					invoiceLine.JI_CEI = entryinstruction.PK;
					invoiceLine2.JI_CEI = entryinstruction.PK;
					invoiceLine3.JI_CEI = entryinstruction2.PK;
					invoiceLine4.JI_CEI = entryinstruction2.PK;

					entryinstruction.CEI_OA_Warehouse = ZGuid.Empty;
					AssertNoMessageError("invoiceLine has no CPC, procedure in listCPC, CEI_OA_Warehouse is empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);

					entryinstruction.CEI_OA_Warehouse = organisation.PK;
					AssertNoMessageError("invoiceLine has no CPC, procedure in listCPC, CEI_OA_Warehouse is not empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);

					invoiceLine2.JI_Procedure = "1256000";
					entryinstruction.CEI_OA_Warehouse = ZGuid.Empty;
					AssertNoMessageError("invoiceLine has not a 71 CPC, procedure in listCPC, CEI_OA_Warehouse is empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);
					entryinstruction.CEI_OA_Warehouse2 = organisation.PK;
					AssertNoMessageError("invoiceLine has not a 71 CPC, procedure in listCPC, CEI_OA_Warehouse is not empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);

					invoiceLine4.JI_Procedure = "4071F47";
					entryinstruction.CEI_OA_Warehouse = ZGuid.Empty;
					AssertNoMessageError("invoiceLine4 has a 71 CPC but is not related to this instruction, procedure in listCPC, CEI_OA_Warehouse is empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);
					entryinstruction.CEI_OA_Warehouse = organisation.PK;
					AssertNoMessageError("invoiceLine4 has a 71 CPC but is not related to this instruction, procedure in listCPC, CEI_OA_Warehouse is not empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);

					invoiceLine2.JI_Procedure = "4071F47";
					entryinstruction.CEI_OA_Warehouse = ZGuid.Empty;
					AssertHasMessageError("invoiceLine has a 71 CPC, procedure in listCPC, CEI_OA_Warehouse is empty => error.", entryinstruction.CEI_OA_WarehouseInfo, message);
					entryinstruction.CEI_OA_Warehouse = organisation.PK;
					AssertNoMessageError("invoiceLine has a 71 CPC, procedure in listCPC, CEI_OA_Warehouse is not empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);

					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					entryinstruction.CEI_Procedure = cpc;
					entryinstruction.CEI_OA_Warehouse = ZGuid.Empty;
					AssertNoMessageError("export, ucc6, invoiceLine has a 71 CPC, procedure in listCPC, CEI_OA_Warehouse is empty => error.", entryinstruction.CEI_OA_WarehouseInfo, message);
					entryinstruction.CEI_OA_Warehouse = organisation.PK;
					AssertNoMessageError("export, ucc6, invoiceLine has a 71 CPC, procedure in listCPC, CEI_OA_Warehouse is not empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					entryinstruction.CEI_Procedure = cpc;
					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					entryinstruction.CEI_OA_Warehouse = ZGuid.Empty;
					AssertNoMessageError("import, not ucc6, invoiceLine has a 71 CPC, procedure in listCPC, CEI_OA_Warehouse is empty => error.", entryinstruction.CEI_OA_WarehouseInfo, message);
					entryinstruction.CEI_OA_Warehouse = organisation.PK;
					AssertNoMessageError("import,not ucc6, invoiceLine has a 71 CPC, procedure in listCPC, CEI_OA_Warehouse is not empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);
				}
			}
		}

		public void TestCheckRuleC0853_ConditionsAreNotMet()
		{
			var listCPC = new ZString[] { "01", "07", "40", "42", "43", "44", "45", "46", "48", "51", "53", "61", "63", "68" };

			foreach (string cpc in listCPC)
			{
				AssertFromWareHouseIsMandatory(cpc);
			}

			void AssertFromWareHouseIsMandatory(string cpc)
			{
				var organisation = Factory.New<OrgHeader>();

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var entryinstruction = declaration.CustomsEntryInstructions.AddNew();
				var entryinstruction2 = declaration.CustomsEntryInstructions.AddNew();
				var message = $"[C0853] From Warehouse must be empty for that Requested Procedure and Previous Procedure combination.";

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					entryinstruction.CEI_OA_Warehouse = ZGuid.Empty;
					AssertNoMessageError("no invoiceLine, procedure in listCPC, CEI_OA_Warehouse is empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);
					entryinstruction.CEI_OA_Warehouse = organisation.PK;
					AssertHasMessageError("no invoiceLine, procedure in listCPC, CEI_OA_Warehouse is not empty => error.", entryinstruction.CEI_OA_WarehouseInfo, message);

					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceHeader2 = declaration.Invoices.AddNew();

					var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
					var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

					var invoiceLine3 = invoiceHeader2.InvoiceLines.AddNew();
					var invoiceLine4 = invoiceHeader2.InvoiceLines.AddNew();

					invoiceLine.JI_CEI = entryinstruction.PK;
					invoiceLine2.JI_CEI = entryinstruction.PK;
					invoiceLine3.JI_CEI = entryinstruction2.PK;
					invoiceLine4.JI_CEI = entryinstruction2.PK;

					entryinstruction.CEI_Procedure = cpc;
					entryinstruction.CEI_OA_Warehouse = ZGuid.Empty;
					AssertNoMessageError("invoiceLine has no CPC, procedure in listCPC, CEI_OA_Warehouse is empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);

					entryinstruction.CEI_OA_Warehouse = organisation.PK;
					AssertHasMessageError("invoiceLine has no CPC, procedure in listCPC, CEI_OA_Warehouse is not empty => error.", entryinstruction.CEI_OA_WarehouseInfo, message);

					invoiceLine2.JI_Procedure = "1256000";
					entryinstruction.CEI_OA_Warehouse = ZGuid.Empty;
					AssertNoMessageError("invoiceLine has not a 71 CPC, procedure in listCPC, CEI_OA_Warehouse is empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);
					entryinstruction.CEI_OA_Warehouse2 = organisation.PK;
					AssertNoMessageError("invoiceLine has not a 71 CPC, procedure in listCPC, CEI_OA_Warehouse is not empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);

					invoiceLine4.JI_Procedure = "4071F47";
					entryinstruction.CEI_OA_Warehouse = ZGuid.Empty;
					AssertNoMessageError("invoiceLine4 has a 71 CPC but is not related to this instruction, procedure in listCPC, CEI_OA_Warehouse is empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);
					entryinstruction.CEI_OA_Warehouse = organisation.PK;
					AssertHasMessageError("invoiceLine4 has a 71 CPC but is not related to this instruction, procedure in listCPC, CEI_OA_Warehouse is not empty => error.", entryinstruction.CEI_OA_WarehouseInfo, message);

					invoiceLine2.JI_Procedure = "4071F47";
					entryinstruction.CEI_OA_Warehouse = ZGuid.Empty;
					AssertNoMessageError("invoiceLine has a 71 CPC, procedure in listCPC, CEI_OA_Warehouse is empty => error.", entryinstruction.CEI_OA_WarehouseInfo, message);
					entryinstruction.CEI_OA_Warehouse = organisation.PK;
					AssertNoMessageError("invoiceLine has a 71 CPC, procedure in listCPC, CEI_OA_Warehouse is not empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);

					entryinstruction.CEI_Procedure = "10";
					entryinstruction.CEI_OA_Warehouse = ZGuid.Empty;
					AssertNoMessageErrorContaining("invoiceLine has a 71 CPC, procedure not in listCPC, CEI_OA_Warehouse is empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);
					entryinstruction.CEI_OA_Warehouse = organisation.PK;
					AssertHasMessageErrorContaining("invoiceLine has a 71 CPC, procedure not in listCPC, CEI_OA_Warehouse is not empty => error.", entryinstruction.CEI_OA_WarehouseInfo, message);

					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					entryinstruction.CEI_Procedure = cpc;
					entryinstruction.CEI_OA_Warehouse = ZGuid.Empty;
					AssertNoMessageError("export, ucc6, invoiceLine has a 71 CPC, procedure in listCPC, CEI_OA_Warehouse is empty => error.", entryinstruction.CEI_OA_WarehouseInfo, message);
					entryinstruction.CEI_OA_Warehouse = organisation.PK;
					AssertNoMessageError("export, ucc6, invoiceLine has a 71 CPC, procedure in listCPC, CEI_OA_Warehouse is not empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					entryinstruction.CEI_Procedure = cpc;
					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					entryinstruction.CEI_OA_Warehouse = ZGuid.Empty;
					AssertNoMessageError("import, not ucc6, invoiceLine has a 71 CPC, procedure in listCPC, CEI_OA_Warehouse is empty => error.", entryinstruction.CEI_OA_WarehouseInfo, message);
					entryinstruction.CEI_OA_Warehouse = organisation.PK;
					AssertNoMessageError("import,not ucc6, invoiceLine has a 71 CPC, procedure in listCPC, CEI_OA_Warehouse is not empty => no error.", entryinstruction.CEI_OA_WarehouseInfo, message);
				}
			}
		}

		public void TestValidationDecider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					AssertType<UCC6ImportEntryInstructionValidationDecider>("IMP UCC6 declaration", instruction.Validation.ValidationDecider);
					declaration.JE_MessageType = "EXP";
					AssertType<UCC6ExportEntryInstructionValidationDecider>("EXP UCC6 declaration", instruction.Validation.ValidationDecider);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					declaration.JE_MessageType = "IMP";
					AssertNull("IMP declaration not UCC6", instruction.Validation.ValidationDecider);
					declaration.JE_MessageType = "EXP";
					AssertNull("EXP declaration not UCC6", instruction.Validation.ValidationDecider);
				}
			});
		}

		public void TestCheckMaximumEntryLinesNumber()
		{
			const string expectedMessageError = "The number of entry lines in this entry instruction is greater than the 999 allowed in the message\r\nYou can create an additional Entry instruction to move invoices and/or invoice lines to it.";

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			using (var testContext = new EntryInstructionValidationDeciderTestContext(declaration, isUCC6: true))
			{
				testContext.EnableRule(x => x.IsMaximumEntryLinesAllowedRuleActive);
				CombineAssertions(() =>
				{
					entryInstruction.Validation.ValidateMaximumEntryLineNumber();
					AssertNoRowMessageError(
						"When entry instruction has no entry header/lines",
						entryInstruction,
						expectedMessageError);

					Enumerable.Range(0, 999).ForEach(x => entryHeader.MergedLines.AddNew());
					entryInstruction.Validation.ValidateMaximumEntryLineNumber();
					AssertNoRowMessageError(
						"When the entry header related to the entry instruction has a number of entry lines <= 999",
						entryInstruction,
						expectedMessageError);

					entryHeader.MergedLines.AddNew();
					entryInstruction.Validation.ValidateAll();
					AssertHasRowMessageError(
						"When the entry header related to the entry instruction has a number of entry lines > 999",
						entryInstruction,
						expectedMessageError);
				});

				testContext.DisableRule(x => x.IsMaximumEntryLinesAllowedRuleActive);
				entryInstruction.ClearAllNotifications();
				entryInstruction.Validation.ValidateMaximumEntryLineNumber();
				AssertNoRowMessageError(
						"When the entry header related to the entry instruction has a number of entry lines > 999 but the rule is not active",
						entryInstruction,
						expectedMessageError);
			}
		}
	}
}
