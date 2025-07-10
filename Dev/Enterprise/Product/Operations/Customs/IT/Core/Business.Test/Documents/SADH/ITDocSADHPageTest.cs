using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.DocumentWrappers.Customs.EU.Testing;
using Enterprise.MasterFiles.Business;
using ITCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.IT.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ITDocSADHPageTest : DocSADHPageTest
{
	public void TestBISCaption()
	{
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var page = ITDocSADHPage.New(Factory, entryLine);

		AssertEquals("BIS caption is BIS for Italy", "BIS", page.BISCaption);
	}

	public void TestBox47TaxesTotalsIT()
	{
		var pages = AddNewEntry();
		entryLines[0].Fees.Add(GetFee("405", "G", 136.08m));
		entryLines[0].Fees.Add(GetFee("914", "G", 0m));
		entryLines[0].Fees.Add(GetFee("A00", "E", 66.27m));

		var lastPage = (ITDocSADHPage)pages.Last();
		AssertEquals("Box47TaxesTotals Empty if is not BIS Page", 0, lastPage.Box47TaxesTotals.Count);

		pages = AddNewEntry();
		entryLines[1].Fees.Add(GetFee("B00", "R", 5.31m));
		entryLines[1].Fees.Add(GetFee("405", "G", 103.52m));
		entryLines[1].Fees.Add(GetFee("A00", "E", 28.72));

		CombineAssertions("Page with 2 entry lines", () =>
		{
			lastPage = (ITDocSADHPage)pages.Last();
			var box47TaxesTotalsLastPage = lastPage.Box47TaxesTotals;
			AssertEquals(3, box47TaxesTotalsLastPage.Count);

			AssertDocSADHLineTax(box47TaxesTotalsLastPage, index: 0, type: "A00", methodOfPayment: "E", amount: "94.99");

			AssertDocSADHLineTax(box47TaxesTotalsLastPage, index: 1, type: "914", methodOfPayment: "G", amount: "0.00");

			AssertDocSADHLineTax(box47TaxesTotalsLastPage, index: 2, type: "405", methodOfPayment: "G", amount: "239.60");
		});

		pages = AddNewEntry();
		entryLines[2].Fees.Add(GetFee("405", "G", 31.06m));
		entryLines[2].Fees.Add(GetFee("A00", "E", 8.62));

		CombineAssertions("Page with 3 entry lines", () =>
		{
			lastPage = (ITDocSADHPage)pages.Last();
			var box47TaxesTotalsLastPage = lastPage.Box47TaxesTotals;

			AssertEquals(3, box47TaxesTotalsLastPage.Count);

			AssertDocSADHLineTax(box47TaxesTotalsLastPage, index: 0, type: "A00", methodOfPayment: "E", amount: "103.61");

			AssertDocSADHLineTax(box47TaxesTotalsLastPage, index: 1, type: "914", methodOfPayment: "G", amount: "0.00");

			AssertDocSADHLineTax(box47TaxesTotalsLastPage, index: 2, type: "405", methodOfPayment: "G", amount: "270.66");
		});

		pages = AddNewEntry();
		entryLines[3].Fees.Add(GetFee("916", "A", 0.14m));
		entryLines[3].Fees.Add(GetFee("405", "A", 33.83m));
		entryLines[3].Fees.Add(GetFee("A00", "A", 10.05m));

		pages = AddNewEntry();
		entryLines[4].Fees.Add(GetFee("A00", "O", 5.31m));
		entryLines[4].Fees.Add(GetFee("A00", "R", 5.31m));
		entryLines[4].Fees.Add(GetFee("A00", "S", 28.72));
		entryLines[4].Fees.Add(GetFee("405", "U", 19.15m));
		entryLines[4].Fees.Add(GetFee("405", "V", 19.15m));
		var midPage = pages[1];
		lastPage = (ITDocSADHPage)pages.Last();

		AssertEquals("Page that is not the Last, Box47TaxesTotals Count should be 0", 0, midPage.Box47TaxesTotals.Count);

		CombineAssertions("Page with 4 entry lines", () =>
		{
			lastPage = (ITDocSADHPage)pages.Last();
			var box47TaxesTotalsLastPage = lastPage.Box47TaxesTotals;

			AssertEquals(6, box47TaxesTotalsLastPage.Count);

			AssertDocSADHLineTax(box47TaxesTotalsLastPage, index: 0, type: "A00", methodOfPayment: "A", amount: "10.05");

			AssertDocSADHLineTax(box47TaxesTotalsLastPage, index: 1, type: "A00", methodOfPayment: "E", amount: "103.61");

			AssertDocSADHLineTax(box47TaxesTotalsLastPage, index: 2, type: "914", methodOfPayment: "G", amount: "0.00");

			AssertDocSADHLineTax(box47TaxesTotalsLastPage, index: 3, type: "916", methodOfPayment: "A", amount: "0.14");

			AssertDocSADHLineTax(box47TaxesTotalsLastPage, index: 4, type: "405", methodOfPayment: "A", amount: "33.83");

			AssertDocSADHLineTax(box47TaxesTotalsLastPage, index: 5, type: "405", methodOfPayment: "G", amount: "270.66");
		});
	}

	public void TestBox47TotalAmountIT()
	{
		var pages = AddNewEntry();
		entryLines[0].Fees.Add(GetFee("405", "G", 136.08m));
		entryLines[0].Fees.Add(GetFee("914", "G", 0m));
		entryLines[0].Fees.Add(GetFee("A00", "E", 66.27m));

		var lastPage = (ITDocSADHPage)pages.Last();
		AssertEquals("in the first page", ZString.Empty, lastPage.Box47TotalAmount);

		pages = AddNewEntry();
		entryLines[1].Fees.Add(GetFee("B00", "R", 5.31m));
		entryLines[1].Fees.Add(GetFee("405", "G", 103.52m));
		entryLines[1].Fees.Add(GetFee("A00", "E", 28.72));

		pages = AddNewEntry();
		entryLines[2].Fees.Add(GetFee("405", "G", 31.06m));
		entryLines[2].Fees.Add(GetFee("A00", "E", 8.62));

		pages = AddNewEntry();
		entryLines[3].Fees.Add(GetFee("916", "A", 0.14m));
		entryLines[3].Fees.Add(GetFee("405", "A", 33.83m));
		entryLines[3].Fees.Add(GetFee("A00", "A", 1_010.05m));

		pages = AddNewEntry();
		entryLines[4].Fees.Add(GetFee("A00", "O", 5.31m));
		entryLines[4].Fees.Add(GetFee("A00", "R", 5.31m));
		entryLines[4].Fees.Add(GetFee("A00", "S", 28.72));
		entryLines[4].Fees.Add(GetFee("405", "U", 19.15m));
		entryLines[4].Fees.Add(GetFee("405", "V", 19.15m));
		var midPage = pages[1];
		lastPage = (ITDocSADHPage)pages.Last();

		AssertEquals("Page that is not the Last", ZString.Empty, midPage.Box47TotalAmount);

		AssertEquals("Last Page", "1418.29", lastPage.Box47TotalAmount);
	}

	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		return ITDocSADHPage.New(Factory, null);
	}

	protected override void SetUp()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
		invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
	}

	ITDocSADHPageCollection AddNewEntry()
	{
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.ZG_MethodOfPayment = "A";
		declaration.DoMerge(new Enterprise.Customs.Business.SendsMessagesToCustomsShutterUpperer());
		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryLines = entryHeader.MergedLines;

		return new ITDocSADHPageCollection(entryLines, Factory);
	}

	CusEntryLineFee GetFee(ZString type, ZString methodOfPay, ZDecimal amount)
	{
		var fee = Factory.New<CusEntryLineFee>();

		fee.CF_ChargeType = type;
		fee.CF_MethodOfPayment = methodOfPay;
		fee.CF_ChargeAmount = amount;
		return fee;
	}

	void AssertDocSADHLineTax(DocSADHLineTaxCollection box47TaxesTotalsLastPage, int index, string type, string methodOfPayment, string amount)
	{
		var docSADHLineTax = box47TaxesTotalsLastPage[index];
		AssertEquals(type, docSADHLineTax.G4_Type);
		AssertEquals(methodOfPayment, docSADHLineTax.G4_MethodOfPayment);
		AssertEquals(amount, docSADHLineTax.G4_Amount_InDeclarationCurrency);
	}

	JobDeclaration declaration;

	JobComInvoiceHeader invoiceHeader;

	ITCusEntryLineCollection entryLines;
}
