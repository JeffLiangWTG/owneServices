using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.UniversalCopy;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Module;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.UniversalCopy.Module.Testing
{
	[TestedType(typeof(UniversalCopyScheduleFilterBusinessObject))]
	sealed class UniversalCopyScheduleFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestParentJobFilter()
		{
			var copy1 = NewItemWithTemplateAndSchedule();
			copy1.ScheduleTask.S5_ScheduleDescription = "copy 1";
			copy1.SUC_CopyObjectId = new Guid("A5A30F9E-61B7-4A22-A536-76ABEB82D280");
			var copy2 = NewItemWithTemplateAndSchedule();
			copy2.ScheduleTask.S5_ScheduleDescription = "copy 2";
			copy2.SUC_CopyObjectId = new Guid("ACD34C9A-5100-44B1-AE60-DAF208A38BBF");

			Factory.Save();

			var filterBizo = new UniversalCopyScheduleFilterBusinessObject();
			var filter = (ParentJobModuleFilter)filterBizo["Parent Job"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.IsActive = true;
			filter.Property = copy1.SUC_CopyObjectId;

			var collection = new StmUniversalCopyCollection(Factory);
			collection.Load(filterBizo.Filter);

			AssertEquals(1, collection.Count);
			AssertEquals(copy1.ScheduleTask.S5_ScheduleDescription, collection[0].ScheduleTask.S5_ScheduleDescription);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.Property = copy1.SUC_CopyObjectId;
			collection.Load(filterBizo.Filter);

			AssertEquals(1, collection.Count);
			AssertEquals(copy2.ScheduleTask.S5_ScheduleDescription, collection[0].ScheduleTask.S5_ScheduleDescription);
		}

		public void TestActiveStatusFilter()
		{
			var copy1 = NewItemWithTemplateAndSchedule();
			var copy2 = NewItemWithTemplateAndSchedule();
			copy2.ScheduleTask.S5_IsActive = false;
			Factory.Save();
			AssertOneResult(copy1);
			var filter = (ModuleTextFilter)FilterStrip["Active Status"];
			AllLanguages.ForEach(lan =>
			{
				using (Res.TemporarilySwitchLanguage(lan))
				{
					filter.Property = FilterStripBusinessObject.StatusInactive;
					AssertOneResult(copy2);
					filter.Property = FilterStripBusinessObject.StatusAll;
					var collection = new StmUniversalCopyCollection(Factory);
					collection.Load(FilterStrip.Filter);
					AssertEquals(2, collection.Count);
				}
			});
		}

		public void TestActiveStatusFilter_DefaultStatusIsActive()
		{
			var filter = (ModuleTextFilter)FilterStrip["Active Status"];

			AssertEquals("Active", filter.Property);

			filter.Clear();

			AssertEquals("Active", filter.Property);
		}

		public void TestTaskDescriptionFilter()
		{
			var copy1 = NewItemWithTemplateAndSchedule();
			copy1.ScheduleTask.S5_ScheduleDescription = "One";

			var copy2 = NewItemWithTemplateAndSchedule();
			copy2.ScheduleTask.S5_ScheduleDescription = "Two";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStrip["TaskDescription"];
			filter.IsActive = true;
			filter.Property = "One";
			AssertOneResult(copy1);
			filter.Property = "Two";
			AssertOneResult(copy2);
		}

		public void TestIndexSearchFiltersOfUniversalCopySchedule()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.UniversalCopySchedule))
			using (var mocker = new GlowIndexQueryEngineMock(
				mock =>
				{
					_ = mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns(GetSearchFieldCollection());
					_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IStmUniversalCopy" });
				}))
			{
				var activeStatusFilter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["Active Status"];
				AssertNotNull(activeStatusFilter);
				AssertEquals(FilterVisibility.AlwaysApplied, activeStatusFilter.Visibility);
				AssertEquals("Active", activeStatusFilter.DefaultProperty);
				AssertEquals(FilterCategories.StatusAndFlags, activeStatusFilter.Category);
				AssertEquals("Active Status", activeStatusFilter.MultilingualDescription.ToString());

				var taskDescriptionFilter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["TaskDescription"];
				AssertNotNull(taskDescriptionFilter);
				AssertEquals(StmScheduleTaskSchema.S5_ScheduleDescription.MaxLength, taskDescriptionFilter.MaxLength);
				AssertEquals(FilterCategories.TextSearch, taskDescriptionFilter.Category);
				AssertEquals("Task Description", taskDescriptionFilter.MultilingualDescription.ToString());

				var templateNameFilter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["TemplateName"];
				AssertNotNull(templateNameFilter);
				AssertEquals(StmModuleFilterSchema.S9_FilterName.MaxLength, templateNameFilter.MaxLength);
				AssertEquals(FilterCategories.TextSearch, templateNameFilter.Category);
				AssertEquals("Template Name", templateNameFilter.MultilingualDescription.ToString());

				var parentJobFilter = (IndexSearchParentJobModuleFilter)module.FilterBusinessObject["ParentJob"];
				AssertNotNull(parentJobFilter);
				AssertEquals(FilterCategories.Other, parentJobFilter.Category);
				AssertEquals("Parent Job", parentJobFilter.MultilingualDescription.ToString());
			}
		}

		SearchFieldCollection GetSearchFieldCollection()
		{
			var field1 = SearchField.Create("ScheduleTaskIsActive", "Active Status");
			var field2 = SearchField.Create("TaskDescription", "Task Description");
			var field3 = SearchField.Create("TemplateName", "Template Name");
			var field4 = SearchField.Create("ParentJob", "Parent Job");
			var ret = new SearchFieldCollection("IStmUniversalCopy", new SearchField[] { field1, field2, field3, field4 });
			return ret;
		}

		public void TestTemplateNameFilter()
		{
			var copyTemplate1 = Factory.New<UniversalCopyTemplate>();
			copyTemplate1.S9_ModuleID = ModuleIDs.JobShipment.Name + StmModuleFilter.ModuleIdSuffix.UniversalCopyTemplate;
			copyTemplate1.CopyTemplateTree = CreateCopyTemplateTree();
			copyTemplate1.S9_FilterName = "Template One";
			var copy1 = Factory.New<StmUniversalCopy>();
			copy1.SUC_CopyObjectTableCode = JobShipmentSchema.Constants.Prefix;
			copy1.SUC_S9_CopyTemplate = copyTemplate1.PK;

			var copyTemplate2 = Factory.New<UniversalCopyTemplate>();
			copyTemplate2.S9_ModuleID = ModuleIDs.JobShipment.Name + StmModuleFilter.ModuleIdSuffix.UniversalCopyTemplate;
			copyTemplate2.CopyTemplateTree = CreateCopyTemplateTree();
			copyTemplate2.S9_FilterName = "Template Two";
			var copy2 = Factory.New<StmUniversalCopy>();
			copy2.SUC_CopyObjectTableCode = JobShipmentSchema.Constants.Prefix;
			copy2.SUC_S9_CopyTemplate = copyTemplate2.PK;

			Factory.Save();

			((ModuleTextFilter)FilterStrip["Active Status"]).Property = "All";

			var filter = (ModuleTextFilter)FilterStrip["TemplateName"];
			filter.IsActive = true;
			filter.Property = "Template One";
			AssertOneResult(copy1);
			filter.Property = "Template Two";
			AssertOneResult(copy2);
		}

		StmUniversalCopy NewItemWithTemplateAndSchedule()
		{
			copyTemplate = copyTemplate ?? Factory.New<UniversalCopyTemplate>();
			copyTemplate.S9_ModuleID = ModuleIDs.JobShipment.Name + StmModuleFilter.ModuleIdSuffix.UniversalCopyTemplate;
			var copy = Factory.New<StmUniversalCopy>();
			copy.SUC_CopyObjectTableCode = JobShipmentSchema.Constants.Prefix;
			copy.SUC_S9_CopyTemplate = copyTemplate.PK;
			var sched = Factory.New<StmUniversalCopyScheduleTask>();
			sched.S5_ParentID = copy.PK;
			sched.S5_ParentTableCode = StmUniversalCopySchema.Constants.Prefix;
			return copy;
		}

		CopyTemplateTreeBizo CreateCopyTemplateTree()
		{
			var glowInterface = GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(DummyBusinessObject), true);
			return new CopyTemplateTreeBizo(new CopyTemplateTree(glowInterface, typeof(DummyBusinessObject), BusinessObjectCopyManager.CopyTreeConfiguration), copyTemplate);
		}

		UniversalCopyTemplate copyTemplate;

		void AssertOneResult(StmUniversalCopy expectedResult)
		{
			var collection = new StmUniversalCopyCollection(Factory);
			collection.Load(FilterStrip.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals(expectedResult.PK, collection[0].PK);
		}

		UniversalCopyScheduleFilterBusinessObject FilterStrip
		{
			get
			{
				if (filterStrip == null)
				{
					filterStrip = new UniversalCopyScheduleFilterBusinessObject();
				}

				return filterStrip;
			}
		}
		UniversalCopyScheduleFilterBusinessObject filterStrip;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new UniversalCopyScheduleFilterBusinessObject();
		}
	}
}
