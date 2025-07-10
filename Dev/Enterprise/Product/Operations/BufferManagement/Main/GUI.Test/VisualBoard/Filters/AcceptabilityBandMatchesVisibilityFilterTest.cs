using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	#region BoardFilterTestCase

	[TestedType(typeof(AcceptabilityBandMatchesVisibilityFilter))]
	class AcceptabilityBandMatchesVisibilityFilterTest : BoardFilterTestCase<AcceptabilityBandMatchesVisibilityFilter>
	{
		public override void TestFilterName()
		{
			AssertEquals("Showing items matching Acceptability Band Platinum Golden Rule.", GetFilter().FilterName);
		}

		public override void TestAllowMultiple()
		{
			AssertEquals(false, GetFilter().AllowMultiple);
		}

		public override void TestEquals()
		{
			var filter = GetFilter();
			var otherFilter = new AcceptabilityBandMatchesVisibilityFilter(config.RedRule, ZGuid.NewZGuid(), new AcceptabilityBandSqlBuilderParameters(config.RedRule));
			var samefilter = new AcceptabilityBandMatchesVisibilityFilter(config.PlatinumRule, ZGuid.NewZGuid(), new AcceptabilityBandSqlBuilderParameters(config.PlatinumRule));

			AssertEquals(samefilter, filter);
			AssertEquals(filter, filter);
			AssertNotEquals(otherFilter, filter);
		}

		public override void TestWhenRemovedThroughFilterManager_ShouldRestoreVisualState()
		{
			var section = CreateBoardSection(config.Buffer);
			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(section, config.PlatinumRule);

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();

				var tile = form.FindAll<AcceptabilityBandTileControl>().Single();
				var menuItem = BMSGUITestCase.GetHighlightLabelMenuItem(tile);
				AssertEquals(AcceptabilityBandTileControl.ShowMatchingItemsLabel, menuItem.Text);

				Assert(!menuItem.Checked);
				BMSGUITestCase.ToggleShowMatchingItems(tile);
				Assert(menuItem.Checked);

				var filterManager = form.FindAll<BMComponentControl>().Single().ViewModel.FilterManager;
				filterManager.Clear();
				AssertEquals(AcceptabilityBandTileControl.ShowMatchingItemsLabel, menuItem.Text);
			}
		}

		protected override AcceptabilityBandMatchesVisibilityFilter GetFilter()
		{
			return new AcceptabilityBandMatchesVisibilityFilter(config.PlatinumRule, ZGuid.NewZGuid(), new AcceptabilityBandSqlBuilderParameters(config.PlatinumRule));
		}

		AcceptabilityBandTestConfig config;

		protected override void SetUp()
		{
			base.SetUp();
			disableAsyncBehaviour = VisualBoardsTestCase.DisableAsyncBehaviour();
			config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
		}

		protected override void TearDown()
		{
			disableAsyncBehaviour?.Dispose();
			base.TearDown();
		}

		IDisposable disableAsyncBehaviour;
	}

	#endregion

	#region FilterApplicatorTestCase

	[TestedType(typeof(AcceptabilityBandMatchesVisibilityFilter))]
	class AcceptabilityBandMatchesVisibilityFilterApplicatorTest : TaskVisibilityFilterApplicatorTestCase<AcceptabilityBandMatchesVisibilityFilter>
	{
		public override void TestApply_DbHits()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(false);
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow1").AddTag(config.PlatinumTag);
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow2").AddTag(config.PlatinumTag);
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow3");
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow4");

			Factory.Save();

			var system = config.System;
			var component = CreateBucket(system);
			var section = CreateBoardSection(component);
			var viewModel = BMSTestHelper.CreateViewModel(section);

			var newFactory = Factory.CreateNewFactory();
			var loadedJobHeader = newFactory.Load<ProcessJobHeader>(jobHeader.PK);
			var loadedBand = newFactory.Load<BMComponentAcceptabilityBand>(config.PlatinumRule.PK);
			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(section, loadedBand);
			using (DisableAsyncBehaviour())
			{
				var filter = new AcceptabilityBandMatchesVisibilityFilter(loadedBand, sectionBand.PK, new AcceptabilityBandSqlBuilderParameters(loadedBand));

				foreach (var workflow in loadedJobHeader.ProcessHeaders.Cast<ProcessHeader>())
				{
					filter.IsApplicable(new WorkflowCardContent(workflow, workflow.GetTasksWithoutAccessingWorkflowParent().FirstOrDefault(), viewModel), null, null);
				}

				var moreDbHitsAllowed = new Dictionary<string, int>
				{
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ ProcessHeaderSchema.Constants.TableName, 2 },
					{ ProcessTasksSchema.Constants.TableName, 1 },
					{ BMComponentAcceptabilityBandSchema.Constants.TableName, 1 },
					{ StmModuleFilterSchema.Constants.TableName, 1 },
					{ StmModuleFilterUserDataSchema.Constants.TableName, 1 }
				};

				AssertDbHits(moreDbHitsAllowed, newFactory);
			}
		}

		public override void TestIsApplicable()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow2");

			workflow1.FH_FC_CurrentComponent = workflow2.FH_FC_CurrentComponent = config.Buffer.PK;

			workflow1.AddTag(config.PlatinumTag);
			workflow2.AddTag(config.RedTag);

			Factory.Save();

			var system = config.System;
			var component = CreateBucket(system);
			var section = CreateBoardSection(component);
			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (DisableAsyncBehaviour())
			{
				var filter = GetFilter();

				AssertEquals(true, filter.IsApplicable(new WorkflowCardContent(workflow1, workflow1.GetTasksWithoutAccessingWorkflowParent().FirstOrDefault(), viewModel), null, null));
				AssertEquals(false, filter.IsApplicable(new WorkflowCardContent(workflow2, workflow2.GetTasksWithoutAccessingWorkflowParent().FirstOrDefault(), viewModel), null, null));
			}
		}

		public void TestNullCardDoesNotThrowException()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(false);
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow1").AddTag(config.PlatinumTag);
			Factory.Save();

			var component = CreateBucket(config.System);
			var viewModel = BMSTestHelper.CreateViewModel(CreateBoardSection(component));

			var newFactory = Factory.CreateNewFactory();
			var loadedBand = newFactory.Load<BMComponentAcceptabilityBand>(config.PlatinumRule.PK);

			var testCell = new CellContent(12, 12, CellContentType.Cards);
			testCell.Channel = new UnchanneledChannel("test");
			testCell.Label = "test";

			using (DisableAsyncBehaviour())
			{
				var filter = new AcceptabilityBandMatchesVisibilityFilter(loadedBand, ZGuid.NewZGuid(), new AcceptabilityBandSqlBuilderParameters(loadedBand));

				AssertNoExceptionThrown("A NullReferenceException should not be thrown, because if CardContent is null, an error report should be created and IsApplicable should return true instead.",
					() => filter.IsApplicable((ICardContent)null, testCell, null));
				AssertEquals("cardContent is null. cell channel full name: Un-channeled, cell coordinates X 12 and Y 12, cell.Label: test", ErrorReporter.LastMessageReported);
				AssertEquals(1, ErrorReporter.TotalErrorCount);

				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		protected override AcceptabilityBandMatchesVisibilityFilter GetFilter()
		{
			return new AcceptabilityBandMatchesVisibilityFilter(config.PlatinumRule, ZGuid.NewZGuid(), new AcceptabilityBandSqlBuilderParameters(config.PlatinumRule));
		}

		protected override IEnumerable<KeyValuePair<string, int>> GetExpectedDbHits()
		{
			yield break;
		}

		AcceptabilityBandTestConfig config;

		protected override void SetUp()
		{
			base.SetUp();
			config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
		}
	}

	#endregion
}
