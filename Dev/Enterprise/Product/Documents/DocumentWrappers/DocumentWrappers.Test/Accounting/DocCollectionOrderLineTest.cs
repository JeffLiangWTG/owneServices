using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Riba;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocCollectionOrderLine))]
	sealed class DocCollectionOrderLineTest : DocumentWrapperTestCase
	{
		public void TestProperties()
		{
			AssertEquals("TransactionType", line1.TransactionType, Wrapper.TransactionType);
			AssertEquals("TransactionNumber", line1.TransactionNumber, Wrapper.TransactionNumber);
			AssertEquals("DueDate", line1.DueDate, Wrapper.TransactionDueDate);
			AssertEquals("Description", line1.Description, Wrapper.TransactionDescription);
			AssertEquals("OSCurrencyCode", line1.OSCurrency, Wrapper.OSCurrencyCode);
			AssertEquals("LocalCurrencyCode", line1.CollectionCurrency, Wrapper.LocalCurrencyCode);
			AssertEquals("OSOutstandingAmount", line1.OSOutstandingAmount, Wrapper.OSOutstandingAmount);
			AssertEquals("LocalOutstandingAmount", line1.CollectionAmount, Wrapper.LocalOutstandingAmount);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocCollectionOrderLine.New(line1, Factory) };
		}

		DocCollectionOrderLine Wrapper;
		AccCollectionOrder order;

		AccCollectionBatch batch;
		AccCollectionOrderLine line1;
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
			line1 = TestObjectCreator.CreateCollectionOrderLine(order, invoice1, false);
			TestObjectCreator.CreateCollectionOrderLine(order, invoice2, false);
			Factory.Save();
			Wrapper = (DocCollectionOrderLine)GetDocumentWrappers()[0];

			base.SetUp();
		}
	}
}
