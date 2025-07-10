using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(NFEImportObjectParent))]
	class NFEImportObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestImportInvoices_IncotermAndCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			var nfeImport = new NFEImportObjectParent(declaration);
			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlText, "(1/1) Inv. Header: 180666 - NF-e 180666 - New invoice created, lines successfully imported into Inv. Header.");

			var nfeImportObject = nfeImport.NFEImportObjectCollection.First() as NFEImportObject;
			var invoiceHeader = declaration.Invoices.FirstOrDefault();

			AssertEquals("Invoice Header incoterm should be the same as nfeImportObject", invoiceHeader.IncoTerm, nfeImportObject.Incoterm);
			AssertEquals("Invoice Header currency should be the same as nfeImportObject", invoiceHeader.JZ_RX_NKInvoice_Currency, nfeImportObject.CurrencyCode);
		}

		public void TestImportInvoices_CreateNewInvoices()
		{
			var declaration = Factory.New<JobDeclaration>();
			var nfeImport = new NFEImportObjectParent(declaration);
			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlText,
				"(1/2) Inv. Header: 180666 - NF-e 180666 - New invoice created, lines successfully imported into Inv. Header.\r\n(2/2) Inv. Header: 180667 - NF-e 180667 - New invoice created, lines successfully imported into Inv. Header.", true);

			var nfeImportObject = nfeImport.NFEImportObjectCollection.First() as NFEImportObject;
			var invoiceHeader = declaration.Invoices.FirstOrDefault();

			AssertEquals("A new NFE Entry Instruction created", 1, declaration.CustomsEntryInstructions.Count);
			var entryInstruction = declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().FirstOrDefault();
			AssertEquals("CEI_LegalDocument", LegalDocumentList.Codes.ElectronicLogisticInvoice, entryInstruction.CEI_LegalDocument);
			AssertEquals("CEI_Description", "Created from Import NF-e", entryInstruction.CEI_Description);
			AssertEquals("Imported Files", 2, nfeImport.NFEImportObjectCollection.Count);
			CombineAssertions(() =>
			{
				AssertEquals("NF-e Key", "35200857012650000174550010001806661378259720", nfeImportObject.NfeKey);
				AssertEquals("NF-e Serie", "1", nfeImportObject.NfeSerie);
				AssertEquals("NF-e Number", "180666", nfeImportObject.NfeNumber);
				AssertEquals("Nf-e Date", new ZDateTime(2021, 5, 2), nfeImportObject.NfeDate);
				AssertEquals("NF-e Gross Weight", 280m, nfeImportObject.NfeGrossWeight);
				AssertEquals("NF-e Net Weight", 240m, nfeImportObject.NfeNetWeight);
				AssertEquals("NF-e Invoice Amount", 49687.15m, nfeImportObject.NfeInvoiceAmount);
				AssertEquals("Currency", ZString.Empty, nfeImportObject.CurrencyCode);
				nfeImportObject.CurrencyCode = Core.Constants.CurrencyCodes.Brazil;
				AssertEquals("Currency", Core.Constants.CurrencyCodes.Brazil, nfeImportObject.CurrencyCode);
				AssertEquals("JZ_InvoiceDate should be ", new ZDateTime(2021, 5, 2), invoiceHeader.JZ_InvoiceDate);
				AssertEquals("Exchange Rate Buy", new ZDecimal(1), nfeImportObject.ExchangeRateBuy);
				AssertEquals("Exchange Rate Sell", new ZDecimal(1), nfeImportObject.ExchangeRateSell);
				AssertEquals("Invoices Lines should be ", 2, declaration.InvoiceLines.Count);

				AssertEquals("JZ_InvoiceDate should be ", new ZDateTime(2021, 5, 2), invoiceHeader.JZ_InvoiceDate);
				AssertEquals("JZ_InvoiceAmount should be ", 49846.11m, invoiceHeader.JZ_InvoiceAmount);
				AssertEquals("JZ_Weight should be ", 280m, invoiceHeader.JZ_Weight);
				AssertEquals("JZ_NetWeight should be ", 240m, invoiceHeader.JZ_NetWeight);

				var invoiceLine = declaration.InvoiceLines.FirstOrDefault() as JobComInvoiceLine;
				AssertEquals("JI_PartNo should be ", "FM3555", invoiceLine.JI_PartNo);
				AssertEquals("JI_Description should be ", "COLMEIA ESPECIAL 890 X 910 X 7TBC / 8APP ESTANHADA", invoiceLine.JI_Description);
				AssertEquals("JI_NFENumber should be ", "35200857012650000174550010001806661378259720", invoiceLine.JI_NFeNumber);
				AssertEquals("JI_NFEItemNumber should be ", "1", invoiceLine.JI_NFeItemNumber);
				AssertEquals("JI_Tariff should be ", "87089100", invoiceLine.JI_Tariff);
				AssertEquals("JI_InvoiceUQ should be ", "PC", invoiceLine.JI_InvoiceUQ);
				AssertEquals("JI_InvoiceQuantity should be ", 4m, invoiceLine.JI_InvoiceQuantity);
				AssertEquals("JI_CustomsUnitQty should be ", "UN", invoiceLine.JI_CustomsUnitQty);
				AssertEquals("JI_CustomsQuantity should be ", 4m, invoiceLine.JI_CustomsQuantity);
				AssertEquals("JI_LinePrice should be ", 49687.15m, invoiceLine.JI_LinePrice);
				AssertEquals("JI_Weight should be ", 0m, invoiceLine.JI_Weight);
				AssertEquals("JI_NetWeight should be ", 0m, invoiceLine.JI_NetWeight);
				AssertEquals("JI_NFeLinePrice should be ", 49792.15m, invoiceLine.JI_NFeLinePrice);
				AssertEquals("ComplementaryDescriptionExport should be ", "COMPLEMENTARY", invoiceLine.ComplementaryDescription);
				AssertEquals("JI_NFeLinePriceCurrency should be ", Core.Constants.CurrencyCodes.Brazil, invoiceLine.JI_NFeLinePriceCurrency);

				declaration.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line =>
				{
					AssertEquals("Entry Instruction should be the same for all lines", entryInstruction.PK, line.JI_CEI);
				});
			});
		}

		public void TestImportInvoicesWithHeaderFoundDuringImportProcess()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.FOB;
			Factory.Save();

			var nfeImport = new NFEImportObjectParent(declaration);
			CombineAssertions(() =>
			{
				AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlText, "(1/1) Inv. Header:  - NF-e 180666 lines successfully imported into existing Inv. Header.", false, invoiceHeader.PK);

				var nfeImportObject = nfeImport.NFEImportObjectCollection.First() as NFEImportObject;
				AssertEquals("NF-e Key", "35200857012650000174550010001806661378259720", nfeImportObject.NfeKey);
				AssertEquals("NF-e Serie", "1", nfeImportObject.NfeSerie);
				AssertEquals("NF-e Number", "180666", nfeImportObject.NfeNumber);
				AssertEquals("Nf-e Date", new ZDateTime(2021, 5, 2), nfeImportObject.NfeDate);
				AssertEquals("NF-e Gross Weight", 280m, nfeImportObject.NfeGrossWeight);
				AssertEquals("NF-e Net Weight", 240m, nfeImportObject.NfeNetWeight);
				AssertEquals("NF-e Invoice Amount", 49687.15m, nfeImportObject.NfeInvoiceAmount);
				AssertEquals("Currency", Core.Constants.CurrencyCodes.Brazil, nfeImportObject.CurrencyCode);
				AssertEquals("Invoice No. - PK", invoiceHeader.PK, nfeImportObject.InvoiceHeaderPK);
				AssertEquals("Invoices Lines should be ", 1, declaration.InvoiceLines.Count);
				AssertEquals("JZ_InvoiceDate should be ", new ZDateTime(2021, 5, 2), invoiceHeader.JZ_InvoiceDate);
				AssertEquals("JZ_InvoiceAmount should be ", 0m, invoiceHeader.JZ_InvoiceAmount);
				AssertEquals("JZ_Weight should be ", 0m, invoiceHeader.JZ_Weight);
				AssertEquals("JZ_NetWeight should be ", 0m, invoiceHeader.JZ_NetWeight);
				AssertEquals("JZ_IncoTerm should be ", BRIncoTermList.Codes.FOB, invoiceHeader.JZ_IncoTerm);

				var invoiceLine = declaration.InvoiceLines.FirstOrDefault() as JobComInvoiceLine;
				AssertEquals("JI_PartNo should be ", "FM3555", invoiceLine.JI_PartNo);
				AssertEquals("JI_Description should be ", "COLMEIA ESPECIAL 890 X 910 X 7TBC / 8APP ESTANHADA", invoiceLine.JI_Description);
				AssertEquals("JI_NFENumber should be ", "35200857012650000174550010001806661378259720", invoiceLine.JI_NFeNumber);
				AssertEquals("JI_NFEItemNumber should be ", "1", invoiceLine.JI_NFeItemNumber);
				AssertEquals("JI_Tariff should be ", "87089100", invoiceLine.JI_Tariff);
				AssertEquals("JI_InvoiceUQ should be ", "PC", invoiceLine.JI_InvoiceUQ);
				AssertEquals("JI_InvoiceQuantity should be ", 4m, invoiceLine.JI_InvoiceQuantity);
				AssertEquals("JI_CustomsUnitQty should be ", "UN", invoiceLine.JI_CustomsUnitQty);
				AssertEquals("JI_CustomsQuantity should be ", 4m, invoiceLine.JI_CustomsQuantity);
				AssertEquals("JI_NFeLinePrice should be ", 49792.15m, invoiceLine.JI_NFeLinePrice);
				AssertEquals("JI_LinePrice should be ", 49687.15m, invoiceLine.JI_LinePrice);
				AssertEquals("JI_Weight should be ", 0m, invoiceLine.JI_Weight);
				AssertEquals("JI_NetWeight should be ", 0m, invoiceLine.JI_NetWeight);
				AssertEquals("ComplementaryDescriptionExport should be ", "COMPLEMENTARY", invoiceLine.ComplementaryDescription);
				AssertEquals("JI_LinePrice should be ", 49687.15m, invoiceLine.JI_LinePrice);
				AssertEquals("JI_NFeLinePriceCurrency should be ", Core.Constants.CurrencyCodes.Brazil, invoiceLine.JI_NFeLinePriceCurrency);
			});
		}

		public void TestImportInvoicesWithChargesAndFrete()
		{
			var declaration = Factory.New<JobDeclaration>();
			var nfeImport = new NFEImportObjectParent(declaration);
			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlTextWithProductsAndFrete,
				"(1/1) Inv. Header: 3344 - NF-e 3344 - New invoice created, lines successfully imported into Inv. Header.");
			AssertEquals("Invoices Lines should be ", 3, declaration.InvoiceLines.Count);

			var invoiceLine1 = declaration.InvoiceLines[0];
			var charge1 = invoiceLine1.Charges.First();
			AssertCharge(charge1, Common.CustomsChargeTypeList.Codes.OverseasFreight, new ZDecimal(391.63), ZBool.False, ZBool.True, ZBool.False, ZBool.False);

			var invoiceLine2 = declaration.InvoiceLines[1];
			var charge2 = invoiceLine2.Charges.First();
			AssertCharge(charge2, Common.CustomsChargeTypeList.Codes.OverseasFreight, new ZDecimal(11652.91), ZBool.False, ZBool.True, ZBool.False, ZBool.False);

			var invoiceLine3 = declaration.InvoiceLines[2];
			var charge3 = invoiceLine3.Charges.First();
			AssertCharge(charge3, Common.CustomsChargeTypeList.Codes.OverseasFreight, new ZDecimal(146.84), ZBool.False, ZBool.True, ZBool.False, ZBool.False);
		}

		public void TestImportInvoices_InvoiceHasEmptyIncotermCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;
			invoiceHeader.JZ_IncoTerm = ZString.Empty;
			invoiceHeader.JZ_InvoiceNumber = "7567";

			var nfeImport = new NFEImportObjectParent(declaration);

			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlTextWithProductsAndFreteAndSegAndOutro,
				"(1/1) Inv. Header: 7567 - NF-e 420118 - Error - Incoterm and/or Currency were not entered.", false, invoiceHeader.PK);

			AssertEquals("Invoices Lines should be ", 0, declaration.InvoiceLines.Count);
		}

		public void TestImportInvoices_InvoiceWithSameInvoiceNumberExists()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader1.JZ_IncoTerm = BRIncoTermList.Codes.FOB;
			invoiceHeader1.JZ_InvoiceNumber = "34234";
			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader2.JZ_IncoTerm = BRIncoTermList.Codes.FOB;
			invoiceHeader2.JZ_InvoiceNumber = "53454";
			var invoiceHeader3 = declaration.Invoices.AddNew();
			invoiceHeader3.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader3.JZ_IncoTerm = BRIncoTermList.Codes.FOB;
			invoiceHeader3.JZ_InvoiceNumber = "420118";

			var nfeImport = new NFEImportObjectParent(declaration);
			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlTextWithProductsAndFreteAndSegAndOutro,
				"(1/1) Inv. Header: 420118 - NF-e 420118 - Error - Invoice with the same Invoice Number already exists.");
			AssertEquals("Invoices Lines should be ", 0, declaration.InvoiceLines.Count);
		}

		public void TestImportInvoices_InvoiceAndOneInvoicePreCreated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader1.JZ_IncoTerm = BRIncoTermList.Codes.FOB;
			invoiceHeader1.JZ_InvoiceNumber = "34234";

			var nfeImport = new NFEImportObjectParent(declaration);
			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlText,
				"(1/1) Inv. Header: 34234 - NF-e 180666 lines successfully imported into existing Inv. Header.", false, invoiceHeader1.PK);

			AssertEquals("Invoice No. - PK", invoiceHeader1.PK, nfeImport.NFEImportObjectCollection[0].InvoiceHeaderPK);
			AssertEquals("Invoices Lines should be ", 1, declaration.InvoiceLines.Count);
		}

		public void TestImportInvoicesWithChargesAndFreteAndSegAndOutro()
		{
			var declaration = Factory.New<JobDeclaration>();
			var nfeImport = new NFEImportObjectParent(declaration);

			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlTextWithProductsAndFreteAndSegAndOutro,
				"(1/1) Inv. Header: 420118 - NF-e 420118 - New invoice created, lines successfully imported into Inv. Header.");

			var invoiceLine1 = declaration.InvoiceLines[0];
			var charge11 = invoiceLine1.Charges[0];
			AssertCharge(charge11, Common.CustomsChargeTypeList.Codes.OverseasFreight, new ZDecimal(753.40), ZBool.False, ZBool.True, ZBool.False, ZBool.False);

			var charge12 = invoiceLine1.Charges[1];
			AssertCharge(charge12, Common.CustomsChargeTypeList.Codes.OverseasInsurance, new ZDecimal(0.69), ZBool.False, ZBool.True, ZBool.False, ZBool.False);

			var charge13 = invoiceLine1.Charges[2];
			AssertCharge(charge13, Common.CustomsChargeTypeList.Codes.OtherCharges, new ZDecimal(222.69), ZBool.True, ZBool.True, ZBool.False, ZBool.False);

			var invoiceLine2 = declaration.InvoiceLines[1];
			var charge21 = invoiceLine2.Charges[0];
			AssertCharge(charge21, Common.CustomsChargeTypeList.Codes.OverseasFreight, new ZDecimal(376.70), ZBool.False, ZBool.True, ZBool.False, ZBool.False);

			var charge22 = invoiceLine2.Charges[1];
			AssertCharge(charge22, Common.CustomsChargeTypeList.Codes.OverseasInsurance, new ZDecimal(0.35), ZBool.False, ZBool.True, ZBool.False, ZBool.False);

			var charge23 = invoiceLine2.Charges[2];
			AssertCharge(charge23, Common.CustomsChargeTypeList.Codes.OtherCharges, new ZDecimal(111.35), ZBool.True, ZBool.True, ZBool.False, ZBool.False);

			var invoiceLine3 = declaration.InvoiceLines[2];
			var charge31 = invoiceLine3.Charges[0];
			AssertCharge(charge31, Common.CustomsChargeTypeList.Codes.OverseasFreight, new ZDecimal(2260.19), ZBool.False, ZBool.True, ZBool.False, ZBool.False);

			var charge32 = invoiceLine3.Charges[1];
			AssertCharge(charge32, Common.CustomsChargeTypeList.Codes.OverseasInsurance, new ZDecimal(2.07), ZBool.False, ZBool.True, ZBool.False, ZBool.False);

			var charge33 = invoiceLine3.Charges[2];
			AssertCharge(charge33, Common.CustomsChargeTypeList.Codes.OtherCharges, new ZDecimal(668.07), ZBool.True, ZBool.True, ZBool.False, ZBool.False);
		}

		public void TestImportInvoicesWithChargesAndDesc()
		{
			var declaration = Factory.New<JobDeclaration>();
			var nfeImport = new NFEImportObjectParent(declaration);

			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlTextWithProductsAndDesc,
				"(1/1) Inv. Header: 3907 - NF-e 3907 - New invoice created, lines successfully imported into Inv. Header.");
			AssertEquals("Invoices Lines should be ", 3, declaration.InvoiceLines.Count);

			var invoiceLine1 = declaration.InvoiceLines[0];
			var charge1 = invoiceLine1.Charges.First();
			AssertCharge(charge1, Common.CustomsChargeTypeList.Codes.Discount, new ZDecimal(34.95), ZBool.False, ZBool.False, ZBool.False, ZBool.False);

			var invoiceLine2 = declaration.InvoiceLines[1];
			var charge2 = invoiceLine2.Charges.First();
			AssertCharge(charge2, Common.CustomsChargeTypeList.Codes.Discount, new ZDecimal(12.98), ZBool.False, ZBool.False, ZBool.False, ZBool.False);

			var invoiceLine3 = declaration.InvoiceLines[2];
			var charge3 = invoiceLine3.Charges.First();
			AssertCharge(charge3, Common.CustomsChargeTypeList.Codes.Discount, new ZDecimal(2.24), ZBool.False, ZBool.False, ZBool.False, ZBool.False);
		}

		public void TestImportInvoicesForCPLUSIIncoterm()
		{
			var declaration = Factory.New<JobDeclaration>();
			var nfeImport = new NFEImportObjectParent(declaration);
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.CPLUSI;
			invoiceHeader.JZ_InvoiceNumber = "34234";

			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlTextWithProductsAndFreteAndSegAndOutroAndDesc,
				"(1/1) Inv. Header: 34234 - NF-e 420118 lines successfully imported into existing Inv. Header.", false, invoiceHeader.PK);

			var invoiceLine1 = declaration.InvoiceLines[0];

			var overseasFreight = invoiceLine1.Charges[0];
			AssertCharge(overseasFreight, Common.CustomsChargeTypeList.Codes.OverseasFreight, new ZDecimal(753.40), ZBool.False, ZBool.True, ZBool.False, ZBool.True);

			var overseasInsurance = invoiceLine1.Charges[1];
			AssertCharge(overseasInsurance, Common.CustomsChargeTypeList.Codes.OverseasInsurance, new ZDecimal(4.83), ZBool.False, ZBool.True, ZBool.False, ZBool.False);

			var otherCharges = invoiceLine1.Charges[2];
			AssertCharge(otherCharges, Common.CustomsChargeTypeList.Codes.OtherCharges, new ZDecimal(1558.83), ZBool.True, ZBool.True, ZBool.False, ZBool.False);

			var discount = invoiceLine1.Charges[3];
			AssertCharge(discount, Common.CustomsChargeTypeList.Codes.Discount, new ZDecimal(1.00), ZBool.False, ZBool.False, ZBool.False, ZBool.False);
		}

		public void TestImportInvoicesForFCAIncoterm()
		{
			var declaration = Factory.New<JobDeclaration>();
			var nfeImport = new NFEImportObjectParent(declaration);
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.FCA;
			invoiceHeader.JZ_InvoiceNumber = "34234";

			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlTextWithProductsAndFreteAndSegAndOutroAndDesc,
				"(1/1) Inv. Header: 34234 - NF-e 420118 lines successfully imported into existing Inv. Header.", false, invoiceHeader.PK);

			var invoiceLine1 = declaration.InvoiceLines[0];

			var overseasFreight = invoiceLine1.Charges[0];
			AssertCharge(overseasFreight, Common.CustomsChargeTypeList.Codes.OverseasFreight, new ZDecimal(753.40), ZBool.False, ZBool.True, ZBool.False, ZBool.True);

			var overseasInsurance = invoiceLine1.Charges[1];
			AssertCharge(overseasInsurance, Common.CustomsChargeTypeList.Codes.OverseasInsurance, new ZDecimal(4.83), ZBool.False, ZBool.True, ZBool.False, ZBool.True);

			var otherCharges = invoiceLine1.Charges[2];
			AssertCharge(otherCharges, Common.CustomsChargeTypeList.Codes.OtherCharges, new ZDecimal(1558.83), ZBool.True, ZBool.True, ZBool.False, ZBool.False);

			var discount = invoiceLine1.Charges[3];
			AssertCharge(discount, Common.CustomsChargeTypeList.Codes.Discount, new ZDecimal(1.00), ZBool.False, ZBool.False, ZBool.False, ZBool.False);
		}

		public void TestImportInvoicesForFASIncoterm()
		{
			var declaration = Factory.New<JobDeclaration>();
			var nfeImport = new NFEImportObjectParent(declaration);
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.FAS;
			invoiceHeader.JZ_InvoiceNumber = "34234";

			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlTextWithProductsAndFreteAndSegAndOutroAndDesc,
				"(1/1) Inv. Header: 34234 - NF-e 420118 lines successfully imported into existing Inv. Header.", false, invoiceHeader.PK);

			var invoiceLine1 = declaration.InvoiceLines[0];

			var overseasFreight = invoiceLine1.Charges[0];
			AssertCharge(overseasFreight, Common.CustomsChargeTypeList.Codes.OverseasFreight, new ZDecimal(753.40), ZBool.False, ZBool.True, ZBool.False, ZBool.True);

			var overseasInsurance = invoiceLine1.Charges[1];
			AssertCharge(overseasInsurance, Common.CustomsChargeTypeList.Codes.OverseasInsurance, new ZDecimal(4.83), ZBool.False, ZBool.True, ZBool.False, ZBool.True);

			var otherCharges = invoiceLine1.Charges[2];
			AssertCharge(otherCharges, Common.CustomsChargeTypeList.Codes.OtherCharges, new ZDecimal(1558.83), ZBool.True, ZBool.True, ZBool.False, ZBool.False);

			var discount = invoiceLine1.Charges[3];
			AssertCharge(discount, Common.CustomsChargeTypeList.Codes.Discount, new ZDecimal(1.00), ZBool.False, ZBool.False, ZBool.False, ZBool.False);
		}

		public void TestImportInvoicesForCIPIncoterm()
		{
			var declaration = Factory.New<JobDeclaration>();
			var nfeImport = new NFEImportObjectParent(declaration);
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.CIP;
			invoiceHeader.JZ_InvoiceNumber = "34234";

			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlTextWithProductsAndFreteAndSegAndOutroAndDesc,
				"(1/1) Inv. Header: 34234 - NF-e 420118 lines successfully imported into existing Inv. Header.", false, invoiceHeader.PK);

			var invoiceLine1 = declaration.InvoiceLines[0];

			var overseasFreight = invoiceLine1.Charges[0];
			AssertCharge(overseasFreight, Common.CustomsChargeTypeList.Codes.OverseasFreight, new ZDecimal(753.40), ZBool.False, ZBool.True, ZBool.False, ZBool.False);

			var overseasInsurance = invoiceLine1.Charges[1];
			AssertCharge(overseasInsurance, Common.CustomsChargeTypeList.Codes.OverseasInsurance, new ZDecimal(4.83), ZBool.False, ZBool.True, ZBool.False, ZBool.False);

			var otherCharges = invoiceLine1.Charges[2];
			AssertCharge(otherCharges, Common.CustomsChargeTypeList.Codes.OtherCharges, new ZDecimal(1558.83), ZBool.True, ZBool.True, ZBool.False, ZBool.False);

			var discount = invoiceLine1.Charges[3];
			AssertCharge(discount, Common.CustomsChargeTypeList.Codes.Discount, new ZDecimal(1.00), ZBool.False, ZBool.False, ZBool.False, ZBool.False);
		}

		public void TestImportInvoicesForCPTIncoterm()
		{
			var declaration = Factory.New<JobDeclaration>();
			var nfeImport = new NFEImportObjectParent(declaration);
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.CPT;
			invoiceHeader.JZ_InvoiceNumber = "34234";

			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlTextWithProductsAndFreteAndSegAndOutroAndDesc,
				"(1/1) Inv. Header: 34234 - NF-e 420118 lines successfully imported into existing Inv. Header.", false, invoiceHeader.PK);

			var invoiceLine1 = declaration.InvoiceLines[0];

			var overseasFreight = invoiceLine1.Charges[0];
			AssertCharge(overseasFreight, Common.CustomsChargeTypeList.Codes.OverseasFreight, new ZDecimal(753.40), ZBool.False, ZBool.True, ZBool.False, ZBool.False);

			var overseasInsurance = invoiceLine1.Charges[1];
			AssertCharge(overseasInsurance, Common.CustomsChargeTypeList.Codes.OverseasInsurance, new ZDecimal(4.83), ZBool.False, ZBool.True, ZBool.False, ZBool.True);

			var otherCharges = invoiceLine1.Charges[2];
			AssertCharge(otherCharges, Common.CustomsChargeTypeList.Codes.OtherCharges, new ZDecimal(1558.83), ZBool.True, ZBool.True, ZBool.False, ZBool.False);

			var discount = invoiceLine1.Charges[3];
			AssertCharge(discount, Common.CustomsChargeTypeList.Codes.Discount, new ZDecimal(1.00), ZBool.False, ZBool.False, ZBool.False, ZBool.False);
		}

		public void TestProcessStreamAndAddNew()
		{
			var declaration = Factory.New<JobDeclaration>();
			var parent = new NFEImportObjectParent(declaration);

			CombineAssertions(() =>
			{
				var result = ProcessStreamAndAddNew(parent, nfeXmlText);
				AssertEquals("NFEImportObject should be added", 1, parent.NFEImportObjectCollection.Count);
				AssertEquals("Invoice No. - PK", ZGuid.Empty, result.InvoiceHeaderPK);
				AssertEquals("NF-e Key", "35200857012650000174550010001806661378259720", result.NfeKey);
				AssertEquals("NF-e Serie", "1", result.NfeSerie);
				AssertEquals("NF-e Number", "180666", result.NfeNumber);
				AssertEquals("Nf-e Date", new ZDateTime(2021, 5, 2), result.NfeDate);
				AssertEquals("NF-e Gross Weight", 280m, result.NfeGrossWeight);
				AssertEquals("NF-e Net Weight", 240m, result.NfeNetWeight);
				AssertEquals("NF-e Invoice Amount", 49687.15m, result.NfeInvoiceAmount);
				AssertEquals("NF-e Total Freight", 158.96m, result.NfeTotalFreight);
				AssertEquals("NF-e Supplier CNPJ", "57012650000174", result.SupplierID);
				AssertEquals("NFEImportObjectItem should be added", 1, result.Items.Count);

				result = ProcessStreamAndAddNew(parent, nfeXmlText.Replace("<dhEmi>2021-05-02T06:19:00-03:00</dhEmi>", "<dhEmi>2021-05-02T00:19:00-03:00</dhEmi>"));
				AssertEquals("Nf-e Date", new ZDateTime(2021, 5, 2), result.NfeDate);

				result = ProcessStreamAndAddNew(parent, nfeXmlText.Replace("<dhEmi>2021-05-02T06:19:00-03:00</dhEmi>", "<dhEmi>2021-05-02T23:19:00-03:00</dhEmi>"));
				AssertEquals("Nf-e Date", new ZDateTime(2021, 5, 2), result.NfeDate);

				result = ProcessStreamAndAddNew(parent, nfeXmlText.Replace("<dhEmi>2021-05-02T06:19:00-03:00</dhEmi>", "<dhEmi>2021-05-02</dhEmi>"));
				AssertEquals("Nf-e Date", new ZDateTime(2021, 5, 2), result.NfeDate);

				result = ProcessStreamAndAddNew(parent, nfeXmlText.Replace("<dhEmi>2021-05-02T06:19:00-03:00</dhEmi>", "<dhEmi></dhEmi>"));
				AssertEquals("Nf-e Date", new ZDateTime(2021, 5, 2), result.NfeDate);

				result = ProcessStreamAndAddNew(parent, nfeXmlText.Replace("<dhEmi>2021-05-02T06:19:00-03:00</dhEmi>", ""));
				AssertEquals("Nf-e Date", new ZDateTime(2021, 5, 2), result.NfeDate);
			});
		}

		void AssertCharge(InvoiceLineCharge charge, ZString type, ZDecimal amount, ZBool isDutiable, ZBool isGSTApplicable, ZBool isIncludedInITOT, ZBool isNotIncludedInInvoice)
		{
			AssertEquals("J7_ChargeType should be", type, charge.J7_ChargeType);
			AssertEquals("J7_Amount should be", amount, charge.J7_Amount);
			AssertEquals("J7_RX_NKCurrency should be", charge.InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency, charge.J7_RX_NKCurrency);
			AssertEquals("J7_IsDutiable should be", isDutiable, charge.J7_IsDutiable);
			AssertEquals("J7_IsGSTApplicable should be", isGSTApplicable, charge.J7_IsGSTApplicable);
			AssertEquals("J7_IsIncludedInITOT should be", isIncludedInITOT, charge.J7_IsIncludedInITOT);
			AssertEquals("J7_IsNotIncludedInInvoice should be", isNotIncludedInInvoice, charge.J7_IsNotIncludedInInvoice);
		}

		NFEImportObject ProcessStreamAndAddNew(NFEImportObjectParent parent, string nfeXmlText)
		{
			using (var stream = GenerateStreamFromString(nfeXmlText))
			{
				parent.ProcessStreamAndAddNew(stream);
				return parent.NFEImportObjectCollection.Last() as NFEImportObject;
			}
		}

		public void TestProcessStreamWithOneInvoicePreCreated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.FOB;
			invoiceHeader.JZ_InvoiceNumber = "34234";

			var parent = new NFEImportObjectParent(declaration);
			var result = ProcessStreamAndAddNew(parent, nfeXmlText);

			CombineAssertions(() =>
			{
				AssertEquals("One NFEImportObject should be added", 1, parent.NFEImportObjectCollection.Count);
				AssertEquals("Invoice No. - PK", ZGuid.Empty, result.InvoiceHeaderPK);
				AssertEquals("NF-e Key", "35200857012650000174550010001806661378259720", result.NfeKey);
				AssertEquals("NF-e Serie", "1", result.NfeSerie);
				AssertEquals("NF-e Number", "180666", result.NfeNumber);
				AssertEquals("Nf-e Date", new ZDateTime(2021, 5, 2), result.NfeDate);
				AssertEquals("NF-e Gross Weight", 280m, result.NfeGrossWeight);
				AssertEquals("NF-e Net Weight", 240m, result.NfeNetWeight);
				AssertEquals("NF-e Invoice Amount", 49687.15m, result.NfeInvoiceAmount);
				AssertEquals("NF-e Total Freight", 158.96m, result.NfeTotalFreight);
				AssertEquals("Exchange Rate Date", new ZDateTime(2021, 4, 30), result.ExchangeRateDate);
				AssertEquals("Exchange Rate Buy", new ZDecimal(1), result.ExchangeRateBuy);
				AssertEquals("Exchange Rate Sell", new ZDecimal(1), result.ExchangeRateSell);
				AssertEquals("Declaration", declaration, result.Declaration);
				AssertEquals("Currency", Core.Constants.CurrencyCodes.Brazil, result.CurrencyCode);
				AssertEquals("Incoterm", BRIncoTermList.Codes.FOB, result.Incoterm);
				AssertEquals("Invoice No. - PK", ZGuid.Empty, result.InvoiceHeaderPK);
				AssertEquals("ObjectParent", parent, result.ObjectParent);
				AssertEquals("NFEImportObjectItem should be added", 1, result.Items.Count);
			});
		}

		public void TestProcessDuplicatedStream()
		{
			var declaration = Factory.New<JobDeclaration>();
			var nfeImport = new NFEImportObjectParent(declaration);

			using (var stream = GenerateStreamFromString(nfeXmlText))
			{
				nfeImport.ProcessStreamAndAddNew(stream);

				AssertEquals("One NFEImportObject should be added", 1, nfeImport.NFEImportObjectCollection.Count);
			}

			using (var stream = GenerateStreamFromString(nfeXmlText))
			{
				nfeImport.ProcessStreamAndAddNew(stream);

				AssertEquals("NFEImportObject with same NFE Key should not be imported", 1, nfeImport.NFEImportObjectCollection.Count);
			}
		}

		public void TestImportWithICMSTotFieldsEqualZero()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.CPT;
			invoiceHeader.JZ_InvoiceNumber = "34234";
			var nfeImport = new NFEImportObjectParent(declaration);
			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlTextWithoutICMSTotvFrete,
				"(1/1) Inv. Header: 34234 - NF-e 420118 lines successfully imported into existing Inv. Header.", false, invoiceHeader.PK);

			var invoiceLine1 = declaration.InvoiceLines[0];
			var charges1 = invoiceLine1.Charges;
			CombineAssertions(() =>
			{
				Assert("Must NOT contain OverseasFreight charge", !charges1.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.OverseasFreight));
				Assert("Must contain OverseasInsurance charge", charges1.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.OverseasInsurance));
				Assert("Must contain OtherCharges charge", charges1.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.OtherCharges));
				Assert("Must contain Discount charge", charges1.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.Discount));
			});

			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.CPT;
			invoiceHeader.JZ_InvoiceNumber = "34234";
			nfeImport = new NFEImportObjectParent(declaration);
			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlTextWithoutICMSTotvSeg,
				"(1/1) Inv. Header: 34234 - NF-e 420118 lines successfully imported into existing Inv. Header.", false, invoiceHeader.PK);

			invoiceLine1 = declaration.InvoiceLines[0];
			charges1 = invoiceLine1.Charges;
			CombineAssertions(() =>
			{
				Assert("Must contain OverseasFreight charge", charges1.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.OverseasFreight));
				Assert("Must NOT contain OverseasInsurance charge", !charges1.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.OverseasInsurance));
				Assert("Must contain OtherCharges charge", charges1.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.OtherCharges));
				Assert("Must contain Discount charge", charges1.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.Discount));
			});

			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.CPT;
			invoiceHeader.JZ_InvoiceNumber = "34234";
			nfeImport = new NFEImportObjectParent(declaration);
			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlTextWithoutICMSTotvOutro,
				"(1/1) Inv. Header: 34234 - NF-e 420118 lines successfully imported into existing Inv. Header.", false, invoiceHeader.PK);

			invoiceLine1 = declaration.InvoiceLines[0];
			charges1 = invoiceLine1.Charges;

			CombineAssertions(() =>
			{
				Assert("Must contain OverseasFreight charge", charges1.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.OverseasFreight));
				Assert("Must contain OverseasInsurance charge", charges1.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.OverseasInsurance));
				Assert("Must NOT contain OtherCharges charge", !charges1.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.OtherCharges));
				Assert("Must contain Discount charge", charges1.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.Discount));
			});

			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.CPT;
			invoiceHeader.JZ_InvoiceNumber = "34234";
			nfeImport = new NFEImportObjectParent(declaration);
			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlTextWithoutICMSTotvDesc,
				"(1/1) Inv. Header: 34234 - NF-e 420118 lines successfully imported into existing Inv. Header.", false, invoiceHeader.PK);

			invoiceLine1 = declaration.InvoiceLines[0];
			charges1 = invoiceLine1.Charges;
			CombineAssertions(() =>
			{
				Assert("Must contain OverseasFreight charge", charges1.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.OverseasFreight));
				Assert("Must contain OverseasInsurance charge", charges1.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.OverseasInsurance));
				Assert("Must contain OtherCharges charge", charges1.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.OtherCharges));
				Assert("Must NOT contain Discount charge", !charges1.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.Discount));
			});
		}

		public void TestDistributeValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.CPT;
			invoiceHeader.JZ_InvoiceNumber = "34234";

			using (var stream = GenerateStreamFromString(nfeXmlTextWithProductsAndFrete))
			{
				var parent = new NFEImportObjectParent(declaration);
				var result = parent.ProcessStreamAndAddNew(stream);
				parent.ImportInvoices();

				var invoiceLines = declaration.InvoiceLines.Cast<JobComInvoiceLine>();
				var linePrices = declaration.InvoiceLines.Cast<JobComInvoiceLine>().Select(e => (decimal)e.JI_LinePrice).ToArray();

				decimal[] GetChargeAmount(string chargeCode) => invoiceLines.SelectMany(e => e.Charges.GetCharge(chargeCode).Select(d => (decimal)d.J7_Amount)).ToArray();
				var freightCharges = GetChargeAmount(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight);
				var insuranceCharges = GetChargeAmount(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance);
				var otherCharges = GetChargeAmount(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges);
				var discountCharges = GetChargeAmount(Customs.Business.CustomsChargeTypeList.Codes.Discount);

				var xmlTotalPrice = result.NfeInvoiceAmount;
				var xmlTotalFreight = result.NfeTotalFreight;
				var xmlTotalInsurance = result.NfeTotalInsurance;
				var xmlTotalOtherCharges = result.NfeTotalOtherCharges;
				var xmlTotalDiscount = result.NfeTotalDiscount;
				var xmlTotalNetWeight = result.NfeNetWeight;
				var xmlTotalGrossWeight = result.NfeGrossWeight;

				CombineAssertions(() =>
				{
					AssertEquals("Total Xml Line Price", 111938.54m, xmlTotalPrice);
					AssertEquals("Total of the Line Price", xmlTotalPrice, linePrices.Sum());
					AssertContainsExactElementsInExactOrder("Distributed Line Prices", new[] { 3596.48m, 106993.35m, 1348.71m }, linePrices);

					AssertEquals("Total Xml Freight", 12191.38m, xmlTotalFreight);
					AssertEquals("Total Freight of the items", xmlTotalFreight, freightCharges.Sum());
					AssertContainsExactElementsInExactOrder("Distributed Freight Charges", new[] { 391.63m, 11652.91m, 146.84m }, freightCharges);

					AssertEquals("Total Xml Insurance", 100.58m, xmlTotalInsurance);
					AssertEquals("Total Insurance of the items", xmlTotalInsurance, insuranceCharges.Sum());
					AssertContainsExactElementsInExactOrder("Distributed Insurance Charges", new[] { 34.19m, 33.19m, 33.20m }, insuranceCharges);

					AssertEquals("Total Xml Other Charges", 30.18m, xmlTotalOtherCharges);
					AssertEquals("Total Other Charges of the items", xmlTotalOtherCharges, otherCharges.Sum());
					AssertContainsExactElementsInExactOrder("Distributed Other Charges", new[] { 10.06m, 10.06m, 10.06m }, otherCharges);

					AssertEquals("Total Xml Discount", 10.18m, xmlTotalDiscount);
					AssertEquals("Total Discount of the items", xmlTotalDiscount, discountCharges.Sum());
					AssertContainsExactElementsInExactOrder("Distributed Discount Charges", new[] { 3.40m, 3.39m, 3.39m }, discountCharges);
				});
			}
		}

		public void TestImportInvoices_SupplierID()
		{
			var supplierDeclaration = Factory.New<OrgHeader>();
			supplierDeclaration.OH_Code = "SUJOB";

			var supplierFromFile = Factory.New<OrgHeader>();
			supplierFromFile.OH_Code = "SUFILE";
			supplierFromFile.PrimaryRegistrationNumber.Number = "57012650000174";

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplierDeclaration.PK;

			var nfeImport = new NFEImportObjectParent(declaration);
			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlText, "(1/1) Inv. Header: 180666 - NF-e 180666 - New invoice created, lines successfully imported into Inv. Header.");

			var nfeImportObject = nfeImport.NFEImportObjectCollection.First() as NFEImportObject;
			var invoiceHeader = declaration.Invoices.FirstOrDefault();

			AssertEquals("NF-e Supplier CNPJ", "57012650000174", nfeImportObject.SupplierID);
			AssertEquals("Invoice Header Supplier should be", declaration.JE_OH_Supplier, invoiceHeader.JZ_OH_Supplier);

			supplierFromFile.OH_IsConsignor = true;
			declaration.Invoices.DeleteAll();
			nfeImport.NFEImportObjectCollection.RemoveAndDeleteAll();

			Factory.Save();

			nfeImport = new NFEImportObjectParent(declaration);
			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlText, "(1/1) Inv. Header: 180666 - NF-e 180666 - New invoice created, lines successfully imported into Inv. Header.");

			nfeImportObject = nfeImport.NFEImportObjectCollection.First() as NFEImportObject;
			invoiceHeader = declaration.Invoices.FirstOrDefault();

			AssertEquals("NF-e Supplier CNPJ", "57012650000174", nfeImportObject.SupplierID);
			AssertEquals("Invoice Header Supplier should be", supplierFromFile.PK, invoiceHeader.JZ_OH_Supplier);
		}

		public void TestImportInvoicesWithoutEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var nfeImport = new NFEImportObjectParent(declaration);
			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlText, "(1/1) Inv. Header: 180666 - NF-e 180666 - New invoice created, lines successfully imported into Inv. Header.");

			var nfeImportObject = nfeImport.NFEImportObjectCollection.First() as NFEImportObject;
			var invoiceHeader = declaration.Invoices.FirstOrDefault();

			var entryInstruction = declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertEquals("EntryInstructionPK", ZGuid.Empty, nfeImportObject.EntryInstructionPK);

				AssertEquals("A new NFE Entry Instruction created", 1, declaration.CustomsEntryInstructions.Count);
				AssertEquals("CEI_Description should be ", "Created from Import NF-e", entryInstruction.CEI_Description);
				AssertEquals("CEI_LegalDocument should be ", LegalDocumentList.Codes.ElectronicLogisticInvoice, entryInstruction.CEI_LegalDocument);

				var invoiceLine = declaration.InvoiceLines.FirstOrDefault() as JobComInvoiceLine;
				AssertEquals("JI_CEI should be ", entryInstruction.PK, invoiceLine.JI_CEI);
			});
		}

		public void TestImportInvoicesWithOneEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "ENTRY01";
			entryInstruction.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;

			var nfeImport = new NFEImportObjectParent(declaration);
			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlText, "(1/1) Inv. Header: 180666 - NF-e 180666 - New invoice created, lines successfully imported into Inv. Header.");

			var nfeImportObject = nfeImport.NFEImportObjectCollection.First() as NFEImportObject;

			CombineAssertions(() =>
			{
				AssertEquals("EntryInstructionPK", entryInstruction.PK, nfeImportObject.EntryInstructionPK);
				AssertEquals("No NFE Entry Instruction created", 1, declaration.CustomsEntryInstructions.Count);

				var invoiceLine = declaration.InvoiceLines.FirstOrDefault() as JobComInvoiceLine;
				AssertEquals("JI_CEI should be ", entryInstruction.PK, invoiceLine.JI_CEI);
			});
		}

		public void TestImportInvoicesManyEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Description = "ENTRY01";
			entryInstruction1.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Description = "ENTRY02";
			entryInstruction2.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;

			var nfeImport = new NFEImportObjectParent(declaration);
			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlText, "(1/1) Inv. Header: 180666 - NF-e 180666 - New invoice created, lines successfully imported into Inv. Header.");

			var nfeImportObject = nfeImport.NFEImportObjectCollection.First() as NFEImportObject;
			var entryInstruction = declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().Last();

			CombineAssertions(() =>
			{
				AssertEquals("EntryInstructionPK", ZGuid.Empty, nfeImportObject.EntryInstructionPK);
				AssertEquals("A new NFE Entry Instruction created", 3, declaration.CustomsEntryInstructions.Count);
				AssertEquals("CEI_Description should be ", "Created from Import NF-e", entryInstruction.CEI_Description);
				AssertEquals("CEI_LegalDocument should be ", LegalDocumentList.Codes.ElectronicLogisticInvoice, entryInstruction.CEI_LegalDocument);

				var invoiceLine = declaration.InvoiceLines.FirstOrDefault() as JobComInvoiceLine;
				AssertEquals("JI_CEI should be ", entryInstruction.PK, invoiceLine.JI_CEI);
			});
		}

		public void TestImportInvoicesOnStandaloneCommercialInvoice()
		{
			var invoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.CPT;
			invoiceHeader.JZ_InvoiceNumber = "34234";

			var declaration = new FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData as JobDeclaration;

			var nfeImport = new NFEImportObjectParent(declaration);
			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlText, "(1/1) Inv. Header: 34234 - NF-e 180666 lines successfully imported into existing Inv. Header.", false, invoiceHeader.PK);

			var nfeImportObject = nfeImport.NFEImportObjectCollection.First() as NFEImportObject;

			CombineAssertions(() =>
			{
				AssertEquals("EntryInstructionPK", ZGuid.Empty, nfeImportObject.EntryInstructionPK);

				var invoiceLine = invoiceHeader.InvoiceLines.FirstOrDefault() as JobComInvoiceLine;
				AssertEquals("JI_CEI should be ", ZGuid.Empty, invoiceLine?.JI_CEI ?? ZGuid.Empty);
				AssertEquals("CustomsEntryInstructions should be ", 0, declaration.CustomsEntryInstructions.Count);
			});
		}

		public void TestImportInvoicesWhenInvoiceHeaderPKIsNotEmpty()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.FOB;
			Factory.Save();

			var nfeImport = new NFEImportObjectParent(declaration);
			AssertProcessXmlAndImportInvoices(nfeImport, nfeXmlText, "(1/1) Inv. Header:  - NF-e 180666 lines successfully imported into existing Inv. Header.", false, invoiceHeader.PK);

			CombineAssertions(() =>
			{
				AssertEquals("JZ_InvoiceAmount should be ", 0m, invoiceHeader.JZ_InvoiceAmount);
				AssertEquals("JZ_Weight should be ", 0m, invoiceHeader.JZ_Weight);
				AssertEquals("JZ_NetWeight should be ", 0m, invoiceHeader.JZ_NetWeight);
			});
		}

		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NFEImportObjectParent(Factory.New<JobDeclaration>());
		}

		#endregion

		void AssertProcessXmlAndImportInvoices(NFEImportObjectParent nfeImport, string xml, string expectedMessage, bool addSecondXml = false, ZGuid invoiceHeaderPK = default(ZGuid))
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			using (var stream = GenerateStreamFromString(xml))
			using (var stream2 = GenerateStreamFromString(xml.Replace("NFe35200857012650000174550010001806661378259720", "NFe35200857012650000174550010001806661378259721").Replace("180666", "180667")))
			{
				var messageBuilder = new ZStringBuilder();
				nfeImport.InvoiceImported = (completedCount, totalCount, message) => messageBuilder.Append($"({completedCount}/{totalCount}) {message}");
				nfeImport.ProcessStreamAndAddNew(stream);
				if (invoiceHeaderPK.IsValid)
				{
					var nfeObject = nfeImport.NFEImportObjectCollection[0];
					nfeObject.InvoiceHeaderPK = invoiceHeaderPK;
				}
				if (addSecondXml)
				{
					nfeImport.ProcessStreamAndAddNew(stream2);
				}
				nfeImport.ImportInvoices();
				nfeImport.InvoiceImported = null;

				AssertEquals("Invoices imported message", expectedMessage, messageBuilder.ToStringWithNewLineBetweenAppends());
			}
		}

		Stream GenerateStreamFromString(string s)
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(s);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		#region XMLTestConstants

		const string nfeXmlText = @"<?xml version='1.0' encoding='UTF-8'?>
<nfeProc versao = '4.00' xmlns = 'http://www.portalfiscal.inf.br/nfe'>
	<NFe xmlns = 'http://www.portalfiscal.inf.br/nfe'>
		<infNFe versao = '4.00' Id = 'NFe35200857012650000174550010001806661378259720'>
			<ide>
				<cUF>35</cUF>
				<cNF>37825972</cNF>
				<natOp>Venda de producao</natOp>
				<mod>55</mod>
				<serie>1</serie>
				<nNF>180666</nNF>
				<dhEmi>2021-05-02T06:19:00-03:00</dhEmi>
				<dhSaiEnt>2021-05-02T06:19:00-03:00</dhSaiEnt>
				<tpNF>1</tpNF>
				<idDest>3</idDest>
				<cMunFG>3550308</cMunFG>
				<tpImp>1</tpImp>
				<tpEmis>1</tpEmis>
				<cDV>0</cDV>
				<tpAmb>1</tpAmb>
				<finNFe>1</finNFe>
				<indFinal>1</indFinal>
				<indPres>0</indPres>
				<procEmi>0</procEmi>
				<verProc>4.00</verProc>
			</ide>
			<emit>
				<CNPJ>57012650000174</CNPJ>
				<xNome>PINGUIM IND.E COM. DE RADIADORES EIRELI</xNome>
				<xFant>PINGUIM</xFant>
				<enderEmit>
					<xLgr>RUA MADALENA DE MADUREIRA</xLgr>
					<nro>151</nro>
					<xBairro>LIMAO</xBairro>
					<cMun>3550308</cMun>
					<xMun>Sao Paulo</xMun>
					<UF>SP</UF>
					<CEP>02551040</CEP>
					<cPais>1058</cPais>
					<xPais>BRASIL</xPais>
					<fone>1138566440</fone>
				</enderEmit>
				<IE>111759209113</IE>
				<CRT>3</CRT>
			</emit>
			<dest>
				<idEstrangeiro/>
				<xNome>FELIX RUIZ PADILLA NIT. 3222655014</xNome>
				<enderDest>
					<xLgr>AV SANTA CRUZ</xLgr>
					<nro>2410</nro>
					<xBairro>VILLA VICTORIA</xBairro>
					<cMun>9999999</cMun>
					<xMun>EXTERIOR</xMun>
					<UF>EX</UF>
					<CEP>17201970</CEP>
					<cPais>0973</cPais>
					<xPais>BOLIVIA, ESTADO PLURINACIONAL DA</xPais>
					<fone>1333462299</fone>
				</enderDest>
				<indIEDest>9</indIEDest>
				<email>FALECONOSCO.BR @DHL.COM</email>
			</dest>
			<det nItem = '1'>
				<prod>
					<cProd>FM3555</cProd>
					<cEAN/>
					<xProd>COLMEIA ESPECIAL 890 X 910 X 7TBC / 8APP ESTANHADA</xProd>
					<NCM>87089100</NCM>
					<CFOP>7101</CFOP>
					<uCom>PC</uCom>
					<qCom>4.0000</qCom>
					<vUnCom>12421.7880000000</vUnCom>
					<vProd>49687.15</vProd>
					<cEANTrib/>
					<uTrib>UN</uTrib>
					<qTrib>4.0000</qTrib>
					<vUnTrib>12421.7880000000</vUnTrib>
					<vFrete>100</vFrete>
					<vSeg>50</vSeg>
					<vOutro>10.00</vOutro>
					<vDesc>55.00</vDesc>
					<indTot>1</indTot>
					<xPed>FC 04.20</xPed>
					<nItemPed>1</nItemPed>
				</prod>
				<imposto>
					<ICMS>
						<ICMS40>
							<orig>0</orig>
							<CST>41</CST>
						</ICMS40>
					</ICMS>
					<IPI>
						<cEnq>999</cEnq>
						<IPINT>
							<CST>53</CST>
						</IPINT>
					</IPI>
					<PIS>
						<PISNT>
							<CST>09</CST>
						</PISNT>
					</PIS>
					<COFINS>
						<COFINSNT>
							<CST>09</CST>
						</COFINSNT>
					</COFINS>
				</imposto>
				<infAdProd>COMPLEMENTARY</infAdProd>
			</det>
			<total>
				<ICMSTot>
					<vBC>0.00</vBC>
					<vICMS>0.00</vICMS>
					<vICMSDeson>0.00</vICMSDeson>
					<vFCP>0.00</vFCP>
					<vBCST>0.00</vBCST>
					<vST>0.00</vST>
					<vFCPST>0.00</vFCPST>
					<vFCPSTRet>0.00</vFCPSTRet>
					<vProd>49687.15</vProd>
					<vFrete>158.96</vFrete>
					<vSeg>1.00</vSeg>
					<vDesc>1.00</vDesc>
					<vII>0.00</vII>
					<vIPI>0.00</vIPI>
					<vIPIDevol>0.00</vIPIDevol>
					<vPIS>0.00</vPIS>
					<vCOFINS>0.00</vCOFINS>
					<vOutro>0.00</vOutro>
					<vNF>49687.15</vNF>
				</ICMSTot>
			</total>
			<transp>
				<modFrete>1</modFrete>
				<transporta>
					<CNPJ>58890252000113</CNPJ>
					<xNome>DHL EXPRESS(BRASIL) LTDA</xNome>
					<IE>109746831115</IE>
					<xEnder>AVENIDA MANUEL BANDEIRA,291-VILA LEOPOLDINA</xEnder>
					<xMun>Guarulhos</xMun>
					<UF>SP</UF>
				</transporta>
				<vol>
					<qVol>4</qVol>
					<esp>CAIXAS</esp>
					<nVol>4,0000</nVol>
					<pesoL>240.000</pesoL>
					<pesoB>280.000</pesoB>
				</vol>
			</transp>
			<cobr>
				<fat>
					<nFat>180666</nFat>
					<vOrig>49687.15</vOrig>
					<vLiq>49687.15</vLiq>
				</fat>
				<dup>
					<nDup>001</nDup>
					<dVenc>2020-08-26</dVenc>
					<vDup>49687.15</vDup>
				</dup>
			</cobr>
			<pag>
				<detPag>
					<tPag>15</tPag>
					<vPag>49687.15</vPag>
				</detPag>
			</pag>
			<infAdic>
				<infCpl>(Valor aproximado dos impostos: )(Pedido: 151640)(Vendedor: 8-PINGUIM)(Vendedor: 909-MARIO VIOTTI)(Cobranca: O MESMO(13) 3346 2299)(Entrega: O MESMO)(Pedido Cliente: FC 04.20)(Cidade: VILLA VICTORIA- BOL)</infCpl>
			</infAdic>
			<exporta>
				<UFSaidaPais>SP</UFSaidaPais>
				<xLocExporta>SAO PAULO</xLocExporta>
			</exporta>
		</infNFe>
		<Signature xmlns = 'http://www.w3.org/2000/09/xmldsig#'>
			<SignedInfo>
				<CanonicalizationMethod Algorithm = 'http://www.w3.org/TR/2001/REC-xml-c14n-20010315'/>
				<SignatureMethod Algorithm = 'http://www.w3.org/2000/09/xmldsig#rsa-sha1'/>
				<Reference URI = '#NFe35200857012650000174550010001806661378259720'>
					<Transforms>
						<Transform Algorithm = 'http://www.w3.org/2000/09/xmldsig#enveloped-signature'/>
						<Transform Algorithm = 'http://www.w3.org/TR/2001/REC-xml-c14n-20010315'/>
					</Transforms>
					<DigestMethod Algorithm = 'http://www.w3.org/2000/09/xmldsig#sha1'/>
					<DigestValue>OdEvCQSz9b / a30LqK8yIgCc + mgo =</DigestValue>
				</Reference>
			</SignedInfo>
			<SignatureValue>avYzy + HnzQXUAfKwqNv / YIjnZOB + sBHJCuhXvWcJDfveqxE0ikPtgaXaEEN4it4L67 + Uzqvzwm5QBfZ7FXMm1MkkENqbZ7SobyZnbYZp2Yzy3C92LEliB2dVWclM1H2bf + KLWa / AOCur5yXpg54Q0kXomAE6ZV7VdK5GtZwiL5I =</SignatureValue>
			<KeyInfo>
				<X509Data>
				<X509Certificate>MIICOTCCAaKgAwIBAgIQUXn + h7UWga1DGzpPjVYJZDANBgkqhkiG9w0BAQUFADBbMVkwVwYDVQQDHlAAdwB3AHcALgBmAHMAaQBzAHQALgBjAG8AbQAuAGIAcgAgACgAUwBFAE0AIABWAEEATABJAEQAQQBEAEUAIABKAFUAUgDNAEQASQBDAEEAKTAeFw0yMDA4MTkwOTI3MDZaFw0yMzA4MTkwOTI3MDZaMFsxWTBXBgNVBAMeUAB3AHcAdwAuAGYAcwBpAHMAdAAuAGMAbwBtAC4AYgByACAAKABTAEUATQAgAFYAQQBMAEkARABBAEQARQAgAEoAVQBSAM0ARABJAEMAQQApMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDVuzPKD5jupb87mp0A11qpowBTokuORSdopo6YbwvK9eyM0wi3P0qu1UuuVLSQTnHiKMdWEoEWvBmvMYKXpfvQWgAoyJb + IfI21edIrs / UjRxzaBsq8Ui / J9EAcACt60nAFmWMS3U / ZqIqxmv5VDyb + DE / DU9ZOlQ + KnWcE3o7vQIDAQABMA0GCSqGSIb3DQEBBQUAA4GBABV4U5v2ZnFKblaCRGWLZp / jaupbYpzHyYLSl7ur / 7eLYH8vpK9JPKcnLekNvbrT224FuBfo1tRHV3vAH + lAWQKwsLNJByRHH4 / 5LmYVzIZCWLwWRUapANeouRYSGY7Ye9RuCK1T2I1RmrIBaG7uNFT + d9BXA / +bKcJ5hjYShOs8</X509Certificate>
				</X509Data>
			</KeyInfo>
		</Signature>
	</NFe>
	<protNFe versao = '4.00'>
		<infProt>
			<tpAmb>1</tpAmb>
			<verAplic>4.00</verAplic>
			<chNFe>35200857012650000174550010001806661378259720</chNFe>
			<dhRecbto>2020-08-26T16:21:55-03:00</dhRecbto>
			<nProt>135200743266554</nProt>
			<digVal>AOmRVlRpJQunBxGWoV9ZC3A1+Zo=</digVal>
			<cStat>100</cStat>
			<xMotivo>Autorizado o uso da NF-e</xMotivo>
		</infProt>
	</protNFe>
</nfeProc>
";

		const string nfeXmlTextWithProductsAndFrete = @"<?xml version='1.0' encoding='utf-8'?>
<nfeProc xmlns='http://www.portalfiscal.inf.br/nfe' versao='4.00'>
	<NFe xmlns='http://www.portalfiscal.inf.br/nfe'>
		<infNFe versao='4.00' Id='NFe31200743790666000101550020000033441878442667'>
			<ide>
				<cUF>31</cUF>
				<cNF>87844266</cNF>
				<natOp>Venda de produção do estabelecimento</natOp>
				<mod>55</mod>
				<serie>2</serie>
				<nNF>3344</nNF>
				<dhEmi>2021-05-21T15:42:46-03:00</dhEmi>
				<dhSaiEnt>2020-07-16T18:42:08-03:00</dhSaiEnt>
				<tpNF>1</tpNF>
				<idDest>3</idDest>
				<cMunFG>3118601</cMunFG>
				<tpImp>1</tpImp>
				<tpEmis>1</tpEmis>
				<cDV>7</cDV>
				<tpAmb>1</tpAmb>
				<finNFe>1</finNFe>
				<indFinal>0</indFinal>
				<indPres>9</indPres>
				<procEmi>0</procEmi>
				<verProc>1,00</verProc>
			</ide>
			<emit>
				<CNPJ>43790666000101</CNPJ>
				<xNome>MAGOTTEAUX BRASIL LTDA</xNome>
				<xFant>MAGOTTEAUX BRASIL LTDA</xFant>
				<enderEmit>
					<xLgr>Avenida General David Sarnoff</xLgr>
					<nro>1221</nro>
					<xBairro>Cidade Industrial</xBairro>
					<cMun>3118601</cMun>
					<xMun>Contagem</xMun>
					<UF>MG</UF>
					<CEP>32210110</CEP>
					<xPais>Brasil</xPais>
					<fone>3121918901</fone>
				</enderEmit>
				<IE>1861530820092</IE>
				<IM>52605019</IM>
				<CNAE>2451200</CNAE>
				<CRT>3</CRT>
			</emit>
			<dest>
				<idEstrangeiro>78803130-0</idEstrangeiro>
				<xNome>MAGOTTEAUX ANDINO S.A.</xNome>
				<enderDest>
					<xLgr>Panamericana Norte KM 37</xLgr>
					<nro>S/N</nro>
					<xBairro>EXTERIOR</xBairro>
					<cMun>9999999</cMun>
					<xMun>EXTERIOR</xMun>
					<UF>EX</UF>
					<CEP>00000000</CEP>
					<cPais>1589</cPais>
					<xPais>Chile</xPais>
				</enderDest>
				<indIEDest>9</indIEDest>
			</dest>
			<entrega>
				<CNPJ/>
				<xNome>PLANTA LA CALERA</xNome>
				<xLgr>IGNACIO CARRERA PINTO</xLgr>
				<nro>32</nro>
				<xBairro>EXTERIOR</xBairro>
				<cMun>9999999</cMun>
				<xMun>EXTERIOR</xMun>
				<UF>EX</UF>
				<CEP>00000000</CEP>
				<cPais>1589</cPais>
				<xPais>Chile</xPais>
			</entrega>
			<det nItem='1'>
				<prod>
					<cProd>000000000005000041</cProd>
					<cEAN>SEM GTIN</cEAN>
					<xProd>ESFERAS MOEDORAS EM AÇO FUNDIDO 40MM HAR</xProd>
					<NCM>73259100</NCM>
					<cBenef>PR800002</cBenef>
					<CFOP>7101</CFOP>
					<uCom>KG</uCom>
					<qCom>800.0000</qCom>
					<vUnCom>4.4955500000</vUnCom>
					<vProd>3596.44</vProd>
					<cEANTrib>SEM GTIN</cEANTrib>
					<uTrib>KG</uTrib>
					<qTrib>800.0000</qTrib>
					<vUnTrib>4.4955500000</vUnTrib>
					<vFrete>391.67</vFrete>
					<vSeg>34.52</vSeg>
					<vOutro>10.07</vOutro>
					<vDesc>3.39</vDesc>
					<indTot>1</indTot>
					<xPed>20004928</xPed>
				</prod>
				<imposto>
					<ICMS>
						<ICMS40>
							<orig>0</orig>
							<CST>41</CST>
						</ICMS40>
					</ICMS>
					<IPI>
						<cEnq>002</cEnq>
						<IPINT>
							<CST>54</CST>
						</IPINT>
					</IPI>
					<PIS>
						<PISNT>
							<CST>08</CST>
						</PISNT>
					</PIS>
					<COFINS>
						<COFINSNT>
							<CST>08</CST>
						</COFINSNT>
					</COFINS>
				</imposto>
				<infAdProd>Pedido 20004928 Item</infAdProd>
			</det>
			<det nItem='2'>
				<prod>
					<cProd>000000000005000040</cProd>
					<cEAN>SEM GTIN</cEAN>
					<xProd>ESFERAS MOEDORAS EM AÇO FUNDIDO 30MM HAR</xProd>
					<NCM>73259100</NCM>
					<cBenef>PR800002</cBenef>
					<CFOP>7101</CFOP>
					<uCom>KG</uCom>
					<qCom>23800.0000</qCom>
					<vUnCom>4.4955176471</vUnCom>
					<vProd>106993.32</vProd>
					<cEANTrib>SEM GTIN</cEANTrib>
					<uTrib>KG</uTrib>
					<qTrib>23800.0000</qTrib>
					<vUnTrib>4.4955176471</vUnTrib>
					<vFrete>11652.94</vFrete>
					<vSeg>33.52</vSeg>
					<vOutro>10.07</vOutro>
					<vDesc>3.39</vDesc>
					<indTot>1</indTot>
					<xPed>20004928</xPed>
				</prod>
				<imposto>
					<ICMS>
						<ICMS40>
							<orig>0</orig>
							<CST>41</CST>
						</ICMS40>
					</ICMS>
					<IPI>
						<cEnq>002</cEnq>
						<IPINT>
							<CST>54</CST>
						</IPINT>
					</IPI>
					<PIS>
						<PISNT>
							<CST>08</CST>
						</PISNT>
					</PIS>
					<COFINS>
						<COFINSNT>
							<CST>08</CST>
						</COFINSNT>
					</COFINS>
				</imposto>
				<infAdProd>Pedido 20004928 Item</infAdProd>
			</det>
			<det nItem='3'>
				<prod>
					<cProd>000000000005000035</cProd>
					<cEAN>SEM GTIN</cEAN>
					<xProd>ESFERAS MOEDORAS EM AÇO FUNDIDO 20MM HAR</xProd>
					<NCM>73259100</NCM>
					<cBenef>PR800002</cBenef>
					<CFOP>7101</CFOP>
					<uCom>KG</uCom>
					<qCom>300.0000</qCom>
					<vUnCom>4.4956000000</vUnCom>
					<vProd>1348.68</vProd>
					<cEANTrib>SEM GTIN</cEANTrib>
					<uTrib>KG</uTrib>
					<qTrib>300.0000</qTrib>
					<vUnTrib>4.4956000000</vUnTrib>
					<vFrete>146.87</vFrete>
					<vSeg>33.52</vSeg>
					<vOutro>10.07</vOutro>
					<vDesc>3.39</vDesc>
					<indTot>1</indTot>
					<xPed>20004928</xPed>
				</prod>
				<imposto>
					<ICMS>
						<ICMS40>
							<orig>0</orig>
							<CST>41</CST>
						</ICMS40>
					</ICMS>
					<IPI>
						<cEnq>002</cEnq>
						<IPINT>
							<CST>54</CST>
						</IPINT>
					</IPI>
					<PIS>
						<PISNT>
							<CST>08</CST>
						</PISNT>
					</PIS>
					<COFINS>
						<COFINSNT>
							<CST>08</CST>
						</COFINSNT>
					</COFINS>
				</imposto>
				<infAdProd>Pedido 20004928 Item</infAdProd>
			</det>
			<total>
				<ICMSTot>
					<vBC>0.00</vBC>
					<vICMS>0.00</vICMS>
					<vICMSDeson>0.00</vICMSDeson>
					<vFCP>0.00</vFCP>
					<vBCST>0.00</vBCST>
					<vST>0.00</vST>
					<vFCPST>0.00</vFCPST>
					<vFCPSTRet>0.00</vFCPSTRet>
					<vProd>111938.54</vProd>
					<vFrete>12191.38</vFrete>
					<vSeg>100.58</vSeg>
					<vDesc>10.18</vDesc>
					<vII>0.00</vII>
					<vIPI>0.00</vIPI>
					<vIPIDevol>0.00</vIPIDevol>
					<vPIS>0.00</vPIS>
					<vCOFINS>0.00</vCOFINS>
					<vOutro>30.18</vOutro>
					<vNF>124129.92</vNF>
				</ICMSTot>
			</total>
			<transp>
				<modFrete>0</modFrete>
				<vol>
					<qVol>16</qVol>
					<esp>Pallets</esp>
					<nVol>00016</nVol>
					<pesoL>24900.000</pesoL>
					<pesoB>25828.000</pesoB>
				</vol>
			</transp>
			<cobr>
				<fat>
					<nFat>0000334401</nFat>
					<vOrig>124129.91</vOrig>
					<vDesc>0.00</vDesc>
					<vLiq>124129.91</vLiq>
				</fat>
				<dup>
					<nDup>001</nDup>
					<dVenc>2020-08-15</dVenc>
					<vDup>124129.91</vDup>
				</dup>
			</cobr>
			<pag>
				<detPag>
					<tPag>99</tPag>
					<vPag>124129.92</vPag>
				</detPag>
			</pag>
			<infAdic>
				<infAdFisco>NAO INCIDENCIA DE ICMS CONFORME ARTIGO 5 INCISO III DA PARTE GERAL DO. DECRETO 43.080 2006..</infAdFisco>
				<infCpl>PORTO DE EMBARQUE RIO DE JANEIRO LOCAL DE ENTREGA DESEMBARACO TTC.LOGISTICA LTDA. TERMINAL DE EMBARQUE LIBRA TERMINAIS TAXA DE.CONVERSAO USD 5 3485 DE 15 07 2020..ESFERAS MOEDORAS EM ACO FUNDIDO 40MM HARDALLOY B PARA MOINHO DE CIMENTO.ESFERAS MOEDORAS EM ACO FUNDIDO 30MM HARDALLOY B PARA MOINHO DE CIMENTO.ESFERAS MOEDORAS EM ACO FUNDIDO 20MM HARDALLOY B PARA MOINHO DE CIMENTO.DADOS BANCARIOS BANCO SANTANDER BRASIL S A 033 AGENCIA 3097. CONTA CORRENTE 13001384 03.</infCpl>
			</infAdic>
			<exporta>
				<UFSaidaPais>RJ</UFSaidaPais>
				<xLocExporta>Rio de Janeiro</xLocExporta>
			</exporta>
		</infNFe>
	</NFe>
</nfeProc>
";

		const string nfeXmlTextWithProductsAndFreteAndSegAndOutro = @"<?xml version='1.0' encoding='UTF-8'?>
<nfeProc versao='4.00' xmlns='http://www.portalfiscal.inf.br/nfe'>
	<NFe xmlns='http://www.portalfiscal.inf.br/nfe'>
		<infNFe versao='4.00' Id='NFe35210100857758000736550110004201181945721521'>
			<ide>
				<cUF>35</cUF>
				<cNF>94572152</cNF>
				<natOp>7.101 - Venda de producao do estabelecimento</natOp>
				<mod>55</mod>
				<serie>11</serie>
				<nNF>420118</nNF>
				<dhEmi>2021-01-04T11:33:58-02:00</dhEmi>
				<dhSaiEnt>2021-01-04T11:33:58-02:00</dhSaiEnt>
				<tpNF>1</tpNF>
				<idDest>3</idDest>
				<cMunFG>3524907</cMunFG>
				<tpImp>1</tpImp>
				<tpEmis>1</tpEmis>
				<cDV>1</cDV>
				<tpAmb>1</tpAmb>
				<finNFe>1</finNFe>
				<indFinal>0</indFinal>
				<indPres>9</indPres>
				<procEmi>0</procEmi>
				<verProc>QAD</verProc>
			</ide>
			<emit>
				<CNPJ>00857758000736</CNPJ>
				<xNome>APTIV MANUFATURA E SERVICOS DE DISTRIBUICaO LTDA</xNome>
				<xFant>APTIV MANUFATURA E SERVICOS</xFant>
				<enderEmit>
					<xLgr>ROD DOS TAMOIOS</xLgr>
					<nro>S/N</nro>
					<xCpl>KM 21,8</xCpl>
					<xBairro>TAPANHAO</xBairro>
					<cMun>3524907</cMun>
					<xMun>JAMBEIRO</xMun>
					<UF>SP</UF>
					<CEP>12270000</CEP>
					<cPais>1058</cPais>
					<xPais>BRASIL</xPais>
					<fone>1239782042</fone>
				</enderEmit>
				<IE>397001817113</IE>
				<CRT>3</CRT>
			</emit>
			<dest>
				<idEstrangeiro>95001407</idEstrangeiro>
				<xNome>NIPPON EXPRESS DEUTSCHLAND</xNome>
				<enderDest>
					<xLgr>ERFURTER STRASSE 2</xLgr>
					<nro>_</nro>
					<xBairro>__</xBairro>
					<cMun>9999999</cMun>
					<xMun>EXTERIOR</xMun>
					<UF>EX</UF>
					<cPais>0230</cPais>
					<xPais>GERMANY</xPais>
				</enderDest>
				<indIEDest>9</indIEDest>
			</dest>
			<entrega>
				<CNPJ/>
				<xNome>NIPPON EXPRESS DEUTSCHLAND</xNome>
				<xLgr>ERFURTER STRASSE 2</xLgr>
				<nro>ERFURTER STRASSE 2</nro>
				<xBairro>__</xBairro>
				<cMun>9999999</cMun>
				<xMun>ECHING</xMun>
				<UF>EX</UF>
				<CEP>00085386</CEP>
				<cPais>230</cPais>
				<xPais>GERMANY</xPais>
			</entrega>
			<det nItem='1'>
				<prod>
					<cProd>15364304</cProd>
					<cEAN>SEM GTIN</cEAN>
					<xProd>TERMINAL ELETRICO</xProd>
					<NCM>85369090</NCM>
					<CEST>0199900</CEST>
					<indEscala>S</indEscala>
					<CFOP>7101</CFOP>
					<uCom>PC</uCom>
					<qCom>8000.0000</qCom>
					<vUnCom>0.2800700000</vUnCom>
					<vProd>2240.56</vProd>
					<cEANTrib>SEM GTIN</cEANTrib>
					<uTrib>UN</uTrib>
					<qTrib>8000.0000</qTrib>
					<vUnTrib>0.2800700000</vUnTrib>
					<vFrete>753.40</vFrete>
					<vSeg>0.69</vSeg>
					<vOutro>222.69</vOutro>
					<indTot>1</indTot>
					<nFCI>B44FD755-70D2-4D41-8CE3-1757482C8E66</nFCI>
				</prod>
				<imposto>
					<ICMS>
						<ICMS40>
							<orig>3</orig>
							<CST>41</CST>
						</ICMS40>
					</ICMS>
					<IPI>
						<cEnq>002</cEnq>
						<IPINT>
							<CST>54</CST>
						</IPINT>
					</IPI>
					<PIS>
						<PISNT>
							<CST>07</CST>
						</PISNT>
					</PIS>
					<COFINS>
						<COFINSNT>
							<CST>07</CST>
						</COFINSNT>
					</COFINS>
				</imposto>
			</det>
			<det nItem='2'>
				<prod>
					<cProd>15364304</cProd>
					<cEAN>SEM GTIN</cEAN>
					<xProd>TERMINAL ELETRICO</xProd>
					<NCM>85369090</NCM>
					<CEST>0199900</CEST>
					<indEscala>S</indEscala>
					<CFOP>7101</CFOP>
					<uCom>PC</uCom>
					<qCom>4000.0000</qCom>
					<vUnCom>0.2800700000</vUnCom>
					<vProd>1120.28</vProd>
					<cEANTrib>SEM GTIN</cEANTrib>
					<uTrib>UN</uTrib>
					<qTrib>4000.0000</qTrib>
					<vUnTrib>0.2800700000</vUnTrib>
					<vFrete>376.70</vFrete>
					<vSeg>0.35</vSeg>
					<vOutro>111.35</vOutro>
					<indTot>1</indTot>
					<nFCI>B44FD755-70D2-4D41-8CE3-1757482C8E66</nFCI>
				</prod>
				<imposto>
					<ICMS>
						<ICMS40>
							<orig>3</orig>
							<CST>41</CST>
						</ICMS40>
					</ICMS>
					<IPI>
						<cEnq>002</cEnq>
						<IPINT>
							<CST>54</CST>
						</IPINT>
					</IPI>
					<PIS>
						<PISNT>
							<CST>07</CST>
						</PISNT>
					</PIS>
					<COFINS>
						<COFINSNT>
							<CST>07</CST>
						</COFINSNT>
					</COFINS>
				</imposto>
			</det>
			<det nItem='3'>
				<prod>
					<cProd>15364304</cProd>
					<cEAN>SEM GTIN</cEAN>
					<xProd>TERMINAL ELETRICO</xProd>
					<NCM>85369090</NCM>
					<CEST>0199900</CEST>
					<indEscala>S</indEscala>
					<CFOP>7101</CFOP>
					<uCom>PC</uCom>
					<qCom>24000.0000</qCom>
					<vUnCom>0.2800695833</vUnCom>
					<vProd>6721.67</vProd>
					<cEANTrib>SEM GTIN</cEANTrib>
					<uTrib>UN</uTrib>
					<qTrib>24000.0000</qTrib>
					<vUnTrib>0.2800695833</vUnTrib>
					<vFrete>2260.19</vFrete>
					<vSeg>2.07</vSeg>
					<vOutro>668.07</vOutro>
					<indTot>1</indTot>
					<nFCI>B44FD755-70D2-4D41-8CE3-1757482C8E66</nFCI>
				</prod>
				<imposto>
					<ICMS>
						<ICMS40>
							<orig>3</orig>
							<CST>41</CST>
						</ICMS40>
					</ICMS>
					<IPI>
						<cEnq>002</cEnq>
						<IPINT>
							<CST>54</CST>
						</IPINT>
					</IPI>
					<PIS>
						<PISNT>
							<CST>07</CST>
						</PISNT>
					</PIS>
					<COFINS>
						<COFINSNT>
							<CST>07</CST>
						</COFINSNT>
					</COFINS>
				</imposto>
			</det>
			<total>
				<ICMSTot>
					<vBC>0.00</vBC>
					<vICMS>0.00</vICMS>
					<vICMSDeson>0.00</vICMSDeson>
					<vFCP>0.00</vFCP>
					<vBCST>0.00</vBCST>
					<vST>0.00</vST>
					<vFCPST>0.00</vFCPST>
					<vFCPSTRet>0.00</vFCPSTRet>
					<vProd>15683.91</vProd>
					<vFrete>3390.29</vFrete>
					<vSeg>3.11</vSeg>
					<vDesc>1.00</vDesc>
					<vII>0.00</vII>
					<vIPI>0.00</vIPI>
					<vIPIDevol>0.00</vIPIDevol>
					<vPIS>0.00</vPIS>
					<vCOFINS>0.00</vCOFINS>
					<vOutro>1002.11</vOutro>
					<vNF>22521.35</vNF>
				</ICMSTot>
			</total>
			<transp>
				<modFrete>0</modFrete>
				<transporta>
					<CNPJ>58890252000113</CNPJ>
					<xNome>DHL WORLD WIDE EXPRESS LTDA</xNome>
					<IE>109746831115</IE>
					<xEnder>RUA SANTA MARINA, 1660</xEnder>
					<xMun>SAO PAULO</xMun>
					<UF>SP</UF>
				</transporta>
				<vol>
					<qVol>1</qVol>
					<esp>PALLET DE MADEIRA</esp>
					<pesoL>95.200</pesoL>
					<pesoB>133.000</pesoB>
				</vol>
			</transp>
			<cobr>
				<fat>
					<nFat>FT245192</nFat>
					<vOrig>22521.35</vOrig>
					<vLiq>22521.35</vLiq>
				</fat>
				<dup>
					<nDup>001</nDup>
					<dVenc>2021-03-22</dVenc>
					<vDup>22521.35</vDup>
				</dup>
			</cobr>
			<pag>
				<detPag>
					<indPag>1</indPag>
					<tPag>99</tPag>
					<vPag>22521.35</vPag>
				</detPag>
			</pag>
			<infAdic>
				<infAdFisco>NAO INCIDENCIA DO ICMS CONFORME ARTIGO 7, INCISO V DO DECRETO 45.490/00IMUNE DE IPI CONFORME ART 18, INCISO II DO DECR. N. 7.212/10</infAdFisco>
				<infCpl>LINHA 1: 3273412LINHA 2: 3282159LINHA 3: 3285523LINHA 4: 3287551LINHA 5: 3308438LINHA 6: 3308998LINHA 7: 3311800...VISTO OBTIDO ELETRONICAMENTE - RIEX - SEFAZ /SP...DSE - DECLARACAO SIMPLIFICADA DE EXPORTACAO - VALOR INFERIOR A US$ 50.000OU VALOR EQUIVALENTE EM OUTRA MOEDA ESTRANGEIRA...P.O.NR. 3273412 / 3282159 / 3285523 / 3287551 / 3308438 / 3308998 /3311800...COLETA PELA DHL WORLD WIDE EXPRESS LTDA...LOCAL DE DESTINO - ALEMANHA...M. EXP. = P202788 / TX. CAMBIAL = 5,1961...Frete Internacional 5.273,78...Outras Despesas 1.558,83...Seguro Internacional 4,83</infCpl>
			</infAdic>
			<exporta>
				<UFSaidaPais>SP</UFSaidaPais>
				<xLocExporta>JAMBEIRO</xLocExporta>
				<xLocDespacho>Aereo - Aeroporto de Viracopos</xLocDespacho>
			</exporta>
		</infNFe>
	</NFe>
</nfeProc>
";

		const string nfeXmlTextWithProductsAndFreteAndSegAndOutroAndDesc = @"<?xml version='1.0' encoding='UTF-8'?>
<nfeProc versao='4.00' xmlns='http://www.portalfiscal.inf.br/nfe'>
	<NFe xmlns='http://www.portalfiscal.inf.br/nfe'>
		<infNFe versao='4.00' Id='NFe35210100857758000736550110004201181945721521'>
			<ide>
				<cUF>35</cUF>
				<cNF>94572152</cNF>
				<natOp>7.101 - Venda de producao do estabelecimento</natOp>
				<mod>55</mod>
				<serie>11</serie>
				<nNF>420118</nNF>
				<dhEmi>2021-01-04T11:33:58-02:00</dhEmi>
				<dhSaiEnt>2021-01-04T11:33:58-02:00</dhSaiEnt>
				<tpNF>1</tpNF>
				<idDest>3</idDest>
				<cMunFG>3524907</cMunFG>
				<tpImp>1</tpImp>
				<tpEmis>1</tpEmis>
				<cDV>1</cDV>
				<tpAmb>1</tpAmb>
				<finNFe>1</finNFe>
				<indFinal>0</indFinal>
				<indPres>9</indPres>
				<procEmi>0</procEmi>
				<verProc>QAD</verProc>
			</ide>
			<emit>
				<CNPJ>00857758000736</CNPJ>
				<xNome>APTIV MANUFATURA E SERVICOS DE DISTRIBUICaO LTDA</xNome>
				<xFant>APTIV MANUFATURA E SERVICOS</xFant>
				<enderEmit>
					<xLgr>ROD DOS TAMOIOS</xLgr>
					<nro>S/N</nro>
					<xCpl>KM 21,8</xCpl>
					<xBairro>TAPANHAO</xBairro>
					<cMun>3524907</cMun>
					<xMun>JAMBEIRO</xMun>
					<UF>SP</UF>
					<CEP>12270000</CEP>
					<cPais>1058</cPais>
					<xPais>BRASIL</xPais>
					<fone>1239782042</fone>
				</enderEmit>
				<IE>397001817113</IE>
				<CRT>3</CRT>
			</emit>
			<dest>
				<idEstrangeiro>95001407</idEstrangeiro>
				<xNome>NIPPON EXPRESS DEUTSCHLAND</xNome>
				<enderDest>
					<xLgr>ERFURTER STRASSE 2</xLgr>
					<nro>_</nro>
					<xBairro>__</xBairro>
					<cMun>9999999</cMun>
					<xMun>EXTERIOR</xMun>
					<UF>EX</UF>
					<cPais>0230</cPais>
					<xPais>GERMANY</xPais>
				</enderDest>
				<indIEDest>9</indIEDest>
			</dest>
			<entrega>
				<CNPJ/>
				<xNome>NIPPON EXPRESS DEUTSCHLAND</xNome>
				<xLgr>ERFURTER STRASSE 2</xLgr>
				<nro>ERFURTER STRASSE 2</nro>
				<xBairro>__</xBairro>
				<cMun>9999999</cMun>
				<xMun>ECHING</xMun>
				<UF>EX</UF>
				<CEP>00085386</CEP>
				<cPais>230</cPais>
				<xPais>GERMANY</xPais>
			</entrega>
			<det nItem='1'>
				<prod>
					<cProd>15364304</cProd>
					<cEAN>SEM GTIN</cEAN>
					<xProd>TERMINAL ELETRICO</xProd>
					<NCM>85369090</NCM>
					<CEST>0199900</CEST>
					<indEscala>S</indEscala>
					<CFOP>7101</CFOP>
					<uCom>PC</uCom>
					<qCom>8000.0000</qCom>
					<vUnCom>0.2800700000</vUnCom>
					<vProd>2240.56</vProd>
					<cEANTrib>SEM GTIN</cEANTrib>
					<uTrib>UN</uTrib>
					<qTrib>8000.0000</qTrib>
					<vUnTrib>0.2800700000</vUnTrib>
					<vDesc>10.00</vDesc>
					<vFrete>753.40</vFrete>
					<vSeg>0.69</vSeg>
					<vOutro>222.69</vOutro>
					<indTot>1</indTot>
					<nFCI>B44FD755-70D2-4D41-8CE3-1757482C8E66</nFCI>
				</prod>
				<imposto>
					<ICMS>
						<ICMS40>
							<orig>3</orig>
							<CST>41</CST>
						</ICMS40>
					</ICMS>
					<IPI>
						<cEnq>002</cEnq>
						<IPINT>
							<CST>54</CST>
						</IPINT>
					</IPI>
					<PIS>
						<PISNT>
							<CST>07</CST>
						</PISNT>
					</PIS>
					<COFINS>
						<COFINSNT>
							<CST>07</CST>
						</COFINSNT>
					</COFINS>
				</imposto>
			</det>
			<total>
				<ICMSTot>
					<vBC>0.00</vBC>
					<vICMS>0.00</vICMS>
					<vICMSDeson>0.00</vICMSDeson>
					<vFCP>0.00</vFCP>
					<vBCST>0.00</vBCST>
					<vST>0.00</vST>
					<vFCPST>0.00</vFCPST>
					<vFCPSTRet>0.00</vFCPSTRet>
					<vProd>15683.91</vProd>
					<vFrete>753.40</vFrete>
					<vSeg>4.83</vSeg>
					<vDesc>1.00</vDesc>
					<vII>0.00</vII>
					<vIPI>0.00</vIPI>
					<vIPIDevol>0.00</vIPIDevol>
					<vPIS>0.00</vPIS>
					<vCOFINS>0.00</vCOFINS>
					<vOutro>1558.83</vOutro>
					<vNF>22521.35</vNF>
				</ICMSTot>
			</total>
			<transp>
				<modFrete>0</modFrete>
				<transporta>
					<CNPJ>58890252000113</CNPJ>
					<xNome>DHL WORLD WIDE EXPRESS LTDA</xNome>
					<IE>109746831115</IE>
					<xEnder>RUA SANTA MARINA, 1660</xEnder>
					<xMun>SAO PAULO</xMun>
					<UF>SP</UF>
				</transporta>
				<vol>
					<qVol>1</qVol>
					<esp>PALLET DE MADEIRA</esp>
					<pesoL>95.200</pesoL>
					<pesoB>133.000</pesoB>
				</vol>
			</transp>
			<cobr>
				<fat>
					<nFat>FT245192</nFat>
					<vOrig>22521.35</vOrig>
					<vLiq>22521.35</vLiq>
				</fat>
				<dup>
					<nDup>001</nDup>
					<dVenc>2021-03-22</dVenc>
					<vDup>22521.35</vDup>
				</dup>
			</cobr>
			<pag>
				<detPag>
					<indPag>1</indPag>
					<tPag>99</tPag>
					<vPag>22521.35</vPag>
				</detPag>
			</pag>
			<infAdic>
				<infAdFisco>NAO INCIDENCIA DO ICMS CONFORME ARTIGO 7, INCISO V DO DECRETO 45.490/00IMUNE DE IPI CONFORME ART 18, INCISO II DO DECR. N. 7.212/10</infAdFisco>
				<infCpl>LINHA 1: 3273412LINHA 2: 3282159LINHA 3: 3285523LINHA 4: 3287551LINHA 5: 3308438LINHA 6: 3308998LINHA 7: 3311800...VISTO OBTIDO ELETRONICAMENTE - RIEX - SEFAZ /SP...DSE - DECLARACAO SIMPLIFICADA DE EXPORTACAO - VALOR INFERIOR A US$ 50.000OU VALOR EQUIVALENTE EM OUTRA MOEDA ESTRANGEIRA...P.O.NR. 3273412 / 3282159 / 3285523 / 3287551 / 3308438 / 3308998 /3311800...COLETA PELA DHL WORLD WIDE EXPRESS LTDA...LOCAL DE DESTINO - ALEMANHA...M. EXP. = P202788 / TX. CAMBIAL = 5,1961...Frete Internacional 5.273,78...Outras Despesas 1.558,83...Seguro Internacional 4,83</infCpl>
			</infAdic>
			<exporta>
				<UFSaidaPais>SP</UFSaidaPais>
				<xLocExporta>JAMBEIRO</xLocExporta>
				<xLocDespacho>Aereo - Aeroporto de Viracopos</xLocDespacho>
			</exporta>
		</infNFe>
	</NFe>
</nfeProc>
";

		const string nfeXmlTextWithProductsAndDesc = @"<?xml version='1.0' encoding='UTF-8'?>
<nfeProc versao='4.00' xmlns='http://www.portalfiscal.inf.br/nfe'>
	<NFe xmlns='http://www.portalfiscal.inf.br/nfe'>
		<infNFe versao='4.00' Id='NFe23200601946719000182550010000039071000039088'>
			<ide>
				<cUF>23</cUF>
				<cNF>00003908</cNF>
				<natOp>VENDAS FORA DO PAIS</natOp>
				<mod>55</mod>
				<serie>1</serie>
				<nNF>3907</nNF>
				<dhEmi>2020-06-11T14:54:39-03:00</dhEmi>
				<dhSaiEnt>2020-06-11T14:54:39-03:00</dhSaiEnt>
				<tpNF>1</tpNF>
				<idDest>3</idDest>
				<cMunFG>2304400</cMunFG>
				<tpImp>1</tpImp>
				<tpEmis>1</tpEmis>
				<cDV>8</cDV>
				<tpAmb>1</tpAmb>
				<finNFe>1</finNFe>
				<indFinal>0</indFinal>
				<indPres>1</indPres>
				<procEmi>0</procEmi>
				<verProc>ALPHAIND2.7</verProc>
			</ide>
			<emit>
				<CNPJ>01946719000182</CNPJ>
				<xNome>ANDREA ALMEIDA IND. E COM. DE CONFECCOES LTDA</xNome>
				<xFant>CAUIPE LOJA EXPEDICIONARIOS</xFant>
				<enderEmit>
					<xLgr>RUA AV. EXPEDICIONARIOS</xLgr>
					<nro>5158</nro>
					<xBairro>MONTESE</xBairro>
					<cMun>2304400</cMun>
					<xMun>FORTALEZA</xMun>
					<UF>CE</UF>
					<CEP>60410234</CEP>
					<cPais>1058</cPais>
					<xPais>BRASIL</xPais>
					<fone>8532191944</fone>
				</enderEmit>
				<IE>063073170</IE>
				<CRT>1</CRT>
			</emit>
			<dest>
				<idEstrangeiro>541760933</idEstrangeiro>
				<xNome>TENDA URBANA COM GERAL (SU) LDA</xNome>
				<enderDest>
					<xLgr>AV. HO CHI MIN EDIFICIO DIPANDA TORRES A -5 D APARTADO 7310</xLgr>
					<nro>-</nro>
					<xBairro>APARTADO 7310</xBairro>
					<cMun>9999999</cMun>
					<xMun>EXTERIOR</xMun>
					<UF>EX</UF>
					<cPais>0400</cPais>
					<xPais>Angola</xPais>
				</enderDest>
				<indIEDest>9</indIEDest>
			</dest>
			<det nItem='1'>
				<prod>
					<cProd>1060612</cProd>
					<cEAN>SEM GTIN</cEAN>
					<xProd>BIQUINI FRUFRU SOLARIUM</xProd>
					<NCM>61124100</NCM>
					<CFOP>7101</CFOP>
					<uCom>UN</uCom>
					<qCom>10.0000</qCom>
					<vUnCom>69.9000000000</vUnCom>
					<vProd>699.00</vProd>
					<cEANTrib>SEM GTIN</cEANTrib>
					<uTrib>UN</uTrib>
					<qTrib>10.0000</qTrib>
					<vUnTrib>69.9000000000</vUnTrib>
					<vDesc>34.95</vDesc>
					<indTot>1</indTot>
				</prod>
				<imposto>
					<vTotTrib>219.84</vTotTrib>
					<ICMS>
						<ICMSSN102>
							<orig>0</orig>
							<CSOSN>300</CSOSN>
						</ICMSSN102>
					</ICMS>
					<PIS>
						<PISNT>
							<CST>07</CST>
						</PISNT>
					</PIS>
					<COFINS>
						<COFINSNT>
							<CST>07</CST>
						</COFINSNT>
					</COFINS>
				</imposto>
			</det>
			<det nItem='2'>
				<prod>
					<cProd>1112011</cProd>
					<cEAN>SEM GTIN</cEAN>
					<xProd>BIQUINI CORTINA SOLSTICIO</xProd>
					<NCM>61124100</NCM>
					<CFOP>7101</CFOP>
					<uCom>UN</uCom>
					<qCom>4.0000</qCom>
					<vUnCom>64.9000000000</vUnCom>
					<vProd>259.60</vProd>
					<cEANTrib>SEM GTIN</cEANTrib>
					<uTrib>UN</uTrib>
					<qTrib>4.0000</qTrib>
					<vUnTrib>64.9000000000</vUnTrib>
					<vDesc>12.98</vDesc>
					<indTot>1</indTot>
				</prod>
				<imposto>
					<vTotTrib>81.65</vTotTrib>
					<ICMS>
						<ICMSSN102>
							<orig>0</orig>
							<CSOSN>300</CSOSN>
						</ICMSSN102>
					</ICMS>
					<PIS>
						<PISNT>
							<CST>07</CST>
						</PISNT>
					</PIS>
					<COFINS>
						<COFINSNT>
							<CST>07</CST>
						</COFINSNT>
					</COFINS>
				</imposto>
			</det>
			<det nItem='3'>
				<prod>
					<cProd>1120003</cProd>
					<cEAN>SEM GTIN</cEAN>
					<xProd>BIQUINI CORTINA MISTURA INDIGENA</xProd>
					<NCM>61124100</NCM>
					<CFOP>7101</CFOP>
					<uCom>UN</uCom>
					<qCom>1.0000</qCom>
					<vUnCom>44.9000000000</vUnCom>
					<vProd>44.90</vProd>
					<cEANTrib>SEM GTIN</cEANTrib>
					<uTrib>UN</uTrib>
					<qTrib>1.0000</qTrib>
					<vUnTrib>44.9000000000</vUnTrib>
					<vDesc>2.24</vDesc>
					<indTot>1</indTot>
				</prod>
				<imposto>
					<vTotTrib>14.12</vTotTrib>
					<ICMS>
						<ICMSSN102>
							<orig>0</orig>
							<CSOSN>300</CSOSN>
						</ICMSSN102>
					</ICMS>
					<PIS>
						<PISNT>
							<CST>07</CST>
						</PISNT>
					</PIS>
					<COFINS>
						<COFINSNT>
							<CST>07</CST>
						</COFINSNT>
					</COFINS>
				</imposto>
			</det>
			<total>
				<ICMSTot>
					<vBC>0.00</vBC>
					<vICMS>0.00</vICMS>
					<vICMSDeson>0.00</vICMSDeson>
					<vFCP>0.00</vFCP>
					<vBCST>0.00</vBCST>
					<vST>0.00</vST>
					<vFCPST>0.00</vFCPST>
					<vFCPSTRet>0.00</vFCPSTRet>
					<vProd>7963.30</vProd>
					<vFrete>1.00</vFrete>
					<vSeg>1.00</vSeg>
					<vDesc>50.17</vDesc>
					<vII>0.00</vII>
					<vIPI>0.00</vIPI>
					<vIPIDevol>0.00</vIPIDevol>
					<vPIS>0.00</vPIS>
					<vCOFINS>0.00</vCOFINS>
					<vOutro>0.00</vOutro>
					<vNF>7565.13</vNF>
					<vTotTrib>2504.45</vTotTrib>
				</ICMSTot>
			</total>
			<transp>
				<modFrete>0</modFrete>
				<vol>
					<qVol>1</qVol>
					<esp>VOLUME</esp>
				</vol>
			</transp>
			<pag>
				<detPag>
					<tPag>99</tPag>
					<vPag>7565.13</vPag>
				</detPag>
			</pag>
			<infAdic>
				<infCpl>TRIB. APROX. R$ 1.071,05 FEDERAL 1.433,40 Estadual - FONTE: IBPT/empresometro.com.br D529CB</infCpl>
			</infAdic>
			<exporta>
				<UFSaidaPais>CE</UFSaidaPais>
				<xLocExporta>FORTALEZA</xLocExporta>
			</exporta>
		</infNFe>
	</NFe>
</nfeProc>
";

		const string nfeXmlTextWithoutICMSTotvFrete = @"<?xml version='1.0' encoding='UTF-8'?>
<nfeProc versao='4.00' xmlns='http://www.portalfiscal.inf.br/nfe'>
	<NFe xmlns='http://www.portalfiscal.inf.br/nfe'>
		<infNFe versao='4.00' Id='NFe35210100857758000736550110004201181945721521'>
			<ide>
				<cUF>35</cUF>
				<cNF>94572152</cNF>
				<natOp>7.101 - Venda de producao do estabelecimento</natOp>
				<mod>55</mod>
				<serie>11</serie>
				<nNF>420118</nNF>
				<dhEmi>2021-01-04T11:33:58-02:00</dhEmi>
				<dhSaiEnt>2021-01-04T11:33:58-02:00</dhSaiEnt>
				<tpNF>1</tpNF>
				<idDest>3</idDest>
				<cMunFG>3524907</cMunFG>
				<tpImp>1</tpImp>
				<tpEmis>1</tpEmis>
				<cDV>1</cDV>
				<tpAmb>1</tpAmb>
				<finNFe>1</finNFe>
				<indFinal>0</indFinal>
				<indPres>9</indPres>
				<procEmi>0</procEmi>
				<verProc>QAD</verProc>
			</ide>
			<emit>
				<CNPJ>00857758000736</CNPJ>
				<xNome>APTIV MANUFATURA E SERVICOS DE DISTRIBUICaO LTDA</xNome>
				<xFant>APTIV MANUFATURA E SERVICOS</xFant>
				<enderEmit>
					<xLgr>ROD DOS TAMOIOS</xLgr>
					<nro>S/N</nro>
					<xCpl>KM 21,8</xCpl>
					<xBairro>TAPANHAO</xBairro>
					<cMun>3524907</cMun>
					<xMun>JAMBEIRO</xMun>
					<UF>SP</UF>
					<CEP>12270000</CEP>
					<cPais>1058</cPais>
					<xPais>BRASIL</xPais>
					<fone>1239782042</fone>
				</enderEmit>
				<IE>397001817113</IE>
				<CRT>3</CRT>
			</emit>
			<dest>
				<idEstrangeiro>95001407</idEstrangeiro>
				<xNome>NIPPON EXPRESS DEUTSCHLAND</xNome>
				<enderDest>
					<xLgr>ERFURTER STRASSE 2</xLgr>
					<nro>_</nro>
					<xBairro>__</xBairro>
					<cMun>9999999</cMun>
					<xMun>EXTERIOR</xMun>
					<UF>EX</UF>
					<cPais>0230</cPais>
					<xPais>GERMANY</xPais>
				</enderDest>
				<indIEDest>9</indIEDest>
			</dest>
			<entrega>
				<CNPJ/>
				<xNome>NIPPON EXPRESS DEUTSCHLAND</xNome>
				<xLgr>ERFURTER STRASSE 2</xLgr>
				<nro>ERFURTER STRASSE 2</nro>
				<xBairro>__</xBairro>
				<cMun>9999999</cMun>
				<xMun>ECHING</xMun>
				<UF>EX</UF>
				<CEP>00085386</CEP>
				<cPais>230</cPais>
				<xPais>GERMANY</xPais>
			</entrega>
			<det nItem='1'>
				<prod>
					<cProd>15364304</cProd>
					<cEAN>SEM GTIN</cEAN>
					<xProd>TERMINAL ELETRICO</xProd>
					<NCM>85369090</NCM>
					<CEST>0199900</CEST>
					<indEscala>S</indEscala>
					<CFOP>7101</CFOP>
					<uCom>PC</uCom>
					<qCom>8000.0000</qCom>
					<vUnCom>0.2800700000</vUnCom>
					<vProd>2240.56</vProd>
					<cEANTrib>SEM GTIN</cEANTrib>
					<uTrib>UN</uTrib>
					<qTrib>8000.0000</qTrib>
					<vUnTrib>0.2800700000</vUnTrib>
					<vDesc>10.00</vDesc>
					<vFrete>753.40</vFrete>
					<vSeg>0.69</vSeg>
					<vOutro>222.69</vOutro>
					<indTot>1</indTot>
					<nFCI>B44FD755-70D2-4D41-8CE3-1757482C8E66</nFCI>
				</prod>
				<imposto>
					<ICMS>
						<ICMS40>
							<orig>3</orig>
							<CST>41</CST>
						</ICMS40>
					</ICMS>
					<IPI>
						<cEnq>002</cEnq>
						<IPINT>
							<CST>54</CST>
						</IPINT>
					</IPI>
					<PIS>
						<PISNT>
							<CST>07</CST>
						</PISNT>
					</PIS>
					<COFINS>
						<COFINSNT>
							<CST>07</CST>
						</COFINSNT>
					</COFINS>
				</imposto>
			</det>
			<total>
				<ICMSTot>
					<vBC>0.00</vBC>
					<vICMS>0.00</vICMS>
					<vICMSDeson>0.00</vICMSDeson>
					<vFCP>0.00</vFCP>
					<vBCST>0.00</vBCST>
					<vST>0.00</vST>
					<vFCPST>0.00</vFCPST>
					<vFCPSTRet>0.00</vFCPSTRet>
					<vProd>15683.91</vProd>
					<vFrete>0.00</vFrete>
					<vSeg>4.83</vSeg>
					<vDesc>1.00</vDesc>
					<vII>0.00</vII>
					<vIPI>0.00</vIPI>
					<vIPIDevol>0.00</vIPIDevol>
					<vPIS>0.00</vPIS>
					<vCOFINS>0.00</vCOFINS>
					<vOutro>1558.83</vOutro>
					<vNF>22521.35</vNF>
				</ICMSTot>
			</total>
			<transp>
				<modFrete>0</modFrete>
				<transporta>
					<CNPJ>58890252000113</CNPJ>
					<xNome>DHL WORLD WIDE EXPRESS LTDA</xNome>
					<IE>109746831115</IE>
					<xEnder>RUA SANTA MARINA, 1660</xEnder>
					<xMun>SAO PAULO</xMun>
					<UF>SP</UF>
				</transporta>
				<vol>
					<qVol>1</qVol>
					<esp>PALLET DE MADEIRA</esp>
					<pesoL>95.200</pesoL>
					<pesoB>133.000</pesoB>
				</vol>
			</transp>
			<cobr>
				<fat>
					<nFat>FT245192</nFat>
					<vOrig>22521.35</vOrig>
					<vLiq>22521.35</vLiq>
				</fat>
				<dup>
					<nDup>001</nDup>
					<dVenc>2021-03-22</dVenc>
					<vDup>22521.35</vDup>
				</dup>
			</cobr>
			<pag>
				<detPag>
					<indPag>1</indPag>
					<tPag>99</tPag>
					<vPag>22521.35</vPag>
				</detPag>
			</pag>
			<infAdic>
				<infAdFisco>NAO INCIDENCIA DO ICMS CONFORME ARTIGO 7, INCISO V DO DECRETO 45.490/00IMUNE DE IPI CONFORME ART 18, INCISO II DO DECR. N. 7.212/10</infAdFisco>
				<infCpl>LINHA 1: 3273412LINHA 2: 3282159LINHA 3: 3285523LINHA 4: 3287551LINHA 5: 3308438LINHA 6: 3308998LINHA 7: 3311800...VISTO OBTIDO ELETRONICAMENTE - RIEX - SEFAZ /SP...DSE - DECLARACAO SIMPLIFICADA DE EXPORTACAO - VALOR INFERIOR A US$ 50.000OU VALOR EQUIVALENTE EM OUTRA MOEDA ESTRANGEIRA...P.O.NR. 3273412 / 3282159 / 3285523 / 3287551 / 3308438 / 3308998 /3311800...COLETA PELA DHL WORLD WIDE EXPRESS LTDA...LOCAL DE DESTINO - ALEMANHA...M. EXP. = P202788 / TX. CAMBIAL = 5,1961...Frete Internacional 5.273,78...Outras Despesas 1.558,83...Seguro Internacional 4,83</infCpl>
			</infAdic>
			<exporta>
				<UFSaidaPais>SP</UFSaidaPais>
				<xLocExporta>JAMBEIRO</xLocExporta>
				<xLocDespacho>Aereo - Aeroporto de Viracopos</xLocDespacho>
			</exporta>
		</infNFe>
	</NFe>
</nfeProc>
";

		const string nfeXmlTextWithoutICMSTotvSeg = @"<?xml version='1.0' encoding='UTF-8'?>
<nfeProc versao='4.00' xmlns='http://www.portalfiscal.inf.br/nfe'>
	<NFe xmlns='http://www.portalfiscal.inf.br/nfe'>
		<infNFe versao='4.00' Id='NFe35210100857758000736550110004201181945721521'>
			<ide>
				<cUF>35</cUF>
				<cNF>94572152</cNF>
				<natOp>7.101 - Venda de producao do estabelecimento</natOp>
				<mod>55</mod>
				<serie>11</serie>
				<nNF>420118</nNF>
				<dhEmi>2021-01-04T11:33:58-02:00</dhEmi>
				<dhSaiEnt>2021-01-04T11:33:58-02:00</dhSaiEnt>
				<tpNF>1</tpNF>
				<idDest>3</idDest>
				<cMunFG>3524907</cMunFG>
				<tpImp>1</tpImp>
				<tpEmis>1</tpEmis>
				<cDV>1</cDV>
				<tpAmb>1</tpAmb>
				<finNFe>1</finNFe>
				<indFinal>0</indFinal>
				<indPres>9</indPres>
				<procEmi>0</procEmi>
				<verProc>QAD</verProc>
			</ide>
			<emit>
				<CNPJ>00857758000736</CNPJ>
				<xNome>APTIV MANUFATURA E SERVICOS DE DISTRIBUICaO LTDA</xNome>
				<xFant>APTIV MANUFATURA E SERVICOS</xFant>
				<enderEmit>
					<xLgr>ROD DOS TAMOIOS</xLgr>
					<nro>S/N</nro>
					<xCpl>KM 21,8</xCpl>
					<xBairro>TAPANHAO</xBairro>
					<cMun>3524907</cMun>
					<xMun>JAMBEIRO</xMun>
					<UF>SP</UF>
					<CEP>12270000</CEP>
					<cPais>1058</cPais>
					<xPais>BRASIL</xPais>
					<fone>1239782042</fone>
				</enderEmit>
				<IE>397001817113</IE>
				<CRT>3</CRT>
			</emit>
			<dest>
				<idEstrangeiro>95001407</idEstrangeiro>
				<xNome>NIPPON EXPRESS DEUTSCHLAND</xNome>
				<enderDest>
					<xLgr>ERFURTER STRASSE 2</xLgr>
					<nro>_</nro>
					<xBairro>__</xBairro>
					<cMun>9999999</cMun>
					<xMun>EXTERIOR</xMun>
					<UF>EX</UF>
					<cPais>0230</cPais>
					<xPais>GERMANY</xPais>
				</enderDest>
				<indIEDest>9</indIEDest>
			</dest>
			<entrega>
				<CNPJ/>
				<xNome>NIPPON EXPRESS DEUTSCHLAND</xNome>
				<xLgr>ERFURTER STRASSE 2</xLgr>
				<nro>ERFURTER STRASSE 2</nro>
				<xBairro>__</xBairro>
				<cMun>9999999</cMun>
				<xMun>ECHING</xMun>
				<UF>EX</UF>
				<CEP>00085386</CEP>
				<cPais>230</cPais>
				<xPais>GERMANY</xPais>
			</entrega>
			<det nItem='1'>
				<prod>
					<cProd>15364304</cProd>
					<cEAN>SEM GTIN</cEAN>
					<xProd>TERMINAL ELETRICO</xProd>
					<NCM>85369090</NCM>
					<CEST>0199900</CEST>
					<indEscala>S</indEscala>
					<CFOP>7101</CFOP>
					<uCom>PC</uCom>
					<qCom>8000.0000</qCom>
					<vUnCom>0.2800700000</vUnCom>
					<vProd>2240.56</vProd>
					<cEANTrib>SEM GTIN</cEANTrib>
					<uTrib>UN</uTrib>
					<qTrib>8000.0000</qTrib>
					<vUnTrib>0.2800700000</vUnTrib>
					<vDesc>10.00</vDesc>
					<vFrete>753.40</vFrete>
					<vSeg>0.69</vSeg>
					<vOutro>222.69</vOutro>
					<indTot>1</indTot>
					<nFCI>B44FD755-70D2-4D41-8CE3-1757482C8E66</nFCI>
				</prod>
				<imposto>
					<ICMS>
						<ICMS40>
							<orig>3</orig>
							<CST>41</CST>
						</ICMS40>
					</ICMS>
					<IPI>
						<cEnq>002</cEnq>
						<IPINT>
							<CST>54</CST>
						</IPINT>
					</IPI>
					<PIS>
						<PISNT>
							<CST>07</CST>
						</PISNT>
					</PIS>
					<COFINS>
						<COFINSNT>
							<CST>07</CST>
						</COFINSNT>
					</COFINS>
				</imposto>
			</det>
			<total>
				<ICMSTot>
					<vBC>0.00</vBC>
					<vICMS>0.00</vICMS>
					<vICMSDeson>0.00</vICMSDeson>
					<vFCP>0.00</vFCP>
					<vBCST>0.00</vBCST>
					<vST>0.00</vST>
					<vFCPST>0.00</vFCPST>
					<vFCPSTRet>0.00</vFCPSTRet>
					<vProd>15683.91</vProd>
					<vFrete>5273.78</vFrete>
					<vSeg>0.00</vSeg>
					<vDesc>1.00</vDesc>
					<vII>0.00</vII>
					<vIPI>0.00</vIPI>
					<vIPIDevol>0.00</vIPIDevol>
					<vPIS>0.00</vPIS>
					<vCOFINS>0.00</vCOFINS>
					<vOutro>1558.83</vOutro>
					<vNF>22521.35</vNF>
				</ICMSTot>
			</total>
			<transp>
				<modFrete>0</modFrete>
				<transporta>
					<CNPJ>58890252000113</CNPJ>
					<xNome>DHL WORLD WIDE EXPRESS LTDA</xNome>
					<IE>109746831115</IE>
					<xEnder>RUA SANTA MARINA, 1660</xEnder>
					<xMun>SAO PAULO</xMun>
					<UF>SP</UF>
				</transporta>
				<vol>
					<qVol>1</qVol>
					<esp>PALLET DE MADEIRA</esp>
					<pesoL>95.200</pesoL>
					<pesoB>133.000</pesoB>
				</vol>
			</transp>
			<cobr>
				<fat>
					<nFat>FT245192</nFat>
					<vOrig>22521.35</vOrig>
					<vLiq>22521.35</vLiq>
				</fat>
				<dup>
					<nDup>001</nDup>
					<dVenc>2021-03-22</dVenc>
					<vDup>22521.35</vDup>
				</dup>
			</cobr>
			<pag>
				<detPag>
					<indPag>1</indPag>
					<tPag>99</tPag>
					<vPag>22521.35</vPag>
				</detPag>
			</pag>
			<infAdic>
				<infAdFisco>NAO INCIDENCIA DO ICMS CONFORME ARTIGO 7, INCISO V DO DECRETO 45.490/00IMUNE DE IPI CONFORME ART 18, INCISO II DO DECR. N. 7.212/10</infAdFisco>
				<infCpl>LINHA 1: 3273412LINHA 2: 3282159LINHA 3: 3285523LINHA 4: 3287551LINHA 5: 3308438LINHA 6: 3308998LINHA 7: 3311800...VISTO OBTIDO ELETRONICAMENTE - RIEX - SEFAZ /SP...DSE - DECLARACAO SIMPLIFICADA DE EXPORTACAO - VALOR INFERIOR A US$ 50.000OU VALOR EQUIVALENTE EM OUTRA MOEDA ESTRANGEIRA...P.O.NR. 3273412 / 3282159 / 3285523 / 3287551 / 3308438 / 3308998 /3311800...COLETA PELA DHL WORLD WIDE EXPRESS LTDA...LOCAL DE DESTINO - ALEMANHA...M. EXP. = P202788 / TX. CAMBIAL = 5,1961...Frete Internacional 5.273,78...Outras Despesas 1.558,83...Seguro Internacional 4,83</infCpl>
			</infAdic>
			<exporta>
				<UFSaidaPais>SP</UFSaidaPais>
				<xLocExporta>JAMBEIRO</xLocExporta>
				<xLocDespacho>Aereo - Aeroporto de Viracopos</xLocDespacho>
			</exporta>
		</infNFe>
	</NFe>
</nfeProc>
";

		const string nfeXmlTextWithoutICMSTotvDesc = @"<?xml version='1.0' encoding='UTF-8'?>
<nfeProc versao='4.00' xmlns='http://www.portalfiscal.inf.br/nfe'>
	<NFe xmlns='http://www.portalfiscal.inf.br/nfe'>
		<infNFe versao='4.00' Id='NFe35210100857758000736550110004201181945721521'>
			<ide>
				<cUF>35</cUF>
				<cNF>94572152</cNF>
				<natOp>7.101 - Venda de producao do estabelecimento</natOp>
				<mod>55</mod>
				<serie>11</serie>
				<nNF>420118</nNF>
				<dhEmi>2021-01-04T11:33:58-02:00</dhEmi>
				<dhSaiEnt>2021-01-04T11:33:58-02:00</dhSaiEnt>
				<tpNF>1</tpNF>
				<idDest>3</idDest>
				<cMunFG>3524907</cMunFG>
				<tpImp>1</tpImp>
				<tpEmis>1</tpEmis>
				<cDV>1</cDV>
				<tpAmb>1</tpAmb>
				<finNFe>1</finNFe>
				<indFinal>0</indFinal>
				<indPres>9</indPres>
				<procEmi>0</procEmi>
				<verProc>QAD</verProc>
			</ide>
			<emit>
				<CNPJ>00857758000736</CNPJ>
				<xNome>APTIV MANUFATURA E SERVICOS DE DISTRIBUICaO LTDA</xNome>
				<xFant>APTIV MANUFATURA E SERVICOS</xFant>
				<enderEmit>
					<xLgr>ROD DOS TAMOIOS</xLgr>
					<nro>S/N</nro>
					<xCpl>KM 21,8</xCpl>
					<xBairro>TAPANHAO</xBairro>
					<cMun>3524907</cMun>
					<xMun>JAMBEIRO</xMun>
					<UF>SP</UF>
					<CEP>12270000</CEP>
					<cPais>1058</cPais>
					<xPais>BRASIL</xPais>
					<fone>1239782042</fone>
				</enderEmit>
				<IE>397001817113</IE>
				<CRT>3</CRT>
			</emit>
			<dest>
				<idEstrangeiro>95001407</idEstrangeiro>
				<xNome>NIPPON EXPRESS DEUTSCHLAND</xNome>
				<enderDest>
					<xLgr>ERFURTER STRASSE 2</xLgr>
					<nro>_</nro>
					<xBairro>__</xBairro>
					<cMun>9999999</cMun>
					<xMun>EXTERIOR</xMun>
					<UF>EX</UF>
					<cPais>0230</cPais>
					<xPais>GERMANY</xPais>
				</enderDest>
				<indIEDest>9</indIEDest>
			</dest>
			<entrega>
				<CNPJ/>
				<xNome>NIPPON EXPRESS DEUTSCHLAND</xNome>
				<xLgr>ERFURTER STRASSE 2</xLgr>
				<nro>ERFURTER STRASSE 2</nro>
				<xBairro>__</xBairro>
				<cMun>9999999</cMun>
				<xMun>ECHING</xMun>
				<UF>EX</UF>
				<CEP>00085386</CEP>
				<cPais>230</cPais>
				<xPais>GERMANY</xPais>
			</entrega>
			<det nItem='1'>
				<prod>
					<cProd>15364304</cProd>
					<cEAN>SEM GTIN</cEAN>
					<xProd>TERMINAL ELETRICO</xProd>
					<NCM>85369090</NCM>
					<CEST>0199900</CEST>
					<indEscala>S</indEscala>
					<CFOP>7101</CFOP>
					<uCom>PC</uCom>
					<qCom>8000.0000</qCom>
					<vUnCom>0.2800700000</vUnCom>
					<vProd>2240.56</vProd>
					<cEANTrib>SEM GTIN</cEANTrib>
					<uTrib>UN</uTrib>
					<qTrib>8000.0000</qTrib>
					<vUnTrib>0.2800700000</vUnTrib>
					<vDesc>10.00</vDesc>
					<vFrete>753.40</vFrete>
					<vSeg>0.69</vSeg>
					<vOutro>222.69</vOutro>
					<indTot>1</indTot>
					<nFCI>B44FD755-70D2-4D41-8CE3-1757482C8E66</nFCI>
				</prod>
				<imposto>
					<ICMS>
						<ICMS40>
							<orig>3</orig>
							<CST>41</CST>
						</ICMS40>
					</ICMS>
					<IPI>
						<cEnq>002</cEnq>
						<IPINT>
							<CST>54</CST>
						</IPINT>
					</IPI>
					<PIS>
						<PISNT>
							<CST>07</CST>
						</PISNT>
					</PIS>
					<COFINS>
						<COFINSNT>
							<CST>07</CST>
						</COFINSNT>
					</COFINS>
				</imposto>
			</det>
			<total>
				<ICMSTot>
					<vBC>0.00</vBC>
					<vICMS>0.00</vICMS>
					<vICMSDeson>0.00</vICMSDeson>
					<vFCP>0.00</vFCP>
					<vBCST>0.00</vBCST>
					<vST>0.00</vST>
					<vFCPST>0.00</vFCPST>
					<vFCPSTRet>0.00</vFCPSTRet>
					<vProd>15683.91</vProd>
					<vFrete>5273.78</vFrete>
					<vSeg>4.83</vSeg>
					<vDesc>0.00</vDesc>
					<vII>0.00</vII>
					<vIPI>0.00</vIPI>
					<vIPIDevol>0.00</vIPIDevol>
					<vPIS>0.00</vPIS>
					<vCOFINS>0.00</vCOFINS>
					<vOutro>1558.83</vOutro>
					<vNF>22521.35</vNF>
				</ICMSTot>
			</total>
			<transp>
				<modFrete>0</modFrete>
				<transporta>
					<CNPJ>58890252000113</CNPJ>
					<xNome>DHL WORLD WIDE EXPRESS LTDA</xNome>
					<IE>109746831115</IE>
					<xEnder>RUA SANTA MARINA, 1660</xEnder>
					<xMun>SAO PAULO</xMun>
					<UF>SP</UF>
				</transporta>
				<vol>
					<qVol>1</qVol>
					<esp>PALLET DE MADEIRA</esp>
					<pesoL>95.200</pesoL>
					<pesoB>133.000</pesoB>
				</vol>
			</transp>
			<cobr>
				<fat>
					<nFat>FT245192</nFat>
					<vOrig>22521.35</vOrig>
					<vLiq>22521.35</vLiq>
				</fat>
				<dup>
					<nDup>001</nDup>
					<dVenc>2021-03-22</dVenc>
					<vDup>22521.35</vDup>
				</dup>
			</cobr>
			<pag>
				<detPag>
					<indPag>1</indPag>
					<tPag>99</tPag>
					<vPag>22521.35</vPag>
				</detPag>
			</pag>
			<infAdic>
				<infAdFisco>NAO INCIDENCIA DO ICMS CONFORME ARTIGO 7, INCISO V DO DECRETO 45.490/00IMUNE DE IPI CONFORME ART 18, INCISO II DO DECR. N. 7.212/10</infAdFisco>
				<infCpl>LINHA 1: 3273412LINHA 2: 3282159LINHA 3: 3285523LINHA 4: 3287551LINHA 5: 3308438LINHA 6: 3308998LINHA 7: 3311800...VISTO OBTIDO ELETRONICAMENTE - RIEX - SEFAZ /SP...DSE - DECLARACAO SIMPLIFICADA DE EXPORTACAO - VALOR INFERIOR A US$ 50.000OU VALOR EQUIVALENTE EM OUTRA MOEDA ESTRANGEIRA...P.O.NR. 3273412 / 3282159 / 3285523 / 3287551 / 3308438 / 3308998 /3311800...COLETA PELA DHL WORLD WIDE EXPRESS LTDA...LOCAL DE DESTINO - ALEMANHA...M. EXP. = P202788 / TX. CAMBIAL = 5,1961...Frete Internacional 5.273,78...Outras Despesas 1.558,83...Seguro Internacional 4,83</infCpl>
			</infAdic>
			<exporta>
				<UFSaidaPais>SP</UFSaidaPais>
				<xLocExporta>JAMBEIRO</xLocExporta>
				<xLocDespacho>Aereo - Aeroporto de Viracopos</xLocDespacho>
			</exporta>
		</infNFe>
	</NFe>
</nfeProc>
";

		const string nfeXmlTextWithoutICMSTotvOutro = @"<?xml version='1.0' encoding='UTF-8'?>
<nfeProc versao='4.00' xmlns='http://www.portalfiscal.inf.br/nfe'>
	<NFe xmlns='http://www.portalfiscal.inf.br/nfe'>
		<infNFe versao='4.00' Id='NFe35210100857758000736550110004201181945721521'>
			<ide>
				<cUF>35</cUF>
				<cNF>94572152</cNF>
				<natOp>7.101 - Venda de producao do estabelecimento</natOp>
				<mod>55</mod>
				<serie>11</serie>
				<nNF>420118</nNF>
				<dhEmi>2021-01-04T11:33:58-02:00</dhEmi>
				<dhSaiEnt>2021-01-04T11:33:58-02:00</dhSaiEnt>
				<tpNF>1</tpNF>
				<idDest>3</idDest>
				<cMunFG>3524907</cMunFG>
				<tpImp>1</tpImp>
				<tpEmis>1</tpEmis>
				<cDV>1</cDV>
				<tpAmb>1</tpAmb>
				<finNFe>1</finNFe>
				<indFinal>0</indFinal>
				<indPres>9</indPres>
				<procEmi>0</procEmi>
				<verProc>QAD</verProc>
			</ide>
			<emit>
				<CNPJ>00857758000736</CNPJ>
				<xNome>APTIV MANUFATURA E SERVICOS DE DISTRIBUICaO LTDA</xNome>
				<xFant>APTIV MANUFATURA E SERVICOS</xFant>
				<enderEmit>
					<xLgr>ROD DOS TAMOIOS</xLgr>
					<nro>S/N</nro>
					<xCpl>KM 21,8</xCpl>
					<xBairro>TAPANHAO</xBairro>
					<cMun>3524907</cMun>
					<xMun>JAMBEIRO</xMun>
					<UF>SP</UF>
					<CEP>12270000</CEP>
					<cPais>1058</cPais>
					<xPais>BRASIL</xPais>
					<fone>1239782042</fone>
				</enderEmit>
				<IE>397001817113</IE>
				<CRT>3</CRT>
			</emit>
			<dest>
				<idEstrangeiro>95001407</idEstrangeiro>
				<xNome>NIPPON EXPRESS DEUTSCHLAND</xNome>
				<enderDest>
					<xLgr>ERFURTER STRASSE 2</xLgr>
					<nro>_</nro>
					<xBairro>__</xBairro>
					<cMun>9999999</cMun>
					<xMun>EXTERIOR</xMun>
					<UF>EX</UF>
					<cPais>0230</cPais>
					<xPais>GERMANY</xPais>
				</enderDest>
				<indIEDest>9</indIEDest>
			</dest>
			<entrega>
				<CNPJ/>
				<xNome>NIPPON EXPRESS DEUTSCHLAND</xNome>
				<xLgr>ERFURTER STRASSE 2</xLgr>
				<nro>ERFURTER STRASSE 2</nro>
				<xBairro>__</xBairro>
				<cMun>9999999</cMun>
				<xMun>ECHING</xMun>
				<UF>EX</UF>
				<CEP>00085386</CEP>
				<cPais>230</cPais>
				<xPais>GERMANY</xPais>
			</entrega>
			<det nItem='1'>
				<prod>
					<cProd>15364304</cProd>
					<cEAN>SEM GTIN</cEAN>
					<xProd>TERMINAL ELETRICO</xProd>
					<NCM>85369090</NCM>
					<CEST>0199900</CEST>
					<indEscala>S</indEscala>
					<CFOP>7101</CFOP>
					<uCom>PC</uCom>
					<qCom>8000.0000</qCom>
					<vUnCom>0.2800700000</vUnCom>
					<vProd>2240.56</vProd>
					<cEANTrib>SEM GTIN</cEANTrib>
					<uTrib>UN</uTrib>
					<qTrib>8000.0000</qTrib>
					<vUnTrib>0.2800700000</vUnTrib>
					<vDesc>10.00</vDesc>
					<vFrete>753.40</vFrete>
					<vSeg>0.69</vSeg>
					<vOutro>222.69</vOutro>
					<indTot>1</indTot>
					<nFCI>B44FD755-70D2-4D41-8CE3-1757482C8E66</nFCI>
				</prod>
				<imposto>
					<ICMS>
						<ICMS40>
							<orig>3</orig>
							<CST>41</CST>
						</ICMS40>
					</ICMS>
					<IPI>
						<cEnq>002</cEnq>
						<IPINT>
							<CST>54</CST>
						</IPINT>
					</IPI>
					<PIS>
						<PISNT>
							<CST>07</CST>
						</PISNT>
					</PIS>
					<COFINS>
						<COFINSNT>
							<CST>07</CST>
						</COFINSNT>
					</COFINS>
				</imposto>
			</det>
			<total>
				<ICMSTot>
					<vBC>0.00</vBC>
					<vICMS>0.00</vICMS>
					<vICMSDeson>0.00</vICMSDeson>
					<vFCP>0.00</vFCP>
					<vBCST>0.00</vBCST>
					<vST>0.00</vST>
					<vFCPST>0.00</vFCPST>
					<vFCPSTRet>0.00</vFCPSTRet>
					<vProd>15683.91</vProd>
					<vFrete>5273.78</vFrete>
					<vSeg>4.83</vSeg>
					<vDesc>1.00</vDesc>
					<vII>0.00</vII>
					<vIPI>0.00</vIPI>
					<vIPIDevol>0.00</vIPIDevol>
					<vPIS>0.00</vPIS>
					<vCOFINS>0.00</vCOFINS>
					<vOutro>0.00</vOutro>
					<vNF>22521.35</vNF>
				</ICMSTot>
			</total>
			<transp>
				<modFrete>0</modFrete>
				<transporta>
					<CNPJ>58890252000113</CNPJ>
					<xNome>DHL WORLD WIDE EXPRESS LTDA</xNome>
					<IE>109746831115</IE>
					<xEnder>RUA SANTA MARINA, 1660</xEnder>
					<xMun>SAO PAULO</xMun>
					<UF>SP</UF>
				</transporta>
				<vol>
					<qVol>1</qVol>
					<esp>PALLET DE MADEIRA</esp>
					<pesoL>95.200</pesoL>
					<pesoB>133.000</pesoB>
				</vol>
			</transp>
			<cobr>
				<fat>
					<nFat>FT245192</nFat>
					<vOrig>22521.35</vOrig>
					<vLiq>22521.35</vLiq>
				</fat>
				<dup>
					<nDup>001</nDup>
					<dVenc>2021-03-22</dVenc>
					<vDup>22521.35</vDup>
				</dup>
			</cobr>
			<pag>
				<detPag>
					<indPag>1</indPag>
					<tPag>99</tPag>
					<vPag>22521.35</vPag>
				</detPag>
			</pag>
			<infAdic>
				<infAdFisco>NAO INCIDENCIA DO ICMS CONFORME ARTIGO 7, INCISO V DO DECRETO 45.490/00IMUNE DE IPI CONFORME ART 18, INCISO II DO DECR. N. 7.212/10</infAdFisco>
				<infCpl>LINHA 1: 3273412LINHA 2: 3282159LINHA 3: 3285523LINHA 4: 3287551LINHA 5: 3308438LINHA 6: 3308998LINHA 7: 3311800...VISTO OBTIDO ELETRONICAMENTE - RIEX - SEFAZ /SP...DSE - DECLARACAO SIMPLIFICADA DE EXPORTACAO - VALOR INFERIOR A US$ 50.000OU VALOR EQUIVALENTE EM OUTRA MOEDA ESTRANGEIRA...P.O.NR. 3273412 / 3282159 / 3285523 / 3287551 / 3308438 / 3308998 /3311800...COLETA PELA DHL WORLD WIDE EXPRESS LTDA...LOCAL DE DESTINO - ALEMANHA...M. EXP. = P202788 / TX. CAMBIAL = 5,1961...Frete Internacional 5.273,78...Outras Despesas 1.558,83...Seguro Internacional 4,83</infCpl>
			</infAdic>
			<exporta>
				<UFSaidaPais>SP</UFSaidaPais>
				<xLocExporta>JAMBEIRO</xLocExporta>
				<xLocDespacho>Aereo - Aeroporto de Viracopos</xLocDespacho>
			</exporta>
		</infNFe>
	</NFe>
</nfeProc>
";

		#endregion
	}
}
