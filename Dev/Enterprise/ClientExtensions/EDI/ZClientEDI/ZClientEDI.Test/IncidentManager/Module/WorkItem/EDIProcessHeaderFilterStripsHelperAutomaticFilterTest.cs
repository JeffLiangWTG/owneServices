using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(EDIProcessHeaderFilterStripsHelper))]
	public class EDIProcessHeaderFilterStripsHelperAutomaticFilterTest : AutomaticFilterTest
	{
		public void TestReleaseGroupFilter()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();

			group1.GG_Code = "GG1";
			group2.GG_Code = "GG2";

			var dummyBO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummyBO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummyBO3 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummyBO4 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummyBO5 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var flow1 = dummyBO1.Workflows.AddNew();
			var flow2 = dummyBO2.Workflows.AddNew();
			var flow3 = dummyBO3.Workflows.AddNew();
			var flow4 = dummyBO4.Workflows.AddNew();
			var flow5 = dummyBO5.Workflows.AddNew();

			flow1.FH_ParentId = dummyBO1.PK;
			flow2.FH_ParentId = dummyBO2.PK;
			flow3.FH_ParentId = dummyBO3.PK;
			flow4.FH_ParentId = dummyBO4.PK;
			flow5.FH_ParentId = dummyBO5.PK;

			flow1.FH_WorkflowType = "WKI";
			flow2.FH_WorkflowType = "WKI";
			flow3.FH_WorkflowType = "WKI";
			flow4.FH_WorkflowType = "WKI";
			flow5.FH_WorkflowType = "WKI";

			flow1.FH_GG_ReleaseGroup = group1.PK;
			flow2.FH_GG_ReleaseGroup = group1.PK;
			flow3.FH_GG_ReleaseGroup = group2.PK;
			flow4.FH_GG_ReleaseGroup = group2.PK;

			Factory.Save();

			var allFilters = new DummyFilterStripBizOWithProcessHeaderFilters(Factory);
			var filter = ((ModuleGuidFilter)allFilters["Release Group"]);
			AssertNotNull(filter);
			AssertEquals("Job Workflow", filter.Category.Description);

			filter.Property = group1.PK;
			filter.IsActive = true;

			var results = Factory.Load<DummyWithWorkflow>(allFilters.Filter);
			AssertEquals(2, results.Length);
			Assert("BO1 should be in results", results.Any(x => x.PK == dummyBO1.PK));
			Assert("BO2 should be in results", results.Any(x => x.PK == dummyBO2.PK));
			Assert("BO3 should not be in results", !results.Any(x => x.PK == dummyBO3.PK));
			Assert("BO3 should not be in results", !results.Any(x => x.PK == dummyBO4.PK));
			Assert("BO5 should not be in results", !results.Any(x => x.PK == dummyBO5.PK));
		}

		public override void SetUpForHelperFiltersWorkTests(Type businessObjectType)
		{
		}
	}

	class DummyFilterStripBizOWithProcessHeaderFilters : DummyFilterStripBusinessObject
	{
		public DummyFilterStripBizOWithProcessHeaderFilters(BusinessObjectFactory factory) : base(factory)
		{
			base.QueryObjectType = typeof(DummyBusinessObjectWithWorkflow);
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var filterList = base.GetCustomFilterStripsHelpersCore();
			filterList.Add(new EDIProcessHeaderFilterStripsHelper(typeof(DummyBusinessObjectWithWorkflow), base.Factory));

			return filterList;
		}
	}
}
