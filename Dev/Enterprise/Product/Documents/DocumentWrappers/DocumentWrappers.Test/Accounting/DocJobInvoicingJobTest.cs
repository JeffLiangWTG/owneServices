using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.DocumentWrappers.DocJobInvoicingJob;

namespace Enterprise.DocumentWrappers
{
	[TestedType(typeof(DocJobInvoicingJob))]
	sealed class DocJobInvoicingJobTest : DocumentWrapperTestCase
	{
		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			Factory.SetContext(BusinessContext.JobCreatedFromJobLoader);
			var result = base.CreateDocumentWrapperFromStaticNewMethod();
			Factory.RemoveContext(BusinessContext.JobCreatedFromJobLoader);
			return result;
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			Job job = ObjectCreator.CreateJob("Sx0001000", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);

			return new DocumentWrapper[] { DocJobInvoicingJob.New(job, Factory), DocJobInvoicingJob.New(job.Factory, job.PK) };
		}

		public void TestExchangeRatesCodeSorted()
		{
			var job = Factory.NewJobForTesting<Job>();
			ExchangeRate rate1 = job.ExchangeRates.AddNew();
			ExchangeRate rate2 = job.ExchangeRates.AddNew();
			rate1.JF_RX_NKRateCurrency = "USD";
			rate2.JF_RX_NKRateCurrency = "AUD";

			DocJobInvoicingJob doc = DocJobInvoicingJob.New(job, Factory);
			AssertEquals("AUD first", "AUD", doc.ExchangeRatesCodeSorted[0].Currency.Code);
			AssertEquals("USD first", "USD", doc.ExchangeRatesCodeSorted[1].Currency.Code);
		}

		public void TestChargesDueAgentOrProfitShared()
		{
			OrgHeader agent = Factory.NewWithValidTestData<OrgHeader>();
			Job job = Factory.NewJobForTesting<Job>();
			job.AgentCollectPK = agent.PK;

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode.Value;
			charge1.JR_OH_CostAccount = agent.PK;

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = Env.Registry.FreightChargeCode;
			charge2.JR_OH_CostAccount = agent.PK;

			Charge charge3 = job.Charges.AddNew();
			charge3.JR_AC = Env.Registry.FreightChargeCode;

			Charge charge4 = job.Charges.AddNew();
			charge4.JR_AC = Env.Registry.FreightChargeCode;
			charge4.JR_IsIncludedInProfitShare = true;

			DocJobInvoicingJob doc = DocJobInvoicingJob.New(job, Factory);
			AssertEquals("Should only contain Profit Shared and Agent Charge", 2, doc.ChargesDueAgentOrProfitShared.Count);
			AssertEquals(charge2.PK, doc.ChargesDueAgentOrProfitShared[0].ChargePK);
			AssertEquals(charge4.PK, doc.ChargesDueAgentOrProfitShared[1].ChargePK);
		}

		public void TestChargesNotIncludingCustomsDisbursement()
		{
			var job = Factory.NewJobForTesting<Job>();
			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;
			Charge charge2 = job.Charges.AddNew();
			ZQuery fRTChargeCodeFilter = new ZQuery(AccChargeCodeSchema.AC_Code, "FRT");
			fRTChargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			charge2.JR_AC = Factory.LoadTop1(typeof(AccChargeCode), fRTChargeCodeFilter).PK;

			DocJobInvoicingJob doc = DocJobInvoicingJob.New(job, Factory);
			AssertEquals("Should filter out the customs disbursement", 1, doc.ChargesNotIncludingCustomsDisbursement.Count);
			AssertEquals("The only one available should be freight", "FRT", doc.ChargesNotIncludingCustomsDisbursement[0].ChargeCode.Code);
		}

		public void TestChargesNotIncludingCustomsDisbursementOrZeroCharges()
		{
			var job = Factory.NewJobForTesting<Job>();
			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;
			charge1.JR_OSSellAmt = 1;
			Charge charge2 = job.Charges.AddNew();
			ZQuery fRTChargeCodeFilter = new ZQuery(AccChargeCodeSchema.AC_Code, "FRT");
			fRTChargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			ZGuid fRTChargeCodePK = Factory.LoadTop1(typeof(AccChargeCode), fRTChargeCodeFilter).PK;
			charge2.JR_AC = fRTChargeCodePK;
			charge2.JR_OSSellAmt = 1;
			Charge charge3 = job.Charges.AddNew();
			charge3.JR_AC = fRTChargeCodePK;
			charge3.JR_OSSellAmt = 0;

			DocJobInvoicingJob doc = DocJobInvoicingJob.New(job, Factory);
			AssertEquals("Should filter out the customs disbursement and the zero'd charge", 1, doc.ChargesNotIncludingCustomsDisbursementOrZeroCharges.Count);
			AssertEquals("The only one available should be freight", "FRT", doc.ChargesNotIncludingCustomsDisbursementOrZeroCharges[0].ChargeCode.Code);
		}

		public void TestJH_TotalRevenue()
		{
			Job.Charges.RemoveAll();
			AssertEquals("Precondition: Should not have charges", 0, Job.Charges.Count);
			AssertEquals(0m, Job.JH_TotalRevenue);

			AddCharge(Job, 21.3m, 11.3m);
			AddCharge(Job, 12.5m, 10m);
			AssertEquals("Precondition: Should have two charges", 2, Job.Charges.Count);
			AssertEquals(33.8m, Job.JH_TotalRevenue);

			AddCharge(Job, 10m, 26m);
			AddCharge(Job, 13.14m, 20.14m);
			AssertEquals("Precondition: Should have four charges", 4, Job.Charges.Count);
			AssertEquals(56.94m, Job.JH_TotalRevenue);
		}

		void AddCharge(Job job, ZDecimal sellAmount, ZDecimal costAmount)
		{
			Charge aCharge = job.Charges.AddNew();
			aCharge.JR_LocalSellAmt = sellAmount;
			JCJournalHeader cFXHeader = Factory.New<JCJournalHeader>();
			aCharge.CreateCFXTransactionLine(cFXHeader, ZDateTime.Now);
			aCharge.CFXLine.AL_LineAmount = -costAmount;
			aCharge.CFXLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
		}

		public void TestStatusDescription()
		{
			Job.JH_Status = JobHeaderStatus.Working.Code;
			ZString expectedDescription = JobHeaderStatus.Working.Description.ToUpper();
			AssertEquals("Job Status Description", expectedDescription, JobWrapper.StatusDescription);
		}

		public void TestOperator()
		{
			GlbStaff @operator = Factory.NewWithValidTestData<GlbStaff>();
			@operator.GS_Code = "123";
			Job.JH_GS_NKRepOps = @operator.GS_Code;
			AssertEquals("Operator", @operator.GS_Code, JobWrapper.Operator);
		}

		public void TestSalesRep()
		{
			var salesRep = Factory.NewWithValidTestData<GlbStaff>();
			salesRep.GS_Code = "ABC";
			Job.JH_GS_NKRepSales = salesRep.GS_Code;
			AssertEquals("Sales Rep", salesRep.GS_Code, JobWrapper.SalesRep);
		}

		public void TestJobProfitHeading()
		{
			CommonShipment shipment1 = ObjectCreator.CreateShipment("S00001234", "AUSYD", "NZAKL");
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_PackingMode = "FCL";
			Factory.Save();

			Job.JH_JobNum = "S00001234";
			Job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Job.JH_ParentID = shipment1.PK;
			Factory.Save();

			JobWrapper = DocJobInvoicingJob.New(Job, Factory);
			AssertEquals("Job Profit Heading", "SEA FCL Job Profit", JobWrapper.JobProfitHeading);
		}

		public void TestQuoteNumber()
		{
			Job.JH_TH_NKQuoteNumber = "QuoteNum";
			JobWrapper = DocJobInvoicingJob.New(Job, Factory);
			AssertEquals("Job Quote Number", "QuoteNum", JobWrapper.QuoteNumber);
		}

		public void TestLocalCLientCFX()
		{
			Job.JH_LocalChargesCFX = 5M;
			JobWrapper = DocJobInvoicingJob.New(Job, Factory);
			AssertEquals("Job Local Client CFX", Job.JH_LocalChargesCFX, JobWrapper.LocalClientCFX);
		}

		public void TestOverseasAgentCFX()
		{
			Job.JH_AgentChargesCFX = 5M;
			JobWrapper = DocJobInvoicingJob.New(Job, Factory);
			AssertEquals("Job Local Client CFX", Job.JH_AgentChargesCFX, JobWrapper.OverseasAgentCFX);
		}

		public void TestJobExchangeRatesAsString()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			ExchangeRate rate1 = Job.ExchangeRates.AddNew();
			ExchangeRate rate2 = Job.ExchangeRates.AddNew();
			rate1.JF_RX_NKRateCurrency = "USD";
			rate1.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			rate1.JF_OH_Org = TestObjectCreator.AALSHI.PK;
			rate1.JF_CFXPercent = 5;
			rate1.JF_CFXMinimum = 10;
			rate1.JF_BaseRate = 0.7m;
			rate2.JF_RX_NKRateCurrency = "AUD";
			rate2.JF_BaseRate = 1.0m;

			JobWrapper = DocJobInvoicingJob.New(Job, Factory);
			ZBool originalIsReciprocal = TestObjectCreator.SetCurrentCompanyReciprocal(false);
			ZString expected = "AUD 1.000000 ALL  " + System.Environment.NewLine;
			expected += "USD 0.700000 DEB AALSHI CFX 5% CFX Min. 10.00" + System.Environment.NewLine;
			AssertMultilineASCIIEquals("Job Exchange Rates as a single string", expected, JobWrapper.JobExchangeRatesAsString);

			TestObjectCreator.SetCurrentCompanyReciprocal(true);
			expected = "AUD 1.000000 ALL  " + System.Environment.NewLine;
			expected += "USD 0.700000 DEB AALSHI CFX 5% CFX Min. 10.00" + System.Environment.NewLine;
			AssertMultilineASCIIEquals("Job Exchange Rates as a single string", expected, JobWrapper.JobExchangeRatesAsString);

			TestObjectCreator.SetCurrentCompanyReciprocal(originalIsReciprocal);
		}

		public void TestPrintChargeSummary()
		{
			JobDocPrinter.PrintChargeSummary = false;
			AssertEquals("JobDocPrinter.PrintChargeSummary", false, JobDocPrinter.PrintChargeSummary);
			JobDocPrinter.PrintChargeSummary = true;
			AssertEquals("JobDocPrinter.PrintChargeSummary", true, JobDocPrinter.PrintChargeSummary);
			JobDocPrinter.PrintChargeSummary = false;
			AssertEquals("JobDocPrinter.PrintChargeSummary", false, JobDocPrinter.PrintChargeSummary);
		}

		public void TestPrintChargeDetail()
		{
			JobDocPrinter.PrintChargeDetail = false;
			AssertEquals("JobDocPrinter.PrintChargeDetail", false, JobDocPrinter.PrintChargeDetail);
			JobDocPrinter.PrintChargeDetail = true;
			AssertEquals("JobDocPrinter.PrintChargeDetail", true, JobDocPrinter.PrintChargeDetail);
			JobDocPrinter.PrintChargeDetail = false;
			AssertEquals("JobDocPrinter.PrintChargeDetail", false, JobDocPrinter.PrintChargeDetail);
		}

		public void TestPrintARInvoiceAnalysis()
		{
			JobDocPrinter.PrintARInvoiceAnalysis = false;
			AssertEquals("JobDocPrinter.PrintARInvoiceAnalysis", false, JobDocPrinter.PrintARInvoiceAnalysis);
			JobDocPrinter.PrintARInvoiceAnalysis = true;
			AssertEquals("JobDocPrinter.PrintARInvoiceAnalysis", true, JobDocPrinter.PrintARInvoiceAnalysis);
			JobDocPrinter.PrintARInvoiceAnalysis = false;
			AssertEquals("JobDocPrinter.PrintARInvoiceAnalysis", false, JobDocPrinter.PrintARInvoiceAnalysis);
		}

		public void TestPrintAPInvoiceAnalysis()
		{
			JobDocPrinter.PrintAPInvoiceAnalysis = false;
			AssertEquals("JobDocPrinter.PrintAPInvoiceAnalysis", false, JobDocPrinter.PrintAPInvoiceAnalysis);
			JobDocPrinter.PrintAPInvoiceAnalysis = true;
			AssertEquals("JobDocPrinter.PrintAPInvoiceAnalysis", true, JobDocPrinter.PrintAPInvoiceAnalysis);
			JobDocPrinter.PrintAPInvoiceAnalysis = false;
			AssertEquals("JobDocPrinter.PrintAPInvoiceAnalysis", false, JobDocPrinter.PrintAPInvoiceAnalysis);
		}

		public void TestPrintJobRevenueJournalAnalysis()
		{
			JobDocPrinter.PrintJobRevenueJournalAnalysis = false;
			AssertEquals("JobDocPrinter.PrintJobRevenueJournalAnalysis", false, JobDocPrinter.PrintJobRevenueJournalAnalysis);
			JobDocPrinter.PrintJobRevenueJournalAnalysis = true;
			AssertEquals("JobDocPrinter.PrintJobRevenueJournalAnalysis", true, JobDocPrinter.PrintJobRevenueJournalAnalysis);
			JobDocPrinter.PrintJobRevenueJournalAnalysis = false;
			AssertEquals("JobDocPrinter.PrintJobRevenueJournalAnalysis", false, JobDocPrinter.PrintJobRevenueJournalAnalysis);
		}

		public void TestLocalTaxTitle()
		{
			JobWrapper = New(Job, Factory);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			AssertEquals("LocalTaxTitle", "Local Value", JobWrapper.LocalTaxTitle);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			AssertEquals("LocalTaxTitle", "Local(Excl Tax)", JobWrapper.LocalTaxTitle);
		}

		public void TestRevenueMovementsRecognized()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			var line1 = ObjectCreator.CreateRevenueLineAndCharge(job.PK, 100m, "FRT");
			var line2 = ObjectCreator.CreateRevenueLineAndCharge(job.PK, 200m, "BAF");
			var jobRevenueJournal = ObjectCreator.CreateJobRevenueJournal(ObjectCreator.CC1, job, 500M);
			jobRevenueJournal.JournalLines[0].CostRevenueType = TransactionLineTypes.Revenue;
			jobRevenueJournal.JournalLines[1].CostRevenueType = TransactionLineTypes.Cost;
			Factory.Save();

			line1.AL_ReverseDate = ZDateTime.BrettsBirthday;
			line2.AL_ReverseDate = ZDateTime.BrettsBirthday;
			Factory.Save();

			job.LoadCharges_ForTestOnly();
			JobWrapper = DocJobInvoicingJob.New(job, Factory);

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, line1.PK)).Returns(new[] { (ZDate.Today.AddDays(1), new ZDecimal(8.24m)), (ZDate.Today, new ZDecimal(2.78m)) });
			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, line2.PK)).Returns(new[] { (ZDate.Today, new ZDecimal(6.32m)), (ZDate.Today.AddDays(-1), new ZDecimal(13.35m)), (ZDate.Today, new ZDecimal(10.62m)) });

			AssertEquals(4, JobWrapper.RevenueMovementsRecognized.Count);
			AssertEquals(9, JobWrapper.RevenueMovementsRecognizedForProfitShare.Count);
			AssertEquals(5, JobWrapper.RevenueMovementsRecognizedForProfitShare.Cast<DocJobInvoicingJobCharge>().Count(c => c.IsTaxExpense));
			AssertEquals(300m, JobWrapper.TotalRevenueMovementsRecognized);
		}

		public void TestCostMovementsRecognized()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			var line1 = ObjectCreator.CreateCostLineAndCharge(job.PK, 100m, "FRT");
			var line2 = ObjectCreator.CreateCostLineAndCharge(job.PK, 200m, "BAF");
			var jobRevenueJournal = ObjectCreator.CreateJobRevenueJournal(ObjectCreator.CC1, job, 500M);
			jobRevenueJournal.JournalLines[0].CostRevenueType = TransactionLineTypes.Revenue;
			jobRevenueJournal.JournalLines[1].CostRevenueType = TransactionLineTypes.Cost;
			Factory.Save();

			AssertEquals("JobRevenueJournal is Cost", true, JobWrapper.IsCostOrJobRevenueJournal(jobRevenueJournal.Lines[0]));

			line1.AL_ReverseDate = ZDateTime.BrettsBirthday;
			line2.AL_ReverseDate = ZDateTime.BrettsBirthday;
			Factory.Save();

			job.LoadCharges_ForTestOnly();
			JobWrapper = DocJobInvoicingJob.New(job, Factory);

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, line1.PK)).Returns(new[] { (ZDate.Today.AddDays(1), new ZDecimal(8.24m)), (ZDate.Today, new ZDecimal(2.78m)) });
			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, line2.PK)).Returns(new[] { (ZDate.Today, new ZDecimal(6.32m)), (ZDate.Today.AddDays(-1), new ZDecimal(13.35m)), (ZDate.Today, new ZDecimal(10.62m)) });

			AssertEquals(2, JobWrapper.CostMovementsRecognized.Count);
			AssertEquals(7, JobWrapper.CostMovementsRecognizedForProfitShare.Count);
			AssertEquals(5, JobWrapper.CostMovementsRecognizedForProfitShare.Cast<DocJobInvoicingJobCharge>().Count(c => c.IsTaxExpense));
			AssertEquals(300m, JobWrapper.TotalCostMovementsRecognized);
		}

		public void TestWIPMovementsRecognized()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			ZGuid chargeCodePK = ObjectCreator.GetChargeCodeByCode("FRT");
			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			ObjectCreator.CreateWIPLineAndCharge(job.PK, 900m, "FRT");
			Factory.Save();
			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = chargeCodePK;
			charge2.JR_LocalSellAmt = 400;
			Charge charge3 = job.Charges.AddNew();
			charge3.JR_LocalCostAmt = 600;
			charge3.JR_AC = chargeCodePK;
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals(900m, JobWrapper.TotalWIPMovementsRecognized);
		}

		public void TestACRMovementsRecognized()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			ZGuid chargeCodePK = ObjectCreator.GetChargeCodeByCode("FRT");
			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			ObjectCreator.CreateAccrualLineAndCharge(job.PK, 900m, "FRT");
			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = chargeCodePK;
			charge2.JR_LocalCostAmt = 400;
			Charge charge3 = job.Charges.AddNew();
			charge3.JR_LocalSellAmt = 600;
			charge3.JR_AC = chargeCodePK;
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals(900m, JobWrapper.TotalACRMovementsRecognized);
		}

		public void TestTotalJobProfitRecognizedInGL()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			var line1 = ObjectCreator.CreateRevenueLineAndCharge(job.PK, 300m, "FRT");
			var line2 = ObjectCreator.CreateRevenueLineAndCharge(job.PK, 500m, "BAF");
			var line3 = ObjectCreator.CreateCostLineAndCharge(job.PK, 400m, "FRT");
			var line4 = ObjectCreator.CreateCostLineAndCharge(job.PK, 600m, "BAF");
			ObjectCreator.CreateWIPLineAndCharge(job.PK, 300m, "FRT");
			ObjectCreator.CreateAccrualLineAndCharge(job.PK, 650m, "FRT");
			Factory.Save();
			line1.AL_ReverseDate = ZDateTime.BrettsBirthday;
			line2.AL_ReverseDate = ZDateTime.BrettsBirthday;
			line3.AL_ReverseDate = ZDateTime.BrettsBirthday;
			line4.AL_ReverseDate = ZDateTime.BrettsBirthday;
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals(800m, JobWrapper.TotalRevenueMovementsRecognized);
			AssertEquals(1000m, JobWrapper.TotalCostMovementsRecognized);
			AssertEquals(300m, JobWrapper.TotalWIPMovementsRecognized);
			AssertEquals(650m, JobWrapper.TotalACRMovementsRecognized);
			AssertEquals("TotalJobProfitRecognizedInGL should be TotalRev + TotalWIP - TotalCost - TotalACR", -550M, JobWrapper.TotalJobProfitRecognizedInGL);
		}

		public void TestAllLines()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);

			AssertEquals("All Lines Count", 0, JobWrapper.AllLines.Count);

			AccTransactionLines aRInvoiceLine = CreateARInvoiceLine(Factory.NewWithValidTestData<ARInvoice>(), job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AL_ARLine = aRInvoiceLine.PK;
			AccTransactionLines aPInvoiceLine = CreateAPInvoiceLine(Factory.NewWithValidTestData<APInvoice>(), job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			charge.JR_AL_APLine = aPInvoiceLine.PK;
			charge.SetAmountsFromLinkedLinesForTests();
			AccTransactionLines wip = CreateWIP(job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			AccTransactionLines accrual = CreateAccrual(job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);

			Factory.Save();

			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals("All Lines Count", 4, JobWrapper.AllLines.Count);
		}

		public void TestAllLinesWhenChildJobIsUsed()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			Job childJob = ObjectCreator.CreateJob("S00001234/E", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			childJob.JH_JH_ParentJob = job.PK;

			AssertEquals("All Lines Count", 0, JobWrapper.AllLines.Count);

			AccTransactionLines arInvoiceLineOnChild = CreateARInvoiceLine(Factory.New<ARInvoice>(), childJob, ObjectCreator.CC2, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			JobCharge chargeOnChild = Factory.NewWithValidTestData<JobCharge>();
			chargeOnChild.JR_JH = childJob.PK;
			chargeOnChild.JR_AL_ARLine = arInvoiceLineOnChild.PK;
			var apInvoiceForChild = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), "1", TestObjectCreator.AUD, 1);
			AccTransactionLines apInvoiceLineOnChild = CreateAPInvoiceLine(apInvoiceForChild, childJob, ObjectCreator.CC2, ObjectCreator.AUD, 1M, "APInvoiceLine", 100M);
			chargeOnChild.JR_AL_APLine = apInvoiceLineOnChild.PK;
			chargeOnChild.SetAmountsFromLinkedLinesForTests();
			AccTransactionLines wipOnChild = CreateWIP(childJob, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			AccTransactionLines accrualOnChild = CreateAccrual(childJob, ObjectCreator.CC1, 1M, "APInvoiceLine", 100M);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var jobWrapperF2 = DocJobInvoicingJob.New(job, factory2);
			AssertEquals("All Lines Count", 4, jobWrapperF2.AllLines.Count);

			AccTransactionLines arInvoiceLineOnParent = CreateARInvoiceLine(Factory.New<ARInvoice>(), job, ObjectCreator.CC2, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			JobCharge chargeOnParent = Factory.NewWithValidTestData<JobCharge>();
			chargeOnParent.JR_JH = job.PK;
			chargeOnParent.JR_AL_ARLine = arInvoiceLineOnParent.PK;
			chargeOnParent.JR_OH_CostAccount = ObjectCreator.ZECTRA.PK;
			var apInvoiceForParent = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), "2", TestObjectCreator.AUD, 1);
			AccTransactionLines apInvoiceLineOnParent = CreateAPInvoiceLine(apInvoiceForParent, job, ObjectCreator.CC2, ObjectCreator.AUD, 1M, "APInvoiceLine", 100M);
			chargeOnParent.JR_AL_APLine = apInvoiceLineOnParent.PK;
			chargeOnParent.SetAmountsFromLinkedLinesForTests();
			AccTransactionLines wipOnParent = CreateWIP(job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			AccTransactionLines accrualOnParent = CreateAccrual(job, ObjectCreator.CC1, 1M, "APInvoiceLine", 100M);

			Factory.Save();

			var factory3 = new BusinessObjectFactory();
			var jobWrapperF3 = DocJobInvoicingJob.New(job, factory3);
			AssertEquals("All Lines Count", 8, jobWrapperF3.AllLines.Count);
		}

		public void TestARLines()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);

			AssertEquals("AR Lines Count", 0, JobWrapper.ARLines.Count);
			AssertEquals("Display No AR Invoices Message", true, JobWrapper.DisplayNoARInvoicesMessage);

			AccTransactionLines aRInvoiceLine = CreateARInvoiceLine(Factory.NewWithValidTestData<ARInvoice>(), job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AL_ARLine = aRInvoiceLine.PK;
			AccTransactionLines aPInvoiceLine = CreateAPInvoiceLine(Factory.NewWithValidTestData<APInvoice>(), job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			charge.JR_AL_APLine = aPInvoiceLine.PK;
			charge.SetAmountsFromLinkedLinesForTests();
			AccTransactionLines wip = CreateWIP(job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			AccTransactionLines accrual = CreateAccrual(job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);

			Factory.Save();

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);
			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, aRInvoiceLine.PK)).Returns(new[] { (ZDate.Today, new ZDecimal(8.24m)) });
			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, aPInvoiceLine.PK)).Returns(new[] { (ZDate.Today.AddDays(1), new ZDecimal(12.88m)) });

			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals("AR Lines Count", 1, JobWrapper.ARLines.Count);
			AssertEquals("AR Lines for Tax Expense Count", 1, JobWrapper.ARLinesForTaxExpense.Count);
			Assert(JobWrapper.ARLinesForTaxExpense.Cast<DocJobLineDetail>().All(e => e.IsTaxExpense));
			AssertEquals("Display No AR Invoices Message", false, JobWrapper.DisplayNoARInvoicesMessage);

			aRInvoiceLine.TransactionHeader.AH_IsCancelled = true;
			Factory.ClearCachedValue<DocJobInvoicingJob>(job.PK.ToStringKey());
			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals("AR Lines Count", 0, JobWrapper.ARLines.Count);
			AssertEquals("AR Lines for Tax Expense Count", 0, JobWrapper.ARLinesForTaxExpense.Count);
		}

		public void TestARExcludeJRJLines()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);

			AssertEquals("AR Lines Count", 0, JobWrapper.ARExcludeJRJLines.Count);
			AssertEquals("Display No Exclude JRJ AR Invoices Message", true, JobWrapper.DisplayNoARExcludeJRJInvoicesMessage);

			var aRInvoiceLine = CreateARInvoiceLine(Factory.NewWithValidTestData<ARInvoice>(), job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AL_ARLine = aRInvoiceLine.PK;
			var aPInvoiceLine = CreateAPInvoiceLine(Factory.NewWithValidTestData<APInvoice>(), job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			charge.JR_AL_APLine = aPInvoiceLine.PK;
			charge.SetAmountsFromLinkedLinesForTests();
			var wip = CreateWIP(job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			var accrual = CreateAccrual(job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			var jobRevenueJournal = ObjectCreator.CreateJobRevenueJournal(ObjectCreator.CC1, job, 500M);
			jobRevenueJournal.JournalLines[0].CostRevenueType = TransactionLineTypes.Revenue;
			jobRevenueJournal.JournalLines[1].CostRevenueType = TransactionLineTypes.Cost;

			Factory.Save();

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);
			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, aRInvoiceLine.PK)).Returns(new[] { (ZDate.Today, new ZDecimal(8.24m)) });

			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals("AR Exclude JRJ Lines Count", 1, JobWrapper.ARExcludeJRJLines.Count);
			AssertEquals("AR Exclude JRJ Lines for Profit Share Count", 2, JobWrapper.ARExcludeJRJLinesForProfitShare.Count);
			AssertEquals(1, JobWrapper.ARExcludeJRJLinesForProfitShare.Cast<DocJobLineDetail>().Count(e => e.IsTaxExpense));
			AssertEquals("Display No Exclude JRJ AR Invoices Message", false, JobWrapper.DisplayNoARExcludeJRJInvoicesMessage);
		}

		public void TestAPLines()
		{
			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);

			AssertEquals("AP Lines Count", 0, JobWrapper.APLines.Count);
			AssertEquals("Display No AP Invoices Message", true, JobWrapper.DisplayNoAPInvoicesMessage);

			AccTransactionLines aRInvoiceLine = CreateARInvoiceLine(Factory.NewWithValidTestData<ARInvoice>(), job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AL_ARLine = aRInvoiceLine.PK;
			AccTransactionLines aPInvoiceLine = CreateAPInvoiceLine(Factory.NewWithValidTestData<APInvoice>(), job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "APInvoiceLine", 100M);
			charge.JR_AL_APLine = aPInvoiceLine.PK;
			charge.SetAmountsFromLinkedLinesForTests();
			AccTransactionLines wip = CreateWIP(job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			AccTransactionLines accrual = CreateAccrual(job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);

			Factory.Save();

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);
			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, aRInvoiceLine.PK)).Returns(new[] { (ZDate.Today, new ZDecimal(8.24m)) });
			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, aPInvoiceLine.PK)).Returns(new[] { (ZDate.Today.AddDays(1), new ZDecimal(12.88m)) });

			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals("AP Lines Count", 1, JobWrapper.APLines.Count);
			AssertEquals("AP Lines for Tax Expense Count", 1, JobWrapper.APLinesForTaxExpense.Count);
			Assert(JobWrapper.APLinesForTaxExpense.Cast<DocJobLineDetail>().All(e => e.IsTaxExpense));
			AssertEquals("Display No AP Invoices Message", false, JobWrapper.DisplayNoAPInvoicesMessage);

			aPInvoiceLine.TransactionHeader.AH_IsCancelled = true;
			Factory.ClearCachedValue<DocJobInvoicingJob>(job.PK.ToStringKey());
			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals("AP Lines Count", 0, JobWrapper.APLines.Count);
			AssertEquals("AP Lines for Tax Expense Count", 0, JobWrapper.APLinesForTaxExpense.Count);
		}

		public void TestAPExcludeJRJLines()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);

			AssertEquals("AR Lines Count", 0, JobWrapper.APExcludeJRJLines.Count);
			AssertEquals("Display No Exclude JRJ AR Invoices Message", true, JobWrapper.DisplayNoAPInvoicesMessage);

			var aRInvoiceLine = CreateARInvoiceLine(Factory.NewWithValidTestData<ARInvoice>(), job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AL_ARLine = aRInvoiceLine.PK;
			var aPInvoiceLine = CreateAPInvoiceLine(Factory.NewWithValidTestData<APInvoice>(), job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			charge.JR_AL_APLine = aPInvoiceLine.PK;
			charge.SetAmountsFromLinkedLinesForTests();
			var wip = CreateWIP(job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			var accrual = CreateAccrual(job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			var jobRevenueJournal = ObjectCreator.CreateJobRevenueJournal(ObjectCreator.CC1, job, 500M);
			jobRevenueJournal.JournalLines[0].CostRevenueType = TransactionLineTypes.Revenue;
			jobRevenueJournal.JournalLines[1].CostRevenueType = TransactionLineTypes.Cost;

			Factory.Save();

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);
			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, aPInvoiceLine.PK)).Returns(new[] { (ZDate.Today, new ZDecimal(8.24m)) });

			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals("AP Exclude JRJ Lines Count", 1, JobWrapper.APExcludeJRJLines.Count);
			AssertEquals("AP Exclude JRJ Lines for Profit Share Count", 2, JobWrapper.APExcludeJRJLinesForProfitShare.Count);
			AssertEquals(1, JobWrapper.APExcludeJRJLinesForProfitShare.Cast<DocJobLineDetail>().Count(e => e.IsTaxExpense));
			AssertEquals("Display No AR Exclude JRJ Invoices Message", false, JobWrapper.DisplayNoAPInvoicesMessage);
		}

		public void TestJRJLines()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			AssertEquals("AP Exclude JRJ Lines Count", 0, JobWrapper.JRJLines.Count);

			var aRInvoiceLine = CreateARInvoiceLine(Factory.NewWithValidTestData<ARInvoice>(), job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AL_ARLine = aRInvoiceLine.PK;
			var aPInvoiceLine = CreateAPInvoiceLine(Factory.NewWithValidTestData<APInvoice>(), job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			charge.JR_AL_APLine = aPInvoiceLine.PK;
			charge.SetAmountsFromLinkedLinesForTests();
			var wip = CreateWIP(job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			var accrual = CreateAccrual(job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);

			var jobRevenueJournal1 = ObjectCreator.CreateJobRevenueJournal(ObjectCreator.CC1, job, 500M);
			jobRevenueJournal1.JournalLines[0].CostRevenueType = TransactionLineTypes.Revenue;
			jobRevenueJournal1.JournalLines[1].CostRevenueType = TransactionLineTypes.Cost;

			var jobRevenueJournal2 = ObjectCreator.CreateJobRevenueJournal(ObjectCreator.CC1, job, 600M);
			jobRevenueJournal2.JournalLines[0].CostRevenueType = TransactionLineTypes.Revenue;
			jobRevenueJournal2.JournalLines[1].CostRevenueType = TransactionLineTypes.Revenue;

			var jobRevenueJournal3 = ObjectCreator.CreateJobRevenueJournal(ObjectCreator.CC1, job, 600M);
			jobRevenueJournal3.JournalLines[0].CostRevenueType = TransactionLineTypes.Cost;
			jobRevenueJournal3.JournalLines[1].CostRevenueType = TransactionLineTypes.Cost;

			var jobRevenueJournal4 = ObjectCreator.CreateJobRevenueJournal(ObjectCreator.CC1, job, 600M);
			jobRevenueJournal4.JournalLines[0].CostRevenueType = TransactionLineTypes.Cost;
			jobRevenueJournal4.JournalLines[1].CostRevenueType = TransactionLineTypes.Revenue;

			Factory.Save();

			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals("JRJ Lines Count", 8, JobWrapper.JRJLines.Count);
		}

		public void TestIsCancelled()
		{
			AccTransactionLines aRInvoiceLine = CreateARInvoiceLine(null, Job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			aRInvoiceLine.AL_AH = ObjectCreator.CreateInvoice(typeof(ARInvoice), ObjectCreator.AUD, 1M).PK;
			AssertEquals("AR Invoice Line is cancelled", false, JobWrapper.IsCancelled(aRInvoiceLine));
			aRInvoiceLine.TransactionHeader.AH_IsCancelled = true;
			AssertEquals("AR Invoice Line is cancelled", true, JobWrapper.IsCancelled(aRInvoiceLine));

			AccTransactionLines aPInvoiceLine = CreateAPInvoiceLine(null, Job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			aPInvoiceLine.AL_AH = ObjectCreator.CreateInvoice(typeof(APInvoice), ObjectCreator.AUD, 1M).PK;
			AssertEquals("AP Invoice Line is cancelled", false, JobWrapper.IsCancelled(aPInvoiceLine));
			aPInvoiceLine.TransactionHeader.AH_IsCancelled = true;
			AssertEquals("AP Invoice Line is cancelled", true, JobWrapper.IsCancelled(aPInvoiceLine));

			AccTransactionLines wip = CreateWIP(Job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			AssertEquals("WIP is cancelled", false, JobWrapper.IsCancelled(wip));

			(wip as BaseWIPAccrual).RelatedJobCharge.ReverseWIP(ZDateTime.Now);

			AssertEquals("WIP is cancelled", true, JobWrapper.IsCancelled(wip));

			AccTransactionLines accrual = CreateAccrual(Job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			AssertEquals("Accrual is cancelled", false, JobWrapper.IsCancelled(accrual));

			(accrual as BaseWIPAccrual).RelatedJobCharge.ReverseAccrual(ZDateTime.Now);

			AssertEquals("Accrual is cancelled", true, JobWrapper.IsCancelled(accrual));
		}

		public void TestIsAccrualOrWip()
		{
			AccTransactionLines aRInvoiceLine = CreateARInvoiceLine(null, Job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			AssertEquals("AR Invoice Line is Accrual or Wip", false, JobWrapper.IsAccrualOrWip(aRInvoiceLine));

			AccTransactionLines aPInvoiceLine = CreateAPInvoiceLine(null, Job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			AssertEquals("AP Invoice Line is Accrual or Wip", false, JobWrapper.IsAccrualOrWip(aPInvoiceLine));

			AccTransactionLines wip = CreateWIP(Job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			AssertEquals("WIP is Accrual or Wip", true, JobWrapper.IsAccrualOrWip(wip));

			AccTransactionLines accrual = CreateAccrual(Job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			AssertEquals("Accrual is Accrual or Wip", true, JobWrapper.IsAccrualOrWip(accrual));
		}

		public void TestIsCostOrRevenue()
		{
			AccTransactionLines aRInvoiceLine = CreateARInvoiceLine(null, Job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			AssertEquals("AR Invoice Line Is Cost or Revenue", true, JobWrapper.IsCostOrRevenue(aRInvoiceLine));

			AccTransactionLines aPInvoiceLine = CreateAPInvoiceLine(null, Job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			AssertEquals("AP Invoice Line Is Cost or Revenue", true, JobWrapper.IsCostOrRevenue(aPInvoiceLine));

			AccTransactionLines wip = CreateWIP(Job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			AssertEquals("WIP Is Cost or Revenue", false, JobWrapper.IsCostOrRevenue(wip));

			AccTransactionLines accrual = CreateAccrual(Job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			AssertEquals("Accrual Is Cost or Revenue", false, JobWrapper.IsCostOrRevenue(accrual));
		}

		public void TestIsRevenue()
		{
			AccTransactionLines aRInvoiceLine = CreateARInvoiceLine(null, Job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			AssertEquals("AR Invoice Line Is Revenue", true, JobWrapper.IsRevenue(aRInvoiceLine));

			AccTransactionLines aPInvoiceLine = CreateAPInvoiceLine(null, Job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			AssertEquals("AP Invoice Line Is Revenue", false, JobWrapper.IsRevenue(aPInvoiceLine));

			AccTransactionLines wip = CreateWIP(Job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			AssertEquals("WIP Is Revenue", false, JobWrapper.IsRevenue(wip));

			AccTransactionLines accrual = CreateAccrual(Job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			AssertEquals("Accrual Is Revenue", false, JobWrapper.IsRevenue(accrual));
		}

		public void TestIsCost()
		{
			AccTransactionLines aRInvoiceLine = CreateARInvoiceLine(null, Job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			AssertEquals("AR Invoice Line Is Cost", false, JobWrapper.IsCostOrJobRevenueJournal(aRInvoiceLine));
			AssertEquals("AR Invoice Line Is Cost", false, JobWrapper.IsCostOnly(aRInvoiceLine));

			AccTransactionLines aPInvoiceLine = CreateAPInvoiceLine(null, Job, ObjectCreator.CC1, ObjectCreator.AUD, 1M, "ARInvoiceLine", 100M);
			AssertEquals("AP Invoice Line Is Cost", true, JobWrapper.IsCostOrJobRevenueJournal(aPInvoiceLine));
			AssertEquals("AP Invoice Line Is Cost", true, JobWrapper.IsCostOnly(aPInvoiceLine));

			AccTransactionLines wip = CreateWIP(Job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			AssertEquals("WIP Is Cost", false, JobWrapper.IsCostOrJobRevenueJournal(wip));
			AssertEquals("WIP Is Cost", false, JobWrapper.IsCostOnly(wip));

			AccTransactionLines accrual = CreateAccrual(Job, ObjectCreator.CC1, 1M, "ARInvoiceLine", 100M);
			AssertEquals("Accrual Is Cost", false, JobWrapper.IsCostOrJobRevenueJournal(accrual));
			AssertEquals("Accrual Is Cost", false, JobWrapper.IsCostOnly(accrual));

			var jobRevenueJournal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, TestObjectCreator.Job1, 100M);
			AssertEquals("JobRevenueJournal is Cost", true, JobWrapper.IsCostOrJobRevenueJournal(jobRevenueJournal.Lines[0]));
			AssertEquals("JobRevenueJournal is Cost", false, JobWrapper.IsCostOnly(jobRevenueJournal.Lines[0]));
		}

		public void TestTotalRevenue()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			var line1 = ObjectCreator.CreateRevenueLineAndCharge(job.PK, 100m, "FRT");
			var line2 = ObjectCreator.CreateRevenueLineAndCharge(job.PK, 200m, "BAF");
			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 500m, "FRT");
			Factory.Save();
			line1.AL_ReverseDate = ZDateTime.BrettsBirthday;
			line2.AL_ReverseDate = ZDateTime.BrettsBirthday;
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals(300m, JobWrapper.TotalRevenueMovementsRecognized);
			AssertEquals(500m, JobWrapper.RevenueNotRecognized);
			AssertEquals(800m, JobWrapper.TotalRevenue);
		}

		public void TestTotalWip()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			ZGuid chargeCodePK = ObjectCreator.GetChargeCodeByCode("FRT");
			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			ObjectCreator.CreateWIPLineAndCharge(job.PK, 900m, "FRT");
			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = chargeCodePK;
			charge2.JR_LocalSellAmt = 400;
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals(900m, JobWrapper.TotalWIPMovementsRecognized);
			AssertEquals(400m, JobWrapper.WIPNotRecognized);
			AssertEquals(1300m, JobWrapper.TotalWip);
		}

		public void TestTotalCost()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			var line1 = ObjectCreator.CreateCostLineAndCharge(job.PK, 100m, "FRT");
			var line2 = ObjectCreator.CreateCostLineAndCharge(job.PK, 200m, "BAF");
			ObjectCreator.CreateCostLineAndCharge(job.PK, 500m, "FRT");
			Factory.Save();
			line1.AL_ReverseDate = ZDateTime.BrettsBirthday;
			line2.AL_ReverseDate = ZDateTime.BrettsBirthday;
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals(300m, JobWrapper.TotalCostMovementsRecognized);
			AssertEquals(500m, JobWrapper.CostNotRecognized);
			AssertEquals(800m, JobWrapper.TotalCost);
		}

		public void TestTotalAccrual()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			ZGuid chargeCodePK = ObjectCreator.GetChargeCodeByCode("FRT");
			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			ObjectCreator.CreateAccrualLineAndCharge(job.PK, 900m, "FRT");
			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = chargeCodePK;
			charge2.JR_LocalSellAmt = 400;
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals(900m, JobWrapper.TotalACRMovementsRecognized);
			AssertEquals(400m, JobWrapper.ACRNotRecognized);
			AssertEquals(1300m, JobWrapper.TotalAccrual);
		}

		public void TestTotalIncome()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 500m, "FRT");
			ObjectCreator.CreateWIPLineAndCharge(job.PK, 900m, "FRT");
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			JobWrapper = DocJobInvoicingJob.New(job, Factory);

			AssertEquals(500m, JobWrapper.TotalRevenue);
			AssertEquals(900m, JobWrapper.TotalWip);
			AssertEquals("Total Income (REV + WIP)", 1400M, JobWrapper.TotalIncome);
		}

		public void TestTotalExpense()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			ObjectCreator.CreateCostLineAndCharge(job.PK, 500m, "FRT");
			ObjectCreator.CreateAccrualLineAndCharge(job.PK, 900m, "FRT");
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			JobWrapper = DocJobInvoicingJob.New(job, Factory);

			AssertEquals(500m, JobWrapper.TotalCost);
			AssertEquals(900m, JobWrapper.TotalAccrual);
			AssertEquals("Total Expense (CST + ACR)", 1400M, JobWrapper.TotalExpense);
		}

		public void TestTotalRealisedAmount()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 700m, "FRT");
			ObjectCreator.CreateCostLineAndCharge(job.PK, 500m, "FRT");
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			JobWrapper = DocJobInvoicingJob.New(job, Factory);

			AssertEquals(700m, JobWrapper.TotalRevenue);
			AssertEquals(500m, JobWrapper.TotalCost);
			AssertEquals("Total Realised Amount (REV - CST)", 200M, JobWrapper.TotalRealisedAmount);
		}

		public void TestTotalEstimatedAmount()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			ObjectCreator.CreateWIPLineAndCharge(job.PK, 700m, "FRT");
			ObjectCreator.CreateAccrualLineAndCharge(job.PK, 500m, "FRT");
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			JobWrapper = DocJobInvoicingJob.New(job, Factory);

			AssertEquals(700m, JobWrapper.TotalWip);
			AssertEquals(500m, JobWrapper.TotalAccrual);
			AssertEquals("Total Estimated Amount (WIP - ACR)", 200M, JobWrapper.TotalEstimatedAmount);
		}

		public void TestTotalProfit()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 700m, "FRT");
			ObjectCreator.CreateCostLineAndCharge(job.PK, 500m, "FRT");
			ObjectCreator.CreateWIPLineAndCharge(job.PK, 800m, "FRT");
			ObjectCreator.CreateAccrualLineAndCharge(job.PK, 600m, "FRT");
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			JobWrapper = DocJobInvoicingJob.New(job, Factory);

			AssertEquals(700m, JobWrapper.TotalRevenue);
			AssertEquals(500m, JobWrapper.TotalCost);
			AssertEquals(800m, JobWrapper.TotalWip);
			AssertEquals(600m, JobWrapper.TotalAccrual);
			AssertEquals("Total Profit (REV + WIP - ACR - CST)", 400M, JobWrapper.TotalProfit);
		}

		public void TestProfitCostMargin()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 700m, "FRT");
			ObjectCreator.CreateCostLineAndCharge(job.PK, 400m, "FRT");
			ObjectCreator.CreateWIPLineAndCharge(job.PK, 800m, "FRT");
			ObjectCreator.CreateAccrualLineAndCharge(job.PK, 600m, "FRT");
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			JobWrapper = DocJobInvoicingJob.New(job, Factory);

			AssertEquals(700m, JobWrapper.TotalRevenue);
			AssertEquals(400m, JobWrapper.TotalCost);
			AssertEquals(800m, JobWrapper.TotalWip);
			AssertEquals(600m, JobWrapper.TotalAccrual);
			AssertEquals("Total Profit (REV + WIP - ACR - CST)", 500M, JobWrapper.TotalProfit);
			AssertEquals("Total Expense (CST + ACR)", 1000M, JobWrapper.TotalExpense);
			AssertEquals("Total Profit / Total Expense", 0.5m, JobWrapper.ProfitCostMargin);
		}

		public void TestProfitRevMargin()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 700m, "FRT");
			ObjectCreator.CreateCostLineAndCharge(job.PK, 400m, "FRT");
			ObjectCreator.CreateWIPLineAndCharge(job.PK, 300m, "FRT");
			ObjectCreator.CreateAccrualLineAndCharge(job.PK, 500m, "FRT");
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			JobWrapper = DocJobInvoicingJob.New(job, Factory);

			AssertEquals(700m, JobWrapper.TotalRevenue);
			AssertEquals(400m, JobWrapper.TotalCost);
			AssertEquals(300m, JobWrapper.TotalWip);
			AssertEquals(500m, JobWrapper.TotalAccrual);
			AssertEquals("Total Profit (REV + WIP - ACR - CST)", 100M, JobWrapper.TotalProfit);
			AssertEquals("Total Income (REV + WIP)", 1000M, JobWrapper.TotalIncome);
			AssertEquals("Total Profit / Total Income", 0.1m, JobWrapper.ProfitRevMargin);
		}

		public void TestRevenueRecognitionDates()
		{
			JobChargeRevRecognition revRec1 = Factory.NewWithValidTestData<JobChargeRevRecognition>();
			JobChargeRevRecognition revRec2 = Factory.NewWithValidTestData<JobChargeRevRecognition>();
			JobChargeRevRecognition revRec3 = Factory.NewWithValidTestData<JobChargeRevRecognition>();
			revRec1.D3_JH = Job.PK;
			revRec2.D3_JH = Job.PK;
			revRec3.D3_JH = Job.PK;
			revRec1.D3_RecognitionDate = ZDateTime.BrettsBirthday;
			revRec2.D3_RecognitionDate = AccountingConstants.RevenueRecognitionDateConstants.CustomsClearanceDate;
			revRec3.D3_RecognitionDate = ZDateTime.BrettsBirthday;
			revRec1.D3_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			revRec2.D3_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.OldJob;
			revRec3.D3_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			AssertEquals("ARV 18-Sep-71, IMM, JOB CUS", JobWrapper.RevenueRecognitionDates);
		}

		public void TestRevenueNotRecognized()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			ZGuid chargeCodePK = ObjectCreator.GetChargeCodeByCode("FRT");
			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 900m, "FRT");
			var line2 = ObjectCreator.CreateRevenueLineAndCharge(job.PK, 400m, "FRT");
			Charge charge3 = job.Charges.AddNew();
			charge3.JR_AC = chargeCodePK;
			charge3.JR_LocalSellAmt = 400;
			Factory.Save();
			line2.AL_ReverseDate = ZDateTime.BrettsBirthday;
			Factory.Save();
			job.LoadCharges_ForTestOnly();

			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals(900m, JobWrapper.RevenueNotRecognized);
		}

		public void TestCostNotRecognized()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			ZGuid chargeCodePK = ObjectCreator.GetChargeCodeByCode("FRT");
			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			ObjectCreator.CreateCostLineAndCharge(job.PK, 900m, "FRT");
			var line2 = ObjectCreator.CreateCostLineAndCharge(job.PK, 400m, "FRT");
			Charge charge3 = job.Charges.AddNew();
			charge3.JR_AC = chargeCodePK;
			charge3.JR_LocalCostAmt = 400;
			Factory.Save();
			line2.AL_ReverseDate = ZDateTime.BrettsBirthday;
			Factory.Save();
			job.LoadCharges_ForTestOnly();

			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals(900m, JobWrapper.CostNotRecognized);

			AssertEquals("Cost Not Recognized", 900M, JobWrapper.CostNotRecognized);
		}

		public void TestWIPNotRecognized()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			ZGuid chargeCodePK = ObjectCreator.GetChargeCodeByCode("FRT");
			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			var line1 = ObjectCreator.CreateWIPLineAndCharge(job.PK, 900m, "FRT");
			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = chargeCodePK;
			charge2.JR_LocalSellAmt = 400;
			Charge charge3 = job.Charges.AddNew();
			charge3.JR_LocalCostAmt = 600;
			charge3.JR_AC = chargeCodePK;
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals(400m, JobWrapper.WIPNotRecognized);
		}

		public void TestACRNotRecognized()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			ZGuid chargeCodePK = ObjectCreator.GetChargeCodeByCode("FRT");
			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			var line1 = ObjectCreator.CreateAccrualLineAndCharge(job.PK, 900m, "FRT");
			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = chargeCodePK;
			charge2.JR_LocalCostAmt = 400;
			Charge charge3 = job.Charges.AddNew();
			charge3.JR_LocalSellAmt = 600;
			charge3.JR_AC = chargeCodePK;
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals(400m, JobWrapper.ACRNotRecognized);
		}

		public void TestTotalJobProfitNotRecognizedInGL()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			ZGuid chargeCodePK = ObjectCreator.GetChargeCodeByCode("FRT");
			Job job = ObjectCreator.CreateJob("S00001234", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 900m, "FRT");
			ObjectCreator.CreateCostLineAndCharge(job.PK, 800m, "FRT");
			Charge charge3 = job.Charges.AddNew();
			charge3.JR_AC = chargeCodePK;
			charge3.JR_LocalSellAmt = 600;
			charge3.JR_LocalCostAmt = 500;
			Factory.Save();

			job.LoadCharges_ForTestOnly();

			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertEquals(900m, JobWrapper.RevenueNotRecognized);
			AssertEquals(800m, JobWrapper.CostNotRecognized);
			AssertEquals(600m, JobWrapper.WIPNotRecognized);
			AssertEquals(500m, JobWrapper.ACRNotRecognized);

			AssertEquals("TotalJobProfitNotRecognizedInGL should be Rev + WIP - Cost - ACR", 200M, JobWrapper.TotalJobProfitNotRecognizedInGL);
		}

		public void TestShipmentJob()
		{
			AssertEquals("Is Shipment", false, JobWrapper.IsShipment);
			AssertNull("DocShipment", JobWrapper.Shipment);

			CommonShipment shipment = ObjectCreator.CreateShipment("S00001234", "AUSYD", "NZAKL");
			Job.JH_ParentID = shipment.PK;
			Job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			AssertEquals("Is Shipment", true, JobWrapper.IsShipment);
			AssertNotNull("DocShipment", JobWrapper.Shipment);
		}

		public void TestAUDeclarationJob()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				var currentCompany = Factory.NewWithValidTestData<GlbCompany>();
				var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
				currentBranch.GB_GC = currentCompany.PK;

				currentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

				AssertNull("Declaration", JobWrapper.Declaration);

				Enterprise.Customs.AU.Declaration.Business.JobDeclaration declaration = Factory.New<Enterprise.Customs.AU.Declaration.Business.JobDeclaration>();
				Factory.Save();

				Job.JH_ParentID = declaration.PK;
				Job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;

				JobWrapper = DocJobInvoicingJob.New(Job, Factory);

				AssertNotNull("Declaration", JobWrapper.Declaration);
				AssertEquals("Declaration is of type Enterprise.DocumentWrappers.Customs.AU.DocDeclaration", typeof(Customs.AU.DocDeclaration), JobWrapper.Declaration.GetType());
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		public void TestNZDeclarationJob()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				var currentCompany = Factory.NewWithValidTestData<GlbCompany>();
				var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
				currentBranch.GB_GC = currentCompany.PK;

				currentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);

				AssertNull("Declaration", JobWrapper.Declaration);

				Enterprise.Customs.NZ.Business.Declaration.JobDeclaration declaration = Enterprise.Customs.NZ.Business.Declaration.JobDeclaration.New(Factory);
				declaration.JE_GB = currentBranch.PK;
				Factory.Save();

				Job.JH_ParentID = declaration.PK;
				Job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;

				JobWrapper = DocJobInvoicingJob.New(Job, Factory);
				AssertNotNull("Declaration", JobWrapper.Declaration);
				AssertEquals("Declaration is of type Enterprise.DocumentWrappers.Customs.NZ.DocDeclaration", typeof(Customs.NZ.DocDeclaration), JobWrapper.Declaration.GetType());
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		public void TestFJDeclarationJob()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				var currentCompany = Factory.NewWithValidTestData<GlbCompany>();
				var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
				currentBranch.GB_GC = currentCompany.PK;

				currentCompany.SetCountry(Core.Constants.CountryCodes.Fiji);
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Fiji);

				AssertNull("Declaration", JobWrapper.Declaration);

				BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_GB = currentBranch.PK;
				Factory.Save();

				Job.JH_ParentID = declaration.PK;
				Job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;

				JobWrapper = DocJobInvoicingJob.New(Job, Factory);
				AssertNotNull("Declaration", JobWrapper.Declaration);
				AssertEquals("Declaration is of type Enterprise.DocumentWrappers.Customs.General.DocDeclaration", typeof(Customs.General.DocDeclaration), JobWrapper.Declaration.GetType());
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		public void TestLoadListJob()
		{
			AssertEquals("Is Load List", false, JobWrapper.IsLoadList);
			AssertNull("Load List", JobWrapper.LoadList);

			var loadList = Factory.New<CFSLoadListConsol>();
			Job.JH_ParentID = loadList.PK;
			Job.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;

			AssertEquals("Is Load List", true, JobWrapper.IsLoadList);
			AssertNotNull("Load List", JobWrapper.LoadList);
		}

		public void TestTransportJob()
		{
			AssertEquals("Is Transport", false, JobWrapper.IsTransport);
			AssertNull("Cartage", JobWrapper.Cartage);

			var cartage = Factory.New<CommonCartage>();
			Job.JH_ParentID = cartage.PK;
			Job.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;

			AssertEquals("Is Transport", true, JobWrapper.IsTransport);
			AssertNotNull("Cartage", JobWrapper.Cartage);
		}

		public void TestLoadingJobDocumentPrinter()
		{
			JobDocumentPrinter printer = new JobDocumentPrinter(Factory);
			JobDocumentPrintItem printItem = new JobDocumentPrintItem(printer, Job, Factory);
			JobWrapper = DocJobInvoicingJob.New(printItem, Factory);

			AssertNotNull("Job Document Wrapper should not be null", JobWrapper);
			AssertEquals("Print Charge Summary", printItem.PrintChargeSummary, JobWrapper.PrintChargeSummary);
			Assert("Print Profit Recognition by Date Summary", printItem.PrintProfitRecognitionByDateSummary == JobWrapper.PrintProfitRecognitionByDateSummary && !printer.PrintProfitRecognitionByDateSummary);
			AssertEquals("Print Charge Detail", printItem.PrintChargeDetail, JobWrapper.PrintChargeDetail);
			AssertEquals("Print AR Invoice Analysis", printItem.PrintARInvoiceAnalysis, JobWrapper.PrintARInvoiceAnalysis);
			AssertEquals("Print AP Invoice Analysis", printItem.PrintAPInvoiceAnalysis, JobWrapper.PrintAPInvoiceAnalysis);
			AssertEquals("Print AP Invoice Analysis", printItem.PrintJobRevenueJournalAnalysis, JobWrapper.PrintJobRevenueJournalAnalysis);

			printItem = new JobDocumentPrintItem(printer, Job, Factory);
			printer.PrintChargeSummary = true;
			printer.PrintProfitRecognitionByDateSummary = true;
			printer.PrintChargeDetail = true;
			printer.PrintARInvoiceAnalysis = true;
			printer.PrintAPInvoiceAnalysis = true;
			printer.PrintJobRevenueJournalAnalysis = true;
			JobWrapper = DocJobInvoicingJob.New(printItem, Factory);

			AssertNotNull("Job Document Wrapper should not be null", JobWrapper);
			AssertEquals("Print Charge Summary", printItem.PrintChargeSummary, JobWrapper.PrintChargeSummary);
			Assert("Print Profit Recognition by Date Summary", printItem.PrintProfitRecognitionByDateSummary == JobWrapper.PrintProfitRecognitionByDateSummary && JobWrapper.PrintProfitRecognitionByDateSummary);
			AssertEquals("Print Charge Detail", printItem.PrintChargeDetail, JobWrapper.PrintChargeDetail);
			AssertEquals("Print AR Invoice Analysis", printItem.PrintARInvoiceAnalysis, JobWrapper.PrintARInvoiceAnalysis);
			AssertEquals("Print AP Invoice Analysis", printItem.PrintAPInvoiceAnalysis, JobWrapper.PrintAPInvoiceAnalysis);
			AssertEquals("Print Job Revenue Journal Analysis", printItem.PrintAPInvoiceAnalysis, JobWrapper.PrintJobRevenueJournalAnalysis);
		}

		public void TestJobType()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Job.JH_ParentID = declaration.PK;
			Job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			Factory.Save();

			AssertEquals("Should be a brokerage JobType", JobInvoicingConsumerTypes.Brokerage.Code, JobWrapper.JobType);
		}

		public void TestOrigin()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			Job.JH_ParentID = shipment.PK;
			Job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Factory.Save();

			AssertEquals("Origin should be 'AUSYD'", "AUSYD", JobWrapper.Origin);
		}

		public void TestDestination()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "NZAKL";
			Job.JH_ParentID = shipment.PK;
			Job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Factory.Save();

			AssertEquals("Destination should be 'NZAKL'", "NZAKL", JobWrapper.Destination);
		}

		public void TestFilteredCollectionsWithSecuritySettings()
		{
			GlbStaff testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_IsController = false;

			GlbCompany testCompany = Factory.NewWithValidTestData<GlbCompany>();
			var testBranch = ObjectCreator.CreateBranch("TBR", "Test Branch", GlbCompany.CurrentCompany);
			Factory.Save();

			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var consol = Factory.New<ForwardingConsol>();
				var testShipment1 = ObjectCreator.CreateShipment("S00001234");
				var testJob1 = ObjectCreator.CreateJob(testShipment1);
				var testCharge1 = ObjectCreator.CreateCharge(testJob1, ObjectCreator.CC1, 100m, 100m);
				var testCharge2 = ObjectCreator.CreateCharge(testJob1, ObjectCreator.CC2, 100m, 100m);
				consol.Shipments.Add(testShipment1);

				var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M);
				var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M);
				Factory.Save();

				BusinessObjectFactory securityFactory = new BusinessObjectFactory();

				SecurityCore security = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
				SecurityCore security2 = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, testBranch.PK.ToGuid(), Env.CurrentDepartment.PK);

				GlbSecurity loginSecurity = securityFactory.New<GlbSecurity>();
				loginSecurity.GU_GB = Env.CurrentBranch.PK;
				loginSecurity.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity.GU_GS = Env.CurrentUser.PK;
				loginSecurity.GU_SecurityRight = security.Login.Code;
				GlbSecurity loginSecurity2 = securityFactory.New<GlbSecurity>();
				loginSecurity2.GU_GB = testBranch.PK;
				loginSecurity2.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity2.GU_GS = Env.CurrentUser.PK;
				loginSecurity2.GU_SecurityRight = security2.Login.Code;

				GlbSecurity invoicingSecurity = securityFactory.New<GlbSecurity>();
				invoicingSecurity.GU_GB = Env.CurrentBranch.PK;
				invoicingSecurity.GU_GE = Env.CurrentDepartment.PK;
				invoicingSecurity.GU_GS = Env.CurrentUser.PK;
				invoicingSecurity.GU_SecurityRight = security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowViewCharges).Code;

				loginSecurity.GU_SecurityItemIsAllowed = true;
				security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowViewCosts).IsAllowed = true;
				security2.Login.IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = true;
				loginSecurity2.GU_SecurityItemIsAllowed = false;

				securityFactory.Save();

				var printer = new JobDocumentPrinter(Factory);
				var printItem = new JobDocumentPrintItem(printer, testJob1, Factory);
				printer.IsProfitLossDoc = true;
				var jobWrapper = DocJobInvoicingJob.New(printItem, Factory);

				AssertEquals("PreCondition", true, jobWrapper.IsProfitLossDoc);
				AssertEquals("PreCondition", false, jobWrapper.IsConsolDoc);
				AssertEquals("should show all charges", 2, jobWrapper.Charges.Count);
				AssertEquals("should show all lines", 4, jobWrapper.LineBizObjCollection.Count);
				AssertEquals("should show all lines", 4, jobWrapper.FilteredLineBizObjCollection.Count);

				loginSecurity.GU_SecurityItemIsAllowed = true;
				security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowViewCharges).IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = false;
				securityFactory.Save();

				Factory.ClearCachedValue<DocJobInvoicingJob>(testJob1.PK.ToStringKey());
				JobWrapper = DocJobInvoicingJob.New(printItem, Factory);
				AssertEquals("should show all charges", 2, jobWrapper.Charges.Count);
				AssertEquals("should show all lines", 4, jobWrapper.LineBizObjCollection.Count);
				AssertEquals("should show all lines", 4, jobWrapper.FilteredLineBizObjCollection.Count);

				testCharge1.JR_GB = testBranch.PK;
				Factory.ClearCachedValue<bool>(("Login BRN:" + testBranch.GB_Code + " DEP:" + GlbDepartment.CurrentDepartment.GE_Code));
				Factory.ClearCachedValue<DocJobInvoicingJob>(testJob1.PK.ToStringKey());
				Factory.Save();
				JobWrapper = DocJobInvoicingJob.New(printItem, Factory);
				jobWrapper.LineBizObjCollection.Load();
				AssertEquals("should show all charges", 1, JobWrapper.Charges.Count);
				AssertEquals("should show all lines (2 lines will be created for testBranch)", 6, jobWrapper.LineBizObjCollection.Count);
				AssertEquals("should show previous 4 lines for which branch is not testBranch", 4, jobWrapper.FilteredLineBizObjCollection.Count);

				invoicingSecurity.GU_SecurityRight = security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.AllowViewCosts).Code;
				security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.AllowViewCosts).IsAllowed = true;
				securityFactory.Save();

				Factory.ClearCachedValue<DocJobInvoicingJob>(testJob1.PK.ToStringKey());
				var consolPrinter = new ConsolJobDocumentPrinter(Factory);
				consolPrinter.IsProfitLossDoc = true;
				var consolPrintItem = new ConsolJobDocumentPrintItem(consolPrinter, consol, Factory);
				var consolWrapper = DocForwardingConsol.New(consolPrintItem, Factory);
				var wrappedJob = (DocJobInvoicingJob)consolWrapper.Jobs.FirstOrDefault();

				AssertEquals("PreCondition", true, wrappedJob.IsProfitLossDoc);
				AssertEquals("PreCondition", true, wrappedJob.IsConsolDoc);
				AssertEquals("should show all charges", 2, wrappedJob.Charges.Count);
				AssertEquals("should show all lines", 6, wrappedJob.LineBizObjCollection.Count);
				AssertEquals("should show all lines", 6, wrappedJob.FilteredLineBizObjCollection.Count);

				security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.AllowViewCosts).IsAllowed = false;
				securityFactory.Save();
				Factory.ClearCachedValue<DocJobInvoicingJob>(testJob1.PK.ToStringKey());
				consolWrapper = DocForwardingConsol.New(consolPrintItem, Factory);
				wrappedJob = (DocJobInvoicingJob)consolWrapper.Jobs.FirstOrDefault();
				AssertEquals("should show all charges", 1, wrappedJob.Charges.Count);
				AssertEquals("should show all lines", 6, wrappedJob.LineBizObjCollection.Count);
				AssertEquals("should show 4 lines for which branch is not testBranch", 4, wrappedJob.FilteredLineBizObjCollection.Count);

				var taxProcessorMock = new Mock<ITaxProcessor>();
				ObjectFactory.Substitute(taxProcessorMock.Object);

				var arInvLine = arInvoice.Lines[0];
				taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, arInvLine.PK)).Returns(new[] { (ZDate.Today, new ZDecimal(8.32m)) });

				var apInvLine = apInvoice.Lines[0];
				taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, apInvLine.PK)).Returns(new[] { (ZDate.Today, new ZDecimal(1.56m)) });

				var wip = testCharge2.WIP;
				wip.AL_JH = ZGuid.Empty;
				var accrual = testCharge2.Accrual;
				accrual.AL_JH = ZGuid.Empty;

				wip.AL_ReverseDate = ZDateTime.Now;
				accrual.AL_ReverseDate = ZDateTime.Now;

				testCharge2.JR_AL_ARLine = arInvLine.PK;
				testCharge2.JR_AL_APLine = apInvLine.PK;

				AssertEquals("should show all charges", 1, wrappedJob.Charges.Count);
				AssertEquals("should show all charges", 2, wrappedJob.ChargesForProfitShare.Count);
				AssertEquals("should show all charges", 1, wrappedJob.ChargesForProfitShare.Cast<DocJobInvoicingJobCharge>().Count(c => c.IsTaxExpense));
			}
		}

		public void TestAllProfitLossSummaryLines()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			ForwardingShipment shipment = ObjectCreator.CreateShipment("S00001000");
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 500m, "FRT");
			ObjectCreator.CreateWIPLineAndCharge(job.PK, 400m, "FRT");
			ObjectCreator.CreateCostLineAndCharge(job.PK, 200m, "FRT");
			ObjectCreator.CreateAccrualLineAndCharge(job.PK, 200m, "FRT");
			Factory.Save();

			job.LoadCharges_ForTestOnly();

			JobWrapper = DocJobInvoicingJob.New(job, Factory);
			AssertNotNull(JobWrapper.AllProfitLossSummaryLines);
			AssertEquals(1, JobWrapper.AllProfitLossSummaryLines.Count);
			AssertEquals(500m, JobWrapper.AllProfitLossSummaryLines[0].Revenue);
			AssertEquals(400m, JobWrapper.AllProfitLossSummaryLines[0].WIP);
			AssertEquals(200m, JobWrapper.AllProfitLossSummaryLines[0].Cost);
			AssertEquals(200m, JobWrapper.AllProfitLossSummaryLines[0].Accrual);
			AssertEquals(900m, JobWrapper.AllProfitLossSummaryLines[0].Income);
			AssertEquals(-400m, JobWrapper.AllProfitLossSummaryLines[0].Expense);
			AssertEquals(500m, JobWrapper.AllProfitLossSummaryLines[0].Profit);
			AssertEquals("FRT", JobWrapper.AllProfitLossSummaryLines[0].ChargeCode.Code);
			AssertEquals(job.JH_JobNum, JobWrapper.AllProfitLossSummaryLines[0].Job.JobNum);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Job = ObjectCreator.CreateJob("Sx0001001", ObjectCreator.AALSHI, 0M, ObjectCreator.ABIGAS, 0M);
			JobDocPrinter = new JobDocumentPrinter(Factory);
			JobWrapper = DocJobInvoicingJob.New(Job, Factory);
			Factory.Save();
		}

		Job Job;
		JobDocumentPrinter JobDocPrinter;
		DocJobInvoicingJob JobWrapper;

		TestObjectCreator ObjectCreator
		{
			get
			{
				if (fObjectCreator == null)
				{
					fObjectCreator = new TestObjectCreator(Factory);
				}

				return fObjectCreator;
			}
		}

		TestObjectCreator fObjectCreator;

		public AccTransactionLines CreateARInvoiceLine(ARInvoice parent, Job job, AccChargeCode chargeCode, RefCurrency currency, decimal exchangeRate, string desc, decimal oSAmountIncGST)
		{
			ARInvoiceLine aRInvoiceLine = ObjectCreator.CreateARInvoiceLine(parent, job, chargeCode, currency, exchangeRate, desc, oSAmountIncGST);
			aRInvoiceLine.AL_ExchangeRate = exchangeRate;
			aRInvoiceLine.AL_OSExTaxAmount = oSAmountIncGST;
			aRInvoiceLine.AL_LocalExTaxAmount = oSAmountIncGST;
			return aRInvoiceLine;
		}

		public AccTransactionLines CreateAPInvoiceLine(APInvoice parent, Job job, AccChargeCode chargeCode, RefCurrency currency, decimal exchangeRate, string desc, decimal oSAmountIncGST)
		{
			APInvoiceLine aPInvoiceLine = ObjectCreator.CreateAPInvoiceLine(parent, job, chargeCode, currency, exchangeRate, desc, oSAmountIncGST);
			aPInvoiceLine.AL_ExchangeRate = exchangeRate;
			aPInvoiceLine.AL_OSExTaxAmount = oSAmountIncGST;
			aPInvoiceLine.AL_LocalExTaxAmount = oSAmountIncGST;
			return aPInvoiceLine;
		}

		public Accrual CreateAccrual(Job job, AccChargeCode chargeCode, decimal exchangeRate, string desc, decimal oSAmountIncGST)
		{
			Accrual accrual = ObjectCreator.CreateAccrual(job, chargeCode, exchangeRate, desc, oSAmountIncGST);
			accrual.AL_ExchangeRate = exchangeRate;
			accrual.AL_OSExTaxAmount = oSAmountIncGST;
			accrual.AL_LocalExTaxAmount = oSAmountIncGST;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(accrual);
			return accrual;
		}

		public AccTransactionLines CreateWIP(Job job, AccChargeCode chargeCode, decimal exchangeRate, string desc, decimal oSAmountIncGST)
		{
			WIP wIP = ObjectCreator.CreateWIP(job, chargeCode, exchangeRate, desc, oSAmountIncGST);
			wIP.AL_ExchangeRate = exchangeRate;
			wIP.AL_OSExTaxAmount = oSAmountIncGST;
			wIP.AL_LocalExTaxAmount = oSAmountIncGST;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP);
			return wIP;
		}
		#endregion
	}
}
