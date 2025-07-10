using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	public class CreditD48OperationActionRunnerTest : TestCaseWithFactory
	{
		public void TestCreditD48()
		{
			var entryLines = new List<CusEntryLine>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.MainAddress.Address1 = "ImporterAddress";
			importer.MainAddress.OA_Code = "ImporterAddress";

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";
			supplier.MainAddress.Address1 = "SupplierAddress";
			supplier.MainAddress.OA_Code = "SupplierAddress";

			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI001", ZString.Empty, ZString.Empty, "2DE044F1");
			supplier.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G1, "DGE001", ZString.Empty, ZString.Empty, "C8A4E54E");

			var guarantee1 = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "IGUA", importer.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "IMPO", Core.Constants.CountryCodes.France);
			Factory.Save();

			guarantee1.AddTransaction("Opening Balance", "Opening Balance", "", "", 1000, 0, Customs.Business.PermitTransactionStatusList.Codes.Confirmed, 0, Customs.Business.PermitTransactionTypeList.Codes.OBL);

			var helper1 = new UniversalReferenceTestDataHelper(Factory);
			helper1.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "ZZZ", "zzz", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0001", "statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "2044", "Demande d'autorisation d'importation de radionucléides (DAI) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("US", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM, "GHI", "Anser fabalis/Bean goose", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA, "DEF", "DDezful", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			var helper2 = new UniversalReferenceTestDataHelper(Factory);
			helper2.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document of Export Direction");
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0001", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0003", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "2045", "Demande d'autorisation d'exportation de radionucléides (DAE) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			Factory.Save();
			var declaration1 = SetupJobDeclaration(importer, supplier, "19212081311", 1, entryLines);
			AssertEquals(guarantee1, declaration1.CustomsGuarantee);
			var declaration2 = SetupJobDeclaration(importer, supplier, "19212081312", 2, entryLines);
			AssertEquals(guarantee1, declaration2.CustomsGuarantee);
			var declaration3 = SetupJobDeclaration(importer, supplier, "19212081313", 3, entryLines);
			AssertEquals(guarantee1, declaration3.CustomsGuarantee);

			var declaration4 = SetupJobDeclaration(importer, supplier, "19212081314", 4, entryLines, EntryStatusDescriptionCodeList.Codes.ES060);
			var declaration5 = SetupJobDeclaration(importer, supplier, "19212081315", 5, entryLines, EntryStatusDescriptionCodeList.Codes.ES130);

			Factory.Save();
			var currentTransactions = guarantee1.GetTransactions().ToArray();
			AssertEquals("Guarantee should only have 1 OBL transaction", 1, currentTransactions.Length);
			Assert(!entryLines.SelectMany(x => x.SupportingDocumentsToCustoms).Where(x => x.CSI_Code == "ZZZ" && x.CSI_ReferenceNumber == "AAA").All(x => x.CSI_Quantity3 == 0));

			var log = CreditD48(declaration1, declaration2, declaration3, declaration4, declaration5);
			Factory.Save();

			currentTransactions = guarantee1.GetTransactions().ToArray();
			AssertEquals("Guarantee should now have 4 transactions", 4, currentTransactions.Length);
			AssertEquals("Only first entry line collects CSI_Value of documents set at declaration or invoice Header level.", 260m, currentTransactions[1].CPL_TranValue);
			AssertEquals("D48", currentTransactions[1].CPL_Procedure);
			AssertEquals("D48 for doc ZZZ/AAA for entry line 1", currentTransactions[1].CPL_Comment);
			AssertEquals(220m, currentTransactions[2].CPL_TranValue);
			AssertEquals("D48 for doc ZZZ/AAA for entry line 2", currentTransactions[2].CPL_Comment);
			AssertEquals(330m, currentTransactions[3].CPL_TranValue);
			AssertEquals("D48 for doc ZZZ/AAA for entry line 3", currentTransactions[3].CPL_Comment);
			AssertEquals(@"INFO: Searching for Supporting Documents with Code = ZZZ and Reference = AAA
INFO: 1 entry lines in entry header 19212081311 found that are eligible for crediting transactions for D48
INFO: Declaration B00001000, entry 19212081311, entry line 1, credit 260,00 EUR to guarantee IGUA
INFO: Create COD message 1 in entry header 19212081311 successfully
INFO: 1 entry lines in entry header 19212081312 found that are eligible for crediting transactions for D48
INFO: Declaration B00001001, entry 19212081312, entry line 2, credit 220,00 EUR to guarantee IGUA
INFO: Create COD message 2 in entry header 19212081312 successfully
INFO: 1 entry lines in entry header 19212081313 found that are eligible for crediting transactions for D48
INFO: Declaration B00001002, entry 19212081313, entry line 3, credit 330,00 EUR to guarantee IGUA
INFO: Create COD message 3 in entry header 19212081313 successfully
INFO: 1 entry lines in entry header 19212081314 found that are eligible for crediting transactions for D48
INFO: Declaration B00001003, entry 19212081314, entry line 4, credit 440,00 EUR to guarantee IGUA
INFO: Create COD message 4 in entry header 19212081314 successfully
INFO: 1 entry lines in entry header 19212081315 found that are eligible for crediting transactions for D48
INFO: Declaration B00001004, entry 19212081315, entry line 5, credit 550,00 EUR to guarantee IGUA
INFO: Create COD message 5 in entry header 19212081315 successfully", string.Join("\r\n", log.messages));

			Assert("All entrylines headers documents matching applicator code and reference should have D48 delay reset.", entryLines.SelectMany(x => x.Header.SupportingDocuments).Where(x => x.CSI_Code == "ZZZ" && x.CSI_ReferenceNumber == "AAA").All(x => x.CSI_Quantity3 == 0));
			Assert("All entrylines documents matching applicator code and reference should have D48 delay reset.", entryLines.SelectMany(x => x.SupportingDocuments).Where(x => x.CSI_Code == "ZZZ" && x.CSI_ReferenceNumber == "AAA").All(x => x.CSI_Quantity3 == 0));
			Assert("CSI_Value should keep unchanged even the D48 is closed, as CSI_Value is a critical factor to determine whether the document is D48, see email attached in WI00473052 - Bug - D48 Report", entryLines.SelectMany(x => x.SupportingDocuments).Where(x => x.CSI_Code == "ZZZ" && x.CSI_ReferenceNumber == "AAA").All(x => x.CSI_Value > 0));

			var ediMessages = Factory.Load<CODSendMessage>(new ZQuery()).OrderBy(x => x.EM_MessageNum).ToArray();
			AssertEquals(5, ediMessages.Length);
			AssertEquals(EDIMessage.Status.Queued, ediMessages[0].EM_Status);
			AssertEquals("1", ediMessages[0].EM_MessageNum);
			AssertContains(@"<EnveloppeMessage>
<schemaID>MessageCOD</schemaID>
<schemaVersion>18122012</schemaVersion>
<partyId>2DE044F1</partyId>
<transactionId>0000000001</transactionId>
<numseq>0</numseq>
</EnveloppeMessage>
<Declaration>
<Entete>
<codact>1</codact>
<refdos>19212081311</refdos>
</Entete>
<Articles>
<Article>
<typflux>IMP</typflux>
<numart>1</numart>
<Documents>
<DocumentAapurer>
<doc>ZZZ</doc>
<refdoc>AAA</refdoc>
</DocumentAapurer>
</Documents>
</Article>
</Articles>
</Declaration>
</Message>".Replace(System.Environment.NewLine, ""), ediMessages[0].EM_MessageText);

			AssertEquals(EDIMessage.Status.Queued, ediMessages[1].EM_Status);
			AssertEquals("2", ediMessages[1].EM_MessageNum);
			AssertContains(@"<EnveloppeMessage>
<schemaID>MessageCOD</schemaID>
<schemaVersion>18122012</schemaVersion>
<partyId>2DE044F1</partyId>
<transactionId>0000000002</transactionId>
<numseq>0</numseq>
</EnveloppeMessage>
<Declaration>
<Entete>
<codact>1</codact>
<refdos>19212081312</refdos>
</Entete>
<Articles>
<Article>
<typflux>IMP</typflux>
<numart>2</numart>
<Documents>
<DocumentAapurer>
<doc>ZZZ</doc>
<refdoc>AAA</refdoc>
</DocumentAapurer>
</Documents>
</Article>
</Articles>
</Declaration>
</Message>".Replace(System.Environment.NewLine, ""), ediMessages[1].EM_MessageText);

			AssertEquals(EDIMessage.Status.Queued, ediMessages[2].EM_Status);
			AssertEquals("3", ediMessages[2].EM_MessageNum);
			AssertContains(@"<EnveloppeMessage>
<schemaID>MessageCOD</schemaID>
<schemaVersion>18122012</schemaVersion>
<partyId>2DE044F1</partyId>
<transactionId>0000000003</transactionId>
<numseq>0</numseq>
</EnveloppeMessage>
<Declaration>
<Entete>
<codact>1</codact>
<refdos>19212081313</refdos>
</Entete>
<Articles>
<Article>
<typflux>IMP</typflux>
<numart>3</numart>
<Documents>
<DocumentAapurer>
<doc>ZZZ</doc>
<refdoc>AAA</refdoc>
</DocumentAapurer>
</Documents>
</Article>
</Articles>
</Declaration>
</Message>".Replace(System.Environment.NewLine, ""), ediMessages[2].EM_MessageText);

			ediMessages = Factory.Load<CODSendMessage>(new ZQuery());
			AssertEquals(5, ediMessages.Length);
		}

		DummyOperationalActionSectionLog CreditD48(JobDeclaration declaration1, JobDeclaration declaration2, JobDeclaration declaration3, JobDeclaration declaration4, JobDeclaration declaration5)
		{
			var log = new DummyOperationalActionSectionLog();
			var creditD48OperationalActionRunner = new CreditD48OperationalActionRunner(log, new[] { declaration1, declaration2, declaration3, declaration4, declaration5 });
			var creditD48Applicator = new FrDeclarationCreditD48Applicator(Factory);
			creditD48Applicator.D48DocumentCode = "ZZZ";
			creditD48Applicator.ReferenceNumber = "AAA";
			creditD48OperationalActionRunner.CreditD48(creditD48Applicator);
			return log;
		}

		JobDeclaration SetupJobDeclaration(OrgHeader importer, OrgHeader supplier, ZString entryReference, int lineNum, List<CusEntryLine> entryLines, string entryStatus = EntryStatusDescriptionCodeList.Codes.ES100)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
			declaration.JE_PaymentMethod = MethodOfPaymentList.Codes.M;
			declaration.WithFlux(Common.EU.EUJobMessageTypeList.Codes.Import);
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_CustomsProfile = "DGI001";
			declaration.SetupImporter(importer);

			var suppDoc1 = declaration.SupportingDocuments.AddNew();
			suppDoc1.CSI_Code = "ZZZ";
			suppDoc1.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc1.CSI_Quantity3 = 10;
			suppDoc1.CSI_ReferenceNumber = "aAa";
			suppDoc1.CSI_Value = 100 * lineNum;
			suppDoc1.CSI_RX_NKCurrency = "EUR";
			suppDoc1.CSI_Description = entryReference + "1";
			suppDoc1.CSI_DateOfExpiry = ZDateTime.Now;

			var cei = declaration.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "EUR";

			var suppDoc2 = invoice.SupportingDocuments.AddNew();
			suppDoc2.CSI_Code = "ZZZ";
			suppDoc2.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc2.CSI_Quantity3 = 10;
			suppDoc2.CSI_ReferenceNumber = "AAA";
			suppDoc2.CSI_Value = 50 * lineNum;
			suppDoc2.CSI_RX_NKCurrency = "EUR";
			suppDoc2.CSI_Description = entryReference + "2";
			suppDoc2.CSI_DateOfExpiry = ZDateTime.Now;

			var suppDoc3 = invoice.SupportingDocuments.AddNew();
			suppDoc3.CSI_Code = "2044";
			suppDoc3.CSI_Quantity3 = 0;
			suppDoc3.CSI_ReferenceNumber = "AAA";
			suppDoc3.CSI_Value = 75 * lineNum;
			suppDoc3.CSI_RX_NKCurrency = "EUR";
			suppDoc3.CSI_Description = entryReference + "3";
			suppDoc3.CSI_DateOfExpiry = ZDateTime.Now;

			var suppDoc4 = invoice.SupportingDocuments.AddNew();
			suppDoc4.CSI_Code = "ZZZ";
			suppDoc4.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc4.CSI_Quantity3 = 10;
			suppDoc4.CSI_ReferenceNumber = "bBb";
			suppDoc4.CSI_Value = 25 * lineNum;
			suppDoc4.CSI_RX_NKCurrency = "EUR";
			suppDoc4.CSI_Description = entryReference + "4";
			suppDoc4.CSI_DateOfExpiry = ZDateTime.Now;

			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "2203001010";
			invoiceLine.JI_Description = "Unit Test";
			invoiceLine.JI_LinePrice = 50;
			invoiceLine.JI_CEI = cei.PK;
			invoiceLine.JI_CustomsQuantity = 1m;

			var suppDoc5 = invoiceLine.SupportingDocuments.AddNew();
			suppDoc5.CSI_Code = "ZZZ";
			suppDoc5.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc5.CSI_Quantity3 = 10;
			suppDoc5.CSI_ReferenceNumber = "AAA";
			suppDoc5.CSI_Value = 80 * lineNum;
			suppDoc5.CSI_RX_NKCurrency = "EUR";
			suppDoc5.CSI_Description = entryReference + "5";
			suppDoc5.CSI_DateOfExpiry = ZDateTime.Now;

			var suppDoc6 = invoiceLine.SupportingDocuments.AddNew();
			suppDoc6.CSI_Code = "0003";
			suppDoc6.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc6.CSI_Quantity3 = 10;
			suppDoc6.CSI_ReferenceNumber = "AAA";
			suppDoc6.CSI_Value = 30 * lineNum;
			suppDoc6.CSI_RX_NKCurrency = "EUR";
			suppDoc6.CSI_Description = entryReference + "6";
			suppDoc6.CSI_DateOfExpiry = ZDateTime.Now;

			var suppDoc7 = invoiceLine.SupportingDocuments.AddNew();
			suppDoc7.CSI_Code = "ZZZ";
			suppDoc7.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc7.CSI_Quantity3 = 10;
			suppDoc7.CSI_ReferenceNumber = "AAA";
			suppDoc7.CSI_Value = 30 * lineNum;
			suppDoc7.CSI_RX_NKCurrency = "EUR";
			suppDoc7.CSI_Description = entryReference + "7";
			suppDoc7.CSI_DateOfExpiry = ZDateTime.Now;

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = entryStatus;
			entryHeader.CH_BGMReference = entryReference;

			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = (ZShort)lineNum;
			entryLines.Add(entryLine);
			invoiceLine.JI_CL = entryLine.PK;

			return declaration;
		}
	}
}
