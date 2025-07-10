using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.SupplementaryHelper;
using Enterprise.Customs.FR.Business.Declaration.SupplementaryHelper;
using Enterprise.Customs.FR.Registry;
using Enterprise.Environment;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class FRDeclarationCreatorTest : TestCaseWithFactory
	{
		public void TestGetDeclarationCreator()
		{
			AssertEquals(typeof(FRDeclarationCreator), declarationCreator.GetType());
		}

		public void TestNewRelatedDeclaration()
		{
			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true))
			{
				declarationCreator.RelatedDeclaration();

				AssertEquals("CH_EntryStatus of entryHeader4 is not REL, so the related declaration will not be generated", 3, declaration.RelatedDeclarations.Count);

				var dec1 = declaration.RelatedDeclarations.Cast<JobDeclaration>().FirstOrDefault(dec => dec.CustomsEntryInstructions[0].PreviousDocuments[0].CSI_ReferenceNumber == "MRN0001");
				var dec2 = declaration.RelatedDeclarations.Cast<JobDeclaration>().FirstOrDefault(dec => dec.CustomsEntryInstructions[0].PreviousDocuments[0].CSI_ReferenceNumber == "MRN0002");
				var dec3 = declaration.RelatedDeclarations.Cast<JobDeclaration>().FirstOrDefault(dec => dec.CustomsEntryInstructions[0].PreviousDocuments[0].CSI_ReferenceNumber == "MRN0003");

				AssertEquals("New declaration 1 UCR", "UCR0123", dec1.JE_UCR);
				AssertEquals("New declaration 2 UCR", "UCR0123", dec2.JE_UCR);
				AssertEquals("New declaration 3 UCR", "UCR0123", dec3.JE_UCR);

				AssertEquals("New declaration 1 CustomsEntryInstructions.Count", 1, dec1.CustomsEntryInstructions.Count);
				AssertEquals("New declaration 2 CustomsEntryInstructions.Count", 1, dec2.CustomsEntryInstructions.Count);
				AssertEquals("New declaration 3 CustomsEntryInstructions.Count", 1, dec3.CustomsEntryInstructions.Count);

				AssertEquals("New declaration 1 JE_DateOfFirstArrival", new ZDateTime(2025, 1, 1), dec1.JE_DateOfFirstArrival);
				AssertEquals("New declaration 2 JE_DateOfFirstArrival", new ZDateTime(2025, 1, 1), dec2.JE_DateOfFirstArrival);
				AssertEquals("New declaration 3 JE_DateOfFirstArrival", new ZDateTime(2025, 1, 1), dec3.JE_DateOfFirstArrival);

				AssertEquals("New declaration 1 JE_TotalNoOfPacks = 1", 1, dec1.JE_TotalNoOfPacks);
				AssertEquals("New declaration 2 JE_TotalNoOfPacks = 2+4", 6, dec2.JE_TotalNoOfPacks);
				AssertEquals("New declaration 3 JE_TotalNoOfPacks = 8+16", 24, dec3.JE_TotalNoOfPacks);

				AssertEquals("New declaration 1 Packages.Count", 1, dec1.Packages.Count);
				AssertEquals("New declaration 1 Package[0].CW_MarksAndNos", "AA1", dec1.Packages[0].CW_MarksAndNos);
				AssertEquals("New declaration 1 Package[0].CW_MarksAndNos", 1, dec1.Packages[0].CW_PackQty);

				AssertEquals("New declaration 2 Packages.Count", 2, dec2.Packages.Count);
				AssertEquals("New declaration 2 Package[0].CW_MarksAndNos", "AA1", dec2.Packages[0].CW_MarksAndNos);
				AssertEquals("New declaration 2 Package[0].CW_PackQty", 2, dec2.Packages[0].CW_PackQty);
				AssertEquals("New declaration 2 Package[1].CW_MarksAndNos", "AA3", dec2.Packages[1].CW_MarksAndNos);
				AssertEquals("New declaration 2 Package[1].CW_PackQty", 4, dec2.Packages[1].CW_PackQty);

				AssertEquals("New declaration 3 Packages.Count", 2, dec3.Packages.Count);
				AssertEquals("New declaration 3 Package[0].CW_MarksAndNos", "AA1", dec3.Packages[0].CW_MarksAndNos);
				AssertEquals("New declaration 3 Package[0].CW_PackQty", 8, dec3.Packages[0].CW_PackQty);
				AssertEquals("New declaration 3 Package[1].CW_MarksAndNos", "AA2", dec3.Packages[1].CW_MarksAndNos);
				AssertEquals("New declaration 3 Package[1].CW_PackQty", 16, dec3.Packages[1].CW_PackQty);

				AssertEquals("New declaration 1 EntryInstruction Style", "H6", dec1.CustomsEntryInstructions[0].CEI_Style);
				AssertEquals("New declaration 2 EntryInstruction Style", "H4", dec2.CustomsEntryInstructions[0].CEI_Style);
				AssertEquals("New declaration 3 EntryInstruction Style", "H5", dec3.CustomsEntryInstructions[0].CEI_Style);

				AssertEquals("New declaration 1 EntryInstruction SubStyle", "Y", dec1.CustomsEntryInstructions[0].CEI_SubStyle);
				AssertEquals("New declaration 2 EntryInstruction SubStyle", "Y", dec2.CustomsEntryInstructions[0].CEI_SubStyle);
				AssertEquals("New declaration 3 EntryInstruction SubStyle", "Y", dec3.CustomsEntryInstructions[0].CEI_SubStyle);

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

			AssertEquals("EntryInstruction 1 Style", "H6", declaration.CustomsEntryInstructions[0].CEI_Style);
			AssertEquals("EntryInstruction 2 Style", "H4", declaration.CustomsEntryInstructions[1].CEI_Style);
			AssertEquals("EntryInstruction 3 Style", "H5", declaration.CustomsEntryInstructions[2].CEI_Style);
			AssertEquals("CH_EntryStatus of entryHeader4 is not REL - unchanged", "I1", declaration.CustomsEntryInstructions[3].CEI_Style);

			AssertEquals("EntryInstruction 1 SubStyle", "Y", declaration.CustomsEntryInstructions[0].CEI_SubStyle);
			AssertEquals("EntryInstruction 2 SubStyle", "Y", declaration.CustomsEntryInstructions[1].CEI_SubStyle);
			AssertEquals("EntryInstruction 3 SubStyle", "Y", declaration.CustomsEntryInstructions[2].CEI_SubStyle);
			AssertEquals("EntryInstruction 4 SubStyle - unchanged", "A", declaration.CustomsEntryInstructions[3].CEI_SubStyle);

			var docs1 = declaration.CustomsEntryInstructions[0].PreviousDocuments;
			var docs2 = declaration.CustomsEntryInstructions[1].PreviousDocuments;
			var docs3 = declaration.CustomsEntryInstructions[2].PreviousDocuments;
			var docs4 = declaration.CustomsEntryInstructions[3].PreviousDocuments;
			AssertHasPreviousDocument("EntryInstruction 1", docs1, "MRN0001");
			AssertHasPreviousDocument("EntryInstruction 2", docs2, "MRN0002");
			AssertHasPreviousDocument("EntryInstruction 3", docs3, "MRN0003");
			AssertEquals("CH_EntryStatus of entryHeader4 is not REL - unchanged", 0, docs4.Count);

			AssertEquals("EntryHeader 1.EntryNumber", ZString.Empty, entry1.EntryNumber);
			AssertEquals("EntryHeader 2 EntryNumber", ZString.Empty, entry2.EntryNumber);
			AssertEquals("EntryHeader 3.EntryNumber", ZString.Empty, entry3.EntryNumber);
			AssertEquals("CH_EntryStatus of entryHeader4 is not REL - unchanged", "444", entry4.EntryNumber);

			AssertEquals("EntryHeader 1.CH_EntryStatus", ZString.Empty, entry1.CH_EntryStatus);
			AssertEquals("EntryHeader 2.CH_EntryStatus", ZString.Empty, entry2.CH_EntryStatus);
			AssertEquals("EntryHeader 3.CH_EntryStatus", ZString.Empty, entry3.CH_EntryStatus);
			AssertEquals("CH_EntryStatus of entryHeader4 is not REL - unchanged", DeltaIEImportCusEntryStatusList.Codes.Amended, entry4.CH_EntryStatus);

			AssertEquals("EntryHeader 1 EntryLines Count", 0, entry1.MergedLines.Count);
			AssertEquals("EntryHeader 1 Messages Count", 0, entry1.Messages.Count);
			AssertEquals("EntryHeader 1 MRN", ZString.Empty, entry1.MovementReferenceNumber);
			AssertEquals("EntryHeader 2 EntryLines Count", 0, entry2.MergedLines.Count);
			AssertEquals("EntryHeader 2 Messages Count", 0, entry2.Messages.Count);
			AssertEquals("EntryHeader 2 MRN", ZString.Empty, entry2.MovementReferenceNumber);
			AssertEquals("EntryHeader 3 EntryLines Count", 0, entry3.MergedLines.Count);
			AssertEquals("EntryHeader 3 Messages Count", 0, entry3.Messages.Count);
			AssertEquals("EntryHeader 3 MRN", ZString.Empty, entry3.MovementReferenceNumber);
			AssertEquals("EntryHeader 4 EntryLines Count - not changed", 1, entry4.MergedLines.Count);
			AssertEquals("EntryHeader 4 Messages Count - not changed", 1, entry4.Messages.Count);
			AssertEquals("EntryHeader 4 MRN - not changed", "MRN0004", entry4.MovementReferenceNumber);

			AssertEquals("Declaration Message Count", 3, declaration.Messages.Count);
		}

		void AssertHasPreviousDocument(string message, PreviousDocumentCollection documents, ZString expectedReference)
		{
			AssertEquals(message + " Previous Documents Count", 1, documents.Count);
			AssertEquals(message + " Previous Document Code", UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.NMRN, documents[0].CSI_Code);
			AssertEquals(message + " Previous Document ReferenceNumber", expectedReference, documents[0].CSI_ReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_UCR = "UCR0123";
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Mail;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			declaration.JE_DateOfFirstArrival = new ZDateTime(2025, 1, 1);
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = EUCommonConstants.ImportDeclarationTypeList.I1;
			entryInstruction1.CEI_SubStyle = "A";
			entryInstruction1.CEI_Procedure = "40";
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = EUCommonConstants.ImportDeclarationTypeList.I1;
			entryInstruction2.CEI_SubStyle = "C";
			entryInstruction2.CEI_Procedure = "51";
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction3.CEI_Style = EUCommonConstants.ImportDeclarationTypeList.I1;
			entryInstruction3.CEI_SubStyle = "Z";
			entryInstruction3.CEI_Procedure = "61";
			var entryInstruction4 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction4.CEI_Style = EUCommonConstants.ImportDeclarationTypeList.I1;
			entryInstruction4.CEI_SubStyle = "A";
			entryInstruction4.CEI_Procedure = "61";

			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackQty = 11;
			package1.CW_PackType = "AA";
			package1.CW_MarksAndNos = "AA1";

			var package2 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package2.CW_PackQty = 16;
			package2.CW_PackType = "AA";
			package2.CW_MarksAndNos = "AA2";

			var package3 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package3.CW_PackQty = 36;
			package3.CW_PackType = "AA";
			package3.CW_MarksAndNos = "AA3";

			var invHeader1 = declaration.Invoices.AddNew();
			invHeader1.JZ_Remarks = "Header1";
			var invLine11 = invHeader1.InvoiceLines.AddNew();
			invLine11.JI_CEI = entryInstruction1.PK;
			invLine11.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;
			invLine11.PackagesForInvoiceLinesForBindingOnly[0].PackQty = 1;

			var invLine12 = invHeader1.InvoiceLines.AddNew();
			invLine12.JI_CEI = entryInstruction2.PK;
			invLine12.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;
			invLine12.PackagesForInvoiceLinesForBindingOnly[0].PackQty = 2;

			var invHeader2 = declaration.Invoices.AddNew();
			invHeader2.JZ_Remarks = "Header2";
			var invLine2 = invHeader2.InvoiceLines.AddNew();
			invLine2.JI_CEI = entryInstruction2.PK;
			invLine2.PackagesForInvoiceLinesForBindingOnly[2].IsLinked = true;
			invLine2.PackagesForInvoiceLinesForBindingOnly[2].PackQty = 4;

			var invHeader3 = declaration.Invoices.AddNew();
			invHeader3.JZ_Remarks = "Header3";
			var invLine3 = invHeader3.InvoiceLines.AddNew();
			invLine3.JI_CEI = entryInstruction3.PK;
			invLine3.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;
			invLine3.PackagesForInvoiceLinesForBindingOnly[0].PackQty = 8;
			invLine3.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = true;
			invLine3.PackagesForInvoiceLinesForBindingOnly[1].PackQty = 16;

			var invHeader4 = declaration.Invoices.AddNew();
			invHeader4.JZ_Remarks = "Header4";
			var invLine4 = invHeader4.InvoiceLines.AddNew();
			invLine4.JI_CEI = entryInstruction4.PK;
			invLine4.PackagesForInvoiceLinesForBindingOnly[2].IsLinked = true;
			invLine4.PackagesForInvoiceLinesForBindingOnly[2].PackQty = 32;

			declaration.DoMerge();
			var entryHeader1 = declaration.CustomsEntryHeaders.Single(x => x.CH_CEI_Instruction == entryInstruction1.PK);
			var entryHeader2 = declaration.CustomsEntryHeaders.Single(x => x.CH_CEI_Instruction == entryInstruction2.PK);
			var entryHeader3 = declaration.CustomsEntryHeaders.Single(x => x.CH_CEI_Instruction == entryInstruction3.PK);
			var entryHeader4 = declaration.CustomsEntryHeaders.Single(x => x.CH_CEI_Instruction == entryInstruction4.PK);

			entryHeader1.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.Released;
			entryHeader1.MovementReferenceNumberSetter("MRN0001");
			entryHeader1.EntryNumber = "111";
			entryHeader2.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.Released;
			entryHeader2.MovementReferenceNumberSetter("MRN0002");
			entryHeader2.EntryNumber = "222";
			entryHeader3.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.Released;
			entryHeader3.MovementReferenceNumberSetter("MRN0003");
			entryHeader3.EntryNumber = "333";
			entryHeader4.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.Amended;
			entryHeader4.MovementReferenceNumberSetter("MRN0004");
			entryHeader4.EntryNumber = "444";

			declarationCreator = DeclarationCreator.GetDeclarationCreator(declaration, new SelectClearedSimplifiedEntryHeaders(declaration));
		}

		DeclarationCreator declarationCreator;
		JobDeclaration declaration;
	}
}
