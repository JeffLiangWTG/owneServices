using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class EntryCreationStrategyTest : TestCaseWithFactory
	{
		public void TestGetKeyForLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var provider = declaration.CustomsEntryInstructionProvider;
			var entryInstr1 = provider.CustomsEntryInstructions.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line1.JI_CEI = entryInstr1.PK;

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstr1.PK;

			var lineMerger = new LineMerger(declaration);
			var entryCreationStrategy = new EntryCreationStrategy(declaration);

			lineMerger.DoMerge();

			AssertEquals("Line from different Invoices, should not merge", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			invoice1.JZ_InvoiceNumber = "11111";
			invoice2.JZ_InvoiceNumber = "11111";
			lineMerger.DoMerge();

			AssertEquals("Diffrent invoices with same numbers, should not merge", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			var invoice1Line1PreviousDocument1 = CreatePreviousDocument(invoice1Line1, 1, "KG");
			var invoice1Line1PreviousDocument2 = CreatePreviousDocument(invoice1Line1, 2, "TN");
			var invoice1Line2 = invoice1.JobComInvoiceLines.AddNew();
			var invoice1Line2PreviousDocument1 = CreatePreviousDocument(invoice1Line2, 2, "TN");
			var invoice1Line2PreviousDocument2 = CreatePreviousDocument(invoice1Line2, 1, "KG");
			var invoice1Line2PreviousDocument3 = CreatePreviousDocument(invoice1Line2, 1, "KG");

			invoice1Line2PreviousDocument2.CSI_UnitOfQuantity = "KM";
			lineMerger.DoMerge();

			AssertEquals("Lines with different previous document data, should not merge ", 3, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			invoice1Line2PreviousDocument2.CSI_UnitOfQuantity = "KG";
			lineMerger.DoMerge();

			AssertEquals("Lines with same previous document data HashSet(CSI_Code-CSI_ReferenceNumber-CSI_ReferenceNumber2-CSI_UnitOfQuantity), should merge", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			AssertEquals("Entry Instruction in header", false, provider.IsNoEntryInstruction);
			Assert("Entry Instruction pk", entryCreationStrategy.GetKeyForLine(invoice1Line1).Contains(entryInstr1.PK));

			Assert("Invoice 1", entryCreationStrategy.GetKeyForLine(invoice1Line1).Contains(invoice1Line1.JI_JZ));
			Assert("Invoice 2", entryCreationStrategy.GetKeyForLine(invoiceLine2).Contains(invoiceLine2.JI_JZ));

			var entryManager = new EntryManager(declaration, entryCreationStrategy);
			var entryLine = entryManager.GetOrCreateEntryLine(invoice1Line1);
			AssertEquals(entryInstr1.PK, entryLine.Header.CH_CEI_Instruction);
		}

		static PreviousDocument CreatePreviousDocument(JobComInvoiceLine invoice1Line, int version, string unitOfQuantity)
		{
			var invoice1Line1PreviousDocument = invoice1Line.PreviousDocuments.AddNew();
			invoice1Line1PreviousDocument.CSI_Code = version.ToString();
			invoice1Line1PreviousDocument.CSI_ReferenceNumber = version.ToString();
			invoice1Line1PreviousDocument.CSI_ReferenceNumber2 = version.ToString();
			invoice1Line1PreviousDocument.CSI_UnitOfQuantity = unitOfQuantity;
			return invoice1Line1PreviousDocument;
		}
	}
}
