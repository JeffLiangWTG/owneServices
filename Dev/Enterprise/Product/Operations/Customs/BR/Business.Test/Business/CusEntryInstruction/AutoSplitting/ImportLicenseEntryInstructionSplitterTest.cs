using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportLicenseEntryInstructionSplitterTest : TestCaseWithFactory
	{
		public void TestAutoSplit()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Description = "DESCRIPTION";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryLine = entry.MergedLines.AddNew();

			var invoice = declaration.Invoices.AddNew();
			for (var idx = 0; idx < CusEntryInstruction.MaximumInvoiceLinesAllowedForImportLicense; idx++)
			{
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.JI_Tariff = "1111111";
				invoiceLine.JI_CL = entryLine.PK;
			}

			var splitInstruction = new ImportLicenseEntryInstructionSplitter(instruction);
			AssertEquals(ImportLicenseEntryInstructionSplitter.NoNeedToSplitMessage, splitInstruction.CheckBeforeSplit());
			AssertEquals(false, splitInstruction.AutoSplit());

			var invoiceLineExceeded = invoice.JobComInvoiceLines.AddNew();
			invoiceLineExceeded.JI_CEI = instruction.PK;
			invoiceLineExceeded.JI_Tariff = "1111111";
			invoiceLineExceeded.JI_CL = entry.MergedLines.AddNew().PK;

			var invoiceLineOtherMergeKey = invoice.JobComInvoiceLines.AddNew();
			invoiceLineOtherMergeKey.JI_CEI = instruction.PK;
			invoiceLineOtherMergeKey.JI_Tariff = "1111111";
			invoiceLineOtherMergeKey.NaladiHs = "1";
			invoiceLineOtherMergeKey.JI_CL = entry.MergedLines.AddNew().PK;

			var invoiceLineWithDiffTariff = invoice.JobComInvoiceLines.AddNew();
			invoiceLineWithDiffTariff.JI_CEI = instruction.PK;
			invoiceLineWithDiffTariff.JI_Tariff = "2222222";
			invoiceLineWithDiffTariff.JI_CL = entry.MergedLines.AddNew().PK;

			splitInstruction = new ImportLicenseEntryInstructionSplitter(instruction);
			AssertEquals(4, splitInstruction.EstimatedSplitCount);
			AssertEquals(ZString.Empty, splitInstruction.CheckBeforeSplit());
			AssertEquals(true, splitInstruction.AutoSplit());
			AssertEquals(4, declaration.CustomsEntryInstructions.Count);

			var instruction1 = declaration.CustomsEntryInstructions[0];
			AssertEquals("DESCRIPTION", instruction1.CEI_Description);
			AssertEquals(CusEntryInstruction.MaximumInvoiceLinesAllowedForImportLicense, instruction1.InvoiceLines.Count());

			var instruction2 = declaration.CustomsEntryInstructions[1];
			AssertEquals("DESCRIPTION-2", instruction2.CEI_Description);
			AssertEquals(1, instruction2.InvoiceLines.Count());
			AssertEquals(invoiceLineExceeded.PK, instruction2.InvoiceLines.First().PK);

			var instruction3 = declaration.CustomsEntryInstructions[2];
			AssertEquals("DESCRIPTION-3", instruction3.CEI_Description);
			AssertEquals(1, instruction3.InvoiceLines.Count());
			AssertEquals(invoiceLineOtherMergeKey.PK, instruction3.InvoiceLines.First().PK);

			var instruction4 = declaration.CustomsEntryInstructions[3];
			AssertEquals("DESCRIPTION-4", instruction4.CEI_Description);
			AssertEquals(1, instruction4.InvoiceLines.Count());
			AssertEquals(invoiceLineWithDiffTariff.PK, instruction4.InvoiceLines.First().PK);
		}

		public void TestAutoSplitExceededSize()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Description = "12345678901234567890123456789012345678901234567890";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryLine = entry.MergedLines.AddNew();

			var invoice = declaration.Invoices.AddNew();
			for (var idx = 0; idx < CusEntryInstruction.MaximumInvoiceLinesAllowedForImportLicense; idx++)
			{
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.JI_Tariff = "1111111";
				invoiceLine.JI_CL = entryLine.PK;
			}

			var splitInstruction = new ImportLicenseEntryInstructionSplitter(instruction);
			AssertEquals(ImportLicenseEntryInstructionSplitter.NoNeedToSplitMessage, splitInstruction.CheckBeforeSplit());
			Assert(!splitInstruction.AutoSplit());
			Assert("HasParentEntryInstruction must be false", !instruction.HasParentEntryInstruction);

			var invoiceLineExceeded = invoice.JobComInvoiceLines.AddNew();
			invoiceLineExceeded.JI_CEI = instruction.PK;
			invoiceLineExceeded.JI_Tariff = "1111111";
			invoiceLineExceeded.JI_CL = entry.MergedLines.AddNew().PK;

			var invoiceLineWithDiffTariff = invoice.JobComInvoiceLines.AddNew();
			invoiceLineWithDiffTariff.JI_CEI = instruction.PK;
			invoiceLineWithDiffTariff.JI_Tariff = "2222222";
			invoiceLineWithDiffTariff.JI_CL = entry.MergedLines.AddNew().PK;

			splitInstruction = new ImportLicenseEntryInstructionSplitter(instruction);
			AssertEquals(3, splitInstruction.EstimatedSplitCount);
			Assert(splitInstruction.CheckBeforeSplit().IsEmpty);
			Assert(splitInstruction.AutoSplit());
			AssertEquals(3, declaration.CustomsEntryInstructions.Count);

			var instruction1 = declaration.CustomsEntryInstructions[0];
			AssertEquals("12345678901234567890123456789012345678901234567890", instruction1.CEI_Description);
			AssertEquals(CusEntryInstruction.MaximumInvoiceLinesAllowedForImportLicense, instruction1.InvoiceLines.Count());
			Assert("HasParentEntryInstruction must be false", !instruction1.HasParentEntryInstruction);

			var instruction2 = declaration.CustomsEntryInstructions[1];
			AssertEquals("123456789012345678901234567890123456789012345678-2", instruction2.CEI_Description);
			AssertEquals(1, instruction2.InvoiceLines.Count());
			AssertEquals(invoiceLineExceeded.PK, instruction2.InvoiceLines.First().PK);
			Assert("HasParentEntryInstruction must be false", instruction2.HasParentEntryInstruction);

			var instruction3 = declaration.CustomsEntryInstructions[2];
			AssertEquals("123456789012345678901234567890123456789012345678-3", instruction3.CEI_Description);
			AssertEquals(1, instruction3.InvoiceLines.Count());
			AssertEquals(invoiceLineWithDiffTariff.PK, instruction3.InvoiceLines.First().PK);
			Assert("HasParentEntryInstruction must be false", instruction3.HasParentEntryInstruction);
		}
	}
}
