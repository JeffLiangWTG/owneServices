using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public abstract class DependentTransactionLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild_BranchDepartment()
		{
			var collection = (DependentTransactionLineCollection)GetCollectionToTest();
			var expectedBranchPK = TestObjectCreator.NonCurrentBranch.PK;
			var expectedDepartmetPK = TestObjectCreator.NonCurrentDepartment.PK;
			collection.TransactionHeader.AH_GB = expectedBranchPK;
			collection.TransactionHeader.AH_GE = expectedDepartmetPK;

			var line = collection.AddNew();
			AssertEquals("Branch", expectedBranchPK, line.AL_GB);
			AssertEquals("Department", expectedDepartmetPK, line.AL_GE);
		}

		public void TestSetDefaultsForNewChild_TaxBranch()
		{
			var collection = (DependentTransactionLineCollection)GetCollectionToTest();
			var expectedBranchPK = TestObjectCreator.NonCurrentBranch.PK;
			collection.TransactionHeader.AH_GB_TaxBranch = expectedBranchPK;

			var line = collection.AddNew();
			AssertEquals(expectedBranchPK, line.AL_GB_TaxBranch);
		}

		public void TestShowGLAccountsForImportAction()
		{
			var collection = (DependentTransactionLineCollection)GetCollectionToTest();
			collection.ShowGLAccountsForImportAction = (glHeaderCollection, glHeaderList) => { glHeaderList.Add(Factory.New<AccGLHeader>()); };
			var line = collection.AddNew();
			AssertNotNull(line.ShowGLAccountsForImportAction);
			AssertEquals(collection.ShowGLAccountsForImportAction, line.ShowGLAccountsForImportAction);
		}

		public void TestRemovingFromCollectionRecalulatesAllAmounts()
		{
			Invoice invoice = CreateInvoice(typeof(APInvoice), NewCurrency, .7M);
			InvoiceLine line1 = CreateInvoiceLine(invoice, NewCurrency, .7M, 100M);
			InvoiceLine line2 = CreateInvoiceLine(invoice, NewCurrency, .7M, 200M);
			InvoiceLine line3 = CreateInvoiceLine(invoice, NewCurrency, .7M, 300M);
			InvoiceLine line4 = CreateInvoiceLine(invoice, NewCurrency, .7M, 400M);

			AssertEquals("OS Ex Tax Amount", 1000M, invoice.AH_OSExTaxAmount);
			AssertEquals("OS Tax Amount   ", 100M, invoice.AH_OSTaxAmount);
			AssertEquals("OS WHT Amount   ", 50M, invoice.AH_OSWHTAmount);
			AssertEquals("OS Total Amount ", 1100M, invoice.AH_OSTotalAmount);

			AssertEquals("Local Ex Tax Amount", 1428.57M, invoice.AH_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 142.86M, invoice.AH_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 71.43M, invoice.AH_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 1571.43M, invoice.AH_LocalTotalAmount);

			invoice.Lines.RemoveAndDelete(line4);
			AssertEquals("OS Ex Tax Amount", 600M, invoice.AH_OSExTaxAmount);
			AssertEquals("OS Tax Amount   ", 60M, invoice.AH_OSTaxAmount);
			AssertEquals("OS WHT Amount   ", 30M, invoice.AH_OSWHTAmount);
			AssertEquals("OS Total Amount ", 660M, invoice.AH_OSTotalAmount);

			AssertEquals("Local Ex Tax Amount", 857.14M, invoice.AH_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 85.72M, invoice.AH_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 42.86M, invoice.AH_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 942.86M, invoice.AH_LocalTotalAmount);

			invoice.Lines.RemoveAndDelete(line3);
			AssertEquals("OS Ex Tax Amount", 300M, invoice.AH_OSExTaxAmount);
			AssertEquals("OS Tax Amount   ", 30M, invoice.AH_OSTaxAmount);
			AssertEquals("OS WHT Amount   ", 15M, invoice.AH_OSWHTAmount);
			AssertEquals("OS Total Amount ", 330M, invoice.AH_OSTotalAmount);

			AssertEquals("Local Ex Tax Amount", 428.57M, invoice.AH_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 42.86M, invoice.AH_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 21.43M, invoice.AH_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 471.43M, invoice.AH_LocalTotalAmount);

			invoice.Lines.RemoveAndDelete(line2);
			AssertEquals("OS Ex Tax Amount", 100M, invoice.AH_OSExTaxAmount);
			AssertEquals("OS Tax Amount   ", 10M, invoice.AH_OSTaxAmount);
			AssertEquals("OS WHT Amount   ", 5M, invoice.AH_OSWHTAmount);
			AssertEquals("OS Total Amount ", 110M, invoice.AH_OSTotalAmount);

			AssertEquals("Local Ex Tax Amount", 142.86M, invoice.AH_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 14.29M, invoice.AH_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 7.14M, invoice.AH_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 157.15M, invoice.AH_LocalTotalAmount);

			invoice.Lines.RemoveAndDelete(line1);
			AssertEquals("Invoice Amount", 0M, invoice.AH_InvoiceAmount);
			AssertEquals("OS Ex Tax Amount", 0M, invoice.AH_OSExTaxAmount);
			AssertEquals("OS Tax Amount   ", 0M, invoice.AH_OSTaxAmount);
			AssertEquals("OS WHT Amount   ", 0M, invoice.AH_OSWHTAmount);
			AssertEquals("OS Total Amount ", 0M, invoice.AH_OSTotalAmount);

			AssertEquals("Local Ex Tax Amount", 0M, invoice.AH_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 0M, invoice.AH_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 0M, invoice.AH_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 0M, invoice.AH_LocalTotalAmount);
		}

		public void TestRecalculationAmountsForDeletedTransaction()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			invoice.Lines.AddNew();

			invoice.Delete();
			Assert("Precondition: Test transaction must be deleted.", invoice.IsDeleted);
			invoice.Lines.Add(Factory.New<APInvoiceLine>());
			Assert("Postcondition:We got this far! If update headaer amounts is called when addign new lines to a deleted header and excpetion will be raise", true);
		}

		public void TestSetDefaultsForNewChildAL_Sequence()
		{
			var collection = (DependentTransactionLineCollection)GetCollectionToTest();
			if (!collection.AllowNew)
			{
				Assert(true);
				return;
			}

			var line1 = collection.AddNew();
			AssertEquals("AL_Sequence", (short)1, line1.AL_Sequence);

			var line2 = collection.AddNew();
			AssertEquals("AL_Sequence", (short)2, line2.AL_Sequence);

			var line3 = collection.AddNew();
			AssertEquals("AL_Sequence", (short)3, line3.AL_Sequence);

			collection.Remove(line2);

			var line4 = collection.AddNew();
			AssertEquals("AL_Sequence", (short)4, line4.AL_Sequence);

			line3.AL_Sequence = short.MaxValue;
			line4.AL_Sequence = 1;
			var line5 = collection.AddNew();
			AssertEquals("AL_Sequence", (short)2, line5.AL_Sequence);

			collection.GetPositiveLineSequencesCalls_ForTestOnly = 0;
			DependentTransactionLine line6, line7, line8;
			using (collection.SuspendListChanged())
			{
				line6 = collection.AddNew();
				line7 = collection.AddNew();
				line7.AL_Sequence = 14;
				line8 = collection.AddNew();
			}
			AssertEquals(3, collection.GetPositiveLineSequencesCalls_ForTestOnly);
			AssertEquals("AL_Sequence", (short)3, line6.AL_Sequence);
			AssertEquals("AL_Sequence", (short)14, line7.AL_Sequence);
			AssertEquals("AL_Sequence", (short)4, line8.AL_Sequence);
		}

		public void TestSetDefaultsForNewChildAL_Sequence_Case2()
		{
			var collection = (DependentTransactionLineCollection)GetCollectionToTest();
			if (!collection.AllowNew)
			{
				Assert(true);
				return;
			}

			collection.GetPositiveLineSequencesCalls_ForTestOnly = 0;
			using (collection.SuspendListChanged())
			{
				var line1 = collection.AddNew();
				var line2 = collection.AddNew();
				var line3 = collection.AddNew();
				line3.AL_Sequence = 7;
				var line4 = collection.AddNew();
				line4.AL_Sequence = 1;
				var line5 = collection.AddNew();
				var line6 = collection.AddNew();
				var line7 = collection.AddNew();
				line7.AL_Sequence = 14;
				var line8 = collection.AddNew();

				collection.Remove(line2);
			}
			AssertEquals(1, collection.GetPositiveLineSequencesCalls_ForTestOnly);
			AssertEquals("AL_Sequence", (short)15, collection[0].AL_Sequence);
			AssertEquals("AL_Sequence", (short)7, collection[1].AL_Sequence);
			AssertEquals("AL_Sequence", (short)1, collection[2].AL_Sequence);
			AssertEquals("AL_Sequence", (short)16, collection[3].AL_Sequence);
			AssertEquals("AL_Sequence", (short)17, collection[4].AL_Sequence);
			AssertEquals("AL_Sequence", (short)14, collection[5].AL_Sequence);
			AssertEquals("AL_Sequence", (short)18, collection[6].AL_Sequence);
		}

		public void TestSetDefaultsForNewChildAL_Sequence_Case3()
		{
			var collection = (DependentTransactionLineCollection)GetCollectionToTest();
			if (!collection.AllowNew)
			{
				Assert(true);
				return;
			}

			var line1 = collection.AddNew();
			var line2 = collection.AddNew();

			collection.GetPositiveLineSequencesCalls_ForTestOnly = 0;
			using (collection.SuspendListChanged())
			{
				var line3 = collection.AddNew();
				line3.AL_Sequence = 7;
				var line4 = collection.AddNew();
				line4.AL_Sequence = 1;
				var line5 = collection.AddNew();
				var line6 = collection.AddNew();
				var line7 = collection.AddNew();
				line7.AL_Sequence = 14;
				var line8 = collection.AddNew();

				collection.Remove(line2);
			}
			AssertEquals(1, collection.GetPositiveLineSequencesCalls_ForTestOnly);
			AssertEquals("AL_Sequence", (short)1, collection[0].AL_Sequence);
			AssertEquals("AL_Sequence", (short)7, collection[1].AL_Sequence);
			AssertEquals("AL_Sequence", (short)1, collection[2].AL_Sequence);
			AssertEquals("AL_Sequence", (short)15, collection[3].AL_Sequence);
			AssertEquals("AL_Sequence", (short)16, collection[4].AL_Sequence);
			AssertEquals("AL_Sequence", (short)14, collection[5].AL_Sequence);
			AssertEquals("AL_Sequence", (short)17, collection[6].AL_Sequence);
		}

		public void TestSetDefaultsForNewChildAL_Sequence_Case4()
		{
			var collection = (DependentTransactionLineCollection)GetCollectionToTest();
			if (!collection.AllowNew)
			{
				Assert(true);
				return;
			}

			var line1 = collection.AddNew();
			var line2 = collection.AddNew();

			collection.GetPositiveLineSequencesCalls_ForTestOnly = 0;
			using (collection.SuspendListChanged())
			{
				collection.Remove(line2);
				var line3 = collection.AddNew();
				var line4 = collection.AddNew();
				line4.AL_Sequence = 5;
				var line5 = collection.AddNew();
				var line6 = collection.AddNew();
			}
			AssertEquals(1, collection.GetPositiveLineSequencesCalls_ForTestOnly);
			AssertEquals("AL_Sequence", (short)1, collection[0].AL_Sequence);
			AssertEquals("AL_Sequence", (short)6, collection[1].AL_Sequence);
			AssertEquals("AL_Sequence", (short)5, collection[2].AL_Sequence);
			AssertEquals("AL_Sequence", (short)7, collection[3].AL_Sequence);
			AssertEquals("AL_Sequence", (short)8, collection[4].AL_Sequence);
		}

		public void TestSetDefaultsForNewChildAL_Sequence_Case5()
		{
			var collection = (DependentTransactionLineCollection)GetCollectionToTest();
			if (!collection.AllowNew)
			{
				Assert(true);
				return;
			}

			var line1 = collection.AddNew();
			var line2 = collection.AddNew();

			collection.GetPositiveLineSequencesCalls_ForTestOnly = 0;
			using (collection.SuspendListChanged())
			{
				var line3 = collection.AddNew();
				var line4 = collection.AddNew();
			}
			AssertEquals(1, collection.GetPositiveLineSequencesCalls_ForTestOnly);
			AssertEquals("AL_Sequence", (short)1, collection[0].AL_Sequence);
			AssertEquals("AL_Sequence", (short)2, collection[1].AL_Sequence);
			AssertEquals("AL_Sequence", (short)3, collection[2].AL_Sequence);
			AssertEquals("AL_Sequence", (short)4, collection[3].AL_Sequence);
		}

		public void TestSetDefaultsForNewChildAL_Sequence_Case6()
		{
			var collection = (DependentTransactionLineCollection)GetCollectionToTest();
			if (!collection.AllowNew)
			{
				Assert(true);
				return;
			}

			collection.GetPositiveLineSequencesCalls_ForTestOnly = 0;
			using (collection.SuspendListChanged())
			{
				var line1 = collection.AddNew();
				var line2 = collection.AddNew();
				var line3 = collection.AddNew();
				var line4 = collection.AddNew();
			}
			AssertEquals(1, collection.GetPositiveLineSequencesCalls_ForTestOnly);
			AssertEquals("AL_Sequence", (short)1, collection[0].AL_Sequence);
			AssertEquals("AL_Sequence", (short)2, collection[1].AL_Sequence);
			AssertEquals("AL_Sequence", (short)3, collection[2].AL_Sequence);
			AssertEquals("AL_Sequence", (short)4, collection[3].AL_Sequence);
		}

		public void TestSetDefaultsForNewChildAL_Sequence_Case7()
		{
			var collection = (DependentTransactionLineCollection)GetCollectionToTest();
			if (!collection.AllowNew)
			{
				Assert(true);
				return;
			}

			collection.GetPositiveLineSequencesCalls_ForTestOnly = 0;
			using (collection.SuspendListChanged())
			{
				var line1 = collection.AddNew();
				line1.AL_Sequence = short.MaxValue - 1;
				var line2 = collection.AddNew();
				var line3 = collection.AddNew();
				var line4 = collection.AddNew();
			}
			AssertEquals(3, collection.GetPositiveLineSequencesCalls_ForTestOnly);
			AssertEquals("AL_Sequence", (short)(short.MaxValue - 1), collection[0].AL_Sequence);
			AssertEquals("AL_Sequence", short.MaxValue, collection[1].AL_Sequence);
			AssertEquals("AL_Sequence", (short)1, collection[2].AL_Sequence);
			AssertEquals("AL_Sequence", (short)2, collection[3].AL_Sequence);
		}

		public virtual void FAT_TestSetDefaultsForNewChildAL_Sequence_Case8()
		{
			Factory.SuspendValidation();
			try
			{
				var collection = (DependentTransactionLineCollection)GetCollectionToTest();
				if (!collection.AllowNew)
				{
					Assert(true);
					return;
				}

				collection.GetPositiveLineSequencesCalls_ForTestOnly = 0;
				using (collection.SuspendListChanged())
				{
					for (short i = 0; i < short.MaxValue; i++)
					{
						var line = collection.AddNew();
						line.AL_Sequence = (short)(i + 1);
					}
				}
				AssertEquals(0, collection.GetPositiveLineSequencesCalls_ForTestOnly);

				collection.GetPositiveLineSequencesCalls_ForTestOnly = 0;
				DependentTransactionLine line1, line2, line3, line4, line5;
				using (collection.SuspendListChanged())
				{
					line1 = collection.AddNew();
					line2 = collection.AddNew();
					line3 = collection.AddNew();
					line4 = collection.AddNew();
					line5 = collection.AddNew();
				}
				AssertEquals(2, collection.GetPositiveLineSequencesCalls_ForTestOnly);
				AssertEquals("AL_Sequence", short.MaxValue, line1.AL_Sequence);
				AssertEquals("AL_Sequence", short.MaxValue, line2.AL_Sequence);
				AssertEquals("AL_Sequence", short.MaxValue, line3.AL_Sequence);
				AssertEquals("AL_Sequence", short.MaxValue, line4.AL_Sequence);
				AssertEquals("AL_Sequence", short.MaxValue, line5.AL_Sequence);
			}
			finally
			{
				Factory.ResumeValidation();
			}
		}

		public void TestSuspendListChangedAdditionalSuspenders()
		{
			var collection = GetCollectionToTest() as DependentTransactionLineCollection;
			AssertNotNull(collection);
			using (var suspender = collection.SuspendListChanged())
			{
				var list = suspender as DisposableList;
				AssertNotNull("Is DisposableList", list);
				Assert("Contains more than one element", list.Count > 1);
				Assert("Header Amounts Update is suspended", collection.TransactionHeader.IsHeaderAmountsUpdateSuspended);
			}
		}

		public void TestAddingNonCommittedElementsDoesntCauseValidationErrors()
		{
			var collection = GetCollectionToTest() as DependentTransactionLineCollection;

			var newNonCommittedElement = (BusinessObject)((IBindingList)collection).AddNew();
			AssertNoNotifications("Uncommitted collection element should have any errors.", newNonCommittedElement);
			AssertNoNotifications("Collection Master should have any errors after adding uncommitted element.", collection.Master);

			((ICancelAddNew)collection).CancelNew(collection.Count - 1);
			AssertNoNotifications("Collection Master should have any errors after canceling uncommitted element.", collection.Master);
		}

		public void TestPlaceOfSupplyIsSetFromHeader()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var collection = GetCollectionToTest() as DependentTransactionLineCollection;
				var header = collection.Master as TransactionHeader;
				header.AH_PlaceOfSupply = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[0].Code;

				var line = (TransactionLine)((IBindingList)collection).AddNew();
				AssertEquals("PlaceOfSupply", header.AH_PlaceOfSupply, line.AL_PlaceOfSupply);
				AssertEquals("PlaceOfSupplyType", header.AH_PlaceOfSupplyType, line.AL_PlaceOfSupplyType);
			}
		}

		public void TestTransactionAmountNotUpdateAfterTransactionLineDescriptionChangeAndDataRefresh()
		{
			var job = TestObjectCreator.CreateJob("Job1", TestObjectCreator.LocalClient, 1, TestObjectCreator.Agent, 1);
			var invoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1, TestObjectCreator.AALSHI);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 100);
			job.JH_JobLocalReference = "123";
			line.AL_JH = job.PK;
			line.AL_AC = TestObjectCreator.CC1.PK;
			var charge = TestObjectCreator.CreateCharge(line);

			Factory.Save();

			invoice.AH_FullyPaidDate = ZDateTime.Today;
			invoice.AH_OutstandingAmount = 0m;

			TransactionMatchLinkGroup matchLinks = new TransactionMatchLinkGroup(Factory);
			TransactionMatchLink matchLink = matchLinks.AddNew();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = invoice.AH_InvoiceAmount;

			var arReceipt = Factory.NewWithValidTestData<ARReceipt>();
			arReceipt.AH_OSExTaxAmount = invoice.AH_OSExTaxAmount;
			arReceipt.AH_LocalExTaxAmount = arReceipt.AH_OSExTaxAmount;
			arReceipt.AH_OutstandingAmount = 0M;
			arReceipt.AH_FullyPaidDate = ZDateTime.Today;

			matchLink = matchLinks.AddNew();
			matchLink.AP_AH = arReceipt.PK;
			matchLink.AP_Amount = arReceipt.AH_InvoiceAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLinks);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var lineForNewFactory = newFactory.Load<ARInvoiceLine>(line.PK);

			lineForNewFactory.AL_Desc = "AAAA";

			AssertEquals("Precondition", "tee he he", line.AL_Desc);
			AssertEquals("Precondition", 0m, invoice.AH_OutstandingAmount);

			newFactory.Save();

			AssertEquals("AAAA", line.AL_Desc);
			AssertEquals("OutstandingAmount should not be recalculated.", 0m, invoice.AH_OutstandingAmount);
		}

		public void TestRelationshipFilterHasNotNullPredicateOnAL_AH()
		{
			var collection = (DependentTransactionLineCollection)GetCollectionToTest();
			AssertContains("Filter should contain 'AL_AH is not NULL' predicate to hint to SQL Server to use filtered index on AL_AH", "AL_AH is not NULL", collection.CompleteFilter.LiteralTextSqlFormatted);
		}

		#region Implementation

		protected Invoice CreateInvoice(Type invoiceType, RefCurrency currency, decimal exchangeRate)
		{
			Invoice invoice = (Invoice)Factory.New(invoiceType);
			invoice.AH_Desc = "Test Invoice";
			invoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			invoice.AH_ExchangeRate = exchangeRate;
			return invoice;
		}

		protected InvoiceLine CreateInvoiceLine(Invoice invoice, RefCurrency currency, decimal exchangeRate, decimal aH_OSExTaxAmount)
		{
			InvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_RX_NKTransactionCurrency = currency.RX_Code;
			line.AL_AT = GST.PK;
			line.AL_AW = WHT.PK;
			line.AL_ExchangeRate = exchangeRate;
			line.AL_OSExTaxAmount = aH_OSExTaxAmount;
			return line;
		}

		protected RefCurrency fNewCurrency;
		protected RefCurrency NewCurrency
		{
			get
			{
				if (fNewCurrency == null)
				{
					fNewCurrency = Factory.New<RefCurrency>();
					fNewCurrency.RX_Code = "XXX";
				}
				return fNewCurrency;
			}
		}

		protected AccTaxRate fGST;
		protected AccTaxRate GST
		{
			get
			{
				if (fGST == null)
				{
					fGST = Factory.New<AccTaxRate>();
					fGST.AT_Code = "GST";
					fGST.SetRateNumerator_ForTestOnly(10);
				}
				return fGST;
			}
		}

		protected AccWithholding fWHT;
		protected AccWithholding WHT
		{
			get
			{
				if (fWHT == null)
				{
					fWHT = Factory.New<AccWithholding>();
					fWHT.AW_Code = "WHT";
					fWHT.AW_Rate = 5;
				}
				return fWHT;
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
