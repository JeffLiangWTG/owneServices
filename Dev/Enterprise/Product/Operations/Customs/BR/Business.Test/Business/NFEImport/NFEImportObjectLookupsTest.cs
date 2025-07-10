using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class NFEImportObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIncoTermList()
		{
			var nfeImportObject = new NFEImportObject(Factory);
			var lookups = new NFEImportObjectLookups(nfeImportObject);
			var incoTermList = "CFR, CIF, CIP, C+F, C+I, CPT, DAP, DAT, DDP, DPU, EXW, FAS, FCA, FOB, OCV";
			AssertContainsExactElementsInAnyOrder(incoTermList, lookups.IncoTermList.CodesAsString);
		}

		public void TestCurrencyList()
		{
			var nfeImportObject = new NFEImportObject(Factory);
			var lookups = new NFEImportObjectLookups(nfeImportObject);
			AssertType<RefCurrencyCollection>(lookups.CurrencyList);
		}

		public void TestInvoiceHeaderList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.FOB;

			var nfeImportObject = new NFEImportObject(Factory);
			var lookups = new NFEImportObjectLookups(nfeImportObject);
			AssertEquals(0, lookups.InvoiceHeaderList.Count);

			nfeImportObject.Declaration = declaration;
			AssertEquals(nfeImportObject.Declaration.Invoices, lookups.InvoiceHeaderList);
		}

		public void TestEntryInstructionsList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_LegalDocument = LegalDocumentList.Codes.NoInvoice;

			var nfeImportObject = new NFEImportObject(Factory);
			var lookups = new NFEImportObjectLookups(nfeImportObject);
			AssertNull(lookups.EntryInstructionList);

			nfeImportObject.Declaration = declaration;
			AssertContainsExactElementsInAnyOrder(new[] { entryInstruction1.PK }, lookups.EntryInstructionList.Select(x => x.PK));

			declaration.MakeNonPersistent();
			AssertNull(lookups.EntryInstructionList);
		}
	}
}
