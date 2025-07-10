using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Accounting.Business.Riba.AccCollectionOrder;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	internal class AccCollectionOrderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckACO_Amount()
		{
			var order = Factory.NewWithValidTestData<AccCollectionOrder>();
			order.IncludeInBatch = true;
			order.ACO_Amount = 100m;
			AssertNoErrors(order.ACO_AmountInfo);
			order.ACO_Amount = 0m;
			AssertHasError(order.ACO_AmountInfo, "Order total amount can not be 0.");
			order.IncludeInBatch = false;
			order.ACO_Amount = 0m;
			AssertNoErrors(order.ACO_AmountInfo);
			order.ACO_Amount = -10m;
			AssertNoErrors(order.ACO_AmountInfo);
			order.IncludeInBatch = true;
			order.ACO_Amount = -10m;
			AssertHasError(order.ACO_AmountInfo, "Order total amount can not be negative.");
			AccountingMasterFilesRegistry.Instance.CollectionOrderMinimumAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			var errorForMinimumAmount = @"Please revise or exclude this collection order from this batch.The amount for this collection order is less than the minimum amount '100' in Local Currency stipulated in the ‘Minimum Collection Order Amount’ registry and you do not have access right to ‘Manage > Receivables > Collection Batch > Collection Order > Allow Order Below Minimum  Amount’.";
			order.ACO_Amount = 90;
			AssertNoError(order.ACO_AmountInfo, errorForMinimumAmount);
			var batch = Factory.NewWithValidTestData<AccCollectionBatch>();
			batch.ACB_GC = GlbCompany.CurrentCompany.PK;
			batch.ACB_BatchNumber = "0001000";
			order.ACO_ACB = batch.PK;
			order.IncludeInBatch = true;
			Env.Security.CollectionOrderAllowBelowMinAmount.IsAllowed = false;
			order.ACO_Amount = 90;
			AssertHasError(order.ACO_AmountInfo, errorForMinimumAmount);
			order.ACO_Amount = 150;
			AssertNoError(order.ACO_AmountInfo, errorForMinimumAmount);
			Env.Security.CollectionOrderAllowBelowMinAmount.IsAllowed = true;
			order.ACO_Amount = 90;
			AssertNoError(order.ACO_AmountInfo, errorForMinimumAmount);
			order.IncludeInBatch = false;
			Env.Security.CollectionOrderAllowBelowMinAmount.IsAllowed = false;
			order.ACO_Amount = 90;
			AssertNoError(order.ACO_AmountInfo, errorForMinimumAmount);
		}

		[TestDate(2016, 05, 06)]
		public void TestCheckACO_CollectionDate()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var batch = TestObjectCreator.CreateCollectionBatch(bankAccount, GlbCompany.CurrentCompany, "00001001", 100m, false);
			var invoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 20, 0M, 20, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK, ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);
			var order = TestObjectCreator.CreateCollectionOrder(batch, ZDateTime.Today.Date, TestObjectCreator.ABIGAS, "0000001", 50m, false);
			var line = TestObjectCreator.CreateCollectionOrderLine(order, invoice, false);

			order.IncludeInBatch = true;
			order.ACO_CollectionDate = new ZDate(2016, 05, 05);
			AssertHasError(order.ACO_CollectionDateInfo, "Collection date can not less than today.");

			order.IncludeInBatch = false;
			order.ACO_CollectionDate = new ZDate(2016, 05, 05);
			AssertNoErrors(order.ACO_CollectionDateInfo);

			order.IncludeInBatch = true;
			order.ACO_CollectionDate = new ZDate(2016, 05, 06);
			AssertNoErrors(order.ACO_CollectionDateInfo);

			order.ACO_CollectionDate = new ZDate(2016, 05, 07);
			AssertNoErrors(order.ACO_CollectionDateInfo);
			Factory.Save();

			TestDateAttribute.AddDays(4);
			order.ACO_Amount = 20;
			order.ACO_CollectionDate = new ZDate(2016, 05, 07);
			AssertNoErrors(order.ACO_CollectionDateInfo);

			order.ACO_CollectionDate = new ZDate(2016, 05, 08);
			AssertHasError(order.ACO_CollectionDateInfo, "Collection date can not less than today.");

			order.ACO_IsCancelled = true;
			order.ACO_CollectionDate = new ZDate(2016, 05, 09);
			AssertNoErrors(order.ACO_CollectionDateInfo);
		}

		public void TestCheckCollectionOrderWithDepositedDate()
		{
			var order = Factory.NewWithValidTestData<AccCollectionOrder>();
			order.ACO_Amount = 200m;
			order.ACO_DepositedDate = ZDate.Today;
			Factory.Save();

			order.IncludeInBatch = true;
			var expectedMessage = "The collection order has been completed with a deposited date saved. No further changes are allowed.";
			order.ACO_Amount = 100m;
			AssertHasError(order.ACO_AmountInfo, expectedMessage);

			order.ACO_CollectionDate = ZDate.Today.AddDays(10);
			AssertHasError(order.ACO_CollectionDateInfo, expectedMessage);

			order.IncludeInBatch = false;
			order.ACO_Amount = 100m;
			AssertNoErrors(order.ACO_AmountInfo);

			order.ACO_CollectionDate = ZDate.Today.AddDays(10);
			AssertNoErrors(order.ACO_CollectionDateInfo);

			order.IncludeInBatch = true;
			order.IsCancelled = true;

			order.ACO_Amount = 100m;
			AssertHasError(order.ACO_AmountInfo, expectedMessage);

			order.ACO_CollectionDate = ZDate.Today.AddDays(10);
			AssertNoErrors(order.ACO_CollectionDateInfo);
		}

		public void TestCheckACO_OH_Debtor()
		{
			var collectionOrder = Factory.New<AccCollectionOrder>();
			collectionOrder.DebtorValidationType = DebtorValidation.DebtorIsRequired;
			collectionOrder.ACO_OH_Debtor = ZGuid.Empty;

			AssertEquals("Precondition: DebtorValidationType", DebtorValidation.DebtorIsRequired, collectionOrder.DebtorValidationType);
			Assert("Precondition: ACO_OH_Debtor.IsEmpty", collectionOrder.ACO_OH_Debtor.IsEmpty);
			AssertHasError(collectionOrder.ACO_OH_DebtorInfo, "Please enter a Debtor.");

			collectionOrder.ACO_OH_Debtor = TestObjectCreator.Debtor.PK;
			AssertNoErrors(collectionOrder.ACO_OH_DebtorInfo);

			collectionOrder.DebtorValidationType = DebtorValidation.NoDebtorValidation;
			collectionOrder.Validation.ValidateACO_OH_Debtor();
			AssertNoErrors(collectionOrder.ACO_OH_DebtorInfo);
			AssertNoWarnings(collectionOrder.ACO_OH_DebtorInfo);

			collectionOrder.ACO_OH_Debtor = ZGuid.Empty;
			AssertNoErrors(collectionOrder.ACO_OH_DebtorInfo);
			AssertNoWarnings(collectionOrder.ACO_OH_DebtorInfo);

			collectionOrder.DebtorValidationType = DebtorValidation.DebtorShouldBeEmpty;
			collectionOrder.ACO_OH_Debtor = TestObjectCreator.Debtor.PK;
			AssertEquals("Precondition: DebtorValidationType", DebtorValidation.DebtorShouldBeEmpty, collectionOrder.DebtorValidationType);
			Assert("Precondition: ACO_OH_Debtor.IsEmpty", !collectionOrder.ACO_OH_Debtor.IsEmpty);
			AssertNoErrors(collectionOrder.ACO_OH_DebtorInfo);
			AssertHasWarning(collectionOrder.ACO_OH_DebtorInfo, "This collection order contains transactions belonging to more than one debtor, this value must be empty.");

			collectionOrder.ACO_OH_Debtor = ZGuid.Empty;
			AssertNoWarnings(collectionOrder.ACO_OH_DebtorInfo);
			AssertNoErrors(collectionOrder.ACO_OH_DebtorInfo);
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator => fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
	}
}
