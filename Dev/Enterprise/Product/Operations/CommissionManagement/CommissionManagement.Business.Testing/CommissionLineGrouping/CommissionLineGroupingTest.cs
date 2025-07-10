using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CommissionLineGroupingForTest))]
	internal class CommissionLineGroupingTest : NonPersistentBusinessObjectTestCase
	{
		#region Constructor

		public void TestConstructor_DoesNotHitDb()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();

			var jobCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			jobCommissionHeader.CH0_GroupingSourceID = job.PK;
			jobCommissionHeader.CH0_GroupingSourceTableCode = job.TablePrefix;
			var jobCommissionLine = jobCommissionHeader.Lines.AddNew();
			jobCommissionLine.FillWithValidTestData();

			var invoiceCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			invoiceCommissionHeader.CH0_GroupingSourceID = job.PK;
			invoiceCommissionHeader.CH0_GroupingSourceTableCode = job.TablePrefix;
			var invoiceCommissionLine = invoiceCommissionHeader.Lines.AddNew();
			invoiceCommissionLine.FillWithValidTestData();

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var jobLineInOtherFactory = otherFactory.Load<ViewCommissionLine>(jobCommissionLine.PK);
			var invoiceLineInOtherFactory = otherFactory.Load<ViewCommissionLine>(invoiceCommissionLine.PK);

			otherFactory.ResetDatabaseLoadCount();
			var expectedDbHits = new Dictionary<string, int>();
			var jobGrouping = GetNewGrouping(otherFactory, new[] { jobLineInOtherFactory });
			var invoiceGrouping = GetNewGrouping(otherFactory, new[] { invoiceLineInOtherFactory });
			AssertDbHits(expectedDbHits, otherFactory);
		}

		#endregion

		#region Property

		public void TestZDecimalsHaveCorrectDecimalPlacesCommissionLineGrouping()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreate();
			var jobLine = Factory.New<ViewCommissionLine>();
			jobLine.VCL_GroupingSourceID = job.PK;
			jobLine.VCL_GroupingSourceTableCode = job.TablePrefix;
			var jobGrouping = GetNewGrouping(new[] { jobLine });

			var localList = new List<string>
			{
				nameof(jobGrouping.TotalCommissionableAmountInLocalCurrency),
				nameof(jobGrouping.ShareCommissionAmountInLocalCurrency),
				nameof(jobGrouping.EntityCommissionAmountInLocalCurrency)
			};

			var prefList = new List<string>
			{
				nameof(jobGrouping.TotalCommissionableAmountInPreferredCurrency),
				nameof(jobGrouping.ShareCommissionAmountInPreferredCurrency),
				nameof(jobGrouping.EntityCommissionAmountInPreferredCurrency),
				nameof(jobGrouping.PaidEntityCommissionAmountInPreferredCurrency),
				nameof(jobGrouping.OutstandingEntityCommissionAmountInPreferredCurrency)
			};

			var transactionList = new List<string>
			{
				nameof(jobGrouping.TotalCommissionableAmountInTransactionCurrency)
			};

			var tester = new DecimalPlacesAttributeTester(jobGrouping, jobGrouping.Company);
			tester.CheckNonLocalCurrency(localList, nameof(jobGrouping.LocalCurrencyDecimalPlaces), nameof(jobGrouping.LocalCurrencyCode), jobGrouping);
			tester.CheckNonLocalCurrency(prefList, nameof(jobGrouping.PreferredCurrencyDecimalPlaces), nameof(jobGrouping.PreferredCurrencyCode), jobGrouping);
			tester.CheckNonLocalCurrency(transactionList, nameof(jobGrouping.TransactionCurrencyDecimalPlaces), nameof(jobGrouping.TransactionCurrencyCode), jobGrouping);
		}

		public void TestSource()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoice = Factory.New<ARInvoice>();

			var jobLine = Factory.New<ViewCommissionLine>();
			jobLine.VCL_GroupingSourceID = job.PK;
			jobLine.VCL_GroupingSourceTableCode = job.TablePrefix;
			var invoiceLine = Factory.New<ViewCommissionLine>();
			invoiceLine.VCL_GroupingSourceID = invoice.PK;
			invoiceLine.VCL_GroupingSourceTableCode = invoice.TablePrefix;
			var jobGrouping = GetNewGrouping(new[] { jobLine });
			var invoiceGrouping = GetNewGrouping(new[] { invoiceLine });

			AssertEquals(job, jobGrouping.Job);
			AssertNull(invoiceGrouping.Job);

			AssertNull(jobGrouping.Transaction);
			AssertEquals(invoice, invoiceGrouping.Transaction);
		}

		public void TestSourceNumber()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_JobNum = "H00001000";
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_TransactionNum = "00001000";

			var jobLine = Factory.New<ViewCommissionLine>();
			jobLine.VCL_GroupingSourceID = job.PK;
			jobLine.VCL_GroupingSourceTableCode = job.TablePrefix;
			jobLine.VCL_JobNumber = job.JH_JobNum;
			var invoiceLine = Factory.New<ViewCommissionLine>();
			invoiceLine.VCL_GroupingSourceID = invoice.PK;
			invoiceLine.VCL_GroupingSourceTableCode = invoice.TablePrefix;
			var jobGrouping = GetNewGrouping(new[] { jobLine });
			var invoiceGrouping = GetNewGrouping(new[] { invoiceLine });

			AssertEquals("H00001000", jobGrouping.SourceNumber);
			AssertEquals("00001000", invoiceGrouping.SourceNumber);
		}

		public void TestSourceUniqueId()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_JobNum = "H00001000";
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_TransactionNum = "00001000";

			var jobCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			jobCommissionHeader.CH0_GroupingSourceID = job.PK;
			jobCommissionHeader.CH0_GroupingSourceTableCode = job.TablePrefix;
			jobCommissionHeader.CH0_JobNumber = job.JH_JobNum;
			jobCommissionHeader.CH0_GC = GlbCompany.CurrentCompany.PK;
			var jobCommissionLine = jobCommissionHeader.Lines.AddNew();
			jobCommissionLine.FillWithValidTestData();

			var invoiceCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			invoiceCommissionHeader.CH0_GroupingSourceID = invoice.PK;
			invoiceCommissionHeader.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			var invoiceCommissionLine = invoiceCommissionHeader.Lines.AddNew();
			invoiceCommissionLine.FillWithValidTestData();

			Factory.Save();

			var jobViewLine = Factory.Load<ViewCommissionLine>(jobCommissionLine.PK);
			var invoiceViewLine = Factory.Load<ViewCommissionLine>(invoiceCommissionLine.PK);

			var jobGrouping = GetNewGrouping(new[] { jobViewLine });
			var invoiceGrouping = GetNewGrouping(new[] { invoiceViewLine });

			AssertEquals($"{GlbCompany.CurrentCompany.PK.ToString().ToUpper()}-H00001000", jobGrouping.SourceUniqueId);
			AssertEquals($"{invoice.PK.ToString().ToUpper()}", invoiceGrouping.SourceUniqueId);
		}

		public void TestSourceClientPk()
		{
			var clientA = Factory.New<OrgHeader>();
			var clientB = Factory.New<OrgHeader>();

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_OA_LocalChargesAddr = clientA.MainAddress.PK;
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = clientB.PK;

			var jobLine = Factory.New<ViewCommissionLine>();
			jobLine.VCL_GroupingSourceID = job.PK;
			jobLine.VCL_GroupingSourceTableCode = job.TablePrefix;
			var invoiceLine = Factory.New<ViewCommissionLine>();
			invoiceLine.VCL_GroupingSourceID = invoice.PK;
			invoiceLine.VCL_GroupingSourceTableCode = invoice.TablePrefix;
			var jobGrouping = GetNewGrouping(new[] { jobLine });
			var invoiceGrouping = GetNewGrouping(new[] { invoiceLine });

			AssertEquals(clientA.PK, jobGrouping.SourceClientPk);
			AssertEquals(clientB.PK, invoiceGrouping.SourceClientPk);
		}

		public void TestSource_JobRevenueJournal()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var jrj = Factory.New<JobRevenueJournal>();

			var jobLine = Factory.New<ViewCommissionLine>();
			jobLine.VCL_GroupingSourceID = job.PK;
			jobLine.VCL_GroupingSourceTableCode = job.TablePrefix;
			var jrjLine = Factory.New<ViewCommissionLine>();
			jrjLine.VCL_GroupingSourceID = jrj.PK;
			jrjLine.VCL_GroupingSourceTableCode = jrj.TablePrefix;
			var jobGrouping = GetNewGrouping(new[] { jobLine });
			var jrjGrouping = GetNewGrouping(new[] { jrjLine });

			AssertEquals(job, jobGrouping.Job);
			AssertNull(jrjGrouping.Job);

			AssertNull(jobGrouping.Transaction);
			AssertEquals(jrj, jrjGrouping.Transaction);
		}

		public void TestSourceNumber_JobRevenueJournal()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_JobNum = "H00001000";
			var jrj = Factory.New<JobRevenueJournal>();
			jrj.AH_TransactionNum = "00001000";

			var jobLine = Factory.New<ViewCommissionLine>();
			jobLine.VCL_GroupingSourceID = job.PK;
			jobLine.VCL_GroupingSourceTableCode = job.TablePrefix;
			jobLine.VCL_JobNumber = job.JH_JobNum;
			var jrjLine = Factory.New<ViewCommissionLine>();
			jrjLine.VCL_GroupingSourceID = jrj.PK;
			jrjLine.VCL_GroupingSourceTableCode = jrj.TablePrefix;
			var jobGrouping = GetNewGrouping(new[] { jobLine });
			var jrjGrouping = GetNewGrouping(new[] { jrjLine });

			AssertEquals("H00001000", jobGrouping.SourceNumber);
			AssertEquals("00001000", jrjGrouping.SourceNumber);
		}

		public void TestSourceClientPk_JobRevenueJournal()
		{
			var clientA = Factory.New<OrgHeader>();
			var clientB = Factory.New<OrgHeader>();

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_OA_LocalChargesAddr = clientA.MainAddress.PK;
			var jrj = Factory.New<JobRevenueJournal>();
			jrj.AH_OH = clientB.PK;

			var jobLine = Factory.New<ViewCommissionLine>();
			jobLine.VCL_GroupingSourceID = job.PK;
			jobLine.VCL_GroupingSourceTableCode = job.TablePrefix;
			var jrjLine = Factory.New<ViewCommissionLine>();
			jrjLine.VCL_GroupingSourceID = jrj.PK;
			jrjLine.VCL_GroupingSourceTableCode = jrj.TablePrefix;
			var jobGrouping = GetNewGrouping(new[] { jobLine });
			var jrjGrouping = GetNewGrouping(new[] { jrjLine });

			AssertEquals(clientA.PK, jobGrouping.SourceClientPk);
			AssertEquals(clientB.PK, jrjGrouping.SourceClientPk);
		}

		public void TestFirstRecognitionDate()
		{
			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true).AddToFilter(AccChargeCodeSchema.AC_Code, "CCLR"));
			var aud = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");

			var job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			var job1ARInvoiceA = Factory.NewWithValidTestData<ARInvoice>();
			job1ARInvoiceA.AH_JH = job1.PK;

			var job1ARInvoiceB = Factory.NewWithValidTestData<ARInvoice>();
			job1ARInvoiceB.AH_JH = job1.PK;

			var job1ARInvoiceACommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			job1ARInvoiceACommissionHeader.CH0_AH_Source = job1ARInvoiceA.PK;
			job1ARInvoiceACommissionHeader.CH0_GroupingSourceTableCode = job1.TablePrefix;
			job1ARInvoiceACommissionHeader.CH0_GroupingSourceID = job1.PK;
			job1ARInvoiceACommissionHeader.CH0_CommissionDate = new ZDate(2004, 1, 1);
			var job1ARInvoiceACommissionLineB = job1ARInvoiceACommissionHeader.LineGroups.AddNew(chargeCode).Lines.AddNew();
			job1ARInvoiceACommissionHeader.LineGroups[0].CLG_CommissionDate = new ZDate(2004, 6, 30);
			job1ARInvoiceACommissionLineB.FillWithValidTestData();

			var job1ARInvoiceBCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			job1ARInvoiceBCommissionHeader.CH0_AH_Source = job1ARInvoiceB.PK;
			job1ARInvoiceBCommissionHeader.CH0_GroupingSourceTableCode = job1.TablePrefix;
			job1ARInvoiceBCommissionHeader.CH0_GroupingSourceID = job1.PK;
			job1ARInvoiceBCommissionHeader.CH0_CommissionDate = new ZDate(2005, 1, 1);
			var job1ARInvoiceBCommissionLineA = job1ARInvoiceBCommissionHeader.Lines.AddNew();
			job1ARInvoiceBCommissionLineA.FillWithValidTestData();
			var job1ARInvoiceBCommissionLineB = job1ARInvoiceBCommissionHeader.LineGroups.AddNew(chargeCode).Lines.AddNew();
			job1ARInvoiceBCommissionLineB.FillWithValidTestData();

			Factory.Save();

			var lines = Factory.Load<ViewCommissionLine>(new ZQuery(ViewCommissionLineSchema.VCL_GroupingSourceID, job1.PK));

			var grouping = GetNewGrouping(lines);

			AssertEquals(new ZDate(2004, 6, 30), grouping.FirstRecognitionDate);

			var job1ARInvoiceACommissionLineA = job1ARInvoiceACommissionHeader.Lines.AddNew();
			job1ARInvoiceACommissionLineA.FillWithValidTestData();

			Factory.Save();

			lines = Factory.Load<ViewCommissionLine>(new ZQuery(ViewCommissionLineSchema.VCL_GroupingSourceID, job1.PK));

			grouping = GetNewGrouping(lines);

			AssertEquals(new ZDate(2004, 1, 1), grouping.FirstRecognitionDate);
		}

		public void TestEntityCode()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ADL";
			var party = Factory.New<OrgHeader>();
			party.OH_Code = "TESTORG";

			var adlLine = Factory.New<ViewCommissionLine>();
			adlLine.VCL_GS_NKStaff = "ADL";
			var orgLine = Factory.New<ViewCommissionLine>();
			orgLine.VCL_OH_Party = party.PK;

			var staffGrouping = GetNewGrouping(new[] { adlLine });
			var partyGrouping = GetNewGrouping(new[] { orgLine });

			AssertEquals("ADL", staffGrouping.EntityCode);
			AssertEquals("TESTORG", partyGrouping.EntityCode);
		}

		public void TestEntityName()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ADL";
			staff.GS_FullName = "Andrew";
			var party = Factory.New<OrgHeader>();
			party.OH_Code = "TESTORG";
			party.OH_FullName = "Test Organization";

			var adlLine = Factory.New<ViewCommissionLine>();
			adlLine.VCL_GS_NKStaff = "ADL";
			var partyLine = Factory.New<ViewCommissionLine>();
			partyLine.VCL_OH_Party = party.PK;
			var staffGrouping = GetNewGrouping(new[] { adlLine });
			var partyGrouping = GetNewGrouping(new[] { partyLine });

			AssertEquals("Andrew", staffGrouping.EntityName);
			AssertEquals("Test Organization", partyGrouping.EntityName);
		}

		public void TestTransactionCurrencyDecimalPlaces()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			var grouping = GetNewGrouping(new[] { commissionLine });

			grouping.TransactionCurrencyCode = "";
			AssertEquals(2, grouping.TransactionCurrencyDecimalPlaces);

			grouping.TransactionCurrencyCode = "IDR";
			AssertEquals(0, grouping.TransactionCurrencyDecimalPlaces);

			grouping.TransactionCurrencyCode = "USD";
			AssertEquals(2, grouping.TransactionCurrencyDecimalPlaces);
		}

		public void TestLocalCurrencyDecimalPlaces()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			var grouping = GetNewGrouping(new[] { commissionLine });

			grouping.LocalCurrencyCode = "";
			AssertEquals(2, grouping.LocalCurrencyDecimalPlaces);

			grouping.LocalCurrencyCode = "IDR";
			AssertEquals(0, grouping.LocalCurrencyDecimalPlaces);

			grouping.LocalCurrencyCode = "USD";
			AssertEquals(2, grouping.LocalCurrencyDecimalPlaces);
		}

		public void TestPreferredCurrencyDecimalPlaces()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			var grouping = GetNewGrouping(new[] { commissionLine });

			grouping.PreferredCurrencyCode = "";
			AssertEquals(2, grouping.PreferredCurrencyDecimalPlaces);

			grouping.PreferredCurrencyCode = "IDR";
			AssertEquals(0, grouping.PreferredCurrencyDecimalPlaces);

			grouping.PreferredCurrencyCode = "USD";
			AssertEquals(2, grouping.PreferredCurrencyDecimalPlaces);
		}

		public void TestTotalCommissionableAmount()
		{
			var audLine = Factory.New<ViewCommissionLine>();
			audLine.VCL_RX_NKCommissionCurrency = "AUD";
			audLine.VCL_CommissionToLocalExchangeRate = 0.625m;
			audLine.VCL_RX_NKLocalCurrency = "USD";
			audLine.VCL_LocalToPreferredExchangeRate = 1m;
			audLine.VCL_RX_NKPreferredPaymentCurrency = "AUD";
			audLine.VCL_TotalCommissionableAmount = 1000;

			var usdLine = Factory.New<ViewCommissionLine>();
			usdLine.VCL_RX_NKCommissionCurrency = "USD";
			usdLine.VCL_CommissionToLocalExchangeRate = 1m;
			usdLine.VCL_RX_NKLocalCurrency = "USD";
			usdLine.VCL_LocalToPreferredExchangeRate = 1.6m;
			usdLine.VCL_RX_NKPreferredPaymentCurrency = "AUD";
			usdLine.VCL_TotalCommissionableAmount = 100;

			var gbpLine = Factory.New<ViewCommissionLine>();
			gbpLine.VCL_RX_NKCommissionCurrency = "GBP";
			gbpLine.VCL_CommissionToLocalExchangeRate = 1.25m;
			gbpLine.VCL_RX_NKLocalCurrency = "USD";
			gbpLine.VCL_LocalToPreferredExchangeRate = 1.6m;
			gbpLine.VCL_RX_NKPreferredPaymentCurrency = "AUD";
			gbpLine.VCL_TotalCommissionableAmount = 10;

			var grouping = GetNewGrouping(new[] { audLine, usdLine, gbpLine });

			AssertEquals((ZDecimal)625m +
				100m +
				12.5m,
				grouping.TotalCommissionableAmountInLocalCurrency);

			AssertEquals((ZDecimal)625m +
				160m +
				20m,
				grouping.TotalCommissionableAmountInPreferredCurrency);
		}

		public void TestShareCommissionAmount()
		{
			var audLine = Factory.New<ViewCommissionLine>();
			audLine.VCL_RX_NKCommissionCurrency = "AUD";
			audLine.VCL_CommissionToLocalExchangeRate = 0.625m;
			audLine.VCL_RX_NKLocalCurrency = "USD";
			audLine.VCL_LocalToPreferredExchangeRate = 1m;
			audLine.VCL_RX_NKPreferredPaymentCurrency = "AUD";
			audLine.VCL_ShareCommissionAmount = 1000;

			var usdLine = Factory.New<ViewCommissionLine>();
			usdLine.VCL_RX_NKCommissionCurrency = "USD";
			usdLine.VCL_CommissionToLocalExchangeRate = 1m;
			usdLine.VCL_RX_NKLocalCurrency = "USD";
			usdLine.VCL_LocalToPreferredExchangeRate = 1.6m;
			usdLine.VCL_RX_NKPreferredPaymentCurrency = "AUD";
			usdLine.VCL_ShareCommissionAmount = 100;

			var gbpLine = Factory.New<ViewCommissionLine>();
			gbpLine.VCL_RX_NKCommissionCurrency = "GBP";
			gbpLine.VCL_CommissionToLocalExchangeRate = 1.25m;
			gbpLine.VCL_RX_NKLocalCurrency = "USD";
			gbpLine.VCL_LocalToPreferredExchangeRate = 1.6m;
			gbpLine.VCL_RX_NKPreferredPaymentCurrency = "AUD";
			gbpLine.VCL_ShareCommissionAmount = 10;

			var grouping = GetNewGrouping(new[] { audLine, usdLine, gbpLine });

			AssertEquals((ZDecimal)625m +
				100m +
				12.5m,
				grouping.ShareCommissionAmountInLocalCurrency);

			AssertEquals((ZDecimal)625m +
				160m +
				20m,
				grouping.ShareCommissionAmountInPreferredCurrency);
		}

		public void TestEntityCommissionAmount()
		{
			var audLine = Factory.New<ViewCommissionLine>();
			audLine.VCL_RX_NKCommissionCurrency = "AUD";
			audLine.VCL_CommissionToLocalExchangeRate = 0.625m;
			audLine.VCL_RX_NKLocalCurrency = "USD";
			audLine.VCL_LocalToPreferredExchangeRate = 1.6m;
			audLine.VCL_RX_NKPreferredPaymentCurrency = "AUD";
			audLine.VCL_EntityCommissionAmount = 1000;

			var usdLine = Factory.New<ViewCommissionLine>();
			usdLine.VCL_RX_NKCommissionCurrency = "USD";
			usdLine.VCL_CommissionToLocalExchangeRate = 1m;
			usdLine.VCL_RX_NKLocalCurrency = "USD";
			usdLine.VCL_LocalToPreferredExchangeRate = 1.6m;
			usdLine.VCL_RX_NKPreferredPaymentCurrency = "AUD";
			usdLine.VCL_EntityCommissionAmount = 100;

			var gbpLine = Factory.New<ViewCommissionLine>();
			gbpLine.VCL_RX_NKCommissionCurrency = "GBP";
			gbpLine.VCL_CommissionToLocalExchangeRate = 1.25m;
			gbpLine.VCL_RX_NKLocalCurrency = "USD";
			gbpLine.VCL_LocalToPreferredExchangeRate = 1.6m;
			gbpLine.VCL_RX_NKPreferredPaymentCurrency = "AUD";
			gbpLine.VCL_EntityCommissionAmount = 10;

			var grouping = GetNewGrouping(new[] { audLine, usdLine, gbpLine });

			AssertEquals((ZDecimal)625m +
				100m +
				12.5m,
				grouping.EntityCommissionAmountInLocalCurrency);

			AssertEquals((ZDecimal)1000m +
				160m +
				20m,
				grouping.EntityCommissionAmountInPreferredCurrency);

			AssertEquals((ZDecimal)0,
				grouping.PaidEntityCommissionAmountInPreferredCurrency);

			AssertEquals((ZDecimal)1000m +
				160m +
				20m,
				grouping.OutstandingEntityCommissionAmountInPreferredCurrency);

			usdLine.VCL_PaidDateTimeUtc = new ZDate(2000, 1, 1);
			gbpLine.VCL_PaidDateTimeUtc = new ZDate(2000, 1, 1);

			AssertEquals((ZDecimal)625m +
				100m +
				12.5m,
				grouping.EntityCommissionAmountInLocalCurrency);

			AssertEquals((ZDecimal)1000m +
				160m +
				20m,
				grouping.EntityCommissionAmountInPreferredCurrency);

			AssertEquals((ZDecimal)160m +
				20m,
				grouping.PaidEntityCommissionAmountInPreferredCurrency);

			AssertEquals((ZDecimal)1000m,
				grouping.OutstandingEntityCommissionAmountInPreferredCurrency);
		}

		public void TestCustomerNamesAndCodes()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "SCP";
			org.OH_FullName = "Some Company";

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_JobNum = "H00001000";

			var commissionHeader = Factory.New<AccCommissionHeader>();
			commissionHeader.CH0_OH_Customer = org.PK;

			var jobLine = Factory.New<ViewCommissionLine>();
			jobLine.VCL_GroupingSourceID = job.PK;
			jobLine.VCL_GroupingSourceTableCode = job.TablePrefix;
			jobLine.VCL_CH0 = commissionHeader.PK;

			var jobGrouping = GetNewGrouping(new[] { jobLine });

			AssertEquals("SCP", jobGrouping.CustomerCodes);
			AssertEquals("Some Company", jobGrouping.CustomerNames);
		}

		public void TestTransportModeOriginAndDestination()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUMEL";

			Job job = new Job.Loader(shipment).TryLoadOrCreate();

			var jobLine = Factory.New<ViewCommissionLine>();
			jobLine.VCL_GroupingSourceID = job.PK;
			jobLine.VCL_GroupingSourceTableCode = job.TablePrefix;

			var jobGrouping = GetNewGrouping(new[] { jobLine });

			AssertEquals("AIR", jobGrouping.TransportMode);
			AssertEquals("AUSYD", jobGrouping.Origin);
			AssertEquals("AUMEL", jobGrouping.Destination);
		}

		public void TestPropertyMaxLengths()
		{
			AssertEquals(RefCurrency.Schema.RX_CodeMaxLength, AutoCommissionLineGrouping.Schema.LocalCurrencyCodeMaxLength);
			AssertEquals(RefCurrency.Schema.RX_CodeMaxLength, AutoCommissionLineGrouping.Schema.PreferredCurrencyCodeMaxLength);
			AssertEquals(3, AutoCommissionLineGrouping.Schema.SourceTableCodeMaxLength);
			AssertEquals(GlbStaff.Schema.GS_CodeMaxLength, AutoCommissionLineGrouping.Schema.StaffCodeMaxLength);
			AssertEquals(RefCurrency.Schema.RX_CodeMaxLength, AutoCommissionLineGrouping.Schema.TransactionCurrencyCodeMaxLength);
		}

		#endregion

		#region Implementation

		CommissionLineGroupingForTest GetNewGrouping(IEnumerable<ViewCommissionLine> commissionLines)
		{
			return GetNewGrouping(Factory, commissionLines);
		}

		static CommissionLineGroupingForTest GetNewGrouping(BusinessObjectFactory factory, IEnumerable<ViewCommissionLine> commissionLines)
		{
			var grouping = new CommissionLineGroupingForTest(factory);
			grouping.Init(commissionLines);
			return grouping;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewGrouping(new[] { Factory.New<ViewCommissionLine>() });
		}

		#endregion
	}

	class CommissionLineGroupingForTest : CommissionLineGrouping<CommissionLineGroupingForTest, ViewCommissionLine>
	{
		public CommissionLineGroupingForTest(BusinessObjectFactory factory, ViewCommissionLineGrouper<ViewCommissionLine>[] subGroupers = null)
			: base(factory, subGroupers)
		{
		}

		protected override CommissionLineGroupingCollection<CommissionLineGroupingForTest, ViewCommissionLine> GetNewSubGroupingCollection(BusinessObjectFactory factory)
		{
			return new CommissionLineGroupingCollectionForTest(factory);
		}
	}
}
