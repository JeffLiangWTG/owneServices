using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;

using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.Base.Reversing
{
	public class PeriodApportionmentsReverseTest : TestCaseWithFactory
	{
		[TestDate(2020, 3, 21)]
		public void TestPeriodApportionmentReversal_PrepaymentScenario()
		{
			var initialJournals = Factory.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLStandardJournal));
			Assert("Precondition", !initialJournals.Any());

			var testObjectCreator = new TestObjectCreator(Factory);
			var gLHeader = testObjectCreator.GLHeader1;
			var clearingAccount = testObjectCreator.GLJournalClearingAccount;

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);

			var shipment = testObjectCreator.CreateShipment("S001001");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0M, testObjectCreator.Agent, 0M);
			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), "001001", testObjectCreator.GBP, 0.55M, testObjectCreator.AALSHI);

			var line = testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.GBP, 0.55M, 110M);
			line.AL_AC = testObjectCreator.CC1.PK;
			line.AL_JH = job.PK;

			gLHeader.AG_Description = "GL Header 1";
			line.AL_AG = gLHeader.PK;

			var jobCharge = testObjectCreator.CreateCharge(line, job, testObjectCreator.CC1, testObjectCreator.AUD);
			jobCharge.JR_AT_CostGSTRate = testObjectCreator.GST1.PK;
			line.AL_AT = testObjectCreator.GST1.PK;
			line.PeriodApportionmentMethod = "PER";
			line.PeriodClearingGLAccountPK = clearingAccount.PK;
			line.PeriodStartDate = new ZDate(2020, 2, 15);
			line.PeriodEndDate = new ZDate(2020, 4, 29);

			invoice.AH_PostDate = new ZDateTime(2020, 2, 15);

			//posting
			AssertEquals(4, invoice.MultiPeriodApportionmentJournals.Count);
			Factory.Save();

			var sub1CRLineDescription = @"GL CLEARING ACCOUNT
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: 6820.00.00 for period 202008
Amount Cleared To Date: GBP 36.67 AUD 66.67
Amount Yet to be Cleared: GBP 73.33 AUD 133.33";
			var sub1DRLineDescription = string.Format(@"GL Header 1
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: {0} for period 202008
Amount Cleared To Date: GBP 36.67 AUD 66.67
Amount Yet to be Cleared: GBP 73.33 AUD 133.33", gLHeader.AG_AccountNum);

			var sub2CRLineDescription = @"GL CLEARING ACCOUNT
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: 6820.00.00 for period 202009
Amount Cleared To Date: GBP 73.34 AUD 133.34
Amount Yet to be Cleared: GBP 36.66 AUD 66.66";
			var sub2DRLineDescription = string.Format(@"GL Header 1
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: {0} for period 202009
Amount Cleared To Date: GBP 73.34 AUD 133.34
Amount Yet to be Cleared: GBP 36.66 AUD 66.66", gLHeader.AG_AccountNum);

			var sub3CRLineDescription = @"GL CLEARING ACCOUNT
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: 6820.00.00 for period 202010
Amount Cleared To Date: GBP 110.00 AUD 200.00
Amount Yet to be Cleared: GBP 0.00 AUD 0.00";
			var sub3DRLineDescription = string.Format(@"GL Header 1
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: {0} for period 202010
Amount Cleared To Date: GBP 110.00 AUD 200.00
Amount Yet to be Cleared: GBP 0.00 AUD 0.00", gLHeader.AG_AccountNum);

			IEnumerable<string> actual()
			{
				var result = Factory.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLStandardJournal))
					.SelectMany(x => x.Lines.Cast<GLJournalLine>()).ToArray();

				return
					result
					.Select(x => (
						x.TransactionHeader.AH_Desc,
						x.TransactionHeader.AH_PostDate.ToShortDateString(),
						x.DebitCreditSign,
						x.IsInDatabase,
						x.AL_LineAmount,
						Math.Round(x.ExchangeRate.Rate, 2),         //round because exchange rate is calculated during PeriodApportionmentLine creation. Alternatively we can make PeriodApportionmentLine.LocalAmount calculated instead
						x.AL_RX_NKTransactionCurrency,
						x.AL_AG,
						x.AL_Desc,
						x.TransactionHeader.AH_TransactionBelongsToGroup).ToString());
			}

			var expected = new[]
			{
				("MASTER JOURNAL FOR AP INV 001001", "29-Feb-20",       "CR",   true,   -200,   0.55m, "GBP",       gLHeader.PK,            "MASTER JOURNAL FOR AP INV 001001", invoice.PK).ToString(),
				("MASTER JOURNAL FOR AP INV 001001", "29-Feb-20",       "DR",   true,   200,    0.55m, "GBP",       clearingAccount.PK,     "MASTER JOURNAL FOR AP INV 001001", invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                   "29-Feb-20",      "CR",   true,   -66.67, 0.55m, "GBP",       clearingAccount.PK,     sub1CRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                   "29-Feb-20",      "DR",   true,   66.67,  0.55m, "GBP",       gLHeader.PK,            sub1DRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                   "31-Mar-20",      "CR",   true,   -66.67, 0.55m, "GBP",       clearingAccount.PK,     sub2CRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                   "31-Mar-20",      "DR",   true,   66.67,  0.55m, "GBP",       gLHeader.PK,            sub2DRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202010",                   "30-Apr-20",      "CR",   true,   -66.66, 0.55m, "GBP",       clearingAccount.PK,     sub3CRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202010",                   "30-Apr-20",      "DR",   true,   66.66,  0.55m, "GBP",       gLHeader.PK,            sub3DRLineDescription, invoice.PK).ToString(),
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());

			//reversing
			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(invoice);
			reversing.Reverse();
			reversing.ReverseTransaction.TransactionNumber = "555";

			Assert("We need to hide multi-period apportionment functionality from reverse transaction screen", !(reversing.ReverseTransaction as InvoicingBase).SupportMultiPeriodApportionment);

			Factory.Save();

			var reversalInvoice = invoice.ReverseInvoice;

			expected = new[]
			{
				//original
				("MASTER JOURNAL FOR AP INV 001001",                "29-Feb-20",        "CR",   true,   -200,       0.55m, "GBP",       gLHeader.PK,            "MASTER JOURNAL FOR AP INV 001001", invoice.PK).ToString(),
				("MASTER JOURNAL FOR AP INV 001001",                "29-Feb-20",        "DR",   true,   200,        0.55m, "GBP",       clearingAccount.PK,     "MASTER JOURNAL FOR AP INV 001001", invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                             "29-Feb-20",        "CR",   true,   -66.67,     0.55m, "GBP",       clearingAccount.PK,     sub1CRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                             "29-Feb-20",        "DR",   true,   66.67,      0.55m, "GBP",       gLHeader.PK,            sub1DRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                             "31-Mar-20",        "CR",   true,   -66.67,     0.55m, "GBP",       clearingAccount.PK,     sub2CRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                             "31-Mar-20",        "DR",   true,   66.67,      0.55m, "GBP",       gLHeader.PK,            sub2DRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202010",                             "30-Apr-20",        "CR",   true,   -66.66,     0.55m, "GBP",       clearingAccount.PK,     sub3CRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202010",                             "30-Apr-20",        "DR",   true,   66.66,      0.55m, "GBP",        gLHeader.PK,            sub3DRLineDescription, invoice.PK).ToString(),
				//reverse
				("(REVERSE) MASTER JOURNAL FOR AP INV 001001",  "31-Mar-20",        "DR",   true,   200,        0.55m, "GBP",       gLHeader.PK,            "(REVERSE) MASTER JOURNAL FOR AP INV 001001", reversalInvoice.PK).ToString(),
				("(REVERSE) MASTER JOURNAL FOR AP INV 001001",  "31-Mar-20",        "CR",   true,   -200,       0.55m, "GBP",       clearingAccount.PK,     "(REVERSE) MASTER JOURNAL FOR AP INV 001001", reversalInvoice.PK).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202008",                       "31-Mar-20",        "DR",   true,   66.67,      0.55m, "GBP",       clearingAccount.PK,     "(REVERSE) " + sub1CRLineDescription, reversalInvoice.PK).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202008",                       "31-Mar-20",        "CR",   true,   -66.67,     0.55m, "GBP",       gLHeader.PK,            "(REVERSE) " + sub1DRLineDescription, reversalInvoice.PK).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202009",                       "31-Mar-20",        "DR",   true,   66.67,      0.55m, "GBP",       clearingAccount.PK,     "(REVERSE) " + sub2CRLineDescription, reversalInvoice.PK).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202009",                       "31-Mar-20",        "CR",   true,   -66.67,     0.55m, "GBP",       gLHeader.PK,            "(REVERSE) " + sub2DRLineDescription, reversalInvoice.PK).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202010",                       "30-Apr-20",        "DR",   true,   66.66,      0.55m, "GBP",       clearingAccount.PK,     "(REVERSE) " + sub3CRLineDescription, reversalInvoice.PK).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202010",                       "30-Apr-20",        "CR",   true,   -66.66,     0.55m, "GBP",        gLHeader.PK,           "(REVERSE) " + sub3DRLineDescription, reversalInvoice.PK).ToString(),
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());
		}

		[TestDate(2020, 3, 21)]
		public void TestPeriodApportionmentReversalWithPostDateChange_PrePaymentScenario()
		{
			var initialJournals = Factory.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLStandardJournal));
			Assert("Precondition", !initialJournals.Any());

			var testObjectCreator = new TestObjectCreator(Factory);
			var gLHeader = testObjectCreator.GLHeader1;
			var clearingAccount = testObjectCreator.GLJournalClearingAccount;

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);

			var shipment = testObjectCreator.CreateShipment("S001001");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0M, testObjectCreator.Agent, 0M);
			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), "001001", testObjectCreator.AUD, 1M, testObjectCreator.AALSHI);
			var line = testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.AUD, 1M, 200M);
			line.AL_AC = testObjectCreator.CC1.PK;
			line.AL_JH = job.PK;

			gLHeader.AG_Description = "GL Header 1";
			line.AL_AG = gLHeader.PK;

			var jobCharge = testObjectCreator.CreateCharge(line, job, testObjectCreator.CC1, testObjectCreator.AUD);
			jobCharge.JR_AT_CostGSTRate = testObjectCreator.GST1.PK;
			line.AL_AT = testObjectCreator.GST1.PK;
			line.PeriodApportionmentMethod = "PER";
			line.PeriodClearingGLAccountPK = clearingAccount.PK;
			line.PeriodStartDate = new ZDate(2020, 1, 15);
			line.PeriodEndDate = new ZDate(2020, 4, 29);

			invoice.AH_PostDate = new ZDateTime(2020, 1, 15);

			//posting
			AssertEquals(5, invoice.MultiPeriodApportionmentJournals.Count);
			Factory.Save();

			var sub1CRLineDescription = @"GL CLEARING ACCOUNT
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: 6820.00.00 for period 202007
Amount Cleared To Date: AUD 50 AUD 50
Amount Yet to be Cleared: AUD 150 AUD 150";
			var sub1DRLineDescription = string.Format(@"GL Header 1
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: {0} for period 202007
Amount Cleared To Date: AUD 50 AUD 50
Amount Yet to be Cleared: AUD 150 AUD 150", gLHeader.AG_AccountNum);

			var sub2CRLineDescription = @"GL CLEARING ACCOUNT
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: 6820.00.00 for period 202008
Amount Cleared To Date: AUD 100 AUD 100
Amount Yet to be Cleared: AUD 100 AUD 100";
			var sub2DRLineDescription = string.Format(@"GL Header 1
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: {0} for period 202008
Amount Cleared To Date: AUD 100 AUD 100
Amount Yet to be Cleared: AUD 100 AUD 100", gLHeader.AG_AccountNum);

			var sub3CRLineDescription = @"GL CLEARING ACCOUNT
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: 6820.00.00 for period 202009
Amount Cleared To Date: AUD 150 AUD 150
Amount Yet to be Cleared: AUD 50 AUD 50";
			var sub3DRLineDescription = string.Format(@"GL Header 1
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: {0} for period 202009
Amount Cleared To Date: AUD 150 AUD 150
Amount Yet to be Cleared: AUD 50 AUD 50", gLHeader.AG_AccountNum);

			var sub4CRLineDescription = @"GL CLEARING ACCOUNT
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: 6820.00.00 for period 202010
Amount Cleared To Date: AUD 200 AUD 200
Amount Yet to be Cleared: AUD 0 AUD 0";
			var sub4DRLineDescription = string.Format(@"GL Header 1
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: {0} for period 202010
Amount Cleared To Date: AUD 200 AUD 200
Amount Yet to be Cleared: AUD 0 AUD 0", gLHeader.AG_AccountNum);

			IEnumerable<string> actual()
			{
				var result = Factory.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLStandardJournal))
					.SelectMany(x => x.Lines.Cast<GLJournalLine>()).ToArray();

				return
					result
					.Select(x => (
						x.TransactionHeader.AH_Desc,
						x.TransactionHeader.AH_PostDate.ToShortDateString(),
						x.DebitCreditSign,
						x.IsInDatabase,
						x.AL_LineAmount,
						x.AL_AG,
						x.AL_Desc).ToString());
			}

			var expected = new[]
			{
				("MASTER JOURNAL FOR AP INV 001001", "31-Jan-20",        "CR",   true,   -200,    gLHeader.PK,            "MASTER JOURNAL FOR AP INV 001001").ToString(),
				("MASTER JOURNAL FOR AP INV 001001", "31-Jan-20",        "DR",   true,   200,     clearingAccount.PK,     "MASTER JOURNAL FOR AP INV 001001").ToString(),
				("Sub Journal For AP INV 001001 Period 202007",                   "31-Jan-20",        "CR",   true,   -50,     clearingAccount.PK,     sub1CRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202007",                   "31-Jan-20",        "DR",   true,   50,      gLHeader.PK,            sub1DRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                   "29-Feb-20",        "CR",   true,   -50,     clearingAccount.PK,     sub2CRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                   "29-Feb-20",        "DR",   true,   50,      gLHeader.PK,            sub2DRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                   "31-Mar-20",        "CR",   true,   -50,     clearingAccount.PK,     sub3CRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                   "31-Mar-20",        "DR",   true,   50,      gLHeader.PK,            sub3DRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202010",                   "30-Apr-20",        "CR",   true,   -50,     clearingAccount.PK,     sub4CRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202010",                   "30-Apr-20",        "DR",   true,   50,      gLHeader.PK,            sub4DRLineDescription).ToString(),
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());

			//reversing
			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(invoice);
			reversing.Reverse();
			reversing.ReverseTransaction.TransactionNumber = "555";
			(reversing.ReverseTransaction as APCreditNote).AH_PostDate = new ZDateTime(2020, 2, 15);

			Factory.Save();

			expected = new[]
			{
				//original
				("MASTER JOURNAL FOR AP INV 001001",                "31-Jan-20",        "CR",   true,   -200,    gLHeader.PK,               "MASTER JOURNAL FOR AP INV 001001").ToString(),
				("MASTER JOURNAL FOR AP INV 001001",                "31-Jan-20",        "DR",   true,   200,     clearingAccount.PK,        "MASTER JOURNAL FOR AP INV 001001").ToString(),
				("Sub Journal For AP INV 001001 Period 202007",                             "31-Jan-20",        "CR",   true,   -50,     clearingAccount.PK,        sub1CRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202007",                             "31-Jan-20",        "DR",   true,   50,      gLHeader.PK,               sub1DRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                             "29-Feb-20",        "CR",   true,   -50,     clearingAccount.PK,        sub2CRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                             "29-Feb-20",        "DR",   true,   50,      gLHeader.PK,               sub2DRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                             "31-Mar-20",        "CR",   true,   -50,     clearingAccount.PK,        sub3CRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                             "31-Mar-20",        "DR",   true,   50,      gLHeader.PK,               sub3DRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202010",                             "30-Apr-20",        "CR",   true,   -50,     clearingAccount.PK,        sub4CRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202010",                             "30-Apr-20",        "DR",   true,   50,      gLHeader.PK,               sub4DRLineDescription).ToString(),
				//reverse
				("(REVERSE) MASTER JOURNAL FOR AP INV 001001",  "29-Feb-20",        "DR",   true,   200,    gLHeader.PK,                "(REVERSE) MASTER JOURNAL FOR AP INV 001001").ToString(),
				("(REVERSE) MASTER JOURNAL FOR AP INV 001001",  "29-Feb-20",        "CR",   true,   -200,     clearingAccount.PK,       "(REVERSE) MASTER JOURNAL FOR AP INV 001001").ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202007",                       "29-Feb-20",        "DR",   true,   50,     clearingAccount.PK,         "(REVERSE) " + sub1CRLineDescription).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202007",                     "29-Feb-20",        "CR",   true,   -50,      gLHeader.PK,                "(REVERSE) " + sub1DRLineDescription).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202008",                       "29-Feb-20",        "DR",   true,   50,     clearingAccount.PK,         "(REVERSE) " + sub2CRLineDescription).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202008",                       "29-Feb-20",        "CR",   true,   -50,      gLHeader.PK,              "(REVERSE) " + sub2DRLineDescription).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202009",                       "31-Mar-20",        "DR",   true,   50,     clearingAccount.PK,         "(REVERSE) " + sub3CRLineDescription).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202009",                       "31-Mar-20",        "CR",   true,   -50,      gLHeader.PK,              "(REVERSE) " + sub3DRLineDescription).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202010",                       "30-Apr-20",        "DR",   true,   50,     clearingAccount.PK,         "(REVERSE) " + sub4CRLineDescription).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202010",                       "30-Apr-20",        "CR",   true,   -50,      gLHeader.PK,              "(REVERSE) " + sub4DRLineDescription).ToString(),
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());
		}

		[TestDate(2020, 3, 21)]
		public void TestPeriodApportionmentReversalWithPostDateChange_PostPaymentScenario()
		{
			var initialJournals = Factory.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLStandardJournal));
			Assert("Precondition", !initialJournals.Any());

			var testObjectCreator = new TestObjectCreator(Factory);
			var gLHeader = testObjectCreator.GLHeader1;
			var clearingAccount = testObjectCreator.GLJournalClearingAccount;

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);

			var shipment = testObjectCreator.CreateShipment("S001001");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0M, testObjectCreator.Agent, 0M);
			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), "001001", testObjectCreator.AUD, 1M, testObjectCreator.AALSHI);
			var line = testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.AUD, 1M, 200M);
			line.AL_AC = testObjectCreator.CC1.PK;
			line.AL_JH = job.PK;

			gLHeader.AG_Description = "GL Header 1";
			line.AL_AG = gLHeader.PK;

			var jobCharge = testObjectCreator.CreateCharge(line, job, testObjectCreator.CC1, testObjectCreator.AUD);
			jobCharge.JR_AT_CostGSTRate = testObjectCreator.GST1.PK;
			line.AL_AT = testObjectCreator.GST1.PK;
			line.PeriodApportionmentMethod = "PER";
			line.PeriodClearingGLAccountPK = clearingAccount.PK;
			line.PeriodStartDate = new ZDate(2020, 1, 15);
			line.PeriodEndDate = new ZDate(2020, 4, 29);

			invoice.AH_PostDate = new ZDateTime(2020, 2, 15);

			//posting
			AssertEquals(5, invoice.MultiPeriodApportionmentJournals.Count);
			Factory.Save();

			var sub1CRLineDescription = @"GL CLEARING ACCOUNT
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: 6820.00.00 for period 202007
Amount Cleared To Date: AUD 50 AUD 50
Amount Yet to be Cleared: AUD 150 AUD 150";
			var sub1DRLineDescription = string.Format(@"GL Header 1
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: {0} for period 202007
Amount Cleared To Date: AUD 50 AUD 50
Amount Yet to be Cleared: AUD 150 AUD 150", gLHeader.AG_AccountNum);

			var sub2CRLineDescription = @"GL CLEARING ACCOUNT
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: 6820.00.00 for period 202008
Amount Cleared To Date: AUD 100 AUD 100
Amount Yet to be Cleared: AUD 100 AUD 100";
			var sub2DRLineDescription = string.Format(@"GL Header 1
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: {0} for period 202008
Amount Cleared To Date: AUD 100 AUD 100
Amount Yet to be Cleared: AUD 100 AUD 100", gLHeader.AG_AccountNum);

			var sub3CRLineDescription = @"GL CLEARING ACCOUNT
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: 6820.00.00 for period 202009
Amount Cleared To Date: AUD 150 AUD 150
Amount Yet to be Cleared: AUD 50 AUD 50";
			var sub3DRLineDescription = string.Format(@"GL Header 1
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: {0} for period 202009
Amount Cleared To Date: AUD 150 AUD 150
Amount Yet to be Cleared: AUD 50 AUD 50", gLHeader.AG_AccountNum);

			var sub4CRLineDescription = @"GL CLEARING ACCOUNT
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: 6820.00.00 for period 202010
Amount Cleared To Date: AUD 200 AUD 200
Amount Yet to be Cleared: AUD 0 AUD 0";
			var sub4DRLineDescription = string.Format(@"GL Header 1
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: {0} for period 202010
Amount Cleared To Date: AUD 200 AUD 200
Amount Yet to be Cleared: AUD 0 AUD 0", gLHeader.AG_AccountNum);

			IEnumerable<string> actual()
			{
				var result = Factory.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLStandardJournal))
					.SelectMany(x => x.Lines.Cast<GLJournalLine>()).ToArray();

				return
					result
					.Select(x => (
						x.TransactionHeader.AH_Desc,
						x.TransactionHeader.AH_PostDate.ToShortDateString(),
						x.DebitCreditSign,
						x.IsInDatabase,
						x.AL_LineAmount,
						x.AL_AG,
						x.AL_Desc).ToString());
			}

			var expected = new[]
			{
				("MASTER JOURNAL FOR AP INV 001001", "29-Feb-20",        "CR",   true,   -200,    gLHeader.PK,            "MASTER JOURNAL FOR AP INV 001001").ToString(),
				("MASTER JOURNAL FOR AP INV 001001", "29-Feb-20",        "DR",   true,   200,     clearingAccount.PK,     "MASTER JOURNAL FOR AP INV 001001").ToString(),
				("Sub Journal For AP INV 001001 Period 202007",                   "31-Jan-20",        "CR",   true,   -50,     clearingAccount.PK,     sub1CRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202007",                   "31-Jan-20",        "DR",   true,   50,      gLHeader.PK,            sub1DRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                   "29-Feb-20",        "CR",   true,   -50,     clearingAccount.PK,     sub2CRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                   "29-Feb-20",        "DR",   true,   50,      gLHeader.PK,            sub2DRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                   "31-Mar-20",        "CR",   true,   -50,     clearingAccount.PK,     sub3CRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                   "31-Mar-20",        "DR",   true,   50,      gLHeader.PK,            sub3DRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202010",                   "30-Apr-20",        "CR",   true,   -50,     clearingAccount.PK,     sub4CRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202010",                   "30-Apr-20",        "DR",   true,   50,      gLHeader.PK,            sub4DRLineDescription).ToString(),
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());

			//reversing
			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(invoice);
			reversing.Reverse();
			reversing.ReverseTransaction.TransactionNumber = "555";
			(reversing.ReverseTransaction as APCreditNote).AH_PostDate = new ZDateTime(2020, 3, 15);

			Factory.Save();

			expected = new[]
			{
				//original
				("MASTER JOURNAL FOR AP INV 001001",             "29-Feb-20",        "CR",   true,   -200,    gLHeader.PK,               "MASTER JOURNAL FOR AP INV 001001").ToString(),
				("MASTER JOURNAL FOR AP INV 001001",             "29-Feb-20",        "DR",   true,   200,     clearingAccount.PK,        "MASTER JOURNAL FOR AP INV 001001").ToString(),
				("Sub Journal For AP INV 001001 Period 202007",                               "31-Jan-20",        "CR",   true,   -50,     clearingAccount.PK,        sub1CRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202007",                               "31-Jan-20",        "DR",   true,   50,      gLHeader.PK,               sub1DRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                               "29-Feb-20",        "CR",   true,   -50,     clearingAccount.PK,        sub2CRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                               "29-Feb-20",        "DR",   true,   50,      gLHeader.PK,               sub2DRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                               "31-Mar-20",        "CR",   true,   -50,     clearingAccount.PK,        sub3CRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                               "31-Mar-20",        "DR",   true,   50,      gLHeader.PK,               sub3DRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202010",                               "30-Apr-20",        "CR",   true,   -50,     clearingAccount.PK,        sub4CRLineDescription).ToString(),
				("Sub Journal For AP INV 001001 Period 202010",                               "30-Apr-20",        "DR",   true,   50,      gLHeader.PK,               sub4DRLineDescription).ToString(),
				//reverse
				("(REVERSE) MASTER JOURNAL FOR AP INV 001001",   "29-Feb-20",        "DR",   true,   200,    gLHeader.PK,                "(REVERSE) MASTER JOURNAL FOR AP INV 001001").ToString(),
				("(REVERSE) MASTER JOURNAL FOR AP INV 001001",   "29-Feb-20",        "CR",   true,   -200,     clearingAccount.PK,       "(REVERSE) MASTER JOURNAL FOR AP INV 001001").ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202007",                     "31-Jan-20",        "DR",   true,   50,     clearingAccount.PK,         "(REVERSE) " + sub1CRLineDescription).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202007",                     "31-Jan-20",        "CR",   true,   -50,      gLHeader.PK,              "(REVERSE) " + sub1DRLineDescription).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202008",                     "29-Feb-20",        "DR",   true,   50,     clearingAccount.PK,         "(REVERSE) " + sub2CRLineDescription).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202008",                     "29-Feb-20",        "CR",   true,   -50,      gLHeader.PK,              "(REVERSE) " + sub2DRLineDescription).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202009",                     "31-Mar-20",        "DR",   true,   50,     clearingAccount.PK,         "(REVERSE) " + sub3CRLineDescription).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202009",                     "31-Mar-20",        "CR",   true,   -50,      gLHeader.PK,              "(REVERSE) " + sub3DRLineDescription).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202010",                     "30-Apr-20",        "DR",   true,   50,     clearingAccount.PK,         "(REVERSE) " + sub4CRLineDescription).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202010",                     "30-Apr-20",        "CR",   true,   -50,      gLHeader.PK,              "(REVERSE) " + sub4DRLineDescription).ToString(),
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());
		}

		[TestDate(2020, 2, 21)]
		public void TestPeriodApportionment_PrePaymentScenario_CannotReverseIfGlLedgerPeriodClosed()
		{
			var initialJournals = Factory.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLStandardJournal));
			Assert("Precondition", !initialJournals.Any());

			var testObjectCreator = new TestObjectCreator(Factory);
			var gLHeader = testObjectCreator.GLHeader1;
			var clearingAccount = testObjectCreator.GLJournalClearingAccount;

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);

			var shipment = testObjectCreator.CreateShipment("S001001");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0M, testObjectCreator.Agent, 0M);
			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), "001001", testObjectCreator.AUD, 1M, testObjectCreator.AALSHI);
			var line = testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.AUD, 1M, 200M);
			line.AL_AC = testObjectCreator.CC1.PK;
			line.AL_JH = job.PK;

			gLHeader.AG_Description = "GL Header 1";
			line.AL_AG = gLHeader.PK;

			var jobCharge = testObjectCreator.CreateCharge(line, job, testObjectCreator.CC1, testObjectCreator.AUD);
			jobCharge.JR_AT_CostGSTRate = testObjectCreator.GST1.PK;
			line.AL_AT = testObjectCreator.GST1.PK;
			line.PeriodApportionmentMethod = "PER";
			line.PeriodClearingGLAccountPK = clearingAccount.PK;
			line.PeriodStartDate = new ZDate(2020, 1, 15);
			line.PeriodEndDate = new ZDate(2020, 3, 29);

			invoice.AH_PostDate = new ZDateTime(2020, 1, 15);

			//posting
			AssertEquals(4, invoice.MultiPeriodApportionmentJournals.Count);
			Factory.Save();

			IEnumerable<string> actual()
			{
				var result = Factory.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLStandardJournal))
					.SelectMany(x => x.Lines.Cast<GLJournalLine>()).ToArray();

				return
					result
					.Select(x => (
						x.TransactionHeader.AH_Desc,
						x.TransactionHeader.AH_PostDate.ToShortDateString(),
						x.DebitCreditSign,
						x.IsInDatabase,
						x.AL_LineAmount,
						x.AL_AG).ToString());
			}

			var expected = new[]
			{
				("MASTER JOURNAL FOR AP INV 001001", "31-Jan-20",        "CR",   true,   -200,       gLHeader.PK).ToString(),
				("MASTER JOURNAL FOR AP INV 001001", "31-Jan-20",        "DR",   true,   200,        clearingAccount.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202007",                   "31-Jan-20",        "CR",   true,   -66.67,     clearingAccount.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202007",                   "31-Jan-20",        "DR",   true,   66.67,      gLHeader.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                   "29-Feb-20",        "CR",   true,   -66.67,     clearingAccount.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                   "29-Feb-20",        "DR",   true,   66.67,      gLHeader.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                   "31-Mar-20",        "CR",   true,   -66.66,     clearingAccount.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                   "31-Mar-20",        "DR",   true,   66.66,      gLHeader.PK).ToString(),
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());

			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var currentPeriod = periodCalculator.GetPeriodFromDate(ZDateTime.Today);
			var prevPeriod = periodCalculator.GetPreviousPeriod(currentPeriod);
			closeGLOnly(prevPeriod);
			closeGLOnly(currentPeriod);

			//reversing
			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(invoice);

			reversing.Reverse();            //error should be triggered here

			reversing.ReverseTransaction.TransactionNumber = "555";
			InvoicingBase creditNote = (reversing.ReverseTransaction as InvoicingBase);

			var failedJournalDueToClosedPeriod = creditNote.RelatedApportionmentReversings.FirstOrDefault(x => !x.CanReverseTransaction);
			AssertNotNull(failedJournalDueToClosedPeriod);
			AssertEquals("202008 GL period required for posting is closed.", failedJournalDueToClosedPeriod.CantReverseErrorMessage);

			AssertNoErrors(creditNote.AH_PostDateInfo);
			creditNote.AH_PostDate = new ZDateTime(2020, 1, 15);
			creditNote.Validation.ValidateAH_PostDate();
			AssertHasError(creditNote.AH_PostDateInfo, "Unable to post one or more journals for multi period apportionment. 202007 GL period required for posting is closed.");
		}

		[TestDate(2020, 3, 21)]
		public void TestPeriodApportionmentReversal_PostPaymentSecnario()
		{
			var initialJournals = Factory.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLStandardJournal));
			Assert("Precondition", !initialJournals.Any());

			var testObjectCreator = new TestObjectCreator(Factory);
			var gLHeader = testObjectCreator.GLHeader1;
			var clearingAccount = testObjectCreator.GLJournalClearingAccount;

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);

			var shipment = testObjectCreator.CreateShipment("S001001");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0M, testObjectCreator.Agent, 0M);
			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), "001001", testObjectCreator.GBP, 0.55M, testObjectCreator.AALSHI);

			var line = testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.GBP, 0.55M, 110M);
			line.AL_AC = testObjectCreator.CC1.PK;
			line.AL_JH = job.PK;

			gLHeader.AG_Description = "GL Header 1";
			line.AL_AG = gLHeader.PK;

			var jobCharge = testObjectCreator.CreateCharge(line, job, testObjectCreator.CC1, testObjectCreator.AUD);
			jobCharge.JR_AT_CostGSTRate = testObjectCreator.GST1.PK;
			line.AL_AT = testObjectCreator.GST1.PK;
			line.PeriodApportionmentMethod = "PER";
			line.PeriodClearingGLAccountPK = clearingAccount.PK;
			line.PeriodStartDate = new ZDate(2020, 2, 15);
			line.PeriodEndDate = new ZDate(2020, 4, 29);

			invoice.AH_PostDate = new ZDateTime(2020, 3, 15);

			//posting
			AssertEquals(4, invoice.MultiPeriodApportionmentJournals.Count);
			Factory.Save();

			var sub1CRLineDescription = @"GL CLEARING ACCOUNT
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: 6820.00.00 for period 202008
Amount Cleared To Date: GBP 36.67 AUD 66.67
Amount Yet to be Cleared: GBP 73.33 AUD 133.33";
			var sub1DRLineDescription = string.Format(@"GL Header 1
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: {0} for period 202008
Amount Cleared To Date: GBP 36.67 AUD 66.67
Amount Yet to be Cleared: GBP 73.33 AUD 133.33", gLHeader.AG_AccountNum);

			var sub2CRLineDescription = @"GL CLEARING ACCOUNT
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: 6820.00.00 for period 202009
Amount Cleared To Date: GBP 73.34 AUD 133.34
Amount Yet to be Cleared: GBP 36.66 AUD 66.66";
			var sub2DRLineDescription = string.Format(@"GL Header 1
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: {0} for period 202009
Amount Cleared To Date: GBP 73.34 AUD 133.34
Amount Yet to be Cleared: GBP 36.66 AUD 66.66", gLHeader.AG_AccountNum);

			var sub3CRLineDescription = @"GL CLEARING ACCOUNT
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: 6820.00.00 for period 202010
Amount Cleared To Date: GBP 110.00 AUD 200.00
Amount Yet to be Cleared: GBP 0.00 AUD 0.00";
			var sub3DRLineDescription = string.Format(@"GL Header 1
AP INV 001001, Organization: AALSHI
Charge: ZZCC1 GL Account: {0} for period 202010
Amount Cleared To Date: GBP 110.00 AUD 200.00
Amount Yet to be Cleared: GBP 0.00 AUD 0.00", gLHeader.AG_AccountNum);

			IEnumerable<string> actual()
			{
				var result = Factory.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLStandardJournal))
					.SelectMany(x => x.Lines.Cast<GLJournalLine>()).ToArray();

				return
					result
					.Select(x => (
						x.TransactionHeader.AH_Desc,
						x.TransactionHeader.AH_PostDate.ToShortDateString(),
						x.DebitCreditSign,
						x.IsInDatabase,
						x.AL_LineAmount,
						Math.Round(x.ExchangeRate.Rate, 2),         //round because exchange rate is calculated during PeriodApportionmentLine creation. Alternatively we can make PeriodApportionmentLine.LocalAmount calculated instead
						x.AL_RX_NKTransactionCurrency,
						x.AL_AG,
						x.AL_Desc,
						x.TransactionHeader.AH_TransactionBelongsToGroup).ToString());
			}

			var expected = new[]
			{
				("MASTER JOURNAL FOR AP INV 001001", "31-Mar-20",        "CR",   true,   -200,   0.55m, "GBP",       gLHeader.PK,            "MASTER JOURNAL FOR AP INV 001001", invoice.PK).ToString(),
				("MASTER JOURNAL FOR AP INV 001001", "31-Mar-20",        "DR",   true,   200,    0.55m, "GBP",       clearingAccount.PK,     "MASTER JOURNAL FOR AP INV 001001", invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                   "29-Feb-20",        "CR",   true,   -66.67, 0.55m, "GBP",       clearingAccount.PK,     sub1CRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                   "29-Feb-20",        "DR",   true,   66.67,  0.55m, "GBP",       gLHeader.PK,            sub1DRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                   "31-Mar-20",        "CR",   true,   -66.67, 0.55m, "GBP",       clearingAccount.PK,     sub2CRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                   "31-Mar-20",        "DR",   true,   66.67,  0.55m, "GBP",       gLHeader.PK,            sub2DRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202010",                   "30-Apr-20",        "CR",   true,   -66.66, 0.55m, "GBP",       clearingAccount.PK,     sub3CRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202010",                   "30-Apr-20",        "DR",   true,   66.66,  0.55m, "GBP",       gLHeader.PK,            sub3DRLineDescription, invoice.PK).ToString(),
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());

			//reversing
			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(invoice);
			reversing.Reverse();
			reversing.ReverseTransaction.TransactionNumber = "555";

			Assert("We need to hide multi-period apportionment functionality from reverse transaction screen", !(reversing.ReverseTransaction as InvoicingBase).SupportMultiPeriodApportionment);

			Factory.Save();

			var reversalInvoice = invoice.ReverseInvoice;

			expected = new[]
			{
				//original
				("MASTER JOURNAL FOR AP INV 001001",             "31-Mar-20",        "CR",   true,   -200,       0.55m, "GBP",       gLHeader.PK,            "MASTER JOURNAL FOR AP INV 001001", invoice.PK).ToString(),
				("MASTER JOURNAL FOR AP INV 001001",             "31-Mar-20",        "DR",   true,   200,        0.55m, "GBP",       clearingAccount.PK,     "MASTER JOURNAL FOR AP INV 001001", invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                               "29-Feb-20",        "CR",   true,   -66.67,     0.55m, "GBP",       clearingAccount.PK,     sub1CRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                               "29-Feb-20",        "DR",   true,   66.67,      0.55m, "GBP",       gLHeader.PK,            sub1DRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                               "31-Mar-20",        "CR",   true,   -66.67,     0.55m, "GBP",       clearingAccount.PK,     sub2CRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                               "31-Mar-20",        "DR",   true,   66.67,      0.55m, "GBP",       gLHeader.PK,            sub2DRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202010",                               "30-Apr-20",        "CR",   true,   -66.66,     0.55m, "GBP",       clearingAccount.PK,     sub3CRLineDescription, invoice.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202010",                               "30-Apr-20",        "DR",   true,   66.66,      0.55m, "GBP",        gLHeader.PK,            sub3DRLineDescription, invoice.PK).ToString(),
				//reverse
				("(REVERSE) MASTER JOURNAL FOR AP INV 001001",   "31-Mar-20",        "DR",   true,   200,        0.55m, "GBP",       gLHeader.PK,            "(REVERSE) MASTER JOURNAL FOR AP INV 001001", reversalInvoice.PK).ToString(),
				("(REVERSE) MASTER JOURNAL FOR AP INV 001001",   "31-Mar-20",        "CR",   true,   -200,       0.55m, "GBP",       clearingAccount.PK,     "(REVERSE) MASTER JOURNAL FOR AP INV 001001", reversalInvoice.PK).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202008",                     "29-Feb-20",        "DR",   true,   66.67,      0.55m, "GBP",       clearingAccount.PK,     "(REVERSE) " + sub1CRLineDescription, reversalInvoice.PK).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202008",                     "29-Feb-20",        "CR",   true,   -66.67,     0.55m, "GBP",       gLHeader.PK,            "(REVERSE) " + sub1DRLineDescription, reversalInvoice.PK).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202009",                     "31-Mar-20",        "DR",   true,   66.67,      0.55m, "GBP",       clearingAccount.PK,     "(REVERSE) " + sub2CRLineDescription, reversalInvoice.PK).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202009",                     "31-Mar-20",        "CR",   true,   -66.67,     0.55m, "GBP",       gLHeader.PK,            "(REVERSE) " + sub2DRLineDescription, reversalInvoice.PK).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202010",                     "30-Apr-20",        "DR",   true,   66.66,      0.55m, "GBP",       clearingAccount.PK,     "(REVERSE) " + sub3CRLineDescription, reversalInvoice.PK).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202010",                     "30-Apr-20",        "CR",   true,   -66.66,     0.55m, "GBP",        gLHeader.PK,           "(REVERSE) " + sub3DRLineDescription, reversalInvoice.PK).ToString(),
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());
		}

		[TestDate(2020, 2, 21)]
		public void TestPeriodApportionment_PostPaymentScenario_AdjustPostPeriodIfGlLedgerPeriodClosed()
		{
			var initialJournals = Factory.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLStandardJournal));
			Assert("Precondition", !initialJournals.Any());

			var testObjectCreator = new TestObjectCreator(Factory);
			var gLHeader = testObjectCreator.GLHeader1;
			var clearingAccount = testObjectCreator.GLJournalClearingAccount;

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);

			var shipment = testObjectCreator.CreateShipment("S001001");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0M, testObjectCreator.Agent, 0M);
			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), "001001", testObjectCreator.AUD, 1M, testObjectCreator.AALSHI);
			var line = testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.AUD, 1M, 200M);
			line.AL_AC = testObjectCreator.CC1.PK;
			line.AL_JH = job.PK;

			gLHeader.AG_Description = "GL Header 1";
			line.AL_AG = gLHeader.PK;

			var jobCharge = testObjectCreator.CreateCharge(line, job, testObjectCreator.CC1, testObjectCreator.AUD);
			jobCharge.JR_AT_CostGSTRate = testObjectCreator.GST1.PK;
			line.AL_AT = testObjectCreator.GST1.PK;
			line.PeriodApportionmentMethod = "PER";
			line.PeriodClearingGLAccountPK = clearingAccount.PK;
			line.PeriodStartDate = new ZDate(2020, 1, 15);
			line.PeriodEndDate = new ZDate(2020, 3, 29);

			invoice.AH_PostDate = new ZDateTime(2020, 2, 15);

			//posting
			AssertEquals(4, invoice.MultiPeriodApportionmentJournals.Count);
			Factory.Save();

			IEnumerable<string> actual()
			{
				var result = Factory.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLStandardJournal))
					.SelectMany(x => x.Lines.Cast<GLJournalLine>()).ToArray();

				return
					result
					.Select(x => (
						x.TransactionHeader.AH_Desc,
						x.TransactionHeader.AH_PostDate.ToShortDateString(),
						x.DebitCreditSign,
						x.IsInDatabase,
						x.AL_LineAmount,
						x.AL_AG).ToString());
			}

			var expected = new[]
			{
				("MASTER JOURNAL FOR AP INV 001001", "29-Feb-20",        "CR",   true,   -200,       gLHeader.PK).ToString(),
				("MASTER JOURNAL FOR AP INV 001001", "29-Feb-20",        "DR",   true,   200,        clearingAccount.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202007",                   "31-Jan-20",        "CR",   true,   -66.67,     clearingAccount.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202007",                   "31-Jan-20",        "DR",   true,   66.67,      gLHeader.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                   "29-Feb-20",        "CR",   true,   -66.67,     clearingAccount.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                   "29-Feb-20",        "DR",   true,   66.67,      gLHeader.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                   "31-Mar-20",        "CR",   true,   -66.66,     clearingAccount.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                   "31-Mar-20",        "DR",   true,   66.66,      gLHeader.PK).ToString(),
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());

			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var currentPeriod = periodCalculator.GetPeriodFromDate(ZDateTime.Today);
			var prevPeriod = periodCalculator.GetPreviousPeriod(currentPeriod);
			closeGLOnly(prevPeriod);
			closeGLOnly(currentPeriod);

			//reversing
			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(invoice);

			reversing.Reverse();            //error should be triggered here

			reversing.ReverseTransaction.TransactionNumber = "555";
			InvoicingBase creditNote = (reversing.ReverseTransaction as InvoicingBase);

			Assert("expect all journals can be reversed", creditNote.RelatedApportionmentReversings.All(x => x.CanReverseTransaction));
			Factory.Save();

			expected = new[]
			{
				//original
				("MASTER JOURNAL FOR AP INV 001001", "29-Feb-20",        "CR",   true,   -200,       gLHeader.PK).ToString(),
				("MASTER JOURNAL FOR AP INV 001001", "29-Feb-20",        "DR",   true,   200,        clearingAccount.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202007",                   "31-Jan-20",        "CR",   true,   -66.67,     clearingAccount.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202007",                   "31-Jan-20",        "DR",   true,   66.67,      gLHeader.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                   "29-Feb-20",        "CR",   true,   -66.67,     clearingAccount.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202008",                   "29-Feb-20",        "DR",   true,   66.67,      gLHeader.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                   "31-Mar-20",        "CR",   true,   -66.66,     clearingAccount.PK).ToString(),
				("Sub Journal For AP INV 001001 Period 202009",                   "31-Mar-20",        "DR",   true,   66.66,      gLHeader.PK).ToString(),
				//reverse
				("(REVERSE) MASTER JOURNAL FOR AP INV 001001", "31-Mar-20",        "DR",   true,   200,       gLHeader.PK).ToString(),
				("(REVERSE) MASTER JOURNAL FOR AP INV 001001", "31-Mar-20",        "CR",   true,   -200,        clearingAccount.PK).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202007",                   "31-Mar-20",        "DR",   true,   66.67,     clearingAccount.PK).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202007",                   "31-Mar-20",        "CR",   true,   -66.67,      gLHeader.PK).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202008",                   "31-Mar-20",        "DR",   true,   66.67,     clearingAccount.PK).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202008",                   "31-Mar-20",        "CR",   true,   -66.67,      gLHeader.PK).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202009",                   "31-Mar-20",        "DR",   true,   66.66,     clearingAccount.PK).ToString(),
				("(REVERSE) Sub Journal For AP INV 001001 Period 202009",                   "31-Mar-20",        "CR",   true,   -66.66,      gLHeader.PK).ToString()
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());

			AssertNoErrors(creditNote.AH_PostDateInfo);
			creditNote.AH_PostDate = new ZDateTime(2020, 1, 15);
			creditNote.Validation.ValidateAH_PostDate();
			AssertNoErrors(creditNote.AH_PostDateInfo);
		}

		void closeGLOnly(int period)
		{
			var periods = Factory.Load<AccPeriodManagement>(new ZQuery(AccPeriodManagementSchema.AM_Period, period));
			AssertEquals("precondition", 1, periods.Length);

			var periodObj = periods.First();
			periodObj.AM_IsGeneralLedgerClosed = true;
			Factory.Save();
		}

		protected AccPeriodManagement SetupPeriod(BusinessObjectFactory factory, ZBool isGLPeriodClosed, ZBool isSubLedgerPeriodClosed, ZInt period, ZDateTime startDate, ZDateTime endDate)
		{
			AccPeriodManagement result = factory.New<AccPeriodManagement>();

			result.AM_IsGeneralLedgerClosed = isGLPeriodClosed;
			result.AM_IsSubLedgerClosed = isSubLedgerPeriodClosed;
			result.AM_StartDate = startDate;
			result.AM_EndDate = endDate;
			result.AM_Period = period;
			result.AM_Year = Convert.ToInt16(period / 100);
			result.AM_GC_Company = Env.CurrentCompany.PK;
			return result;
		}
	}
}
