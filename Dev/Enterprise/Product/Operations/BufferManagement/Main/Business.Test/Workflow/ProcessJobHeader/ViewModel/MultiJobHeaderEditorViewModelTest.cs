using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(MultiJobHeaderEditorViewModel))]
	class MultiJobHeaderEditorViewModelTest : NonPersistentBusinessObjectTestCase
	{
		#region Properties

		[TestDate(2015, 1, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestProperties()
		{
			BMSTestCaseWithFactory.EnableBMSInRegistry();
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			job1.O1_CompanyName = "Company1";
			job1.O1_LeadUniqueReference = "jobcode1";
			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();
			job2.O1_CompanyName = "Company2";
			job2.O1_LeadUniqueReference = "jobcode2";
			var job3 = Factory.NewWithValidTestData<SalesEnquiry>();
			job3.O1_CompanyName = "Company3";
			job3.O1_LeadUniqueReference = "jobcode3";

			var jobHeader1 = BMSTestHelper.CreateJobHeader(job1, false, description: "test1");
			jobHeader1.FH_DoNotStartBeforeDate = ZDateTime.UtcNow;
			jobHeader1.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(1);

			var jobHeader2 = BMSTestHelper.CreateJobHeader(job2, false, description: "test2");
			var jobHeader3 = BMSTestHelper.CreateJobHeader(job3, false, description: "test3");

			Factory.Save();

			var viewModel = new MultiJobHeaderEditorViewModel(new[] { job1, job2, job3 }, Factory);
			AssertEquals(false, viewModel.HasChanges);

			AssertEquals(3, viewModel.JobHeaderViews.Count);
			var view1 = viewModel.JobHeaderViews[0];
			var view2 = viewModel.JobHeaderViews[1];
			var view3 = viewModel.JobHeaderViews[2];

			AssertEquals("test1", view1.Description);
			AssertEquals("test2", view2.Description);
			AssertEquals("test3", view3.Description);

			AssertEquals("Inquiry (jobcode1)", view1.Job);
			AssertEquals("Inquiry (jobcode2)", view2.Job);
			AssertEquals("Inquiry (jobcode3)", view3.Job);

			AssertEquals("jobcode1", view1.JobDescription);
			AssertEquals("jobcode2", view2.JobDescription);
			AssertEquals("jobcode3", view3.JobDescription);

			AssertEquals("Should default jobHeader's schedule onto view", new ZDateTime(2015, 1, 14, 10, 0, 0), view1.EarliestStartDateLocal);
			AssertEquals("Should default jobHeader's schedule onto view", new ZDateTime(2015, 1, 15, 10, 0, 0), view1.AgreedDeliveryDateLocal);
			AssertEquals(ZDateTime.Empty, view2.EarliestStartDateLocal);
			AssertEquals(ZDateTime.Empty, view2.AgreedDeliveryDateLocal);
			AssertEquals(ZDateTime.Empty, view3.EarliestStartDateLocal);
			AssertEquals(ZDateTime.Empty, view3.AgreedDeliveryDateLocal);

			view1.EarliestStartDateLocal = ZDateTime.Now.AddDays(2);
			AssertEquals("viewModel should now have changes since a child was edited", true, viewModel.HasChanges);
			AssertEquals("jobHeader should not have changes yet until save", false, jobHeader1.HasChanges);
			AssertEquals(ZDateTime.Now, jobHeader1.DoNotStartBeforeDateLocal);
			AssertEquals(ZDateTime.Now.AddDays(1), jobHeader1.AgreedDeliveryDateLocal);

			view2.AgreedDeliveryDateLocal = ZDateTime.UtcNow.AddDays(-1);

			Factory.Save();

			AssertEquals("Should have pushed new DNSB date to jobHeader1", ZDateTime.UtcNow.AddDays(2), jobHeader1.FH_DoNotStartBeforeDate);
			AssertEquals("jobHeader1 ADD has not changed", ZDateTime.UtcNow.AddDays(1), jobHeader1.FH_AgreedDeliveryDate);
		}

		#endregion

		#region MassUpdate - Set Value

		[TestDate(2015, 7, 14)]
		public void TestMassUpdate_EarliestStartDate()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");

			var viewModel = new MultiJobHeaderEditorViewModel(new[] { jobHeader }.Concat(jobHeader.ProcessHeaders), Factory);
			viewModel.EarliestStartDateLocal = new ZDateTime(2015, 6, 24);

			viewModel.MassUpdate(viewModel.JobHeaderViews.Cast<JobHeaderView>().Take(2).ToArray(), MultiJobHeaderEditorViewModel.SetProperty.EarliestStartDate);

			AssertEquals(ZDateTime.Empty, jobHeader.DoNotStartBeforeDateLocal);
			AssertEquals(ZDateTime.Empty, workflow1.DoNotStartBeforeDateLocal);
			AssertEquals(ZDateTime.Empty, workflow2.DoNotStartBeforeDateLocal);
			AssertEquals(ZDateTime.Empty, workflow3.DoNotStartBeforeDateLocal);

			Factory.Save();

			AssertEquals(new ZDateTime(2015, 6, 24), jobHeader.DoNotStartBeforeDateLocal);
			AssertEquals(new ZDateTime(2015, 6, 24), workflow1.DoNotStartBeforeDateLocal);
			AssertEquals(ZDateTime.Empty, workflow2.DoNotStartBeforeDateLocal);
			AssertEquals(ZDateTime.Empty, workflow3.DoNotStartBeforeDateLocal);
		}

		[TestDate(2015, 7, 14)]
		public void TestMassUpdate_AgreedDeliveryDate()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");

			var viewModel = new MultiJobHeaderEditorViewModel(new[] { jobHeader }.Concat(jobHeader.ProcessHeaders), Factory);
			viewModel.AgreedDeliveryDateLocal = new ZDateTime(2015, 6, 24);

			viewModel.MassUpdate(viewModel.JobHeaderViews.Cast<JobHeaderView>().Take(2).ToArray(), MultiJobHeaderEditorViewModel.SetProperty.AgreedDeliveryDate);

			AssertEquals(ZDateTime.Empty, jobHeader.AgreedDeliveryDateLocal);
			AssertEquals(ZDateTime.Empty, workflow1.AgreedDeliveryDateLocal);
			AssertEquals(ZDateTime.Empty, workflow2.AgreedDeliveryDateLocal);
			AssertEquals(ZDateTime.Empty, workflow3.AgreedDeliveryDateLocal);

			Factory.Save();

			AssertEquals(new ZDateTime(2015, 6, 24), jobHeader.AgreedDeliveryDateLocal);
			AssertEquals(new ZDateTime(2015, 6, 24), workflow1.AgreedDeliveryDateLocal);
			AssertEquals(ZDateTime.Empty, workflow2.AgreedDeliveryDateLocal);
			AssertEquals(ZDateTime.Empty, workflow3.AgreedDeliveryDateLocal);
		}

		public void TestMassUpdate_DateAcceptability()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");

			var viewModel = new MultiJobHeaderEditorViewModel(new[] { jobHeader }.Concat(jobHeader.ProcessHeaders), Factory);

			viewModel.FH_DateAcceptability = DateAcceptabilityList.Codes.ExtendedStartExtendedFinish;

			viewModel.MassUpdate(viewModel.JobHeaderViews.Cast<JobHeaderView>().Take(2).ToArray(), MultiJobHeaderEditorViewModel.SetProperty.FH_DateAcceptability);

			AssertEquals(ZString.Empty, jobHeader.FH_DateAcceptability);
			AssertEquals(ZString.Empty, workflow1.FH_DateAcceptability);
			AssertEquals(ZString.Empty, workflow2.FH_DateAcceptability);
			AssertEquals(ZString.Empty, workflow3.FH_DateAcceptability);

			Factory.Save();

			AssertEquals(DateAcceptabilityList.Codes.ExtendedStartExtendedFinish, jobHeader.FH_DateAcceptability);
			AssertEquals(DateAcceptabilityList.Codes.ExtendedStartExtendedFinish, workflow1.FH_DateAcceptability);
			AssertEquals(ZString.Empty, workflow2.FH_DateAcceptability);
			AssertEquals(ZString.Empty, workflow3.FH_DateAcceptability);
		}
		#endregion

		#region MassUpdate - Add to Values

		[TestDate(2015, 7, 14)]
		public void TestMassUpdateIncrementalValues_EarliestStartDate()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4");

			workflow1.DoNotStartBeforeDateLocal = ZDateTime.UtcNow.AddYears(-1);
			workflow2.DoNotStartBeforeDateLocal = ZDateTime.UtcNow.AddYears(1);

			var viewModel = new MultiJobHeaderEditorViewModel(new[] { jobHeader }.Concat(jobHeader.ProcessHeaders), Factory);
			var selectedItems = viewModel.JobHeaderViews.Cast<JobHeaderView>().Where(v => v.ProcessHeader.FH_CompletionStatement != "workflow4");

			viewModel.EarliestStartDateOffset = new ZInt(60).GetDateTimeFromMinutes();
			viewModel.EarliestStartDateDays = 1;

			viewModel.MassUpdateIncrementalValues(selectedItems, MultiJobHeaderEditorViewModel.SetProperty.EarliestStartDate);

			AssertEquals(ZDateTime.Empty, jobHeader.DoNotStartBeforeDateLocal);
			AssertEquals(new ZDateTime(2014, 7, 14, 0, 0, 0), workflow1.DoNotStartBeforeDateLocal);
			AssertEquals(new ZDateTime(2016, 7, 14, 0, 0, 0), workflow2.DoNotStartBeforeDateLocal);
			AssertEquals(ZDateTime.Empty, workflow3.DoNotStartBeforeDateLocal);
			AssertEquals(ZDateTime.Empty, workflow4.DoNotStartBeforeDateLocal);

			AssertEquals(ZDateTime.Empty, jobHeader.AgreedDeliveryDateLocal);
			AssertEquals(ZDateTime.Empty, workflow1.AgreedDeliveryDateLocal);
			AssertEquals(ZDateTime.Empty, workflow2.AgreedDeliveryDateLocal);
			AssertEquals(ZDateTime.Empty, workflow3.AgreedDeliveryDateLocal);
			AssertEquals(ZDateTime.Empty, workflow4.AgreedDeliveryDateLocal);

			Factory.Save();

			CombineAssertions("MassUpdateIncrementalValues should update Earliest Start Date", () =>
			{
				AssertEquals("jobHeader: should set to current time and then add increments", new ZDateTime(2015, 7, 15, 1, 0, 0), jobHeader.DoNotStartBeforeDateLocal);
				AssertEquals("workflow1 should add increments to existing values", new ZDateTime(2014, 7, 15, 1, 0, 0), workflow1.DoNotStartBeforeDateLocal);
				AssertEquals("workflow2 should add increments to existing values", new ZDateTime(2016, 7, 15, 1, 0, 0), workflow2.DoNotStartBeforeDateLocal);
				AssertEquals("workflow3 should set to current time and then add increments", new ZDateTime(2015, 7, 15, 1, 0, 0), workflow3.DoNotStartBeforeDateLocal);
				AssertEquals("workflow4: workflow was not selected so value should not be changed.", ZDateTime.Empty, workflow4.DoNotStartBeforeDateLocal);
			});

			CombineAssertions("MassUpdateIncrementalValues should leave Agreed Delivery Date alone", () =>
			{
				AssertEquals("jobHeader", ZDateTime.Empty, jobHeader.AgreedDeliveryDateLocal);
				AssertEquals("workflow1", ZDateTime.Empty, workflow1.AgreedDeliveryDateLocal);
				AssertEquals("workflow2", ZDateTime.Empty, workflow2.AgreedDeliveryDateLocal);
				AssertEquals("workflow3", ZDateTime.Empty, workflow3.AgreedDeliveryDateLocal);
				AssertEquals("workflow4", ZDateTime.Empty, workflow4.AgreedDeliveryDateLocal);
			});
		}

		[TestDate(2015, 7, 14)]
		public void TestMassUpdateIncrementalValues_AgreedDeliveryDate()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4");

			workflow1.AgreedDeliveryDateLocal = ZDateTime.UtcNow.AddYears(-1);
			workflow2.AgreedDeliveryDateLocal = ZDateTime.UtcNow.AddYears(1);

			var viewModel = new MultiJobHeaderEditorViewModel(new[] { jobHeader }.Concat(jobHeader.ProcessHeaders), Factory);
			var selectedItems = viewModel.JobHeaderViews.Cast<JobHeaderView>().Where(v => v.ProcessHeader.FH_CompletionStatement != "workflow4");

			viewModel.AgreedDeliveryDateOffset = new ZInt(60).GetDateTimeFromMinutes();
			viewModel.AgreedDeliveryDateDays = 1;

			viewModel.MassUpdateIncrementalValues(selectedItems, MultiJobHeaderEditorViewModel.SetProperty.AgreedDeliveryDate);

			AssertEquals(ZDateTime.Empty, jobHeader.AgreedDeliveryDateLocal);
			AssertEquals(new ZDateTime(2014, 7, 14, 0, 0, 0), workflow1.AgreedDeliveryDateLocal);
			AssertEquals(new ZDateTime(2016, 7, 14, 0, 0, 0), workflow2.AgreedDeliveryDateLocal);
			AssertEquals(ZDateTime.Empty, workflow3.AgreedDeliveryDateLocal);
			AssertEquals(ZDateTime.Empty, workflow4.AgreedDeliveryDateLocal);

			AssertEquals(ZDateTime.Empty, jobHeader.DoNotStartBeforeDateLocal);
			AssertEquals(ZDateTime.Empty, workflow1.DoNotStartBeforeDateLocal);
			AssertEquals(ZDateTime.Empty, workflow2.DoNotStartBeforeDateLocal);
			AssertEquals(ZDateTime.Empty, workflow3.DoNotStartBeforeDateLocal);
			AssertEquals(ZDateTime.Empty, workflow4.DoNotStartBeforeDateLocal);

			Factory.Save();

			CombineAssertions("MassUpdateIncrementalValues should update Agreed Delivery Date", () =>
			{
				AssertEquals("jobHeader: should set to current time and then add increments", new ZDateTime(2015, 7, 15, 1, 0, 0), jobHeader.AgreedDeliveryDateLocal);
				AssertEquals("workflow1 should add increments to existing values", new ZDateTime(2014, 7, 15, 1, 0, 0), workflow1.AgreedDeliveryDateLocal);
				AssertEquals("workflow2 should add increments to existing values", new ZDateTime(2016, 7, 15, 1, 0, 0), workflow2.AgreedDeliveryDateLocal);
				AssertEquals("workflow3 should set to current time and then add increments", new ZDateTime(2015, 7, 15, 1, 0, 0), workflow3.AgreedDeliveryDateLocal);
				AssertEquals("workflow4: workflow was not selected so value should not be changed.", ZDateTime.Empty, workflow4.AgreedDeliveryDateLocal);
			});

			CombineAssertions("MassUpdateIncrementalValues should leave Earliest Start Date alone", () =>
			{
				AssertEquals("jobHeader", ZDateTime.Empty, jobHeader.DoNotStartBeforeDateLocal);
				AssertEquals("workflow1", ZDateTime.Empty, workflow1.DoNotStartBeforeDateLocal);
				AssertEquals("workflow2", ZDateTime.Empty, workflow2.DoNotStartBeforeDateLocal);
				AssertEquals("workflow3", ZDateTime.Empty, workflow3.DoNotStartBeforeDateLocal);
				AssertEquals("workflow4", ZDateTime.Empty, workflow4.DoNotStartBeforeDateLocal);
			});
		}

		[TestDate(2015, 7, 14)]
		public void TestMassUpdateIncrementalValues_ShouldHandleNegativeOffsets()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var viewModel = new MultiJobHeaderEditorViewModel(new[] { jobHeader }, Factory);
			var selectedItems = viewModel.JobHeaderViews.Cast<JobHeaderView>();

			viewModel.AgreedDeliveryDateOffset = new ZInt(-60).GetDateTimeFromMinutes();
			viewModel.EarliestStartDateOffset = new ZInt(-120).GetDateTimeFromMinutes();

			viewModel.MassUpdateIncrementalValues(selectedItems, MultiJobHeaderEditorViewModel.SetProperty.AgreedDeliveryDate);
			viewModel.MassUpdateIncrementalValues(selectedItems, MultiJobHeaderEditorViewModel.SetProperty.EarliestStartDate);

			AssertEquals(ZDateTime.Empty, jobHeader.AgreedDeliveryDateLocal);
			AssertEquals(ZDateTime.Empty, jobHeader.DoNotStartBeforeDateLocal);

			Factory.Save();

			AssertEquals("Should set to current time and then add negative increments", new ZDateTime(2015, 7, 13, 23, 0, 0), jobHeader.AgreedDeliveryDateLocal);
			AssertEquals("Should set to current time and then add negative increments", new ZDateTime(2015, 7, 13, 22, 0, 0), jobHeader.DoNotStartBeforeDateLocal);
		}

		#endregion

		#region Db Hits

		public void TestJobHeaderViewsConstructor_DbHits()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var jobHeader4 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var dummy5 = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var dummy6 = BMSTestHelper.CreateDummyWorkflowProvider(Factory);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var providers = newFactory.Load<DummyWithWorkflow>(new ZQuery(DummyBizoSchema.PK, new[] { jobHeader1.FH_ParentId, jobHeader2.FH_ParentId, jobHeader3.FH_ParentId, jobHeader4.FH_ParentId, dummy5.PK, dummy6.PK }));
			var dbHits = newFactory.GetTableHitCount(ProcessHeaderSchema.Constants.TableName);

			var viewModel = new MultiJobHeaderEditorViewModel(providers, newFactory);
			AssertEquals(6, viewModel.JobHeaderViews.Count);

			AssertEquals(dbHits + 1, newFactory.GetTableHitCount(ProcessHeaderSchema.Constants.TableName));

			newFactory.Save();
			AssertEquals(dbHits + 1, newFactory.GetTableHitCount(ProcessHeaderSchema.Constants.TableName));
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			return new MultiJobHeaderEditorViewModel(new[] { dummy }, Factory);
		}

		#endregion
	}
}
