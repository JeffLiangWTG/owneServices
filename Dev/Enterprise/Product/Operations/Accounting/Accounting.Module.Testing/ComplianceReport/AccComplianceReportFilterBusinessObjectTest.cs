using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccComplianceReportFilterBusinessObject))]
	public class AccComplianceReportFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		public void TestReportTypeFilter()
		{
			var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
			var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO["Report Type"];
			AssertNotNull(filter);
			filter.IsActive = true;

			var collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 2, collection.Count);
			Assert("Expecting collection to contain report1", collection.Contains(report1));
			Assert("Expecting collection to contain report2", collection.Contains(report2));

			var list = filter.List as CodeDescriptionPairList;
			AssertNotNull(list);
			AssertEquals("list Count", 0, list.Count);

			Creator.CreateConfigurationForComplianceReport(report1);
			FilterBO = (AccComplianceReportFilterBusinessObject)GetNewFilterStripBusinessObject();
			filter = (ModuleTextFilter)FilterBO["Report Type"];
			AssertNotNull(filter);

			list = filter.List as CodeDescriptionPairList;
			AssertNotNull(list);
			AssertEquals("list Count", 1, list.Count);
			Assert("Expecting Lookup List to contain report1 type", list.ContainsCode(report1.ACR_ReportType));

			Creator.CreateConfigurationForComplianceReport(report2);
			FilterBO = (AccComplianceReportFilterBusinessObject)GetNewFilterStripBusinessObject();
			filter = (ModuleTextFilter)FilterBO["Report Type"];
			AssertNotNull(filter);

			list = filter.List as CodeDescriptionPairList;
			AssertNotNull(list);
			AssertEquals("list Count", 2, list.Count);
			Assert("Expecting Lookup List to contain report1 type", list.ContainsCode(report1.ACR_ReportType));
			Assert("Expecting Lookup List to contain report2 type", list.ContainsCode(report2.ACR_ReportType));

			filter.Property = report1.ACR_ReportType;
			AssertNoErrors(filter.PropertyInfo);
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 1, collection.Count);
			Assert("Expecting collection to contain report1", collection.Contains(report1));

			filter.Property = report2.ACR_ReportType;
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 1, collection.Count);
			Assert("Expecting collection to contain report2", collection.Contains(report2));
		}

		public void TestReportStatusFilter()
		{
			var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
			var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
			Factory.Save();
			AssertEquals("report1.ACR_Status", AccComplianceReport.Status.ReportCreated, report1.ACR_Status);
			AssertEquals("report2.ACR_Status", AccComplianceReport.Status.ReportCreated, report1.ACR_Status);

			var filter = (ModuleTextFilter)FilterBO["Report Status"];
			AssertNotNull(filter);
			filter.IsActive = true;

			var collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 2, collection.Count);
			Assert("Expecting collection to contain report1", collection.Contains(report1));
			Assert("Expecting collection to contain report2", collection.Contains(report2));

			filter.Property = AccComplianceReport.Status.ReportCreated;
			AssertNoErrors(filter.PropertyInfo);
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 2, collection.Count);
			Assert("Expecting collection to contain report1", collection.Contains(report1));
			Assert("Expecting collection to contain report2", collection.Contains(report2));

			report1.ACR_Status = AccComplianceReport.Status.ReportGenerated;

			filter.Property = AccComplianceReport.Status.ReportGenerated;
			AssertNoErrors(filter.PropertyInfo);
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 1, collection.Count);
			Assert("Expecting collection to contain report1", collection.Contains(report1));

			filter.Property = AccComplianceReport.Status.ReportFinalised;
			AssertNoErrors(filter.PropertyInfo);
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 0, collection.Count);
		}

		public void TestReportPeriodicityFilter()
		{
			var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
			report1.ACR_Periodicity = "PER";
			report1.ACR_DateFrom = ZDateTime.Now.AddMonths(-1).Date;
			report1.ACR_DateTo = ZDateTime.Now.Date;
			var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
			report2.ACR_Periodicity = "MNT";
			report2.ACR_DateFrom = ZDateTime.Now.AddMonths(-1).Date;
			report2.ACR_DateTo = ZDateTime.Now.Date;
			var report3 = Factory.NewWithValidTestData<AccComplianceReport>();
			report3.ACR_Periodicity = "RNG";
			report3.ACR_DateFrom = ZDateTime.Now.AddMonths(-1).Date;
			report3.ACR_DateTo = ZDateTime.Now.Date;
			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO["Report Periodicity"];
			AssertNotNull(filter);
			filter.IsActive = true;

			var collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 3, collection.Count);
			Assert("Expecting collection to contain report1", collection.Contains(report1));
			Assert("Expecting collection to contain report2", collection.Contains(report2));
			Assert("Expecting collection to contain report3", collection.Contains(report3));

			filter.Property = report1.ACR_Periodicity;
			AssertNoErrors(filter.PropertyInfo);
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 1, collection.Count);
			Assert("Expecting collection to contain report1", collection.Contains(report1));

			filter.Property = report2.ACR_Periodicity;
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 1, collection.Count);
			Assert("Expecting collection to contain report2", collection.Contains(report2));

			filter.Property = report3.ACR_Periodicity;
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 1, collection.Count);
			Assert("Expecting collection to contain report2", collection.Contains(report3));
		}

		public void TestDescriptionFilter()
		{
			var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
			var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
			var report3 = Factory.NewWithValidTestData<AccComplianceReport>();
			report1.ACR_Description = ZString.Empty;
			report2.ACR_Description = "ABC Description";
			report3.ACR_Description = "BBC Description";
			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO["Report Description"];
			AssertNotNull(filter);
			filter.IsActive = true;

			var collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 3, collection.Count);
			Assert("Expecting collection to contain report1", collection.Contains(report1));
			Assert("Expecting collection to contain report2", collection.Contains(report2));
			Assert("Expecting collection to contain report3", collection.Contains(report3));

			filter.Property = "AB";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 1, collection.Count);
			Assert("Expecting collection to contain report2", collection.Contains(report2));

			filter.Property = "BC";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 0, collection.Count);

			filter.Property = "BC";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 2, collection.Count);
			Assert("Expecting collection to contain report2", collection.Contains(report2));
			Assert("Expecting collection to contain report3", collection.Contains(report3));
		}

		public void TestBranchFilter()
		{
			var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
			var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
			var report3 = Factory.NewWithValidTestData<AccComplianceReport>();
			report1.ACR_GB_Branch = ZGuid.Empty;
			report2.ACR_GB_Branch = GlbBranch.CurrentBranch.PK;
			report3.ACR_GB_Branch = Creator.NonCurrentBranch.PK;
			Factory.Save();

			var filter = (ModuleGuidFilter)FilterBO["Branch"];
			AssertNotNull(filter);
			filter.IsActive = true;

			var collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 3, collection.Count);
			Assert("Expecting collection to contain report1", collection.Contains(report1));
			Assert("Expecting collection to contain report2", collection.Contains(report2));
			Assert("Expecting collection to contain report3", collection.Contains(report3));

			filter.Property = GlbBranch.CurrentBranch.PK;
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 1, collection.Count);
			Assert("Expecting collection to contain report2", collection.Contains(report2));

			filter.Property = Creator.NonCurrentBranch.PK;
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 1, collection.Count);
			Assert("Expecting collection to contain report3", collection.Contains(report3));
		}

		public void TestDateFromFilter()
		{
			var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
			var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
			var report3 = Factory.NewWithValidTestData<AccComplianceReport>();

			report1.ACR_DateFrom = ZDate.Today.AddDays(-30);
			report1.ACR_DateTo = ZDate.Today.AddDays(-5);

			report2.ACR_DateFrom = ZDate.Today.AddDays(-10);
			report2.ACR_DateTo = ZDate.Today.AddDays(10);

			report3.ACR_DateFrom = ZDate.Today.AddDays(5);
			report3.ACR_DateTo = ZDate.Today.AddDays(30);

			Factory.Save();

			var filter = (ModuleDateFilter)FilterBO["Report Post From"];
			AssertNotNull(filter);
			filter.IsActive = true;

			var collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 3, collection.Count);
			Assert("Expecting collection to contain report1", collection.Contains(report1));
			Assert("Expecting collection to contain report2", collection.Contains(report2));
			Assert("Expecting collection to contain report3", collection.Contains(report3));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today;
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 1, collection.Count);
			Assert("Expecting collection to contain report3", collection.Contains(report3));

			filter.Property1 = ZDateTime.Today.AddDays(-10);
			filter.Property2 = ZDateTime.Today;
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 1, collection.Count);
			Assert("Expecting collection to contain report2", collection.Contains(report2));

			filter.Property1 = ZDateTime.Today.AddDays(-30);
			filter.Property2 = ZDateTime.Today.AddDays(-10);
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 2, collection.Count);
			Assert("Expecting collection to contain report1", collection.Contains(report1));
			Assert("Expecting collection to contain report2", collection.Contains(report2));

			filter.Property1 = ZDateTime.Today.AddDays(-40);
			filter.Property2 = ZDateTime.Today.AddDays(-31);
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 0, collection.Count);
		}

		public void TestDateToFilter()
		{
			var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
			var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
			var report3 = Factory.NewWithValidTestData<AccComplianceReport>();

			report1.ACR_DateFrom = ZDate.Today.AddDays(-30);
			report1.ACR_DateTo = ZDate.Today.AddDays(-5);

			report2.ACR_DateFrom = ZDate.Today.AddDays(-10);
			report2.ACR_DateTo = ZDate.Today.AddDays(10);

			report3.ACR_DateFrom = ZDate.Today.AddDays(5);
			report3.ACR_DateTo = ZDate.Today.AddDays(30);

			Factory.Save();

			var filter = (ModuleDateFilter)FilterBO["Report Post To"];
			AssertNotNull(filter);
			filter.IsActive = true;

			var collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 3, collection.Count);
			Assert("Expecting collection to contain report1", collection.Contains(report1));
			Assert("Expecting collection to contain report2", collection.Contains(report2));
			Assert("Expecting collection to contain report3", collection.Contains(report3));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today;
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 2, collection.Count);
			Assert("Expecting collection to contain report2", collection.Contains(report2));
			Assert("Expecting collection to contain report3", collection.Contains(report3));

			filter.Property1 = ZDateTime.Today;
			filter.Property2 = ZDateTime.Today.AddDays(10);
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 1, collection.Count);
			Assert("Expecting collection to contain report2", collection.Contains(report2));

			filter.Property1 = ZDateTime.Today.AddDays(-5);
			filter.Property2 = ZDateTime.Today.AddDays(10);
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 2, collection.Count);
			Assert("Expecting collection to contain report1", collection.Contains(report1));
			Assert("Expecting collection to contain report2", collection.Contains(report2));

			filter.Property1 = ZDateTime.Today.AddDays(31);
			filter.Property2 = ZDateTime.Today.AddDays(40);
			filter.IsActive = true;

			collection = new AccComplianceReportCollection(Factory, FilterBO.Filter);
			AssertEquals("Count", 0, collection.Count);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccComplianceReportFilterBusinessObject();
		}

		TestObjectCreator Creator;
		AccComplianceReportFilterBusinessObject FilterBO;

		protected override void SetUp()
		{
			base.SetUp();

			Creator = new TestObjectCreator(Factory);
			FilterBO = (AccComplianceReportFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		#endregion
	}
}
