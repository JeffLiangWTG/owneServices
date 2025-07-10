using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class GeneralLedgerDataQueueTest : TestCaseWithFactory
	{
		[TestDate(2023, 10, 20)]
		public void TestRemoveNarrowGLD()
		{
			var currentCompany20221020 = CreateDummyGLD(new DateTime(2022, 10, 20), GlbCompany.CurrentCompany);
			var anotherCompany20221020 = CreateDummyGLD(new DateTime(2022, 10, 20), TestObjectCreator.NonCurrentCompany);
			var currentCompany20221021 = CreateDummyGLD(new DateTime(2022, 10, 21), GlbCompany.CurrentCompany);
			var anotherCompany20221021 = CreateDummyGLD(new DateTime(2022, 10, 21), TestObjectCreator.NonCurrentCompany);

			Factory.Save();
			AssertContainsExactElementsInAnyOrder("PreCondition"
				, new[] { currentCompany20221020, anotherCompany20221020, currentCompany20221021, anotherCompany20221021 }
				, ReloadAccGeneralLedgerDataPKs()
			);

			GeneralLedgerDataQueue.RemoveNarrowGLD(Db.Connection, GlbCompany.CurrentCompany.PK.ToGuid(), new DateTime(2022, 10, 20), new DateTime(2022, 10, 21));
			AssertContainsExactElementsInAnyOrder("Run current copmany 2022-10-20 to 2022-10-20 (add 1 day for date range)"
				, new[] { anotherCompany20221020, currentCompany20221021, anotherCompany20221021 }
				, ReloadAccGeneralLedgerDataPKs()
			);

			GeneralLedgerDataQueue.RemoveNarrowGLD(Db.Connection, GlbCompany.CurrentCompany.PK.ToGuid(), new DateTime(2022, 10, 21), new DateTime(2022, 10, 22));
			AssertContainsExactElementsInAnyOrder("Run current copmany 2022-10-21 to 2022-10-21 (add 1 day for date range)"
				, new[] { anotherCompany20221020, anotherCompany20221021 }
				, ReloadAccGeneralLedgerDataPKs()
			);

			GeneralLedgerDataQueue.RemoveNarrowGLD(Db.Connection, TestObjectCreator.NonCurrentCompany.PK.ToGuid(), new DateTime(2022, 10, 20), new DateTime(2022, 10, 22));
			AssertContainsExactElementsInAnyOrder("Run another copmany 2022-10-20 to 2022-10-21 (add 1 day for date range)"
				, Array.Empty<Guid>()
				, ReloadAccGeneralLedgerDataPKs()
			);

			ZGuid CreateDummyGLD(DateTime recognitionDate, GlbCompany company)
			{
				var narrowGLD = Factory.NewWithValidTestData<AccGeneralLedgerData>();
				narrowGLD.GLD_PostDate = recognitionDate;
				narrowGLD.GLD_GC_Company = company.PK;
				narrowGLD.GLD_Type = "PST";
				narrowGLD.GLD_GLAccountType = "ARC";
				narrowGLD.GLD_AH_TransactionHeader = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m).PK;
				narrowGLD.GLD_PostPeriod = 202301;
				return narrowGLD.PK;
			}

			IEnumerable<Guid> ReloadAccGeneralLedgerDataPKs() => new BusinessObjectFactory().Load<AccGeneralLedgerData>(new ZQuery()).Select(x => x.PK.ToGuid());
		}

		[SuspendCriticalValidation]
		[TestDate(2023, 10, 20)]
		public void TestQueueTransaction_MiscTransactions()
		{
			var date = new DateTime(2023, 02, 01);

			var arJournal = TestObjectCreator.CreateJournal<ARJournal>(100m, date, TestObjectCreator.Debtor.PK);
			var arTransfer = TestObjectCreator.CreateTransfer<ARTransfer>(100m, date, TestObjectCreator.Debtor.PK, TestObjectCreator.Creditor1.PK);
			var arReceipt = TestObjectCreator.CreateARReceipt(1m, 100m, date, date, TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);
			var arPayment = TestObjectCreator.CreateARPayment(1m, 100m, date, date, TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);
			var arDiscount = TestObjectCreator.CreateARDiscount(100, date, TestObjectCreator.Debtor.PK);
			var arOverpayment = TestObjectCreator.CreateOverpayment<AROverpayment>(100m, date, TestObjectCreator.Debtor.PK);
			var arExchangeDifference = TestObjectCreator.CreateExchangeDifference<ARExchangeDifference>(100m, date, TestObjectCreator.Debtor.PK);

			var apJournal = TestObjectCreator.CreateJournal<APJournal>(100m, date, TestObjectCreator.Creditor1.PK);
			var apTransfer = TestObjectCreator.CreateTransfer<APTransfer>(100m, date, TestObjectCreator.Creditor1.PK, TestObjectCreator.Debtor.PK);
			var apReceipt = TestObjectCreator.CreateAPReceipt(1m, 100m, date, date, TestObjectCreator.Creditor1.PK, TestObjectCreator.AUDBankAccount.PK);
			var apPayment = TestObjectCreator.CreateAPPayment(1m, 100m, date, date, TestObjectCreator.Creditor1.PK, TestObjectCreator.AUDBankAccount.PK);
			var apDiscount = TestObjectCreator.CreateAPDiscount(100, date, TestObjectCreator.Creditor1.PK);
			var apOverpayment = TestObjectCreator.CreateOverpayment<APOverpayment>(100m, date, TestObjectCreator.Creditor1.PK);
			var apExchangeDifference = TestObjectCreator.CreateExchangeDifference<APExchangeDifference>(100m, date, TestObjectCreator.Creditor1.PK);

			var contra = TestObjectCreator.CreateContra(100m, date, TestObjectCreator.Debtor.PK, TestObjectCreator.Creditor1.PK);

			var bankTransfer = TestObjectCreator.CreateBankTransfer(date, TestObjectCreator.CHNBankAccount.PK, TestObjectCreator.AUDBankAccount.PK, 100m, 2m);
			TestObjectCreator.AUDBankAccount.Factory.Save();
			var cbExchangeDifference = TestObjectCreator.CreateCashbookExchangeDifference(date, 100m, TestObjectCreator.AUDBankAccount, false);

			Factory.Save();

			var expected = new BusinessObjectFactory().Load<AccTransactionHeader>(new ZQuery()).Select(x => x.PK.ToString());
			AssertEquals(21, expected.Count());

			AssertQueueTransaction(new[] { (date, date) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (date, date.AddDays(1)) }
				, expectedPost: expected
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (date, DateTime.MinValue) }
				, expectedPost: expected
				, expectedReverse: Array.Empty<string>());
		}

		[TestDate(2023, 10, 20)]
		public void TestQueueTransaction_JobCostingTransactionLines()
		{
			var date20230201 = new DateTime(2023, 02, 01);
			var date20221230 = new DateTime(2022, 12, 30);

			var jcJournal20230201 = TestObjectCreator.CreateJCJournalHeader(date20230201, 100m);
			jcJournal20230201.AH_TransactionType = TransactionTypes.Journal;
			var jcJournalLine20230201 = TestObjectCreator.CreateJCJournalLine(jcJournal20230201, TestObjectCreator.CC1, null, date20230201, 100m);

			var job20230201 = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0m, TestObjectCreator.Agent, 1.0m);
			var jobJournal20230201 = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job20230201, 250m);
			jobJournal20230201.AH_PostDate = date20230201;

			var accrual20230201 = TestObjectCreator.CreateAccrual();
			accrual20230201.AL_PostDate = date20230201;
			var wip20230201 = TestObjectCreator.CreateWIP();
			wip20230201.AL_PostDate = date20230201;

			var jcJournal20221230 = TestObjectCreator.CreateJCJournalHeader(date20221230, 100m);
			jcJournal20221230.AH_TransactionType = TransactionTypes.Journal;
			var jcJournalLine20221230 = TestObjectCreator.CreateJCJournalLine(jcJournal20221230, TestObjectCreator.CC1, null, date20221230, 100m);

			var job20221230 = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0m, TestObjectCreator.Agent, 1.0m);
			var jobJournal20221230 = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job20221230, 250m);
			jobJournal20221230.AH_PostDate = date20221230;

			var accrual20221230 = TestObjectCreator.CreateAccrual();
			accrual20221230.AL_PostDate = date20221230;
			var wip20221230 = TestObjectCreator.CreateWIP();
			wip20221230.AL_PostDate = date20221230;

			Factory.Save();

			var expected20230201 = new[] { jcJournalLine20230201.PK.ToString()
				, jobJournal20230201.Lines[0].PK.ToString()
				, jobJournal20230201.Lines[1].PK.ToString()
				, accrual20230201.PK.ToString()
				, wip20230201.PK.ToString()
			};
			var expected20230201_Reverse = new[] { jcJournalLine20230201.PK.ToString()
				, jobJournal20230201.Lines[0].PK.ToString()
				, jobJournal20230201.Lines[1].PK.ToString()
			};

			var expected20221230 = new[] { jcJournalLine20221230.PK.ToString()
				, jobJournal20221230.Lines[0].PK.ToString()
				, jobJournal20221230.Lines[1].PK.ToString()
				, accrual20221230.PK.ToString()
				, wip20221230.PK.ToString()
			};
			var expected20221230_Reverse = new[] {  jcJournalLine20221230.PK.ToString()
				, jobJournal20221230.Lines[0].PK.ToString()
				, jobJournal20221230.Lines[1].PK.ToString()
			};

			AssertQueueTransaction(new[] { (date20230201, date20230201) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (date20230201, date20230201.AddDays(1)) }
				, expectedPost: expected20230201
				, expectedReverse: expected20230201_Reverse);

			AssertQueueTransaction(new[] { (date20221230, date20221230.AddDays(1)) }
				, expectedPost: expected20221230
				, expectedReverse: expected20221230_Reverse);

			AssertQueueTransaction(new[] { (date20230201, date20230201.AddDays(1)), (date20221230, date20221230.AddDays(1)) }
				, expectedPost: expected20221230.Concat(expected20230201)
				, expectedReverse: expected20221230_Reverse.Concat(expected20230201_Reverse));

			AssertQueueTransaction(new[] { (date20221230, date20230201.AddDays(1)) }
				, expectedPost: expected20221230.Concat(expected20230201)
				, expectedReverse: expected20221230_Reverse.Concat(expected20230201_Reverse));

			AssertQueueTransaction(new[] { (date20221230, DateTime.MinValue) }
				, expectedPost: expected20221230.Concat(expected20230201)
				, expectedReverse: expected20221230_Reverse.Concat(expected20230201_Reverse));
		}

		[TestDate(2022, 2, 1)]
		[SuspendCriticalValidation]
		public void TestQueueTransaction_CashBasisVATTransaction()
		{
			AssertEquals("PreCondition", new ZDateTime(2022, 02, 01), ZDateTime.Today);
			var today20220201 = ZDateTime.Today.ToDateTime();
			var fullyPaidDate20220720 = new DateTime(2022, 07, 20);

			var apInvoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1, 100, 10, 100, 10);
			apInvoice.AH_FullyPaidDate = fullyPaidDate20220720;
			var line = apInvoice.Lines[0];
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;

			var cashVATRecord = TestObjectCreator.CreateCashBasisVAT(line, -90, -9);
			var cashVATRecord2 = TestObjectCreator.CreateCashBasisVAT(line, -90, -9);

			Factory.Save();

			var expectedLines = new[] { line.PK.ToString() };
			var expectedVats = new[] { cashVATRecord.PK.ToString(), cashVATRecord2.PK.ToString() };

			AssertQueueTransaction(new[] { (today20220201, today20220201) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (today20220201, today20220201.AddDays(1)) }
				, expectedPost: expectedLines
				, expectedReverse: expectedLines);

			AssertQueueTransaction(new[] { (fullyPaidDate20220720, fullyPaidDate20220720) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (fullyPaidDate20220720, fullyPaidDate20220720.AddDays(1)) }
				, expectedPost: expectedVats
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (today20220201, today20220201.AddDays(1)), (fullyPaidDate20220720, fullyPaidDate20220720.AddDays(1)) }
				, expectedPost: expectedLines.Concat(expectedVats)
				, expectedReverse: expectedLines);

			AssertQueueTransaction(new[] { (today20220201, fullyPaidDate20220720.AddDays(1)) }
				, expectedPost: expectedLines.Concat(expectedVats)
				, expectedReverse: expectedLines);

			AssertQueueTransaction(new[] { (today20220201, DateTime.MinValue) }
				, expectedPost: expectedLines.Concat(expectedVats)
				, expectedReverse: expectedLines);
		}

		[TestDate(2023, 10, 20)]
		public void TestQueueTransaction_GenerateJournalEntry()
		{
			var postDate20230102 = new DateTime(2023, 01, 02);
			var postDate20230120 = new DateTime(2023, 01, 20);
			var arJournal20230102 = TestObjectCreator.CreateJournal<ARJournal>(100m, postDate20230102, TestObjectCreator.Debtor.PK);
			var arJournal20230120 = TestObjectCreator.CreateJournal<ARJournal>(100m, postDate20230120, TestObjectCreator.Debtor.PK);
			Factory.Save();

			var expected20230102 = new[] { arJournal20230102.PK.ToString() };
			var expected20230120 = new[] { arJournal20230120.PK.ToString() };

			AssertQueueTransaction(new[] { (postDate20230102, postDate20230102) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (postDate20230102, postDate20230102.AddDays(1)) }
				, expectedPost: expected20230102
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (postDate20230120, postDate20230120) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (postDate20230120, postDate20230120.AddDays(1)) }
				, expectedPost: expected20230120
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (postDate20230102, postDate20230102.AddDays(1)), (postDate20230120, postDate20230120.AddDays(1)) }
				, expectedPost: expected20230120.Concat(expected20230102)
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (postDate20230102, postDate20230120.AddDays(1)) }
				, expectedPost: expected20230120.Concat(expected20230102)
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (postDate20230102, DateTime.MinValue) }
				, expectedPost: expected20230120.Concat(expected20230102)
				, expectedReverse: Array.Empty<string>());
		}

		[TestDate(2023, 10, 20)]
		public void TestQueueTransaction_QueueGLDDataForGlMovementDetails()
		{
			var postDate20220102 = new DateTime(2022, 1, 2);
			var postDate20230102 = new DateTime(2023, 1, 2);

			var invoice20220102 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARNV1", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
			invoice20220102.AH_PostDate = postDate20220102;
			invoice20220102.Lines[0].AL_AG = TestObjectCreator.GLHeader2.PK;
			invoice20220102.Lines[0].AL_PostDate = postDate20220102;
			var taxRecord20220102 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord20220102.ATT_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			taxRecord20220102.ATT_PostDate = new ZDateTime(postDate20220102).Date;
			taxRecord20220102.ATT_AG_LedgerControlAccount = TestObjectCreator.GLHeader1.PK;
			taxRecord20220102.ATT_AG_TaxExpenseAccount = TestObjectCreator.GLHeader2.PK;
			taxRecord20220102.ATT_AH = invoice20220102.PK;
			taxRecord20220102.ATT_Basis = TaxBasisList.Posting.Code;

			var pivots20220102 = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord20220102.PK) { FetchOnlyFromLocalCache = true });
			pivots20220102[0].ATP_IsTaxExpense = true;

			var invoice20230102 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARNV2", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
			invoice20230102.AH_PostDate = postDate20230102;
			invoice20230102.Lines[0].AL_AG = TestObjectCreator.GLHeader2.PK;
			invoice20230102.Lines[0].AL_PostDate = postDate20230102;
			var taxRecord20230102 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord20230102.ATT_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			taxRecord20230102.ATT_PostDate = new ZDateTime(postDate20230102).Date;
			taxRecord20230102.ATT_AG_LedgerControlAccount = TestObjectCreator.GLHeader1.PK;
			taxRecord20230102.ATT_AG_TaxExpenseAccount = TestObjectCreator.GLHeader2.PK;
			taxRecord20230102.ATT_AH = invoice20230102.PK;
			taxRecord20230102.ATT_Basis = TaxBasisList.Posting.Code;

			var pivots20230102 = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord20230102.PK) { FetchOnlyFromLocalCache = true });
			pivots20230102[0].ATP_IsTaxExpense = true;

			Factory.Save();
			var glMovement20220102 = Factory.LoadTop1<AccTaxGLMovement>(new ZQuery(AccTaxGLMovementSchema.ATM_ATT_TaxTransaction, taxRecord20220102.PK) { FetchOnlyFromLocalCache = true });
			var glMovement20230102 = Factory.LoadTop1<AccTaxGLMovement>(new ZQuery(AccTaxGLMovementSchema.ATM_ATT_TaxTransaction, taxRecord20230102.PK) { FetchOnlyFromLocalCache = true });

			AssertQueueTransaction(new[] { (postDate20220102, postDate20220102.AddDays(1)) }
				, expectedPost: new[] { glMovement20220102.PK.ToString(), invoice20220102.Lines[0].PK.ToString() }
				, expectedReverse: new[] { invoice20220102.Lines[0].PK.ToString() });

			AssertQueueTransaction(new[] { (postDate20230102, postDate20230102.AddDays(1)) }
				, expectedPost: new[] { glMovement20230102.PK.ToString(), invoice20230102.Lines[0].PK.ToString() }
				, expectedReverse: new[] { invoice20230102.Lines[0].PK.ToString() });

			AssertQueueTransaction(new[] { (postDate20220102, postDate20220102.AddDays(1)), (postDate20230102, postDate20230102.AddDays(1)) }
				, expectedPost: new[] { glMovement20220102.PK.ToString(), glMovement20230102.PK.ToString(), invoice20220102.Lines[0].PK.ToString(), invoice20230102.Lines[0].PK.ToString() }
				, expectedReverse: new[] { invoice20220102.Lines[0].PK.ToString(), invoice20230102.Lines[0].PK.ToString() });

			AssertQueueTransaction(new[] { (postDate20220102, postDate20230102.AddDays(1)) }
				, expectedPost: new[] { glMovement20220102.PK.ToString(), glMovement20230102.PK.ToString(), invoice20220102.Lines[0].PK.ToString(), invoice20230102.Lines[0].PK.ToString() }
				, expectedReverse: new[] { invoice20220102.Lines[0].PK.ToString(), invoice20230102.Lines[0].PK.ToString() });

			AssertQueueTransaction(new[] { (postDate20220102, DateTime.MinValue) }
				, expectedPost: new[] { glMovement20220102.PK.ToString(), glMovement20230102.PK.ToString(), invoice20220102.Lines[0].PK.ToString(), invoice20230102.Lines[0].PK.ToString() }
				, expectedReverse: new[] { invoice20220102.Lines[0].PK.ToString(), invoice20230102.Lines[0].PK.ToString() });
		}

		[TestDate(2023, 10, 20)]
		public void TestQueueTransaction_QueueGLDDataForMultipleCompanies()
		{
			var postDate20230102 = new DateTime(2023, 01, 02);
			TestObjectCreator.CreateTestPeriodsForEntireYear(TestObjectCreator.NonCurrentCompanyBranch.Company, 2023);
			Factory.Save();

			var arJournalCurrentCompany = TestObjectCreator.CreateJournal<ARJournal>(100m, postDate20230102, TestObjectCreator.Debtor.PK);
			Factory.Save();

			ARJournal arJournalAnotherCompany;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var newFactory = new BusinessObjectFactory();
				arJournalAnotherCompany = new TestObjectCreator(newFactory).CreateJournal<ARJournal>(100m, postDate20230102, TestObjectCreator.Debtor.PK);
				newFactory.Save();
			}

			AssertQueueTransaction(new[] { (GlbCompany.CurrentCompany, postDate20230102, postDate20230102.AddDays(1)) }
				, expectedPost: new[] { arJournalCurrentCompany.PK.ToString() }
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (TestObjectCreator.NonCurrentCompanyBranch.Company, postDate20230102, postDate20230102.AddDays(1)) }
				, expectedPost: new[] { arJournalAnotherCompany.PK.ToString() }
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] {
					(GlbCompany.CurrentCompany, postDate20230102, postDate20230102.AddDays(1))
					, (TestObjectCreator.NonCurrentCompanyBranch.Company, postDate20230102, postDate20230102.AddDays(1))
				}
				, expectedPost: new[] { arJournalCurrentCompany.PK.ToString(), arJournalAnotherCompany.PK.ToString() }
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] {
					(GlbCompany.CurrentCompany, postDate20230102, DateTime.MinValue)
					, (TestObjectCreator.NonCurrentCompanyBranch.Company, postDate20230102, DateTime.MinValue)
				}
				, expectedPost: new[] { arJournalCurrentCompany.PK.ToString(), arJournalAnotherCompany.PK.ToString() }
				, expectedReverse: Array.Empty<string>());
		}

		[TestDate(2023, 2, 1)]
		public void TestQueueTransaction_TransactionLines()
		{
			AssertEquals("PreCondition", new DateTime(2023, 2, 1), ZDateTime.Today);
			var today20230201 = ZDateTime.Today.ToDateTime();
			var postDate20221001 = new DateTime(2022, 10, 1);

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			var arCreditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "CRD1", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			var arAdjustmentNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARAdjustmentNote), "ADJ1", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);

			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			var apCreditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "CRD2", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			var apAdjustmentNote = TestObjectCreator.CreateInvoiceWithLine(typeof(APAdjustmentNote), "ADJ2", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);

			var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV3", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			arInvoice1.AH_PostDate = postDate20221001;
			arInvoice1.Lines[0].AL_PostDate = postDate20221001;
			var arCreditNote1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "CRD3", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			arCreditNote1.AH_PostDate = postDate20221001;
			arCreditNote1.Lines[0].AL_PostDate = postDate20221001;
			var arAdjustmentNote1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARAdjustmentNote), "ADJ3", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			arAdjustmentNote1.AH_PostDate = postDate20221001;
			arAdjustmentNote1.Lines[0].AL_PostDate = postDate20221001;

			var apInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV4", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			apInvoice1.AH_PostDate = postDate20221001;
			apInvoice1.Lines[0].AL_PostDate = postDate20221001;
			var apCreditNote1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "CRD4", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			apCreditNote1.AH_PostDate = postDate20221001;
			apCreditNote1.Lines[0].AL_PostDate = postDate20221001;
			var apAdjustmentNote1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APAdjustmentNote), "ADJ4", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			apAdjustmentNote1.AH_PostDate = postDate20221001;
			apAdjustmentNote1.Lines[0].AL_PostDate = postDate20221001;

			Factory.Save();

			var expected20230201 = new[] {
				arInvoice.Lines[0].PK.ToString()
				, arCreditNote.Lines[0].PK.ToString()
				, arAdjustmentNote.Lines[0].PK.ToString()
				, apInvoice.Lines[0].PK.ToString()
				, apCreditNote.Lines[0].PK.ToString()
				, apAdjustmentNote.Lines[0].PK.ToString()
			};

			var expected20221001 = new[] {
				arInvoice1.Lines[0].PK.ToString()
				, arCreditNote1.Lines[0].PK.ToString()
				, arAdjustmentNote1.Lines[0].PK.ToString()
				, apInvoice1.Lines[0].PK.ToString()
				, apCreditNote1.Lines[0].PK.ToString()
				, apAdjustmentNote1.Lines[0].PK.ToString()
			};

			AssertQueueTransaction(new[] { (today20230201, today20230201.AddDays(1)) }
				, expectedPost: expected20230201
				, expectedReverse: expected20230201);

			AssertQueueTransaction(new[] { (postDate20221001, postDate20221001.AddDays(1)) }
				, expectedPost: expected20221001
				, expectedReverse: expected20221001);

			AssertQueueTransaction(new[] { (today20230201, today20230201.AddDays(1)), (postDate20221001, postDate20221001.AddDays(1)) }
				, expectedPost: expected20230201.Concat(expected20221001)
				, expectedReverse: expected20230201.Concat(expected20221001));

			AssertQueueTransaction(new[] { (postDate20221001, today20230201.AddDays(1)) }
				, expectedPost: expected20230201.Concat(expected20221001)
				, expectedReverse: expected20230201.Concat(expected20221001));

			AssertQueueTransaction(new[] { (postDate20221001, DateTime.MinValue) }
				, expectedPost: expected20230201.Concat(expected20221001)
				, expectedReverse: expected20230201.Concat(expected20221001));
		}

		[TestDate(2023, 10, 20)]
		public void TestQueueTransaction_DRCAndDPYTransactionLines()
		{
			var postDate20230201 = new DateTime(2023, 02, 01);

			var directReceipt = TestObjectCreator.CreateDirectReceipt(postDate20230201, 100, 10, 50, 5);

			var directPayment = TestObjectCreator.CreateDirectPayment(postDate20230201, 100, 10, 50, 5);

			Factory.Save();

			AssertQueueTransaction(new[] { (postDate20230201, postDate20230201) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: Array.Empty<string>());

			var expected = new BusinessObjectFactory().Load<AccTransactionLines>(new ZQuery()).Select(x => x.PK.ToString());
			AssertEquals(4, expected.Count());

			AssertQueueTransaction(new[] { (postDate20230201, postDate20230201.AddDays(1)) }
				, expectedPost: expected
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (postDate20230201, DateTime.MinValue) }
				, expectedPost: expected
				, expectedReverse: Array.Empty<string>());
		}

		[TestDate(2023, 10, 20)]
		public void TestQueueTransaction_DRCTransactionLinesPartMoveToQueueTable()
		{
			var postDate20230102 = new DateTime(2023, 01, 02);
			var postDate20230120 = new DateTime(2023, 01, 20);
			var directReceipt20230102 = TestObjectCreator.CreateDirectReceipt(postDate20230102, 100, 10, 50, 5);
			var directReceipt20230120 = TestObjectCreator.CreateDirectReceipt(postDate20230120, 100, 10, 50, 5);

			Factory.Save();

			var expect20230102 = directReceipt20230102.Lines.Select(x => x.PK.ToString());
			var expect20230120 = directReceipt20230120.Lines.Select(x => x.PK.ToString());

			AssertQueueTransaction(new[] { (postDate20230102, postDate20230102.AddDays(1)) }
				, expectedPost: expect20230102
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (postDate20230120, postDate20230120.AddDays(1)) }
				, expectedPost: expect20230120
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (postDate20230102, postDate20230102.AddDays(1)), (postDate20230120, postDate20230120.AddDays(1)) }
				, expectedPost: expect20230102.Concat(expect20230120)
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (postDate20230102, postDate20230120.AddDays(1)) }
				, expectedPost: expect20230102.Concat(expect20230120)
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (postDate20230102, DateTime.MinValue) }
				, expectedPost: expect20230102.Concat(expect20230120)
				, expectedReverse: Array.Empty<string>());
		}

		[TestDate(2023, 03, 01)]
		public void TestQueueTransaction_GJLNJLTransactionLinesPartMoveToQueueTable()
		{
			AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AssertEquals("PreCondition", new DateTime(2023, 03, 01), ZDateTime.Today);
			var today20230301 = ZDateTime.Today.ToDateTime();
			var dueDate20230405 = new DateTime(2023, 04, 05);
			var dueDate20210505 = new DateTime(2021, 05, 05);
			var dueDate20220505 = new DateTime(2022, 05, 05);
			var dueDate20230505 = new DateTime(2023, 05, 05);

			var glStdJournal20230405 = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, dueDate20230405);
			AddGLLines(glStdJournal20230405);

			var glStdJournal20210505 = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, dueDate20210505);
			AddGLLines(glStdJournal20210505);

			var glNoteJournal20220505 = TestObjectCreator.CreateGLJournal(TransactionTypes.GLNoteJournal, ZDateTime.Today, dueDate20220505);
			AddGLLines(glNoteJournal20220505);

			var glNoteJournal20230505 = TestObjectCreator.CreateGLJournal(TransactionTypes.GLNoteJournal, ZDateTime.Today, dueDate20230505);
			AddGLLines(glNoteJournal20230505);

			Factory.Save();

			var expected20230405 = glStdJournal20230405.Lines.Select(x => x.PK.ToString());
			var expected20210505 = glStdJournal20210505.Lines.Select(x => x.PK.ToString());
			var expected20220505 = glNoteJournal20220505.Lines.Select(x => x.PK.ToString());
			var expected20230505 = glNoteJournal20230505.Lines.Select(x => x.PK.ToString());

			AssertQueueTransaction(new[] { (today20230301, today20230301.AddDays(1)) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (dueDate20230405, dueDate20230405.AddDays(1)) }
				, expectedPost: expected20230405
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (dueDate20210505, dueDate20210505.AddDays(1)), (dueDate20230405, dueDate20230405.AddDays(1)) }
				, expectedPost: expected20230405.Concat(expected20210505)
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (dueDate20210505, dueDate20210505.AddDays(1)), (dueDate20220505, dueDate20220505.AddDays(1)), (dueDate20230405, dueDate20230405.AddDays(1)) }
				, expectedPost: expected20230405.Concat(expected20210505).Concat(expected20220505)
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] {
					(dueDate20210505, dueDate20210505.AddDays(1)), (dueDate20220505, dueDate20220505.AddDays(1))
					, (dueDate20230405, dueDate20230405.AddDays(1)), (dueDate20230505, dueDate20230505.AddDays(1)) }
				, expectedPost: expected20230405.Concat(expected20210505).Concat(expected20220505).Concat(expected20230505)
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] {
					(dueDate20210505, dueDate20230505.AddDays(1)) }
				, expectedPost: expected20230405.Concat(expected20210505).Concat(expected20220505).Concat(expected20230505)
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] {
					(dueDate20210505, DateTime.MinValue) }
				, expectedPost: expected20230405.Concat(expected20210505).Concat(expected20220505).Concat(expected20230505)
				, expectedReverse: Array.Empty<string>());

			void AddGLLines(GLJournal gLJournal)
			{
				TestObjectCreator.CreateGLJournalLine(gLJournal, 20m, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
				TestObjectCreator.CreateGLJournalLine(gLJournal, 20m, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			}
		}

		[TestDate(2023, 10, 20)]
		public void TestQueueTransaction_RJLAJLTransactionLinesPartMoveToQueueTable()
		{
			AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var postDate20220201 = new DateTime(2022, 02, 01);
			var dueDate20230509 = new DateTime(2023, 05, 09);
			var dueDate20230101 = new DateTime(2023, 01, 01);
			var dueDate20220301 = new DateTime(2022, 03, 01);
			var autoJournalPostDate20220228 = new DateTime(2022, 02, 28, 23, 59, 00);
			var autoJournalDueDate20230228 = new DateTime(2023, 02, 28, 23, 59, 00);

			var glReversingJournal20230509 = TestObjectCreator.CreateGLJournal(TransactionTypes.GLReversingJournal, postDate20220201, postDate20220201, dueDate20230509);
			AddGLLines(glReversingJournal20230509);

			var glReversingJournal20230101 = TestObjectCreator.CreateGLJournal(TransactionTypes.GLReversingJournal, postDate20220201, postDate20220201, dueDate20230101);
			AddGLLines(glReversingJournal20230101);

			var glReversingJournal20220301 = TestObjectCreator.CreateGLJournal(TransactionTypes.GLReversingJournal, postDate20220201, postDate20220201, dueDate20220301);
			AddGLLines(glReversingJournal20220301);

			var glAutoJournal20230228 = TestObjectCreator.CreateGLJournal(TransactionTypes.GLAutoJournal, new DateTime(2022, 02, 01), new DateTime(2022, 02, 01), new DateTime(2023, 02, 01));
			AddGLLines(glAutoJournal20230228);

			Factory.Save();
			CombineAssertions("PreCondition", () =>
			{
				AssertEquals(postDate20220201, glReversingJournal20230509.AH_PostDate);
				AssertEquals(postDate20220201, glReversingJournal20230101.AH_PostDate);
				AssertEquals(postDate20220201, glReversingJournal20220301.AH_PostDate);

				AssertEquals(dueDate20230509, glReversingJournal20230509.AH_DueDate);
				AssertEquals(dueDate20230101, glReversingJournal20230101.AH_DueDate);
				AssertEquals(dueDate20220301, glReversingJournal20220301.AH_DueDate);

				AssertEquals(autoJournalPostDate20220228, glAutoJournal20230228.AH_PostDate);
				AssertEquals(autoJournalDueDate20230228, glAutoJournal20230228.AH_DueDate);
			});

			var expectedPost20220201 = glReversingJournal20230509.Lines
				.Concat(glReversingJournal20230101.Lines)
				.Concat(glReversingJournal20220301.Lines)
				.Select(x => x.PK.ToString());
			var expectedPost20220228 = glAutoJournal20230228.Lines.Select(x => x.PK.ToString());
			var expectedDue20220301 = glReversingJournal20220301.Lines.Select(x => x.PK.ToString());
			var expectedDue20230101 = glReversingJournal20230101.Lines.Select(x => x.PK.ToString());
			var expectedDue20230228 = glAutoJournal20230228.Lines.Select(x => x.PK.ToString());
			var expectedDue20230509 = glReversingJournal20230509.Lines.Select(x => x.PK.ToString());

			AssertQueueTransaction(new[] { (postDate20220201, postDate20220201.AddDays(1)) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (autoJournalPostDate20220228.Date, autoJournalPostDate20220228.Date.AddDays(1)) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (dueDate20230509, dueDate20230509.AddDays(1)) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: expectedDue20230509);

			AssertQueueTransaction(new[] { (dueDate20230101, dueDate20230101.AddDays(1)), (dueDate20230509, dueDate20230509.AddDays(1)) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: expectedDue20230101.Concat(expectedDue20230509));

			AssertQueueTransaction(new[] { (dueDate20220301, dueDate20220301.AddDays(1)), (dueDate20230101, dueDate20230101.AddDays(1)), (dueDate20230509, dueDate20230509.AddDays(1)) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: expectedDue20220301.Concat(expectedDue20230101).Concat(expectedDue20230509));

			AssertQueueTransaction(new[] { (dueDate20220301, dueDate20220301.AddDays(1)), (dueDate20230101, dueDate20230101.AddDays(1)), (autoJournalDueDate20230228.Date, autoJournalDueDate20230228.Date.AddDays(1)), (dueDate20230509, dueDate20230509.AddDays(1)) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: expectedDue20220301.Concat(expectedDue20230101).Concat(expectedDue20230228).Concat(expectedDue20230509));

			AssertQueueTransaction(new[] { (dueDate20220301, dueDate20230509.AddDays(1)) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: expectedDue20220301.Concat(expectedDue20230101).Concat(expectedDue20230228).Concat(expectedDue20230509));

			AssertQueueTransaction(new[] { (dueDate20220301, DateTime.MinValue) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: expectedDue20220301.Concat(expectedDue20230101).Concat(expectedDue20230228).Concat(expectedDue20230509));

			void AddGLLines(GLJournal gLJournal)
			{
				TestObjectCreator.CreateGLJournalLine(gLJournal, 20m, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
				TestObjectCreator.CreateGLJournalLine(gLJournal, 20m, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			}
		}

		[TestDate(2023, 2, 1)]
		public void TestQueueTransaction_TransactionLines_ReverseDate()
		{
			var today20230201 = new DateTime(2023, 02, 01);
			var backPostDate20221001 = new DateTime(2022, 10, 1);

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			var arCreditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "CRD1", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			var arAdjustmentNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARAdjustmentNote), "ADJ1", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);

			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			var apCreditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "CRD2", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			var apAdjustmentNote = TestObjectCreator.CreateInvoiceWithLine(typeof(APAdjustmentNote), "ADJ2", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);

			var arInvoiceWithReverseDate = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV3", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			arInvoiceWithReverseDate.AH_PostDate = backPostDate20221001;
			var arCreditNoteWithReverseDate = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "CRD3", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			arCreditNoteWithReverseDate.AH_PostDate = backPostDate20221001;
			var arAdjustmentNoteWithReverseDate = TestObjectCreator.CreateInvoiceWithLine(typeof(ARAdjustmentNote), "ADJ3", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			arAdjustmentNoteWithReverseDate.AH_PostDate = backPostDate20221001;

			var apInvoiceWithReverseDate = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV4", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			apInvoiceWithReverseDate.AH_PostDate = backPostDate20221001;
			var apCreditNoteWithReverseDate = TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "CRD4", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			apCreditNoteWithReverseDate.AH_PostDate = backPostDate20221001;
			var apAdjustmentNoteWithReverseDate = TestObjectCreator.CreateInvoiceWithLine(typeof(APAdjustmentNote), "ADJ4", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			apAdjustmentNoteWithReverseDate.AH_PostDate = backPostDate20221001;

			var arInvoiceWithoutReverseDate = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV5", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			arInvoiceWithoutReverseDate.AH_PostDate = ZDateTime.Empty;
			var arCreditNoteWithoutReverseDate = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "CRD5", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			arCreditNoteWithoutReverseDate.AH_PostDate = ZDateTime.Empty;
			var arAdjustmentNoteWithoutReverseDate = TestObjectCreator.CreateInvoiceWithLine(typeof(ARAdjustmentNote), "ADJ5", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			arAdjustmentNoteWithoutReverseDate.AH_PostDate = ZDateTime.Empty;

			var apInvoiceWithoutReverseDate = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV6", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			apInvoiceWithoutReverseDate.AH_PostDate = ZDateTime.Empty;
			var apCreditNoteWithoutReverseDate = TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "CRD6", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			apCreditNoteWithoutReverseDate.AH_PostDate = ZDateTime.Empty;
			var apAdjustmentNoteWithoutReverseDate = TestObjectCreator.CreateInvoiceWithLine(typeof(APAdjustmentNote), "ADJ6", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			apAdjustmentNoteWithoutReverseDate.AH_PostDate = ZDateTime.Empty;

			Factory.Save();

			AssertEquals(today20230201, arInvoice.Lines[0].AL_ReverseDate);
			AssertEquals(today20230201, arCreditNote.Lines[0].AL_ReverseDate);
			AssertEquals(today20230201, arAdjustmentNote.Lines[0].AL_ReverseDate);
			AssertEquals(today20230201, apInvoice.Lines[0].AL_ReverseDate);
			AssertEquals(today20230201, apCreditNote.Lines[0].AL_ReverseDate);
			AssertEquals(today20230201, apAdjustmentNote.Lines[0].AL_ReverseDate);
			AssertEquals(backPostDate20221001, arInvoiceWithReverseDate.Lines[0].AL_ReverseDate);
			AssertEquals(backPostDate20221001, arCreditNoteWithReverseDate.Lines[0].AL_ReverseDate);
			AssertEquals(backPostDate20221001, arAdjustmentNoteWithReverseDate.Lines[0].AL_ReverseDate);
			AssertEquals(backPostDate20221001, apInvoiceWithReverseDate.Lines[0].AL_ReverseDate);
			AssertEquals(backPostDate20221001, apCreditNoteWithReverseDate.Lines[0].AL_ReverseDate);
			AssertEquals(backPostDate20221001, apAdjustmentNoteWithReverseDate.Lines[0].AL_ReverseDate);
			AssertEquals(ZDateTime.Empty, arInvoiceWithoutReverseDate.Lines[0].AL_ReverseDate);
			AssertEquals(ZDateTime.Empty, arCreditNoteWithoutReverseDate.Lines[0].AL_ReverseDate);
			AssertEquals(ZDateTime.Empty, arAdjustmentNoteWithoutReverseDate.Lines[0].AL_ReverseDate);
			AssertEquals(ZDateTime.Empty, apInvoiceWithoutReverseDate.Lines[0].AL_ReverseDate);
			AssertEquals(ZDateTime.Empty, apCreditNoteWithoutReverseDate.Lines[0].AL_ReverseDate);
			AssertEquals(ZDateTime.Empty, apAdjustmentNoteWithoutReverseDate.Lines[0].AL_ReverseDate);
			AssertEquals(today20230201, arInvoiceWithoutReverseDate.Lines[0].AL_PostDate);
			AssertEquals(today20230201, arCreditNoteWithoutReverseDate.Lines[0].AL_PostDate);
			AssertEquals(today20230201, arAdjustmentNoteWithoutReverseDate.Lines[0].AL_PostDate);
			AssertEquals(today20230201, apInvoiceWithoutReverseDate.Lines[0].AL_PostDate);
			AssertEquals(today20230201, apCreditNoteWithoutReverseDate.Lines[0].AL_PostDate);
			AssertEquals(today20230201, apAdjustmentNoteWithoutReverseDate.Lines[0].AL_PostDate);

			var expectedReverse20230201 = new[] {
				arInvoice.Lines[0].PK.ToString()
				, arCreditNote.Lines[0].PK.ToString()
				, arAdjustmentNote.Lines[0].PK.ToString()
				, apInvoice.Lines[0].PK.ToString()
				, apCreditNote.Lines[0].PK.ToString()
				, apAdjustmentNote.Lines[0].PK.ToString()
			};

			var expectedPost20230201 = new[] {
				arInvoice.Lines[0].PK.ToString()
				, arCreditNote.Lines[0].PK.ToString()
				, arAdjustmentNote.Lines[0].PK.ToString()
				, apInvoice.Lines[0].PK.ToString()
				, apCreditNote.Lines[0].PK.ToString()
				, apAdjustmentNote.Lines[0].PK.ToString()
				, arInvoiceWithoutReverseDate.Lines[0].PK.ToString()
				, arCreditNoteWithoutReverseDate.Lines[0].PK.ToString()
				, arAdjustmentNoteWithoutReverseDate.Lines[0].PK.ToString()
				, apInvoiceWithoutReverseDate.Lines[0].PK.ToString()
				, apCreditNoteWithoutReverseDate.Lines[0].PK.ToString()
				, apAdjustmentNoteWithoutReverseDate.Lines[0].PK.ToString()
			};

			var expected20221001 = new[] {
				arInvoiceWithReverseDate.Lines[0].PK.ToString()
				, arCreditNoteWithReverseDate.Lines[0].PK.ToString()
				, arAdjustmentNoteWithReverseDate.Lines[0].PK.ToString()
				, apInvoiceWithReverseDate.Lines[0].PK.ToString()
				, apCreditNoteWithReverseDate.Lines[0].PK.ToString()
				, apAdjustmentNoteWithReverseDate.Lines[0].PK.ToString()
			};

			AssertQueueTransaction(new[] { (today20230201, today20230201.AddDays(1)) }
				, expectedPost: expectedPost20230201
				, expectedReverse: expectedReverse20230201);

			AssertQueueTransaction(new[] { (backPostDate20221001, backPostDate20221001.AddDays(1)) }
				, expectedPost: expected20221001
				, expectedReverse: expected20221001);

			AssertQueueTransaction(new[] { (backPostDate20221001, backPostDate20221001.AddDays(1)), (today20230201, today20230201.AddDays(1)) }
				, expectedPost: expected20221001.Concat(expectedPost20230201)
				, expectedReverse: expected20221001.Concat(expectedReverse20230201));

			AssertQueueTransaction(new[] { (backPostDate20221001, today20230201.AddDays(1)) }
				, expectedPost: expected20221001.Concat(expectedPost20230201)
				, expectedReverse: expected20221001.Concat(expectedReverse20230201));

			AssertQueueTransaction(new[] { (backPostDate20221001, DateTime.MinValue) }
				, expectedPost: expected20221001.Concat(expectedPost20230201)
				, expectedReverse: expected20221001.Concat(expectedReverse20230201));
		}

		[TestDate(2023, 10, 20)]
		public void TestQueueTransaction_JobCostingTransactionLinesForReverseDate_WipAccrual()
		{
			var postDate20201030 = new DateTime(2020, 10, 30);
			var reverseDate20201230 = new DateTime(2020, 12, 30);
			var reverseDate20220201 = new DateTime(2022, 2, 1);

			var accrual20201230 = TestObjectCreator.CreateAccrual();
			accrual20201230.AL_PostDate = postDate20201030;
			accrual20201230.Reverse();
			accrual20201230.AL_ReverseDate = reverseDate20201230;

			var wip20201230 = TestObjectCreator.CreateWIP();
			wip20201230.AL_PostDate = postDate20201030;
			wip20201230.Reverse();
			wip20201230.AL_ReverseDate = reverseDate20201230;

			var accrual20220201 = TestObjectCreator.CreateAccrual();
			accrual20220201.AL_PostDate = postDate20201030;
			accrual20220201.Reverse();
			accrual20220201.AL_ReverseDate = reverseDate20220201;

			var wip20220201 = TestObjectCreator.CreateWIP();
			wip20220201.AL_PostDate = postDate20201030;
			wip20220201.Reverse();
			wip20220201.AL_ReverseDate = reverseDate20220201;

			Factory.Save();

			var expectedReverse20201230 = new[] {
				accrual20201230.PK.ToString()
				, wip20201230.PK.ToString()
			};

			var expectedReverse20220201 = new[] {
				accrual20220201.PK.ToString()
				, wip20220201.PK.ToString()
			};

			AssertQueueTransaction(new[] { (postDate20201030, postDate20201030.AddDays(1)) }
				, expectedPost: expectedReverse20201230.Concat(expectedReverse20220201)
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (reverseDate20220201, reverseDate20220201.AddDays(1)) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: expectedReverse20220201);

			AssertQueueTransaction(new[] { (reverseDate20201230, reverseDate20201230.AddDays(1)), (reverseDate20220201, reverseDate20220201.AddDays(1)) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: expectedReverse20201230.Concat(expectedReverse20220201));

			AssertQueueTransaction(new[] { (postDate20201030, reverseDate20220201.AddDays(1)) }
				, expectedPost: expectedReverse20201230.Concat(expectedReverse20220201)
				, expectedReverse: expectedReverse20201230.Concat(expectedReverse20220201));

			AssertQueueTransaction(new[] { (postDate20201030, DateTime.MinValue) }
				, expectedPost: expectedReverse20201230.Concat(expectedReverse20220201)
				, expectedReverse: expectedReverse20201230.Concat(expectedReverse20220201));
		}

		[TestDate(2023, 10, 20)]
		public void TestQueueTransaction_JobCostingTransactionLinesForReverseDate_JNLJRJ()
		{
			var postdate20201230 = new ZDateTime(2020, 12, 30);
			var reverseDate20230201 = new DateTime(2023, 02, 01);
			var reverseDate20220201 = new DateTime(2022, 2, 1);

			var job20230201 = CreateJobForReverseDateTesting(reverseDate20230201);

			var jCJournal20230201 = TestObjectCreator.CreateJCJournalHeader(postdate20201230, 20m);
			var jCJournalLine20230201 = TestObjectCreator.CreateJCJournalLine(jCJournal20230201, TestObjectCreator.RevenueChargeCode, job20230201, postdate20201230, 20m);

			var jobRevenueJournal20230201 = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.RevenueChargeCode, job20230201, 20m);
			jobRevenueJournal20230201.AH_PostDate = postdate20201230;

			Factory.Save();

			jCJournalLine20230201.AL_ReverseDate = reverseDate20230201;

			Factory.Save();

			var job20220201 = CreateJobForReverseDateTesting(reverseDate20220201);

			var jcJournal20220201 = TestObjectCreator.CreateJCJournalHeader(postdate20201230, 20m);
			var jCJournalLine20220201 = TestObjectCreator.CreateJCJournalLine(jcJournal20220201, TestObjectCreator.RevenueChargeCode, job20220201, postdate20201230, 20m);

			var jobRevenueJournal20220201 = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.RevenueChargeCode, job20220201, 20m);
			jobRevenueJournal20220201.AH_PostDate = postdate20201230;

			Factory.Save();

			jCJournalLine20220201.AL_ReverseDate = reverseDate20220201;

			Factory.Save();

			var expected20230201 = new[] {
				jCJournalLine20230201.PK.ToString()
				, jobRevenueJournal20230201.Lines[0].PK.ToString()
				, jobRevenueJournal20230201.Lines[1].PK.ToString()
			};

			var expected20220201 = new[] {
				jCJournalLine20220201.PK.ToString()
				, jobRevenueJournal20220201.Lines[0].PK.ToString()
				, jobRevenueJournal20220201.Lines[1].PK.ToString()
			};

			AssertQueueTransaction(new[] { (reverseDate20230201, reverseDate20230201.AddDays(1)) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: expected20230201);

			AssertQueueTransaction(new[] { (reverseDate20220201, reverseDate20220201.AddDays(1)) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: expected20220201);

			AssertQueueTransaction(new[] { (reverseDate20220201, reverseDate20220201.AddDays(1)), (reverseDate20230201, reverseDate20230201.AddDays(1)) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: expected20220201.Concat(expected20230201));

			AssertQueueTransaction(new[] { (reverseDate20220201, reverseDate20230201.AddDays(1)) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: expected20220201.Concat(expected20230201));

			AssertQueueTransaction(new[] { (reverseDate20220201, DateTime.MinValue) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: expected20220201.Concat(expected20230201));

			Job CreateJobForReverseDateTesting(ZDateTime pickUpCartageCompletedDate)
			{
				var valuesForTest = new RevenueRecognitionCollection();
				var setting = valuesForTest.AddNew();
				setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
				setting.DirectionCode = Core.Constants.FreightShipmentDirection.Code.All;
				setting.Mode = Core.Constants.TransportModes.Air;
				setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.DocsAndCartage.JP_PickupCartageCompleted = pickUpCartageCompletedDate;

				var job = Factory.NewJobWithValidTestDataForTesting<Job>();
				job.Parent = shipment;
				return job;
			}
		}

		[TestDate(2023, 10, 20)]
		public void TestQueueTransaction_ExcludeAPARWithTransactionCategory_PBW()
		{
			var postDate20230102 = new DateTime(2023, 01, 02);
			var arJournal = TestObjectCreator.CreateJournal<ARJournal>(100m, postDate20230102, TestObjectCreator.Debtor.PK);
			arJournal.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.PaymentBasisWithholding;

			var apJournal = TestObjectCreator.CreateJournal<APJournal>(100m, postDate20230102, TestObjectCreator.Debtor.PK);
			apJournal.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.PaymentBasisWithholding;

			Factory.Save();

			AssertQueueTransaction(new[] { (postDate20230102, postDate20230102.AddDays(1)) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: Array.Empty<string>());

			AssertQueueTransaction(new[] { (postDate20230102, DateTime.MinValue) }
				, expectedPost: Array.Empty<string>()
				, expectedReverse: Array.Empty<string>());
		}

		[SuspendCriticalValidation]
		[TestDate(2023, 10, 20)]
		public void TestHandleGLDComplianceReport_WhenRegenerateGLD()
		{
			var gld1 = CreateDummyGLD(new DateTime(2022, 10, 20), GlbCompany.CurrentCompany);
			var gld2 = CreateDummyGLD(new DateTime(2022, 10, 21), GlbCompany.CurrentCompany);

			var complianceReportFIN = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReportFIN.ACR_ReportType = "TST";
			complianceReportFIN.ACR_DateFrom = ZDate.Today.AddDays(-2);
			complianceReportFIN.ACR_DateTo = ZDate.Today.AddDays(2);
			complianceReportFIN.ACR_Status = AccComplianceReport.Status.ReportFinalised;

			var complianceReportGEN = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReportGEN.ACR_ReportType = "TST";
			complianceReportGEN.ACR_DateFrom = ZDate.Today.AddDays(-2);
			complianceReportGEN.ACR_DateTo = ZDate.Today.AddDays(2);
			complianceReportGEN.ACR_Status = AccComplianceReport.Status.ReportGenerated;

			var complianceReportADD = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReportADD.ACR_ReportType = "TST";
			complianceReportADD.ACR_DateFrom = ZDate.Today.AddDays(-2);
			complianceReportADD.ACR_DateTo = ZDate.Today.AddDays(2);
			complianceReportADD.ACR_Status = AccComplianceReport.Status.ReportCreated;

			Factory.Save();

			var sql = $@"INSERT INTO AccComplianceReportTransactionPivot (ACL_PK, ACL_ParentID, ACL_ParentTableCode, ACL_GC_Company, ACL_ACR_Report, ACL_ReportSequence) VALUES
(NEWID(), '{gld1}', 'GLD', '{GlbCompany.CurrentCompany.PK}', '{complianceReportFIN.PK}', 1),
(NEWID(), '{gld2}', 'GLD', '{GlbCompany.CurrentCompany.PK}', '{complianceReportGEN.PK}', 2),
(NEWID(), NEWID(), 'AL', '{GlbCompany.CurrentCompany.PK}', '{complianceReportADD.PK}', 3)";

			TestConnection.ExecuteNonQuery(sql);

			GeneralLedgerDataQueue.RemoveNarrowGLD(Db.Connection, GlbCompany.CurrentCompany.PK.ToGuid(), new DateTime(2022, 10, 20), new DateTime(2022, 10, 22));

			var newFactory = Factory.CreateNewFactory();
			var complianceReports = newFactory.Load<AccComplianceReport>(new ZQuery());
			AssertEquals(false, complianceReports.Any(x => x.ACR_Status == AccComplianceReport.Status.ReportFinalised));

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM AccComplianceReportTransactionPivot");
			AssertEquals(1, result.Rows.Count);
			AssertNotEquals("GLD", result.Rows[0]["ACL_ParentTableCode"]);

			ZGuid CreateDummyGLD(DateTime recognitionDate, GlbCompany company)
			{
				var narrowGLD = Factory.NewWithValidTestData<AccGeneralLedgerData>();
				narrowGLD.GLD_PostDate = recognitionDate;
				narrowGLD.GLD_GC_Company = company.PK;
				narrowGLD.GLD_Type = "PST";
				narrowGLD.GLD_GLAccountType = "ARC";
				narrowGLD.GLD_AH_TransactionHeader = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m).PK;
				narrowGLD.GLD_PostPeriod = 202301;
				return narrowGLD.PK;
			}
		}

		void AssertQueueTransaction(IEnumerable<(DateTime StarDate, DateTime EndDate)> queueDates, IEnumerable<string> expectedPost, IEnumerable<string> expectedReverse)
		{
			AssertQueueTransaction(queueDates.Select(x => (Company: GlbCompany.CurrentCompany, x.StarDate, x.EndDate))
				, expectedPost
				, expectedReverse
			);
		}

		void AssertQueueTransaction(IEnumerable<(GlbCompany Company, DateTime StarDate, DateTime EndDate)> queueConditions, IEnumerable<string> expectedPost, IEnumerable<string> expectedReverse)
		{
			var systemCreateTime = ZDateTime.UtcNow.ToDateTime().Date;
			AssertBasic();

			if (expectedPost.Any() || expectedReverse.Any())
			{
				AssertSystemCreateTime();
				AssertCheckDuplicated();
			}

			void AssertBasic()
			{
				ClearTestingQueue();

				foreach (var queueCondition in queueConditions)
				{
					GeneralLedgerDataQueue.QueueTransaction(Db.Connection, queueCondition.Company.PK.ToGuid(), queueCondition.StarDate, queueCondition.EndDate, systemCreateTime, true);
				}

				AssertQueueTransactionCore("", expectedPost, expectedReverse);
			}

			void AssertSystemCreateTime()
			{
				ClearTestingQueue();

				foreach (var queueCondition in queueConditions)
				{
					GeneralLedgerDataQueue.QueueTransaction(Db.Connection, queueCondition.Company.PK.ToGuid(), queueCondition.StarDate, queueCondition.EndDate, systemCreateTime.AddDays(-1), true);
				}
				AssertQueueTransactionCore("EndSystemCreateTime is less than SystemCreateTime"
					, expectedPost: Array.Empty<string>()
					, expectedReverse: Array.Empty<string>()
				);

				foreach (var queueCondition in queueConditions)
				{
					GeneralLedgerDataQueue.QueueTransaction(Db.Connection, queueCondition.Company.PK.ToGuid(), queueCondition.StarDate, queueCondition.EndDate, systemCreateTime.AddDays(1), true);
				}
				AssertQueueTransactionCore("EndSystemCreateTime is greater than SystemCreateTime"
					, expectedPost
					, expectedReverse);
			}

			void AssertCheckDuplicated()
			{
				ClearTestingQueue();
				foreach (var queueCondition in queueConditions)
				{
					GeneralLedgerDataQueue.QueueTransaction(Db.Connection, queueCondition.Company.PK.ToGuid(), queueCondition.StarDate, queueCondition.EndDate, systemCreateTime, true);
					GeneralLedgerDataQueue.QueueTransaction(Db.Connection, queueCondition.Company.PK.ToGuid(), queueCondition.StarDate, queueCondition.EndDate, systemCreateTime, true);
				}
				AssertQueueTransactionCore("Check duplicated"
					, expectedPost
					, expectedReverse);

				foreach (var queueCondition in queueConditions)
				{
					GeneralLedgerDataQueue.QueueTransaction(Db.Connection, queueCondition.Company.PK.ToGuid(), queueCondition.StarDate, queueCondition.EndDate, systemCreateTime, false);
				}
				AssertQueueTransactionCore("Do not check duplicated"
					, expectedPost.Concat(expectedPost)
					, expectedReverse.Concat(expectedReverse));
			}
		}

		void ClearTestingQueue()
		{
			using (var command = Db.Connection.Command("TRUNCATE TABLE AccTransactionPostingToGLDQueue"))
			{
				command.ExecuteNonQuery();
			}
			AssertQueueTransactionCore("ClearTestingQueue", expectedPost: Array.Empty<string>(), expectedReverse: Array.Empty<string>());
		}

		void AssertQueueTransactionCore(string message, IEnumerable<string> expectedPost, IEnumerable<string> expectedReverse)
		{
			var collection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			collection.Load(@"
SELECT 
  APQ_ParentID
, CONVERT(char(1), APQ_IsReverse) as APQ_IsReverse
, APQ_SystemCreateUser 
FROM AccTransactionPostingToGLDQueue");

			var actualPost = new List<string>();
			var actualReverse = new List<string>();
			collection.ForEach(x =>
			{
				if (x["APQ_IsReverse"].ToString() == "1")
				{
					actualReverse.Add(x["APQ_ParentID"].ToString());
				}
				else
				{
					actualPost.Add(x["APQ_ParentID"].ToString());
				}
				AssertEquals($"{message}-PreCondition APQ_SystemCreateUser", x["APQ_SystemCreateUser"].ToString(), GlbStaff.CurrentUser.GS_Code);
			});

			AssertContainsExactElementsInAnyOrder($"{message}-Post Transaction PK List", expectedPost, actualPost);
			AssertContainsExactElementsInAnyOrder($"{message}-Reverse Transaction PK List", expectedReverse, actualReverse);
		}

		protected override void SetUp()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(GlbCompany.CurrentCompany, 2021);
			TestObjectCreator.CreateTestPeriodsForEntireYear(GlbCompany.CurrentCompany, 2022);
			TestObjectCreator.CreateTestPeriodsForEntireYear(GlbCompany.CurrentCompany, 2023);
			Factory.Save();
		}

		GeneralLedgerDataQueue GeneralLedgerDataQueue => generalLedgerDataQueue ?? (generalLedgerDataQueue = new GeneralLedgerDataQueue());
		GeneralLedgerDataQueue generalLedgerDataQueue;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
