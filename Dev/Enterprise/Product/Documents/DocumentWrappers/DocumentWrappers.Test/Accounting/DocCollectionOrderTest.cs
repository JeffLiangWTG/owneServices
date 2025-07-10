using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Riba;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocCollectionOrder))]
	sealed class DocCollectionOrderTest : DocumentWrapperTestCase
	{
		public void TestPropertiesOfDocCollectionOrder()
		{
			AssertEquals(TestObjectCreator.ABIGAS.OH_Code, Wrapper.AccountCode);
			AssertEquals(new ZDate(2019, 1, 1), Wrapper.CollectionDate);
			AssertEquals("00001001", Wrapper.CollectionBatchNumber);
			AssertEquals("0000001", Wrapper.CollectionOrderNumber);
			AssertEquals(bankAccount.AB_Code, Wrapper.CollectionBankCode);
			AssertEquals(order.ACO_RX_NKCurrency, Wrapper.CollectionBatchBankCurrency);
			AssertEquals(order.ACO_Amount, Wrapper.OrderTotalAmount);
			AssertEquals(order.CollectionRequestBankName, Wrapper.DebtorBankName);
			AssertEquals(order.CollectionRequestAccountName, Wrapper.DebtorBankAccountName);
			AssertEquals(order.CollectionRequestBankBsb, Wrapper.DebtorBankAndBranchCode);
			AssertEquals(order.CollectionRequestBankSwift, Wrapper.DebtorBankSwift);
			AssertEquals(order.CollectionRequestBankCountry, Wrapper.DebtorBankCountry);
			AssertEquals(order.CollectionRequestAccountCurrency, Wrapper.DebtorBankAccountCurrency);
			AssertEquals(order.CollectionRequestAccountNumber, Wrapper.DebtorBankAccountNumber);
			AssertEquals(order.CollectionRequestIBANNumber, Wrapper.DebtorBankAccountIBAN);
		}

		public void TestAccountCode()
		{
			AssertNotNull("Precondition: CollectionOrder.Debtor", order.Debtor);
			AssertEquals(TestObjectCreator.ABIGAS, order.Debtor);
			AssertEquals(TestObjectCreator.ABIGAS.OH_Code, Wrapper.AccountCode);

			order.ACO_OH_Debtor = ZGuid.Empty;
			AssertNull("Precondition: CollectionOrder.Debtor", order.Debtor);
			AssertEquals(ZString.Empty, Wrapper.AccountCode);
		}

		public void TestOrderLines()
		{
			AssertEquals(2, Wrapper.OrderLines.Count);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocCollectionOrder.New(order, Factory) };
		}

		DocCollectionOrder Wrapper;
		AccCollectionOrder order;

		AccCollectionBatch batch;
		AccBankAccount bankAccount;
		ARInvoice invoice1;
		ARInvoice invoice2;

		protected override void SetUp()
		{
			bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			batch = TestObjectCreator.CreateCollectionBatch(bankAccount, GlbCompany.CurrentCompany, "00001001", 100m, false);
			invoice1 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 20, 0M, 20, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK, ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);
			invoice2 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", TestObjectCreator.AUD, 1M, 30, 0M, 30, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK, ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);
			order = TestObjectCreator.CreateCollectionOrder(batch, ZDateTime.Today.Date, TestObjectCreator.ABIGAS, "0000001", 50m, false);
			TestObjectCreator.CreateCollectionOrderLine(order, invoice1, false);
			TestObjectCreator.CreateCollectionOrderLine(order, invoice2, false);
			Factory.Save();
			Wrapper = (DocCollectionOrder)GetDocumentWrappers()[0];
			order.ACO_CollectionDate = new ZDate(2019, 1, 1);

			base.SetUp();
		}
	}
}
