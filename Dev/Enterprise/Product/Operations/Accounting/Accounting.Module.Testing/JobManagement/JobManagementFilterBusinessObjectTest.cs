using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(JobManagementFilterBusinessObject))]
	public class JobManagementFilterBusinessObjectTest : JobManagementFilterBusinessObjectBaseTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			FilterBO = (JobManagementFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		JobManagementFilterBusinessObject FilterBO;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new JobManagementFilterBusinessObject();
		}

		#region Job Header Tests

		public void TestValidateProfitLossReason()
		{
			FilterBO = new JobManagementFilterBusinessObject();
			AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.Value.RemoveAndDeleteAll();
			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Profit/Loss Reason"];
			Assert("Must have warning", filter.PropertyInfo.HasWarning("Job Profit/Loss Reason Codes registry item (Accounting/Job Invoicing/Job Profit Reason) is empty. Please set correct values."));
			JobProfitLossReasonCodeCollection collection = new JobProfitLossReasonCodeCollection();
			JobProfitLossReasonCode reasonCode = collection.AddNew();
			reasonCode.Code = "TST";
			AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
			FilterBO = new JobManagementFilterBusinessObject();
			filter = (ModuleTextFilter)FilterBO["Profit/Loss Reason"];
			Assert("Mustn't have warning", !filter.PropertyInfo.HasWarning("Job Profit/Loss Reason Codes registry item (Accounting/Job Invoicing/Job Profit Reason) is empty. Please set correct values."));
		}

		public void TestSpotQuotesExcludedWithMissingInvalidJobParentFilter()
		{
			var flagsFilter = (ModuleFlagsFilter)FilterBO["Missing/Invalid Job Parent"];
			flagsFilter["Jobs without a valid parent"] = true;
			flagsFilter.IsActive = true;

			var shipment = TestObjectCreator.CreateShipment("S001");
			Job2.JH_ParentTableCode = "JS";
			Job2.JH_ParentID = shipment.PK;
			Factory.Save();

			var spotQuoteJob = Factory.NewJobForTesting<JobHeader>();
			spotQuoteJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			spotQuoteJob.JH_GB = GlbBranch.CurrentBranch.PK;
			spotQuoteJob.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
			spotQuoteJob.JH_JobNum = "Job1";
			Factory.Save();

			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to not contain spotQuote", !FilterCollection.Contains(spotQuoteJob));
			Assert("Expecting collection to not contain Job2 because it has a valid parent", !FilterCollection.Contains(Job2));
		}

		public void TestMissingInvalidJobParentFilterDoesNotIncludeNonForwardingConsolsOnlyQuery()
		{
			Assert("job management default query exclude JobConsol if no parent", FilterBO.Filter.FilterString.Contains(JobConsolSchema.Constants.TableName));

			var flagsFilter = (ModuleFlagsFilter)FilterBO["Missing/Invalid Job Parent"];
			flagsFilter["Jobs without a valid parent"] = true;
			flagsFilter.IsActive = true;

			Assert("This filter should not exclude these jobConsols", !FilterBO.Filter.FilterString.Contains(JobConsolSchema.Constants.TableName));
		}

		public void TestMissingInvalidJobParentFilter()
		{
			var newFactory = new BusinessObjectFactory();
			var newTestObjectCreator = new TestObjectCreator(newFactory);

			var jobTypesToIgnore = new string[]
			{
					JobInvoicingConsumerTypes.QuotedBookingCode,
					JobInvoicingConsumerTypes.ForwardingConsolCode,
					JobInvoicingConsumerTypes.GatewayConsolCode,
					JobInvoicingConsumerTypes.OrganisationCode,
					JobInvoicingConsumerTypes.CAeManifestCode,
					JobInvoicingConsumerTypes.WorkRequestCode,
					JobInvoicingConsumerTypes.PostClearanceBrokerageCode,
					JobInvoicingConsumerTypes.TransitDispatchLoadListCode,
					JobInvoicingConsumerTypes.TransitReceiveTransportationUnitCode,
					JobInvoicingConsumerTypes.TransitDispatchTransportationUnitCode,
			};

			var jobTypesForTesting = JobInvoicingConsumerTypes.New().Cast<JobInvoicingConsumerType>().Where(x => !jobTypesToIgnore.Contains(x.Code)).ToList();

			foreach (var jobType in jobTypesForTesting)
			{
				TestObjectCreator.CreateJob(newTestObjectCreator.CreateJobPlugIn(jobType), false);
			}

			Job1.JH_ParentID = ZGuid.Empty;

			var shipment = TestObjectCreator.CreateShipment("S001");
			Job2.JH_ParentTableCode = "JS";
			Job2.JH_ParentID = shipment.PK;

			Factory.Save();

			AssertNotNull((FilterBO["Missing/Invalid Job Parent"]));
			var flagsFilter = (ModuleFlagsFilter)FilterBO["Missing/Invalid Job Parent"];
			flagsFilter["Jobs without a valid parent"] = false;
			flagsFilter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
			Assert("JK job is filtered by default filter", !FilterCollection.Cast<JobHeader>().Any(x => x.JH_ParentTableCode == JobConsolSchema.Constants.Prefix));
			AssertEquals("Expecting collection to not contain Job1", jobTypesForTesting.Count, FilterCollection.Count);

			flagsFilter["Jobs without a valid parent"] = true;

			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to not contain Job2", !FilterCollection.Contains(Job2));
			Assert("JK job is not filtered when the new filter is used", FilterCollection.Cast<JobHeader>().Any(x => x.JH_ParentTableCode == JobConsolSchema.Constants.Prefix));
			AssertEquals(jobTypesForTesting.Count, FilterCollection.Count);
		}

		public void TestProfitLossReasonQuery()
		{
			Job1.JH_ProfitLossReasonCode = "CD1";
			Job2.JH_ProfitLossReasonCode = "CD2";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Profit/Loss Reason"];

			filter.Property = "CD1";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));

			filter.Property = "CD2";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
		}

		public void TestProfitLossReasonFilterOperatorList()
		{
			var filter = (ModuleTextFilter)FilterBO["Profit/Loss Reason"];
			var expectedOperators = new string[]
			{
				ModuleTextBaseFilter.ComparisonConstants.Exact,
				ModuleTextBaseFilter.ComparisonConstants.StartsWith,
				ModuleTextBaseFilter.ComparisonConstants.Contains,
				ModuleTextBaseFilter.ComparisonConstants.NotEqual,
				ModuleTextBaseFilter.ComparisonConstants.NotStartsWith,
				ModuleTextBaseFilter.ComparisonConstants.NotContain,
				ModuleTextBaseFilter.ComparisonConstants.IsBlank,
				ModuleTextBaseFilter.ComparisonConstants.IsNotBlank
			};

			Assert("Expect to have comparisson operators", filter.HasComparisonOperator);
			AssertContainsExactElementsInAnyOrder(expectedOperators, filter.ComparisonOperator_List.GetAllCodesZString());
		}

		public void TestProfitLossReasonFilterWithOperators()
		{
			Job1.JH_ProfitLossReasonCode = "ND1";
			Job2.JH_ProfitLossReasonCode = "CD2";
			Job3.JH_ProfitLossReasonCode = string.Empty;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO["Profit/Loss Reason"];

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "N";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			filter.Property = "N";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			filter.Property = "N";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "CD2";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.Property = "CD2";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));

			Job1.JH_ProfitLossReasonCode = ZString.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Job3", FilterCollection.Contains(Job3));
			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
		}

		public void TestJobNumberFilterDescription()
		{
			var filter = (ModuleNumberFilter)FilterBO[AccountingUtils.NumberFilterTypes.JobNumber];
			AssertEquals("Localized Filter Name", filter.LocalizedDescription, AccountingUtils.NumberFilterTypes.JobNumber);
		}

		#region DSB Job Close Batch Filter

		Job PrepareAccTransactionLinesWithDSBJobCloseBatch(string num)
		{
			var batch = TestObjectCreator.CreateDsbJobCloseBatch("B" + num);

			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "J" + num;
			var charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Charge", TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 100m, TestObjectCreator.Agent);
			charge.JR_APInvoiceNum = "INV" + num;
			charge.JR_APInvoiceDate = ZDateTime.Now;
			new InvoicingPostManager(testJob).CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();
			var lines = Factory.LoadTop1<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.AL_JH, testJob.PK).AddToFilter(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.Cost));
			lines.AL_JBB = batch.PK;
			Factory.Save();

			return testJob;
		}

		public void TestDSBJobCloseBatchFilter()
		{
			var filterCode = "Disbursement Job Close Batch #";

			var testJob1 = PrepareAccTransactionLinesWithDSBJobCloseBatch("001");
			var testJob2 = PrepareAccTransactionLinesWithDSBJobCloseBatch("002");

			using (AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ModuleFilterCollection filters = FilterBO.ModuleFilters;
				Assert(filters.Filter_List.ContainsCode(filterCode));

				var filter = (ModuleTextFilter)FilterBO[filterCode];
				filter.Property = "B001";
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.IsActive = true;

				FilterCollection.Load(FilterBO.Filter);
				Assert(FilterCollection.Contains(testJob1));
				Assert(!FilterCollection.Contains(testJob2));

				filter.Property = "B00";
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				FilterCollection.Load(FilterBO.Filter);
				Assert(FilterCollection.Contains(testJob1));
				Assert(FilterCollection.Contains(testJob2));
			}

			using (AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				FilterBO = (JobManagementFilterBusinessObject)GetNewFilterStripBusinessObject();
				ModuleFilterCollection filters = FilterBO.ModuleFilters;
				Assert(!filters.Filter_List.ContainsCode(filterCode));
			}
		}

		#endregion

		#endregion

		#region Get Module Filters test
		readonly string[] expectedJobManagementFilterStripList = new string[]
		{
				"Job Status",
				"Job #",
				"Job Local Client",
				"Job Overseas Agent",

				"Job Branch",
				"Job Department",
				"Job Operation Staff",
				"Job Sales Staff",

				"Job Open or Close",
				"Job Open",
				"Job Close",
				"Job Revenue Recognition Date",

				"Consol #",
				"House Bill #",
				"Master Bill #",
				"Flight/Voyage # and Vessel",
				"Customs Entry #",
				"Order #",
				"Container #",
				"Carrier/Principal",
				"Job Local Reference",

				"No Recognized Date",

				"Job Profit Amount",
				"Job Revenue Amount",
				"Job Cost Amount",
				"Job WIP Amount",
				"Job WIP Amount (Excluding Deferred Charges)",
				"Job WIP Amount (Deferred Charges Only)",
				"Job Accrual Amount",
				"Job Margin %",

				"Profit/Loss Reason"
		};

		public void TestGetModuleFilters()
		{
			ModuleFilterCollection filters = FilterBO.ModuleFilters;

			foreach (string code in expectedJobManagementFilterStripList)
			{
				Assert("Job management Filter strip contains code: " + code, filters.Filter_List.ContainsCode(code));
			}
		}

		#endregion
	}
}
