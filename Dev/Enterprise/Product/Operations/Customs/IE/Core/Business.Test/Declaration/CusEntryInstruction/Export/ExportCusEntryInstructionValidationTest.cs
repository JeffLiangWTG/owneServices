using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ExportCusEntryInstructionValidationTest : CommonExportCusEntryInstructionValidationTest<ExportCusEntryInstructionValidation>
	{
		public void TestCheckCEI_SubStyle()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.CEI_SubStyleInfo);
		}

		public void TestCheckCEI_SubStyle_MustHavePreviousDocumentWhenXOrY()
		{
			var invoice = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var messageError = "For Sub Style X or Y, please enter at least one Previous Document at Entry Instruction or Invoice Header level.";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
			{
				instruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
				AssertNoMessageError("A", instruction.CEI_SubStyleInfo, messageError);
				instruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
				AssertHasMessageError("Y", instruction.CEI_SubStyleInfo, messageError);
				var previousDocument = instruction.PreviousDocuments.AddNew();
				instruction.Validation.ValidateCEI_SubStyle();
				AssertNoMessageError("Y has instruction previous document", instruction.CEI_SubStyleInfo, messageError);
				previousDocument.Delete();
				previousDocument = invoice.PreviousDocuments.AddNew();
				instruction.Validation.ValidateCEI_SubStyle();
				AssertNoMessageError("Y has invoice previous document", instruction.CEI_SubStyleInfo, messageError);
				instruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
				AssertNoMessageError("X has invoice previous document", instruction.CEI_SubStyleInfo, messageError);
				previousDocument.Delete();
				instruction.Validation.ValidateCEI_SubStyle();
				AssertHasMessageError("X", instruction.CEI_SubStyleInfo, messageError);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, false))
			{
				instruction.Validation.ValidateCEI_SubStyle();
				instruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
				AssertNoMessageError("X", instruction.CEI_SubStyleInfo, messageError);
				instruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
				AssertNoMessageError("Y", instruction.CEI_SubStyleInfo, messageError);
			}
		}

		public void TestCheckCEI_SubStyle_NotAmendCheck()
		{
			var entry = jobDeclaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.MovementReferenceNumberSetter("MRN0000001");
			entry.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			instruction.CEI_SubStyle = "A";
			Factory.Save();

			instruction = NewFactory().Load<CusEntryInstruction>(instruction.PK);
			instruction.CEI_SubStyle = "B";
			AssertHasMessageErrorContaining("Amend check.", instruction.CEI_SubStyleInfo, CommonResStrings.ShouldNotAmendThisValue);
			instruction.ValidationModes = EU.Business.Declaration.ValidationModes.None;
			instruction.Validation.ValidateCEI_SubStyle();
			AssertNoMessageErrorContaining("Skip Amend check when ValidationModes is NONE.", instruction.CEI_SubStyleInfo, CommonResStrings.ShouldNotAmendThisValue);
			instruction.ValidationModes = EU.Business.Declaration.ValidationModes.Amendment;

			instruction.CEI_SubStyle = "A";
			AssertNoMessageErrorContaining("Amend check(changed back to same as in the ouggoing message, validation passes).", instruction.CEI_SubStyleInfo, CommonResStrings.ShouldNotAmendThisValue);
		}

		public void TestB1EntryInstructionShouldHaveCentralisedClearanceAuthorizations()
		{
			string rowMessageError = "Centralized clearance is indicated when Presentation Office Code is entered. In this case an appropriate authorization for centralized clearance should be entered (CCL).";

			instruction.CEI_Style = "B2";
			var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = "ACE";
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError("Presentation office not set", instruction, rowMessageError);

			jobDeclaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation, "IE000001");
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = "ACE";
			instruction.Validation.ValidateAll();
			AssertHasRowMessageError("Should show Row Message Error if no CCL AuthorizationUsage", instruction, rowMessageError);

			cusAuthorizationUsage.AGC_Code = "CCL";
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError("CCL AuthorizationUsage existed", instruction, rowMessageError);
		}

		public void TestCheckMultipleMergingKeyFields_DeliveryTerm()
		{
			string rowMessageError = "Entry Instruction 'CusEntryInstruction' is linked to invoices with different delivery terms (Incoterm, Incoterm Location, Incoterm Place Code).";
			var invoice1 = jobDeclaration.Invoices.AddNew();
			invoice1.JZ_IncoTerm = "FoB";
			invoice1.JZ_IncoTermPlace = "BoB'S PLACE";
			invoice1.ZG_AgreedPlaceCode = "HeRE";
			var invoice1Line = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line.JI_CEI = instruction.PK;
			var invoice2 = jobDeclaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = "CIF";
			invoice2.JZ_IncoTermPlace = "JOE'S PLACE";
			invoice2.ZG_AgreedPlaceCode = "THERE";
			var invoice2Line = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line.JI_CEI = instruction.PK;
			instruction.Validation.ValidateAll();
			AssertHasRowMessageError("When HasMultipleDeliveryTerms is true", instruction, rowMessageError);

			invoice2.Delete();
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError("When HasMultipleDeliveryTerms is false", instruction, rowMessageError);
		}

		public void TestCheckMultipleMergingKeyFields_Currency()
		{
			var rowMessageError = "Entry Instruction 'CusEntryInstruction' is linked to invoices with different currencies (Invoice Currency).";

			var invoice1 = jobDeclaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoice1Line = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line.JI_CEI = instruction.PK;
			var invoice2 = jobDeclaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var invoice2Line = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line.JI_CEI = instruction.PK;

			instruction.Validation.ValidateAll();
			AssertHasRowMessageError("When HasMultipleCurrencies is true", instruction, rowMessageError);

			invoice2.Delete();
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError("When HasMultipleCurrencies is false", instruction, rowMessageError);
		}

		public void TestCheckAdditionalInfo1D23Mandatory()
		{
			var rowMessageError = "An Additional Document with Kind = Additional Reference (REF) and Full Type = '1D23' is required.";
			instruction.CEI_Style = "B1";
			instruction.CEI_SubStyle = "A";
			instruction.Validation.ValidateAll();
			AssertHasRowMessageError("Should show Row Message Error if no 1D23 exists", instruction, rowMessageError);

			var instructionAddInfo = instruction.AdditionalInfos.AddNew();
			instructionAddInfo.CSI_SubType = "REF";
			instructionAddInfo.CSI_Code = "1D23";
			instructionAddInfo.CSI_ReferenceNumber = "202210111236";
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError("1D23 Exists under entry instruction", instruction, rowMessageError);

			instructionAddInfo.CSI_Code = "1D24";
			instruction.Validation.ValidateAll();
			AssertHasRowMessageError("Should show Row Message Error if no 1D23 exists", instruction, rowMessageError);

			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invLine = invoiceHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = instruction.PK;
			var invoiceHeaderAddInfo = invoiceHeader.AdditionalInfos.AddNew();
			invoiceHeaderAddInfo.CSI_SubType = "REF";
			invoiceHeaderAddInfo.CSI_Code = "1D23";
			invoiceHeaderAddInfo.CSI_ReferenceNumber = "202210111236";
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError("1D23 Exists under invoice header", instruction, rowMessageError);

			invoiceHeaderAddInfo.CSI_Code = "1D24";
			instruction.CEI_SubStyle = "Y";
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError("Instruction Sub-style in (Y, Z), 1D23 not required", instruction, rowMessageError);
		}

		protected override string MessageType => IEJobMessageTypeList.Codes.Export;

		protected override ExportCusEntryInstructionValidation GetValidation() => new ExportCusEntryInstructionValidation(instruction);
	}
}
