using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportLicenseEntryInstructionUnsplitterTest : TestCaseWithFactory
	{
		public void TestUnsplitEntryInstructions_WithoutCustomsMessage()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

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

			CombineAssertions(() =>
			{
				var splitInstruction = new ImportLicenseEntryInstructionSplitter(instruction);
				Assert("AutoSplit must return TRUE", splitInstruction.AutoSplit());
				AssertEquals("Precondition: split to 4 Entry Instructions", 4, declaration.CustomsEntryInstructions.Count);
			});

			invoiceLineWithDiffTariff.JI_Tariff = "1111111";

			var instructionWithDiffTariff = invoiceLineWithDiffTariff.EntryInstruction;
			var instructionOtherMergeKey = invoiceLineOtherMergeKey.EntryInstruction;
			var instructionLineExceeded = invoiceLineExceeded.EntryInstruction;

			var pivotDiffTariff = instructionWithDiffTariff.ParentEntryInstructionGenPivot;
			var pivotOtherMergeKey = instructionOtherMergeKey.ParentEntryInstructionGenPivot;
			var pivotLineExceeded = instructionLineExceeded.ParentEntryInstructionGenPivot;

			new ImportLicenseEntryInstructionUnsplitter(declaration).UnsplitEntryInstructions();

			CombineAssertions(() =>
			{
				AssertEquals("Merge keys changed, Entry Instruction unsplit", 1, declaration.CustomsEntryInstructions.Count);
				Assert("Entry Instruction should be Deleted", instructionWithDiffTariff.IsDeleted);
				Assert("Entry Instruction should be Deleted", instructionOtherMergeKey.IsDeleted);
				Assert("Entry Instruction should be Deleted", instructionLineExceeded.IsDeleted);

				Assert("pivotDiffTariff should be Deleted", pivotDiffTariff.IsDeleted);
				Assert("pivotOtherMergeKey should be Deleted", pivotOtherMergeKey.IsDeleted);
				Assert("pivotLineExceeded should be Deleted", pivotLineExceeded.IsDeleted);

				AssertEquals("JI_CEI reset", invoiceLineWithDiffTariff.JI_CEI, instruction.PK);
				AssertEquals("JI_CEI reset", invoiceLineOtherMergeKey.JI_CEI, instruction.PK);
			});
		}

		public void TestUnsplitEntryInstructions_WithCustomsMessage()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

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

			var invoiceLineExceeded = invoice.JobComInvoiceLines.AddNew();
			invoiceLineExceeded.JI_CEI = instruction.PK;
			invoiceLineExceeded.JI_Tariff = "1111111";
			invoiceLineExceeded.NaladiHs = "1";
			invoiceLineExceeded.JI_CL = entry.MergedLines.AddNew().PK;

			var invoiceLineWithDiffTariff = invoice.JobComInvoiceLines.AddNew();
			invoiceLineWithDiffTariff.JI_CEI = instruction.PK;
			invoiceLineWithDiffTariff.JI_Tariff = "2222222";
			invoiceLineWithDiffTariff.JI_CL = entry.MergedLines.AddNew().PK;

			CombineAssertions(() =>
			{
				var splitInstruction = new ImportLicenseEntryInstructionSplitter(instruction);
				Assert("AutoSplit must return TRUE", splitInstruction.AutoSplit());
				declaration.DoMerge();
				AssertEquals("Precondition: split to 3 Entry Instructions", 3, declaration.CustomsEntryInstructions.Count);
				AssertEquals("Precondition: split to 3 ActiveEntryHeaders", 3, declaration.ActiveEntryHeaders.Count);
			});

			var entryInstruction3 = declaration.CustomsEntryInstructions[2];
			var entryHeader3 = entryInstruction3.EntryHeader;
			var message = entryHeader3.Messages.AddNew();
			message.EM_MessageType = MessageTypeList.Codes.LIC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;

			var instructionWithDiffTariff = invoiceLineWithDiffTariff.EntryInstruction;
			var instructionLineExceeded = invoiceLineExceeded.EntryInstruction;

			var pivotDiffTariff = instructionWithDiffTariff.ParentEntryInstructionGenPivot;
			var pivotLineExceeded = instructionLineExceeded.ParentEntryInstructionGenPivot;

			invoiceLineExceeded.NaladiHs = ZString.Empty;
			invoiceLineWithDiffTariff.JI_Tariff = "1111111";

			CombineAssertions(() =>
			{
				new ImportLicenseEntryInstructionUnsplitter(declaration).UnsplitEntryInstructions();
				AssertEquals("Precondition: split to 2 Entry Instructions", 2, declaration.CustomsEntryInstructions.Count);
				Assert("pivotDiffTariff should be Deleted", pivotDiffTariff.IsDeleted);
				Assert("pivotLineExceeded should be Deleted", pivotLineExceeded.IsDeleted);

				AssertEquals("JI_CEI reset", invoiceLineExceeded.JI_CEI, instruction.PK);
				AssertEquals("JI_CEI reset", invoiceLineWithDiffTariff.JI_CEI, instruction.PK);
			});
		}

		public void TestHasEntryInstructionsNeedToUnsplit()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

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

			CombineAssertions(() =>
			{
				var splitInstruction = new ImportLicenseEntryInstructionSplitter(instruction);
				Assert("AutoSplit must return TRUE", splitInstruction.AutoSplit());
				declaration.DoMerge();

				AssertEquals("Precondition: split to 4 Entry Instructions", 4, declaration.CustomsEntryInstructions.Count);
			});

			declaration.DoMerge();

			Assert("No merge keys changed, NO need to unsplit", !new ImportLicenseEntryInstructionUnsplitter(declaration).HasEntryInstructionsNeedToUnsplit());

			invoiceLineExceeded.JI_Tariff = "3333333";
			invoiceLineExceeded.EntryInstruction.EntryHeader.EntryNumber = "TST_3";
			Assert("Merge keys changed but with EntryNumber, NO need to unsplit", !new ImportLicenseEntryInstructionUnsplitter(declaration).HasEntryInstructionsNeedToUnsplit());

			declaration.InvoiceLines[0].JI_Tariff = "2222222";
			invoiceLineWithDiffTariff.JI_LinePrice = 100m;
			Assert("No merge keys changed, NO need to unsplit", !new ImportLicenseEntryInstructionUnsplitter(declaration).HasEntryInstructionsNeedToUnsplit());

			invoiceLineWithDiffTariff.JI_Tariff = "1111111";
			invoiceLineOtherMergeKey.NaladiHs = "2";
			Assert("Merge keys changed, need to unsplit", new ImportLicenseEntryInstructionUnsplitter(declaration).HasEntryInstructionsNeedToUnsplit());
		}

		public void TestUnsplitEntryInstructions_WithoutImportLicenseNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Description = "DESCRIPTION";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_Tariff = "1111111";
			invoiceLine1.JI_CL = entry.MergedLines.AddNew().PK;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_Tariff = "2222222";
			invoiceLine2.JI_CL = entry.MergedLines.AddNew().PK;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = instruction.PK;
			invoiceLine3.JI_Tariff = "3333333";
			invoiceLine3.JI_CL = entry.MergedLines.AddNew().PK;

			CombineAssertions(() =>
			{
				var splitInstruction = new ImportLicenseEntryInstructionSplitter(instruction);
				Assert("AutoSplit must return TRUE", splitInstruction.AutoSplit());
				declaration.DoMerge();
				AssertEquals("Precondition: split to 3 Entry Instructions", 3, declaration.CustomsEntryInstructions.Count);
				AssertEquals("Precondition: split to 3 ActiveEntryHeaders", 3, declaration.ActiveEntryHeaders.Count);

				invoiceLine2.JI_Tariff = "1111111";

				new ImportLicenseEntryInstructionUnsplitter(declaration).UnsplitEntryInstructions();
				AssertEquals("Precondition: split to 1 Entry Instructions", 1, declaration.CustomsEntryInstructions.Count);
			});
		}

		public void TestUnsplitEntryInstructions_WithImportLicenseNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Description = "DESCRIPTION";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_Tariff = "1111111";
			invoiceLine1.JI_CL = entry.MergedLines.AddNew().PK;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_Tariff = "2222222";
			invoiceLine2.JI_CL = entry.MergedLines.AddNew().PK;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = instruction.PK;
			invoiceLine3.JI_Tariff = "3333333";
			invoiceLine3.JI_CL = entry.MergedLines.AddNew().PK;

			CombineAssertions(() =>
			{
				var splitInstruction = new ImportLicenseEntryInstructionSplitter(instruction);
				Assert("AutoSplit must return TRUE", splitInstruction.AutoSplit());
				declaration.DoMerge();
				AssertEquals("Precondition: split to 3 Entry Instructions", 3, declaration.CustomsEntryInstructions.Count);
				AssertEquals("Precondition: split to 3 ActiveEntryHeaders", 3, declaration.ActiveEntryHeaders.Count);

				invoiceLine2.JI_Tariff = "1111111";
				invoiceLine3.EntryInstruction.EntryHeader.EntryNumber = "TST_3";

				var pivotWithoutEntryNum = declaration.CustomsEntryInstructions.Where(x => x.HasParentEntryInstruction && x.EntryHeader.EntryNumber.IsEmpty).Select(s => s.ParentEntryInstructionGenPivot).FirstOrDefault();
				var pivotWithEntryNum = declaration.CustomsEntryInstructions.Where(x => x.HasParentEntryInstruction && !x.EntryHeader.EntryNumber.IsEmpty).Select(s => s.ParentEntryInstructionGenPivot).FirstOrDefault();

				new ImportLicenseEntryInstructionUnsplitter(declaration).UnsplitEntryInstructions();
				AssertEquals("Precondition: split to 1 Entry Instructions", 2, declaration.CustomsEntryInstructions.Count);
				Assert("Pivot Without EntryNum should be Deleted", pivotWithoutEntryNum.IsDeleted);
				Assert("Pivot With EntryNum should NOT be Deleted", !pivotWithEntryNum.IsDeleted);
			});
		}
	}
}
