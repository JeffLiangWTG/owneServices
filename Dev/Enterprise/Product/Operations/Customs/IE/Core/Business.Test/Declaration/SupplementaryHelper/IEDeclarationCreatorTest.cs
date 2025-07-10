using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration.SupplementaryHelper;
using Moq;

namespace Enterprise.Customs.IE.Business.Declaration.SupplementaryHelper.Testing
{
	sealed class IEDeclarationCreatorTest : TestCaseWithFactory
	{
		public void TestGetDeclarationCreator()
		{
			AssertEquals(typeof(IEDeclarationCreator), declarationCreator.GetType());
		}

		public void TestRelatedDeclaration()
		{
			declarationCreator.RelatedDeclaration();

			AssertEquals("RelatedDeclarations.Count", 3, declaration.RelatedDeclarations.Count);

			var dec1 = declaration.RelatedDeclarations[0];
			var dec2 = declaration.RelatedDeclarations[1];
			var dec3 = declaration.RelatedDeclarations[2];
			AssertEquals("New declaration 1 CustomsEntryInstructions.Count", 1, dec1.CustomsEntryInstructions.Count);
			AssertEquals("New declaration 2 CustomsEntryInstructions.Count", 1, dec2.CustomsEntryInstructions.Count);
			AssertEquals("New declaration 3 CustomsEntryInstructions.Count", 1, dec3.CustomsEntryInstructions.Count);
			AssertEquals("New declaration 1 EntryInstruction Style", "H3", dec1.CustomsEntryInstructions[0].CEI_Style);
			AssertEquals("New declaration 2 EntryInstruction Style", "H4", dec2.CustomsEntryInstructions[0].CEI_Style);
			AssertEquals("New declaration 3 EntryInstruction Style", "H1", dec3.CustomsEntryInstructions[0].CEI_Style);

			var docs1 = ((CusEntryInstruction)dec1.CustomsEntryInstructions[0]).PreviousDocuments;
			var docs2 = ((CusEntryInstruction)dec2.CustomsEntryInstructions[0]).PreviousDocuments;
			var docs3 = ((CusEntryInstruction)dec3.CustomsEntryInstructions[0]).PreviousDocuments;
			AssertHasPreviousDocument("New declaration 1 EntryInstruction", docs1, "MRN0001");
			AssertHasPreviousDocument("New declaration 2 EntryInstruction", docs2, "MRN0002");
			AssertHasPreviousDocument("New declaration 3 EntryInstruction", docs3, "MRN0004");

			AssertEquals("New declaration 1 Invoices.Count", 1, dec1.Invoices.Count);
			AssertEquals("New declaration 1 Invoice 1 is original 1", "Header1", dec1.Invoices[0].JZ_Remarks);
			AssertEquals("New declaration 1 Invoice 1 Lines.Count", 1, dec1.Invoices[0].InvoiceLines.Count);
			AssertEquals("New declaration 1 Invoice 1 Line 1 linked to Instruction", dec1.CustomsEntryInstructions[0].PK, dec1.Invoices[0].InvoiceLines[0].JI_CEI);

			AssertEquals("New declaration 2 Invoices.Count", 2, dec2.Invoices.Count);
			AssertEquals("New declaration 2 Invoice 1 is original 1", "Header1", dec2.Invoices[0].JZ_Remarks);
			AssertEquals("New declaration 2 Invoice 2 is original 2", "Header2", dec2.Invoices[1].JZ_Remarks);
			AssertEquals("New declaration 2 Invoice 1 Lines.Count", 1, dec2.Invoices[0].InvoiceLines.Count);
			AssertEquals("New declaration 2 Invoice 2 Lines.Count", 1, dec2.Invoices[1].InvoiceLines.Count);
			AssertEquals("New declaration 2 Invoice 1 Line 1 linked to Instruction", dec2.CustomsEntryInstructions[0].PK, dec2.Invoices[0].InvoiceLines[0].JI_CEI);
			AssertEquals("New declaration 2 Invoice 2 Line 1 linked to Instruction", dec2.CustomsEntryInstructions[0].PK, dec2.Invoices[1].InvoiceLines[0].JI_CEI);

			AssertEquals("New declaration 3 Invoices.Count", 1, dec3.Invoices.Count);
			AssertEquals("New declaration 3 Invoice 1 is original 3", "Header3", dec3.Invoices[0].JZ_Remarks);
			AssertEquals("New declaration 3 Invoice 1 Lines.Count", 1, dec3.Invoices[0].InvoiceLines.Count);
			AssertEquals("New declaration 3 Invoice 1 Line 1 linked to Instruction", dec3.CustomsEntryInstructions[0].PK, dec3.Invoices[0].InvoiceLines[0].JI_CEI);
		}

		public void TestNewInstruction()
		{
			declarationCreator.NewInstruction();

			AssertEquals("CustomsEntryInstructions.Count", 7, declaration.CustomsEntryInstructions.Count);

			var oldInstruction1 = declaration.CustomsEntryInstructions[0];
			var oldInstruction2 = declaration.CustomsEntryInstructions[1];
			var oldInstruction3 = declaration.CustomsEntryInstructions[2];
			var oldInstruction4 = declaration.CustomsEntryInstructions[3];
			var newInstruction1 = declaration.CustomsEntryInstructions[4];
			var newInstruction2 = declaration.CustomsEntryInstructions[5];
			var newInstruction4 = declaration.CustomsEntryInstructions[6];

			AssertEquals("New EntryInstruction 1 Style", "H3", newInstruction1.CEI_Style);
			AssertEquals("New EntryInstruction 2 Style", "H4", newInstruction2.CEI_Style);
			AssertEquals("New EntryInstruction 4 Style", "H1", newInstruction4.CEI_Style);

			var docs1 = newInstruction1.PreviousDocuments;
			var docs2 = newInstruction2.PreviousDocuments;
			var docs4 = newInstruction4.PreviousDocuments;
			AssertHasPreviousDocument("New EntryInstruction 1", docs1, "MRN0001");
			AssertHasPreviousDocument("New EntryInstruction 2", docs2, "MRN0002");
			AssertHasPreviousDocument("New EntryInstruction 4", docs4, "MRN0004");

			var invHeader1 = declaration.Invoices[0];
			var invHeader2 = declaration.Invoices[1];
			var invHeader3 = declaration.Invoices[2];

			AssertEquals("Invoice 1 Lines Count", 4, invHeader1.InvoiceLines.Count);
			AssertEquals("Invoice 2 Lines Count", 3, invHeader2.InvoiceLines.Count);
			AssertEquals("Invoice 3 Lines Count", 3, invHeader3.InvoiceLines.Count);

			AssertEquals("Invoice 1 Line 1 linked to old EntryInstruction 1", oldInstruction1.PK, invHeader1.InvoiceLines[0].JI_CEI);
			AssertEquals("Invoice 1 Line 2 linked to old EntryInstruction 2", oldInstruction2.PK, invHeader1.InvoiceLines[1].JI_CEI);
			AssertEquals("Invoice 1 Line 3 linked to new EntryInstruction 1", newInstruction1.PK, invHeader1.InvoiceLines[2].JI_CEI);
			AssertEquals("Invoice 1 Line 4 linked to new EntryInstruction 2", newInstruction2.PK, invHeader1.InvoiceLines[3].JI_CEI);

			AssertEquals("Invoice 2 Line 1 linked to old EntryInstruction 2", oldInstruction2.PK, invHeader2.InvoiceLines[0].JI_CEI);
			AssertEquals("Invoice 2 Line 2 linked to old EntryInstruction 3", oldInstruction3.PK, invHeader2.InvoiceLines[1].JI_CEI);
			AssertEquals("Invoice 2 Line 3 linked to new EntryInstruction 2", newInstruction2.PK, invHeader2.InvoiceLines[2].JI_CEI);

			AssertEquals("Invoice 3 Line 1 linked to old EntryInstruction 3", oldInstruction3.PK, invHeader3.InvoiceLines[0].JI_CEI);
			AssertEquals("Invoice 3 Line 2 linked to old EntryInstruction 4", oldInstruction4.PK, invHeader3.InvoiceLines[1].JI_CEI);
			AssertEquals("Invoice 3 Line 3 linked to new EntryInstruction 4", newInstruction4.PK, invHeader3.InvoiceLines[2].JI_CEI);
		}

		public void TestReuseInstruction()
		{
			foreach (var header in declaration.CustomsEntryHeaders.Cast<CusEntryHeader>())
			{
				header.Messages.AddNew();
			}
			declarationCreator.ReuseInstruction();

			AssertEquals("CustomsEntryInstructions.Count", 4, declaration.CustomsEntryInstructions.Count);
			AssertEquals("CustomsEntryHeaders.Count", 4, declaration.CustomsEntryHeaders.Count);

			var entry1 = declaration.CustomsEntryHeaders.Single(x => x.CH_CEI_Instruction == declaration.CustomsEntryInstructions[0].PK);
			var entry2 = declaration.CustomsEntryHeaders.Single(x => x.CH_CEI_Instruction == declaration.CustomsEntryInstructions[1].PK);
			var entry3 = declaration.CustomsEntryHeaders.Single(x => x.CH_CEI_Instruction == declaration.CustomsEntryInstructions[2].PK);
			var entry4 = declaration.CustomsEntryHeaders.Single(x => x.CH_CEI_Instruction == declaration.CustomsEntryInstructions[3].PK);

			AssertEquals("EntryInstruction 1 Style", "H3", declaration.CustomsEntryInstructions[0].CEI_Style);
			AssertEquals("EntryInstruction 2 Style", "H4", declaration.CustomsEntryInstructions[1].CEI_Style);
			AssertEquals("EntryInstruction 3 Style - unchanged", "EI3", declaration.CustomsEntryInstructions[2].CEI_Style);
			AssertEquals("EntryInstruction 4 Style", "H1", declaration.CustomsEntryInstructions[3].CEI_Style);

			var docs1 = declaration.CustomsEntryInstructions[0].PreviousDocuments;
			var docs2 = declaration.CustomsEntryInstructions[1].PreviousDocuments;
			var docs3 = declaration.CustomsEntryInstructions[2].PreviousDocuments;
			var docs4 = declaration.CustomsEntryInstructions[3].PreviousDocuments;
			AssertHasPreviousDocument("EntryInstruction 1", docs1, "MRN0001");
			AssertHasPreviousDocument("EntryInstruction 2", docs2, "MRN0002");
			AssertEquals("EntryInstruction 3 Previous Documents Count", 0, docs3.Count);
			AssertHasPreviousDocument("EntryInstruction 4", docs4, "MRN0004");

			AssertEquals("EntryHeader 1.CH_EntryStatus", ZString.Empty, entry1.CH_EntryStatus);
			AssertEquals("EntryHeader 2.CH_EntryStatus", ZString.Empty, entry2.CH_EntryStatus);
			AssertEquals("EntryHeader 3.CH_EntryStatus - not changed", (ZString)EntryStatusList.Codes.Clear, entry3.CH_EntryStatus);
			AssertEquals("EntryHeader 4.CH_EntryStatus", ZString.Empty, entry4.CH_EntryStatus);

			AssertEquals("EntryHeader 1 EntryLines Count", 0, entry1.MergedLines.Count);
			AssertEquals("EntryHeader 1 Messages Count", 0, entry1.Messages.Count);
			AssertEquals("EntryHeader 1 MRN", ZString.Empty, entry1.MovementReferenceNumber);
			AssertEquals("EntryHeader 2 EntryLines Count", 0, entry2.MergedLines.Count);
			AssertEquals("EntryHeader 2 Messages Count", 0, entry2.Messages.Count);
			AssertEquals("EntryHeader 2 MRN", ZString.Empty, entry2.MovementReferenceNumber);
			AssertEquals("EntryHeader 3 EntryLines Count - not changed", 1, entry3.MergedLines.Count);
			AssertEquals("EntryHeader 3 Messages Count - not changed", 1, entry3.Messages.Count);
			AssertEquals("EntryHeader 3 MRN - not changed", "MRN0003", entry3.MovementReferenceNumber);
			AssertEquals("EntryHeader 4 EntryLines Count", 0, entry4.MergedLines.Count);
			AssertEquals("EntryHeader 4 Messages Count", 0, entry4.Messages.Count);
			AssertEquals("EntryHeader 4 MRN", ZString.Empty, entry4.MovementReferenceNumber);

			AssertEquals("Declaration Message Count", 3, declaration.Messages.Count);
		}

		void AssertHasPreviousDocument(string message, PreviousDocumentCollection documents, ZString expectedReference)
		{
			AssertEquals(message + " Previous Documents Count", 1, documents.Count);
			AssertEquals(message + " Previous Document Code", "MRN", documents[0].CSI_Code);
			AssertEquals(message + " Previous Document ReferenceNumber", expectedReference, documents[0].CSI_ReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			declaration.JE_UCR = "UCR0123";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var ei1 = declaration.CustomsEntryInstructions.AddNew();
			ei1.CEI_Style = "EI1";
			var ei2 = declaration.CustomsEntryInstructions.AddNew();
			ei2.CEI_Style = "EI2";
			var ei3 = declaration.CustomsEntryInstructions.AddNew();
			ei3.CEI_Style = "EI3";
			var ei4 = declaration.CustomsEntryInstructions.AddNew();
			ei4.CEI_Style = "EI4";

			var invHeader1 = declaration.Invoices.AddNew();
			invHeader1.JZ_Remarks = "Header1";
			var invLine11 = invHeader1.InvoiceLines.AddNew();
			var invLine12 = invHeader1.InvoiceLines.AddNew();
			invLine11.JI_InvoiceQuantity = 11;
			invLine12.JI_InvoiceQuantity = 12;
			invLine11.JI_CEI = ei1.PK;
			invLine11.JI_Procedure = "53";
			invLine12.JI_CEI = ei2.PK;
			invLine12.JI_Procedure = "51";

			var invHeader2 = declaration.Invoices.AddNew();
			invHeader2.JZ_Remarks = "Header2";
			var invLine21 = invHeader2.InvoiceLines.AddNew();
			var invLine22 = invHeader2.InvoiceLines.AddNew();
			invLine21.JI_InvoiceQuantity = 21;
			invLine22.JI_InvoiceQuantity = 22;
			invLine21.JI_CEI = ei2.PK;
			invLine22.JI_CEI = ei3.PK;
			invLine22.JI_Procedure = "01";

			var invHeader3 = declaration.Invoices.AddNew();
			invHeader3.JZ_Remarks = "Header3";
			var invLine31 = invHeader3.InvoiceLines.AddNew();
			var invLine32 = invHeader3.InvoiceLines.AddNew();
			invLine31.JI_InvoiceQuantity = 31;
			invLine32.JI_InvoiceQuantity = 32;
			invLine31.JI_CEI = ei3.PK;
			invLine32.JI_CEI = ei4.PK;
			invLine32.JI_Procedure = "01";

			declaration.DoMerge();
			var eh1 = declaration.CustomsEntryHeaders.Single(x => x.CH_CEI_Instruction == ei1.PK);
			var eh2 = declaration.CustomsEntryHeaders.Single(x => x.CH_CEI_Instruction == ei2.PK);
			var eh3 = declaration.CustomsEntryHeaders.Single(x => x.CH_CEI_Instruction == ei3.PK);
			var eh4 = declaration.CustomsEntryHeaders.Single(x => x.CH_CEI_Instruction == ei4.PK);

			eh1.CH_EntryStatus = EntryStatusList.Codes.Clear;
			eh1.MovementReferenceNumberSetter("MRN0001");
			eh2.CH_EntryStatus = EntryStatusList.Codes.Clear;
			eh2.MovementReferenceNumberSetter("MRN0002");
			eh3.CH_EntryStatus = EntryStatusList.Codes.Clear;
			eh3.MovementReferenceNumberSetter("MRN0003");
			eh4.CH_EntryStatus = EntryStatusList.Codes.Clear;
			eh4.MovementReferenceNumberSetter("MRN0004");

			var mockFilter = new Mock<IEntryHeaderFilter>();
			mockFilter.Setup(x => x.EntryHeaders).Returns(new CusEntryHeader[] { eh1, eh2, eh4 });

			declarationCreator = DeclarationCreator.GetDeclarationCreator(declaration, mockFilter.Object);
		}

		DeclarationCreator declarationCreator;
		JobDeclaration declaration;
	}
}
