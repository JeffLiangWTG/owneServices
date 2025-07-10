using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.IE.Business.Constants;
using static Enterprise.Customs.IE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ImportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationTest<ImportJobComInvoiceLineValidation>
	{
		protected override string MessageType => IEJobMessageTypeList.Codes.Import;

		public void TestCheckJI_Tariff_BR1115()
		{
			var errorMessage = "[BR1115] Tariff Code must start with 03 when Additional Procedure F21 or F22 is declared.";
			(_, var invoiceLine, _, _) = SetupData();
			var info = invoiceLine.JI_TariffInfo;
			invoiceLine.JI_Procedure = "4000F21";
			invoiceLine.JI_Tariff = "0303001010";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError("JI_Tariff starts with 03", info, errorMessage);
			invoiceLine.JI_Tariff = "2203001010";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageError("JI_Tariff not starts with 03, JI_Procedure ends with F21", info, errorMessage);
			invoiceLine.JI_Procedure = "4000F22";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageError("JI_Tariff not starts with 03, JI_Procedure ends with F22", info, errorMessage);
			invoiceLine.JI_Procedure = "4000F55";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError("JI_Tariff not starts with 03, JI_Procedure not ends with F21/F22", info, errorMessage);
		}

		public void TestCheckJI_LinePrice_BR8072_H1_A() => AssertCheckJI_LinePrice(ImportDeclarationTypeList.Codes.H1, EntrySubStyleList.Codes.NormalDeclaration);

		public void TestCheckJI_LinePrice_BR8072_H3_A() => AssertCheckJI_LinePrice(ImportDeclarationTypeList.Codes.H3, EntrySubStyleList.Codes.NormalDeclaration);

		public void TestCheckJI_LinePrice_BR8072_H4_A() => AssertCheckJI_LinePrice(ImportDeclarationTypeList.Codes.H4, EntrySubStyleList.Codes.NormalDeclaration);

		public void TestCheckJI_LinePrice_BR8072_H5_A() => AssertCheckJI_LinePrice(ImportDeclarationTypeList.Codes.H5, EntrySubStyleList.Codes.NormalDeclaration);

		public void TestCheckJI_LinePrice_BR8072_H1_D() => AssertCheckJI_LinePrice(ImportDeclarationTypeList.Codes.H1, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA);

		public void TestCheckJI_LinePrice_BR8072_H3_D() => AssertCheckJI_LinePrice(ImportDeclarationTypeList.Codes.H3, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA);

		public void TestCheckJI_LinePrice_BR8072_H4_D() => AssertCheckJI_LinePrice(ImportDeclarationTypeList.Codes.H4, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA);

		public void TestCheckJI_LinePrice_BR8072_H5_D() => AssertCheckJI_LinePrice(ImportDeclarationTypeList.Codes.H5, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA);

		void AssertCheckJI_LinePrice(string declarationType, string additionalDeclarationType)
		{
			(_, var invoiceLine, _, var declaration) = SetupData();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var linePriceInfo = invoiceLine.JI_LinePriceInfo;
			CombineAssertions(declarationType, () =>
			{
				AssertNoMessageError("No Declaration Type, No error", linePriceInfo, Message_BR0072);
				instruction.CEI_Style = declarationType;
				instruction.CEI_SubStyle = additionalDeclarationType;
				invoiceLine.JI_LinePrice = 0;
				AssertHasMessageError("Declaration Type, Has error", linePriceInfo, Message_BR0072);
				invoiceLine.JI_LinePrice = 10;
				AssertNoMessageError("Declaration Type, No error", linePriceInfo, Message_BR0072);
			});
		}

		public void TestCheckJI_Tariff_CD0104_H1() => AssertCheckJI_Tariff(ImportDeclarationTypeList.Codes.H1);

		public void TestCheckJI_Tariff_CD0104_H2() => AssertCheckJI_Tariff(ImportDeclarationTypeList.Codes.H2);

		public void TestCheckJI_Tariff_CD0104_H3() => AssertCheckJI_Tariff(ImportDeclarationTypeList.Codes.H3);

		public void TestCheckJI_Tariff_CD0104_H4() => AssertCheckJI_Tariff(ImportDeclarationTypeList.Codes.H4);

		public void TestCheckJI_Tariff_CD0104_H5() => AssertCheckJI_Tariff(ImportDeclarationTypeList.Codes.H5);

		public void TestCheckJI_Tariff_CD0104_H6() => AssertCheckJI_Tariff(ImportDeclarationTypeList.Codes.H6);

		void AssertCheckJI_Tariff(string declarationType)
		{
			(_, var invoiceLine, _, var declaration) = SetupData();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var tariffInfo = invoiceLine.JI_TariffInfo;
			CombineAssertions(declarationType, () =>
			{
				AssertNoMessageError("No Declaration Type, No error", tariffInfo, Message_CD0104);
				instruction.CEI_Style = declarationType;
				invoiceLine.JI_Tariff = "";
				AssertHasMessageError("Declaration Type, Has error", tariffInfo, Message_CD0104);
				invoiceLine.JI_Tariff = "1234567";
				AssertNoMessageError("Declaration Type, No error", tariffInfo, Message_CD0104);
			});
		}

		public void TestCheckAdditionalProcedureCodesAsString_RuleBR600008()
		{
			var messageError = "[BR600008] If [11 10 001 000] Additional Procedure contains 'F49', then  [12 03 002 000] Supporting Document type must have '1A06' and [12 03 001 000] Supporting Document Reference Number should have the authorization number.";
			(_, var line, _, var dec) = SetupData();
			dec.JE_MessageType = "IMP";
			line.JI_Procedure = "00P1001";
			var collection = line.AdditionalProcedureCodes;
			(collection.FirstOrDefault() ?? collection.AddNew()).CY_Code = "1000C01";
			var cusEntryInstruction = line.EntryInstruction;

			CombineAssertions(() =>
			{
				line.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("No F49 in AdditionalProcedureCodes", line.AdditionalProcedureCodesAsStringInfo, messageError);
				var additionalProcedure = collection.FirstOrDefault();
				additionalProcedure.CY_Code = "1000F49";
				line.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("Have F49 in AdditionalProcedureCodes but no valid support document", line.AdditionalProcedureCodesAsStringInfo, messageError);
				var supportDocument = line.SupportingDocuments.AddNew();
				supportDocument.CSI_Code = "1A06";
				supportDocument.CSI_ReferenceNumber = "Ye";
				line.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("Have F49 in AdditionalProcedureCodes and valid support document", line.AdditionalProcedureCodesAsStringInfo, messageError);
				supportDocument.CSI_Code = "1A05";
				line.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("Have F49 in AdditionalProcedureCodes but no valid support document", line.AdditionalProcedureCodesAsStringInfo, messageError);

				if (cusEntryInstruction != null &&
				(cusEntryInstruction.CEI_Style == ImportDeclarationTypeList.Codes.H1 || cusEntryInstruction.CEI_Style == ImportDeclarationTypeList.Codes.H5))
				{
					var supportDoc = cusEntryInstruction.SupportingDocuments.AddNew();
					supportDoc.CSI_Code = "1A06";
					supportDoc.CSI_ReferenceNumber = "Ye";

					line.Validation.ValidateAdditionalProcedureCodesAsString();
					AssertNoMessageError("Have F49 in AdditionalProcedureCodes and valid support document", line.AdditionalProcedureCodesAsStringInfo, messageError);
				}
			});
		}

		public void TestCheckJI_Procedure_BR600009HaveF48()
		{
			const string messageError = "[BR600009] Please enter a Fiscal Reference where Code is 'FR5' under Entry Instruction> Fiscal References.";
			(_, var invoiceLine, _, var declaration) = SetupData();
			var referenceInvoiceLine = invoiceLine.FiscalReferences.AddNew();
			var instruction = Factory.New<CusEntryInstruction>();
			invoiceLine.JI_CEI = instruction.PK;
			var referenceInstruction = instruction.FiscalReferences.AddNew();

			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "4800F07";
				AssertNoMessageError("No additional procedure code F48", invoiceLine.JI_ProcedureInfo, messageError);

				invoiceLine.JI_Procedure = "1234F48";
				AssertHasMessageError("Have additional procedure code F48 and no fiscal reference code FR5", invoiceLine.JI_ProcedureInfo, messageError);

				referenceInvoiceLine.CFR_Code = "FR5";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("Have additional procedure code F48 and have fiscal reference code FR5", invoiceLine.JI_ProcedureInfo, messageError);

				referenceInvoiceLine.CFR_Code = "FR4";
				referenceInstruction.CFR_Code = "FR5";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("Have additional procedure code F48 and have fiscal reference code FR5", invoiceLine.JI_ProcedureInfo, messageError);
			});
		}

		public void TestCheckJI_Procedure_BR600009NotHaveF48()
		{
			const string messageError = "[BR600009] Please enter an Additional Procedure Code where Code is 'F48'.";
			(_, var invoiceLine, _, var declaration) = SetupData();
			var referenceInvoiceLine = invoiceLine.FiscalReferences.AddNew();
			var instruction = Factory.New<CusEntryInstruction>();
			invoiceLine.JI_CEI = instruction.PK;
			var referenceInstruction = instruction.FiscalReferences.AddNew();

			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "4800B07";
				AssertNoMessageError("No additional procedure code F48", invoiceLine.JI_ProcedureInfo, messageError);

				referenceInvoiceLine.CFR_Code = "FR5";
				invoiceLine.JI_Procedure = "1234F08";
				AssertHasMessageError("No additional procedure code F48 and have fiscal reference code FR5", invoiceLine.JI_ProcedureInfo, messageError);

				invoiceLine.JI_Procedure = "1234F48";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("Have additional procedure code F48 and have fiscal reference code FR5", invoiceLine.JI_ProcedureInfo, messageError);

				referenceInvoiceLine.CFR_Code = "FR4";
				referenceInstruction.CFR_Code = "FR5";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("Have additional procedure code F48 and have fiscal reference code FR5", invoiceLine.JI_ProcedureInfo, messageError);
			});
		}

		public void TestCheckAdditionalProcedureCodesAsString_BR600009HaveF48()
		{
			const string messageError = "[BR600009] Please enter a Fiscal Reference where Code is 'FR5' under Entry Instruction> Fiscal References.";
			(_, var invoiceLine, _, var declaration) = SetupData();
			var referenceInvoiceLine = invoiceLine.FiscalReferences.AddNew();
			var instruction = Factory.New<CusEntryInstruction>();
			invoiceLine.JI_CEI = instruction.PK;
			var referenceInstruction = instruction.FiscalReferences.AddNew();
			var additionalProcedure = invoiceLine.AdditionalProcedureCodes.AddNew();

			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "4800";
				additionalProcedure.CY_Code = "1234B07";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("No additional procedure code F48", invoiceLine.AdditionalProcedureCodesAsStringInfo, messageError);

				additionalProcedure.CY_Code = "1234F48";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("Have additional procedure code F48 and no fiscal reference code FR5", invoiceLine.AdditionalProcedureCodesAsStringInfo, messageError);

				referenceInvoiceLine.CFR_Code = "FR5";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("Have additional procedure code F48 and have fiscal reference code FR5", invoiceLine.AdditionalProcedureCodesAsStringInfo, messageError);

				referenceInvoiceLine.CFR_Code = "FR4";
				referenceInstruction.CFR_Code = "FR5";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("Have additional procedure code F48 and have fiscal reference code FR5", invoiceLine.AdditionalProcedureCodesAsStringInfo, messageError);
			});
		}

		public void TestCheckAdditionalProcedureCodesAsString_BR600009NotHaveF48()
		{
			const string messageError = "[BR600009] Please enter an Additional Procedure Code where Code is 'F48'.";
			(_, var invoiceLine, _, var declaration) = SetupData();
			var referenceInvoiceLine = invoiceLine.FiscalReferences.AddNew();
			var instruction = Factory.New<CusEntryInstruction>();
			invoiceLine.JI_CEI = instruction.PK;
			var referenceInstruction = instruction.FiscalReferences.AddNew();
			var additionalProcedure = invoiceLine.AdditionalProcedureCodes.AddNew();

			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "4800";
				additionalProcedure.CY_Code = "1234B07";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("No additional procedure code F48", invoiceLine.AdditionalProcedureCodesAsStringInfo, messageError);

				referenceInvoiceLine.CFR_Code = "FR5";
				additionalProcedure.CY_Code = "1234B08";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("No additional procedure code F48 and have fiscal reference code FR5", invoiceLine.AdditionalProcedureCodesAsStringInfo, messageError);

				additionalProcedure.CY_Code = "1234F48";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("Have additional procedure code F48 and have fiscal reference code FR5", invoiceLine.AdditionalProcedureCodesAsStringInfo, messageError);

				referenceInvoiceLine.CFR_Code = "FR4";
				referenceInstruction.CFR_Code = "FR5";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("Have additional procedure code F48 and have fiscal reference code FR5", invoiceLine.AdditionalProcedureCodesAsStringInfo, messageError);
			});
		}

		public void TestCheckJI_PrimaryPreference_BR4170()
		{
			(_, var invoiceLine, _, _) = SetupData();
			var entryInstruction = invoiceLine.EntryInstruction;
			var targetInfo = invoiceLine.JI_PrimaryPreferenceInfo;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H3;
			invoiceLine.JI_PrimaryPreference = "120";
			AssertHasMessageError("CEI_Style is not 'H1' and 2nd and 3rd characters of JI_PrimaryPreference are '20', '25', or '28'", targetInfo, Message_BR4170);
			invoiceLine.JI_PrimaryPreference = "225";
			AssertHasMessageError("CEI_Style is not 'H1' and 2nd and 3rd characters of JI_PrimaryPreference are '20', '25', or '28'", targetInfo, Message_BR4170);
			invoiceLine.JI_PrimaryPreference = "328";
			AssertHasMessageError("CEI_Style is not 'H1' and 2nd and 3rd characters of JI_PrimaryPreference are '20', '25', or '28'", targetInfo, Message_BR4170);
			invoiceLine.JI_PrimaryPreference = "121";
			AssertNoMessageError("CEI_Style is not 'H1' and 2nd and 3rd characters of JI_PrimaryPreference are not '20', '25', or '28'", targetInfo, Message_BR4170);

			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			invoiceLine.JI_PrimaryPreference = "120";
			AssertNoMessageError("CEI_Style is 'H1' and 2nd and 3rd characters of JI_PrimaryPreference are '20', '25', or '28'", targetInfo, Message_BR4170);
			invoiceLine.JI_PrimaryPreference = "225";
			AssertNoMessageError("CEI_Style is 'H1' and 2nd and 3rd characters of JI_PrimaryPreference are '20', '25', or '28'", targetInfo, Message_BR4170);
			invoiceLine.JI_PrimaryPreference = "328";
			AssertNoMessageError("CEI_Style is 'H1' and 2nd and 3rd characters of JI_PrimaryPreference are '20', '25', or '28'", targetInfo, Message_BR4170);
		}

		public void TestCheckJI_PrimaryPreference_BR4176()
		{
			var (_, invoiceLine, invoice, declaration) = SetupData();
			var entryInstruction = invoiceLine.EntryInstruction;
			var entryInstructionDocumentC100 = entryInstruction.SupportingDocuments.AddNew();
			var entryInstructionDocumentMissingOrInvalid = entryInstruction.SupportingDocuments.AddNew();
			var invoiceHeaderDocumentC100 = invoice.SupportingDocuments.AddNew();
			var invoiceHeaderDocumentMissingOrInvalid = invoice.SupportingDocuments.AddNew();
			const string condition1 = "1 show error only if JobComInvoiceLine.JI_PrimaryPreference = '200' or '218' or '220' or '225' or '250'";
			const string condition2a = "2a show error only if **Entry Instruction** / Inv Header has a Supporting Documend of type 'C100'";
			const string condition2b = "2b show error only if Entry Instruction / **Inv Header** has a Supporting Documend of type 'C100'";
			const string condition3a = "3a show error only if **Entry Instruction** / Inv Header is missing a Supporting Document of type 'U164' or 'U166' or 'N865'";
			const string condition3b = "3b show error only if Entry Instruction / **Inv Header** is missing a Supporting Document of type 'U164' or 'U166' or 'N865'";
			const string condition4a = "4a show error only if **Entry Instruction** / Inv Header has a Supporting Document of type 'U165' or 'U167'";
			const string condition4b = "4b show error only if Entry Instruction / **Inv Header** has a Supporting Document of type 'U165' or 'U167'";
			CombineAssertions("Asserting missing supporting document", () =>
			{
				const string errorMessage = Message_BR4176_MissingSupportingDocument;
				invoiceLine.JI_PrimaryPreference = "200";
				entryInstructionDocumentC100.CSI_Type = "SUP";
				entryInstructionDocumentC100.CSI_Code = "C101";
				entryInstructionDocumentMissingOrInvalid.CSI_Type = "SUP";
				entryInstructionDocumentMissingOrInvalid.CSI_Code = "U163";
				invoiceHeaderDocumentC100.CSI_Type = "SUP";
				invoiceHeaderDocumentC100.CSI_Code = "C101";
				invoiceHeaderDocumentMissingOrInvalid.CSI_Type = "SUP";
				invoiceHeaderDocumentMissingOrInvalid.CSI_Code = "U163";
				AssertHasMessageErrorIf<ZString>(condition1, invoiceLine.JI_PrimaryPreferenceInfo, "210", errorMessage, "200", "218", "220", "225", "250");
				AssertNoMessageErrorIf<ZString>(condition2a, entryInstructionDocumentC100.CSI_CodeInfo, "C101", errorMessage, "C100");
				AssertNoMessageErrorIf<ZString>(condition2b, invoiceHeaderDocumentC100.CSI_CodeInfo, "C101", errorMessage, "C100");
				AssertNoMessageErrorIf<ZString>(condition3a, entryInstructionDocumentMissingOrInvalid.CSI_CodeInfo, "U163", errorMessage, "U164", "U166", "N865");
				entryInstructionDocumentMissingOrInvalid.CSI_Code = "U163";
				AssertNoMessageErrorIf<ZString>(condition3b, invoiceHeaderDocumentMissingOrInvalid.CSI_CodeInfo, "U163", errorMessage, "U164", "U166", "N865");
				invoiceHeaderDocumentMissingOrInvalid.CSI_Code = "U163";
			});
			CombineAssertions("Asserting invalid supporting document", () =>
			{
				const string errorMessage = Message_BR4176_InvalidSupportingDocument;
				invoiceLine.JI_PrimaryPreference = "200";
				entryInstructionDocumentC100.CSI_Type = "SUP";
				entryInstructionDocumentC100.CSI_Code = "C101";
				entryInstructionDocumentMissingOrInvalid.CSI_Type = "SUP";
				entryInstructionDocumentMissingOrInvalid.CSI_Code = "U165";
				invoiceHeaderDocumentC100.CSI_Type = "SUP";
				invoiceHeaderDocumentC100.CSI_Code = "C101";
				invoiceHeaderDocumentMissingOrInvalid.CSI_Type = "SUP";
				invoiceHeaderDocumentMissingOrInvalid.CSI_Code = "U163";
				AssertHasMessageErrorIf<ZString>(condition1, invoiceLine.JI_PrimaryPreferenceInfo, "210", errorMessage, "200", "218", "220", "225", "250");
				AssertNoMessageErrorIf<ZString>(condition2a, entryInstructionDocumentC100.CSI_CodeInfo, "C101", errorMessage, "C100");
				AssertNoMessageErrorIf<ZString>(condition2b, invoiceHeaderDocumentC100.CSI_CodeInfo, "C101", errorMessage, "C100");
				AssertHasMessageErrorIf<ZString>(condition4a, entryInstructionDocumentMissingOrInvalid.CSI_CodeInfo, "U163", errorMessage, "U165", "U167");
				entryInstructionDocumentMissingOrInvalid.CSI_Code = "U163";
				invoiceHeaderDocumentMissingOrInvalid.CSI_Code = "U165";
				AssertHasMessageErrorIf<ZString>(condition4b, invoiceHeaderDocumentMissingOrInvalid.CSI_CodeInfo, "U163", errorMessage, "U165", "U167");
				invoiceHeaderDocumentMissingOrInvalid.CSI_Code = "U163";
			});
			CombineAssertions("When invoice header has C101 and instruction is null", () =>
			{
				invoiceLine.JI_CEI = ZGuid.Empty;
				invoiceHeaderDocumentC100.CSI_Type = "SUP";
				invoiceHeaderDocumentC100.CSI_Code = "C101";
				AssertNoExceptionThrown(() => invoiceLine.JI_PrimaryPreference = "200");
			});
			void AssertHasMessageErrorIf<T>(string assertMessage, ZPropertyInfo conditionalProperty, T validValue, string messageErrorText, params T[] triggeringValues) where T : IZType
				=> triggeringValues.ForEach(triggeringValue
					=> AssertConditionalMessageError($"{assertMessage} should have message error when {conditionalProperty.Name} is {triggeringValue}", conditionalProperty, messageErrorText, validValue, triggeringValue));
			void AssertNoMessageErrorIf<T>(string assertMessage, ZPropertyInfo conditionalProperty, T triggeringValue, string messageErrorText, params T[] validValues) where T : IZType
				=> validValues.ForEach(validValue
					=> AssertConditionalMessageError($"{assertMessage} should have no message error when {conditionalProperty.Name} is {validValue}", conditionalProperty, messageErrorText, validValue, triggeringValue));
			void AssertConditionalMessageError<T>(string assertMessage, ZPropertyInfo conditionalProperty, string messageErrorText, T validValue, T invalidValue) where T : IZType
			{
				conditionalProperty.Value = validValue;
				invoiceLine.JI_PrimaryPreference = invoiceLine.JI_PrimaryPreference;
				AssertNoMessageError($"{assertMessage}, {conditionalProperty.Name} = {validValue}", invoiceLine.JI_PrimaryPreferenceInfo, messageErrorText);

				conditionalProperty.Value = invalidValue;
				invoiceLine.JI_PrimaryPreference = invoiceLine.JI_PrimaryPreference;
				AssertHasMessageError($"{assertMessage}, {conditionalProperty.Name} = {invalidValue}", invoiceLine.JI_PrimaryPreferenceInfo, messageErrorText);
			}
		}

		public void TestCheckJI_PrimaryPreference_BR4175()
		{
			var errorMessage = "[BR4175] Please enter a Supporting Document Type of type 'U164' or 'U165' or ('U165' and 'U167' jointly) under Supporting Documents.";
			(var validation, var invoiceLine, _, _) = SetupData();
			invoiceLine.JI_PrimaryPreference = "200";

			AssertNoMessageError("No error when no support document code is C100", invoiceLine.JI_PrimaryPreferenceInfo, errorMessage);

			var supportdocumentHeader = invoiceLine.InvoiceHeader.SupportingDocuments.AddNew();
			supportdocumentHeader.CSI_Code = "C100";
			validation.ValidateJI_PrimaryPreference();
			AssertHasMessageError("Has error when have support document code is C100 and no support document code is U164 or U165", invoiceLine.JI_PrimaryPreferenceInfo, errorMessage);

			var instruction = invoiceLine.Declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var supportdocumentInstruction = instruction.SupportingDocuments.AddNew();
			supportdocumentInstruction.CSI_Code = "U165";
			validation.ValidateJI_PrimaryPreference();
			AssertNoMessageError("No error when have support document code is C100 and support document code is U164 or U165", invoiceLine.JI_PrimaryPreferenceInfo, errorMessage);
		}

		public void TestCheckJI_PrimaryPreference_BR4181()
		{
			(_, var invoiceLine, _, var declaration) = SetupData();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H5;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_PrimaryPreference = string.Empty;

			CombineAssertions(() =>
			{
				AssertHasMessageError("CEI_Style is 'H5' and JI_PrimaryPreference is empty", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR4181);

				invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.TemporaryTariffSuspension;
				AssertHasMessageError("CEI_Style is 'H5' and JI_PrimaryPreference is different from '100'", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR4181);

				invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.NormalThirdCountryTariffDuty;
				AssertNoMessageError("CEI_Style is 'H5' and JI_PrimaryPreference is '100'", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR4181);

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
				invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.TemporaryTariffSuspension;
				AssertNoMessageError("CEI_Style is 'H1'", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR4181);
			});
		}

		public void TestCheckJI_PrimaryPreference_BR5160()
		{
			(_, var invoiceLine, _, var declaration) = SetupData();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.ZG_CountryOfSupply = string.Empty;
			invoiceLine.JI_PrimaryPreference = string.Empty;

			CombineAssertions(() =>
			{
				AssertNoMessageError("ZG_CountryOfSupply is empty and JI_PrimaryPreference is empty", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5160);

				invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.TemporaryTariffQuota;
				AssertNoMessageError("ZG_CountryOfSupply is empty and JI_PrimaryPreference is '320'", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5160);

				invoiceLine.ZG_CountryOfSupply = Core.Constants.CountryCodes.KoreaSouth;
				invoiceLine.JI_PrimaryPreference = string.Empty;
				AssertNoMessageError("ZG_CountryOfSupply is 'KR' and JI_PrimaryPreference is empty", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5160);

				invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.TemporaryTariffQuota;
				AssertHasMessageError("ZG_CountryOfSupply is 'KR' and JI_PrimaryPreference is '320' without Supporting Document U059", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5160);

				var supportingDocument = entryInstruction.SupportingDocuments.AddNew();
				supportingDocument.CSI_Code = Constants.SupportingDocumentCodes._U059;
				invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.TemporaryTariffQuota;
				AssertNoMessageError("ZG_CountryOfSupply is 'KR' and JI_PrimaryPreference is '320' with Supporting Document U059", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5160);
			});
		}

		public void TestCheckJI_PrimaryPreference_BR5157()
		{
			(_, var invoiceLine, _, var declaration) = SetupData();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.ZG_CountryOfSupply = string.Empty;
			invoiceLine.JI_PrimaryPreference = string.Empty;

			CombineAssertions(() =>
			{
				AssertNoMessageError("ZG_CountryOfSupply is empty and JI_PrimaryPreference is empty", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5157);

				invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.TemporaryTariffQuota;
				AssertNoMessageError("ZG_CountryOfSupply is empty and JI_PrimaryPreference begins with '3'", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5157);

				invoiceLine.ZG_CountryOfSupply = Core.Constants.CountryCodes.UnitedKingdom;
				invoiceLine.JI_PrimaryPreference = string.Empty;
				AssertNoMessageError("ZG_CountryOfSupply is 'GB' and JI_PrimaryPreference is empty", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5157);

				invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.NormalThirdCountryTariffDuty;
				AssertNoMessageError("ZG_CountryOfSupply is 'GB' and JI_PrimaryPreference not begins with '3'", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5157);

				invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.TemporaryTariffQuota;
				AssertHasMessageError("ZG_CountryOfSupply is 'GB' and JI_PrimaryPreference begins with '3' without Supporting Document U116/U117/U118", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5157);

				var supportingDocument = entryInstruction.SupportingDocuments.AddNew();
				supportingDocument.CSI_Code = Constants.SupportingDocumentCodes._U116;
				invoiceLine.Validation.ValidateJI_PrimaryPreference();
				AssertNoMessageError("ZG_CountryOfSupply is 'GB' and JI_PrimaryPreference begins with '3' with Supporting Document U116", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5157);

				supportingDocument.CSI_Code = Constants.SupportingDocumentCodes._U117;
				invoiceLine.Validation.ValidateJI_PrimaryPreference();
				AssertNoMessageError("ZG_CountryOfSupply is 'GB' and JI_PrimaryPreference begins with '3' with Supporting Document U117", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5157);

				supportingDocument.CSI_Code = Constants.SupportingDocumentCodes._U118;
				invoiceLine.Validation.ValidateJI_PrimaryPreference();
				AssertNoMessageError("ZG_CountryOfSupply is 'GB' and JI_PrimaryPreference begins with '3' with Supporting Document U118", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5157);

				supportingDocument.CSI_Code = Constants.SupportingDocumentCodes._U059;
				invoiceLine.Validation.ValidateJI_PrimaryPreference();
				AssertHasMessageError("ZG_CountryOfSupply is 'GB' and JI_PrimaryPreference begins with '3' with Supporting Document not U116/U117/U118", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5157);
			});
		}

		public void TestCheckJI_PrimaryPreference_BR5159()
		{
			(_, var invoiceLine, _, var declaration) = SetupData();
			var entryInstruction = invoiceLine.EntryInstruction;

			var supportingDocumentOnEntryInstruction = entryInstruction.SupportingDocuments.AddNew();
			supportingDocumentOnEntryInstruction.CSI_Code = Constants.SupportingDocumentCodes._U059;

			var supportingDocumentOnInvoiceHeader = invoiceLine.InvoiceHeader.SupportingDocuments.AddNew();
			supportingDocumentOnInvoiceHeader.CSI_Code = Constants.SupportingDocumentCodes._U059;

			CombineAssertions(() =>
			{
				invoiceLine.ZG_CountryOfSupply = Core.Constants.CountryCodes.Pakistan;
				invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.TemporaryTariffQuota;
				AssertHasMessageError("ZG_CountryOfSupply is 'PK', JI_PrimaryPreference starts with '3', no U164 and U165 supporting docs", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5159);

				invoiceLine.ZG_CountryOfSupply = Core.Constants.CountryCodes.Latvia;
				invoiceLine.Validation.ValidateJI_PrimaryPreference();
				AssertNoMessageError("ZG_CountryOfSupply is 'LV', JI_PrimaryPreference starts with '3', no U164 and U165 supporting docs", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5159);

				invoiceLine.ZG_CountryOfSupply = Core.Constants.CountryCodes.Pakistan;
				invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.NormalThirdCountryTariffDuty;
				AssertNoMessageError("ZG_CountryOfSupply is 'PK', JI_PrimaryPreference starts with '1', no U164 and U165 supporting docs", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5159);

				invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.TemporaryTariffQuota;
				supportingDocumentOnEntryInstruction.CSI_Code = Constants.SupportingDocumentCodes._U164;
				invoiceLine.Validation.ValidateJI_PrimaryPreference();
				AssertNoMessageError("ZG_CountryOfSupply is 'PK', JI_PrimaryPreference starts with '3', U164 supporting doc on entry instruction", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5159);

				supportingDocumentOnEntryInstruction.CSI_Code = Constants.SupportingDocumentCodes._U165;
				invoiceLine.Validation.ValidateJI_PrimaryPreference();
				AssertNoMessageError("ZG_CountryOfSupply is 'PK', JI_PrimaryPreference starts with '3', U165 supporting doc on entry instruction", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5159);

				supportingDocumentOnEntryInstruction.CSI_Code = Constants.SupportingDocumentCodes._U059;
				supportingDocumentOnInvoiceHeader.CSI_Code = Constants.SupportingDocumentCodes._U164;
				invoiceLine.Validation.ValidateJI_PrimaryPreference();
				AssertNoMessageError("ZG_CountryOfSupply is 'PK', JI_PrimaryPreference starts with '3', U164 supporting doc on invoice header", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5159);

				supportingDocumentOnInvoiceHeader.CSI_Code = Constants.SupportingDocumentCodes._U165;
				invoiceLine.Validation.ValidateJI_PrimaryPreference();
				AssertNoMessageError("ZG_CountryOfSupply is 'PK', JI_PrimaryPreference starts with '3', U165 supporting doc on invoice header", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5159);
			});
		}

		public void TestCheckJI_PrimaryPreference_BR5150()
		{
			const string Message_BR5150 = "[BR5150] Please entry a Supporting Document of type 'N954' or 'N864' or 'U162' or 'U163' or 'U168' or 'U169' or 'U170' or 'U171' under Supporting Documents.";

			(_, var invoiceLine, _, var declaration) = SetupData();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_PrimaryPreference = string.Empty;
			AssertNoMessageError("ZG_CountryOfSupply is empty and JI_PrimaryPreference is empty without Supporting Doucument N864", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5150);

			invoiceLine.ZG_CountryOfSupply = Core.Constants.CountryCodes.Andorra;
			AssertNoMessageError("ZG_CountryOfSupply is 'AD' and JI_PrimaryPreference is empty without Supporting Doucument N864", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5150);

			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.TemporaryTariffQuota;
			AssertHasMessageError("ZG_CountryOfSupply is 'AD' and JI_PrimaryPreference is '320' without Supporting Doucument N864", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5150);

			var supportingDocument = entryInstruction.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = Constants.SupportingDocumentCodes._N864;
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.TemporaryTariffQuota;
			AssertNoMessageError("ZG_CountryOfSupply is 'AD' and JI_PrimaryPreference is '320' with Supporting Doucument N864", invoiceLine.JI_PrimaryPreferenceInfo, Message_BR5150);
		}

		const string Message_BR0072 = "[BR8072] Price is required when Declaration is H1, H3, H4, or H5, and Additional Declaration Type is A or D";
		const string Message_BR4170 = "[BR4170] Quota request preferences where the last two digits of the code is '20', '25' or '28' are only allowed when Declaration Type is 'H1'.";
		const string Message_BR4176_MissingSupportingDocument = "[BR4176] Please enter a Supporting Document of type 'U164' or 'U166' or 'N865' under Supporting Documents when Preference is '200' or '218' or '220' or '225' or '250' and there is no Supporting Document of type 'C100' declared on the Invoice Header or Entry Instruction.";
		const string Message_BR4176_InvalidSupportingDocument = "[BR4176] The Supporting Document of 'U165' or 'U167' under Supporting Documents / Invoice Header is not allowed when Preference is '200' or '218' or '220' or '225' or '250' and there is no Supporting Document of type 'C100' declared on the Invoice Header or Entry Instruction.";
		const string Message_BR4181 = "[BR4181] Preference must be 100 when Declaration is H5.";
		const string Message_CD0104 = "[CD0104] Tariff is required when Declaration is H1, H2, H3, H4, H5, or H6";
		const string Message_BR5157 = "[BR5157] A supporting document of type U116, U117, or U118 is required on the entry instruction when there is at least an invoice line with preference starting with 3 and the country of preferential origin is GB.";
		const string Message_BR5159 = "[BR5159] A supporting document of type U164 or U165 is required on the entry instruction when there is at least an invoice line where preference starts with 3 and country of preferential origin is PK.";
		const string Message_BR5160 = "[BR5160] A supporting document of type U059 is required on the entry instruction when there is at least an invoice line where preference is 320 and the country of preferential origin is KR.";
		const string MessageError_BR1112 = "[BR1112] When Requested Procedure is not '76' nor '77', and Transport Mode is 'AIR', then the only allowable combinations of two Transport Documents under the Additional Documents tab are 'N741' together with 'N703' or 'N740'.";

		public void TestIsTariffMandatory()
		{
			(var validation, _, _, _) = SetupData();
			AssertEquals(false, validation.GetType().GetProperty("IsTariffMandatory", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(validation));
		}

		static IEnumerable<(string submitType, string declarationType, string ceistyle, bool ruleAvailable, string description)> BR1117TestCases()
		{
			yield return (submitType: "V2", declarationType: "IM", ceistyle: "H5", ruleAvailable: false, "UCC6(V2), BR1117 unavailable for non-CO(IM) jobs.");
			yield return (submitType: "V2", declarationType: "CO", ceistyle: "H1", ruleAvailable: false, "UCC6(V2), BR1117 unavailable for non-H5(H1) jobs.");
			yield return (submitType: "V2", declarationType: "CO", ceistyle: "H5", ruleAvailable: true, "UCC6(V2), BR1117 available for CO-H5 jobs.");

			yield return (submitType: "V1", declarationType: "CO", ceistyle: "H5", ruleAvailable: false, "UCC5, BR1117 unavailable.");
		}

		public void TestCheckAdditionalProcedureCodesWith3CharactersAsString_BR1117()
		{
			var br1117message = "[BR1117] If Requested Procedure is '40', '42', '61', '63', '95' or '96', an 'F15' Additional Procedure must be declared.";
			(var validation, var invoiceLine, _, var declaration) = SetupData();
			var targetInfo = invoiceLine.AdditionalProcedureCodesAsStringInfo;

			var requestedProceduresForBR1117 = new[] { "40", "42", "61", "63", "95", "96" };

			foreach (var testCase in BR1117TestCases())
			{
				declaration.JE_ApplicationCode = testCase.submitType;
				declaration.JE_EntryStyle = testCase.declarationType;
				invoiceLine.EntryInstruction.CEI_Style = testCase.ceistyle;

				foreach (var requestedProcedure in requestedProceduresForBR1117)
				{
					var nonF15AdditionalProcedure = $"{requestedProcedure}00C01";
					var f15AdditionalProcedure = $"{requestedProcedure}00F15";

					invoiceLine.JI_Procedure = nonF15AdditionalProcedure;
					invoiceLine.AdditionalProcedureCodes.RemoveAndDeleteAll();
					var additionalProcedure = invoiceLine.AdditionalProcedureCodes.AddNew();
					additionalProcedure.CY_Code = nonF15AdditionalProcedure;

					validation.ValidateAdditionalProcedureCodesAsString();
					if (testCase.ruleAvailable)
					{
						AssertHasMessageError(testCase.description, targetInfo, br1117message);

						additionalProcedure.CY_Code = f15AdditionalProcedure;
						validation.ValidateAdditionalProcedureCodesAsString();
						AssertNoMessageError($"{testCase.description}, validation pass.", targetInfo, br1117message);
					}
					else
					{
						AssertNoMessageError(testCase.description, targetInfo, br1117message);
					}
				}
			}
		}

		public void TestCheckJI_Procedure_BR1126_40() => AssertCheckJI_Procedure_BR1126("4021F15");

		public void TestCheckJI_Procedure_BR1126_42() => AssertCheckJI_Procedure_BR1126("4221F15");

		public void TestCheckJI_Procedure_BR1126_61() => AssertCheckJI_Procedure_BR1126("6121F15");

		public void TestCheckJI_Procedure_BR1126_63() => AssertCheckJI_Procedure_BR1126("6321F15");

		public void TestCheckJI_Procedure_BR1126_95() => AssertCheckJI_Procedure_BR1126("9521F15");

		public void TestCheckJI_Procedure_BR1126_96() => AssertCheckJI_Procedure_BR1126("9621F15");

		void AssertCheckJI_Procedure_BR1126(string procedure)
		{
			const string message_BR1126 = "[BR1126] Requested Procedure must be '40', '42', '61', '63', '95' or '96' when Declaration is H5";
			(_, var invoiceLine, _, var declaration) = SetupData();
			var procedureInfo = invoiceLine.JI_ProcedureInfo;
			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = ImportDeclarationTypeList.Codes.H5;
			invoiceLine.JI_CEI = cei.PK;
			CombineAssertions(procedure, () =>
			{
				invoiceLine.JI_Procedure = procedure;
				AssertNoMessageError("No error", procedureInfo, message_BR1126);
				invoiceLine.JI_Procedure = "1521F15";
				AssertHasMessageError("Has error", procedureInfo, message_BR1126);
			});
		}

		public void TestCheckJI_Procedure_BR8062()
		{
			const string fromWarehouseMessage = "[BR8062] Please enter a EORI Number for From Warehouse.";
			const string toWarehouseMessage = "[BR8062] Please enter a EORI Number for To Warehouse.";
			(_, var invoiceLine, _, var declaration) = SetupData();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Ireland, "", "76", "71", "F15", "", "IMP", "H2");
			procedure.ZZ6_IntoWarehouse = "N";
			procedure.ZZ6_OutOfWarehouse = "N";
			invoiceLine.JI_Procedure = "7671F15";

			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, fromWarehouseMessage);
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, toWarehouseMessage);

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var toWarehouse = Factory.NewWithValidTestData<OrgHeader>();
			var fromWarehouse = Factory.NewWithValidTestData<OrgHeader>();
			instruction.CEI_OA_Warehouse = fromWarehouse.MainAddress.PK;
			instruction.CEI_OA_Warehouse2 = toWarehouse.MainAddress.PK;

			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, fromWarehouseMessage);
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, toWarehouseMessage);

			procedure.ZZ6_IntoWarehouse = "Y";
			procedure.ZZ6_OutOfWarehouse = "Y";

			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError(invoiceLine.JI_ProcedureInfo, fromWarehouseMessage);
			AssertHasMessageError(invoiceLine.JI_ProcedureInfo, toWarehouseMessage);

			fromWarehouse.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TESTID001", Core.Constants.CountryCodes.Ireland);
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, fromWarehouseMessage);

			toWarehouse.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TESTID002", Core.Constants.CountryCodes.Ireland);
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, toWarehouseMessage);
		}

		public void TestCheckJI_Procedure_BR8078()
		{
			const string message = "[BR8078] Please enter Charge '2X' under Invoice Header > Invoice Charges when Requested Procedure is '6121' or '7121'.";
			(var validation, var invoiceLine, _, _) = SetupData();
			var targetInfo = invoiceLine.JI_ProcedureInfo;
			invoiceLine.JI_Procedure = "512101";
			AssertNoMessageError("When Requested Procedure + Previous Procedure is 5121", targetInfo, message);
			invoiceLine.JI_Procedure = "612101";
			AssertHasMessageError("When Requested Procedure + Previous Procedure is 6121", targetInfo, message);
			invoiceLine.JI_Procedure = "712101";
			AssertHasMessageError("When Requested Procedure + Previous Procedure is 7121", targetInfo, message);
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = AISChargeCodeList.Codes._2X;
			validation.ValidateJI_Procedure();
			AssertNoMessageError("When Requested Procedure + Previous Procedure is 7121, charge.J7_ChargeType is 2X", targetInfo, message);
			charge.J7_ChargeType = AISChargeCodeList.Codes._1X;
			validation.ValidateJI_Procedure();
			AssertHasMessageError("When Requested Procedure + Previous Procedure is 7121, charge.J7_ChargeType is 1X", targetInfo, message);

			var apportionedCharge = invoiceLine.ApportionedCharges.AddNew();
			apportionedCharge.J7_ChargeType = AISChargeCodeList.Codes._2X;
			validation.ValidateJI_Procedure();
			AssertNoMessageError("When Requested Procedure + Previous Procedure is 7121, apportionedCharge.J7_ChargeType is 2X", targetInfo, message);
			apportionedCharge.J7_ChargeType = AISChargeCodeList.Codes._1X;
			validation.ValidateJI_Procedure();
			AssertHasMessageError("When Requested Procedure + Previous Procedure is 7121, apportionedCharge.J7_ChargeType is 1X", targetInfo, message);
		}

		public void TestCheckJI_AdditionalProcedure_NotEmpty()
		{
			const string messageError_AdditionalProcedureNotEmpty = "Additional Procedure code is required when declaration is H1, H2, H3, H4, H5, H6.";
			(var validation, var invoiceLine, _, _) = SetupData();
			var cusEntryInstruction = invoiceLine.EntryInstruction;
			var collection = invoiceLine.AdditionalProcedureCodes;
			CombineAssertions(() =>
			{
				AssertAdditionalProcedureIsRequired(ImportDeclarationTypeList.Codes.H1);
				AssertAdditionalProcedureIsRequired(ImportDeclarationTypeList.Codes.H2);
				AssertAdditionalProcedureIsRequired(ImportDeclarationTypeList.Codes.H3);
				AssertAdditionalProcedureIsRequired(ImportDeclarationTypeList.Codes.H4);
				AssertAdditionalProcedureIsRequired(ImportDeclarationTypeList.Codes.H5);
				AssertAdditionalProcedureIsRequired(ImportDeclarationTypeList.Codes.H6);

				collection.RemoveAll();
				cusEntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H7;
				validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError($"if CEI_Style is 'H7' and AdditionalProcedure empty", invoiceLine.AdditionalProcedureCodesAsStringInfo, messageError_AdditionalProcedureNotEmpty);
			});

			void AssertAdditionalProcedureIsRequired(ZString cei_Style)
			{
				cusEntryInstruction.CEI_Style = cei_Style;
				collection.RemoveAll();
				validation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError($"if CEI_Style is '{cei_Style}' and AdditionalProcedure empty", invoiceLine.AdditionalProcedureCodesAsStringInfo, messageError_AdditionalProcedureNotEmpty);

				var newCode = collection.AddNew();
				newCode.CY_Code = "1000C01";
				validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError($"if CEI_Style is '{cei_Style}' and AdditionalProcedure not empty", invoiceLine.AdditionalProcedureCodesAsStringInfo, messageError_AdditionalProcedureNotEmpty);
			}
		}

		[StressTest]
		public void TestCheckJI_ZZF_NKTaxType_NotEmpty()
		{
			(var validation, var invoiceLine, _, _) = SetupData();
			var cusEntryInstruction = invoiceLine.EntryInstruction;
			const string messageError_NoVAT = "You have not entered a VAT.";

			CombineAssertions(() =>
			{
				var additionalProcedure = invoiceLine.AdditionalProcedureCodes.AddNew();
				additionalProcedure.CY_Code = "1000F03";
				AssertVATisRequired(ImportDeclarationTypeList.Codes.H1);
				AssertVATisRequired(ImportDeclarationTypeList.Codes.H5);

				additionalProcedure.CY_Code = "1000F05";
				AssertNoMessageError("VAT can be empty for AdditionalProcedureCode F05", invoiceLine.JI_ZZF_NKTaxTypeInfo, messageError_NoVAT);

				additionalProcedure.CY_Code = "1000F03";
				cusEntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H4;
				validation.ValidateJI_ZZF_NKTaxType();
				AssertNoMessageError("VAT can be empty for CEI_Style H4", invoiceLine.JI_ZZF_NKTaxTypeInfo, messageError_NoVAT);
			});

			void AssertVATisRequired(ZString cei_style)
			{
				cusEntryInstruction.CEI_Style = cei_style;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_ZZF_NKTaxTypeInfo, messageError_NoVAT);
			}
		}

		public void TestCheckJI_PrimaryPreference_NotEmpty()
		{
			(var validation, var invoiceLine, _, _) = SetupData();
			var cusEntryInstruction = invoiceLine.EntryInstruction;
			const string messageError_NoPref = "You have not entered a Preference.";

			CombineAssertions(() =>
			{
				AssertPreferenceisRequired(ImportDeclarationTypeList.Codes.H1);
				AssertPreferenceisRequired(ImportDeclarationTypeList.Codes.H5);

				cusEntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H4;
				validation.ValidateJI_PrimaryPreference();
				AssertNoMessageError("Primary preference can be empty for H4", invoiceLine.JI_PrimaryPreferenceInfo, messageError_NoPref);
			});

			void AssertPreferenceisRequired(ZString cei_style)
			{
				cusEntryInstruction.CEI_Style = cei_style;
				validation.ValidateJI_PrimaryPreference();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_PrimaryPreferenceInfo, messageError_NoPref);
			}
		}

		const string MessageError_BR1030 = "[BR1030] If Requested Procedure is '{0}', then please enter a {1} Authorization under the Entry Instructions > Authorizations tab.";
		const string MessageError_BR1106_UCC5 = "[BR1106] If Requested Procedure is not '76' nor '77', then please enter at least one Transport Document under the Supporting Documents tab.";
		const string MessageError_BR1106_UCC6 = "[BR1106] If Requested Procedure is not '76' nor '77', then please enter at least one Transport Document under the Additional Documents tab.";
		const string MessageError_BR1107_UCC5 = "[BR1107] If Requested Procedure is not '76' nor '77', and Transport Mode is 'SEA', then please uniquely enter one or two 'N703', 'N704', 'N705', 'N714', or 'N730' Supporting Documents under the Supporting Documents tab.";
		const string MessageError_BR1107_UCC6 = "[BR1107] If Requested Procedure is not '76' nor '77', and Transport Mode is 'SEA', then please uniquely enter one or two 'N703', 'N704', 'N705', 'N714', or 'N730' Transport Documents under the Additional Documents tab.";
		const string MessageError_BR1108_UCC5 = "[BR1108] If Requested Procedure is not '76' nor '77', and Transport Mode is 'SEA', then the only allowable combinations of two Supporting Documents under the Supporting Documents tab are 'N704', with 'N703', 'N705', 'N714', or 'N730'.";
		const string MessageError_BR1108_UCC6 = "[BR1108] If Requested Procedure is not '76' nor '77', and Transport Mode is 'SEA', then the only allowable combinations of two Transport Documents under the Additional Documents tab are 'N704', with 'N703', 'N705', 'N714', or 'N730'.";
		const string MessageError_BR1110_UCC6 = "[BR1110] If Requested Procedure is not '76' nor '77', and Transport Mode is 'AIR', then please uniquely enter one or two 'N703', 'N740', or 'N741' Transport Documents under the Additional Documents tab.";
		const string MessageError_BR1110_UCC5 = "[BR1110] If Requested Procedure is not '76' nor '77', and Transport Mode is 'AIR', then please uniquely enter one or two 'N703', 'N740', or 'N741' Transport Documents under the Supporting Documents tab.";

		public void TestCheckCEI_AuthUsage_BR1030()
		{
			CombineAssertions(() =>
			{
				AssertCEIProcedureByAuthUsageMessage(ProcedureCodes.ProcedureCode._44, AuthorizationUsage.Codes.EUS, $"'{AuthorizationUsage.Codes.EUS}'");
				AssertCEIProcedureByAuthUsageMessage(ProcedureCodes.ProcedureCode._51, AuthorizationUsage.Codes.IPO, $"'{AuthorizationUsage.Codes.IPO}'");
				AssertCEIProcedureByAuthUsageMessage(ProcedureCodes.ProcedureCode._53, AuthorizationUsage.Codes.TEA, $"'{AuthorizationUsage.Codes.TEA}'");
				AssertCEIProcedureByAuthUsageMessage(ProcedureCodes.ProcedureCode._71, AuthorizationUsage.Codes.CW1, $"'{AuthorizationUsage.Codes.CW1}', '{AuthorizationUsage.Codes.CW2}' or '{AuthorizationUsage.Codes.CWP}'");
				AssertCEIProcedureByAuthUsageMessage(ProcedureCodes.ProcedureCode._71, AuthorizationUsage.Codes.CW2, $"'{AuthorizationUsage.Codes.CW1}', '{AuthorizationUsage.Codes.CW2}' or '{AuthorizationUsage.Codes.CWP}'");
				AssertCEIProcedureByAuthUsageMessage(ProcedureCodes.ProcedureCode._71, AuthorizationUsage.Codes.CWP, $"'{AuthorizationUsage.Codes.CW1}', '{AuthorizationUsage.Codes.CW2}' or '{AuthorizationUsage.Codes.CWP}'");
			});
		}

		void AssertCEIProcedureByAuthUsageMessage(string procedureCode, string authCode, string authMessageString)
		{
			(var validation, var invoiceLine, _, var declaration) = SetupData();
			invoiceLine.JI_Procedure = procedureCode;
			var entryInstruction = declaration.CustomsEntryInstructions.First();
			entryInstruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			validation.ValidateJI_Procedure();
			var targetInfo = invoiceLine.JI_ProcedureInfo;
			AssertHasMessageError
			(
				$"When procedure is '{procedureCode}' and no auth usage, should have error message.",
				targetInfo,
				string.Format(MessageError_BR1030, procedureCode, authMessageString)
			);

			var authUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authUsage.AGC_Code = "RND";
			validation.ValidateJI_Procedure();
			AssertHasMessageError
			(
				$"When procedure is '{procedureCode}' and no '{authMessageString}' auth usage, should have error message.",
				targetInfo,
				string.Format(MessageError_BR1030, procedureCode, authMessageString)
			);

			authUsage.AGC_Code = authCode;
			validation.ValidateJI_Procedure();
			AssertNoMessageError
			(
				$"When procedure is '{procedureCode}' and '{authCode}' auth usage, should have no error message.",
				targetInfo,
				string.Format(MessageError_BR1030, procedureCode, authMessageString)
			);
		}

		public void TestCheckCEI_Procedure_BR1106_UCC5()
		{
			(var validation, var invoiceLine, _, var declaration) = LocalSetupData();
			var entryInstruction = declaration.CustomsEntryInstructions.FirstOrDefault();
			ValidateProcedureUCC5("Entry Instruction", invoiceLine, () => entryInstruction.SupportingDocuments.AddNew());

			(validation, invoiceLine, var invoiceHeader, declaration) = LocalSetupData();
			ValidateProcedureUCC5("Invoice Header related to invoice line", invoiceLine, () => invoiceHeader.SupportingDocuments.AddNew());

			(validation, invoiceLine, _, declaration) = LocalSetupData();
			ValidateProcedureUCC5("Invoice Lines related to this invoice line", invoiceLine, () => invoiceLine.SupportingDocuments.AddNew());

			(validation, invoiceLine, _, declaration) = LocalSetupData();
			ValidateProcedureUCC5("Invoice Lines unrelated to this invoice line", invoiceLine, () =>
			{
				var invoiceUnrelated = declaration.Invoices.AddNew();
				var invoiceLineUnrelated = invoiceUnrelated.JobComInvoiceLines.AddNew();
				return invoiceLineUnrelated.SupportingDocuments.AddNew();
			});

			(validation, invoiceLine, _, declaration) = LocalSetupData();
			invoiceLine.JI_CEI = ZGuid.Empty;
			ValidateProcedureUCC5("Invoice Lines unrelated to entry instruction", invoiceLine, () => invoiceLine.SupportingDocuments.AddNew());

			(ImportJobComInvoiceLineValidation validation, JobComInvoiceLine invoiceLine, JobComInvoiceHeader invoice, JobDeclaration declaration) LocalSetupData()
			{
				(var validation, var invoiceLine, var invoice, var declaration) = SetupData();
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				invoiceLine.JI_Procedure = "1234";
				return ((ImportJobComInvoiceLineValidation)invoiceLine.Validation, invoiceLine, invoice, declaration);
			}
		}

		void ValidateProcedureUCC5(string testMessage, JobComInvoiceLine invoiceLine, Func<SupportingDocument> addDocument)
		{
			var targetInfo = invoiceLine.JI_ProcedureInfo;
			CombineAssertions(testMessage, () =>
			{
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertHasMessageError("No document", targetInfo, MessageError_BR1106_UCC5);

				var addedInfo = addDocument();
				addedInfo.CSI_Code = "$%&";

				invoiceLine.Validation.ValidateJI_Procedure();
				AssertHasMessageError("Wrong Transport document code", targetInfo, MessageError_BR1106_UCC5);

				addedInfo.CSI_Code = "N705";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("Has valid Transport document code", targetInfo, MessageError_BR1106_UCC5);

				invoiceLine.JI_Procedure = "7634";
				AssertNoMessageError("Procedure code is 76", targetInfo, MessageError_BR1106_UCC5);

				invoiceLine.Declaration.JE_MessageType = MessageTypeList.Codes.Export;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("Not Import", targetInfo, MessageError_BR1106_UCC5);
			});
		}

		public void TestCheckCEI_Procedure_BR1106_UCC6()
		{
			(var validation, var invoiceLine, _, var declaration) = LocalSetupData();
			var entryInstruction = declaration.CustomsEntryInstructions.FirstOrDefault();
			ValidateProcedureUCC6("Entry Instruction", invoiceLine, () => entryInstruction.AdditionalInfos.AddNew());

			(validation, invoiceLine, var invoiceHeader, declaration) = LocalSetupData();
			ValidateProcedureUCC6("Invoice Header related to invoice line", invoiceLine, () => invoiceHeader.AdditionalInfos.AddNew());

			(validation, invoiceLine, _, declaration) = LocalSetupData();
			ValidateProcedureUCC6("Invoice Lines related to this invoice line", invoiceLine, () => invoiceLine.AdditionalInfos.AddNew());

			(validation, invoiceLine, _, declaration) = LocalSetupData();
			ValidateProcedureUCC6("Invoice Lines unrelated to this invoice line", invoiceLine, () =>
			{
				var invoiceUnrelated = declaration.Invoices.AddNew();
				var invoiceLineUnrelated = invoiceUnrelated.JobComInvoiceLines.AddNew();
				return invoiceLineUnrelated.AdditionalInfos.AddNew();
			});

			(validation, invoiceLine, _, declaration) = LocalSetupData();
			invoiceLine.JI_CEI = ZGuid.Empty;
			ValidateProcedureUCC6("Invoice Lines unrelated to entry instruction", invoiceLine, () => invoiceLine.AdditionalInfos.AddNew());

			(ImportJobComInvoiceLineValidation validation, JobComInvoiceLine invoiceLine, JobComInvoiceHeader invoice, JobDeclaration declaration) LocalSetupData()
			{
				(var validation, var invoiceLine, var invoice, var declaration) = SetupData();
				invoiceLine.JI_Procedure = "1234";
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				return ((ImportJobComInvoiceLineValidation)invoiceLine.Validation, invoiceLine, invoice, declaration);
			}
		}

		void ValidateProcedureUCC6(string testMessage, JobComInvoiceLine invoiceLine, Func<AdditionalInfo> addDocument)
		{
			var targetInfo = invoiceLine.JI_ProcedureInfo;

			CombineAssertions(testMessage, () =>
			{
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertHasMessageError("No document", targetInfo, MessageError_BR1106_UCC6);

				var addedInfo = addDocument();

				addedInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertHasMessageError("No TRA document", targetInfo, MessageError_BR1106_UCC6);

				addedInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("Has TRA document", targetInfo, MessageError_BR1106_UCC6);

				invoiceLine.JI_Procedure = "7634";
				AssertNoMessageError("Procedure code is 76", targetInfo, MessageError_BR1106_UCC6);

				invoiceLine.Declaration.JE_MessageType = MessageTypeList.Codes.Export;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("Not Import", targetInfo, MessageError_BR1106_UCC6);
			});
		}

		public void TestIsValidForBR1106()
		{
			(var validation, var _, _, _) = SetupData();

			Assert(validation.IsValidForBR1106(Constants.TransportDocumentCodes._N235));
			Assert(validation.IsValidForBR1106(Constants.TransportDocumentCodes._N271));
			Assert(validation.IsValidForBR1106(Constants.TransportDocumentCodes._N703));
			Assert(validation.IsValidForBR1106(Constants.TransportDocumentCodes._N704));
			Assert(validation.IsValidForBR1106(Constants.TransportDocumentCodes._N705));
			Assert(validation.IsValidForBR1106(Constants.TransportDocumentCodes._N710));
			Assert(validation.IsValidForBR1106(Constants.TransportDocumentCodes._N714));
			Assert(validation.IsValidForBR1106(Constants.TransportDocumentCodes._N720));
			Assert(validation.IsValidForBR1106(Constants.TransportDocumentCodes._N722));
			Assert(validation.IsValidForBR1106(Constants.TransportDocumentCodes._N730));
			Assert(validation.IsValidForBR1106(Constants.TransportDocumentCodes._N740));
			Assert(validation.IsValidForBR1106(Constants.TransportDocumentCodes._N741));
			Assert(validation.IsValidForBR1106(Constants.TransportDocumentCodes._N750));
			Assert(validation.IsValidForBR1106(Constants.TransportDocumentCodes._N760));
			Assert(validation.IsValidForBR1106(Constants.TransportDocumentCodes._N785));
			Assert(validation.IsValidForBR1106(Constants.TransportDocumentCodes._N787));
			Assert(validation.IsValidForBR1106(Constants.TransportDocumentCodes._N952));
			Assert(validation.IsValidForBR1106(Constants.TransportDocumentCodes._N955));

			Assert(!validation.IsValidForBR1106("INVALID_CODE"));
		}

		public void TestCheckCEI_TransportDocs_BR1107_UCC6()
		{
			(var validation, var invoiceLine, var invoiceHeader, var declaration) = LocalSetupData();
			string messageError = MessageError_BR1107_UCC6;

			var entryInstruction = declaration.CustomsEntryInstructions.FirstOrDefault();

			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError("When no documents", invoiceLine.JI_ProcedureInfo, messageError);

			var doc1 = entryInstruction.AdditionalInfos.AddNew();
			doc1.CSI_Code = "ABC";
			doc1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError("When only documents of wrong type", invoiceLine.JI_ProcedureInfo, messageError);

			doc1.CSI_Code = Constants.TransportDocumentCodes._N703;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("When has only acceptable code N703", invoiceLine.JI_ProcedureInfo, messageError);

			var doc2 = invoiceHeader.AdditionalInfos.AddNew();
			doc2.CSI_Code = Constants.TransportDocumentCodes._N704;
			doc2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("When has two acceptable codes N703 and N704", invoiceLine.JI_ProcedureInfo, messageError);

			var doc3 = invoiceLine.AdditionalInfos.AddNew();
			doc3.CSI_Code = Constants.TransportDocumentCodes._N705;
			doc3.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError("When has more than two acceptable codes", invoiceLine.JI_ProcedureInfo, messageError);

			foreach (var procedure in new[] { string.Empty, ProcedureCodes.ProcedureCode._76, ProcedureCodes.ProcedureCode._77 })
			{
				invoiceLine.JI_Procedure = procedure;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError($"When procedure is not validated against BR1107: {procedure}", invoiceLine.JI_ProcedureInfo, messageError);
			}

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._44;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("When transport mode is other than SEA", invoiceLine.JI_ProcedureInfo, messageError);

			(validation, invoiceLine, _, declaration) = LocalSetupData();
			invoiceLine.JI_CEI = ZGuid.Empty;
			var doc = invoiceLine.AdditionalInfos.AddNew();
			doc.CSI_Code = Constants.TransportDocumentCodes._N703;
			doc.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("Invoice Lines unrelated to entry instruction", invoiceLine.JI_ProcedureInfo, messageError);

			(ImportJobComInvoiceLineValidation validation, JobComInvoiceLine invoiceLine, JobComInvoiceHeader invoice, JobDeclaration declaration) LocalSetupData()
			{
				(var validation, var invoiceLine, var invoice, var declaration) = SetupData();
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._44;
				return ((ImportJobComInvoiceLineValidation)invoiceLine.Validation, invoiceLine, invoice, declaration);
			}
		}

		public void TestCheckCEI_TransportDocs_BR1107_UCC5()
		{
			(var validation, var invoiceLine, var invoiceHeader, var declaration) = LocalSetupData();
			string messageError = MessageError_BR1107_UCC5;

			var entryInstruction = declaration.CustomsEntryInstructions.FirstOrDefault();

			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError("When no documents", invoiceLine.JI_ProcedureInfo, messageError);

			var doc1 = entryInstruction.SupportingDocuments.AddNew();
			doc1.CSI_Code = "ABC";
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError("When only documents of wrong type", invoiceLine.JI_ProcedureInfo, messageError);

			doc1.CSI_Code = Constants.TransportDocumentCodes._N703;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("When has only acceptable code N703", invoiceLine.JI_ProcedureInfo, messageError);

			var doc2 = invoiceHeader.SupportingDocuments.AddNew();
			doc2.CSI_Code = Constants.TransportDocumentCodes._N704;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("When has two acceptable codes N703 and N704", invoiceLine.JI_ProcedureInfo, messageError);

			var doc3 = invoiceLine.SupportingDocuments.AddNew();
			doc3.CSI_Code = Constants.TransportDocumentCodes._N705;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError("When has more than two acceptable codes", invoiceLine.JI_ProcedureInfo, messageError);

			foreach (var procedure in new[] { string.Empty, ProcedureCodes.ProcedureCode._76, ProcedureCodes.ProcedureCode._77 })
			{
				invoiceLine.JI_Procedure = procedure;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError($"When procedure is not validated against BR1107: {procedure}", invoiceLine.JI_ProcedureInfo, messageError);
			}

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._44;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("When transport mode is other than SEA", invoiceLine.JI_ProcedureInfo, messageError);

			(validation, invoiceLine, _, declaration) = LocalSetupData();
			invoiceLine.JI_CEI = ZGuid.Empty;
			var doc = invoiceLine.SupportingDocuments.AddNew();
			doc.CSI_Code = Constants.TransportDocumentCodes._N703;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("Invoice Lines unrelated to entry instruction", invoiceLine.JI_ProcedureInfo, messageError);

			(ImportJobComInvoiceLineValidation validation, JobComInvoiceLine invoiceLine, JobComInvoiceHeader invoice, JobDeclaration declaration) LocalSetupData()
			{
				(var validation, var invoiceLine, var invoice, var declaration) = SetupData();
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._44;
				return ((ImportJobComInvoiceLineValidation)invoiceLine.Validation, invoiceLine, invoice, declaration);
			}
		}

		public void TestCheckCEI_TransportDocs_BR1108_UCC5()
		{
			(var validation, var invoiceLine, var invoiceHeader, var declaration) = LocalSetupData();
			var entryInstruction = declaration.CustomsEntryInstructions.FirstOrDefault();

			string messageError = MessageError_BR1108_UCC5;

			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("When no transport documents", invoiceLine.JI_ProcedureInfo, messageError);

			var doc1 = entryInstruction.SupportingDocuments.AddNew();
			doc1.CSI_Code = "ABC";
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("When only transport documents of wrong type", invoiceLine.JI_ProcedureInfo, messageError);

			doc1.CSI_Code = Constants.TransportDocumentCodes._N703;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("When has only acceptable code N703", invoiceLine.JI_ProcedureInfo, messageError);

			var doc2 = invoiceHeader.SupportingDocuments.AddNew();
			doc2.CSI_Code = Constants.TransportDocumentCodes._N703;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError("When has two acceptable codes N703 twice but not has N704", invoiceLine.JI_ProcedureInfo, messageError);

			doc2.CSI_Code = Constants.TransportDocumentCodes._N704;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("When has an acceptable code N703 and has N704", invoiceLine.JI_ProcedureInfo, messageError);

			var doc3 = invoiceLine.SupportingDocuments.AddNew();
			doc3.CSI_Code = Constants.TransportDocumentCodes._N705;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("When has more than two acceptable codes", invoiceLine.JI_ProcedureInfo, messageError);

			foreach (var procedure in new[] { string.Empty, ProcedureCodes.ProcedureCode._76, ProcedureCodes.ProcedureCode._77 })
			{
				invoiceLine.JI_Procedure = procedure;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError($"When procedure is not validated against BR1108: {procedure}", invoiceLine.JI_ProcedureInfo, messageError);
			}

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._44;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("When transport mode is other than SEA", invoiceLine.JI_ProcedureInfo, messageError);

			(validation, invoiceLine, _, declaration) = LocalSetupData();
			invoiceLine.JI_CEI = ZGuid.Empty;
			var doc = invoiceLine.SupportingDocuments.AddNew();
			doc.CSI_Code = Constants.TransportDocumentCodes._N703;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("Invoice Lines unrelated to entry instruction", invoiceLine.JI_ProcedureInfo, messageError);

			(ImportJobComInvoiceLineValidation validation, JobComInvoiceLine invoiceLine, JobComInvoiceHeader invoice, JobDeclaration declaration) LocalSetupData()
			{
				(var validation, var invoiceLine, var invoice, var declaration) = SetupData();
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._44;
				return ((ImportJobComInvoiceLineValidation)invoiceLine.Validation, invoiceLine, invoice, declaration);
			}
		}

		public void TestCheckCEI_TransportDocs_BR1108_UCC6()
		{
			(var validation, var invoiceLine, var invoiceHeader, var declaration) = LocalSetupData();
			var entryInstruction = declaration.CustomsEntryInstructions.FirstOrDefault();

			string messageError = MessageError_BR1108_UCC6;

			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("When no transport documents", invoiceLine.JI_ProcedureInfo, messageError);

			var doc1 = entryInstruction.AdditionalInfos.AddNew();
			doc1.CSI_Code = "ABC";
			doc1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("When only transport documents of wrong type", invoiceLine.JI_ProcedureInfo, messageError);

			doc1.CSI_Code = Constants.TransportDocumentCodes._N703;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("When has only acceptable code N703", invoiceLine.JI_ProcedureInfo, messageError);

			var doc2 = invoiceHeader.AdditionalInfos.AddNew();
			doc2.CSI_Code = Constants.TransportDocumentCodes._N703;
			doc2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError("When has two acceptable codes N703 twice but not has N704", invoiceLine.JI_ProcedureInfo, messageError);

			doc2.CSI_Code = Constants.TransportDocumentCodes._N704;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("When has an acceptable code N703 and has N704", invoiceLine.JI_ProcedureInfo, messageError);

			var doc3 = invoiceLine.AdditionalInfos.AddNew();
			doc3.CSI_Code = Constants.TransportDocumentCodes._N705;
			doc3.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("When has more than two acceptable codes", invoiceLine.JI_ProcedureInfo, messageError);

			foreach (var procedure in new[] { string.Empty, ProcedureCodes.ProcedureCode._76, ProcedureCodes.ProcedureCode._77 })
			{
				invoiceLine.JI_Procedure = procedure;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError($"When procedure is not validated against BR1108: {procedure}", invoiceLine.JI_ProcedureInfo, messageError);
			}

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._44;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("When transport mode is other than SEA", invoiceLine.JI_ProcedureInfo, messageError);

			(validation, invoiceLine, _, declaration) = LocalSetupData();
			invoiceLine.JI_CEI = ZGuid.Empty;
			var doc = invoiceLine.AdditionalInfos.AddNew();
			doc.CSI_Code = Constants.TransportDocumentCodes._N703;
			doc.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("Invoice Lines unrelated to entry instruction", invoiceLine.JI_ProcedureInfo, messageError);

			(ImportJobComInvoiceLineValidation validation, JobComInvoiceLine invoiceLine, JobComInvoiceHeader invoice, JobDeclaration declaration) LocalSetupData()
			{
				(var validation, var invoiceLine, var invoice, var declaration) = SetupData();
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._44;
				return ((ImportJobComInvoiceLineValidation)invoiceLine.Validation, invoiceLine, invoice, declaration);
			}
		}

		public void TestCheckCEI_Procedure_BR1110_UCC5()
		{
			(var validation, var invoiceLine, var invoiceHeader, var declaration) = LocalSetupData();
			var messageError = MessageError_BR1110_UCC5;
			var targetInfo = invoiceLine.JI_ProcedureInfo;

			var entryInstruction = declaration.CustomsEntryInstructions.FirstOrDefault();

			CombineAssertions(() =>
			{
				validation.ValidateJI_Procedure();
				AssertNoMessageErrorContaining("Empty JI_Procedure", targetInfo, messageError);

				invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._76;
				AssertNoMessageErrorContaining("JI_Procedure is 76", targetInfo, messageError);

				invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._77;
				AssertNoMessageErrorContaining("JI_Procedure is 77", targetInfo, messageError);

				invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._21;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertHasMessageError("When no documents", targetInfo, messageError);

				var doc1 = entryInstruction.SupportingDocuments.AddNew();
				doc1.CSI_Code = "ABC";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertHasMessageError("The count of supporting documents where type is not N703, N740 nor N741 is greater than 0", targetInfo, messageError);

				doc1.CSI_Code = "N703";
				validation.ValidateJI_Procedure();
				AssertNoMessageErrorContaining("When has only one acceptable code N703", targetInfo, messageError);

				var doc2 = invoiceHeader.SupportingDocuments.AddNew();
				doc2.CSI_Code = "N703";
				validation.ValidateJI_Procedure();
				AssertHasMessageErrorContaining("There are duplicated supporting document types", targetInfo, messageError);

				doc2.CSI_Code = "N740";
				validation.ValidateJI_Procedure();
				AssertNoMessageErrorContaining("Two correct document", targetInfo, messageError);

				var doc3 = invoiceLine.SupportingDocuments.AddNew();
				doc3.CSI_Code = "N741";
				validation.ValidateJI_Procedure();
				AssertHasMessageErrorContaining("The count of supporting documents is not 1 or 2", targetInfo, messageError);

				declaration.JE_TransportMode = TransportTypeList.Codes.Road;
				invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._44;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageErrorContaining("When transport mode is other than AIR", targetInfo, messageError);

				(validation, invoiceLine, _, declaration) = LocalSetupData();
				invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._44;
				invoiceLine.JI_CEI = ZGuid.Empty;
				var doc = invoiceLine.SupportingDocuments.AddNew();
				doc.CSI_Code = Constants.TransportDocumentCodes._N703;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageErrorContaining("Invoice Lines unrelated to entry instruction", targetInfo, messageError);
			});

			(ImportJobComInvoiceLineValidation validation, JobComInvoiceLine invoiceLine, JobComInvoiceHeader invoice, JobDeclaration declaration) LocalSetupData()
			{
				(var validation, var invoiceLine, var invoice, var declaration) = SetupData();
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				return ((ImportJobComInvoiceLineValidation)invoiceLine.Validation, invoiceLine, invoice, declaration);
			}
		}

		public void TestCheckCEI_Procedure_BR1110_UCC6()
		{
			var messageError = MessageError_BR1110_UCC6;
			(var validation, var invoiceLine, var invoiceHeader, var declaration) = LocalSetupData();
			var targetInfo = invoiceLine.JI_ProcedureInfo;

			var entryInstruction = declaration.CustomsEntryInstructions.FirstOrDefault();

			CombineAssertions(() =>
			{
				validation.ValidateJI_Procedure();
				AssertNoMessageErrorContaining("Empty JI_Procedure", targetInfo, messageError);

				invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._76;
				AssertNoMessageErrorContaining("JI_Procedure is 76", targetInfo, messageError);

				invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._77;
				AssertNoMessageErrorContaining("JI_Procedure is 77", targetInfo, messageError);

				invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._21;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertHasMessageError("When no documents", targetInfo, messageError);

				var doc1 = entryInstruction.AdditionalInfos.AddNew();
				doc1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				doc1.CSI_Code = "N700";
				validation.ValidateJI_Procedure();
				AssertHasMessageErrorContaining("The count of transport documents where type is not N703, N740 nor N741 is greater than 0", targetInfo, messageError);

				doc1.CSI_Code = "N703";
				validation.ValidateJI_Procedure();
				AssertNoMessageErrorContaining("When has only one acceptable code N703", targetInfo, messageError);

				var doc2 = invoiceHeader.AdditionalInfos.AddNew();
				doc2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				doc2.CSI_Code = "N703";
				validation.ValidateJI_Procedure();
				AssertHasMessageErrorContaining("There are duplicated transport document types", targetInfo, messageError);

				doc2.CSI_Code = "N740";
				validation.ValidateJI_Procedure();
				AssertNoMessageErrorContaining("Two correct document", targetInfo, messageError);

				var doc3 = invoiceLine.AdditionalInfos.AddNew();
				doc3.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				doc3.CSI_Code = "N741";
				validation.ValidateJI_Procedure();
				AssertHasMessageErrorContaining("The count of transport documents is not 1 or 2", targetInfo, messageError);

				declaration.JE_TransportMode = TransportTypeList.Codes.Road;
				invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._44;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageErrorContaining("When transport mode is other than AIR", targetInfo, messageError);

				(validation, invoiceLine, _, declaration) = LocalSetupData();
				invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._44;
				invoiceLine.JI_CEI = ZGuid.Empty;
				var doc = invoiceLine.SupportingDocuments.AddNew();
				doc.CSI_Code = Constants.TransportDocumentCodes._N703;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageErrorContaining("Invoice Lines unrelated to entry instruction", targetInfo, messageError);
			});

			(ImportJobComInvoiceLineValidation validation, JobComInvoiceLine invoiceLine, JobComInvoiceHeader invoice, JobDeclaration declaration) LocalSetupData()
			{
				(var validation, var invoiceLine, var invoice, var declaration) = SetupData();
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				return ((ImportJobComInvoiceLineValidation)invoiceLine.Validation, invoiceLine, invoice, declaration);
			}
		}

		public void TestCheckCEI_Procedure_BR1112()
		{
			(var validation, var invoiceLine, _, var declaration) = SetupData();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var targetInfo = invoiceLine.JI_ProcedureInfo;
			var addInfo1 = invoiceLine.AdditionalInfos.AddNew();
			addInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			addInfo1.CSI_Code = TransportDocumentCodes._N741;

			CombineAssertions("AdditionalInfos CSI_Code allowable combination N703, N740, N741", () =>
			{
				invoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._78;
				AssertNoMessageError("AIR | NOT 76/77 | TRA | only N741", targetInfo, MessageError_BR1112);

				var addInfoInstruction2 = invoiceLine.AdditionalInfos.AddNew();
				addInfoInstruction2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				addInfoInstruction2.CSI_Code = TransportDocumentCodes._N703;
				validation.ValidateJI_Procedure();
				AssertNoMessageError("AIR | NOT 76/77 | TRA | N741 + N703", targetInfo, MessageError_BR1112);

				addInfoInstruction2.CSI_Code = TransportDocumentCodes._N740;
				validation.ValidateJI_Procedure();
				AssertNoMessageError("AIR | NOT 76/77 | TRA | N741 + N740", targetInfo, MessageError_BR1112);

				addInfoInstruction2.CSI_Code = "N700";
				validation.ValidateJI_Procedure();
				AssertNoMessageError("AIR | NOT 76/77 | TRA | N741 + N700", targetInfo, MessageError_BR1112);

				addInfo1.CSI_Code = "N700";
				addInfoInstruction2.CSI_Code = TransportDocumentCodes._N703;
				validation.ValidateJI_Procedure();
				AssertNoMessageError("AIR | NOT 76/77 | TRA | N700 + N703", targetInfo, MessageError_BR1112);

				addInfo1.CSI_Code = "N700";
				addInfoInstruction2.CSI_Code = TransportDocumentCodes._N740;
				validation.ValidateJI_Procedure();
				AssertNoMessageError("AIR | NOT 76/77 | TRA | N700 + N740", targetInfo, MessageError_BR1112);

				addInfo1.CSI_Code = TransportDocumentCodes._N703;
				addInfoInstruction2.CSI_Code = TransportDocumentCodes._N740;
				validation.ValidateJI_Procedure();
				AssertHasMessageError("AIR | NOT 76/77 | TRA | N703 + N740", targetInfo, MessageError_BR1112);

				addInfoInstruction2.CSI_Code = TransportDocumentCodes._N703;
				validation.ValidateJI_Procedure();
				AssertHasMessageError("AIR | NOT 76/77 | TRA | N703 + _N703", targetInfo, MessageError_BR1112);

				addInfo1.CSI_Code = TransportDocumentCodes._N703;
				addInfoInstruction2.CSI_Code = TransportDocumentCodes._N740;
				validation.ValidateJI_Procedure();
				AssertHasMessageError("AIR | NOT 76/77 | TRA | N740 + _N740", targetInfo, MessageError_BR1112);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				addInfoInstruction2.CSI_Code = TransportDocumentCodes._N740;
				validation.ValidateJI_Procedure();
				AssertNoMessageError("SEA | NOT 76/77 | TRA | N703 + N740", targetInfo, MessageError_BR1112);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				addInfoInstruction2.CSI_Code = TransportDocumentCodes._N740;
				invoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._76;
				AssertNoMessageError("AIR | 76 | TRA | N703 + N740", targetInfo, MessageError_BR1112);

				invoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._77;
				AssertNoMessageError("AIR | 77 | TRA | N703 + N740", targetInfo, MessageError_BR1112);

				addInfoInstruction2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				invoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._78;
				AssertNoMessageError("AIR | NOT 76/77 | INF | N703 + N740", targetInfo, MessageError_BR1112);

				addInfoInstruction2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				var addInfoInstruction3 = invoiceLine.AdditionalInfos.AddNew();
				addInfoInstruction3.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				addInfoInstruction3.CSI_Code = TransportDocumentCodes._N703;
				validation.ValidateJI_Procedure();
				AssertNoMessageError("AIR | NOT 76/77 | TRA | N703 + N703 + N740", targetInfo, MessageError_BR1112);
			});
		}

		public void TestCheckCEI_Procedure_BR3401()
		{
			(var validation, var invoiceLine, _, var declaration) = SetupData();
			var message = "[BR3401] If the Requested Procedure is coded as '42' or '63', please ensure that the information stipulated by Article 143 (2) of Directive 2006/112/EC is entered in the 'Fiscal References' tab, by inputting an FR2 along with either an FR1 or an FR3.";
			var fiscalReference1 = invoiceLine.FiscalReferences.AddNew();
			var fiscalReference2 = invoiceLine.FiscalReferences.AddNew();
			var fiscalReference3 = invoiceLine.FiscalReferences.AddNew();
			fiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			fiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			fiscalReference3.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;

			CombineAssertions(() =>
			{
				var targetInfo = invoiceLine.JI_ProcedureInfo;
				invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._22;
				AssertNoMessageError("No BR3401 check when Requested Procedure not one of 42/63.", targetInfo, message);

				invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._42;
				AssertHasMessageError("BR3401 check when for Requested Procedure 42.", targetInfo, message);
				invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._63;
				AssertHasMessageError("BR3401 check when for Requested Procedure 63.", targetInfo, message);

				fiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR2_Customer;
				fiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
				fiscalReference3.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
				validation.ValidateJI_Procedure();
				AssertNoMessageError("BR3401 check when for Requested Procedure 63, fiscal reference combination FR2+FR1, check passes.", targetInfo, message);

				fiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
				validation.ValidateJI_Procedure();
				AssertNoMessageError("BR3401 check when for Requested Procedure 63, fiscal reference combination FR2+FR3, check passes.", targetInfo, message);
			});
		}

		public void TestCheckBR2312_Condition1() => CombineAssertions(() =>
		{
			var messageError = "[BR2312] If the Dataset is 'I1' or the Requested Procedure is '71', the Additional Documents tab must include both '1D94' and '1D95' Additional References, or none of them.";

			(var validation, var invoiceLine, _, var declaration) = SetupData();
			var instruction = invoiceLine.EntryInstruction;
			var additionalDocument = invoiceLine.AdditionalInfos.AddNew();

			string GetAssertMessage()
			{
				return $"when Requested Procedure: {invoiceLine.JI_Procedure}, CEI_Style: {instruction.CEI_Style}, Document with CSI_SubType: {additionalDocument.CSI_SubType}, CSI_Code: {additionalDocument.CSI_Code}";
			}

			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalDocument.CSI_Code = Constants.AdditionalInformationCodes._1D94;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(GetAssertMessage(), invoiceLine.JI_ProcedureInfo, messageError);

			invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError(GetAssertMessage(), invoiceLine.JI_ProcedureInfo, messageError);

			invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._01;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.I1;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError(GetAssertMessage(), invoiceLine.JI_ProcedureInfo, messageError);

			additionalDocument.CSI_Code = Constants.AdditionalInformationCodes._1D95;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError(GetAssertMessage(), invoiceLine.JI_ProcedureInfo, messageError);

			additionalDocument.CSI_Code = Constants.AdditionalInformationCodes.N9001;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(GetAssertMessage(), invoiceLine.JI_ProcedureInfo, messageError);

			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(GetAssertMessage(), invoiceLine.JI_ProcedureInfo, messageError);

			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(GetAssertMessage(), invoiceLine.JI_ProcedureInfo, messageError);

			var stylesThatShouldNotTriggerError = new ImportDeclarationTypeList().GetAllCodes().ToList();
			stylesThatShouldNotTriggerError.Remove(ImportDeclarationTypeList.Codes.I1);
			foreach (var style in stylesThatShouldNotTriggerError)
			{
				instruction.CEI_Style = style;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError(GetAssertMessage(), invoiceLine.JI_ProcedureInfo, messageError);
			}
		});

		public void TestCheckBR2312_Condition2() => CombineAssertions(() =>
		{
			var messageError = "[BR2312] If the Dataset is 'I1' or the Requested Procedure is '71', the Additional Documents tab must include both '1D94' and '1D95' Additional References, or none of them.";
			(var validation, var invoiceLine, _, var declaration) = SetupData();
			var instruction = invoiceLine.EntryInstruction;

			var additionalDocument1 = invoiceLine.AdditionalInfos.AddNew();
			var additionalDocument2 = invoiceLine.AdditionalInfos.AddNew();

			string GetAssertMessage()
			{
				return $"when Requested Procedure: {invoiceLine.JI_Procedure}, CEI_Style: {instruction.CEI_Style}, " +
				$"Document 1 with CSI_SubType: {additionalDocument1.CSI_SubType}, CSI_Code: {additionalDocument1.CSI_Code}, " +
				$"Document 2 with CSI_SubType: {additionalDocument2.CSI_SubType}, CSI_Code: {additionalDocument2.CSI_Code}";
			}

			additionalDocument1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalDocument1.CSI_Code = Constants.AdditionalInformationCodes._1D94;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("When one document is added but Requested Procedure not equal to 71 and CEI_Style not equal to I1", invoiceLine.JI_ProcedureInfo, messageError);

			invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._01;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.I1;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError("When one document is added and CEI_Style equal to I1", invoiceLine.JI_ProcedureInfo, messageError);

			additionalDocument2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalDocument2.CSI_Code = Constants.AdditionalInformationCodes._1D95;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(GetAssertMessage(), invoiceLine.JI_ProcedureInfo, messageError);

			additionalDocument1.CSI_Code = Constants.AdditionalInformationCodes.N9001;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError(GetAssertMessage(), invoiceLine.JI_ProcedureInfo, messageError);
			additionalDocument1.CSI_Code = Constants.AdditionalInformationCodes._1D94;

			invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._01;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(GetAssertMessage(), invoiceLine.JI_ProcedureInfo, messageError);
			invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;

			additionalDocument2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError(GetAssertMessage(), invoiceLine.JI_ProcedureInfo, messageError);

			additionalDocument2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError(GetAssertMessage(), invoiceLine.JI_ProcedureInfo, messageError);

			additionalDocument2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(GetAssertMessage(), invoiceLine.JI_ProcedureInfo, messageError);
		});

		public void TestCheckJI_ValuationCode_BR4130()
		{
			(var validation, var invoiceLine, _, var declaration) = SetupData();
			var cusEntryInstruction = invoiceLine.EntryInstruction;
			CombineAssertions(() =>
			{
				AssertValidationCodeIsRequired(ImportDeclarationTypeList.Codes.H1);
				AssertValidationCodeIsRequired(ImportDeclarationTypeList.Codes.H4);
				AssertValidationCodeIsRequired(ImportDeclarationTypeList.Codes.H5);

				cusEntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H3;
				invoiceLine.JI_ValuationCode = ZString.Empty;
				AssertNoMessageError($"if CEI_Style is 'H3' and JI_ValidationCode empty", invoiceLine.JI_ValuationCodeInfo, Message_BR4130);
			});

			void AssertValidationCodeIsRequired(ZString cei_Style)
			{
				cusEntryInstruction.CEI_Style = cei_Style;
				invoiceLine.JI_ValuationCode = ZString.Empty;
				AssertHasMessageError($"if CEI_Style is '{cei_Style}' and JI_ValidationCode empty", invoiceLine.JI_ValuationCodeInfo, Message_BR4130);

				invoiceLine.JI_ValuationCode = "Z";
				AssertNoMessageError($"if CEI_Style is '{cei_Style}' and JI_ValidationCode 'Z'", invoiceLine.JI_ValuationCodeInfo, Message_BR4130);
			}
		}

		const string Message_BR4130 = "[BR4130] Valuation Method is required when Declaration Type is H1, H4 or H5.";

		public void TestCheckAdditionalProcedureCodesAsString_BR1119()
		{
			(_, var invoiceLine, _, _) = SetupData();
			CombineAssertions("[BR1119]", () =>
			{
				var message = "[BR1119] Additional Procedure F48 or F49 cannot be used when Additional Procedure C08 is used.";
				invoiceLine.JI_FormattedProcedure = "4000C08";
				var collection = invoiceLine.AdditionalProcedureCodes;
				var additionalProcedureCode1 = collection.AddNew();
				additionalProcedureCode1.CY_Code = "4000F48";
				var additionalProcedureCode2 = collection.AddNew();
				additionalProcedureCode2.CY_Code = "4000C02";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("Additional procedure [C08, F48, C02]", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);

				additionalProcedureCode1.CY_Code = "4000C05";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("Additional procedure [C08, C05, C02]", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);

				additionalProcedureCode1.CY_Code = "4000F49";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("Additional procedure [C08, F49, C02]", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);

				invoiceLine.JI_FormattedProcedure = "4000C01";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("Additional procedure [C01, F49, C02]", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);

				additionalProcedureCode2.CY_Code = "6000C08";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("Additional procedure [C01, F49, C08]", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);

				additionalProcedureCode1.CY_Code = "4000C03";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("Additional procedure [C01, C03, C08]", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);

				invoiceLine.JI_FormattedProcedure = "5000F49";
				additionalProcedureCode1.CY_Code = "7000C08";
				additionalProcedureCode2.CY_Code = "6000C01";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("Additional procedure [F49, C08, C01]", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);
			});
		}

		public void TestCheckJI_ConcessionOrder_BR8010()
		{
			const string message_BR8010 = "[BR8010] Quota cannot be entered when Declaration Type is 'I1', or when Declaration Type is 'H1' and Sub Type is 'Z'.";
			(_, var invoiceLine, _, var declaration) = SetupData();
			var concessionOrderInfo = invoiceLine.JI_ConcessionOrderInfo;
			var cei = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = cei.PK;

			cei.CEI_Style = ImportDeclarationTypeList.Codes.I1;
			CombineAssertions(cei.CEI_Style, () =>
			{
				invoiceLine.JI_ConcessionOrder = "1";
				AssertHasMessageError("Has error", concessionOrderInfo, message_BR8010);
				invoiceLine.JI_ConcessionOrder = ZString.Empty;
				AssertNoMessageError("No error", concessionOrderInfo, message_BR8010);
			});

			cei.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			CombineAssertions(cei.CEI_Style, () =>
			{
				cei.CEI_SubStyle = EU.Business.EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic;
				invoiceLine.JI_ConcessionOrder = "1";
				AssertHasMessageError("Has error", concessionOrderInfo, message_BR8010);
				invoiceLine.JI_ConcessionOrder = ZString.Empty;
				AssertNoMessageError("No error", concessionOrderInfo, message_BR8010);

				cei.CEI_SubStyle = EU.Business.EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
				invoiceLine.JI_ConcessionOrder = "1";
				AssertNoMessageError("Has error", concessionOrderInfo, message_BR8010);
				invoiceLine.JI_ConcessionOrder = ZString.Empty;
				AssertNoMessageError("No error", concessionOrderInfo, message_BR8010);
			});
		}

		public void TestCheckJI_ConcessionOrder_BR8011()
		{
			const string errorMessage = "[BR8011] The combination of Tariff + Quota + Country of Origin (if Preference starts with 1) + Country of Preferential Origin (if Preference starts with 2, 3, 4, or 5) must be unique across all invoice lines.";

			(_, var invoiceLine1, var invoiceHeader, var declaration) = SetupData();

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine4 = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine5 = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine6 = invoiceHeader.JobComInvoiceLines.AddNew();

			CombineAssertions("[BR8011]", () =>
			{
				invoiceLine1.JI_CEI = entryInstruction1.PK;
				invoiceLine1.JI_Tariff = "0303001010";
				invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
				invoiceLine1.ZG_CountryOfSupply = Core.Constants.CountryCodes.UnitedStates;
				invoiceLine1.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.NormalThirdCountryTariffDuty;
				invoiceLine1.JI_ConcessionOrder = "100001";
				AssertNoMessageError("In invoiceLine1, JI_Tariff is 0303001010, JI_ConcessionOrder is 100001 and target country is AU", invoiceLine1.JI_ConcessionOrderInfo, errorMessage);

				invoiceLine2.JI_CEI = entryInstruction1.PK;
				invoiceLine2.JI_Tariff = "0303001010";
				invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
				invoiceLine2.ZG_CountryOfSupply = Core.Constants.CountryCodes.UnitedStates;
				invoiceLine2.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.TemporaryTariffQuota;
				invoiceLine2.JI_ConcessionOrder = "100001";
				AssertNoMessageError("In invoiceLine2, JI_Tariff is 0303001010, JI_ConcessionOrder is 100001 and target country is US", invoiceLine2.JI_ConcessionOrderInfo, errorMessage);

				invoiceLine3.JI_CEI = entryInstruction1.PK;
				invoiceLine3.JI_Tariff = "0303001010";
				invoiceLine3.JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
				invoiceLine3.ZG_CountryOfSupply = Core.Constants.CountryCodes.Australia;
				invoiceLine3.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.TemporaryTariffQuota;
				invoiceLine3.JI_ConcessionOrder = "100001";
				AssertHasMessageError("In invoiceLine3, JI_Tariff is 0303001010, JI_ConcessionOrder is 100001 and target country is AU", invoiceLine3.JI_ConcessionOrderInfo, errorMessage);
				invoiceLine1.Validation.ValidateJI_ConcessionOrder();
				AssertHasMessageError("In invoiceLine1, JI_Tariff is 0303001010, JI_ConcessionOrder is 100001 and target country is AU", invoiceLine1.JI_ConcessionOrderInfo, errorMessage);

				invoiceLine4.JI_CEI = entryInstruction2.PK;
				invoiceLine4.JI_Tariff = "0303001010";
				invoiceLine4.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
				invoiceLine4.ZG_CountryOfSupply = Core.Constants.CountryCodes.UnitedStates;
				invoiceLine4.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.NormalThirdCountryTariffDuty;
				invoiceLine4.JI_ConcessionOrder = "100001";
				AssertNoMessageError("In invoiceLine4, JI_Tariff is 0303001010, JI_ConcessionOrder is 100001 and target country is AU, with different entry instruction", invoiceLine4.JI_ConcessionOrderInfo, errorMessage);

				invoiceLine1.JI_ConcessionOrder = "100005";
				invoiceLine5.JI_CEI = entryInstruction1.PK;
				invoiceLine5.JI_Tariff = "0303001010";
				invoiceLine5.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
				invoiceLine5.ZG_CountryOfSupply = Core.Constants.CountryCodes.UnitedStates;
				invoiceLine5.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.NormalThirdCountryTariffDuty;
				invoiceLine5.JI_ConcessionOrder = "100005";
				AssertHasMessageError("In invoiceLine5, JI_Tariff is 0303001010, JI_ConcessionOrder is 100005 and target country is US (same as invoiceLine1 now)", invoiceLine5.JI_ConcessionOrderInfo, errorMessage);

				invoiceLine1.JI_ConcessionOrder = "";
				invoiceLine6.JI_CEI = entryInstruction1.PK;
				invoiceLine6.JI_Tariff = "0303001010";
				invoiceLine6.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
				invoiceLine6.ZG_CountryOfSupply = Core.Constants.CountryCodes.UnitedStates;
				invoiceLine6.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.NormalThirdCountryTariffDuty;
				invoiceLine6.JI_ConcessionOrder = "";
				AssertNoMessageError("In invoiceLine6, JI_Tariff is 0303001010, JI_ConcessionOrder is empty and target country is US (same as invoiceLine1 now)", invoiceLine6.JI_ConcessionOrderInfo, errorMessage);
			});
		}

		public void TestCheckAdditionalProcedureCodesAsString_BR11106()
		{
			(_, var invoiceLine, var invoice, var declaration) = SetupData();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var message = "[BR11106] The combination of additional procedures C07 and C08 is not allowed, even in different invoice lines.";
			CombineAssertions("[BR11106] Single line", () =>
			{
				invoiceLine.JI_FormattedProcedure = "4000C07";
				var collection = invoiceLine.AdditionalProcedureCodes;
				var additionalProcedureCode1 = collection.AddNew();
				additionalProcedureCode1.CY_Code = "4000F48";
				var additionalProcedureCode2 = collection.AddNew();
				additionalProcedureCode2.CY_Code = "4000C08";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("Additional procedure [C07, F48, C08]", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);

				invoiceLine.JI_FormattedProcedure = "4000C06";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("Additional procedure [C06, F48, C08]", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);

				additionalProcedureCode1.CY_Code = "5000C07";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("Additional procedure [C06, C07, C08]", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);

				invoiceLine.JI_FormattedProcedure = "8000C08";
				additionalProcedureCode1.CY_Code = "5000C06";
				additionalProcedureCode2.CY_Code = "4000C09";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("Additional procedure [C08, C06, C09]", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);

				additionalProcedureCode1.CY_Code = "3000C07";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("Additional procedure [C08, C07, C09]", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);
			});

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = entryInstruction.PK;
			CombineAssertions("[BR11106] Multiple lines Case 1", () =>
			{
				invoiceLine.AdditionalProcedureCodes.RemoveAndDeleteAll();
				invoiceLine.JI_FormattedProcedure = "4000C01";
				invoiceLine2.JI_FormattedProcedure = "4000C01";
				invoiceLine3.JI_FormattedProcedure = "4000C01";
				invoiceLine4.JI_FormattedProcedure = "4000C01";

				var line1Collection = invoiceLine.AdditionalProcedureCodes;
				var line1AdditionalProcedureCode1 = line1Collection.AddNew();
				line1AdditionalProcedureCode1.CY_Code = "4000C08";

				var line2Collection = invoiceLine2.AdditionalProcedureCodes;
				var line2AdditionalProcedureCode1 = line2Collection.AddNew();
				line2AdditionalProcedureCode1.CY_Code = "4000C08";

				var line3Collection = invoiceLine3.AdditionalProcedureCodes;
				var line3AdditionalProcedureCode1 = line3Collection.AddNew();
				line3AdditionalProcedureCode1.CY_Code = "1000C07";
				var line3AdditionalProcedureCode2 = line3Collection.AddNew();
				line3AdditionalProcedureCode2.CY_Code = "4000F48";

				var line4Collection = invoiceLine4.AdditionalProcedureCodes;
				var line4AdditionalProcedureCode1 = line4Collection.AddNew();
				line4AdditionalProcedureCode1.CY_Code = "1000C05";
				var line4AdditionalProcedureCode2 = line4Collection.AddNew();
				line4AdditionalProcedureCode2.CY_Code = "4000C02";

				invoice.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.Validation.ValidateAdditionalProcedureCodesAsString());
				AssertHasMessageError("Line1", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);
				AssertNoMessageError("Line2", invoiceLine2.AdditionalProcedureCodesAsStringInfo, message);
				AssertHasMessageError("Line3", invoiceLine3.AdditionalProcedureCodesAsStringInfo, message);
				AssertNoMessageError("Line4", invoiceLine4.AdditionalProcedureCodesAsStringInfo, message);
			});

			CombineAssertions("[BR11106] Multiple lines Case 2", () =>
			{
				invoiceLine.AdditionalProcedureCodes.RemoveAndDeleteAll();
				invoiceLine2.AdditionalProcedureCodes.RemoveAndDeleteAll();
				invoiceLine3.AdditionalProcedureCodes.RemoveAndDeleteAll();
				invoiceLine4.AdditionalProcedureCodes.RemoveAndDeleteAll();
				invoiceLine.JI_FormattedProcedure = "4000C01";
				invoiceLine2.JI_FormattedProcedure = "4000C07";
				invoiceLine3.JI_FormattedProcedure = "4000C07";
				invoiceLine4.JI_FormattedProcedure = "4000C01";

				var line1Collection = invoiceLine.AdditionalProcedureCodes;
				var line1AdditionalProcedureCode1 = line1Collection.AddNew();
				line1AdditionalProcedureCode1.CY_Code = "4000C03";

				var line2Collection = invoiceLine2.AdditionalProcedureCodes;
				var line2AdditionalProcedureCode1 = line2Collection.AddNew();
				line2AdditionalProcedureCode1.CY_Code = "4000C08";

				var line3Collection = invoiceLine3.AdditionalProcedureCodes;
				var line3AdditionalProcedureCode1 = line3Collection.AddNew();
				line3AdditionalProcedureCode1.CY_Code = "1000C08";
				var line3AdditionalProcedureCode2 = line3Collection.AddNew();
				line3AdditionalProcedureCode2.CY_Code = "4000F48";

				var line4Collection = invoiceLine4.AdditionalProcedureCodes;
				var line4AdditionalProcedureCode1 = line4Collection.AddNew();
				line4AdditionalProcedureCode1.CY_Code = "1000C05";
				var line4AdditionalProcedureCode2 = line4Collection.AddNew();
				line4AdditionalProcedureCode2.CY_Code = "4000C02";

				invoice.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.Validation.ValidateAdditionalProcedureCodesAsString());
				AssertNoMessageError("Line1", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);
				AssertHasMessageError("Line2", invoiceLine2.AdditionalProcedureCodesAsStringInfo, message);
				AssertHasMessageError("Line3", invoiceLine3.AdditionalProcedureCodesAsStringInfo, message);
				AssertNoMessageError("Line4", invoiceLine4.AdditionalProcedureCodesAsStringInfo, message);
			});

			CombineAssertions("[BR11106] Multiple lines Case 3", () =>
			{
				invoiceLine.AdditionalProcedureCodes.RemoveAndDeleteAll();
				invoiceLine2.AdditionalProcedureCodes.RemoveAndDeleteAll();
				invoiceLine3.AdditionalProcedureCodes.RemoveAndDeleteAll();
				invoiceLine4.AdditionalProcedureCodes.RemoveAndDeleteAll();
				invoiceLine.JI_FormattedProcedure = "4000C01";
				invoiceLine2.JI_FormattedProcedure = "4000C08";
				invoiceLine3.JI_FormattedProcedure = "4000C07";
				invoiceLine4.JI_FormattedProcedure = "4000C08";

				var line1Collection = invoiceLine.AdditionalProcedureCodes;
				var line1AdditionalProcedureCode1 = line1Collection.AddNew();
				line1AdditionalProcedureCode1.CY_Code = "4000C03";

				var line2Collection = invoiceLine2.AdditionalProcedureCodes;
				var line2AdditionalProcedureCode1 = line2Collection.AddNew();
				line2AdditionalProcedureCode1.CY_Code = "4000C10";

				var line3Collection = invoiceLine3.AdditionalProcedureCodes;
				var line3AdditionalProcedureCode1 = line3Collection.AddNew();
				line3AdditionalProcedureCode1.CY_Code = "2000C05";
				var line3AdditionalProcedureCode2 = line3Collection.AddNew();
				line3AdditionalProcedureCode2.CY_Code = "4000F48";

				var line4Collection = invoiceLine4.AdditionalProcedureCodes;
				var line4AdditionalProcedureCode1 = line4Collection.AddNew();
				line4AdditionalProcedureCode1.CY_Code = "1000C05";
				var line4AdditionalProcedureCode2 = line4Collection.AddNew();
				line4AdditionalProcedureCode2.CY_Code = "4000C02";

				invoice.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.Validation.ValidateAdditionalProcedureCodesAsString());
				AssertNoMessageError("Line1", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);
				AssertNoMessageError("Line2", invoiceLine2.AdditionalProcedureCodesAsStringInfo, message);
				AssertHasMessageError("Line3", invoiceLine3.AdditionalProcedureCodesAsStringInfo, message);
				AssertHasMessageError("Line4", invoiceLine4.AdditionalProcedureCodesAsStringInfo, message);
			});
		}

		public void TestCheckAdditionalProcedureCodesAsString_BR11107()
		{
			(_, var invoiceLine, var invoice, var declaration) = SetupData();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var message = "[BR11107] The combination of additional procedures C07 and 1C1 is not allowed, even in different invoice lines.";
			CombineAssertions("[BR11107] Single line", () =>
			{
				invoiceLine.JI_FormattedProcedure = "4000C07";
				var collection = invoiceLine.AdditionalProcedureCodes;
				var additionalProcedureCode1 = collection.AddNew();
				additionalProcedureCode1.CY_Code = "4000F48";
				var additionalProcedureCode2 = collection.AddNew();
				additionalProcedureCode2.CY_Code = "40001C1";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("Additional procedure [C07, F48, 1C1]", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);

				invoiceLine.JI_FormattedProcedure = "4000C06";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("Additional procedure [C06, F48, 1C1]", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);

				additionalProcedureCode1.CY_Code = "5000C07";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("Additional procedure [C06, C07, 1C1]", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);

				invoiceLine.JI_FormattedProcedure = "80001C1";
				additionalProcedureCode1.CY_Code = "5000C06";
				additionalProcedureCode2.CY_Code = "4000C09";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("Additional procedure [1C1, C06, C09]", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);

				additionalProcedureCode1.CY_Code = "3000C07";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("Additional procedure [1C1, C07, C09]", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);
			});

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = entryInstruction.PK;
			CombineAssertions("[BR11107] Multiple lines Case 1", () =>
			{
				invoiceLine.AdditionalProcedureCodes.RemoveAndDeleteAll();
				invoiceLine.JI_FormattedProcedure = "4000C01";
				invoiceLine2.JI_FormattedProcedure = "4000C01";
				invoiceLine3.JI_FormattedProcedure = "4000C01";
				invoiceLine4.JI_FormattedProcedure = "4000C01";

				var line1Collection = invoiceLine.AdditionalProcedureCodes;
				var line1AdditionalProcedureCode1 = line1Collection.AddNew();
				line1AdditionalProcedureCode1.CY_Code = "40001C1";

				var line2Collection = invoiceLine2.AdditionalProcedureCodes;
				var line2AdditionalProcedureCode1 = line2Collection.AddNew();
				line2AdditionalProcedureCode1.CY_Code = "40001C1";

				var line3Collection = invoiceLine3.AdditionalProcedureCodes;
				var line3AdditionalProcedureCode1 = line3Collection.AddNew();
				line3AdditionalProcedureCode1.CY_Code = "1000C07";
				var line3AdditionalProcedureCode2 = line3Collection.AddNew();
				line3AdditionalProcedureCode2.CY_Code = "4000F48";

				var line4Collection = invoiceLine4.AdditionalProcedureCodes;
				var line4AdditionalProcedureCode1 = line4Collection.AddNew();
				line4AdditionalProcedureCode1.CY_Code = "1000C05";
				var line4AdditionalProcedureCode2 = line4Collection.AddNew();
				line4AdditionalProcedureCode2.CY_Code = "4000C02";

				invoice.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.Validation.ValidateAdditionalProcedureCodesAsString());
				AssertHasMessageError("Line1", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);
				AssertNoMessageError("Line2", invoiceLine2.AdditionalProcedureCodesAsStringInfo, message);
				AssertHasMessageError("Line3", invoiceLine3.AdditionalProcedureCodesAsStringInfo, message);
				AssertNoMessageError("Line4", invoiceLine4.AdditionalProcedureCodesAsStringInfo, message);
			});

			CombineAssertions("[BR11107] Multiple lines Case 2", () =>
			{
				invoiceLine.AdditionalProcedureCodes.RemoveAndDeleteAll();
				invoiceLine2.AdditionalProcedureCodes.RemoveAndDeleteAll();
				invoiceLine3.AdditionalProcedureCodes.RemoveAndDeleteAll();
				invoiceLine4.AdditionalProcedureCodes.RemoveAndDeleteAll();
				invoiceLine.JI_FormattedProcedure = "4000C01";
				invoiceLine2.JI_FormattedProcedure = "4000C07";
				invoiceLine3.JI_FormattedProcedure = "4000C07";
				invoiceLine4.JI_FormattedProcedure = "4000C01";

				var line1Collection = invoiceLine.AdditionalProcedureCodes;
				var line1AdditionalProcedureCode1 = line1Collection.AddNew();
				line1AdditionalProcedureCode1.CY_Code = "4000C03";

				var line2Collection = invoiceLine2.AdditionalProcedureCodes;
				var line2AdditionalProcedureCode1 = line2Collection.AddNew();
				line2AdditionalProcedureCode1.CY_Code = "40001C1";

				var line3Collection = invoiceLine3.AdditionalProcedureCodes;
				var line3AdditionalProcedureCode1 = line3Collection.AddNew();
				line3AdditionalProcedureCode1.CY_Code = "10001C1";
				var line3AdditionalProcedureCode2 = line3Collection.AddNew();
				line3AdditionalProcedureCode2.CY_Code = "4000F48";

				var line4Collection = invoiceLine4.AdditionalProcedureCodes;
				var line4AdditionalProcedureCode1 = line4Collection.AddNew();
				line4AdditionalProcedureCode1.CY_Code = "1000C05";
				var line4AdditionalProcedureCode2 = line4Collection.AddNew();
				line4AdditionalProcedureCode2.CY_Code = "4000C02";

				invoice.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.Validation.ValidateAdditionalProcedureCodesAsString());
				AssertNoMessageError("Line1", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);
				AssertHasMessageError("Line2", invoiceLine2.AdditionalProcedureCodesAsStringInfo, message);
				AssertHasMessageError("Line3", invoiceLine3.AdditionalProcedureCodesAsStringInfo, message);
				AssertNoMessageError("Line4", invoiceLine4.AdditionalProcedureCodesAsStringInfo, message);
			});

			CombineAssertions("[BR11107] Multiple lines Case 3", () =>
			{
				invoiceLine.AdditionalProcedureCodes.RemoveAndDeleteAll();
				invoiceLine2.AdditionalProcedureCodes.RemoveAndDeleteAll();
				invoiceLine3.AdditionalProcedureCodes.RemoveAndDeleteAll();
				invoiceLine4.AdditionalProcedureCodes.RemoveAndDeleteAll();
				invoiceLine.JI_FormattedProcedure = "4000C01";
				invoiceLine2.JI_FormattedProcedure = "40001C1";
				invoiceLine3.JI_FormattedProcedure = "4000C07";
				invoiceLine4.JI_FormattedProcedure = "40001C1";

				var line1Collection = invoiceLine.AdditionalProcedureCodes;
				var line1AdditionalProcedureCode1 = line1Collection.AddNew();
				line1AdditionalProcedureCode1.CY_Code = "4000C03";

				var line2Collection = invoiceLine2.AdditionalProcedureCodes;
				var line2AdditionalProcedureCode1 = line2Collection.AddNew();
				line2AdditionalProcedureCode1.CY_Code = "4000C10";

				var line3Collection = invoiceLine3.AdditionalProcedureCodes;
				var line3AdditionalProcedureCode1 = line3Collection.AddNew();
				line3AdditionalProcedureCode1.CY_Code = "2000C05";
				var line3AdditionalProcedureCode2 = line3Collection.AddNew();
				line3AdditionalProcedureCode2.CY_Code = "4000F48";

				var line4Collection = invoiceLine4.AdditionalProcedureCodes;
				var line4AdditionalProcedureCode1 = line4Collection.AddNew();
				line4AdditionalProcedureCode1.CY_Code = "1000C05";
				var line4AdditionalProcedureCode2 = line4Collection.AddNew();
				line4AdditionalProcedureCode2.CY_Code = "4000C02";

				invoice.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.Validation.ValidateAdditionalProcedureCodesAsString());
				AssertNoMessageError("Line1", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);
				AssertNoMessageError("Line2", invoiceLine2.AdditionalProcedureCodesAsStringInfo, message);
				AssertHasMessageError("Line3", invoiceLine3.AdditionalProcedureCodesAsStringInfo, message);
				AssertHasMessageError("Line4", invoiceLine4.AdditionalProcedureCodesAsStringInfo, message);
			});
		}

		public void TestCheckAdditionalProcedureCodesAsString_BR600000()
		{
			(var invoiceLineValidation, var invoiceLine, var invoice, var declaration) = SetupData();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var errorMessage = "[BR600000] In H1, if C07 is declared as an Additional Procedure, then only F48 or none can also be included as Additional Procedure.";
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
				invoiceLine.JI_FormattedProcedure = "4000C07";
				invoiceLineValidation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("The invoice line has only one procedure code C07", invoiceLine.AdditionalProcedureCodesAsStringInfo, errorMessage);

				var additionalCodes = invoiceLine.AdditionalProcedureCodes;
				var additionalCode1 = additionalCodes.AddNew();
				additionalCode1.CY_Code = "4000F15";
				invoiceLineValidation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("The invoice line has procedure code C07 and another code that is not F48", invoiceLine.AdditionalProcedureCodesAsStringInfo, errorMessage);

				additionalCode1.CY_Code = "4000F48";
				invoiceLineValidation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("The invoice line has procedure code C07 and another code that is F48", invoiceLine.AdditionalProcedureCodesAsStringInfo, errorMessage);

				var additionalCode2 = additionalCodes.AddNew();
				additionalCode2.CY_Code = "4000F15";
				invoiceLineValidation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("The invoice line has procedure code C07, F48 and a code that is not F48", invoiceLine.AdditionalProcedureCodesAsStringInfo, errorMessage);

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H2;
				invoiceLine.JI_FormattedProcedure = "4000C07";
				additionalCode2.CY_Code = "4000F15";
				invoiceLineValidation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("The invoice line's EntryInstruction style code is not H1", invoiceLine.AdditionalProcedureCodesAsStringInfo, errorMessage);
			});
		}

		public void TestCheckAdditionalProcedureCodesAsString_BR3399()
		{
			var (invoiceLineValidation, invoiceLine, invoice, declaration) = SetupData();

			Assert_BR3399(invoiceLineValidation, invoiceLine, invoice, declaration, invoiceLine.AdditionalProcedureCodesAsStringInfo);
		}

		public void TestJI_FormattedProcedure_BR11106()
		{
			(_, var invoiceLine, var invoice, var declaration) = SetupData();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var message = "[BR11106] The combination of additional procedures C07 and C08 is not allowed, even in different invoice lines.";
			CombineAssertions("[BR11106] Single line", () =>
			{
				invoiceLine.JI_FormattedProcedure = "4000C07";
				var collection = invoiceLine.AdditionalProcedureCodes;
				var additionalProcedureCode1 = collection.AddNew();
				additionalProcedureCode1.CY_Code = "4000F48";
				var additionalProcedureCode2 = collection.AddNew();
				additionalProcedureCode2.CY_Code = "4000C08";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertHasMessageError("Additional procedure [C07, F48, C08]", invoiceLine.JI_FormattedProcedureInfo, message);

				additionalProcedureCode2.CY_Code = "4000C09";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("Additional procedure [C07, F48, C09]", invoiceLine.JI_FormattedProcedureInfo, message);

				invoiceLine.JI_FormattedProcedure = "4000C08";
				additionalProcedureCode2.CY_Code = "3000C07";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertHasMessageError("Additional procedure [C08, F48, C07]", invoiceLine.JI_FormattedProcedureInfo, message);

				additionalProcedureCode2.CY_Code = "3000C06";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("Additional procedure [C08, F48, C06]", invoiceLine.JI_FormattedProcedureInfo, message);

				invoiceLine.JI_FormattedProcedure = "4000C02";
				additionalProcedureCode1.CY_Code = "4000C07";
				additionalProcedureCode2.CY_Code = "4000C08";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("Additional procedure [C02, C07, C08]", invoiceLine.JI_FormattedProcedureInfo, message);
			});

			invoiceLine.AdditionalProcedureCodes.RemoveAndDeleteAll();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = entryInstruction.PK;
			CombineAssertions("[BR11106] Multiple lines", () =>
			{
				invoiceLine.JI_FormattedProcedure = "4000C09";
				var line1AddCPCs = invoiceLine.AdditionalProcedureCodes.AddNew();
				line1AddCPCs.CY_Code = "4000C08";

				invoiceLine2.JI_FormattedProcedure = "4000C09";
				var line2AddCPCs = invoiceLine2.AdditionalProcedureCodes.AddNew();
				line2AddCPCs.CY_Code = "4000C08";

				invoiceLine3.JI_FormattedProcedure = "4000C07";
				var line3AddCPCs = invoiceLine3.AdditionalProcedureCodes.AddNew();
				line3AddCPCs.CY_Code = "4000C10";

				invoice.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.Validation.ValidateJI_Procedure());
				AssertNoMessageError("Line1", invoiceLine.JI_FormattedProcedureInfo, message);
				AssertNoMessageError("Line2", invoiceLine2.JI_FormattedProcedureInfo, message);
				AssertHasMessageError("Line3", invoiceLine3.JI_FormattedProcedureInfo, message);

				invoiceLine4.JI_FormattedProcedure = "4000C08";
				var line4AddCPCs = invoiceLine4.AdditionalProcedureCodes.AddNew();
				line4AddCPCs.CY_Code = "4000C10";
				invoice.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.Validation.ValidateJI_Procedure());
				AssertNoMessageError("Line1", invoiceLine.JI_FormattedProcedureInfo, message);
				AssertNoMessageError("Line2", invoiceLine2.JI_FormattedProcedureInfo, message);
				AssertHasMessageError("Line3", invoiceLine3.JI_FormattedProcedureInfo, message);
				AssertHasMessageError("Line4", invoiceLine4.JI_FormattedProcedureInfo, message);

				invoiceLine3.JI_FormattedProcedure = "4000C01";
				invoice.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.Validation.ValidateJI_Procedure());
				AssertNoMessageError("Line1", invoiceLine.JI_FormattedProcedureInfo, message);
				AssertNoMessageError("Line2", invoiceLine2.JI_FormattedProcedureInfo, message);
				AssertNoMessageError("Line3", invoiceLine3.JI_FormattedProcedureInfo, message);
				AssertNoMessageError("Line4", invoiceLine4.JI_FormattedProcedureInfo, message);

				line1AddCPCs.CY_Code = "4000C07";
				invoice.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.Validation.ValidateJI_Procedure());
				AssertNoMessageError("Line1", invoiceLine.JI_FormattedProcedureInfo, message);
				AssertNoMessageError("Line2", invoiceLine2.JI_FormattedProcedureInfo, message);
				AssertNoMessageError("Line3", invoiceLine3.JI_FormattedProcedureInfo, message);
				AssertHasMessageError("Line4", invoiceLine4.JI_FormattedProcedureInfo, message);
			});
		}

		public void TestJI_FormattedProcedure_BR11107()
		{
			(_, var invoiceLine, var invoice, var declaration) = SetupData();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var message = "[BR11107] The combination of additional procedures C07 and 1C1 is not allowed, even in different invoice lines.";
			CombineAssertions("[BR11107] Single line", () =>
			{
				invoiceLine.JI_FormattedProcedure = "4000C07";
				var collection = invoiceLine.AdditionalProcedureCodes;
				var additionalProcedureCode1 = collection.AddNew();
				additionalProcedureCode1.CY_Code = "4000F48";
				var additionalProcedureCode2 = collection.AddNew();
				additionalProcedureCode2.CY_Code = "40001C1";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertHasMessageError("Additional procedure [C07, F48, 1C1]", invoiceLine.JI_FormattedProcedureInfo, message);

				additionalProcedureCode2.CY_Code = "4000C09";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("Additional procedure [C07, F48, C09]", invoiceLine.JI_FormattedProcedureInfo, message);

				invoiceLine.JI_FormattedProcedure = "40001C1";
				additionalProcedureCode2.CY_Code = "3000C07";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertHasMessageError("Additional procedure [1C1, F48, C07]", invoiceLine.JI_FormattedProcedureInfo, message);

				additionalProcedureCode2.CY_Code = "3000C06";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("Additional procedure [1C1, F48, C06]", invoiceLine.JI_FormattedProcedureInfo, message);

				invoiceLine.JI_FormattedProcedure = "4000C02";
				additionalProcedureCode1.CY_Code = "4000C07";
				additionalProcedureCode2.CY_Code = "40001C1";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("Additional procedure [C02, C07, 1C1]", invoiceLine.JI_FormattedProcedureInfo, message);
			});

			invoiceLine.AdditionalProcedureCodes.RemoveAndDeleteAll();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = entryInstruction.PK;
			CombineAssertions("[BR11107] Multiple lines", () =>
			{
				invoiceLine.JI_FormattedProcedure = "4000C09";
				var line1AddCPCs = invoiceLine.AdditionalProcedureCodes.AddNew();
				line1AddCPCs.CY_Code = "40001C1";

				invoiceLine2.JI_FormattedProcedure = "4000C09";
				var line2AddCPCs = invoiceLine2.AdditionalProcedureCodes.AddNew();
				line2AddCPCs.CY_Code = "40001C1";

				invoiceLine3.JI_FormattedProcedure = "4000C07";
				var line3AddCPCs = invoiceLine3.AdditionalProcedureCodes.AddNew();
				line3AddCPCs.CY_Code = "4000C10";

				invoice.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.Validation.ValidateJI_Procedure());
				AssertNoMessageError("Line1", invoiceLine.JI_FormattedProcedureInfo, message);
				AssertNoMessageError("Line2", invoiceLine2.JI_FormattedProcedureInfo, message);
				AssertHasMessageError("Line3", invoiceLine3.JI_FormattedProcedureInfo, message);

				invoiceLine4.JI_FormattedProcedure = "40001C1";
				var line4AddCPCs = invoiceLine4.AdditionalProcedureCodes.AddNew();
				line4AddCPCs.CY_Code = "4000C10";
				invoice.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.Validation.ValidateJI_Procedure());
				AssertNoMessageError("Line1", invoiceLine.JI_FormattedProcedureInfo, message);
				AssertNoMessageError("Line2", invoiceLine2.JI_FormattedProcedureInfo, message);
				AssertHasMessageError("Line3", invoiceLine3.JI_FormattedProcedureInfo, message);
				AssertHasMessageError("Line4", invoiceLine4.JI_FormattedProcedureInfo, message);

				invoiceLine3.JI_FormattedProcedure = "4000C01";
				invoice.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.Validation.ValidateJI_Procedure());
				AssertNoMessageError("Line1", invoiceLine.JI_FormattedProcedureInfo, message);
				AssertNoMessageError("Line2", invoiceLine2.JI_FormattedProcedureInfo, message);
				AssertNoMessageError("Line3", invoiceLine3.JI_FormattedProcedureInfo, message);
				AssertNoMessageError("Line4", invoiceLine4.JI_FormattedProcedureInfo, message);

				line1AddCPCs.CY_Code = "4000C07";
				invoice.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.Validation.ValidateJI_Procedure());
				AssertNoMessageError("Line1", invoiceLine.JI_FormattedProcedureInfo, message);
				AssertNoMessageError("Line2", invoiceLine2.JI_FormattedProcedureInfo, message);
				AssertNoMessageError("Line3", invoiceLine3.JI_FormattedProcedureInfo, message);
				AssertHasMessageError("Line4", invoiceLine4.JI_FormattedProcedureInfo, message);
			});
		}

		public void TestCheckAdditionalProcedureCodesAsString_BR599999()
		{
			(_, var invoiceLine, var invoice, var declaration) = SetupData();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var message = "[BR599999] If Additional Procedure F48 or F49 is declared, then the same code must be declared in all the declaration items.";
			CombineAssertions(() =>
			{
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_FormattedProcedure = "4000C01";
				var collection1 = invoiceLine.AdditionalProcedureCodes;
				var additionalProcedureCode1 = collection1.AddNew();
				additionalProcedureCode1.CY_Code = "4000F48";
				var additionalProcedureCode2 = collection1.AddNew();
				additionalProcedureCode2.CY_Code = "4000F44";
				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction.PK;
				invoiceLine2.JI_FormattedProcedure = "4000C02";
				var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine3.JI_CEI = entryInstruction.PK;
				invoiceLine3.JI_FormattedProcedure = "4000C03";
				var additionalProcedureCode3 = invoiceLine3.AdditionalProcedureCodes.AddNew();
				additionalProcedureCode3.CY_Code = "5000F48";

				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("invoice line 1 has F48 code.", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);

				invoiceLine2.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("invoice line 2 has not F48 code.", invoiceLine2.AdditionalProcedureCodesAsStringInfo, message);

				invoiceLine3.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("invoice line 3 has F48 code.", invoiceLine3.AdditionalProcedureCodesAsStringInfo, message);

				additionalProcedureCode1.CY_Code = "4000F49";
				additionalProcedureCode3.CY_Code = "5000F49";
				invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("invoice line 1 has F49 code.", invoiceLine.AdditionalProcedureCodesAsStringInfo, message);

				invoiceLine2.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("invoice line 2 has not F49 code.", invoiceLine2.AdditionalProcedureCodesAsStringInfo, message);

				invoiceLine3.Validation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("invoice line 3 has F49 code.", invoiceLine3.AdditionalProcedureCodesAsStringInfo, message);
			});
		}

		public void TestJI_FormattedProcedure_BR599999()
		{
			(_, var invoiceLine, var invoice, var declaration) = SetupData();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var message = "[BR599999] If Additional Procedure F48 or F49 is declared, then the same code must be declared in all the declaration items.";
			CombineAssertions(() =>
			{
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_FormattedProcedure = "4000C01";
				var collection1 = invoiceLine.AdditionalProcedureCodes;
				var additionalProcedureCode1 = collection1.AddNew();
				additionalProcedureCode1.CY_Code = "4000F48";
				var additionalProcedureCode2 = collection1.AddNew();
				additionalProcedureCode2.CY_Code = "4000F44";
				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction.PK;
				invoiceLine2.JI_FormattedProcedure = "4000C02";
				var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine3.JI_CEI = entryInstruction.PK;
				invoiceLine3.JI_FormattedProcedure = "4000C03";
				var additionalProcedureCode3 = invoiceLine3.AdditionalProcedureCodes.AddNew();
				additionalProcedureCode3.CY_Code = "5000F48";

				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("invoice line 1 has F48 code.", invoiceLine.JI_FormattedProcedureInfo, message);

				invoiceLine2.Validation.ValidateJI_Procedure();
				AssertHasMessageError("invoice line 2 has not F48 code.", invoiceLine2.JI_FormattedProcedureInfo, message);

				invoiceLine3.Validation.ValidateJI_Procedure();
				AssertNoMessageError("invoice line 3 has F48 code.", invoiceLine3.JI_FormattedProcedureInfo, message);

				additionalProcedureCode1.CY_Code = "4000F49";
				additionalProcedureCode3.CY_Code = "5000F49";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("invoice line 1 has F49 code.", invoiceLine.JI_FormattedProcedureInfo, message);

				invoiceLine2.Validation.ValidateJI_Procedure();
				AssertHasMessageError("invoice line 2 has not F49 code.", invoiceLine2.JI_FormattedProcedureInfo, message);

				invoiceLine3.Validation.ValidateJI_Procedure();
				AssertNoMessageError("invoice line 3 has F49 code.", invoiceLine3.JI_FormattedProcedureInfo, message);
			});
		}

		public void TestJI_FormattedProcedure_BR600000()
		{
			(var invoiceLineValidation, var invoiceLine, var invoice, var declaration) = SetupData();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var errorMessage = "[BR600000] In H1, if C07 is declared as an Additional Procedure, then only F48 or none can also be included as Additional Procedure.";
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
				invoiceLine.JI_FormattedProcedure = "4000C07";
				invoiceLineValidation.ValidateJI_Procedure();
				AssertNoMessageError("The invoice line has only one procedure code C07", invoiceLine.JI_FormattedProcedureInfo, errorMessage);

				var additionalCodes = invoiceLine.AdditionalProcedureCodes;
				var additionalCode1 = additionalCodes.AddNew();
				additionalCode1.CY_Code = "4000F15";
				invoiceLineValidation.ValidateJI_Procedure();
				AssertHasMessageError("The invoice line has procedure code C07 and another code that is not F48", invoiceLine.JI_FormattedProcedureInfo, errorMessage);

				additionalCode1.CY_Code = "4000F48";
				invoiceLineValidation.ValidateJI_Procedure();
				AssertNoMessageError("The invoice line has procedure code C07 and another code that is F48", invoiceLine.JI_FormattedProcedureInfo, errorMessage);

				var additionalCode2 = additionalCodes.AddNew();
				additionalCode2.CY_Code = "4000F15";
				invoiceLineValidation.ValidateJI_Procedure();
				AssertHasMessageError("The invoice line has procedure code C07, F48 and a code that is not F48", invoiceLine.JI_FormattedProcedureInfo, errorMessage);

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H2;
				invoiceLine.JI_FormattedProcedure = "4000C07";
				additionalCode2.CY_Code = "4000F15";
				invoiceLineValidation.ValidateJI_Procedure();
				AssertNoMessageError("The invoice line's EntryInstruction style code is not H1", invoiceLine.JI_FormattedProcedureInfo, errorMessage);
			});
		}

		public void TestJI_FormattedProcedure_BR2001()
		{
			(_, var invoiceLine, var invoice, var declaration) = SetupData();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var procedureCodeIsNotForBR2001 = new List<ZString> { "01", "08", "09" };
			var procedureCodeForBR2001 = new[] { "07", "41", "43", "45", "51", "53", "54", "71", "76", "77", "78", "10", "11", "21", "22", "23" };
			CombineAssertions(() =>
			{
				procedureCodeForBR2001.ForEach(x =>
				{
					AssertProcedure_BR2001(invoiceLine, x, invoiceLine.JI_ProcedureInfo, true);
				});

				procedureCodeIsNotForBR2001.ForEach(x =>
				{
					AssertProcedure_BR2001(invoiceLine, x, invoiceLine.JI_ProcedureInfo);
				});

				var preDoc = entryInstruction.PreviousDocuments.AddNew();
				preDoc.CSI_Code = Constants.PreviousDocumentTypeCodes.MRN;
				procedureCodeForBR2001.ForEach(x =>
				{
					AssertProcedure_BR2001(invoiceLine, x, invoiceLine.JI_ProcedureInfo);
				});

				entryInstruction.PreviousDocuments.RemoveAndDeleteAll();
				preDoc = invoice.PreviousDocuments.AddNew();
				preDoc.CSI_Code = Constants.PreviousDocumentTypeCodes.MRN;
				procedureCodeForBR2001.ForEach(x =>
				{
					AssertProcedure_BR2001(invoiceLine, x, invoiceLine.JI_ProcedureInfo);
				});

				invoice.PreviousDocuments.RemoveAndDeleteAll();
				preDoc = invoiceLine.PreviousDocuments.AddNew();
				preDoc.CSI_Code = Constants.PreviousDocumentTypeCodes.MRN;
				procedureCodeForBR2001.ForEach(x =>
				{
					AssertProcedure_BR2001(invoiceLine, x, invoiceLine.JI_ProcedureInfo);
				});
			});
		}

		void AssertProcedure_BR2001(JobComInvoiceLine line, ZString previousProcedureCode, ZPropertyInfo info, bool hasErrorMessage = false)
		{
			var errorMessage = "[BR2001] Please enter an MRN number under 'Previous Documents' if that MRN number applies to all invoice lines of the entry. Alternatively, enter an MRN number under 'Invoice Lines > Previous Documents' if that MRN number applies specifically to this invoice line.";
			line.JI_Procedure = $"40{previousProcedureCode}C01";
			if (hasErrorMessage)
			{
				AssertHasMessageError($"Previous Procedure is {previousProcedureCode}", info, errorMessage);
			}
			else
			{
				AssertNoMessageError($"Previous Procedure is {previousProcedureCode}", info, errorMessage);
			}
		}

		public void TestJI_FormattedProcedure_BR3399()
		{
			var (invoiceLineValidation, invoiceLine, invoice, declaration) = SetupData();

			Assert_BR3399(invoiceLineValidation, invoiceLine, invoice, declaration, invoiceLine.JI_FormattedProcedureInfo);
		}

		void Assert_BR3399(ImportJobComInvoiceLineValidation invoiceLineValidation, JobComInvoiceLine invoiceLine, JobComInvoiceHeader invoice, JobDeclaration declaration, ZPropertyInfo propertyInfo)
		{
			const string errorMessage = "[BR3399] When Declaration Type is 'H1', Additional Procedure 'F48' can only be entered if another Additional Procedure 'C07' is declared or Tariff is '3303001000' or '3303009000'.";

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_FormattedProcedure = "4000C01";
			var additionalProcedureCodes = invoiceLine.AdditionalProcedureCodes;
			var additionalCode1 = additionalProcedureCodes.AddNew();
			additionalCode1.CY_Code = ZString.Empty;
			var additionalCode2 = additionalProcedureCodes.AddNew();
			additionalCode2.CY_Code = ZString.Empty;
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
				additionalCode1.CY_Code = "4000F15";
				additionalCode2.CY_Code = "4000F48";
				invoiceLine.JI_Tariff = "3333333333";
				invoiceLineValidation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("Has error: CEI_Style = H1, JI_Tariff = '3333333333', additionalProcedureCodes [F15,F48]", propertyInfo, errorMessage);

				invoiceLine.JI_FormattedProcedure = "4000F48";
				additionalCode2.CY_Code = ZString.Empty;
				invoiceLineValidation.ValidateAdditionalProcedureCodesAsString();
				AssertHasMessageError("Has error: CEI_Style = H1, JI_Tariff = '3333333333', additionalProcedureCodes [F15,F48]", propertyInfo, errorMessage);

				invoiceLine.JI_FormattedProcedure = "4000C01";

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
				additionalCode1.CY_Code = ZString.Empty;
				additionalCode2.CY_Code = "4000F48";
				invoiceLine.JI_Tariff = "3303001000";
				invoiceLineValidation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("No error: CEI_Style = H1, JI_Tariff = '3303001000' additionalProcedureCode [F48]", propertyInfo, errorMessage);

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
				additionalCode1.CY_Code = ZString.Empty;
				additionalCode2.CY_Code = "4000F48";
				invoiceLine.JI_Tariff = "3303009000";
				invoiceLineValidation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("No error: CEI_Style = H1, JI_Tariff = '3303009000' additionalProcedureCode [F48]", propertyInfo, errorMessage);

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
				additionalCode1.CY_Code = "4000C07";
				additionalCode2.CY_Code = "4000F48";
				invoiceLine.JI_Tariff = "3333333333";
				invoiceLineValidation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("No error: CEI_Style = H1, JI_Tariff = '3333333333' additionalProcedureCode [C07,F48]", propertyInfo, errorMessage);

				invoiceLine.JI_FormattedProcedure = "4000C07";
				additionalCode1.CY_Code = ZString.Empty;
				invoiceLineValidation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("No error: CEI_Style = H1, JI_Tariff = '3333333333' additionalProcedureCode [C07,F48]", propertyInfo, errorMessage);

				invoiceLine.JI_FormattedProcedure = "4000C01";

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
				additionalCode1.CY_Code = ZString.Empty;
				additionalCode2.CY_Code = ZString.Empty;
				invoiceLine.JI_Tariff = "3333333333";
				invoiceLineValidation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("No error: CEI_Style = H1, JI_Tariff = '3333333333' additionalProcedureCode []", propertyInfo, errorMessage);

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H3;
				additionalCode1.CY_Code = ZString.Empty;
				additionalCode2.CY_Code = "4000F48";
				invoiceLine.JI_Tariff = "3333333333";
				invoiceLineValidation.ValidateAdditionalProcedureCodesAsString();
				AssertNoMessageError("No error: CEI_Style = H3, JI_Tariff = '3333333333', additionalProcedureCode [F48]", propertyInfo, errorMessage);
			});
		}

		public void TestCheckJI_PrimaryPreference_BR5158()
		{
			var (validation, invoiceLine, invoice, declaration) = SetupData();
			var instruction = invoiceLine.EntryInstruction;
			var supDocEntryInstruction = instruction.SupportingDocuments.AddNew();
			var supDocInvoiceHeader = invoice.SupportingDocuments.AddNew();
			var message = "[BR5158] A supporting document of type U110, U111, U112 is required on the entry instruction when there is at least an invoice line where preference starts with 3 and country of preferential origin is JP.";
			CombineAssertions(() =>
			{
				supDocInvoiceHeader.CSI_Code = "YYYY";
				supDocEntryInstruction.CSI_Code = "YYYY";
				invoiceLine.ZG_CountryOfSupply = "JP";
				invoiceLine.JI_PrimaryPreference = "300";
				AssertHasMessageError("Country - JP, Preference - 300, No U110, U111, U112 Supporting Document", invoiceLine.JI_PrimaryPreferenceInfo, message);

				invoiceLine.ZG_CountryOfSupply = "IE";
				validation.ValidateJI_PrimaryPreference();
				AssertNoMessageError("Country - IE, Preference - 300, No U110, U111, U112 Supporting Document", invoiceLine.JI_PrimaryPreferenceInfo, message);

				invoiceLine.ZG_CountryOfSupply = "JP";
				invoiceLine.JI_PrimaryPreference = "200";
				AssertNoMessageError("Country - JP, Preference - 200, No U110, U111, U112 Supporting Document", invoiceLine.JI_PrimaryPreferenceInfo, message);

				invoiceLine.JI_PrimaryPreference = "300";
				foreach (var code in new[] { Constants.SupportingDocumentCodes._U110, Constants.SupportingDocumentCodes._U111, Constants.SupportingDocumentCodes._U112 })
				{
					supDocInvoiceHeader.CSI_Code = "YYYY";
					supDocEntryInstruction.CSI_Code = code;
					validation.ValidateJI_PrimaryPreference();
					AssertNoMessageError($"Country - JP, Preference - 300, EntryInstruction has {code} Supporting Document", invoiceLine.JI_PrimaryPreferenceInfo, message);

					supDocEntryInstruction.CSI_Code = "YYYY";
					supDocInvoiceHeader.CSI_Code = code;
					validation.ValidateJI_PrimaryPreference();
					AssertNoMessageError($"Country - JP, Preference - 300, InvoiceHeader has {code} Supporting Document", invoiceLine.JI_PrimaryPreferenceInfo, message);
				}
			});
		}

		public void TestCountryOfOriginForCD5151()
		{
			var (validation, invoiceLine, invoice, declaration) = SetupData();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			instruction.CEI_Style = "I1";
			invoiceLine.JI_PrimaryPreference = "475";
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();

			var countryOfSupplyRequiredForPreferenceMsgError = "[CD5151] Country of Origin is required when the first digit of Preference is '1', '4' or '5' and is not equal to Pref. Orig.";
			AssertHasMessageError("CEI_Style = 'I1' and PrimaryPreference starts with 4", invoiceLine.JI_CountryOfOriginInfo, countryOfSupplyRequiredForPreferenceMsgError);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Netherlands;
			AssertNoMessageError(invoiceLine.JI_CountryOfOriginInfo, countryOfSupplyRequiredForPreferenceMsgError);

			var originNotEqualSupplyMessageError = "[CD5151] Country of Origin must be equal to Pref. Orig. for Import and Declaration Type of 'I1'.";
			invoiceLine.JI_PrimaryPreference = "2B";
			invoiceLine.ZG_CountryOfSupply = Core.Constants.CountryCodes.Sweden;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertHasMessageError("CEI_Style = 'I1' and Country of Supply does not equal Country of Origin", invoiceLine.JI_CountryOfOriginInfo, originNotEqualSupplyMessageError);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Sweden;
			AssertNoMessageError(invoiceLine.JI_CountryOfOriginInfo, originNotEqualSupplyMessageError);
		}

		public void TestCheckJI_CountryOfOrigin_CD0103_H1() => AssertCheckJI_CountryOfOrigin_CD0103(ImportDeclarationTypeList.Codes.H1);

		public void TestCheckJI_CountryOfOrigin_CD0103_H2() => AssertCheckJI_CountryOfOrigin_CD0103(ImportDeclarationTypeList.Codes.H2);

		public void TestCheckJI_CountryOfOrigin_CD0103_H3() => AssertCheckJI_CountryOfOrigin_CD0103(ImportDeclarationTypeList.Codes.H3);

		public void TestCheckJI_CountryOfOrigin_CD0103_H4() => AssertCheckJI_CountryOfOrigin_CD0103(ImportDeclarationTypeList.Codes.H4);

		public void TestCheckJI_CountryOfOrigin_CD0103_H5() => AssertCheckJI_CountryOfOrigin_CD0103(ImportDeclarationTypeList.Codes.H5);

		public void TestCheckJI_CountryOfOrigin_CD0103_I1() => AssertCheckJI_CountryOfOrigin_CD0103(ImportDeclarationTypeList.Codes.I1);

		void AssertCheckJI_CountryOfOrigin_CD0103(string declarationType)
		{
			var (validation, invoiceLine, invoice, declaration) = SetupData();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var countryOfOriginInfo = invoiceLine.JI_CountryOfOriginInfo;
			CombineAssertions(declarationType, () =>
			{
				AssertNoMessageError("No Declaration Type, No error", countryOfOriginInfo, Message_CD0103);
				instruction.CEI_Style = declarationType;
				invoiceLine.JI_CountryOfOrigin = "";
				AssertHasMessageError("Declaration Type, Has error", countryOfOriginInfo, Message_CD0103);
				invoiceLine.JI_CountryOfOrigin = "IE";
				AssertNoMessageError("Declaration Type, No error", countryOfOriginInfo, Message_CD0103);
			});
		}

		public void TestCheckJI_CountryOfOrigin()
		{
			var errorMessage = "For countries within the European Union the Country of Origin must be code 'EU'";
			(_, var invoiceLine, _, _) = SetupData();

			var myCountry = Factory.NewWithValidTestData<RefCountry>();
			myCountry.RN_Code = "~!";
			myCountry.RN_EconomicGrouping = "";
			AssertEquals("Pre-requisite - myCountry is not in EU", false, myCountry.IsPartOfEuropeanUnion);
			invoiceLine.JI_CountryOfOrigin = myCountry.RN_Code;
			AssertNoMessageError("No message error when JI_CountryOfOrigin is a non EU Country Code", invoiceLine.JI_CountryOfOriginInfo, errorMessage);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.EuropeanUnion;
			AssertNoMessageError("No message error when JI_CountryOfOrigin is EU", invoiceLine.JI_CountryOfOriginInfo, errorMessage);

			myCountry.RN_EconomicGrouping = "EUN";
			AssertEquals("Pre-requisite - myCountry is changed to be in EU", true, myCountry.IsPartOfEuropeanUnion);
			invoiceLine.JI_CountryOfOrigin = myCountry.RN_Code;
			AssertHasMessageError("Has message error when JI_CountryOfOrigin is an EU Country Code", invoiceLine.JI_CountryOfOriginInfo, errorMessage);
		}

		public void TestCheckJI_CountryOfOrigin_BR5151()
		{
			var (validation, invoiceLine, invoice, declaration) = SetupData();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H5;
			invoiceLine.JI_CountryOfOrigin = string.Empty;

			CombineAssertions(() =>
			{
				AssertHasMessageError("CEI_Style is 'H5' and JI_CountryOfOrigin is empty", invoiceLine.JI_CountryOfOriginInfo, Message_BR5151);

				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
				AssertHasMessageError("CEI_Style is 'H5' and JI_CountryOfOrigin is non-EU country", invoiceLine.JI_CountryOfOriginInfo, Message_BR5151);

				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
				AssertNoMessageError("CEI_Style is 'H5' and JI_CountryOfOrigin is 'FR'", invoiceLine.JI_CountryOfOriginInfo, Message_BR5151);

				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.EuropeanUnion;
				AssertNoMessageError("CEI_Style is 'H5' and JI_CountryOfOrigin is 'EU'", invoiceLine.JI_CountryOfOriginInfo, Message_BR5151);

				instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
				AssertNoMessageError("CEI_Style is 'H1'", invoiceLine.JI_CountryOfOriginInfo, Message_BR5151);
			});
		}

		public void TestCheckJI_CountryOfOrigin_BR5152()
		{
			var (validation, invoiceLine, invoice, declaration) = SetupData();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			CombineAssertions(() =>
			{
				foreach (var preference in new string[] { "400", "410", "415", "418", "420", "423", "425", "428", "440", "450" })
				{
					invoiceLine.JI_PrimaryPreference = preference;
					invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Turkey;

					AssertHasMessageError($"Preference {preference}: no supporting document found.", invoiceLine.JI_CountryOfOriginInfo, Message_BR5152);

					var supportingDocEntryInstruction = instruction.SupportingDocuments.AddNew();
					supportingDocEntryInstruction.CSI_Code = Constants.SupportingDocumentCodes._N018;
					invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Turkey;
					AssertNoMessageError($"Preference {preference}: supporting document found on entry instruction.", invoiceLine.JI_CountryOfOriginInfo, Message_BR5152);

					supportingDocEntryInstruction.CSI_Code = "YYYY";
					var supportingDocInvoiceHeader = invoiceLine.InvoiceHeader.SupportingDocuments.AddNew();
					supportingDocInvoiceHeader.CSI_Code = Constants.SupportingDocumentCodes._N018;
					invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Turkey;
					AssertNoMessageError($"Preference {preference}: supporting document found on invoice header.", invoiceLine.JI_CountryOfOriginInfo, Message_BR5152);

					supportingDocInvoiceHeader.CSI_Code = "YYYY";
					invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Ireland;
					AssertNoMessageError($"Preference {preference}: no supporting document found and Country of origin not 'TR'.", invoiceLine.JI_CountryOfOriginInfo, Message_BR5152);

					supportingDocEntryInstruction = instruction.SupportingDocuments.AddNew();
					supportingDocEntryInstruction.CSI_Code = Constants.SupportingDocumentCodes._N018;
					invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Ireland;
					AssertNoMessageError($"Preference {preference}: supporting document found on entry instruction and Country of origin not 'TR'.", invoiceLine.JI_CountryOfOriginInfo, Message_BR5152);

					supportingDocEntryInstruction.CSI_Code = "YYYY";
					supportingDocInvoiceHeader = invoiceLine.InvoiceHeader.SupportingDocuments.AddNew();
					supportingDocInvoiceHeader.CSI_Code = Constants.SupportingDocumentCodes._N018;
					invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Ireland;
					AssertNoMessageError($"Preference {preference}: supporting document found on invoice header and Country of origin not 'TR'.", invoiceLine.JI_CountryOfOriginInfo, Message_BR5152);
					supportingDocInvoiceHeader.CSI_Code = "YYYY";
				}

				invoiceLine.JI_PrimaryPreference = "XXX";
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Turkey;
				AssertNoMessageError($"Preference not in the list: supporting document found on invoice header.", invoiceLine.JI_CountryOfOriginInfo, Message_BR5152);
			});
		}

		const string Message_BR5151 = "[BR5151] Country of Origin must be 'EU' when Declaration is H5.";
		const string Message_BR5152 = "A Supporting document of type N018 is required on the entry instruction, invoice header when there is at least an invoice line where preference is one of 400, 410, 415, 418, 420, 423, 425, 428, 440, 450, and country of origin is TR.";
		const string Message_CD0103 = "[CD0103] Country of Origin is required when Declaration is H1, H2, H3, H4, H5, or I1";

		public void TestCheckJI_Description()
		{
			string message = "Goods description can have up to 512 alpha numeric characters.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			{
				invoiceLine.JI_Description = new ZString('A', 513);
				AssertHasWarning("UCC5, 513 characters", invoiceLine.JI_DescriptionInfo, message);
				invoiceLine.JI_Description = new ZString('A', 512);
				AssertNoWarning("UCC5, 512 characters", invoiceLine.JI_DescriptionInfo, message);
			}
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				invoiceLine.JI_Description = new ZString('A', 513);
				AssertNoWarning("UCC6, 513 characters", invoiceLine.JI_DescriptionInfo, message);
			}
		}
	}
}
