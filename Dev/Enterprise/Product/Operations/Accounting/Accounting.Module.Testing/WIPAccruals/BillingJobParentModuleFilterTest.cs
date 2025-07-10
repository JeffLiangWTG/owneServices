using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(BillingJobParentModuleFilter))]
	public class BillingJobParentModuleFilterTest : ModuleFilterTestCase<BillingJobParentModuleFilter>
	{
		public void TestXMLRoundTrip()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var declaration = testObjectCreator.CreateDeclaration("001");
			var jobFordeclaration = testObjectCreator.CreateJobHeader();
			jobFordeclaration.JH_ParentID = declaration.PK;
			var accrualForDeclaration = testObjectCreator.CreateAccrual(jobFordeclaration);

			var shipment = testObjectCreator.CreateShipment("S001001", false);
			var jobForshipment = testObjectCreator.CreateJob(shipment, false, false);
			var accrualForShipment = testObjectCreator.CreateAccrual(jobForshipment);
			var wipForShipment = testObjectCreator.CreateWIP(jobForshipment);

			Factory.Save();

			var filterBizo = new WIPAccrualsFilterBusinessObject();
			var filter = filterBizo.AddFilterStrip<BillingJobParentModuleFilter>(ExpectedDescription);

			filter.IsActive = true;
			filter.SelectedModule = JobInvoicingConsumerTypes.Brokerage.Code;
			filter.Property = declaration.PK;

			var stmModuleFilter = Factory.New<StmModuleFilter>();
			((IModifyModuleAndGridLayout)filterBizo).SerialiseLayoutAndWriteTo(stmModuleFilter);
			filterBizo.LoadLayout(stmModuleFilter, shouldReset: true);
			filter = (BillingJobParentModuleFilter)filterBizo[ExpectedDescription];
			AssertEquals(true, filter.IsActive);
			AssertEquals(declaration.PK, filter.Property);
			AssertEquals(JobInvoicingConsumerTypes.Brokerage.Code, filter.SelectedModule);
		}

		public void TestAllowedComparisonOperators()
		{
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"exact",
				"not equal",
				"filters match"
			}, BillingJobParentModuleFilter.AllowedComparisonOperators);
		}

		public void TestBillingJobParentModuleFilterWithSelectedModule()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var declaration = testObjectCreator.CreateDeclaration("001");
			var jobFordeclaration = testObjectCreator.CreateJobHeader();
			jobFordeclaration.JH_ParentID = declaration.PK;
			var accrualForDeclaration = testObjectCreator.CreateAccrual(jobFordeclaration);

			var shipment = testObjectCreator.CreateShipment("S001001", false);
			var jobForshipment = testObjectCreator.CreateJob(shipment, false, false);
			var accrualForShipment = testObjectCreator.CreateAccrual(jobForshipment);
			var wipForShipment = testObjectCreator.CreateWIP(jobForshipment);

			Factory.Save();

			var filterBizo = new WIPAccrualsFilterBusinessObject();
			var filter = (BillingJobParentModuleFilter)filterBizo[ExpectedDescription];

			filter.IsActive = true;
			filter.SelectedModule = JobInvoicingConsumerTypes.Brokerage.Code;
			filter.Property = declaration.PK;

			var result = Factory.Load<AccTransactionLines>(filterBizo.Filter);
			AssertEquals("The result is accrual when job parent is declaration", accrualForDeclaration.PK, result.Single().PK);

			filter.SelectedModule = JobInvoicingConsumerTypes.Shipment.Code;
			filter.Property = shipment.PK;
			var result2 = Factory.Load<AccTransactionLines>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("The result are WIP or accrual when job parent is shipment", new List<ZGuid>(new ZGuid[] { accrualForShipment.PK, wipForShipment.PK }), result2.Select(x => x.PK));
		}

		public void TestBillingJobParentModuleFilterWithComparisonOperator()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var declaration = testObjectCreator.CreateDeclaration("001");
			var jobFordeclaration = testObjectCreator.CreateJobHeader();
			jobFordeclaration.JH_ParentID = declaration.PK;
			var accrualForDeclaration = testObjectCreator.CreateAccrual(jobFordeclaration);
			var wipForDeclaration = testObjectCreator.CreateWIP(jobFordeclaration);

			var shipment1 = testObjectCreator.CreateShipment("S001001", false);
			var job3 = testObjectCreator.CreateJob(shipment1, false, false);
			var accrualForShipment = testObjectCreator.CreateAccrual(job3);
			var wipForShipment = testObjectCreator.CreateWIP(job3);

			var shipment2 = testObjectCreator.CreateShipment("S001002", false);
			var jobForShipment2 = testObjectCreator.CreateJob(shipment2, false, false);
			var accrualForShipment2 = testObjectCreator.CreateAccrual(jobForShipment2);
			var wipForShipment2 = testObjectCreator.CreateWIP(jobForShipment2);

			Factory.Save();

			var filterBizo = new WIPAccrualsFilterBusinessObject();
			var filter = (BillingJobParentModuleFilter)filterBizo[ExpectedDescription];

			filter.IsActive = true;
			filter.SelectedModule = JobInvoicingConsumerTypes.Shipment.Code;

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = shipment1.PK;
			var result = Factory.Load<AccTransactionLines>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("The filter result when comparison operator is 'exact' and value is not empty.",
				new List<ZGuid>(new ZGuid[] { accrualForShipment.PK, wipForShipment.PK }), result.Select(x => x.PK));

			filter.Property = shipment1.PK;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			result = Factory.Load<AccTransactionLines>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("The filter result when comparison operator is 'not equal' and value is not empty.",
				new List<ZGuid>(new ZGuid[] { accrualForShipment2.PK, wipForShipment2.PK, accrualForDeclaration.PK, wipForDeclaration.PK }), result.Select(x => x.PK));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			filter.SelectedModule = JobInvoicingConsumerTypes.Shipment.Code;
			result = Factory.Load<AccTransactionLines>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("The filter result when comparison operator is 'filters match' and no filter selected.",
				new List<ZGuid>(new ZGuid[] { accrualForShipment.PK, wipForShipment.PK, accrualForShipment2.PK, wipForShipment2.PK }), result.Select(x => x.PK));

			var shipmentModuleFilter = filter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Shipment #");
			shipmentModuleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			shipmentModuleFilter.Property = "S001002";
			result = Factory.Load<AccTransactionLines>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("The filter result when comparison operator is 'filters match' and select shipment filter.",
				new List<ZGuid>(new ZGuid[] { accrualForShipment2.PK, wipForShipment2.PK }), result.Select(x => x.PK));
		}

		public void TestBillingJobParentModuleFilterWithModuleOptions()
		{
			var list = BillingJobParentModuleFilter.ModuleOptions.Cast<ICodeDescription>().Select(m => m.Code).ToArray();

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"ABK",
				"ACD",
				"ACR",
				"AGB",
				"AGS",
				"ASC",
				"ATB",
				"AVA",
				"AWB",
				"BRK",
				"CAE",
				"CLL",
				"CSH",
				"CST",
				"GCN",
				"ISF",
				"LTC",
				"MAN",
				"NCT",
				"PCB",
				"QSH",
				"SHP",
				"TBM",
				"TCW",
				"TDC",
				"TDU",
				"TDL",
				"TRC",
				"TRU",
				"TRN",
				"UBR",
				"WSJ",
				"WIN",
				"WKI",
				"WKP",
				"WKR",
				"WOU",
				"WSC",
				"WST",
				"WVO",
				"YRA",
				"YRE",
				"YTU",
				"MWO",
				"YAO",
				"YPI",
			}, list);
		}

		public void TestCheckPropertyIsEmptyShouldHaveValidationError()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var shipment = testObjectCreator.CreateShipment("11", false);
			Factory.Save();

			foreach (var comparisonOperator in BillingJobParentModuleFilter.AllowedComparisonOperators)
			{
				BillingJobParentModuleFilter.ComparisonOperator = comparisonOperator;
				BillingJobParentModuleFilter.SelectedModule = JobInvoicingConsumerTypes.Shipment.Code;

				BillingJobParentModuleFilter.Property = shipment.PK;
				AssertNoErrors($"The comparison operator is {comparisonOperator} and property is not empty.", Filter.PropertyInfo);

				BillingJobParentModuleFilter.Property = ZGuid.Empty;
				if (comparisonOperator == ModuleTextFilter.ComparisonConstants.FiltersMatch)
				{
					AssertNoErrors($"The comparison operator is {comparisonOperator} and property is empty.", Filter.PropertyInfo);
				}
				else
				{
					AssertHasError($"The comparison operator is {comparisonOperator} and property is empty.", Filter.PropertyInfo, "Please enter a value.");
				}
			}
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override BillingJobParentModuleFilter GetNewModuleFilter()
		{
			return BillingJobParentModuleFilter;
		}

		protected override ZString ExpectedDescription => "Billing Job Parent";

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		protected override Dictionary<string, IZType> GetDummyValuesForCacheInvalidationTest(BillingJobParentModuleFilter filter)
		{
			var values = base.GetDummyValuesForCacheInvalidationTest(filter);
			values.Add(nameof(filter.SelectedModule), new ZString(JobInvoicingConsumerTypes.Shipment.Code));
			return values;
		}

		public override void TestQueryIsEmptyByDefault()
		{
			AssertEquals(false, Filter.Query.IsEmpty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			BillingJobParentModuleFilter = new BillingJobParentModuleFilter(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_JH, Factory);
		}
		BillingJobParentModuleFilter BillingJobParentModuleFilter;
	}
}
