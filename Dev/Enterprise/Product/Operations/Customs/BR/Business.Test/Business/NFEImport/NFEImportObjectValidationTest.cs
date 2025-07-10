using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class NFEImportObjectValidationTest : TestCaseWithFactory
	{
		public void TestParent()
		{
			var parent = new NFEImportObject(Factory);
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestCheckIncoterm()
		{
			var nfeImportObject = new NFEImportObject(Factory);
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(nfeImportObject.IncotermInfo, "XXX", "FOB");
		}

		public void TestCheckIncotermDiferentInSameInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.FOB;

			var nfeImport = new NFEImportObjectParent(declaration);
			var nfe1 = nfeImport.NFEImportObjectCollection.AddNew();
			nfe1.ObjectParent = nfeImport;
			nfe1.Declaration = declaration;
			nfe1.InvoiceHeaderPK = invoiceHeader.PK;
			nfe1.Incoterm = "FOB";

			var nfe2 = nfeImport.NFEImportObjectCollection.AddNew();
			nfe2.ObjectParent = nfeImport;
			nfe2.Declaration = declaration;
			nfe2.InvoiceHeaderPK = invoiceHeader.PK;
			nfe2.Incoterm = "CIF";

			AssertHasError(nfe1.IncotermInfo, "You can not enter different Incoterms in NF-e linked to the same Invoice No.");
			AssertHasError(nfe2.IncotermInfo, "You can not enter different Incoterms in NF-e linked to the same Invoice No.");

			nfe2.Incoterm = "FOB";
			AssertNoError(nfe1.IncotermInfo, "You can not enter different Incoterms in NF-e linked to the same Invoice No.");
			AssertNoError(nfe2.IncotermInfo, "You can not enter different Incoterms in NF-e linked to the same Invoice No.");
		}

		public void TestCheckCurrencyCode()
		{
			var nfeImportObject = new NFEImportObject(Factory);
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(nfeImportObject.CurrencyCodeInfo, "XXX", "USD");
		}

		public void TestCheckCurrencyDiferentInSameInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.FOB;

			var nfeImport = new NFEImportObjectParent(declaration);
			var nfe1 = nfeImport.NFEImportObjectCollection.AddNew();
			nfe1.ObjectParent = nfeImport;
			nfe1.Declaration = declaration;
			nfe1.InvoiceHeaderPK = invoiceHeader.PK;
			nfe1.CurrencyCode = "BRL";

			var nfe2 = nfeImport.NFEImportObjectCollection.AddNew();
			nfe2.ObjectParent = nfeImport;
			nfe2.Declaration = declaration;
			nfe2.InvoiceHeaderPK = invoiceHeader.PK;
			nfe2.CurrencyCode = "USD";

			AssertHasError(nfe1.CurrencyCodeInfo, "You can not enter different Currencies in NF-e linked to the same Invoice No.");
			AssertHasError(nfe2.CurrencyCodeInfo, "You can not enter different Currencies in NF-e linked to the same Invoice No.");

			nfe2.CurrencyCode = "BRL";
			AssertNoError(nfe1.CurrencyCodeInfo, "You can not enter different Currencies in NF-e linked to the same Invoice No.");
			AssertNoError(nfe2.CurrencyCodeInfo, "You can not enter different Currencies in NF-e linked to the same Invoice No.");
		}

		public void TestCheckExchangeRate()
		{
			var nfeImportObject = new NFEImportObject(Factory);
			nfeImportObject.ExchangeRate = ZDecimal.Zero;
			AssertHasWarningContaining(nfeImportObject.ExchangeRateInfo, MandatoryValidation.ValueCannotBeZero);

			nfeImportObject.ExchangeRate = 23m;
			AssertNoWarningContaining(nfeImportObject.ExchangeRateInfo, MandatoryValidation.ValueCannotBeZero);
		}

		public void TestCheckInvoiceHeaderPK()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.FOB;

			var nfeImport = new NFEImportObjectParent(declaration);
			var nfeImportObject = nfeImport.NFEImportObjectCollection.AddNew();
			nfeImportObject.Declaration = declaration;

			var lookups = new NFEImportObjectLookups(nfeImportObject);
			AssertEquals(lookups.InvoiceHeaderList, nfeImportObject.Declaration.Invoices);

			nfeImportObject.InvoiceHeaderPK = new ZGuid("EBBFB0B0-2C6C-44E3-9F20-88B1AC0BE42F");
			AssertHasError(nfeImportObject.InvoiceHeaderPKInfo, "Enter a valid Invoice No..");

			nfeImportObject.InvoiceHeaderPK = nfeImportObject.Declaration.Invoices.First().PK;
			AssertNoError(nfeImportObject.InvoiceHeaderPKInfo, "Enter a valid Invoice No..");
		}

		public void TestCheckEntryInstructionPK()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "ENTRY01";
			entryInstruction.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;

			var nfeImport = new NFEImportObjectParent(declaration);
			var nfeImportObject = nfeImport.NFEImportObjectCollection.AddNew();
			nfeImportObject.Declaration = declaration;

			nfeImportObject.Validation.ValidateEntryInstructionPK();
			AssertNoError(nfeImportObject.EntryInstructionPKInfo, "Enter a valid Entry Instruction.");

			nfeImportObject.EntryInstructionPK = new ZGuid("EBBFB0B0-2C6C-44E3-9F20-88B1AC0BE42F");
			AssertHasError(nfeImportObject.EntryInstructionPKInfo, "Enter a valid Entry Instruction.");

			nfeImportObject.EntryInstructionPK = entryInstruction.PK;
			AssertNoError(nfeImportObject.EntryInstructionPKInfo, "Enter a valid Entry Instruction.");
		}
	}
}
