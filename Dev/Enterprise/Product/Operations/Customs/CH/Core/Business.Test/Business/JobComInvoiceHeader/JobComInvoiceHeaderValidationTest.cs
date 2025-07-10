using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CH;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(JobComInvoiceHeaderValidation))]
sealed class JobComInvoiceHeaderValidationTest : Customs.Business.Testing.JobComInvoiceHeaderValidationTest
{
	public void TestCheckSpecialMentions()
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;

		CombineAssertions(() =>
		{
			SpecialMentionsValidationTestHelper.TestCheckSpecialMentions(InvoiceHeader.SpecialMentionsInfo);
		});
	}

	public void TestCheckSpecialMentionsTotal99Lines()
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;

		const string errorMessage = "Special Mentions overall may have a maximum of 99 lines.";

		Declaration.CustomsEntryInstructions.AddNew();
		InvoiceHeader.InvoiceLines.AddNew();

		var invoiceHeader2 = Declaration.Invoices.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();

		var eightyLines = new ZString(string.Join("\n", Enumerable.Range(1, 80).Select(n => $"Line {n}")));
		var tenLines = new ZString(string.Join("\n", Enumerable.Range(1, 10).Select(n => $"Line {n}")));

		CombineAssertions("Two Invoice Lines linked to one Entry Instruction", () =>
		{
			InvoiceHeader.SpecialMentions = eightyLines;
			invoiceHeader2.SpecialMentions = eightyLines;
			AssertHasMessageError("160 lines", InvoiceHeader.SpecialMentionsInfo, errorMessage);

			InvoiceHeader.SpecialMentions = tenLines;
			invoiceHeader2.SpecialMentions = tenLines;
			AssertNoMessageError("20 lines", InvoiceHeader.SpecialMentionsInfo, errorMessage);

			InvoiceHeader.SpecialMentions = tenLines;
			invoiceHeader2.SpecialMentions = eightyLines;
			AssertNoMessageError("Errors cleared for this invoice trigger test", InvoiceHeader.SpecialMentionsInfo, errorMessage);
			InvoiceHeader.SpecialMentions = eightyLines;
			AssertHasMessageError("This invoice should trigger validation", InvoiceHeader.SpecialMentionsInfo, errorMessage);

			InvoiceHeader.SpecialMentions = eightyLines;
			invoiceHeader2.SpecialMentions = tenLines;
			AssertNoMessageError("Errors cleared for other invoice trigger test", InvoiceHeader.SpecialMentionsInfo, errorMessage);
			invoiceHeader2.SpecialMentions = eightyLines;
			AssertHasMessageError("Other invoice should trigger validation", InvoiceHeader.SpecialMentionsInfo, errorMessage);

			InvoiceHeader.SpecialMentions = eightyLines;
			invoiceHeader2.SpecialMentions = eightyLines + eightyLines;
			AssertNoMessageError("No total limit error if single item is too large", InvoiceHeader.SpecialMentionsInfo, errorMessage);
		});

		CombineAssertions("Two Invoice Lines linked to two Entry Instructions", () =>
		{
			InvoiceHeader.SpecialMentions = eightyLines;
			invoiceHeader2.SpecialMentions = eightyLines;
			invoiceHeader2.Validation.ValidateSpecialMentions();
			InvoiceHeader.Validation.ValidateSpecialMentions();

			AssertHasMessageError("160 lines on one entry instruction", InvoiceHeader.SpecialMentionsInfo, errorMessage);
			AssertHasMessageError("160 lines on one entry instruction", invoiceHeader2.SpecialMentionsInfo, errorMessage);

			invoiceLine2.JI_CEI = Declaration.CustomsEntryInstructions.AddNew().PK;
			invoiceHeader2.Validation.ValidateSpecialMentions();
			InvoiceHeader.Validation.ValidateSpecialMentions();
			AssertNoMessageError("160 lines on two entry instructions", InvoiceHeader.SpecialMentionsInfo, errorMessage);
			AssertNoMessageError("160 lines on two entry instructions", invoiceHeader2.SpecialMentionsInfo, errorMessage);
		});
	}

	public void TestJZ_UCR_NS30130_OrdinaryDeclaraton() => CombineAssertions(() =>
	{
		const string errorMessageType = "[NS30130] Reference Number UCR must be provided either at Declaration or Invoice Header level.";

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		var ordinaryEntryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		ordinaryEntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		var simplifiedEntryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		simplifiedEntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		Declaration.JE_UCR = ZString.Empty;
		InvoiceHeader.JZ_UCR = ZString.Empty;
		InvoiceHeader.TransportDocuments.RemoveAndDeleteAll();
		var invoiceLine1 = InvoiceHeader.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = ordinaryEntryInstruction.PK;
		var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = ordinaryEntryInstruction.PK;

		InvoiceHeader.Validation.ValidateJZ_UCR();
		AssertHasMessageError($"JZ_UCR T1='{InvoiceHeader.JZ_UCR}', CEI_Style='{ordinaryEntryInstruction.CEI_Style}', TransportDocuments.Count='{InvoiceHeader.TransportDocuments.Count}', JZ_UCR=all empty", InvoiceHeader.JZ_UCRInfo, errorMessageType);

		invoiceLine2.JI_CEI = simplifiedEntryInstruction.PK;
		InvoiceHeader.Validation.ValidateJZ_UCR();
		AssertHasMessageError($"JZ_UCR T2='{InvoiceHeader.JZ_UCR}', CEI_Style='{ordinaryEntryInstruction.CEI_Style}', TransportDocuments.Count='{InvoiceHeader.TransportDocuments.Count}', JZ_UCR=all empty", InvoiceHeader.JZ_UCRInfo, errorMessageType);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		InvoiceHeader.Validation.ValidateJZ_UCR();
		AssertNoMessageError($"JZ_UCR T3=Import", InvoiceHeader.JZ_UCRInfo, errorMessageType);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		InvoiceHeader.Validation.ValidateJZ_UCR();
		AssertNoMessageError($"JZ_UCR T4=ExportDeclarationActivation", InvoiceHeader.JZ_UCRInfo, errorMessageType);
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;

		Declaration.JE_UCR = "TestDta";
		InvoiceHeader.Validation.ValidateJZ_UCR();
		AssertNoMessageError($"JE_UCR T5='{Declaration.JE_UCR}', CEI_Style='{ordinaryEntryInstruction.CEI_Style}', TransportDocuments.Count='{InvoiceHeader.TransportDocuments.Count}', JZ_UCR=all empty", InvoiceHeader.JZ_UCRInfo, errorMessageType);
		Declaration.JE_UCR = ZString.Empty;

		InvoiceHeader.JZ_UCR = "TestData";
		AssertNoMessageError($"JZ_UCR T6='{InvoiceHeader.JZ_UCR}', CEI_Style='{ordinaryEntryInstruction.CEI_Style}', TransportDocuments.Count='{InvoiceHeader.TransportDocuments.Count}', JZ_UCR={InvoiceHeader.JZ_UCR}", Declaration.JE_UCRInfo, errorMessageType);
		InvoiceHeader.JZ_UCR = ZString.Empty;

		InvoiceHeader.TransportDocuments.AddNew();
		InvoiceHeader.Validation.ValidateJZ_UCR();
		AssertNoMessageError($"JZ_UCR T7='{InvoiceHeader.JZ_UCR}', CEI_Style='{ordinaryEntryInstruction.CEI_Style}', TransportDocuments.Count='{InvoiceHeader.TransportDocuments.Count}', JZ_UCR=all empty", InvoiceHeader.JZ_UCRInfo, errorMessageType);
		InvoiceHeader.TransportDocuments.RemoveAndDeleteAll();

		ordinaryEntryInstruction.CEI_Style = ZString.Empty;
		InvoiceHeader.Validation.ValidateJZ_UCR();
		AssertNoMessageError($"JZ_UCR T8='{InvoiceHeader.JZ_UCR}', CEI_Style='{ordinaryEntryInstruction.CEI_Style}', TransportDocuments.Count='{InvoiceHeader.TransportDocuments.Count}', JZ_UCR=all empty", InvoiceHeader.JZ_UCRInfo, errorMessageType);
	});

	public void TestJZ_UCR_NS30130_SimplifiedDeclaration() => CombineAssertions(() =>
	{
		const string errorMessageType = "[NS30130] Reference Number UCR must be empty in simplified Declaration.";

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		var simplifiedEntryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		simplifiedEntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		var ordinaryEntryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		ordinaryEntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		var invoiceLine1 = InvoiceHeader.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = simplifiedEntryInstruction.PK;
		var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = simplifiedEntryInstruction.PK;
		InvoiceHeader.JZ_UCR = "TestData";

		AssertHasMessageError($"JZ_UCR T1='{InvoiceHeader.JZ_UCR}', CEI_Style='{simplifiedEntryInstruction.CEI_Style}'", InvoiceHeader.JZ_UCRInfo, errorMessageType);

		invoiceLine2.JI_CEI = ordinaryEntryInstruction.PK;
		InvoiceHeader.Validation.ValidateJZ_UCR();
		AssertHasMessageError($"JZ_UCR T2='{InvoiceHeader.JZ_UCR}', CEI_Style='{simplifiedEntryInstruction.CEI_Style}'", InvoiceHeader.JZ_UCRInfo, errorMessageType);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		InvoiceHeader.Validation.ValidateJZ_UCR();
		AssertNoMessageError($"JZ_UCR T3=Import", InvoiceHeader.JZ_UCRInfo, errorMessageType);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		InvoiceHeader.Validation.ValidateJZ_UCR();
		AssertNoMessageError($"JZ_UCR T4=ExportDeclarationActivation", InvoiceHeader.JZ_UCRInfo, errorMessageType);
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;

		simplifiedEntryInstruction.CEI_Style = ZString.Empty;
		InvoiceHeader.Validation.ValidateJZ_UCR();
		AssertNoMessageError($"JZ_UCR T5='{InvoiceHeader.JZ_UCR}', CEI_Style='{simplifiedEntryInstruction.CEI_Style}'", InvoiceHeader.JZ_UCRInfo, errorMessageType);
		simplifiedEntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;

		InvoiceHeader.JZ_UCR = ZString.Empty;
		AssertNoMessageError($"JZ_UCR T6='{InvoiceHeader.JZ_UCR}', CEI_Style='{simplifiedEntryInstruction.CEI_Style}'", InvoiceHeader.JZ_UCRInfo, errorMessageType);
	});

	public void TestCheckJZ_IncoTerm_Mandatory() => CombineAssertions(() =>
	{
		const string warningMessage = "Please enter an Incoterm.";
		const string messageError = "You have not entered an Incoterm.";

		AssertMandatory(CHJobMessageTypeList.Codes.Import, expectedWarning: true);
		AssertMandatory(CHJobMessageTypeList.Codes.Export, expectedMessageError: true);
		AssertMandatory(CHJobMessageTypeList.Codes.ExportDeclarationActivation, expectedWarning: true);

		void AssertMandatory(string messageType, bool expectedWarning = false, bool expectedMessageError = false)
		{
			var assertionMessage = $"JE_MessageType={messageType}";
			Declaration.JE_MessageType = messageType;
			InvoiceHeader.JZ_IncoTerm = ZString.Empty;
			if (expectedWarning)
			{
				AssertHasWarning(assertionMessage, InvoiceHeader.JZ_IncoTermInfo, warningMessage);
			}
			else
			{
				AssertNoWarning(assertionMessage, InvoiceHeader.JZ_IncoTermInfo, warningMessage);
			}
			if (expectedMessageError)
			{
				AssertHasMessageError(assertionMessage, InvoiceHeader.JZ_IncoTermInfo, messageError);
			}
			else
			{
				AssertNoMessageError(assertionMessage, InvoiceHeader.JZ_IncoTermInfo, messageError);
			}
		}
	});

	public void TestCheckJZ_Weight() => CombineAssertions(() =>
	{
		var invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_Weight = 100;
		invoiceLine1.JI_WeightUQ = "KG";
		var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_Weight = 100;
		invoiceLine2.JI_WeightUQ = "KG";

		InvoiceHeader.JZ_Weight = 0m;
		AssertNoWarnings("InvoiceHeader Gross Weight Zero", InvoiceHeader.JZ_WeightInfo);

		InvoiceHeader.JZ_WeightUQ = "KG";
		InvoiceHeader.JZ_Weight = 200m;
		AssertNoWarnings("InvoiceHeader Gross Weight correct", InvoiceHeader.JZ_WeightInfo);

		InvoiceHeader.JZ_WeightUQ = "T";
		InvoiceHeader.JZ_Weight = 0.2m;
		AssertNoWarnings("InvoiceHeader Gross Weight in T correct", InvoiceHeader.JZ_WeightInfo);

		InvoiceHeader.JZ_WeightUQ = "KG";
		InvoiceHeader.JZ_Weight = 300m;
		var warningMessage = $"The sum of Gross Weight {InvoiceHeader.TotalWeightInKG} KG in Invoice Lines does not match with the total gross weight of the invoice.";
		AssertEquals("Total Gross Weight in KG", 200m, InvoiceHeader.TotalWeightInKG);
		AssertHasWarning("InvoiceHeader Gross Weight incorrect", InvoiceHeader.JZ_WeightInfo, warningMessage);
	});

	public void TestCheckJZ_NetWeight() => CombineAssertions(() =>
	{
		var invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_NetWeight = 10;
		invoiceLine1.JI_NetWeightUQ = "KG";
		var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_NetWeight = 10;
		invoiceLine2.JI_NetWeightUQ = "KG";

		InvoiceHeader.JZ_NetWeight = 0m;
		AssertNoWarnings("InvoiceHeader Net Weight Zero", InvoiceHeader.JZ_NetWeightInfo);

		InvoiceHeader.JZ_NetWeightUQ = "KG";
		InvoiceHeader.JZ_NetWeight = 20m;
		AssertNoWarnings("InvoiceHeader Net Weight correct", InvoiceHeader.JZ_NetWeightInfo);

		InvoiceHeader.JZ_NetWeightUQ = "T";
		InvoiceHeader.JZ_NetWeight = 0.02m;
		AssertNoWarnings("InvoiceHeader Net Weight in T correct", InvoiceHeader.JZ_NetWeightInfo);

		InvoiceHeader.JZ_NetWeightUQ = "KG";
		InvoiceHeader.JZ_NetWeight = 30m;
		var warningMessage = $"The sum of Net Weight {InvoiceHeader.TotalNetWeightInKG} KG in Invoice Lines does not match with the total net weight of the invoice.";
		AssertEquals("Total Net Weight in KG", 20m, InvoiceHeader.TotalNetWeightInKG);
		AssertHasWarning("InvoiceHeader Net Weight incorrect", InvoiceHeader.JZ_NetWeightInfo, warningMessage);
	});

	public override void TestValidateAbsenceOfOFTOrONS()
	{
		InvoiceHeader.JZ_InvoiceAmount = 1000m;
		InvoiceHeader.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
		InvoiceHeader.RunPreSaveValidation();
		AssertNoNotifications(InvoiceHeader.JZ_Calc_CIFAmountInfo);
	}

	protected override Type GetTypeForTest() => typeof(JobComInvoiceHeaderValidation);

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	new JobDeclaration declaration;

	JobComInvoiceHeader InvoiceHeader => invoiceHeader ??= Declaration.Invoices.AddNew();
	new JobComInvoiceHeader invoiceHeader;
}
