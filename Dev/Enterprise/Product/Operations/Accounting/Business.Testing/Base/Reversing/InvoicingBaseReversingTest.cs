using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Base.Reversing.Testing
{
	class InvoicingBaseReversingTest : PayablesAndReceivablesReversingTest
	{
		public void TestShouldShowReverseConfirmationMessage_HasRealisedAPPaymentRetentionRecords()
		{
			InvoicingReversing.OriginalInvocingBase_ForTestOnly.AH_TransactionNum = "SOME NUMBER";
			InvoicingReversing.OriginalInvocingBase_ForTestOnly.AH_TransactionType = "DDD";

			Assert("Precondition: ShouldShowReverseConfirmationMessage", !InvoicingReversing.ShouldShowReverseConfirmationMessage());
			AssertEquals("Precondition: GetReverseConfirmationMessage", "", InvoicingReversing.GetReverseConfirmationMessage());

			var taxProcessor = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessor.Object);

			taxProcessor.Setup(x => x.HasRealisedAPPaymentRetentionRecords(TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(InvoicingReversing.OriginalInvocingBase_ForTestOnly))).Returns(true);
			Assert("ShouldShowReverseConfirmationMessage", InvoicingReversing.ShouldShowReverseConfirmationMessage());
			AssertEquals("GetReverseConfirmationMessage",
@"This transaction AP DDD 'SOME NUMBER' is linked to Realized Payments Basis Withholding Tax Journals. Do you want to proceed?
Select 'No' to cancel this reversal action should you want to review and reverse AP PBW JNL/s first.
Select 'Yes' to proceed without reviewing the related AP PBW JNL transaction/s.", InvoicingReversing.GetReverseConfirmationMessage());

			taxProcessor.Setup(x => x.HasRealisedAPPaymentRetentionRecords(TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(InvoicingReversing.OriginalInvocingBase_ForTestOnly))).Returns(false);
			Assert("ShouldShowReverseConfirmationMessage", !InvoicingReversing.ShouldShowReverseConfirmationMessage());
			AssertEquals("GetReverseConfirmationMessage", "", InvoicingReversing.GetReverseConfirmationMessage());
		}

		#region Disallow Reversing Original Transactions When Amendments Are Not Reversed Tests

		public void TestRelatedAmendingTransactionsAreLoadedOnlyIfOriginalTransactionImplementsIAmending()
		{
			using (AccountingConfigurationRegistry.Instance.DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				var testObjectCreator = new TestObjectCreator(newFactory);
				var adjustmentNote = testObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>(TestObjectCreator.GetRandomString(3), 100m, 0m, ZDate.Today, testObjectCreator.ABIGAS.PK);
				Assert("AR Adjustment Note does not implement IAmending", !(adjustmentNote is IAmending));
				newFactory.Save();

				var reversing = new InvoicingBaseReversing(adjustmentNote);
				var dbHitsBefore = newFactory.GetTableHitCount(AccTransactionHeaderSchema.Constants.TableName);
				var result = reversing.HasNonCancelledAmendingTransactions_ForTestOnly;
				AssertEquals("Should not hit DB, when we know beforehand that this transaction type cannot be amended", dbHitsBefore, newFactory.GetTableHitCount(AccTransactionHeaderSchema.Constants.TableName));
			}
		}

		public void TestReversingOriginalTransaction_WhenNoAmendmentsAreAvailable()
		{
			using (AccountingConfigurationRegistry.Instance.DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertReversingOriginalTransactionWithAmendments(false, false, true, true);
				AssertReversingOriginalTransactionWithAmendments(false, false, true, false);
			}
			using (AccountingConfigurationRegistry.Instance.DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				AssertReversingOriginalTransactionWithAmendments(false, false, true, true);
				AssertReversingOriginalTransactionWithAmendments(false, false, true, false);
			}
		}

		public void TestReversingOriginalTransaction_WhenAmendmentsAreAvailableAndTheyAreReversed()
		{
			using (AccountingConfigurationRegistry.Instance.DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertReversingOriginalTransactionWithAmendments(true, true, true, true);
				AssertReversingOriginalTransactionWithAmendments(true, true, true, false);
			}
			using (AccountingConfigurationRegistry.Instance.DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				AssertReversingOriginalTransactionWithAmendments(true, true, true, true);
				AssertReversingOriginalTransactionWithAmendments(true, true, true, false);
			}
		}

		public void TestReversingOriginalTransaction_WhenAmendmentsAreAvailableAndTheyAreNotReversed()
		{
			using (AccountingConfigurationRegistry.Instance.DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertReversingOriginalTransactionWithAmendments(true, false, false, true);
				AssertReversingOriginalTransactionWithAmendments(true, false, false, false);
			}
			using (AccountingConfigurationRegistry.Instance.DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				AssertReversingOriginalTransactionWithAmendments(true, false, true, true);
				AssertReversingOriginalTransactionWithAmendments(true, false, true, false);
			}
		}

		void AssertReversingOriginalTransactionWithAmendments(bool amendmentsAvailable, bool amendmentsCancelled, bool isTransactionAllowedToReverse, bool isTestForAR)
		{
			var originalInvoice = isTestForAR ? TestObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(3), TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS)
									: (InvoicingBase)TestObjectCreator.CreateAPInvoice<APInvoice>(TestObjectCreator.GetRandomString(3), TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.AALSHI);
			if (amendmentsAvailable)
			{
				var amendingCreditNote = isTestForAR ? TestObjectCreator.CreateARCreditNote(TestObjectCreator.GetRandomString(3), TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1m)
									: (InvoicingBase)TestObjectCreator.CreateAPCreditNote(TestObjectCreator.GetRandomString(3), TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "");
				amendingCreditNote.AH_TransactionBelongsToGroup = originalInvoice.PK;
				if (amendmentsCancelled)
				{
					var reversing = new InvoicingBaseReversing(amendingCreditNote);
					reversing.Reverse();
					Assert(amendingCreditNote.IsReversed);
					if (amendingCreditNote is APCreditNote)
					{
						amendingCreditNote.ReverseInvoice.AH_TransactionNum = TestObjectCreator.GetRandomString(3);
					}
					Factory.Save();
					AssertEquals("Amending transaction is cancelled", 0, originalInvoice.GetRelatedAmendingTransactions().Count);

					reversing = new InvoicingBaseReversing(originalInvoice);
					AssertEquals("Should be able to reverse transaction", isTransactionAllowedToReverse, reversing.CanReverseTransaction);
				}
				else
				{
					Factory.Save();
					var amendingTransactions = originalInvoice.GetRelatedAmendingTransactions();
					AssertEquals("Amending transaction available", 1, amendingTransactions.Count);
					Assert(amendingTransactions.Contains(amendingCreditNote));

					var reversing = new InvoicingBaseReversing(originalInvoice);
					AssertEquals("Should not be able to reverse transaction", isTransactionAllowedToReverse, reversing.CanReverseTransaction);
					if (!isTransactionAllowedToReverse)
					{
						AssertMultilineASCIIEquals("Error message", $@"You are attempting to cancel an amended transaction.
Please cancel the following amendment documents before proceeding:
{amendingCreditNote.AH_Ledger}CRD{amendingCreditNote.AH_TransactionNum}", reversing.GenerateCantReverseErrorMessage_ForTestOnly());
					}
				}
			}
			else
			{
				Factory.Save();
				AssertEquals("No amending transactions available", 0, originalInvoice.GetRelatedAmendingTransactions().Count);
				var reversing = new InvoicingBaseReversing(originalInvoice);
				AssertEquals("Should be able to reverse transaction", isTransactionAllowedToReverse, reversing.CanReverseTransaction);
			}
		}

		public void TestReversingOriginalTransaction_EInvoicingTransactionValidation()
		{
			var eInvoicingValidationMock = new Mock<IEInvoicingTransactionValidation>();
			var extensionFactory = new Mock<ICountryComplianceEInvoicingExtensionFactory>();
			ObjectFactory.Substitute(extensionFactory.Object);
			extensionFactory.Setup(x => x.GetIEInvoicingTransactionValidation(GlbCompany.CurrentCompany.Country.Code)).Returns(() => eInvoicingValidationMock.Object);

			var originalInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(3), TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateEInvoicingTransactionPivot(originalInvoice, status: EInvoicingPivotState.Succeed);
			Factory.Save();

			var reversing = new InvoicingBaseReversing(originalInvoice);
			AssertEquals("Precondition", true, reversing.CanReverseTransaction);

			var expectedError = "EInvoicing validation error message.";
			eInvoicingValidationMock.Setup(x => x.GetCantReverseErrorMessage(originalInvoice)).Returns(() => expectedError);

			AssertEquals("Should not be able to reverse transaction", false, reversing.CanReverseTransaction);
			AssertEquals("Error message", expectedError, reversing.GenerateCantReverseErrorMessage_ForTestOnly());

			eInvoicingValidationMock.Setup(x => x.GetCantReverseErrorMessage(originalInvoice)).Returns(() => ZString.Empty);

			AssertEquals("Should be able to reverse transaction", true, reversing.CanReverseTransaction);
		}

		#endregion

		public void TestIsAPJobConsolInvoice()
		{
			APInvoice jobConsolAPInvoice = Factory.New<APInvoice>();
			ARInvoice jobConsolARInvoice = Factory.New<ARInvoice>();
			JobConsolCost consolCost = Factory.New<JobConsolCost>();
			consolCost.E6_AH_APInvoice = jobConsolAPInvoice.PK;
			consolCost.E6_AH_ARInvoice = jobConsolARInvoice.PK;
			consolCost.E6_GC = GlbCompany.CurrentCompany.PK;
			jobConsolARInvoice.AH_TransactionNum = "00001001";
			jobConsolARInvoice.AH_GC = GlbCompany.CurrentCompany.PK;
			jobConsolAPInvoice.AH_GC = GlbCompany.CurrentCompany.PK;

			InvoicingBaseReversing reversing = new InvoicingBaseReversing(jobConsolAPInvoice);
			Assert("Should not be able to reverse transaction", !reversing.CanReverseTransaction);
			string expectedErrorMessage = "This transaction cannot be reversed because it contains Profit Share and/or Master Freight charges." +
				System.Environment.NewLine + "You will need to reverse corresponding agent invoice INV 00001001. " +
				$"{BrandingFactory.Instance.ProductName} will automatically reverse this AP invoice when the corresponding agent invoice is reversed.";
			AssertEquals("Can't reverse reason", expectedErrorMessage, reversing.CantReverseErrorMessage);

			consolCost.E6_AH_APInvoice = ZGuid.Empty;

			Assert("Should be able to reverse now", reversing.CanTransactionBeReversed_ForTestOnly());
		}

		public void TestReverseARJobConsolInvoice()
		{
			ARInvoice aRInvoice = Factory.NewWithValidTestData<ARInvoice>();
			APInvoice aPInvoice = Factory.NewWithValidTestData<APInvoice>();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			ApportionmentListing appListing = new ApportionmentListing(Factory, consol);

			try
			{
				JobConsolCost cost = appListing.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				cost.E6_OSCostAmount = 50m;
				cost.E6_ApportionmentMethod = "SHP";
				cost.E6_AH_APInvoice = aPInvoice.PK;
				cost.E6_AH_ARInvoice = aRInvoice.PK;
				APInvoiceLine line1 = (APInvoiceLine)aPInvoice.Lines.AddNew();
				line1.AL_AG = TestObjectCreator.GLHeader1.PK;
				APInvoiceLine line2 = (APInvoiceLine)aPInvoice.Lines.AddNew();
				line2.AL_AG = TestObjectCreator.GLHeader1.PK;
				cost.ApportionmentCharges[0].JR_AL_APLine = line1.PK;
				cost.ApportionmentCharges[1].JR_AL_APLine = line2.PK;

				new InvoicingBaseReversing(aRInvoice).Reverse();

				Factory.Save();

				Assert("AP Invoice should be cancelled", aPInvoice.AH_IsCancelled);
				Assert("Should have unposted JobConsolInvoice", !cost.IsPosted);
			}
			finally
			{
				appListing.ReleaseMutexes();
			}
		}

		public void TestCanUATransactionBeReversed()
		{
			InvoicingBase uATransaction = Factory.New<UAInvoice>();
			AssertEquals("UAInvoice can't be reversed.", true, new InvoicingBaseReversing(uATransaction).CanTransactionBeReversed_ForTestOnly());

			uATransaction = Factory.New<UACreditNote>();
			AssertEquals("UAInvoice can't be reversed.", true, new InvoicingBaseReversing(uATransaction).CanTransactionBeReversed_ForTestOnly());
		}

		public void TestCantReversePayablesAndReceivablesRelatedToUnclosedClaim()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			APCreditNote creditNote = Factory.NewWithValidTestData<APCreditNote>();
			invoice.AH_TransactionBelongsToGroup = creditNote.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			Reversing = ReversingFactory.NewReversing(creditNote);
			ARAccQueryClaim claim = Factory.NewWithValidTestData<ARAccQueryClaim>();
			claim.AY_AH = invoice.PK;

			List<string> closedStatuses = new List<string> { QueryClaimStatusCodeList.Codes.QCStatus3AcceptedAndCreditNoteIssuedAndClosed, QueryClaimStatusCodeList.Codes.QCStatus5RejectedClosed };
			List<string> allStatuses = new List<string>();

			foreach (FieldInfo field in typeof(QueryClaimStatusCodeList.Codes).GetFields(BindingFlags.Static | BindingFlags.Public))
			{
				allStatuses.Add((string)field.GetValue(null));
			}

			foreach (ZString status in allStatuses)
			{
				claim.AY_QueryClaimStatus = status;
				Factory.Save();
				AssertEquals(String.Format("CanReverseTransaction when transaction is linked to a claim with status = {0}", status), !closedStatuses.Contains(status), Reversing.CanReverseTransaction);
			}
		}

		public void TestCantReverseTransactionAttachedToClaim()
		{
			UACreditNote creditNote = Factory.NewWithValidTestData<UACreditNote>();
			creditNote.AH_TransactionCategory = Constants.TransactionCategory.Codes.ClaimRelated;
			Reversing = ReversingFactory.NewReversing(creditNote);
			AssertEquals("CanReverseTransaction when transaction is related to claim", false, InvoicingReversing.CanReverseTransaction);
			AssertEquals("GenerateCantReverseErrorMessage when transaction is related to claim", InvoicingReversing.CantReverseTransactionAttachedToClaimErrorMessage_ForTestOnly, InvoicingReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public void TestCantReverseTransactionIncludedInInvoiceBatch()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_AH_InvoiceStatement = ZGuid.NewZGuid();
			Reversing = ReversingFactory.NewReversing(invoice);
			AssertEquals("CanReverseTransaction when transaction is in Invoice Batch", false, InvoicingReversing.CanReverseTransaction);
			AssertEquals("GenerateCantReverseErrorMessage when transaction is in Invoice Batch", InvoicingReversing.TransactionIsInInvoiceBatchErrorMessage_ForTestOnly, InvoicingReversing.GenerateCantReverseErrorMessage_ForTestOnly());

			invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_AH_InvoiceStatement = ZGuid.NewZGuid();
			Reversing = ReversingFactory.NewReversing(invoice);
			AssertEquals("CanReverseTransaction when transaction is in Invoice Batch", false, InvoicingReversing.CanReverseTransaction);
			AssertEquals("GenerateCantReverseErrorMessage when transaction is in Invoice Batch", InvoicingReversing.TransactionIsInInvoiceBatchErrorMessage_ForTestOnly, InvoicingReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public void TestCantReverseTransactionFromOtherCompany()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			Reversing = ReversingFactory.NewReversing(invoice);
			AssertEquals("CanReverseTransaction when transaction is in different company", false, InvoicingReversing.CanReverseTransaction);
			AssertEquals("GenerateCantReverseErrorMessage when transaction is in different company", InvoicingReversing.TransactionFromOtherCompanyErrorMessage_ForTestOnly, InvoicingReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public void TestDoReverseTaxTransaction()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new InvoicingBaseReversing(null));

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			ARInvoice originalInvoice = Factory.New<ARInvoice>();
			var invoicingBaseReversingObject = new InvoicingBaseReversing(originalInvoice);

			AssertNoExceptionThrown(() => Reversing = new InvoicingBaseReversingForTest(originalInvoice));
			var reverseTransactionPostDate = ZDate.Today.AddDays(-5);
			((InvoicingBaseReversingForTest)Reversing).ReverseTransactionPostDate = reverseTransactionPostDate;
			Reversing.Reverse();

			taxProcessorMock.Verify(x => x.ProcessOnParentReversing(TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent((InvoicingBase)Reversing.OriginalTransaction),
																	TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent((InvoicingBase)Reversing.ReverseTransaction),
																	reverseTransactionPostDate));
		}

		[TestDate(2012, 03, 08)]
		public void TestReversingDefaultsInvoiceDate_JobRelated_ARInvoice()
		{
			TestReversingDefaultsInvoiceDate_JobRelated(typeof(ARInvoice));
		}

		[TestDate(2012, 03, 08)]
		public void TestReversingDefaultsInvoiceDate_JobRelated_ARCreditNote()
		{
			TestReversingDefaultsInvoiceDate_JobRelated(typeof(ARCreditNote));
		}

		void TestReversingDefaultsInvoiceDate_JobRelated(Type invoiceType)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			TestObjectCreator testObjectCreator = new TestObjectCreator(factory);

			Job testJob = testObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			ForwardingShipment forwardingShipment = testJob.PlugInData as ForwardingShipment;
			AssertNotNull("Forwarding Shipment should not be null", forwardingShipment);

			testJob = factory.Load<Job>(testJob.PK);
			ZDecimal amount = invoiceType == typeof(ARInvoice) ? 1000m : -1000m;
			Charge charge = testObjectCreator.CreateCharge(testJob, testObjectCreator.CC1, "Desc",
				testObjectCreator.AUD, amount, testObjectCreator.Creditor1,
				testObjectCreator.AUD, amount, testObjectCreator.LocalClient);
			charge.JR_APInvoiceNum = "001";
			charge.JR_APInvoiceDate = ZDateTime.Today;
			factory.Save();

			InvoicingPostManager postManager = new InvoicingPostManager(testJob);
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.All);
			factory.Save();

			InvoicingBase[] invoices = transactions.GetAllARInvoicesAndCreditNotes();
			AssertEquals("invoices.Length", 1, invoices.Length);
			InvoicingBase transaction = invoices[0];

			AssertEquals("AH_InvoiceDate", new ZDateTime(2012, 03, 08), transaction.AH_InvoiceDate.Date);
			AssertEquals("AH_PostDate", new ZDateTime(2012, 03, 08), transaction.AH_PostDate.Date);

			BackDateInvoicesConfiguration configuration = new BackDateInvoicesConfiguration();
			configuration.InvoiceDateConfigurationCollection.RemoveAll();
			testObjectCreator.AddInvoiceDateConfiguration(configuration, "SHP", "ALL", "ALL", "ALL",
				InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceAddDate,
				true, true);
			configuration.OverridePostDate = true;
			configuration.DefaultPostDateFromInvoiceDate = true;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			ZFormModaliser.LastFormShownDialogForTest = null;

			ReversingFactory reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(transaction);
			reversing.Reverse();

			AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);

			InvoicingBase[] reversedInvoices;
			if (transaction is ARInvoice)
			{
				reversedInvoices = reversing.ReverseTransaction.Factory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			}
			else
			{
				reversedInvoices = reversing.ReverseTransaction.Factory.Load<ARInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			}
			AssertNotNull("reversedInvoices", reversedInvoices);
			AssertEquals("reversedInvoices.Length", 1, reversedInvoices.Length);

			InvoicingBase reversingResult = reversedInvoices[0];
			AssertNotNull("reversingResult", reversingResult);

			AssertEquals("AH_InvoiceDate", new ZDateTime(2012, 02, 29), reversingResult.AH_InvoiceDate);
			AssertEquals("AH_PostDate", new ZDateTime(2012, 03, 08), reversingResult.AH_PostDate);
		}

		[TestDate(2012, 03, 03)]
		public void TestReversingDefaultsInvoiceDate_NonJobRelated_ARInvoice()
		{
			testReversingDefaultsInvoiceDate_NonJobRelated(typeof(ARInvoice));
		}

		[TestDate(2012, 03, 03)]
		public void TestReversingDefaultsInvoiceDate_NonJobRelated_ARCreditNote()
		{
			testReversingDefaultsInvoiceDate_NonJobRelated(typeof(ARCreditNote));
		}

		void testReversingDefaultsInvoiceDate_NonJobRelated(Type invoiceType)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(factory);
			creator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);
			ZDateTime date = new ZDateTime(2012, 02, 15);

			InvoicingBase invoice = (InvoicingBase)factory.New(invoiceType);

			invoice.AH_InvoiceDate = date;
			invoice.AH_PostDate = date.AddDays(-1);
			creator.CreateInvoiceLine(invoice, 100m, creator.AUD, 1.0m);
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			factory.Save();

			AccountingConfigurationRegistry.Instance.AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionInvoiceDateOrCurrentDate);
			AccountingConfigurationRegistry.Instance.AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionInvoiceDateOrCurrentDate);

			ZFormModaliser.LastFormShownDialogForTest = null;

			ReversingFactory reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(invoice);
			reversing.Reverse();

			AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);

			InvoicingBase[] reversedInvoices;
			if (invoice is ARInvoice)
			{
				reversedInvoices = reversing.ReverseTransaction.Factory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			}
			else
			{
				reversedInvoices = reversing.ReverseTransaction.Factory.Load<ARInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			}
			AssertNotNull("reversedInvoices", reversedInvoices);
			AssertEquals("reversedInvoices.Length", 1, reversedInvoices.Length);

			InvoicingBase reversingResult = reversedInvoices[0];
			AssertNotNull("reversingResult", reversingResult);

			AssertEquals(date, reversingResult.AH_InvoiceDate);
			AssertEquals(date, reversingResult.AH_PostDate);
		}

		[TestDate(2012, 03, 15)]
		public void TestReversingDefaultsPostDate_APInvoice()
		{
			testReversingDefaultsPostDate_AP(typeof(APInvoice));
		}

		[TestDate(2012, 03, 15)]
		public void TestReversingDefaultsPostDate_APCreditNote()
		{
			testReversingDefaultsPostDate_AP(typeof(APCreditNote));
		}

		void testReversingDefaultsPostDate_AP(Type invoiceType)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			TestObjectCreator testObjectCreator = new TestObjectCreator(factory);
			testObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);

			BackDateAPInvoicesConfiguration backDateAPInvoicesConfiguration = new BackDateAPInvoicesConfiguration();
			backDateAPInvoicesConfiguration.PostDateConfigurationCollection.RemoveAll();
			testObjectCreator.AddPostDateConfiguration(backDateAPInvoicesConfiguration, "FCN", "IMP", "AIR", "", "ARV", "EPP", "SGN", "SGN", "ADD", PostDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionPostDateOrCurrentDate);
			AccountingConfigurationRegistry.Instance.BackDateAPInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backDateAPInvoicesConfiguration);
			AccountingConfigurationRegistry.Instance.AllowAPInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var consol = testObjectCreator.CreateConsol("NZAKL", "AUSYD", "C00001000");
			consol.JK_TransportMode = "AIR";
			ZDateTime arrivalDate = new ZDateTime(2012, 03, 02);
			consol.Transports.ArrivalTransport.JW_ATA = arrivalDate;
			var shipment = testObjectCreator.CreateShipment("S00001000", "NZAKL", "AUSYD", consol);
			var job = testObjectCreator.CreateJob(shipment, false);

			factory.Save();

			var invoice = testObjectCreator.CreateInvoice(invoiceType, testObjectCreator.AUD, 1.0m, testObjectCreator.AALSHI);
			ZDateTime postDate = new ZDateTime(2012, 03, 03);
			ZDateTime invoiceDate = new ZDateTime(2012, 03, 04);
			invoice.AH_PostDate = postDate;
			invoice.AH_InvoiceDate = invoiceDate;

			var consolCost = invoice.ConsolCosting.ConsolCosts.AddNew();
			consolCost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
			consolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
			ZDecimal multiplier = invoiceType == typeof(APCreditNote) ? -1m : 1m;
			consolCost.E6_OSCostAmount = 1000m * multiplier;

			invoice.AH_JH = ZGuid.Empty;
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			invoice.SubmittedFromInvoicingForm = true;
			invoice.ImportAllApportionmentsFromCosting();

			AssertEquals("invoice.Lines.Count", 1, invoice.Lines.Count);

			factory.Save();

			ZFormModaliser.LastFormShownDialogForTest = null;

			ReversingFactory reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(invoice);
			reversing.Reverse();

			AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);

			InvoicingBase[] reversedInvoices;
			if (invoice is APInvoice)
			{
				reversedInvoices = reversing.ReverseTransaction.Factory.Load<APCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			}
			else
			{
				reversedInvoices = reversing.ReverseTransaction.Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			}
			AssertNotNull("reversedInvoices", reversedInvoices);
			AssertEquals("reversedInvoices.Length", 1, reversedInvoices.Length);

			InvoicingBase reversingResult = reversedInvoices[0];
			AssertNotNull("reversingResult", reversingResult);

			AssertEquals("Post Date", postDate, reversingResult.AH_PostDate);
			AssertEquals("Invoice Date", invoiceDate, reversingResult.AH_InvoiceDate);
		}

		#region Multiple Installments Invoice Term Unmatching and Reversing

		protected ARInvoice SetupInvoiceWithMultipleInstallmentsTerm()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);

			TestObjectCreator.AALSHI.OH_IsActive = true;
			TestObjectCreator.AALSHI.CompanyData.OB_IsCreditor = true;
			TestObjectCreator.AALSHI.CompanyData.OB_IsDebtor = true;
			Factory.Save();

			var arInvoice = Factory.New<ARInvoice>();
			arInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			arInvoice.AH_GE = GlbDepartment.CurrentDepartment.PK;

			var arTerms = TestObjectCreator.AALSHI.CompanyData.ARTerms;
			var arTermMLI = arTerms.AddNew();
			arTermMLI.PY_JobType = "ALL";
			arTermMLI.PY_GB_Branch = arInvoice.AH_GB;
			arTermMLI.PY_GE_Department = arInvoice.AH_GE;
			arTermMLI.PY_Direction = "ALL";
			arTermMLI.PY_TransportMode = "ALL";
			arTermMLI.PY_InvoiceClass = InvoiceTypesList.Codes.FinalInvoice;
			arTermMLI.PY_InvoiceTerm = InvoiceTerms.FromInvoiceDate;

			var arTermMLInstalments = arTermMLI.ARTermsInstallments;
			CreateInstallment(arTermMLInstalments, 1, 33.33, 30, OrgConstants.CreditAgreedPaymentMethods.Code.BankTransfer);
			CreateInstallment(arTermMLInstalments, 2, 33.33, 60, OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck);
			CreateInstallment(arTermMLInstalments, 3, 33.34, 90, OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck);

			var today = ZDateTime.Today;
			arInvoice.AH_InvoiceDate = new ZDateTime(today.Year, today.Month, 1);
			arInvoice.AH_PostDate = today;
			arInvoice.AH_DueDate = today.AddMonths(1);

			arInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			arInvoice.AH_TransactionNum = TestObjectCreator.GetRandomString(6);
			var currency = TestObjectCreator.EUR;
			arInvoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			arInvoice.AH_ExchangeRate = 1;
			arInvoice.AH_InvoiceTerm = InvoiceTerms.FromInvoiceDate;
			arInvoice.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest;

			arInvoice.Lines.DeleteAll();
			var line = arInvoice.Lines.AddNew();
			line.AL_GB = arInvoice.AH_GB;
			line.AL_GE = arInvoice.AH_GE;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_OSExTaxAmount = 111.10;
			line.AL_OSTaxAmount = 11.11;

			arInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			Factory.Save();

			AssertEquals(InvoiceTerms.MultipleInstallments, arInvoice.AH_InvoiceTerm);
			return arInvoice;

			void CreateInstallment(OrgARTermsInstallmentCollection arTermsMli, ZByte sequenceNumber, ZDecimal splitPercentage, ZByte daysFromInvoiceDate, ZString agreedPaymentMethod)
			{
				var installment = arTermsMli.AddNew();
				installment.ML_SequenceNumber = sequenceNumber;
				installment.ML_SplitPercentage = splitPercentage;
				installment.ML_DaysFromInvoiceDate = daysFromInvoiceDate;
				installment.ML_AgreedPaymentMethod = agreedPaymentMethod;
			}
		}

		public void TestReversingWithMultipleInstallmentInvoiceTerm()
		{
			var currCompany = GlbCompany.CurrentCompany;
			var arInvoice = SetupInvoiceWithMultipleInstallmentsTerm();
			var transactionNum = arInvoice.AH_TransactionNum;

			AssertEquals("Transaction should not be reversed", false, arInvoice.IsReversed);
			AssertEquals("Transaction should not be cancelled", false, arInvoice.IsCancelled);

			var clearingJournal = arInvoice.GetMultipleInstallmentsJournals(TransactionCategory.Codes.ClearingJournal)[0];
			AssertEquals("Clearing Journal should be fully payed today", ZDateTime.Today, clearingJournal.AH_FullyPaidDate);
			AssertEquals("Clearing Journal have 0 outstanding amount", 0m, clearingJournal.AH_OutstandingAmount);

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(arInvoice);
			reversing.Reverse();

			AssertEquals("Transaction should be reversed", true, arInvoice.IsReversed);
			AssertEquals("Transaction should be cancelled", true, arInvoice.IsCancelled);
			AssertEquals("Transaction should be fully payed today", ZDateTime.Today, arInvoice.AH_FullyPaidDate);
			AssertEquals("Transaction should have 0 outstanding amount", 0m, arInvoice.AH_OutstandingAmount);

			var reversedTransaction = reversing.ReverseTransaction;
			var creditNote = Factory.Load<ARCreditNote>(reversedTransaction.PK);

			AssertNotNull("Credit note should exist", creditNote);
			AssertEquals("Credit note should be cancelled", true, creditNote.IsCancelled);
			AssertEquals("Credit note should be fully payed today", ZDateTime.Today, creditNote.AH_FullyPaidDate);
			AssertEquals("Credit note should have 0 outstanding amount", 0m, creditNote.AH_OutstandingAmount);
			AssertEquals("Credit note description", $"REVERSAL RELATED TO {transactionNum}", creditNote.AH_Desc);

			var multipleInstallments = arInvoice.GetMultipleInstallmentsJournals();
			foreach (var installment in multipleInstallments)
			{
				AssertEquals("Installment should be reversed", true, installment.IsReversed);
				AssertEquals("Installment should be cancelled", true, installment.IsCancelled);
				AssertEquals("Installment should be fully payed today", ZDateTime.Today.Date, installment.AH_FullyPaidDate.Date);
				AssertEquals("Installment should have 0 outstanding amount", 0m, installment.AH_OutstandingAmount);
				var reversedInstallment = installment.ReverseTransaction as ARJournal;
				var expectedDesc = $"REVERSAL RELATED TO {installment.AH_TransactionNum} - {installment.AH_Desc}";
				AssertEquals("Reversed installment should have the proper description", expectedDesc, reversedInstallment.AH_Desc);
			}
		}

		public void TestCantReverseMultipleInstallmentInvoiceTerm_UnmatchedClearingJournal()
		{
			var currCompany = GlbCompany.CurrentCompany;
			var arInvoice = SetupInvoiceWithMultipleInstallmentsTerm();
			var transactionNum = arInvoice.AH_TransactionNum;

			var unmatchingRow = new UnmatchingRow(Factory) { MatchGroupNum = arInvoice.LatestMatchLink.AP_MatchGroupNum };
			unmatchingRow.UnmatchAnyGroup();

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(arInvoice);
			AssertEquals("Should not be able to reverse transaction", false, reversing.CanReverseTransaction);
			AssertEquals("Error message", $"This transaction cannot be reversed because it is not matched with its linked CLJ type AR Journal.\r\n{CantReverseMLINotes}", reversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public void TestCantReverseMultipleInstallmentInvoiceTerm_ClearingJournalMatchedWithAnotherJournal()
		{
			var currCompany = GlbCompany.CurrentCompany;
			var arInvoice = SetupInvoiceWithMultipleInstallmentsTerm();
			var transactionNum = arInvoice.AH_TransactionNum;

			var unmatchingRow = new UnmatchingRow(Factory) { MatchGroupNum = arInvoice.LatestMatchLink.AP_MatchGroupNum };
			unmatchingRow.UnmatchAnyGroup();

			var anotherJournal = TestObjectCreator.CreateJournal<ARJournal>(122.21m, arInvoice.AH_PostDate, TestObjectCreator.AALSHI.PK);
			anotherJournal.DebitCreditSign = DebitCreditDataEntry.DR;

			var matching = new ARMatchingBase(Factory);
			matching.PrimaryOrganization = TestObjectCreator.AALSHI.PK;
			Dictionary<BusinessObject, ZDecimal> transactionsToMatch = new Dictionary<BusinessObject, ZDecimal>();
			transactionsToMatch.Add(arInvoice, arInvoice.AH_OSTotal);
			transactionsToMatch.Add(anotherJournal, anotherJournal.AH_OSTotal);
			matching.MoveFromUnmatchToMatch(transactionsToMatch);
			matching.MatchAndClearTransactions();

			Factory.Save();

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(arInvoice);
			AssertEquals("Should not be able to reverse transaction", false, reversing.CanReverseTransaction);
			AssertEquals("Error message", $"This transaction cannot be reversed because it is not matched with its linked CLJ type AR Journal.\r\n{CantReverseMLINotes}", reversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public void TestCantReverseMultipleInstallmentInvoiceTerm_AnyMatchedJournal()
		{
			var currCompany = GlbCompany.CurrentCompany;
			var arInvoice = SetupInvoiceWithMultipleInstallmentsTerm();
			var transactionNum = arInvoice.AH_TransactionNum;

			var anotherJournal = TestObjectCreator.CreateJournal<ARJournal>(40.73m, arInvoice.AH_PostDate, TestObjectCreator.AALSHI.PK);
			anotherJournal.DebitCreditSign = DebitCreditDataEntry.DR;

			var firstInstallment = arInvoice.GetMultipleInstallmentsJournals(Constants.TransactionCategory.Codes.InstalmentJournal)[0];

			var matching = new ARMatchingBase(Factory);
			matching.PrimaryOrganization = TestObjectCreator.AALSHI.PK;
			Dictionary<BusinessObject, ZDecimal> transactionsToMatch = new Dictionary<BusinessObject, ZDecimal>();
			transactionsToMatch.Add(firstInstallment, firstInstallment.AH_OSTotal);
			transactionsToMatch.Add(anotherJournal, anotherJournal.AH_OSTotal);
			matching.MoveFromUnmatchToMatch(transactionsToMatch);
			matching.MatchAndClearTransactions();

			Factory.Save();

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(arInvoice);
			AssertEquals("Should not be able to reverse transaction", false, reversing.CanReverseTransaction);
			AssertEquals("Error message", $"This transaction cannot be reversed because at least one of its linked INJ type AR Journals has been matched with other transactions.\r\n{CantReverseMLINotes}", reversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public void TestCantReverseMultipleInstallmentInvoiceTerm_AnyReversedJournal()
		{
			var currCompany = GlbCompany.CurrentCompany;
			var arInvoice = SetupInvoiceWithMultipleInstallmentsTerm();
			var transactionNum = arInvoice.AH_TransactionNum;

			var firstInstallment = arInvoice.GetMultipleInstallmentsJournals(Constants.TransactionCategory.Codes.InstalmentJournal)[0];

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(firstInstallment);
			reversing.Reverse();

			reversingFactory = new ReversingFactory();
			reversing = reversingFactory.NewReversing(arInvoice);
			AssertEquals("Should not be able to reverse transaction", false, reversing.CanReverseTransaction);
			AssertEquals("Error message", $"This transaction cannot be reversed because it has linked INJ type AR Journal/s which have already been reversed.\r\n{CantReverseMLINotes}", reversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public void TestCantReverseMultipleInstallmentInvoiceTerm_AnyJournalInCollectionOrder()
		{
			var currCompany = GlbCompany.CurrentCompany;
			var arInvoice = SetupInvoiceWithMultipleInstallmentsTerm();
			var transactionNum = arInvoice.AH_TransactionNum;

			var firstInstallment = arInvoice.GetMultipleInstallmentsJournals(Constants.TransactionCategory.Codes.InstalmentJournal)[0];

			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var batch = TestObjectCreator.CreateCollectionBatch(bankAccount, GlbCompany.CurrentCompany, "00001001", 40.73m, false);
			var order = TestObjectCreator.CreateCollectionOrder(batch, ZDateTime.Today.Date, TestObjectCreator.AALSHI, "0000001", 40.73m, false);
			var line = TestObjectCreator.CreateCollectionOrderLine(order, firstInstallment, false);
			Factory.Save();

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(arInvoice);
			AssertEquals("Should not be able to reverse transaction", false, reversing.CanReverseTransaction);
			AssertEquals("Error message", $"This transaction cannot be reversed because it has linked INJ type AR Journal/s which are included in an active collection batch order.\r\n{CantReverseMLINotes}", reversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		const string CantReverseMLINotes = @"Note: Invoices, Credit Notes and Adjustment Notes with Invoice Term INV for Multiple Installments can only be reversed if:
They are matched with their linked CLJ AR Journal;
They or any associated INJ journals are not reversed;
Any associated INJ journals are not matched;
They or any associated INJ journals are not included in an active Collection Order.";

		#endregion

		public void TestAlreadyGeneratedComplianceDocument_Reverse()
		{
			SetupInvoiceForAlreadyGeneratedComplianceDocument(false, true);
			AssertEnableComplianceDocumentModuleForGeneratedComplianceDocument(registryValue: true, expectedHasGeneratedComplianceDocument: true);
			AssertEnableComplianceDocumentModuleForGeneratedComplianceDocument(registryValue: false, expectedHasGeneratedComplianceDocument: true);
		}

		public void TestAlreadyGeneratedComplianceDocument_WritingOff()
		{
			SetupInvoiceForAlreadyGeneratedComplianceDocument(true, true);
			AssertEnableComplianceDocumentModuleForGeneratedComplianceDocument(registryValue: true, expectedHasGeneratedComplianceDocument: false);
			AssertEnableComplianceDocumentModuleForGeneratedComplianceDocument(registryValue: false, expectedHasGeneratedComplianceDocument: false);
		}

		public void TestAlreadyGeneratedComplianceDocument_NotBadDebtWritingOff()
		{
			SetupInvoiceForAlreadyGeneratedComplianceDocument(false, false);
			AssertEnableComplianceDocumentModuleForGeneratedComplianceDocument(registryValue: true, expectedHasGeneratedComplianceDocument: true);
			AssertEnableComplianceDocumentModuleForGeneratedComplianceDocument(registryValue: false, expectedHasGeneratedComplianceDocument: true);
		}

		void AssertEnableComplianceDocumentModuleForGeneratedComplianceDocument(bool registryValue, bool expectedHasGeneratedComplianceDocument)
		{
			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			AssertEquals(registryValue, AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals(expectedHasGeneratedComplianceDocument, InvoicingReversing.HasGeneratedComplianceDocument_ForTestOnly);
			AssertEquals("You cannot reverse an INV or CRD that is linked to a compliance document record. You need to void all compliance document records related to the transaction before proceeding to reverse.", InvoicingReversing.HasGeneratedComplianceDocumentErrorMessage_ForTestOnly);
		}

		void SetupInvoiceForAlreadyGeneratedComplianceDocument(bool isWritingOff, bool isBadDebtWritingOff)
		{
			var vat3 = TestObjectCreator.CreateTaxRate("VAT3", "VAT3", 3);
			vat3.AT_PostingGroupId = 0;

			var ac1 = TestObjectCreator.CreateChargeCode("AC1");
			ac1.AC_AT_GSTRate = vat3.PK;

			InvoicingBase invoicingBase;
			if (isBadDebtWritingOff)
			{
				invoicingBase = Factory.NewWithValidTestData<ARInvoice>();
			}
			else
			{
				invoicingBase = Factory.NewWithValidTestData<APInvoice>();
				AssertEquals(false, invoicingBase is IBadDebtWritingOff);
			}
			var invoiceLine = invoicingBase.Lines.AddNew() as InvoicingLineBase;
			invoiceLine.AL_JH = TestObjectCreator.Job1.PK;
			invoiceLine.AL_AC = ac1.PK;
			invoiceLine.AL_AT = vat3.PK;

			TestObjectCreator.CreateJobCharge(invoiceLine, TestObjectCreator.Job1, ac1);
			Factory.Save();

			if (isWritingOff && isBadDebtWritingOff)
			{
				(invoicingBase as IBadDebtWritingOff).IsWritingOff = true;
			}

			Reversing = ReversingFactory.NewReversing(invoicingBase);
			AssertEquals("AlreadyGeneratedComplianceDocument is false when transaction doesn't have compliance document.", false, InvoicingReversing.HasGeneratedComplianceDocument_ForTestOnly);

			TestObjectCreator.CreateComplianceDocumentHeaderWithLine(invoicingBase.AH_Ledger, "desc", "0001", "NTC", "lineDesc", invoicingBase.Lines[0]);
			Factory.Save();
		}

		InvoicingBaseReversing InvoicingReversing
		{
			get { return (InvoicingBaseReversing)Reversing; }
		}
		protected override Type GetTestingClassType()
		{
			return typeof(InvoicingBaseReversing);
		}

		protected override void SetupReversingInstance()
		{
			TestIReversingInstance = Factory.NewWithValidTestData<InvoicingBaseForReversingTest>();
		}

		protected override void SetupReversingIReversingInstance()
		{
			TestReversingIReversingInstance = Factory.NewWithValidTestData<InvoicingBaseForReversingTest>();
		}

		class InvoicingBaseForReversingTest : APAdjustmentNote, IPayablesAndReceivablesForTests
		{
			public InvoicingBaseForReversingTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IReversingForTests Members

			public void SetIsClearedInCashbook(bool value)
			{
				AH_DateClearedInCashbook = value ? ZDateTime.Now : ZDateTime.Empty;
			}

			public void SetIsReversed(bool value)
			{
				AH_IsCancelled = value;
			}

			public void SetReverseTransactionToBeGenerated(IReversing reverseTransaction)
			{
				fReverseTransaction = (TransactionHeader)reverseTransaction;
			}

			#endregion

			#region ITransactionForTests Members

			public void SetFactory(BusinessObjectFactory factory)
			{
			}

			public ZDateTime FullyPaidDate
			{
				get
				{
					return AH_FullyPaidDate;
				}
				set
				{
					AH_FullyPaidDate = value;
				}
			}

			#endregion

			#region IPayablesAndReceivablesForTests Members

			public void SetIsMatched(bool value)
			{
				if (((IMatching)this).IsMatched != value)
				{
					if (value)
					{
						AH_LocalOutstandingAmount = AH_LocalExTaxAmount + AH_LocalTaxAmount + 1M;
					}
					else
					{
						AH_LocalOutstandingAmount = AH_LocalExTaxAmount + AH_LocalTaxAmount;
					}
				}
			}

			public void SetMatchLinksToBeGenerated(TransactionMatchLinkGroup matchLinksToSet)
			{
				fCurrentMatchGroup = matchLinksToSet;
			}

			#endregion
		}

		class InvoicingBaseReversingForTest : InvoicingBaseReversing
		{
			public InvoicingBaseReversingForTest(IPayablesAndReceivables payablesAndReceivablesTransaction)
			: base(payablesAndReceivablesTransaction)
			{
			}

			public ZDate ReverseTransactionPostDate { get; set; }

			protected override void DoReverseTaxTransaction()
			{
				((InvoicingBase)ReverseTransaction).AH_PostDate = ReverseTransactionPostDate;
				base.DoReverseTaxTransaction();
			}
		}
	}
}
