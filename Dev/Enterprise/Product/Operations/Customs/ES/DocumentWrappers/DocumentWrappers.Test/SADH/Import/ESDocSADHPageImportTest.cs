using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using ESCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.ES.Business.Declaration.CusEntryLine>;
using JobDeclaration = Enterprise.Customs.ES.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH.Testing
{
	sealed class ESDocSADHPageImportTest : DocSADHPageTest
	{
		public void TestNewPageFromListOfImportLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
				CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];

				var lines = new List<ESDocSADHLineImport>();
				lines.Add(ESDocSADHLineImport.New(entryHeader.MergedLines[0], Factory));
				lines.Add(ESDocSADHLineImport.New(entryHeader.MergedLines[1], Factory));

				AssertNotNull("Page is created when is there less than 3 lines", ESDocSADHPageImport.New(Factory, entryHeader.MergedLines, lines, 0));
			});
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return ESDocSADHPageImport.New(Factory, null);
		}

		public void TestBISCaption()
		{
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			ESDocSADHPageImport page = ESDocSADHPageImport.New(Factory, entryLine);

			AssertEquals("BIS caption for ES", "BIS", page.BISCaption);
		}

		public void TestBox47TotalMethodOfPaymentES()
		{
			CombineAssertions(() =>
			{
				var pages = AddNewEntry();
				AddFeesToEntry(entryLines[0]);
				var lastPage = (ESDocSADHPageImport)pages.Last();
				AssertEquals("Box47TotalMethodOfPayment Empty if is not BIS Page", ZString.Empty, lastPage.Box47TotalMethodOfPayment);

				pages = AddNewEntry();
				AddFeesToEntry(entryLines[1]);
				lastPage = (ESDocSADHPageImport)pages.Last();
				AssertEquals("Box47TotalMethodOfPayment Fill in Last BIS Page with 1 Line in Bis Page", "A", lastPage.Box47TotalMethodOfPayment);

				pages = AddNewEntry();
				AddFeesToEntry(entryLines[2]);
				lastPage = (ESDocSADHPageImport)pages.Last();
				AssertEquals("Box47TotalMethodOfPayment Fill in Last BIS Page with 2 Lines in Bis Page", "A", lastPage.Box47TotalMethodOfPayment);

				pages = AddNewEntry();
				AddFeesToEntry(entryLines[3]);
				lastPage = (ESDocSADHPageImport)pages.Last();
				AssertEquals("Box47TotalMethodOfPayment Fill in Last BIS Page with the 3 Lines in Bis Page", "A", lastPage.Box47TotalMethodOfPayment);

				pages = AddNewEntry();
				AddFeesToEntry(entryLines[4]);
				var midPage = pages[1];
				AssertEquals("Box47TotalMethodOfPayment Empty in BIS Page that is not the Last", ZString.Empty, midPage.Box47TotalMethodOfPayment);
			});
		}

		public void TestBox47TotalMethodOfPaymentESWhenAllDiffered()
		{
			var pages = AddNewEntry();
			entryLines[0].Fees.Add(GetFee("B00", 5000m, FeeMethodOfPayment.Deferred));
			pages = AddNewEntry();
			entryLines[1].Fees.Add(GetFee("B00", 5000m, FeeMethodOfPayment.Deferred));
			var lastPage = (ESDocSADHPageImport)pages.Last();
			AssertEquals("Box47TotalMethodOfPayment Empty in Last BIS Page with 1 Line in Bis Page when all taxes are deferred", ZString.Empty, lastPage.Box47TotalMethodOfPayment);
		}

		public void TestBox47TotalAmountES()
		{
			var pages = AddNewEntry();
			AddFeesToEntry(entryLines[0]);
			var lastPage = (ESDocSADHPageImport)pages.Last();
			CombineAssertions(() =>
			{
				AssertEquals("Box47TotalAmount Empty if is not BIS Page", ZString.Empty, lastPage.Box47TotalAmount);

				pages = AddNewEntry();
				AddFeesToEntry(entryLines[1]);
				lastPage = (ESDocSADHPageImport)pages.Last();
				AssertEquals("Box47TotalAmount Fill in Last BIS Page with 1 Line in Bis Page", "200.00", lastPage.Box47TotalAmount);

				pages = AddNewEntry();
				AddFeesToEntry(entryLines[2]);
				lastPage = (ESDocSADHPageImport)pages.Last();
				AssertEquals("Box47TotalAmount Fill in Last BIS Page with 2 Lines in Bis Page", "300.00", lastPage.Box47TotalAmount);

				pages = AddNewEntry();
				AddFeesToEntry(entryLines[3]);
				lastPage = (ESDocSADHPageImport)pages.Last();
				AssertEquals("Box47TotalAmount Fill in Last BIS Page with the 3 Lines in Bis Page", "400.00", lastPage.Box47TotalAmount);

				pages = AddNewEntry();
				AddFeesToEntry(entryLines[4]);
				entryLines[4].Fees.Add(GetFee("B00", 5000m, FeeMethodOfPayment.Deferred));
				entryLines[4].Fees.Add(GetFee("A20", 500m, "A"));
				var midPage = pages[1];
				lastPage = (ESDocSADHPageImport)pages.Last();
				AssertEquals("Box47TotalAmount Empty in BIS Page that is not the Last", ZString.Empty, midPage.Box47TotalAmount);
				AssertEquals("Box47TotalAmount Fill in Last BIS Page with One Fee Deferred", "1,000.00", lastPage.Box47TotalAmount);
			});
		}

		public void TestBox47TaxesTotalsES()
		{
			var pages = AddNewEntry();
			AddFeesToEntry(entryLines[0]);
			var lastPage = (ESDocSADHPageImport)pages.Last();
			CombineAssertions(() =>
			{
				AssertEquals("Box47TaxesTotals Empty if is not BIS Page", 0, lastPage.Box47TaxesTotals.Count);

				pages = AddNewEntry();
				AddFeesToEntry(entryLines[1]);
				lastPage = (ESDocSADHPageImport)pages.Last();
				AssertEquals("Box47TaxesTotals Fill in Last BIS Page with 1 Line in Bis Page. Total Taxes 2", 2, lastPage.Box47TaxesTotals.Count);
				AssertEquals($"'{lastPage.Box47TaxesTotals[0].G4_Type}' amount", "100.00", lastPage.Box47TaxesTotals[0].G4_Amount_InDeclarationCurrency);
				AssertEquals($"'{lastPage.Box47TaxesTotals[1].G4_Type}' amount", "100.00", lastPage.Box47TaxesTotals[1].G4_Amount_InDeclarationCurrency);

				pages = AddNewEntry();
				AddFeesToEntry(entryLines[2]);
				lastPage = (ESDocSADHPageImport)pages.Last();
				AssertEquals("Box47TaxesTotals Fill in Last BIS Page with 2 Line in Bis Page. Total Taxes 2", 2, lastPage.Box47TaxesTotals.Count);
				AssertEquals($"'{lastPage.Box47TaxesTotals[0].G4_Type}' amount", "150.00", lastPage.Box47TaxesTotals[0].G4_Amount_InDeclarationCurrency);
				AssertEquals($"'{lastPage.Box47TaxesTotals[1].G4_Type}' amount", "150.00", lastPage.Box47TaxesTotals[1].G4_Amount_InDeclarationCurrency);

				pages = AddNewEntry();
				AddFeesToEntry(entryLines[3]);
				lastPage = (ESDocSADHPageImport)pages.Last();
				AssertEquals("Box47TaxesTotals Fill in Last BIS Page with the 3 Lines in Bis Page. Total Taxes 2", 2, lastPage.Box47TaxesTotals.Count);
				AssertEquals($"'{lastPage.Box47TaxesTotals[0].G4_Type}' amount", "200.00", lastPage.Box47TaxesTotals[0].G4_Amount_InDeclarationCurrency);
				AssertEquals($"'{lastPage.Box47TaxesTotals[1].G4_Type}' amount", "200.00", lastPage.Box47TaxesTotals[1].G4_Amount_InDeclarationCurrency);

				pages = AddNewEntry();
				AddFeesToEntry(entryLines[4]);
				entryLines[4].Fees.Add(GetFee("B00", 5000m, FeeMethodOfPayment.Deferred));
				entryLines[4].Fees.Add(GetFee("A20", 1500m, "A"));
				var midPage = pages[1];
				lastPage = (ESDocSADHPageImport)pages.Last();

				AssertEquals("BBox47TaxesTotals Empty in BIS Page that is not the Last", 0, midPage.Box47TaxesTotals.Count);
				AssertEquals("Box47TaxesTotals Fill in Last BIS Page with the 3 Lines in Bis Page. Total Taxes 3", 3, lastPage.Box47TaxesTotals.Count);
				AssertEquals($"'{lastPage.Box47TaxesTotals[0].G4_Type}' amount", "250.00", lastPage.Box47TaxesTotals[0].G4_Amount_InDeclarationCurrency);
				AssertEquals($"'{lastPage.Box47TaxesTotals[1].G4_Type}' amount", "1,500.00", lastPage.Box47TaxesTotals[1].G4_Amount_InDeclarationCurrency);
				AssertEquals($"'{lastPage.Box47TaxesTotals[2].G4_Type}' amount with One Fee Deferred", "5,250.00", lastPage.Box47TaxesTotals[2].G4_Amount_InDeclarationCurrency);
			});
		}

		public void TestBox47TaxesTotalsESWithMethodOfPayemntDEF()
		{
			var pages = AddNewEntry();
			AddFeesToEntry(entryLines[0], FeeMethodOfPayment.Deferred);
			var lastPage = (ESDocSADHPageImport)pages.Last();
			CombineAssertions(() =>
			{
				AssertEquals("Box47TaxesTotals Empty if is not BIS Page", 0, lastPage.Box47TaxesTotals.Count);

				pages = AddNewEntry();
				AddFeesToEntry(entryLines[1], FeeMethodOfPayment.Deferred);
				lastPage = (ESDocSADHPageImport)pages.Last();
				AssertEquals("Box47TaxesTotals Fill in Last BIS Page with 1 Line in Bis Page. Total Taxes 2", 2, lastPage.Box47TaxesTotals.Count);
				AssertEquals($"'{lastPage.Box47TaxesTotals[0].G4_Type}' amount (Pos 0)", "100.00", lastPage.Box47TaxesTotals[0].G4_Amount_InDeclarationCurrency);
				AssertEquals($"'{lastPage.Box47TaxesTotals[1].G4_Type}' amount (Pos 1)", "100.00", lastPage.Box47TaxesTotals[1].G4_Amount_InDeclarationCurrency);

				pages = AddNewEntry();
				AddFeesToEntry(entryLines[2], FeeMethodOfPayment.Deferred);
				lastPage = (ESDocSADHPageImport)pages.Last();
				AssertEquals("Box47TaxesTotals Fill in Last BIS Page with 2 Line in Bis Page. Total Taxes 2", 2, lastPage.Box47TaxesTotals.Count);
				AssertEquals($"'{lastPage.Box47TaxesTotals[0].G4_Type}' amount (Pos 0)", "150.00", lastPage.Box47TaxesTotals[0].G4_Amount_InDeclarationCurrency);
				AssertEquals($"'{lastPage.Box47TaxesTotals[1].G4_Type}' amount (Pos 1)", "150.00", lastPage.Box47TaxesTotals[1].G4_Amount_InDeclarationCurrency);
			});
		}

		void AddFeesToEntry(CusEntryLine entryLine, string methodOfPayment = "A")
		{
			entryLine.Fees.Add(GetFee("A00", 50m, "A"));
			entryLine.Fees.Add(GetFee("B00", 50m, methodOfPayment));
		}

		ESDocSADHPageCollectionImport AddNewEntry()
		{
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.ZG_MethodOfPayment = "A";
			invoiceLine.ZG_MethodOfPayment2 = "R";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			entryLines = entryHeader.MergedLines;

			return new ESDocSADHPageCollectionImport(entryLines, Factory);
		}

		CusEntryLineFee GetFee(ZString type, ZDecimal amount, ZString methodOfPay)
		{
			var fee = Factory.New<CusEntryLineFee>();

			fee.CF_ChargeType = type;
			fee.CF_ChargeAmount = amount;
			fee.CF_MethodOfPayment = methodOfPay;
			return fee;
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		ESCusEntryLineCollection entryLines;
	}
}
