using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

sealed class ExportAddInfoJobDeclarationValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckZG_AgreedPlaceCode_UCC6()
	{
		using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			CombineAssertions(() =>
			{
				declaration.JE_ShipmentIncoTerm = "AH3";
				declaration.ZG_AgreedPlaceCode = "1";
				AssertNoMessageErrorContaining("Assert mandatory ZG_AgreedPlaceCode has value for Export", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.ZG_AgreedPlaceCode = ZString.Empty;
				AssertHasMessageErrorContaining("Assert mandatory ZG_AgreedPlaceCode has no value for Export", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.ZG_AgreedPlaceCode = "XXX";
				AssertNoMessageErrorContaining("Assert mandatory ZG_AgreedPlaceCode has value XXX for Export", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				declaration.ZG_AgreedPlaceCode = "1";
				AssertNoMessageErrorContaining("Assert mandatory ZG_AgreedPlaceCode has value for Export with more than 1 entry instruction", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.ZG_AgreedPlaceCode = ZString.Empty;
				AssertHasMessageErrorContaining("Assert mandatory ZG_AgreedPlaceCode has no value for Export with more than 1 entry instruction", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
				AssertNoMessageErrorContaining("Assert mandatory ZG_AgreedPlaceCode has no value for Export with more than 1 entry instruction EXS T2L", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.T2C;
				declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
				AssertNoMessageErrorContaining("Assert mandatory ZG_AgreedPlaceCode has no value for Export with more than 1 entry instruction T2L T2C", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				declaration.JE_ShipmentIncoTerm = ZString.Empty;
				declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
				AssertNoMessageErrorContaining("Assert mandatory ZG_AgreedPlaceCode has no value for Export with empty IncoTerm", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}

	public void TestCheckZG_AgreedPlaceCode_Empty_OnlyOneInstructionEXS()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.ZG_AgreedPlaceCode = ZString.Empty;
		declaration.JE_ShipmentIncoTermPlace = ZString.Empty;
		declaration.JE_ShipmentIncoTerm = "AH3";

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew().CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
		CombineAssertions(() =>
		{
			declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
			AssertNoMessageErrorContaining("ZG_AgreedPlaceCodeInfo for only one instruction with EXS", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
			declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
			AssertHasMessageErrorContaining("ZG_AgreedPlaceCodeInfo for two instruction (EXS and A)", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

			entryInstruction2.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
			AssertNoMessageErrorContaining("ZG_AgreedPlaceCodeInfo for two instruction with EXS", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckZG_IsSecurityDeclaration()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.EXNOSEGU, "Description");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Spain, UniversalReferenceConstants.RefCusCodeListTypes.EXNOSEGU, Core.Constants.CountryCodes.Spain, "Spain", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		Factory.Save();

		var warningText = "Export AES: If country of destination is not in EXNOSEGU, Security data (EXS) must be submitted. Security checkbox should be ticked.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Spain;

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader1.ZG_UCC6Version = 1;
		entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader2.ZG_UCC6Version = 1;
		entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;

		CombineAssertions(() =>
		{
			declaration.ZG_IsSecurityDeclaration = true;
			declaration.AddInfoValidation.ValidateZG_IsSecurityDeclaration();
			AssertNoWarningContaining("When ZG_IsSecurityDeclaration is true there is no warning", declaration.ZG_IsSecurityDeclarationInfo, warningText);

			declaration.ZG_IsSecurityDeclaration = false;
			declaration.AddInfoValidation.ValidateZG_IsSecurityDeclaration();
			AssertNoWarningContaining("When ZG_IsSecurityDeclaration is false and there is at least one Ucc6 entry header with entry instruction A (in A, B, C, Y, Z) but destination country is in EXNOSEGU there is no warning", declaration.ZG_IsSecurityDeclarationInfo, warningText);

			declaration.JE_GoodsDestination = "AU";
			declaration.AddInfoValidation.ValidateZG_IsSecurityDeclaration();
			AssertHasWarningContaining("When ZG_IsSecurityDeclaration is false, destination country is not in EXNOSEGU and there is at least one Ucc6 entry header with entry instruction A (in A, B, C, Y, Z) there is a warning", declaration.ZG_IsSecurityDeclarationInfo, warningText);

			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			declaration.AddInfoValidation.ValidateZG_IsSecurityDeclaration();
			AssertNoWarningContaining("When ZG_IsSecurityDeclaration is false, destination country is not in EXNOSEGU and there is at least one Ucc6 entry header but without entry instruction A, B, C, Y, Z there is no warning", declaration.ZG_IsSecurityDeclarationInfo, warningText);

			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.B;
			declaration.AddInfoValidation.ValidateZG_IsSecurityDeclaration();
			AssertHasWarningContaining("When ZG_IsSecurityDeclaration is false, destination country is not in EXNOSEGU and there is at least one Ucc6 entry header with entry instruction B (in A, B, C, Y, Z) there is a warning", declaration.ZG_IsSecurityDeclarationInfo, warningText);

			entryHeader1.ZG_UCC6Version = 0;
			declaration.AddInfoValidation.ValidateZG_IsSecurityDeclaration();
			AssertNoWarningContaining("When ZG_IsSecurityDeclaration is false, destination country is not in EXNOSEGU and there is at least one entry header with entry instruction A, B, C, Y, Z but it is not ucc6 there is no warning", declaration.ZG_IsSecurityDeclarationInfo, warningText);
		});
	}

	public void TestCheckZG_CTStatusID_T2L()
	{
		var warningText = "For Export AES declarations, if T2L value is selected, country of destination must be Andorra and all Procedure Code in entry lines must start with 10 or 21.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.ZG_CTStatusID = "T2L";

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader1.ZG_UCC6Version = 1;
		entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
		var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryLine1 = entryHeader1.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;

		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader2.ZG_UCC6Version = 0;
		entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
		var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine2.JI_FormattedProcedure = "2100";
		var entryLine2 = entryHeader2.MergedLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;

		CombineAssertions(() =>
		{
			declaration.JE_GoodsDestination = "AD";
			declaration.AddInfoValidation.ValidateZG_CTStatusID();
			AssertHasWarningContaining("When ZG_CTStatusID is T2L, at least one entry is ucc6 and country of destination is AD but not all line CPCs start with 10 or 21 there is a warning", declaration.ZG_CTStatusIDInfo, warningText);

			invoiceLine1.JI_FormattedProcedure = "1000";
			declaration.AddInfoValidation.ValidateZG_CTStatusID();
			AssertNoWarningContaining("When ZG_CTStatusID is T2L, at least one entry is ucc6 and country of destination is AD and all line CPCs start with 10 or 21 there is no warning", declaration.ZG_CTStatusIDInfo, warningText);

			declaration.JE_GoodsDestination = "FR";
			declaration.AddInfoValidation.ValidateZG_CTStatusID();
			AssertHasWarningContaining("When ZG_CTStatusID is T2L, at least one entry is ucc6 and country of destination is not AD there is a warning", declaration.ZG_CTStatusIDInfo, warningText);

			entryHeader1.ZG_UCC6Version = 0;
			declaration.AddInfoValidation.ValidateZG_CTStatusID();
			AssertNoWarningContaining("When ZG_CTStatusID is T2L country of destination is not AD but no entry is ucc6 there is no warning", declaration.ZG_CTStatusIDInfo, warningText);
		});
	}

	public void TestCheckZG_CTStatusIDRequiredForT2lPousAndExport()
	{
		var errorText = "CT Status is required for T2L request";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;

		CombineAssertions(() =>
		{
			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.ProofOfUnionStatus))
			{
				declaration.AddInfoValidation.ValidateZG_CTStatusID();
				AssertNoMessageErrorContaining("POUS When empty Entry Instruction and Export and ZG_CTStatusID empty", declaration.ZG_CTStatusIDInfo, errorText);

				var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;

				declaration.AddInfoValidation.ValidateZG_CTStatusID();
				AssertNoMessageErrorContaining("POUS When none Entry Instruction is T2l and Export and ZG_CTStatusID empty", declaration.ZG_CTStatusIDInfo, errorText);

				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				declaration.AddInfoValidation.ValidateZG_CTStatusID();
				AssertHasMessageErrorContaining("POUS When any Entry Instruction is T2l and Export and ZG_CTStatusID empty", declaration.ZG_CTStatusIDInfo, errorText);

				declaration.ZG_CTStatusID = "T2L";
				declaration.AddInfoValidation.ValidateZG_CTStatusID();
				AssertNoMessageErrorContaining("POUS When any Entry Instruction is T2l and Export and ZG_CTStatusID not empty", declaration.ZG_CTStatusIDInfo, errorText);

				declaration.ZG_CTStatusID = ZString.Empty;
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.AddInfoValidation.ValidateZG_CTStatusID();
				AssertNoMessageErrorContaining("POUS When any Entry Instruction is T2l and Import and ZG_CTStatusID empty", declaration.ZG_CTStatusIDInfo, errorText);
			}

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.NoProofOfUnionStatus))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;

				var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

				declaration.AddInfoValidation.ValidateZG_CTStatusID();
				AssertNoMessageErrorContaining("NOPOUS When any Entry Instruction is T2l and Export and ZG_CTStatusID empty", declaration.ZG_CTStatusIDInfo, errorText);
			}

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.RequestJecAndReceptionPous))
			{
				declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
				declaration.AddInfoValidation.ValidateZG_CTStatusID();
				AssertNoMessageErrorContaining("POUS2 When empty Entry Instruction and Export and ZG_CTStatusID empty", declaration.ZG_CTStatusIDInfo, errorText);

				var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;

				declaration.AddInfoValidation.ValidateZG_CTStatusID();
				AssertNoMessageErrorContaining("POUS2 When none Entry Instruction is T2l and Export and ZG_CTStatusID empty", declaration.ZG_CTStatusIDInfo, errorText);

				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				declaration.AddInfoValidation.ValidateZG_CTStatusID();
				AssertHasMessageErrorContaining("POUS2 When any Entry Instruction is T2l and Export and ZG_CTStatusID empty", declaration.ZG_CTStatusIDInfo, errorText);

				declaration.ZG_CTStatusID = "T2L";
				declaration.AddInfoValidation.ValidateZG_CTStatusID();
				AssertNoMessageErrorContaining("POUS2 When any Entry Instruction is T2l and Export and ZG_CTStatusID not empty", declaration.ZG_CTStatusIDInfo, errorText);

				declaration.ZG_CTStatusID = ZString.Empty;
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.AddInfoValidation.ValidateZG_CTStatusID();
				AssertNoMessageErrorContaining("POUS2 When any Entry Instruction is T2l and Import and ZG_CTStatusID empty", declaration.ZG_CTStatusIDInfo, errorText);
			}
		});
	}

	public void TestCheckZG_CTStatusID_T2LF()
	{
		var warningText = "For Export AES declarations, if T2LF value is selected, country of destination must be Spain (Canary Islands) and all Procedure Code in entry lines must start with 10, 21 or 23 and Entry Style must be CO.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.ZG_CTStatusID = "T2LF";
		declaration.JE_EntryStyle = "CO";

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader1.ZG_UCC6Version = 1;
		entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
		var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryLine1 = entryHeader1.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;

		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader2.ZG_UCC6Version = 0;
		entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
		var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine2.JI_FormattedProcedure = "2100";
		var entryLine2 = entryHeader2.MergedLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;

		CombineAssertions(() =>
		{
			declaration.JE_GoodsDestination = "ES";
			declaration.AddInfoValidation.ValidateZG_CTStatusID();
			AssertHasWarningContaining("When ZG_CTStatusID is T2LF, at least one entry is ucc6 and country of destination is ES, entry style is CO but not all line CPCs start with 10, 21 or 23 there is a warning", declaration.ZG_CTStatusIDInfo, warningText);

			invoiceLine1.JI_FormattedProcedure = "1000";
			declaration.AddInfoValidation.ValidateZG_CTStatusID();
			AssertNoWarningContaining("When ZG_CTStatusID is T2LF, at least one entry is ucc6 and country of destination is ES, entry style is CO and all line CPCs start with 10, 21 or 23 there is no warning", declaration.ZG_CTStatusIDInfo, warningText);

			declaration.JE_GoodsDestination = "FR";
			declaration.AddInfoValidation.ValidateZG_CTStatusID();
			AssertHasWarningContaining("When ZG_CTStatusID is T2LF, at least one entry is ucc6 and country of destination is not ES there is a warning", declaration.ZG_CTStatusIDInfo, warningText);

			entryHeader1.ZG_UCC6Version = 0;
			declaration.AddInfoValidation.ValidateZG_CTStatusID();
			AssertNoWarningContaining("When ZG_CTStatusID is T2LF country of destination is not ES but no entry is ucc6 there is no warning", declaration.ZG_CTStatusIDInfo, warningText);

			declaration.JE_GoodsDestination = "ES";
			declaration.JE_EntryStyle = "EX";
			entryHeader1.ZG_UCC6Version = 1;
			declaration.AddInfoValidation.ValidateZG_CTStatusID();
			AssertHasWarningContaining("When ZG_CTStatusID is T2LF, at least one entry is ucc6 and entry style is not CO there is a warning", declaration.ZG_CTStatusIDInfo, warningText);
		});
	}

	public void TestCheckZG_LCPDepart_IsEmpty()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.ZG_UCC6Version = 1;
		entryHeader.MovementReferenceNumber = "ABC123";
		declaration.ZG_LCPDepart = ZDateTime.Empty;

		var expectedError = "If exists any declaration with sub style Y or Z, EIDR date is mandatory.";

		CombineAssertions(() =>
		{
			AssertHasMessageError("When Entry Instructions/Sub Style with value C, MRN is not empty", declaration.ZG_LCPDepartInfo, expectedError);

			entryHeader.MovementReferenceNumber = ZString.Empty;
			declaration.AddInfoValidation.ValidateZG_LCPDepart();
			AssertNoMessageError("When Entry Instructions/Sub Style with value C, MRN is empty", declaration.ZG_LCPDepartInfo, expectedError);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Y;
			declaration.AddInfoValidation.ValidateZG_LCPDepart();
			AssertHasMessageError("When Entry Instructions/Sub Style with value Y, MRN is empty", declaration.ZG_LCPDepartInfo, expectedError);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			declaration.AddInfoValidation.ValidateZG_LCPDepart();
			AssertNoMessageError("No error when Entry Instructions/Sub Style is not Y/Z/C.", declaration.ZG_LCPDepartInfo, expectedError);

			entryHeader.MovementReferenceNumber = "ABC123";
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Y;
			declaration.AddInfoValidation.ValidateZG_LCPDepart();
			AssertHasMessageError("When Entry Instructions/Sub Style with value Y, MRN is not empty", declaration.ZG_LCPDepartInfo, expectedError);

			entryHeader.ZG_UCC6Version = 0;
			declaration.AddInfoValidation.ValidateZG_LCPDepart();
			AssertNoMessageError("When UCC6Version <> 1", declaration.ZG_LCPDepartInfo, expectedError);

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.Z;
			entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
			entryHeader2.ZG_UCC6Version = 1;
			entryHeader2.MovementReferenceNumber = ZString.Empty;

			declaration.AddInfoValidation.ValidateZG_LCPDepart();
			AssertHasMessageError("When Entry Instructions/Sub Style with value Z, MRN is empty", declaration.ZG_LCPDepartInfo, expectedError);

			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			declaration.AddInfoValidation.ValidateZG_LCPDepart();
			AssertNoMessageError("No EntryHeader with UCC6Version = 1 and corresponding Entry Instructions/Sub Style with value Y/Z/C", declaration.ZG_LCPDepartInfo, expectedError);

			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.Z;
			entryHeader.MovementReferenceNumber = "ABC123";
			declaration.AddInfoValidation.ValidateZG_LCPDepart();
			AssertHasMessageError("When Entry Instructions/Sub Style with value Z, MRN is not empty", declaration.ZG_LCPDepartInfo, expectedError);
		});
	}

	public void TestCheckZG_LCPDepart_IsFuture()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Y;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.ZG_UCC6Version = 1;
		var expectedError = "EIDR date cannot be higher to current date.";

		CombineAssertions(() =>
		{
			declaration.ZG_LCPDepart = ZDateTime.UtcToday.AddDays(-2);
			AssertNoMessageError("When Entry Instructions/Sub Style with value Y and ZG_LCPDepartInfo <= CurrentDate", declaration.ZG_LCPDepartInfo, expectedError);

			declaration.ZG_LCPDepart = ZDateTime.UtcToday.AddDays(4);
			AssertHasMessageError("When Entry Instructions/Sub Style with value Y and ZG_LCPDepartInfo > CurrentDate", declaration.ZG_LCPDepartInfo, expectedError);

			entryHeader.MovementReferenceNumber = ZString.Empty;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			declaration.AddInfoValidation.ValidateZG_LCPDepart();
			AssertNoMessageError("When Entry Instructions/Sub Style with value C, MRN is empty", declaration.ZG_LCPDepartInfo, expectedError);

			entryHeader.MovementReferenceNumber = "ABC123";
			declaration.ZG_LCPDepart = ZDateTime.UtcToday.AddDays(-2);
			AssertNoMessageError("When Entry Instructions/Sub Style with value C, MRN is not empty and ZG_LCPDepartInfo <= CurrentDate", declaration.ZG_LCPDepartInfo, expectedError);

			declaration.ZG_LCPDepart = ZDateTime.UtcToday.AddDays(4);
			AssertHasMessageError("When Entry Instructions/Sub Style with value C, MRN is not empty and ZG_LCPDepartInfo > CurrentDate", declaration.ZG_LCPDepartInfo, expectedError);

			entryHeader.ZG_UCC6Version = 0;
			declaration.AddInfoValidation.ValidateZG_LCPDepart();
			AssertNoMessageError("When UCC6Version <> 1", declaration.ZG_LCPDepartInfo, expectedError);

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.Z;
			entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
			entryHeader2.ZG_UCC6Version = 1;
			entryHeader2.MovementReferenceNumber = ZString.Empty;

			declaration.ZG_LCPDepart = ZDateTime.UtcToday.AddDays(-2);
			AssertNoMessageError("When Entry Instructions/Sub Style with value Z and ZG_LCPDepartInfo <= CurrentDate", declaration.ZG_LCPDepartInfo, expectedError);

			declaration.ZG_LCPDepart = ZDateTime.UtcToday.AddDays(4);
			AssertHasMessageError("When Entry Instructions/Sub Style with value Z and ZG_LCPDepartInfo > CurrentDate", declaration.ZG_LCPDepartInfo, expectedError);

			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			declaration.AddInfoValidation.ValidateZG_LCPDepart();
			AssertNoMessageError("No EntryHeader with UCC6Version = 1, corresponding Entry Instructions/Sub Style with value Y/Z/C and ZG_LCPDepartInfo > CurrentDate", declaration.ZG_LCPDepartInfo, expectedError);
		});
	}
}
