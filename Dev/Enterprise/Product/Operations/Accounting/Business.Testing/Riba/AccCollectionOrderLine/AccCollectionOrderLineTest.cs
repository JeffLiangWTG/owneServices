using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	using Enterprise.Accounting.Business.ARAP.Invoicing;
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(AccCollectionOrderLine))]
	public class AccCollectionOrderLineTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<AccCollectionOrderLine>();
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesAccCollectionOrderLine()
		{
			var localList = new List<string> {
				nameof(line1_1.LocalInvoiceAmount),
				nameof(line1_1.LocalOutstandingAmount),
			};

			var osList = new List<string>
			{
				nameof(line1_1.OSInvoiceAmount),
				nameof(line1_1.OSOutstandingAmount),
			};

			var collectionList = new List<string>
			{
				nameof(line1_1.CollectionAmount)
			};

			var tester = new DecimalPlacesAttributeTester(line1_1, line1_1.Transaction.Company);
			tester.CheckLocalCurrency(localList, nameof(line1_1.LocalDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(line1_1.OSDecimals), nameof(line1_1.Transaction.AH_RX_NKTransactionCurrency), line1_1.Transaction);
			tester.CheckNonLocalCurrency(collectionList, nameof(line1_1.CollectionAmountDecimals), nameof(line1_1.CollectionOrder.CollectionBatch.ACB_RX_NKCurrency), line1_1.CollectionOrder.CollectionBatch);
		}

		public void TestCollectonOrder()
		{
			AssertNotNull(line1_1.CollectionOrder);
			AssertEquals(line1_1.CollectionOrder.PK, order1.PK);
		}

		public void TestIsMatchedWithReceipt()
		{
			invoice1.AH_OutstandingAmount = 100m;
			batch.ACB_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Assert(!line1_1.IsMatchedWithReceipt);
			invoice1.AH_OutstandingAmount = 0m;
			invoice1.AH_FullyPaidDate = ZDateTime.Empty;
			Assert(!line1_1.IsMatchedWithReceipt);
			invoice1.AH_FullyPaidDate = ZDateTime.Today;
			Assert(line1_1.IsMatchedWithReceipt);
		}

		public void TestIncludeInOrder()
		{
			batch.ACB_TotalAmount = 100m;
			order1.ACO_Amount = 30m;
			order1.IncludeInBatch = true;
			AssertEquals(130m, batch.ACB_TotalAmount);
			line1_1.IncludeInOrder = true;
			AssertEquals(60m, line1_1.CollectionAmount);
			AssertEquals(90m, order1.ACO_Amount);
			AssertEquals(190m, batch.ACB_TotalAmount);
		}

		public void TestCollectionCurrency()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, batch.ACB_RX_NKCurrency);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, order1.ACO_RX_NKCurrency);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, line1_1.CollectionCurrency);
			batch.ACB_RX_NKCurrency = "EUR";
			AssertEquals("EUR", batch.ACB_RX_NKCurrency);
			AssertEquals("EUR", order1.ACO_RX_NKCurrency);
			AssertEquals("EUR", line1_1.CollectionCurrency);
		}

		public void TestCollectionAmount()
		{
			invoice1.AH_OutstandingAmount = 100m;
			invoice1.AH_OSTotal = 120m;
			batch.ACB_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(100m, line1_1.CollectionAmount);
			batch.ACB_RX_NKCurrency = "EUR";
			AssertNotEquals(line1_1.CollectionCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			// GetHighPrecisionOutstandingAmount: OSTotal - ( OSTotal/InvoiceAmount x (InvoiceAmount - OutstandingAmount) ) = 120 - ( 120/60 x (60-100) ) = 200
			AssertEquals(200m, line1_1.CollectionAmount);
		}

		public void TestIncludeInOrder_ReadOnly()
		{
			line1_1.IsCancelled = false;
			Assert(!line1_1.IncludeInOrder_ReadOnly);
			invoice1.AH_OutstandingAmount = 100m;
			batch.ACB_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Assert(!line1_1.IsMatchedWithReceipt);
			Assert(!line1_1.IncludeInOrder_ReadOnly);
			invoice1.AH_OutstandingAmount = 0m;
			invoice1.AH_FullyPaidDate = ZDateTime.Today;
			Assert(line1_1.IsMatchedWithReceipt);
			Assert(line1_1.IncludeInOrder_ReadOnly);
			line1_1.IsCancelled = true;
			Assert(line1_1.IncludeInOrder_ReadOnly);
		}

		public void TestTransactionDescription()
		{
			AssertEquals("Test Des", line1_1.Description);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Deletion is tested in TestDelete method", true);
		}

		AccCollectionBatch batch;
		AccCollectionOrder order1;
		AccCollectionOrderLine line1_1;
		TestObjectCreator creator;
		AccBankAccount bankAccount;
		ARInvoice invoice1;
		protected override void SetUp()
		{
			base.SetUp();
			creator = new TestObjectCreator(Factory);
			batch = Factory.New<AccCollectionBatch>();
			bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			batch.ACB_AB = bankAccount.PK;
			batch.ACB_GC = GlbCompany.CurrentCompany.PK;
			batch.IsCancelled = false;
			batch.ACB_BatchNumber = ZString.Empty;
			batch.ACB_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			batch.ACB_TotalAmount = 60m;
			invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_OutstandingAmount = 60m;
			invoice1.AH_InvoiceAmount = 60m;
			invoice1.AH_Desc = "Test Des";
			order1 = Factory.New<AccCollectionOrder>();
			order1.ACO_ACB = batch.PK;
			order1.ACO_CollectionDate = ZDateTime.Today.Date;
			order1.ACO_IsCancelled = false;
			order1.ACO_OH_Debtor = creator.ABIGAS.PK;
			order1.ACO_OrderNumber = ZString.Empty;
			order1.ACO_Amount = 60m;
			line1_1 = Factory.New<AccCollectionOrderLine>();
			line1_1.AOL_ACO = order1.PK;
			line1_1.AOL_AH = invoice1.PK;
			line1_1.IsCancelled = false;
		}
	}
}
