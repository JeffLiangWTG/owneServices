using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccountingFilterStripCreator))]
	class AccountingFilterStripCreatorTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccountingFilterStripCreator(new DummyAccountingFilterStripHolder());
		}

		public void TestAddDateFilters()
		{
			var filterStripCreator = new AccountingFilterStripCreator(new DummyAccountingFilterStripHolder());
			ModuleFilterCollection filters = new ModuleFilterCollection();
			filterStripCreator.AddDateFilters(filters);

			Assert("Filters contain Job Open or Close", filters.Filter_List.ContainsCode("Job Open or Close"));
			Assert("Filters contain Job Open", filters.Filter_List.ContainsCode("Job Open"));
			Assert("Filters contain Job Close", filters.Filter_List.ContainsCode("Job Close"));

			Assert("Filters contain Job Revenue Recognition Date", filters.Filter_List.ContainsCode("Job Revenue Recognition Date"));
			Assert("Created filter is of type JobManagementModuleDateFilter", filters["Job Revenue Recognition Date"] is JobManagementModuleDateFilter);
		}

		public void TestAddBillingFiltersHaveCorrectMaxLengths()
		{
			var filterStrip = new DummyAccountingFilterStripHolder();
			var collection = new ModuleFilterCollection();
			var creator = new AccountingFilterStripCreator(filterStrip);

			creator.Initialize();
			creator.AddBillingFilters(collection);

			collection.ForEach(x => Assert(string.Format("{0} filter should have the correct max length", x.Description), checkHasCorrectMaxLength(x)));
		}

		bool checkHasCorrectMaxLength(ModuleFilter filter)
		{
			if (filter is ModuleGuidFilter)
			{
				return true;
			}

			switch (filter.Description)
			{
				case "AP Invoice #":
				case "AR Transaction #":
					return filter.MaxLength == ModuleNumberFilter.MultiplyMaxLength(AccTransactionHeaderSchema.AH_TransactionNum.MaxLength);
				case "Supplier Cost Reference":
					return filter.MaxLength == ModuleNumberFilter.MultiplyMaxLength(JobChargeSchema.JR_CostReference.MaxLength);
				default:
					return false;
			}
		}

		public void TestAddJobInvoicingStatusFilter()
		{
			var filterStripCreator = new AccountingFilterStripCreator(new DummyAccountingFilterStripHolderWithConfigOverrideNames());
			var filters = new ModuleFilterCollection();
			filterStripCreator.AddJobManagementFilters(filters, Env.Security.None);

			Assert("Invoicing Job Status filter", filters.Filter_List.ContainsCode("New Invoice Status label filter"));
		}

		public void TestAddInvoicedChargesFilter()
		{
			var filterStripCreator = new AccountingFilterStripCreator(new DummyAccountingFilterStripHolderWithConfigOverrideNames());
			var filters = new ModuleFilterCollection();
			filterStripCreator.AddJobManagementFilters(filters, Env.Security.None);

			Assert("Invoiced charges filter", filters.Filter_List.ContainsCode("New Invoiced Charges label filter"));
		}

		public void TestJobNotInvoicedSubQueryFilter()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var invoiceLine1 = TestObjectCreator.CreateWIPLineAndCharge(job1.PK, 100m, TestObjectCreator.CC1.AC_Code);

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			var invoiceLine2 = TestObjectCreator.CreateShipmentChargeJobAndLine("S0002", invoice2, TestObjectCreator.ABIGAS, true);
			invoiceLine2.AL_LineType = TransactionLineTypes.Revenue;
			TestObjectCreator.ReverseTransaction(invoice2, out _);

			// the below shipment will be ignored with the filter query
			var invoice3 = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			var invoiceLine3 = TestObjectCreator.CreateShipmentChargeJobAndLine("S0003", invoice3, TestObjectCreator.ABIGAS, true);
			invoiceLine3.AL_LineType = TransactionLineTypes.Revenue;

			Factory.Save();

			var filterStripCreator = new AccountingFilterStripCreator(new DummyAccountingFilterStripHolderWithConfig());
			var getNotInvoicedSubQuery = filterStripCreator.GetNotInvoicedSubQuery_ForTestOnly();

			var parentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
			parentQuery.AddSubQuery((ZDBOnlySubQuery)getNotInvoicedSubQuery, JoinCondition.And);

			var jobShipmentUniqueRefs = Factory.Load<ForwardingShipment>(parentQuery).Select(x => x.JS_UniqueConsignRef);

			AssertEquals("The filtered count should be 2", 2, jobShipmentUniqueRefs.Count());
			AssertCollectionContains("S0001", jobShipmentUniqueRefs);
			AssertCollectionContains("S0002", jobShipmentUniqueRefs);
			AssertCollectionNotContains("S0003", jobShipmentUniqueRefs);
		}

		public void TestOverrideJobInvoicingStatusFilterLabel()
		{
			var filterStripCreator = new AccountingFilterStripCreator(new DummyAccountingFilterStripHolderWithConfig());
			var filters = new ModuleFilterCollection();
			filterStripCreator.AddJobManagementFilters(filters, Env.Security.None);

			Assert("Invoicing Job Status filter", filters.Filter_List.ContainsCode("Invoicing Job Status"));
		}

		public void TestOverrideInvoicedChargesFilterLabel()
		{
			var filterStripCreator = new AccountingFilterStripCreator(new DummyAccountingFilterStripHolderWithConfig());
			var filters = new ModuleFilterCollection();
			filterStripCreator.AddJobManagementFilters(filters, Env.Security.None);

			Assert("Invoiced charges filter", filters.Filter_List.ContainsCode("Invoiced / Charges"));
		}

		public void TestFactoryInstance()
		{
			var filterStrip = new DummyAccountingFilterStripHolder();
			var filterStripCreator = new AccountingFilterStripCreator(filterStrip);
			AssertEquals("AccountingFilterStripCreator must reuse filter strip factory.", filterStrip.Factory, filterStripCreator.Factory);
		}

		public void TestAmountFiltersCategoryNameCustomization()
		{
			var filterHolder = new DummyAccountingFilterStripHolder();
			var filterStripCreator = new AccountingFilterStripCreator(filterHolder);
			var filters = new ModuleFilterCollection();
			filterStripCreator.AddAmountFilters(filters);

			Action<string> assertCategoryName = expectedCategoryName =>
			{
				foreach (var filter in filters)
				{
					AssertEquals(string.Format("filter.Category.Description for {0} filter", filter.Description), expectedCategoryName, filter.Category.Description);
				}
			};
			assertCategoryName("Amount Filters");

			filters = new ModuleFilterCollection();
			filterHolder.AmountFiltersCategoryNameOveride = (NoResString)"Some Amount Category Text";
			filterStripCreator = new AccountingFilterStripCreator(filterHolder);
			filterStripCreator.AddAmountFilters(filters);
			assertCategoryName("Some Amount Category Text");
		}

		public void TestBillingFiltersCategoryNameCustomization()
		{
			var filterHolder = new DummyAccountingFilterStripHolder();
			var filterStripCreator = new AccountingFilterStripCreator(filterHolder);
			var filters = new ModuleFilterCollection();
			filterStripCreator.AddBillingFilters(filters);

			Action<string> assertCategoryName = expectedCategoryName =>
			{
				foreach (var filter in filters)
				{
					AssertEquals(string.Format("filter.Category.Description for {0} filter", filter.Description), expectedCategoryName, filter.Category.Description);
				}
			};
			assertCategoryName("Billing");

			filters = new ModuleFilterCollection();
			filterHolder.BillingFiltersCategoryNameOveride = (NoResString)"Some Billing Category Text";
			filterStripCreator = new AccountingFilterStripCreator(filterHolder);
			filterStripCreator.AddBillingFilters(filters);
			assertCategoryName("Some Billing Category Text");
		}

		public void TestFilterNameSuffixInOtherCategoriesCustomization()
		{
			var filterHolder = new DummyAccountingFilterStripHolder();
			var filterStripCreator = new AccountingFilterStripCreator(filterHolder);
			var filters = new ModuleFilterCollection();
			filterStripCreator.AddJobManagementFilters(filters, Env.Security.None);

			var filtersSupportSuffixes = new[]
				{
					"Job Local Reference",
					"Job Open",
					"Job Close",
					"Job Open or Close",
					"Job Revenue Recognition Date",
					"Job Branch",
					"Job Department",
					"Job Sales Staff",
					"Job Operation Staff",
					"Job Branch Management Code"
				};

			Action<string> assertFilterName = expectedSuffix =>
			{
				var listOfFiltersSupportSuffixes = new List<string>(filtersSupportSuffixes);
				foreach (var filter in filters)
				{
					bool isSuffixSupported = listOfFiltersSupportSuffixes.Contains(filter.Description);
					listOfFiltersSupportSuffixes.Remove(filter.Description);
					var suffix = isSuffixSupported ? expectedSuffix : "";

					AssertEquals("Filter Name", filter.Description + suffix, filter.LocalizedDescription);
				}
				Assert(string.Format("All testing filters must be in the collection. Missed filters: \r\n{0}", new ZStringBuilder(listOfFiltersSupportSuffixes).ToStringWithNewLineBetweenAppends()), listOfFiltersSupportSuffixes.Count == 0);
			};
			assertFilterName("");

			filters = new ModuleFilterCollection();
			filterHolder.FilterNameSuffixInOtherCategories = (NoResString)"(Some Suffix)";
			filterStripCreator = new AccountingFilterStripCreator(filterHolder);
			filterStripCreator.AddJobManagementFilters(filters, Env.Security.None);
			assertFilterName(" (Some Suffix)");
		}

		public void TestARTransactionNumberFilterComparisonOperatorList()
		{
			var filterStripCreator = new AccountingFilterStripCreator(new DummyAccountingFilterStripHolder());
			var filters = new ModuleFilterCollection();
			filterStripCreator.AddBillingFilters(filters);

			var aRTransactionNumberFilter = (ModuleTextFilter)filters["AR Transaction #"];
			var expectedOperators = new string[]
			{
				ModuleTextBaseFilter.ComparisonConstants.Exact,
				ModuleTextBaseFilter.ComparisonConstants.StartsWith,
				ModuleTextBaseFilter.ComparisonConstants.Contains,
				ModuleTextBaseFilter.ComparisonConstants.NotEqual,
				ModuleTextBaseFilter.ComparisonConstants.NotStartsWith,
				ModuleTextBaseFilter.ComparisonConstants.NotContain
			};

			AssertContainsExactElementsInAnyOrder(expectedOperators, aRTransactionNumberFilter.ComparisonOperator_List.GetAllCodesZString());
		}

		public void TestInvoicingJobStatusFilterHaveCorrectMaxLength()
		{
			var filterStripCreator = new AccountingFilterStripCreator(new DummyAccountingFilterStripHolderWithConfig());
			var filters = new ModuleFilterCollection();
			filterStripCreator.AddJobManagementFilters(filters, Env.Security.None);

			var invoicingJobStatusFilter = (ModuleTextFilter)filters["Invoicing Job Status"];

			AssertNotNull(invoicingJobStatusFilter);
			AssertEquals("Invoicing Job Status filter should have the correct max length", JobHeaderSchema.JH_Status.MaxLength, invoicingJobStatusFilter.MaxLength);
		}

		public void TestInvoicingJobStatusFilterComparisonOperatorList()
		{
			var filterStripCreator = new AccountingFilterStripCreator(new DummyAccountingFilterStripHolderWithConfig());
			var filters = new ModuleFilterCollection();
			filterStripCreator.AddJobManagementFilters(filters, Env.Security.None);

			var invoicingJobStatusFilter = (ModuleTextFilter)filters["Invoicing Job Status"];
			var expectedOperators = new string[]
			{
				ModuleTextBaseFilter.ComparisonConstants.Exact,
				ModuleTextBaseFilter.ComparisonConstants.NotEqual
			};

			AssertContainsExactElementsInAnyOrder(expectedOperators, invoicingJobStatusFilter.ComparisonOperator_List.GetAllCodesZString());
		}

		public void TestInvoicingJobStatusWithOPNValue()
		{
			var filterStripCreator = new AccountingFilterStripCreator(new DummyAccountingFilterStripHolder());
			var filters = new ModuleFilterCollection();
			filterStripCreator.AddJobManagementFilters(filters, Env.Security.None);
			var invoicingJobStatusFilter = (ModuleTextFilter)filters["Invoicing Job Status"];
			AssertNotNull(invoicingJobStatusFilter);

			invoicingJobStatusFilter.Property = "OPN";
			invoicingJobStatusFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			invoicingJobStatusFilter.IsActive = true;

			var expectedQuery = JobHeaderSchema.JH_Status.Name + " <> '" + JobHeaderStatus.Closed.Code + "'";
			var filterQuery = filters.GetFilterQuery(new[] { invoicingJobStatusFilter }).LiteralTextSqlFormatted;
			Assert(filterQuery.Contains(expectedQuery));

			invoicingJobStatusFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;

			expectedQuery = JobHeaderSchema.JH_Status.Name + " = '" + JobHeaderStatus.Closed.Code + "'";
			filterQuery = filters.GetFilterQuery(new[] { invoicingJobStatusFilter }).LiteralTextSqlFormatted;
			Assert(filterQuery.Contains(expectedQuery));

			invoicingJobStatusFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			expectedQuery = JobHeaderSchema.JH_Status.Name + " <> '" + JobHeaderStatus.Closed.Code + "'";
			filterQuery = filters.GetFilterQuery(new[] { invoicingJobStatusFilter }).LiteralTextSqlFormatted;
			Assert(filterQuery.Contains(expectedQuery));
		}

		public void TestInvoicingJobStatusCustomFilter()
		{
			var filterStripCreator = new AccountingFilterStripCreator(
				new DummyAccountingFilterStripHolderWithCustomInvoicingJobStatusFilter());
			var filters = new ModuleFilterCollection();
			filterStripCreator.AddJobManagementFilters(filters, Env.Security.None);
			var invoicingJobStatusFilter = (ModuleTextFilter)filters["Invoicing Job Status"];
			AssertNotNull(invoicingJobStatusFilter);

			invoicingJobStatusFilter.Property = "OPN";
			invoicingJobStatusFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			invoicingJobStatusFilter.IsActive = true;

			var filterQuery = filters.GetFilterQuery(new[] { invoicingJobStatusFilter }).LiteralTextSqlFormatted;
			var expectedCustomFilterQuery = JobHeaderSchema.JH_Status.Name + " = 'custom filter value'";
			Assert(
				$"Custom filter should be applied: query \"{filterQuery}\" should contain \"{expectedCustomFilterQuery}\"",
				filterQuery.Contains(expectedCustomFilterQuery));
			var expectedBaseQuery = JobHeaderSchema.JH_GC.Name + " = ";
			Assert(
				$"Custom filter should be able to call original filter: query \"{filterQuery}\" should contain \"{expectedBaseQuery}\"",
				filterQuery.Contains(expectedBaseQuery));
		}

		public void TestInvoicingJobStatusNotExceedMaxLengthAndNoException()
		{
			var filterStripCreator = new AccountingFilterStripCreator(new DummyAccountingFilterStripHolder());
			var filters = new ModuleFilterCollection();
			filterStripCreator.AddJobManagementFilters(filters, Env.Security.None);
			var filter = (ModuleTextFilter)filters["Invoicing Job Status"];
			AssertNotNull(filter);

			filter.Property = "Exceed Max Length";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;

			AssertNoExceptionThrown(() =>
			{
				var filterQuery = filters.GetFilterQuery(new[] { filter }).LiteralTextSqlFormatted.Replace("\t", "").Replace("\r\n", " ").Replace("\r", "");
				AssertNullOrEmpty(filterQuery);
			});
		}

		public void TestShowRevenueFilters()
		{
			var filterStripCreator = new AccountingFilterStripCreator(new DummyAccountingFilterStripHolder());
			var filters = new ModuleFilterCollection();
			filterStripCreator.AddBillingFilters(filters);
			filterStripCreator.AddJobManagementFilters(filters, Env.Security.None);

			var revenueFilters = new[]
				{
					"AR Transaction #",
					"Job Revenue Amount",
					"Job WIP Amount",
					"Job WIP Amount (Deferred Charges Only)",
					"Job WIP Amount (Excluding Deferred Charges)",
					"Job Margin %",
					"Job Profit Amount"
				};

			var allFilterNames = filters.Select(filter => filter.Description.ToString()).ToArray();
			var notFoundRevenueFilterNames = (from name in revenueFilters
											  where !allFilterNames.Contains(name)
											  select name).ToArray();
			Assert(string.Format("All revenue filters must be in the collection. Missed filters: \r\n{0}", new ZStringBuilder(notFoundRevenueFilterNames).ToStringWithNewLineBetweenAppends()), !notFoundRevenueFilterNames.Any());

			var filterNamesWithoutRevenueFilters = from name in allFilterNames
												   where !revenueFilters.Contains(name)
												   select name;
			filters = new ModuleFilterCollection();
			filterStripCreator.Initialize(false);
			filterStripCreator.AddBillingFilters(filters);
			filterStripCreator.AddJobManagementFilters(filters, Env.Security.None);
			AssertContainsExactElementsInAnyOrder(filterNamesWithoutRevenueFilters, filters.Select(filter => filter.Description.ToString()));

			var filterNamesWithoutSupplierCostReferenceFilterFilters = from name in allFilterNames
																	   where name != "Supplier Cost Reference"
																	   select name;
			filters = new ModuleFilterCollection();
			filterStripCreator.Initialize(addSupplierCostReferenceFilters: false);
			filterStripCreator.AddBillingFilters(filters);
			filterStripCreator.AddJobManagementFilters(filters, Env.Security.None);
			AssertContainsExactElementsInAnyOrder(filterNamesWithoutSupplierCostReferenceFilterFilters, filters.Select(filter => filter.Description.ToString()));
		}

		public void TestJobProfitQueryDoesNotUseCaseStatements()
		{
			var filterStripCreator = new AccountingFilterStripCreator(new DummyAccountingFilterStripHolder());
			var filters = new ModuleFilterCollection();
			filterStripCreator.AddAmountFilters(filters);

			var profitFilter = (JobManagementAmountFilter)filters["Job Profit Amount"];
			profitFilter.SqlComparisonOperator = SQLComparisonOperator.GreaterThan;
			profitFilter.Property = 1500m;
			profitFilter.IsActive = true;

			var expectedProfitQuery = "( ( SELECT SUM ( X.Amount ) FROM  " +
					"( SELECT CAST ( AL_LineAmount AS DECIMAL ( 24,9 ) )  AS Amount FROM dbo.AccTransactionLines   " +
					"WHERE AL_JH = JH_PK  AND AL_LineType IN  ( 'CST', 'REV' ) " +
					"UNION ALL " +
					"SELECT CAST ( -AL_LineAmount AS DECIMAL ( 24,9 ) )  AS Amount FROM dbo.AccTransactionLines   " +
					"WHERE AL_JH = JH_PK  AND AL_ReverseDate IS NULL  AND AL_LineType IN  ( 'WIP', 'ACR' ) )" +
					"  AS X )  >  1500 )";
			var filterQuery = filters.GetFilterQuery(new[] { profitFilter }).LiteralTextSqlFormatted.Replace("\t", "").Replace("\r\n", " ").Replace("\r", "");

			Assert(!filterQuery.Contains("CASE", StringComparison.InvariantCultureIgnoreCase));
			Assert(filterQuery.Contains(expectedProfitQuery));
		}

		public void TestGetBaseJobSubQuery_GetInactiveJob()
		{
			var activeJob = Factory.NewWithValidTestData<JobManagement>();
			activeJob.JH_GB = GlbBranch.CurrentBranch.PK;
			activeJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			activeJob.JH_GC = GlbCompany.CurrentCompany.PK;

			var deactiveJob = Factory.NewWithValidTestData<JobManagement>();
			deactiveJob.JH_GB = GlbBranch.CurrentBranch.PK;
			deactiveJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			deactiveJob.JH_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			deactiveJob.MarkAsInactive();

			Factory.Save();

			var filterStripCreator = new AccountingFilterStripCreator(new DummyAccountingFilterStripHolder());
			var getBaseJobSubQuery = filterStripCreator.GetBaseJobSubQuery_ForTestOnly();

			var jobPKs = Factory.Load<JobHeader>(getBaseJobSubQuery).Select(x => x.PK);

			AssertCollectionContains("GetBaseJobSubQuery can get active job.", activeJob.PK, jobPKs);
			AssertCollectionContains("GetBaseJobSubQuery can get inactive job.", deactiveJob.PK, jobPKs);
		}

		public void TestGetOnlyActiveJobHeaderForParentTableJobFilters()
		{
			var activeJob = Factory.NewWithValidTestData<JobManagement>();
			activeJob.JH_GB = GlbBranch.CurrentBranch.PK;
			activeJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			activeJob.JH_GC = GlbCompany.CurrentCompany.PK;

			var inactiveJob = Factory.NewWithValidTestData<JobManagement>();
			inactiveJob.JH_GB = GlbBranch.CurrentBranch.PK;
			inactiveJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			inactiveJob.JH_GC = GlbCompany.CurrentCompany.PK;

			var shipmentWithActiveHeader = Factory.NewWithValidTestData<ForwardingShipment>();
			activeJob.JH_ParentID = shipmentWithActiveHeader.PK;

			var shipmentWithInactiveHeader = Factory.NewWithValidTestData<ForwardingShipment>();
			inactiveJob.JH_ParentID = shipmentWithInactiveHeader.PK;

			Factory.Save();

			inactiveJob.MarkAsInactive();

			Factory.Save();

			var filterStripCreator = new AccountingFilterStripCreator(new DummyAccountingFilterStripHolderForParentTable());
			var getBaseJobSubQuery = filterStripCreator.GetBaseJobSubQuery_ForTestOnly();

			ZDBOnlyQuery parentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
			parentQuery.AddSubQuery((ZDBOnlySubQuery)getBaseJobSubQuery, JoinCondition.And);

			var jobPKs = Factory.Load<ForwardingShipment>(parentQuery).Select(x => x.PK);

			AssertCollectionContains("Jobs with active job header gets selected in jobs filter output.", shipmentWithActiveHeader.PK, jobPKs);
			AssertCollectionNotContains("Jobs with inactive job header does not get selected in jobs filter output.", shipmentWithInactiveHeader.PK, jobPKs);
		}

		public void TestChargesWithManyDebtorsAndCreditors_OnlyIncludeJobChargeSubQueryOnce()
		{
			var dummyFilterStripHolder = new DummyAccountingFilterStripHolderWithChargeWithDebtorAndCreditorFilter();
			var filterStripCreator = new AccountingFilterStripCreator(dummyFilterStripHolder);
			var filters = new ModuleFilterCollection();
			var activeModuelfiltesforquery = new List<ModuleFilter>();
			filterStripCreator.AddBillingFilters(filters);

			var debtorFilter = (ModuleGuidFilter)filters[filterStripCreator.ChargesWithDebtor];
			debtorFilter.IsActive = true;
			debtorFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			debtorFilter.Property = ZGuid.NewZGuid();
			activeModuelfiltesforquery.Add(debtorFilter);

			var description = debtorFilter.Description;
			debtorFilter = (ModuleGuidFilter)filters.GetVisibleModuleFilterAndDuplicateAndDeactivateIfActive(description);
			debtorFilter.IsActive = true;
			debtorFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			debtorFilter.Property = ZGuid.NewZGuid();
			activeModuelfiltesforquery.Add(debtorFilter);

			var creditorFilter = (ModuleGuidFilter)filters[filterStripCreator.ChargesWithCreditor];
			creditorFilter.IsActive = true;
			creditorFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			creditorFilter.Property = ZGuid.NewZGuid();
			description = creditorFilter.Description;
			activeModuelfiltesforquery.Add(creditorFilter);

			creditorFilter = (ModuleGuidFilter)filters.GetVisibleModuleFilterAndDuplicateAndDeactivateIfActive(description);
			creditorFilter.IsActive = true;
			creditorFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			creditorFilter.Property = ZGuid.NewZGuid();
			activeModuelfiltesforquery.Add(creditorFilter);

			var filterQuery = filters.GetFilterQuery(activeModuelfiltesforquery).LiteralTextSqlFormatted;
			var strToFind = @"SELECT JR_JH FROM dbo.JobCharge";
			var first = filterQuery.IndexOf(strToFind);
			var onlyOneJobCharge = first != -1 && first == filterQuery.LastIndexOf(strToFind);
			var allIndexes = filterQuery.AllIndexesOf(strToFind).Select(i => i.ToString()).ToArray();
			Assert($"Expected one instance of JobCharge subquery, but found {allIndexes.Length} at indexes {string.Join(",", allIndexes)}", onlyOneJobCharge);
		}

		public void TestAddWIPSubQuery_GeneratesValidSQL()
		{
			var filterQuery = CreateWIPAmountFilterQuery("Job WIP Amount", 1500m, SQLComparisonOperator.Equal);

			AssertEquals(filterQuery.Contains("--WipOrAcrRecognized"), true);
			AssertEquals("Comment should not break into multiple lines.", filterQuery.Contains("--Wip\nOR\nAcr Recognized"), false);
		}

		public void TestAddWIPSubQueryConsideringDeferredCharges_GeneratesValidSQL()
		{
			var filterQuery = CreateWIPAmountFilterQuery("Job WIP Amount (Deferred Charges Only)", 1500m, SQLComparisonOperator.Equal);

			AssertEquals(filterQuery.Contains("--WipOrAcrRecognized"), true);
			AssertEquals("Comment should not break into multiple lines.", filterQuery.Contains("--Wip\nOR\nAcr Recognized"), false);

			filterQuery = CreateWIPAmountFilterQuery("Job WIP Amount (Excluding Deferred Charges)", 1500m, SQLComparisonOperator.Equal);

			AssertEquals(filterQuery.Contains("--WipOrAcrRecognized"), true);
			AssertEquals("Comment should not break into multiple lines.", filterQuery.Contains("--Wip\nOR\nAcr Recognized"), false);
		}

		string CreateWIPAmountFilterQuery(string filterName, decimal amount, SQLComparisonOperator comparisonOperator)
		{
			var filterStripCreator = new AccountingFilterStripCreator(new DummyAccountingFilterStripHolder());
			var filters = new ModuleFilterCollection();
			filterStripCreator.AddAmountFilters(filters);

			var customSQLFilter = new ModuleSQLFilter("Custom SQL Filter", typeof(ForwardingShipment))
			{
				IsActive = true,
				Property1 = "1=1"
			};

			filters.AddCustomFilter(customSQLFilter);

			var wipFilter = (JobManagementAmountFilter)filters[filterName];
			wipFilter.SqlComparisonOperator = comparisonOperator;
			wipFilter.Property = amount;
			wipFilter.IsActive = true;

			return filters.GetFilterQuery(new ModuleFilter[] { wipFilter, customSQLFilter }).LiteralTextSqlFormatted;
		}

		class DummyAccountingFilterStripHolderForParentTable : DummyAccountingFilterStripHolder, IAccountingFilterStripHolder
		{
			ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;
		}

		class DummyAccountingFilterStripHolderWithCustomInvoicingJobStatusFilter : DummyAccountingFilterStripHolder, IAccountingFilterStripHolder
		{
			Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>()
			{
				{
					AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterMakeCustomFilter,
					(Func<ZDBOnlyQuery, ZDBOnlyQuery>)(query =>
					{
						query.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_Status, SQLComparisonOperator.Equal, "custom filter value");
						return query;
					})
				}
			};
		}

		class DummyAccountingFilterStripHolderWithConfigOverrideNames : DummyAccountingFilterStripHolder, IAccountingFilterStripHolder
		{
			Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>()
			{
				{ AccountingFilterStripConfigurationKeys.BusinessObjectType, typeof(ForwardingShipment) },
				{ AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("Forwarding|JobShipmentFilter|InvoiceStatus", "New Invoice Status label filter") },
				{ AccountingFilterStripConfigurationKeys.InvoicedChargesFilterNameOverride, ResString.GetMultilingualString("Forwarding|JobShipmentFilter|InvoicedCharges", "New Invoiced Charges label filter") }
			};
		}

		class DummyAccountingFilterStripHolderWithConfig : DummyAccountingFilterStripHolder, IAccountingFilterStripHolder
		{
			Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>()
			{
				{ AccountingFilterStripConfigurationKeys.BusinessObjectType, typeof(ForwardingShipment) }
			};
		}

		class DummyAccountingFilterStripHolder : IAccountingFilterStripHolder
		{
			#region IAccountingFilterStripHolder Members

			public BusinessObjectFactory Factory
			{
				get { return factory ?? (factory = new BusinessObjectFactory()); }
			}
			BusinessObjectFactory factory;

			ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => false;

			Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => null;

			public ZQuery TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
			{
				return billingPKSubQuery;
			}

			MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride
			{
				get { return this.AmountFiltersCategoryNameOveride; }
			}

			public MultilingualString AmountFiltersCategoryNameOveride;

			MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride
			{
				get { return this.BillingFiltersCategoryNameOveride; }
			}

			public MultilingualString BillingFiltersCategoryNameOveride;

			MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories
			{
				get { return this.FilterNameSuffixInOtherCategories; }
			}

			public MultilingualString FilterNameSuffixInOtherCategories;

			#endregion
		}

		class DummyAccountingFilterStripHolderWithChargeWithDebtorAndCreditorFilter : DummyAccountingFilterStripHolderForParentTable, IAccountingFilterStripHolder
		{
			ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
			{
				ZDBOnlyQuery parentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
				parentQuery.AddSubQuery(billingPKSubQuery, JoinCondition.And);
				return parentQuery;
			}
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
