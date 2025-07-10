using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(BMComponentLink))]
	public class BMComponentLinkTest : EnterpriseBusinessObjectTestCase
	{
		#region Filter

		public void TestAddNewStripsToLoadedFilter()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var filter = config.ComponentLink.FilterRule;

			FilterStripsTestHelper.AddFilterStrips(filter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.AutoAssignTasks,
				FilterStripValueSetter = f => ((ModuleFlagsFilter)f).Property0 = true,
			});

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var system = newFactory.Load<BMSystem>(config.System.PK);
			var loadedFilter = system.Components.Single(c => c.FC_Name == "bucket").FromMeToOthersLinks.Single().FilterRule;

			FilterStripsTestHelper.AddFilterStrips(loadedFilter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CriticalHandover,
				FilterStripValueSetter = f => ((ModuleFlagsFilter)f).Property0 = true,
			});

			AssertNoExceptionThrown(newFactory.Save);
		}

		#endregion

		#region Properties

		#region SkipTransfer

		public void TestSkipShouldBeFalse_WhenFilterBusinessObjectIsNotIBMFilterRuleFilterBusinessObject()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

			FilterStripsTestHelper.AddFilterStrips(link.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = FilterDescriptions.ActiveStatus
			});

			var componentLink = link as IComponentLink;

			AssertEquals(true, componentLink.SkipTransfer);

			const string filterReference = "BMFilterRule_FilterStripBusinessObject";

			ObjectFactory.Substitute(filterReference, new Mock<IRelatedModuleFilterBusinessObject>().Object);

			AssertEquals(expected: false, componentLink.SkipTransfer);
		}

		public void TestSkipShouldBeTrue_WhenCouldNotLoadFilterRuleLayout()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

			FilterStripsTestHelper.AddFilterStrips(link.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = FilterDescriptions.ActiveStatus
			});

			var componentLink = link as IComponentLink;

			AssertEquals(true, componentLink.SkipTransfer);

			link.FilterRule.Delete();

			AssertEquals(expected: true, componentLink.SkipTransfer);
		}

		public void TestSkipShouldBeTrue_WhenNoFilters()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

			var componentLink = link as IComponentLink;

			AssertEquals(true, componentLink.SkipTransfer);

			FilterStripsTestHelper.AddFilterStrips(link.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = FilterDescriptions.ActiveStatus
			});

			AssertEquals(expected: true, componentLink.SkipTransfer);
		}

		public void TestSkipShouldBetTrue_OnlyWhenContainsAllFiltersThatCanBeSkiped()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

			AddAllFiltersThatCanBeSkiped(link.FilterRule);

			var componentLink = link as IComponentLink;
			AssertEquals(true, componentLink.SkipTransfer);

			//QueueStatus don't trigger responsive transfer
			FilterStripsTestHelper.AddFilterStrips(link.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.QueueStatus
			});

			AssertEquals(false, componentLink.SkipTransfer);
		}

		public static void AddAllFiltersThatCanBeSkiped(StmModuleFilter filter, params string[] moreFilters)
		{
			var filtersAllowedToSkip = new HashSet<string>()
			{
				FilterDescriptions.ActiveStatus,
				ProcessHeader.ModuleFilterConstants.AutoAssignTasks,
				ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterDescriptions.CreatedOnWeb,
				ProcessHeader.ModuleFilterConstants.CriticalHandover,
				ProcessHeader.ModuleFilterConstants.EarliestStartDate,
				ProcessHeader.ModuleFilterConstants.JobOrWorkflow,
				ProcessHeader.ModuleFilterConstants.LastTransferType,
				ProcessHeader.ModuleFilterConstants.PrerequisiteStatus,
				ProcessHeader.ModuleFilterConstants.ReleaseGroup,
				ProcessHeader.ModuleFilterConstants.StandbyTask,
				ProcessHeader.ModuleFilterConstants.TagDefinitionCode,
				ProcessHeader.ModuleFilterConstants.TagMagnitude,
				ProcessHeader.ModuleFilterConstants.TaskAssigned,
				ProcessHeader.ModuleFilterConstants.TaskOpenEstimateRange,
				ProcessHeader.ModuleFilterConstants.Template,
				ProcessHeader.ModuleFilterConstants.WorkflowCategory,
				ProcessHeader.ModuleFilterConstants.WorkflowStatus,
				ProcessHeader.ModuleFilterConstants.WorkflowType,
			};

			var filters = new List<FilterStripsTestHelper.FilterStripDefinition>(filtersAllowedToSkip.Count);

			foreach (var filterCode in filtersAllowedToSkip.Union(moreFilters))
			{
				filters.Add(new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = filterCode,
				});
			}

			FilterStripsTestHelper.AddFilterStrips(filter, filters.ToArray());
		}

		#endregion

		#endregion
	}
	
	[TestedType(typeof(BMComponentLink))]
	public class BMComponentLinkRelatedFilterTest : RelatedModuleFilterSupportableTestCase<BMComponentLink>
	{
		protected override IEnumerable<FilterRuleTestSet> GetFilterRules(BMComponentLink businessObject)
		{
			return new[] { new FilterRuleTestSet(null, () => businessObject.FilterRule, "BMFilterRuleFilterBusinessObject") };
		}

		protected override void ValidateBusinessObject(BMComponentLink businessObject)
		{
			businessObject.Validation.ValidateAll();
		}
	}
}
