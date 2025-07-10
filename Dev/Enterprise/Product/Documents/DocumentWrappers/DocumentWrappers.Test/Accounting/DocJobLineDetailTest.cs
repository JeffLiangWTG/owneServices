using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Moq;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocJobLineDetailTest : TestCaseWithFactory
	{
		public void TestValuesForTaxExpense()
		{
			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);

			var invoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "10001000", ObjectCreator.AUD, 2.5M, 100M, 10M, 25M, 1.2M);
			var line = invoice.Lines[0];
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AL_ARLine = line.PK;
			charge.SetAmountsFromLinkedLinesForTests();

			Factory.Save();

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);
			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, line.PK)).Returns(new[] { (ZDate.Today, new ZDecimal(8.24m)) });

			var lineWrapper = DocJobLineDetail.New(line, Factory);
			lineWrapper.IsTaxExpense = true;
			AssertEquals(ZString.Empty, lineWrapper.OSValueAsString);
			AssertEquals(8.24m, lineWrapper.LocalValue);
			AssertEquals("Tax Expense", lineWrapper.ChargeCodeDescription);
		}

		public void TestReverseDate()
		{
			(Line as Enterprise.Accounting.Business.WIPAccrual.BaseWIPAccrual).RelatedJobCharge.ReverseAccrual(ZDateTime.BrettsBirthday);

			AssertEquals("Line ReverseDate", ZDateTime.BrettsBirthday, LineWrapper.ReverseDate);
		}

		public void TestRecognizeDate()
		{
			Line.AL_RevRecognitionType = "ARV";
			Line.AL_LineAmount = 100;
			Line.AL_GC = Job1.JH_GC;

			TransactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			TransactionHeader.AH_TransactionType = TransactionTypes.Invoice;
			TransactionHeader.AH_InvoiceDate = DateTime.Now;
			TransactionHeader.AH_GE = GlbDepartment.CurrentDepartment.PK;
			TransactionHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			TransactionHeader.AH_InvoiceAmount = 100;
			TransactionHeader.AH_OutstandingAmount = 100;
			TransactionHeader.AH_TransactionNum = "TEST123456";

			Charge charge = Job1.Charges.AddNew();
			charge.JR_AL_ARLine = Line.PK;
			charge.JR_LocalCostAmt = 100;
			charge.JR_AC = ObjectCreator.FRT.PK;

			JobChargeRevRecognition revRec1 = Factory.NewWithValidTestData<JobChargeRevRecognition>();
			revRec1.D3_JH = Line.AL_JH;
			revRec1.D3_RecognitionDate = AccountingConstants.RevenueRecognitionDateConstants.CustomsClearanceDate;
			revRec1.D3_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			Factory.Save();

			AssertEquals("Line RecognizeDate", AccountingConstants.RevenueRecognitionDateConstants.CustomsClearanceDate, LineWrapper.RecognizeDate);
		}

		public void TestLineType()
		{
			Line.AL_LineType = "ABC";
			AssertEquals("Line Type", "ABC", LineWrapper.LineType);
		}

		public void TestBranch()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "BR1";
			Line.AL_GB = branch.PK;
			AssertEquals("Branch", "BR1", LineWrapper.Branch);
		}

		public void TestDepartment()
		{
			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "DP1";
			ARInvoice header = Line.Factory.New<ARInvoice>();
			Line.AL_AH = header.PK;
			Line.AL_GE = department.PK;
			AssertEquals("Department", "DP1", LineWrapper.Department);
		}

		public void TestRevenue()
		{
			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			Line.AL_LineAmount = 100M;
			AssertEquals("Revenue Amount for 'REV' Line", 100M, LineWrapper.Revenue);

			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			AssertEquals("Revenue Amount for 'WIP' Line", 0M, LineWrapper.Revenue);
		}

		public void TestWip()
		{
			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			Line.AL_LineAmount = -100M;
			AssertEquals("WIP Amount for 'WIP' Line", 100M, LineWrapper.WIP);

			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			AssertEquals("WIP Amount for 'REV' Line", 0M, LineWrapper.WIP);
		}

		public void TestCost()
		{
			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			Line.AL_LineAmount = -100M;
			AssertEquals("Cost Amount for 'CST' Line", 100M, LineWrapper.Cost);

			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			AssertEquals("Cost Amount for 'ACR' Line", 0M, LineWrapper.Cost);

			var jobRevenueJournal = ObjectCreator.CreateJobRevenueJournal(ObjectCreator.CC1, ObjectCreator.Job1, 100M);
			LineWrapper = DocJobLineDetail.New(jobRevenueJournal.Lines[0], Factory);
			AssertEquals("Cost Amount for Job Revenue Journal Line", 100M, LineWrapper.Cost);
		}

		public void TestAccrual()
		{
			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			Line.AL_LineAmount = 100M;
			AssertEquals("Accrual Amount for 'ACR' Line", 100M, LineWrapper.Accrual);

			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			AssertEquals("Accrual Amount for 'CST' Line", 0M, LineWrapper.Accrual);
		}

		public void TestIncome()
		{
			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			Line.AL_LineAmount = 100M;
			AssertEquals("Income Amount for 'REV' Line", 100M, LineWrapper.Income);

			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			Line.AL_LineAmount = -100M;
			AssertEquals("Revenue Amount for 'WIP' Line", 100M, LineWrapper.Income);

			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			Line.AL_LineAmount = -100M;
			AssertEquals("Revenue Amount for 'CST' Line", 0M, LineWrapper.Income);

			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			Line.AL_LineAmount = 100M;
			AssertEquals("Revenue Amount for 'ACR' Line", 0M, LineWrapper.Income);
		}

		public void TestExpense()
		{
			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			Line.AL_LineAmount = 100M;
			AssertEquals("Expense Amount for 'REV' Line", 0M, LineWrapper.Expense);

			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			Line.AL_LineAmount = -100M;
			AssertEquals("Expense Amount for 'WIP' Line", 0M, LineWrapper.Expense);

			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			Line.AL_LineAmount = -100M;
			AssertEquals("Expense Amount for 'CST' Line", -100M, LineWrapper.Expense);

			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			Line.AL_LineAmount = 100M;
			AssertEquals("Expense Amount for 'ACR' Line", -100M, LineWrapper.Expense);
		}

		public void TestProfit()
		{
			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			Line.AL_LineAmount = 100M;
			AssertEquals("Profit Amount for 'REV' Line", 100M, LineWrapper.Profit);

			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			Line.AL_LineAmount = -100M;
			AssertEquals("Profit Amount for 'WIP' Line", 100M, LineWrapper.Profit);

			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			Line.AL_LineAmount = -100M;
			AssertEquals("Profit Amount for 'CST' Line", -100M, LineWrapper.Profit);

			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			Line.AL_LineAmount = 100M;
			AssertEquals("Profit Amount for 'ACR' Line", -100M, LineWrapper.Profit);
		}

		public void TestOSValue()
		{
			Line.AL_RX_NKTransactionCurrency = ObjectCreator.AUD.RX_Code;
			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			Line.AL_LineAmount = 100M;
			AssertEquals("OS Value ", 100.00M, LineWrapper.OSValue);
			AssertEquals("OS Value as String", "AUD 100.00", LineWrapper.OSValueAsString);

			Line.AL_RX_NKTransactionCurrency = ObjectCreator.USD.RX_Code;
			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			Line.AL_OSAmount = 81m;
			Line.AL_GSTVAT = 0m;
			AssertEquals("OS Value ", 81.0m, LineWrapper.OSValue);
			AssertEquals("OS Value as String", "USD 81.00", LineWrapper.OSValueAsString);

			Line.AL_ExchangeRate = 0.81m;
			Line.AL_GSTVAT = 10m;
			AssertEquals("OS Value ", 81.0m, LineWrapper.OSValue);
			AssertEquals("OS Value as String", "USD 81.00", LineWrapper.OSValueAsString);
		}

		public void TestLocalValue()
		{
			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			Line.AL_LineAmount = 100M;
			AssertEquals("Local Value for 'REV' Line", 100M, LineWrapper.LocalValue);

			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			Line.AL_LineAmount = -100M;
			AssertEquals("Local Value for 'CST' Line", -100M, LineWrapper.LocalValue);
		}

		public void TestExchangeRate()
		{
			ZGuid oldHeader = Line.AL_AH;
			try
			{
				Line.AL_AH = ZGuid.Empty;
				Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
				Line.AL_LineAmount = 100M;
				Line.AL_ExchangeRate = 1.234M;
				AssertEquals("Exchange Rate", 1.234M, LineWrapper.ExchangeRate);

				ARInvoice header = Line.Factory.New<ARInvoice>();
				header.AH_ExchangeRate = 1.233M;
				Line.AL_AH = header.PK;
				AssertEquals("Exchange Rate", 1.233M, LineWrapper.ExchangeRate);
			}
			finally
			{
				Line.AL_AH = oldHeader;
			}
		}

		public void TestLineExchangeRate()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			var jobRevenueJournal = ObjectCreator.CreateJobRevenueJournal(ObjectCreator.CC1, job, 500M);
			var journalLine = jobRevenueJournal.JournalLines[0];
			journalLine.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			journalLine.AL_ExchangeRate = 0.7M;

			var lineWrapper = DocJobLineDetail.New(journalLine, Factory);
			AssertEquals(1M, journalLine.TransactionHeader.AH_ExchangeRate);
			AssertEquals(0.7M, journalLine.AL_ExchangeRate);
			AssertEquals("JRJ Line Exchange Rate", journalLine.AL_ExchangeRate, lineWrapper.LineExchangeRate);
		}

		public void TestChargeCode()
		{
			ARInvoice header = Line.Factory.New<ARInvoice>();
			Line.AL_AH = header.PK;
			Line.AL_AC = ZGuid.Empty;
			AssertNull("Charge Code should be null", LineWrapper.ChargeCode);
			AssertEquals("Charge Code PK", ZGuid.Empty, LineWrapper.ChargeCodePK);

			Line.AL_AC = ObjectCreator.CC1.PK;
			AssertNotNull("Charge Code should not be null", LineWrapper.ChargeCode);
			AssertEquals("Charge Code", ObjectCreator.CC1.AC_Code, LineWrapper.ChargeCode.Code);
			AssertEquals("Charge Code PK", ObjectCreator.CC1.PK, LineWrapper.ChargeCodePK);
		}

		public void TestOrganisation()
		{
			Line.AL_LineType = "REV";
			AssertNotNull("Organisation should not be null", LineWrapper.Organisation);
			AssertEquals("Organisation Code", ObjectCreator.ABIGAS.OH_Code, LineWrapper.Organisation.Code);

			Line.AL_LineType = "WIP";
			Line.AL_AH = ZGuid.Empty;
			Line.AL_OH = ZGuid.Empty;
			AssertNull("Organsation Code", LineWrapper.Organisation);

			ErrorReporter.Clear();
			Line.AL_OH = ObjectCreator.AALSHI.PK;
			AssertNotNull("Organisation should not be null", LineWrapper.Organisation);
			AssertEquals("Organisation Code", ObjectCreator.AALSHI.OH_Code, LineWrapper.Organisation.Code);
			AssertEquals("Silent exception must not be sent for not CST or REV lines.", string.Empty, ErrorReporter.LastMessageReported);

			Line.AL_PostDate = new ZDateTime(2009, 11, 20);
			Line.AL_LineType = "REV";
			try
			{
				DocOrganisation dummyOrg = LineWrapper.Organisation;
				Assert("Exception must be raised to notify a user that data is incorrect.", false);
			}
			catch (InvalidOperationException ex)
			{
				string expectedMessage = string.Format(@"Db data is not correct. Cost and Revenue Transaction Lines must have Transaction Header.
Line: PK = {2}, Charge Code = ZZCC1, GL Account = {0}, Type = REV, OS Amount = 100, Local Amount = 100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = ERN, Post Date = 20-Nov-09 00:00:00, Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = 00000000-0000-0000-0000-000000000000, Job PK = {1}, Organization = AALSHI, Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.",
					Line.GLHeader.AccountNum, Line.Job.PK, Line.PK);
				AssertEquals("Silent exception must be sent.", expectedMessage, ErrorReporter.LastMessageReported);
				AssertEquals("Silent exception must be sent.", ex, ErrorReporter.LastExceptionReported);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestTransactionHeader()
		{
			TransactionHeader invoice = ObjectCreator.CreateInvoice(typeof(ARInvoice), ObjectCreator.AUD, 1.0M);
			invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			invoice.AH_TransactionNum = "123456789";
			Line.AL_AH = invoice.PK;
			AssertNotNull("Transaction Header should not be null", LineWrapper.TransactionHeader);
			AssertEquals("Transaction Header Type", invoice.AH_TransactionType, LineWrapper.TransactionHeader.TransactionType);
			AssertEquals("Transaction Header Number", invoice.AH_TransactionNum, LineWrapper.TransactionHeader.TransactionNum);

			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			Line.AL_AH = ZGuid.Empty;
			ErrorReporter.Clear();
			AssertNull("Transaction Header should be null", LineWrapper.TransactionHeader);
			AssertEquals("Silent exception must not be sent for not CST or REV lines.", string.Empty, ErrorReporter.LastMessageReported);

			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			DocTransactionHeader result;
			AssertThrowInvalidOperationException(() => result = LineWrapper.TransactionHeader);
		}

		public void TestTransactionHeaderOSTotal()
		{
			TransactionHeader invoice = ObjectCreator.CreateInvoice(typeof(ARInvoice), ObjectCreator.AUD, 1.0M);
			invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			invoice.AH_TransactionNum = "123456789";
			Line.AL_AH = invoice.PK;
			AssertEquals("Transaction Header Type", invoice.AH_TransactionType, LineWrapper.TransactionHeader.TransactionType);

			invoice.AH_OSTotal = 500M;
			AssertEquals("", 500M, LineWrapper.ARTransactionHeaderOSTotal);
			AssertEquals("", -500M, LineWrapper.APTransactionHeaderOSTotal);

			invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.CreditNote;
			AssertEquals("", 500M, LineWrapper.ARTransactionHeaderOSTotal);
			AssertEquals("", -500M, LineWrapper.APTransactionHeaderOSTotal);

			Line.AL_AH = ZGuid.Empty;
			ZDecimal result;
			AssertNotEquals(new string[] { TransactionLineTypes.Cost, TransactionLineTypes.Revenue }, Line.AL_LineType);
			result = LineWrapper.APTransactionHeaderOSTotal;
			result = LineWrapper.ARTransactionHeaderOSTotal;

			Line.AL_LineType = TransactionLineTypes.Revenue;
			AssertThrowInvalidOperationException(() => result = LineWrapper.APTransactionHeaderOSTotal);
			AssertThrowInvalidOperationException(() => result = LineWrapper.ARTransactionHeaderOSTotal);
		}

		void AssertThrowInvalidOperationException(Action action)
		{
			try
			{
				action.Invoke();
				Assert("Exception must be raised to notify a user that data is incorrect.", false);
			}
			catch (InvalidOperationException ex)
			{
				string expectedMessage = @"Db data is not correct. Cost and Revenue Transaction Lines must have Transaction Header.
Line: PK";
				AssertContains("Silent exception must be sent.", expectedMessage, ErrorReporter.LastMessageReported);
				AssertEquals("Silent exception must be sent.", ex, ErrorReporter.LastExceptionReported);
				ExceptionReporterTestListener.Instance.Clear();
				ErrorReporter.Clear();
			}
		}

		public void TestJob()
		{
			ARInvoice header = Line.Factory.New<ARInvoice>();
			Line.AL_AH = header.PK;
			Line.AL_JH = ZGuid.Empty;
			AssertNull("Job should be null", LineWrapper.Job);
			AssertEquals("Job PK", ZGuid.Empty, LineWrapper.JobPK);

			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Line.AL_JH = job.PK;
			AssertNotNull("Job should not be null", LineWrapper.Job);
			AssertEquals("Job PK", job.PK, LineWrapper.JobPK);
		}

		public void TestCurrency()
		{
			Line.AL_RX_NKTransactionCurrency = ObjectCreator.AUD.RX_Code;
			AssertNotNull("Currency should not be null", LineWrapper.Currency);

			Line.AL_RX_NKTransactionCurrency = ZString.Empty;
			AssertNull("Currency should be null", LineWrapper.Currency);
		}

		public void TestJobPK()
		{
			AssertEquals("Job PK", Job1.PK, LineWrapper.JobPK);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ObjectCreator = new TestObjectCreator(Factory);
			Job1 = ObjectCreator.Job1;
			Line = ObjectCreator.CreateAccrual(Job1, ObjectCreator.CC1, 1.0M, "Description", 100M);
			LineWrapper = DocJobLineDetail.New(Line, Factory);
			TransactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			TransactionHeader.AH_OH = ObjectCreator.ABIGAS.PK;
			Line.AL_AH = TransactionHeader.PK;
		}

		Job Job1;
		TestObjectCreator ObjectCreator;
		AccTransactionLines Line;
		DocJobLineDetail LineWrapper;
		AccTransactionHeader TransactionHeader;
	}
}
