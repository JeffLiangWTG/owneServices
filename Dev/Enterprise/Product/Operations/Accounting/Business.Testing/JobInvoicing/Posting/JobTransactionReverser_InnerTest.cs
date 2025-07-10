using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class JobTransactionReverser_InnerTest : TestCaseWithFactory
	{
		#region TestReverseInvoiceAndCreditNote

		public void TestReverseInvoiceAndCreditNote()
		{
			SetupJobTransactions();
			ZQuery collectionFilter = new ZQuery(AccTransactionHeaderSchema.PK, Job1Invoice.PK);
			collectionFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.PK, SQLComparisonOperator.Equal, Job1CreditNote.PK);
			collectionFilter.OrderBy = InvoicingBase.Schema.AH_TransactionType + " DESC";//get invoice and then credit note for testing needs
			InvoicingBaseCollection collection = new InvoicingBaseCollection(Factory, collectionFilter);
			collection.Load();

			// Check that post dates are what they were originally set to
			AssertEquals("Precondition: Job 1 Invoice should have PostDate of 2 days before Today's Date", PreviousDate, Job1Invoice.AH_PostDate);
			foreach (TransactionLine line in Job1Invoice.Lines)
			{
				AssertEquals("Precondition: Line should have PostDate of 2 days before Today'sDate", PreviousDate, line.AL_PostDate);
			}

			AssertEquals("Precondition: Job 1 CreditNote should have PostDate of 2 days before Today'sDate", PreviousDate, Job1CreditNote.AH_PostDate);
			foreach (TransactionLine line in Job1CreditNote.Lines)
			{
				AssertEquals("Precondition: Line should have PostDate of 2 days before Today'sDate", PreviousDate, line.AL_PostDate);
			}

			Reverser = new JobTransactionReverser(collection.ToArray<InvoicingBase>());
			Reverser.ReverseAllInvoices("because I feel like reversing - very very very very long description of my reasons", "tst");
			foreach (InvoicingBase transaction in Reverser.TransactionsToReverse_ForTestOnly)
			{
				AssertEquals("ReversingCode", "tst", transaction.ReverseInvoice.ReversingCode);
				AssertEquals("ReversingReason", "because I feel like reversing - very very very very long description of my reasons", transaction.ReverseInvoice.ReversingReason);
			}

			ZString peekedMatchGroupNum = Env.NumberFountains.MatchNo.GetTodaysPeriodFountain().PeekPreliminaryFormatted(Factory);
			Factory.Save();

			Assert("Job 1 Invoice 1 should have been reversed", Job1Invoice.AH_IsCancelled);
			AssertEquals("Job 1 Invoice 1 should have fully paid date of today", ZDateTime.Today.Date, Job1Invoice.AH_FullyPaidDate.Date);
			Assert("Job 1 Invoice 1 should not have empty TransactionBelongsToGroup", Job1Invoice.AH_TransactionBelongsToGroup.IsEmpty);
			AssertEquals("Job 1 Invoice 1 should have 0 outstanding amount", 0m, Job1Invoice.AH_OutstandingAmount);

			Assert("Job 1 Credit Note should have been reversed", Job1CreditNote.AH_IsCancelled);
			AssertEquals("Job 1 Credit Note should have fully paid date of today", ZDateTime.Today.Date, Job1CreditNote.AH_FullyPaidDate.Date);
			Assert("Job 1 Credit Note should not have empty TransactionBelongsToGroup", Job1CreditNote.AH_TransactionBelongsToGroup.IsEmpty);
			AssertEquals("Job 1 Credit Note should have 0 outstanding amount", 0m, Job1CreditNote.AH_OutstandingAmount);

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			AssertEquals("Should have 2 reversed transactions", 2, Factory.GetDatabaseCount(typeof(InvoicingBase), filter));

			filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.Equal, Job1Invoice.PK);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			InvoicingBase[] invoiceReversals = Factory.Load<InvoicingBase>(filter);
			AssertEquals("Should be only one transaction present", 1, invoiceReversals.Length);
			ARCreditNote invoiceReversal = invoiceReversals[0] as ARCreditNote;
			AssertNotNull(invoiceReversal);

			//Header Assertions
			AssertEquals("Reversing Credit Note should have IsCancelled flag as true", ZBool.True, invoiceReversal.AH_IsCancelled);
			AssertEquals("Fully paid date of today", ZDateTime.Now.Date, invoiceReversal.AH_FullyPaidDate.Date);
			AssertEquals("Reversing credit note oustanding amount", 0m, invoiceReversal.AH_OutstandingAmount);
			AssertEquals("Reversing credit note invoice date", ZDateTime.Now.Date, invoiceReversal.AH_InvoiceDate.Date);
			AssertEquals("Reversing credit note due date", ZDateTime.Now.Date.AddDays(7), invoiceReversal.AH_DueDate.Date);
			AssertEquals("Reversing credit note post date", ZDateTime.Now.Date, invoiceReversal.AH_PostDate.Date);
			AssertEquals("Currency should be the same as original invoice", Job1Invoice.AH_RX_NKTransactionCurrency, invoiceReversal.AH_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate should be the same as original invoice", Job1Invoice.AH_ExchangeRate, invoiceReversal.AH_ExchangeRate);
			AssertEquals("AH_OSExTaxAmount should be the same", Job1Invoice.AH_OSExTaxAmount, invoiceReversal.AH_OSExTaxAmount);
			AssertEquals("OSGSTTaxAmount should be the same", Job1Invoice.AH_OSTaxAmount, invoiceReversal.AH_OSTaxAmount);
			AssertEquals("Description", "Reversal related to ".ToUpper() + Job1Invoice.AH_TransactionNum.ToUpper() + " because I feel like reversing - very very very very long description of my reasons".ToUpper(), invoiceReversal.AH_Desc.ToUpper());
			AssertEquals("Job on header should be the same as reversal", Job1Invoice.AH_JH, invoiceReversal.AH_JH);
			AssertEquals("Branch should be same as reversal", Job1Invoice.AH_GB, invoiceReversal.AH_GB);
			AssertEquals("Department should be same as reversal", Job1Invoice.AH_GE, invoiceReversal.AH_GE);
			AssertEquals("AH_TransactionCategory should be same as reversal", Job1Invoice.AH_TransactionCategory, invoiceReversal.AH_TransactionCategory);
			AssertEquals("Currency should be same as reversal", Job1Invoice.AH_RX_NKTransactionCurrency, invoiceReversal.AH_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate should be same as reversal", Job1Invoice.AH_ExchangeRate, invoiceReversal.AH_ExchangeRate);
			AssertEquals("Consol Invoice Ref should be same as reversal", "Job1/B", invoiceReversal.AH_ConsolidatedInvoiceRef);

			//Line Assertions
			int lineIndex = 0;
			foreach (InvoicingLineBase line in invoiceReversal.Lines)
			{
				AssertEquals(line.AL_LineType, Job1Invoice.Lines[lineIndex].AL_LineType);
				AssertEquals(line.AL_OSExTaxAmount, Job1Invoice.Lines[lineIndex].AL_OSExTaxAmount);
				AssertEquals(line.AL_AT, Job1Invoice.Lines[lineIndex].AL_AT);
				AssertEquals("PostDate on line should be today's date", ZDateTime.Now.Date, line.AL_PostDate.Date);
				AssertEquals("Foreign Key", invoiceReversal.PK, line.AL_AH);
				AssertEquals("PostToGL should be false on the reversing line", "N", line.AL_PostToGL);
				lineIndex++;
			}

			filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.Equal, Job1CreditNote.PK);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			InvoicingBase[] creditNoteReversals = Factory.Load<InvoicingBase>(filter);
			AssertEquals("Should be only one transaction present", 1, creditNoteReversals.Length);
			ARInvoice creditNoteReversal = creditNoteReversals[0] as ARInvoice;
			AssertNotNull(creditNoteReversal);

			//Header Assertions
			AssertEquals("Reversing Invoice should have IsCancelled flag as true", ZBool.True, creditNoteReversal.AH_IsCancelled);
			AssertEquals("Fully paid date of today", ZDateTime.Now.Date, creditNoteReversal.AH_FullyPaidDate.Date);
			AssertEquals("Reversing invoice oustanding amount", 0m, creditNoteReversal.AH_OutstandingAmount);
			AssertEquals("Reversing invoice invoice date", ZDateTime.Now.Date, creditNoteReversal.AH_InvoiceDate.Date);
			AssertEquals("Reversing invoice due date", ZDateTime.Now.Date.AddDays(7), creditNoteReversal.AH_DueDate.Date);
			AssertEquals("Reversing invoice post date", ZDateTime.Now.Date, invoiceReversal.AH_PostDate.Date);
			AssertEquals("Currency should be the same as original invoice", Job1CreditNote.AH_RX_NKTransactionCurrency, creditNoteReversal.AH_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate should be the same as original invoice", Job1CreditNote.AH_ExchangeRate, creditNoteReversal.AH_ExchangeRate);
			AssertEquals("AH_OSExTaxAmount should be the same", Job1CreditNote.AH_OSExTaxAmount, creditNoteReversal.AH_OSExTaxAmount);
			AssertEquals("OSGSTTaxAmount should be the same", Job1CreditNote.AH_OSTaxAmount, creditNoteReversal.AH_OSTaxAmount);
			AssertEquals("Description", ("reversal related to " + Job1CreditNote.AH_TransactionNum + " because I feel like reversing - very very very very long description of my reasons").ToLower(), creditNoteReversal.AH_Desc.ToLower());
			AssertEquals("Job on header should be same as reversal", Job1CreditNote.AH_JH, creditNoteReversal.AH_JH);
			AssertEquals("Branch should be same as reversal", Job1CreditNote.AH_GB, creditNoteReversal.AH_GB);
			AssertEquals("Department should be same as reversal", Job1CreditNote.AH_GE, creditNoteReversal.AH_GE);
			AssertEquals("AH_TransactionCategory should be same as reversal", Job1CreditNote.AH_TransactionCategory, creditNoteReversal.AH_TransactionCategory);
			AssertEquals("Currency should be same as reversal", Job1CreditNote.AH_RX_NKTransactionCurrency, creditNoteReversal.AH_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate should be same as reversal", Job1CreditNote.AH_ExchangeRate, creditNoteReversal.AH_ExchangeRate);
			AssertEquals("Consol Invoice Ref should be same as reversal", "Job1/C", creditNoteReversal.AH_ConsolidatedInvoiceRef);

			//Line Assertions
			lineIndex = 0;
			foreach (InvoicingLineBase line in creditNoteReversal.Lines)
			{
				AssertEquals(line.AL_LineType, Job1CreditNote.Lines[lineIndex].AL_LineType);
				AssertEquals(line.AL_OSExTaxAmount, Job1CreditNote.Lines[lineIndex].AL_OSExTaxAmount);
				AssertEquals(line.AL_AT, Job1CreditNote.Lines[lineIndex].AL_AT);
				AssertEquals("PostDate on line should be today's date", ZDateTime.Now.Date, line.AL_PostDate.Date);
				AssertEquals("Foreign Key", creditNoteReversal.PK, line.AL_AH);
				AssertEquals("PostToGL should be false on the reversing line", "N", line.AL_PostToGL);
				lineIndex++;
			}

			TransactionMatchLinkCollection matchLinkCollection = new TransactionMatchLinkCollection(Factory);
			matchLinkCollection.Load();
			AssertEquals("Should be 4 matchlink rows", 4, matchLinkCollection.Count);

			ZQuery transactionsFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, SQLComparisonOperator.Equal, Job1Invoice.PK);
			TransactionMatchLink job1InvoiceMatchlink = (TransactionMatchLink)matchLinkCollection.Find(transactionsFilter)[0];
			CheckMatchlinkToTransaction(job1InvoiceMatchlink, Job1Invoice);
			transactionsFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, SQLComparisonOperator.Equal, Job1CreditNote.PK);
			TransactionMatchLink job1CreditNoteMatchlink = (TransactionMatchLink)matchLinkCollection.Find(transactionsFilter)[0];
			CheckMatchlinkToTransaction(job1CreditNoteMatchlink, Job1CreditNote);
			transactionsFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, SQLComparisonOperator.Equal, invoiceReversal.PK);
			TransactionMatchLink invoiceReversalMatchlink = (TransactionMatchLink)matchLinkCollection.Find(transactionsFilter)[0];
			CheckMatchlinkToTransaction(invoiceReversalMatchlink, invoiceReversal);
			transactionsFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, SQLComparisonOperator.Equal, creditNoteReversal.PK);
			TransactionMatchLink creditNoteReversalMatchlink = (TransactionMatchLink)matchLinkCollection.Find(transactionsFilter)[0];
			CheckMatchlinkToTransaction(creditNoteReversalMatchlink, creditNoteReversal);

			Factory.Save();
			ZDecimal matchLinkSum = 0;
			foreach (TransactionMatchLink matchLink in matchLinkCollection)
			{
				if (matchLink.AP_AH == Job1Invoice.PK || matchLink.AP_AH == invoiceReversal.PK)
				{
					AssertEquals("Match group number", peekedMatchGroupNum, matchLink.AP_MatchGroupNum);
				}
				else
				{
					ZString nextPeekedMatchGroupNum = peekedMatchGroupNum[0] +
						(int.Parse(peekedMatchGroupNum.Substring(1, peekedMatchGroupNum.Length - 1)) + 1).ToString("00000000");
					AssertEquals("Match group number", nextPeekedMatchGroupNum, matchLink.AP_MatchGroupNum);
				}
				matchLinkSum += matchLink.AP_Amount;
			}
			AssertEquals("Matchlink should always sum to zero", 0m, matchLinkSum);
		}

		#endregion

		#region TestReverseInvoiceUsesOriginalInvoiceAmount

		public void TestReverseInvoiceUsesOriginalInvoiceAmount()
		{
			SetupJobTransactions();
			ARInvoice aRInvToReverse = Factory.NewWithValidTestData<ARInvoice>();
			aRInvToReverse.AH_RX_NKTransactionCurrency = ObjectCreator.USD.RX_Code;
			aRInvToReverse.AH_ExchangeRate = 0.8M;

			ARInvoiceLine aRInvLine = aRInvToReverse.Lines.AddNew() as ARInvoiceLine;
			aRInvLine.AL_AG = ObjectCreator.GLHeader1.PK;
			aRInvLine.AL_OSExTaxAmount = 500M;
			aRInvLine.AL_OSTaxAmount = 50M;
			int multiplier = aRInvLine.AL_LineAmount < 0 ? -1 : 1;
			aRInvLine.AL_LineAmount = 630M * multiplier;    // Incorrect Local Amount but should be copied over to reversing ARCreditNote
			aRInvLine.AL_GSTVAT = 64M * multiplier; // Incorrect LocalTax Amount (62.5 is correct)

			ARInvoiceLine aRInvLine2 = aRInvToReverse.Lines.AddNew() as ARInvoiceLine;
			aRInvLine2.AL_AG = ObjectCreator.GLHeader1.PK;
			aRInvLine2.AL_OSExTaxAmount = 200M;
			aRInvLine2.AL_OSTaxAmount = 20M;
			aRInvLine2.AL_LineAmount = 255M * multiplier;   // Incorrect LocalExTax Amount (250 is correct) but should be copied over to reversing ARCreditNote
			aRInvLine2.AL_GSTVAT = 26M * multiplier;    // Incorrect LocalTax Amount (25 is correct) but should be copied over to reversing ARCreditNote

			aRInvToReverse.AH_InvoiceAmount = 875M * multiplier;    // Correct LocalExTax Amount according to Header OSExTax Amount
																	// Incorrect LocalExTax Amount on Header according to Line LocalExTax Amounts

			aRInvToReverse.AH_GSTAmount = 89M * multiplier;     // Incorrect LocalTax Amount on Header
			aRInvToReverse.AH_OutstandingAmount = aRInvToReverse.AH_InvoiceAmount + aRInvToReverse.AH_GSTAmount;

			AssertEquals("The ARInv should have OSExTax amount = 700", 700M, aRInvToReverse.AH_OSExTaxAmount);
			AssertEquals("The ARInv should have OSTax amount = 70", 70M, aRInvToReverse.AH_OSTaxAmount);
			AssertEquals("ARInv should have LocalExTax amount = 875", 875M, aRInvToReverse.AH_LocalExTaxAmount);
			AssertEquals("ARInv should have LocalTax amount = 89", 89M, aRInvToReverse.AH_LocalTaxAmount);
			AssertEquals("ARInv should have ExchangeRate = 0.8", 0.8M, aRInvToReverse.AH_ExchangeRate);

			Factory.Save();

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			AssertEquals("ReverseTransactions should be empty", 0, Factory.GetDatabaseCount(typeof(InvoicingBase), filter));

			InvoicingBaseCollection invoiceCollection = new InvoicingBaseCollection(Factory);
			invoiceCollection.Add(aRInvToReverse);
			Reverser = new JobTransactionReverser(invoiceCollection.ToArray<InvoicingBase>());
			Reverser.ReverseAllInvoices("!@#", "tst");
			Factory.Save();
			filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.Equal, aRInvToReverse.PK);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			AssertEquals("ReverseTransactions should contain a reversing ARCreditNote", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), filter));
			ARCreditNote revCrd = Factory.LoadTop1<ARCreditNote>(filter);
			AssertNotNull("ReverseTransactions should contain a ARCreditNote", revCrd);
			AssertEquals("The OSExTax amount should be 700", 700M, revCrd.AH_OSExTaxAmount);
			AssertEquals("The OSTax amount should be 70", 70M, revCrd.AH_OSTaxAmount);
			AssertEquals("The ExchangeRate should be 0.8", 0.8M, revCrd.AH_ExchangeRate);
			AssertEquals("The LocalExTax amount should be 885", 885M, revCrd.AH_LocalExTaxAmount);
			AssertEquals("The LocalTax amount should be 90", 90M, revCrd.AH_LocalTaxAmount);

			revCrd.Lines.Sort(AccTransactionLinesSchema.AL_OSAmount.Name, System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("There should be 2 lines in reversing AR CreditNote", 2, revCrd.Lines.Count);
			ARCreditNoteLine aRCrdLine = revCrd.Lines[0] as ARCreditNoteLine;
			AssertEquals("LocalExTax amount on ARCreditNoteLine should be 630", 630M, aRCrdLine.AL_LocalExTaxAmount);
			AssertEquals("OSExTax amount on ARCreditNoteLine should be 500", 500M, aRCrdLine.AL_OSExTaxAmount);
			AssertEquals("LocalTax amount on ARCreditNoteLine should be 64", 64M, aRCrdLine.AL_LocalTaxAmount);
			AssertEquals("OSTaxAmount on ARCreditNoteLine should be 50", 50M, aRCrdLine.AL_OSTaxAmount);

			ARCreditNoteLine aRCrdLine2 = revCrd.Lines[1] as ARCreditNoteLine;
			AssertEquals("LocalExTax amount should be 255", 255M, aRCrdLine2.AL_LocalExTaxAmount);
			AssertEquals("OSExTax amount on ARCrdLine2 should be 200", 200M, aRCrdLine2.AL_OSExTaxAmount);
			AssertEquals("LocalTax amount on ARCrdLine2 should be 26", 26M, aRCrdLine2.AL_LocalTaxAmount);
			AssertEquals("OSTaxAmount should be 20", 20M, aRCrdLine2.AL_OSTaxAmount);
		}

		#endregion

		#region TestReverseLoadedInvoiceUsesBranchAndDeptOnLine

		public void TestReverseLoadedInvoiceUsesBranchAndDeptOnLine()
		{
			GlbDepartment deptLine = Factory.NewWithValidTestData<GlbDepartment>();
			GlbBranch branchLine = GlbBranch.CurrentBranch;

			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoiceLine aRInvLine = (ARInvoiceLine)aRInv.Lines.AddNew();
			aRInvLine.AL_JH = testJob.PK;
			aRInvLine.AL_GB = branchLine.PK;
			aRInvLine.AL_GE = deptLine.PK;
			aRInvLine.AL_AG = ObjectCreator.GLHeader1.PK;
			ObjectCreator.CreateJobCharge(aRInvLine, testJob, ObjectCreator.CC1, ObjectCreator.AUD);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ARInvoice loadedARInv = newFactory.Load<ARInvoice>(aRInv.PK);

			InvoicingBaseCollection invoiceCollection = new InvoicingBaseCollection(newFactory);
			invoiceCollection.Load();
			Reverser = new JobTransactionReverser(invoiceCollection.ToArray<InvoicingBase>());
			Reverser.ReverseAllInvoices("%^^", "tst");
			Factory.Save();

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.Equal, loadedARInv.PK);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			ARCreditNote aRCrd = newFactory.LoadTop1<ARCreditNote>(filter);
			AssertNotNull("There should be 1 ARCreditNote in ReverseTransactions", aRCrd);

			ARCreditNoteLine aRCrdLine = (ARCreditNoteLine)aRCrd.Lines[0];
			AssertEquals("The branch on the reversing line should be the branch on the original line", branchLine.PK, aRCrdLine.AL_GB);
			AssertEquals("The dept on the reversing line should be the dept on the original line", deptLine.PK, aRCrdLine.AL_GE);
		}

		#endregion

		#region TestReverseCreditNoteUsesOriginalInvoiceAmount

		public void TestReverseCreditNoteUsesOriginalInvoiceAmount()
		{
			SetupJobTransactions();
			ARCreditNote aRCrd = Factory.NewWithValidTestData<ARCreditNote>();
			aRCrd.AH_RX_NKTransactionCurrency = ObjectCreator.USD.RX_Code;
			aRCrd.AH_ExchangeRate = 0.4M;
			ARCreditNoteLine aRCrdLine = aRCrd.Lines.AddNew() as ARCreditNoteLine;
			aRCrdLine.AL_AG = ObjectCreator.GLHeader1.PK;
			aRCrdLine.AL_OSExTaxAmount = 300M;
			aRCrdLine.AL_OSTaxAmount = 40M;
			int multiplier = aRCrdLine.AL_LineAmount < 0 ? -1 : 1;
			aRCrdLine.AL_LineAmount = 740M * multiplier;    // incorrect Local Amount but should not be copied over to reversing ARInvoice
			aRCrdLine.AL_GSTVAT = 110M * multiplier;
			aRCrd.AH_InvoiceAmount = 740m * multiplier;
			aRCrd.AH_GSTAmount = 110m * multiplier;

			AssertEquals("LocalExTax amount should be 740", 740M, aRCrd.AH_LocalExTaxAmount);
			AssertEquals("OSExTax amount should be 300", 300M, aRCrd.AH_OSExTaxAmount);
			AssertEquals("LocalTax amount should be 110", 110M, aRCrd.AH_LocalTaxAmount);
			AssertEquals("OSTax amount should be 40", 40M, aRCrd.AH_OSTaxAmount);

			AssertEquals("OSExTax amount on line should be 300", 300M, aRCrdLine.AL_OSExTaxAmount);
			AssertEquals("LocalExTax amount on line should be 740", 740M, aRCrdLine.AL_LocalExTaxAmount);
			AssertEquals("OSTax amount on line should be 40", 40M, aRCrdLine.AL_OSTaxAmount);
			AssertEquals("LocalTax amount on line should be 110", 110M, aRCrdLine.AL_LocalTaxAmount);

			InvoicingBaseCollection invoiceCollection = new InvoicingBaseCollection(Factory);
			invoiceCollection.Add(aRCrd);
			Reverser = new JobTransactionReverser(invoiceCollection.ToArray<InvoicingBase>());
			Reverser.ReverseAllInvoices("&*^", "tst");
			Factory.Save();

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			AssertEquals("There should be 1 ARInvoice in ReverseTransactions", 1, Factory.GetDatabaseCount(typeof(InvoicingBase), filter));
			filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.Equal, aRCrd.PK);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			ARInvoice aRInv = Factory.LoadTop1<ARInvoice>(filter);
			AssertNotNull("There should be 1 ARInvoice in ReverseTransactions", aRInv);

			AssertEquals("The LocalExTax Amount on ARInv should be 740", 740M, aRInv.AH_LocalExTaxAmount);
			AssertEquals("The OSExTax amount on ARInv should be 300", 300M, aRInv.AH_OSExTaxAmount);
			AssertEquals("The ExchangeRate on ARInv should be 0.4", 0.4M, aRInv.AH_ExchangeRate);
			AssertEquals("The OSTax amount on ARInv should be 40", 40M, aRInv.AH_OSTaxAmount);
			AssertEquals("The LocalTax amount on ARInv should be 110", 110M, aRInv.AH_LocalTaxAmount);

			ARInvoiceLine aRInvLine = aRInv.Lines[0] as ARInvoiceLine;
			AssertNotNull("There should be 1 ARInvoiceLine", aRInvLine);
			AssertEquals("The OSExTax amount on ARInvoiceLine should be 300", 300M, aRInvLine.AL_OSExTaxAmount);
			AssertEquals("The LocalExTax amount on ARInvoiceLine should be 740", 740M, aRInvLine.AL_LocalExTaxAmount);
			AssertEquals("The OSTax amount on ARInvoiceLine should be 40", 40M, aRInvLine.AL_OSTaxAmount);
			AssertEquals("The LocalTax amount on ARInvoiceLine should be 110", 110M, aRInvLine.AL_LocalTaxAmount);
		}

		#endregion

		#region TestReverseLoadedCreditNoteUsesBranchAndDeptOnLine

		public void TestReverseLoadedCreditNoteUsesBranchAndDeptOnLine()
		{
			GlbDepartment deptLine = Factory.NewWithValidTestData<GlbDepartment>();
			GlbBranch branchLine = GlbBranch.CurrentBranch;

			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;

			ARCreditNote aRCrd = Factory.NewWithValidTestData<ARCreditNote>();
			ARCreditNoteLine aRCrdLine = (ARCreditNoteLine)aRCrd.Lines.AddNew();
			aRCrdLine.AL_JH = testJob.PK;
			aRCrdLine.AL_GB = branchLine.PK;
			aRCrdLine.AL_GE = deptLine.PK;
			aRCrdLine.AL_AG = ObjectCreator.GLHeader1.PK;
			ObjectCreator.CreateJobCharge(aRCrdLine, testJob, ObjectCreator.CC1, ObjectCreator.AUD);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ARCreditNote aRCrdLoaded = Factory.Load<ARCreditNote>(aRCrd.PK);

			InvoicingBaseCollection invoiceCollection = new InvoicingBaseCollection(newFactory);
			invoiceCollection.Add(aRCrdLoaded);
			Reverser = new JobTransactionReverser(invoiceCollection.ToArray<InvoicingBase>());

			Reverser.ReverseAllInvoices("%^^", "tst");
			Factory.Save();

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.Equal, aRCrdLoaded.PK);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			ARInvoice aRInv = Factory.LoadTop1<ARInvoice>(filter);

			AssertNotNull("There should be 1 ARInvoice in ReverseTransactions", aRInv);
			ARInvoiceLine aRInvLine = (ARInvoiceLine)aRInv.Lines[0];
			AssertEquals("The branch on the reversing line should be the branch on the original line", branchLine.PK, aRInvLine.AL_GB);
			AssertEquals("The dept on the reversing line should be the dept on the original line", deptLine.PK, aRInvLine.AL_GE);
		}

		#endregion

		#region TestReverseInvoiceAndCreditNoteHasCorrectTaxElements

		public void TestReverseInvoiceAndCreditNoteHasCorrectTaxElements()
		{
			SetupJobTransactions();
			ZQuery collectionFilter = new ZQuery(AccTransactionHeaderSchema.PK, Job1Invoice.PK);
			collectionFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.PK, SQLComparisonOperator.Equal, Job1CreditNote.PK);
			collectionFilter.OrderBy = InvoicingBase.Schema.AH_TransactionType + " DESC";//get invoice and then credit note for testing needs
			InvoicingBaseCollection collection = new InvoicingBaseCollection(Factory, collectionFilter);
			collection.Load();

			SetupUniqueTaxID();

			Reverser = new JobTransactionReverser(collection.ToArray<InvoicingBase>());
			Reverser.ReverseAllInvoices("because I feel like reversing - very very very very long description of my reasons", "tst");
			Factory.Save();

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.Equal, Job1Invoice.PK);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			InvoicingBase[] invoiceReversals = Factory.Load<InvoicingBase>(filter);
			AssertEquals("Should be only one transaction present", 1, invoiceReversals.Length);
			ARCreditNote invoiceReversal = (ARCreditNote)invoiceReversals[0];

			int lineIndex = 0;
			foreach (InvoicingLineBase line in invoiceReversal.Lines)
			{
				AssertEquals(line.AL_OSExTaxAmount, Job1Invoice.Lines[lineIndex].AL_OSExTaxAmount);
				AssertEquals(line.AL_LineAmount, -1 * Job1Invoice.Lines[lineIndex].AL_LineAmount);
				AssertEquals(line.AL_AT, Job1Invoice.Lines[lineIndex].AL_AT);
				AssertEquals(line.AL_OSTaxAmount, Job1Invoice.Lines[lineIndex].AL_OSTaxAmount);
				AssertEquals(line.AL_GSTVAT, -1 * Job1Invoice.Lines[lineIndex].AL_GSTVAT);
				lineIndex++;
			}

			filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.Equal, Job1CreditNote.PK);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			InvoicingBase[] creditNoteReversals = Factory.Load<InvoicingBase>(filter);
			AssertEquals("Should be only one transaction present", 1, creditNoteReversals.Length);
			ARInvoice creditNoteReversal = (ARInvoice)creditNoteReversals[0];

			lineIndex = 0;
			foreach (InvoicingLineBase line in creditNoteReversal.Lines)
			{
				AssertEquals(line.AL_OSExTaxAmount, Job1CreditNote.Lines[lineIndex].AL_OSExTaxAmount);
				AssertEquals(line.AL_LineAmount, -1 * Job1CreditNote.Lines[lineIndex].AL_LineAmount);
				AssertEquals(line.AL_AT, Job1CreditNote.Lines[lineIndex].AL_AT);
				AssertEquals(line.AL_OSTaxAmount, Job1CreditNote.Lines[lineIndex].AL_OSTaxAmount);
				AssertEquals(line.AL_GSTVAT, -1 * Job1CreditNote.Lines[lineIndex].AL_GSTVAT);
				lineIndex++;
			}
		}

		#endregion

		#region TestReverseInvoiceMaintainsApprovingUserDetails

		public void TestReverseAllInvoices_AddsApprovingUserInfoFromOriginalToReverseTransaction()
		{
			SecurityTestObject.CreateTestUser(true, "", "tst", "testUser", "password");
			var testObjectCreator = new TestObjectCreator(Factory);
			var shipment = testObjectCreator.CreateShipment("S00001000");
			var job = testObjectCreator.CreateJob(shipment, false);
			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc",
				testObjectCreator.AUD, 100m, testObjectCreator.AALSHI,
				testObjectCreator.AUD, 100m, testObjectCreator.ABIGAS);
			var chargePoster = new ChargePoster(Factory);
			var invoice = chargePoster.Post(charge);
			Factory.Save();

			var reverser = new JobTransactionReverser(new List<InvoicingBase> { invoice });
			invoice.ApprovingUserPK = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "tst")).PK;
			invoice.ApprovalDate = new ZDateTime(2019, 1, 1);

			var reversedInvoices = reverser.ReverseAllInvoices("Incorrect Data Entry", "IDE");
			Assert("reverse successful", reversedInvoices.Any());
			Factory.Save();

			var creditNotes = Factory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("Count", 1, creditNotes.Length);

			var reversedNoteATHLogs = creditNotes.First(x => x.IsReversalTransaction).Logs.GetAllLogs().Where(x => x.SL_SE_NKEvent == Events.Authorised.Code).Cast<StmALog>();
			AssertEquals("Should contain authorisation log when creating a reverse credit note", 1, reversedNoteATHLogs.Count());
			AssertContains("Should contain log for User1", "testUser", reversedNoteATHLogs.First().SL_Reference);
			AssertEquals("Should use date from invoice", new ZDateTime(2019, 1, 1), reversedNoteATHLogs.First().SL_EventTime);
		}

		#endregion

		#region TestReverseCFXJournalOnReversingInvoiceOrCreditNote

		public void TestReverseCFXJournalOnReversingInvoiceOrCreditNote()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			ObjectCreator.CreateExchangeRate(Job1, ObjectCreator.USD, 0.6m);

			Charge localClientChargeWithCFX = ObjectCreator.CreateCharge(Job1, MarginChargeCode, "DESC", ObjectCreator.USD, 100m, ObjectCreator.ABIGAS, "", ObjectCreator.USD, 100m, LocalClient);
			localClientChargeWithCFX.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			localClientChargeWithCFX.JR_GE = ObjectCreator.FESDepartment.PK;

			Charge agentChargeWithoutCFX = ObjectCreator.CreateCharge(Job1, MarginChargeCode, "DESC", ObjectCreator.USD, 100m, ObjectCreator.ABIGAS, "", GlbCompany.CurrentCompany.LocalCurrency, 100m, Agent);
			agentChargeWithoutCFX.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			agentChargeWithoutCFX.JR_GE = ObjectCreator.FESDepartment.PK;
			AssertEquals("Precondition - CFX should be set", 18.52m, localClientChargeWithCFX.JR_CFXAmt);
			AssertEquals("Precondition - CFX should be zero - does not apply", 0m, agentChargeWithoutCFX.JR_CFXAmt);

			new InvoicingPostManager(Job1).CreateTransactions(JobInvoicingPostingOption.All);

			ZQuery cFXFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Journal);
			cFXFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.JobCosting);
			cFXFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			JCJournalHeader[] cFXJournals = Factory.Load<JCJournalHeader>(cFXFilter);

			AssertEquals("Should only be one CFX Journal posted", 1, cFXJournals.Length);

			JCJournalHeader cFXJournal = cFXJournals[0];
			AssertEquals("Should only have one line", 1, cFXJournal.Lines.Count);
			AssertEquals("Should be for -18.52", -18.52m, cFXJournal.Lines[0].AL_LineAmount);

			ZQuery invoicesFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Invoice);
			invoicesFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			invoicesFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			TransactionHeader[] loadedInovices = Factory.Load<TransactionHeader>(invoicesFilter);
			AssertEquals("Should be 2 invoices created", 2, loadedInovices.Length);

			ARInvoice localClientInvoice = null;
			ARInvoice agentInvoice = null;

			foreach (InvoicingBase invoice in loadedInovices)
			{
				if (invoice.AH_OH == Agent.PK)
				{
					agentInvoice = (ARInvoice)invoice;
				}
				else if (invoice.AH_OH == LocalClient.PK)
				{
					localClientInvoice = (ARInvoice)invoice;
				}
				else
				{
					Fail("Org was not local client or agent");
				}
			}

			InvoicingBaseCollection invoicesToReverse = new InvoicingBaseCollection(Factory, new ZQuery(invoicesFilter));
			invoicesToReverse.Load();
			AssertEquals("Should have loaded an invoice and a credit note", 2, invoicesToReverse.Count);

			Reverser = new JobTransactionReverser(invoicesToReverse.ToArray<InvoicingBase>());
			Reverser.ReverseAllInvoices("Imraan and Zubin are code monkeys", "tst");
			Factory.Save();

			foreach (TransactionHeader invoice in loadedInovices)
			{
				Assert("Should have reversed invoice/credit note", invoice.AH_IsCancelled);
			}

			Assert("Should have reversed CFX header as well", cFXJournal.AH_IsCancelled);
			AssertNull("CFX Line should also be null now", localClientChargeWithCFX.CFXLine);
			AssertNull("CFX Line should still be nullnow", agentChargeWithoutCFX.CFXLine);
		}

		#endregion

		#region TestReverseJobConsolInvoiceWhenReversingARAgentInvoice

		public void TestReverseJobConsolInvoiceWhenReversingARAgentInvoice()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ARInvoice aRInvoice = Factory.New<ARInvoice>();
			aRInvoice.AH_JH = creator.Job1.PK;
			aRInvoice.AH_GC = GlbCompany.CurrentCompany.PK;
			APInvoice aPInvoice = Factory.New<APInvoice>();
			aPInvoice.AH_GC = GlbCompany.CurrentCompany.PK;
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			JobConsolCost cost = Factory.New<JobConsolCost>();
			using (cost.ReportSettingParentSuspender.GetSuspender())
			{
				cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			}
			cost.E6_AH_APInvoice = aPInvoice.PK;
			cost.E6_AH_ARInvoice = aRInvoice.PK;

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Add(aRInvoice);

			Reverser = new JobTransactionReverser(invoices.ToArray<InvoicingBase>());
			Reverser.ReverseAllInvoices("blah", "tst");
			Assert("Should reverse ap invoice", aPInvoice.AH_IsCancelled);
		}

		#endregion

		#region TestReverseInvoceSendingEmail

		public void TestReverseInvoceSendingEmail()
		{
			SetupStaffMemberEmailAddress();
			Guid originalReverseGroup = AccountingConfigurationRegistry.Instance.TransactionReverseNotifyGroup.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			try
			{
				AccountingConfigurationRegistry.Instance.TransactionReverseNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, StaffGroupPK.ToGuid());

				SetupJobTransactions();
				ZQuery collectionFilter = new ZQuery(AccTransactionHeaderSchema.PK, Job1Invoice.PK);
				collectionFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.PK, SQLComparisonOperator.Equal, Job1CreditNote.PK);
				collectionFilter.OrderBy = InvoicingBase.Schema.AH_TransactionType + " DESC";//get invoice and then credit note for testing needs
				InvoicingBaseCollection collection = new InvoicingBaseCollection(Factory, collectionFilter);
				collection.Load();

				Reverser = new JobTransactionReverser(collection.ToArray<InvoicingBase>());
				Reverser.ReverseAllInvoices("because I feel like reversing - very very very very long description of my reasons", "tst");

				AssertEquals("Should have not sent email until factory saving", 0, Env.OutgoingMailManager.EmailsCreated.Count);

				Factory.Save();

				AssertEquals("Should have sent email", 2, Env.OutgoingMailManager.EmailsCreated.Count);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.TransactionReverseNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalReverseGroup);
			}
		}

		#endregion

		#region Implementation

		protected void CheckMatchlinkToTransaction(TransactionMatchLink matchLink, InvoicingBase invoice)
		{
			AssertEquals("Matching amount should be local+GST", invoice.AH_InvoiceAmount + invoice.AH_GSTAmount, matchLink.AP_Amount);
			AssertEquals("Matching date should be today", ZDateTime.Now.Date, matchLink.AP_MatchDate.Date);
		}

		protected JobTransactionReverser Reverser;

		protected Job Job1;
		protected Job Job2;
		protected ARInvoice ConsolInvoice;
		protected ARInvoice Job1Invoice;
		protected ARCreditNote Job1CreditNote;

		protected OrgHeader Agent;
		protected OrgHeader LocalClient;

		protected RefCurrency ForeignCurrency;

		protected WIP Job1WIP1;
		protected WIP Job1WIP2;
		protected Accrual Job1Accrual1;
		protected Accrual Job1Accrual2;

		protected WIP Job2WIP1;
		protected WIP Job2WIP2;
		protected Accrual Job2Accrual1;
		protected Accrual Job2Accrual2;

		protected AccChargeCode MarginChargeCode;
		protected AccChargeCode DisbursementChargeCode;

		protected AccTaxRate UniqueTaxRate;
		protected ZGuid MarginChargeCodeOriginalTaxRate;
		protected ZGuid DisbursementChargeCodeOriginalTaxRate;

		protected GlbBranch NonCurrentBranch;
		protected GlbDepartment NonCurrentDepartment;

		protected ZDateTime PreviousDate;

		#region Emailing

		protected ZGuid StaffGroupPK
		{
			get
			{
				GlbGroup group = Factory.New<GlbGroup>();
				group.Staff.Add(Factory.Load<GlbStaff>(new ZGuid(GlbStaff.CurrentUser.PK)));
				return group.PK;
			}
		}

		protected ZString EmailAddress
		{
			get { return new ZString("blahblah@whatever.example"); }
		}

		protected void SetupStaffMemberEmailAddress()
		{
			BusinessObjectFactory staffMemberFactory = new BusinessObjectFactory();
			GlbStaff currentStaffMember = staffMemberFactory.Load<GlbStaff>(new ZGuid(GlbStaff.CurrentUser.PK));
			currentStaffMember.GS_EmailAddress = EmailAddress;
			staffMemberFactory.Save();
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			ObjectCreator = new TestObjectCreator(Factory);
			SetupOrganisations();
			SetupJobs();
			SetupChargeCodes();
			SetupForeignCurrency();
			SetupNonCurrentBranch();
			SetupNonCurrentDepartment();
			PreviousDate = ZDateTime.Now.AddDays(-2);
			Factory.Save();
		}

		TestObjectCreator ObjectCreator;

		protected void SetupJobs()
		{
			Job1 = ObjectCreator.CreateJob(LocalClient, 10.00m, Agent, 5.00m);
			Job1.JH_JobNum = "Job1";
			Job1.JH_UniqueJobInvoiceNumber = 2;

			Job2 = ObjectCreator.CreateJob(LocalClient, 10.00m, Agent, 5.00m);
			Job2.JH_JobNum = "Job2";
		}

		protected void SetupOrganisations()
		{
			LocalClient = Factory.LoadTop1<OrgHeader>(new ZQuery());
			ZQuery agentFilter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.NotEqual, LocalClient.OH_Code);
			Agent = Factory.LoadTop1<OrgHeader>(agentFilter);
			LocalClient.OH_IsDebtor = ZBool.True;
			Agent.OH_IsDebtor = ZBool.True;

			LocalClient.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Core.Constants.InvoiceTerms.FromInvoiceDate;
			LocalClient.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = (ZByte)7;

			Agent.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Core.Constants.InvoiceTerms.FromInvoiceDate;
			Agent.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = (ZByte)7;
		}

		protected void SetupForeignCurrency()
		{
			ForeignCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
		}

		protected void SetupNonCurrentBranch()
		{
			ZQuery filter = new ZQuery(GlbBranchSchema.GB_Code, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.GB_Code);
			filter.AddToFilter(JoinCondition.And, GlbBranchSchema.GB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			NonCurrentBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(filter));
		}

		protected void SetupNonCurrentDepartment()
		{
			ZQuery filter = new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.NotEqual, GlbDepartment.CurrentDepartment.GE_Code);
			NonCurrentDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(filter));
		}

		protected void SetupChargeCodes()
		{
			MarginChargeCode = ObjectCreator.CreateChargeCode("MRG", "MARGIN CHARGE", Core.Constants.ChargeType.Margin, 90.00m, null, null, "ALL");
			DisbursementChargeCode = ObjectCreator.CreateChargeCode("DSB", "Disbursement CHARGE", Core.Constants.ChargeType.Disbursement, 100.00m, null, null, "ALL");
		}

		protected void SetupUniqueTaxID()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			MarginChargeCode = newFactory.Load<AccChargeCode>(MarginChargeCode.PK);
			DisbursementChargeCode = newFactory.Load<AccChargeCode>(DisbursementChargeCode.PK);

			UniqueTaxRate = newFactory.New<AccTaxRate>();
			UniqueTaxRate.AT_Code = "XXXXX";
			UniqueTaxRate.AT_RN_NKCountry = Env.CurrentCompany.Country.Code;

			MarginChargeCodeOriginalTaxRate = MarginChargeCode.AC_AT_GSTRate;
			MarginChargeCode.AC_AT_GSTRate = UniqueTaxRate.PK;
			DisbursementChargeCodeOriginalTaxRate = DisbursementChargeCode.AC_AT_GSTRate;
			DisbursementChargeCode.AC_AT_GSTRate = UniqueTaxRate.PK;
		}

		protected void SetupJobTransactions()
		{
			Job1WIP1 = Factory.New<WIP>();
			Job1WIP2 = Factory.New<WIP>();
			Job1Accrual1 = Factory.New<Accrual>();
			Job1Accrual2 = Factory.New<Accrual>();

			Job2WIP1 = Factory.New<WIP>();
			Job2WIP2 = Factory.New<WIP>();
			Job2Accrual1 = Factory.New<Accrual>();
			Job2Accrual2 = Factory.New<Accrual>();

			SetupWIPAccrual(Job1WIP1, 15m, MarginChargeCode, Job1, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment);
			SetupWIPAccrual(Job1WIP2, 140m, DisbursementChargeCode, Job1, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment);
			SetupWIPAccrual(Job1Accrual1, 110m, MarginChargeCode, Job1, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment);
			SetupWIPAccrual(Job1Accrual2, 25m, DisbursementChargeCode, Job1, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment);

			SetupWIPAccrual(Job2WIP1, 15m, MarginChargeCode, Job2, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment);
			SetupWIPAccrual(Job2WIP2, 140m, DisbursementChargeCode, Job2, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment);
			SetupWIPAccrual(Job2Accrual1, 110m, MarginChargeCode, Job2, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment);
			SetupWIPAccrual(Job2Accrual2, 25m, DisbursementChargeCode, Job2, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment);

			SetupSingleJobInvoice();

			SetupSingleJobCreditNote();

			Factory.Save();
		}

		protected void SetupWIPAccrual(BaseWIPAccrual wIPToSetup, ZDecimal amount, AccChargeCode chargecode, Job job, GlbBranch branch, GlbDepartment department)
		{
			BaseCharge charge = wIPToSetup.Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_JH = job.PK;
			if (wIPToSetup is WIP)
			{
				charge.JR_AL_ARLine = wIPToSetup.PK;
			}

			if (wIPToSetup is Accrual)
			{
				charge.JR_AL_APLine = wIPToSetup.PK;
			}

			wIPToSetup.AL_OSExTaxAmount = amount;
			wIPToSetup.AL_AC = chargecode.PK;
			wIPToSetup.AL_JH = job.PK;
			wIPToSetup.AL_GB = branch.PK;
			wIPToSetup.AL_GE = department.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIPToSetup);
		}

		protected void SetupConsolInvoice()
		{
			ConsolInvoice = Factory.New<ARInvoice>();
			ConsolInvoice.AH_OH = LocalClient.PK;
			ConsolInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;

			ARInvoiceLine job1ConsolInvoiceLine = (ARInvoiceLine)ConsolInvoice.Lines.AddNew();
			job1ConsolInvoiceLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			job1ConsolInvoiceLine.AL_OSExTaxAmount = 200.00m;
			job1ConsolInvoiceLine.AL_OSTaxAmount = 20.00m;
			job1ConsolInvoiceLine.AL_JH = Job1.PK;
			job1ConsolInvoiceLine.AL_AC = MarginChargeCode.PK;

			ARInvoiceLine job2ConsolLine = (ARInvoiceLine)ConsolInvoice.Lines.AddNew();
			job2ConsolLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			job2ConsolLine.AL_OSExTaxAmount = 150.00m;
			job2ConsolLine.AL_OSTaxAmount = 20.00m;
			job2ConsolLine.AL_JH = Job2.PK;
			job2ConsolLine.AL_AC = DisbursementChargeCode.PK;

			ConsolInvoice.AH_OutstandingAmount = ConsolInvoice.AH_InvoiceAmount + ConsolInvoice.AH_GSTAmount;
		}

		protected void SetupSingleJobInvoice()
		{
			Job1Invoice = Factory.New<ARInvoice>();
			Job1Invoice.AH_PostDate = PreviousDate;
			Job1Invoice.AH_OH = LocalClient.PK;
			Job1Invoice.AH_JH = Job1.PK;
			Job1Invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			Job1Invoice.AH_GB = NonCurrentBranch.PK;
			Job1Invoice.AH_GE = NonCurrentDepartment.PK;
			Job1Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			Job1Invoice.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			Job1Invoice.AH_ExchangeRate = 0.6m;
			Job1Invoice.AH_ConsolidatedInvoiceRef = Job1.JH_JobNum;

			ARInvoiceLine job1JobInvoiceLine1 = (ARInvoiceLine)Job1Invoice.Lines.AddNew();
			job1JobInvoiceLine1.AL_LineType = TransactionLineTypes.Revenue;
			job1JobInvoiceLine1.AL_AC = MarginChargeCode.PK;
			job1JobInvoiceLine1.AL_JH = Job1.PK;
			job1JobInvoiceLine1.AL_OSExTaxAmount = 130.00m;
			job1JobInvoiceLine1.AL_OSTaxAmount = 13.00m;
			job1JobInvoiceLine1.AL_GB = NonCurrentBranch.PK;
			job1JobInvoiceLine1.AL_GE = NonCurrentDepartment.PK;
			job1JobInvoiceLine1.AL_PostDate = PreviousDate;
			job1JobInvoiceLine1.AL_PostToGL = "Y";
			ObjectCreator.CreateJobCharge(job1JobInvoiceLine1, Job1, MarginChargeCode, ForeignCurrency);

			ARInvoiceLine job1JobInvoiceLine2 = (ARInvoiceLine)Job1Invoice.Lines.AddNew();
			job1JobInvoiceLine2.AL_LineType = TransactionLineTypes.Revenue;
			job1JobInvoiceLine2.AL_AC = DisbursementChargeCode.PK;
			job1JobInvoiceLine2.AL_JH = Job1.PK;
			job1JobInvoiceLine2.AL_OSExTaxAmount = 200.00m;
			job1JobInvoiceLine2.AL_OSTaxAmount = 20.00m;
			job1JobInvoiceLine2.AL_GB = NonCurrentBranch.PK;
			job1JobInvoiceLine2.AL_GE = NonCurrentDepartment.PK;
			job1JobInvoiceLine2.AL_PostDate = PreviousDate;
			job1JobInvoiceLine2.AL_PostToGL = "Y";
			ObjectCreator.CreateJobCharge(job1JobInvoiceLine2, Job1, MarginChargeCode, ForeignCurrency);

			Job1Invoice.AH_OutstandingAmount = Job1Invoice.AH_InvoiceAmount + Job1Invoice.AH_GSTAmount;
		}

		protected void SetupSingleJobCreditNote()
		{
			Job1CreditNote = Factory.New<ARCreditNote>();
			Job1CreditNote.AH_PostDate = PreviousDate;
			Job1CreditNote.AH_OH = LocalClient.PK;
			Job1CreditNote.AH_JH = Job1.PK;
			Job1CreditNote.AH_Ledger = LedgerTypes.AccountsReceivable;
			Job1CreditNote.AH_GB = NonCurrentBranch.PK;
			Job1CreditNote.AH_GE = NonCurrentDepartment.PK;
			Job1CreditNote.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			Job1CreditNote.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			Job1CreditNote.AH_ExchangeRate = 0.6m;
			Job1CreditNote.AH_ConsolidatedInvoiceRef = Job1.JH_JobNum + "/A";

			ARCreditNoteLine job1JobCreditNoteLine1 = (ARCreditNoteLine)Job1CreditNote.Lines.AddNew();
			job1JobCreditNoteLine1.AL_LineType = TransactionLineTypes.Revenue;
			job1JobCreditNoteLine1.AL_AC = MarginChargeCode.PK;
			job1JobCreditNoteLine1.AL_JH = Job1.PK;
			job1JobCreditNoteLine1.AL_OSExTaxAmount = 99.00m;
			job1JobCreditNoteLine1.AL_OSTaxAmount = 9.90m;
			job1JobCreditNoteLine1.AL_GB = NonCurrentBranch.PK;
			job1JobCreditNoteLine1.AL_GE = NonCurrentDepartment.PK;
			job1JobCreditNoteLine1.AL_PostDate = PreviousDate;
			job1JobCreditNoteLine1.AL_PostToGL = "Y";
			ObjectCreator.CreateJobCharge(job1JobCreditNoteLine1, Job1, MarginChargeCode, ForeignCurrency);

			ARCreditNoteLine job1JobCreditNoteLine2 = (ARCreditNoteLine)Job1CreditNote.Lines.AddNew();
			job1JobCreditNoteLine2.AL_LineType = TransactionLineTypes.Revenue;
			job1JobCreditNoteLine2.AL_AC = DisbursementChargeCode.PK;
			job1JobCreditNoteLine2.AL_JH = Job1.PK;
			job1JobCreditNoteLine2.AL_OSExTaxAmount = 15.00m;
			job1JobCreditNoteLine2.AL_OSTaxAmount = 1.50m;
			job1JobCreditNoteLine2.AL_GB = NonCurrentBranch.PK;
			job1JobCreditNoteLine2.AL_GE = NonCurrentDepartment.PK;
			job1JobCreditNoteLine2.AL_PostDate = PreviousDate;
			job1JobCreditNoteLine2.AL_PostToGL = "Y";
			ObjectCreator.CreateJobCharge(job1JobCreditNoteLine2, Job1, MarginChargeCode, ForeignCurrency);

			Job1CreditNote.AH_OutstandingAmount = Job1CreditNote.AH_InvoiceAmount + Job1CreditNote.AH_GSTAmount;
		}

		#endregion
	}
}
