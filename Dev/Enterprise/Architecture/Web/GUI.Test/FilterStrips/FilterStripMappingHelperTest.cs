using System;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips.Testing
{
	sealed class FilterStripMappingHelperTest : TestCaseWithFactory
	{
		#region TestFilterOfUnknownType

		public void TestFilterOfUnknownType()
		{
			DummyFilter sourceFilter = new DummyFilter("source");
			DummyFilter destinationFilter = new DummyFilter("destination");

			SourceFilter.AddModuleFilterForTest(sourceFilter);
			DestinationFilter.AddModuleFilterForTest(destinationFilter);

			sourceFilter.IsActive = true;

			AssertEquals(sourceFilter, SourceFilter["source"]);
			AssertEquals(destinationFilter, DestinationFilter["destination"]);

			AssertEquals(false, destinationFilter.IsActive);

			Helper.MapModuleFilters("source", "destination");

			AssertEquals(true, destinationFilter.IsActive);
		}

		class DummyFilter : ModuleFilter
		{
			public DummyFilter(ZString description) : base(description) { }

			protected override void ClearCore()
			{
				throw new NotImplementedException();
			}

			protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			{
				throw new NotImplementedException();
			}

			protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
			{
				// do nothing
			}

			protected override ModuleFilterValidation GetNewValidation()
			{
				throw new NotImplementedException();
			}

			protected override ZQuery GetQueryUsingFilterColumns()
			{
				throw new NotImplementedException();
			}

			protected override void SerializePropertiesToXml(XmlWriter writer)
			{
				throw new NotImplementedException();
			}

			protected override void DeserializePropertiesFromXml(XmlReader reader)
			{
				throw new NotImplementedException();
			}

			protected override bool IsEmptyCore
			{
				get { throw new NotImplementedException(); }
			}

			public override bool IsExpensiveQuery
			{
				get { throw new NotImplementedException(); }
			}

			protected override FilterCategory DefaultCategory
			{
				get { return new FilterCategory((NoResString)"dummy"); }
			}

			protected override object[] QueryDelegateParameters
			{
				get { throw new NotImplementedException(); }
			}

			protected override void FillWithValidTestFilterValueCore()
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		#region Module filters

		#region TestMapModuleTextFilters

		public void TestMapModuleTextFilters()
		{
			ModuleTextFilter sourceTextFilter = new ModuleTextFilter("Text Source", DummyGetTextQueryWithOperator);
			SourceFilter.AddModuleFilterForTest(sourceTextFilter);
			AssertNotNull("Source Filter", SourceFilter["Text Source"]);
			AssertEquals("Should be added source filter", sourceTextFilter, SourceFilter["Text Source"]);
			sourceTextFilter.Property = "78513213";
			sourceTextFilter.ComparisonOperator = ModuleTextBaseFilter.ComparisonConstants.NotStartsWith;
			sourceTextFilter.IsActive = true;

			ModuleTextFilter destinationTextFilter = new ModuleTextFilter("Text Destination", DummyGetTextQueryWithOperator);
			DestinationFilter.AddModuleFilterForTest(destinationTextFilter);

			AssertNotNull("Destination Filter", DestinationFilter["Text Destination"]);
			AssertEquals("Should be added destination filter", destinationTextFilter, DestinationFilter["Text Destination"]);
			AssertEquals(false, destinationTextFilter.IsActive);

			Helper.MapModuleFilters("Text Source", "Text Destination");

			AssertEquals(true, destinationTextFilter.IsActive);
			AssertEquals("78513213", destinationTextFilter.Property);
			AssertEquals(ModuleTextBaseFilter.ComparisonConstants.NotStartsWith, destinationTextFilter.ComparisonOperator);
		}

		#endregion

		#region TestMapModuleNumberFilters

		public void TestMapModuleNumberFilters()
		{
			ModuleNumberFilter sourceNumberFilter = new ModuleNumberFilter("Number Source", DummyGetTextQueryWithOperator);
			SourceFilter.AddModuleFilterForTest(sourceNumberFilter);
			AssertNotNull("Source Filter", SourceFilter["Number Source"]);
			AssertEquals("Should be added source filter", sourceNumberFilter, SourceFilter["Number Source"]);
			sourceNumberFilter.Property = "666";
			sourceNumberFilter.ComparisonOperator = ModuleTextBaseFilter.ComparisonConstants.NotStartsWith;
			sourceNumberFilter.IsActive = true;

			ModuleNumberFilter destinationNumberFilter = (new ModuleNumberFilter("Number Destination", DummyGetTextQueryWithOperator));
			DestinationFilter.AddModuleFilterForTest(destinationNumberFilter);

			AssertNotNull("Destination Filter", DestinationFilter["Number Destination"]);
			AssertEquals("Should be added destination filter", destinationNumberFilter, DestinationFilter["Number Destination"]);
			AssertEquals(false, destinationNumberFilter.IsActive);

			Helper.MapModuleFilters("Number Source", "Number Destination");

			AssertEquals(true, destinationNumberFilter.IsActive);
			AssertEquals("666", destinationNumberFilter.Property);
			AssertEquals(ModuleTextBaseFilter.ComparisonConstants.NotStartsWith, destinationNumberFilter.ComparisonOperator);
		}

		#endregion

		#region TestMapModuleFountainFilter

		public void TestMapModuleFountainFilter()
		{
			ModuleFountainFilter sourceFountainFilter = new ModuleFountainFilter("Fountain Source", DummyGetTextQueryWithOperator, "T");
			SourceFilter.AddModuleFilterForTest(sourceFountainFilter);
			AssertNotNull("Source Filter", SourceFilter["Fountain Source"]);
			AssertEquals("Should be added source filter", sourceFountainFilter, SourceFilter["Fountain Source"]);
			sourceFountainFilter.Property = "666";
			sourceFountainFilter.ComparisonOperator = ModuleTextBaseFilter.ComparisonConstants.NotStartsWith;
			sourceFountainFilter.IsActive = true;

			ModuleFountainFilter destinationFountainFilter = new ModuleFountainFilter("Fountain Destination", DummyGetTextQueryWithOperator, "T");
			DestinationFilter.AddModuleFilterForTest(destinationFountainFilter);

			AssertNotNull("Destination Filter", DestinationFilter["Fountain Destination"]);
			AssertEquals("Should be added destination filter", destinationFountainFilter, DestinationFilter["Fountain Destination"]);
			AssertEquals(false, destinationFountainFilter.IsActive);

			Helper.MapModuleFilters("Fountain Source", "Fountain Destination");

			AssertEquals(true, destinationFountainFilter.IsActive);
			AssertEquals(sourceFountainFilter.Property, destinationFountainFilter.Property);
			AssertEquals(ModuleTextBaseFilter.ComparisonConstants.NotStartsWith, destinationFountainFilter.ComparisonOperator);
		}

		#endregion

		#region MapModuleTextAndNkFilters

		public void MapModuleTextAndNkFilters()
		{
			ModuleTextAndNkFilter sourceTextAndNkFilter = new ModuleTextAndNkFilter("TextAndNk Source", DummyGetTextAndNkQuery, DummyModuleIDs.Dummy, DummyCollection);
			SourceFilter.AddModuleFilterForTest(sourceTextAndNkFilter);
			AssertNotNull("Source Filter", SourceFilter["TextAndNk Source"]);
			AssertEquals("Should be added source filter", sourceTextAndNkFilter, SourceFilter["TextAndNk Source"]);
			sourceTextAndNkFilter.Property = "78513213";
			sourceTextAndNkFilter.ComparisonOperator = ModuleTextAndNkFilter.ComparisonConstants.NotStartsWith;
			sourceTextAndNkFilter.IsActive = true;

			ModuleTextAndNkFilter destinationTextAndNkFilter = new ModuleTextAndNkFilter("TextAndNk Destination", DummyGetTextAndNkQuery, DummyModuleIDs.Dummy, DummyCollection);
			DestinationFilter.AddModuleFilterForTest(destinationTextAndNkFilter);

			AssertNotNull("Destination Filter", DestinationFilter["TextAndNk Destination"]);
			AssertEquals("Should be added destination filter", destinationTextAndNkFilter, DestinationFilter["TextAndNk Destination"]);
			AssertEquals(false, destinationTextAndNkFilter.IsActive);

			Helper.MapModuleFilters("TextAndNk Source", "TextAndNk Destination");

			AssertEquals(true, destinationTextAndNkFilter.IsActive);
			AssertEquals("78513213", destinationTextAndNkFilter.Property);
			AssertEquals(ModuleTextAndNkFilter.ComparisonConstants.NotStartsWith, destinationTextAndNkFilter.ComparisonOperator);
		}

		#endregion

		#region TestMapModuleDateFilters

		public void TestMapModuleDateFilters()
		{
			ModuleDateFilter sourceDateFilter = new ModuleDateFilter("Date Source", CargoWise.Schema.Schema.GenericDateTimeColumn);
			SourceFilter.AddModuleFilterForTest(sourceDateFilter);
			AssertNotNull("Source Filter", SourceFilter["Date Source"]);
			AssertEquals("Should be added source filter", sourceDateFilter, SourceFilter["Date Source"]);
			ZDateTime dateFrom = new ZDateTime(2008, 3, 30);
			ZDateTime dateTo = new ZDateTime(2008, 3, 31);
			sourceDateFilter.Property1 = dateFrom;
			sourceDateFilter.Property2 = dateTo;
			sourceDateFilter.PropertySearch = "foo";
			sourceDateFilter.IsActive = true;

			ModuleDateFilter destinationDateFilter = new ModuleDateFilter("Date Destination", CargoWise.Schema.Schema.GenericDateTimeColumn);
			DestinationFilter.AddModuleFilterForTest(destinationDateFilter);

			AssertNotNull("Destination Filter", DestinationFilter["Date Destination"]);
			AssertEquals("Should be added destination filter", destinationDateFilter, DestinationFilter["Date Destination"]);
			AssertEquals(false, destinationDateFilter.IsActive);

			Helper.MapModuleFilters("Date Source", "Date Destination");

			AssertEquals(true, destinationDateFilter.IsActive);
			AssertEquals(dateFrom, destinationDateFilter.Property1);
			AssertEquals(dateTo, destinationDateFilter.Property2);
			AssertEquals(sourceDateFilter.PropertySearch, destinationDateFilter.PropertySearch);
		}

		#endregion

		#region MapModuleNkFilters

		public void MapModuleNkFilters()
		{
			ModuleNkFilter sourceNkFilter = new ModuleNkFilter("Nk Source", DummyGetNkQuery, DummyModuleIDs.Dummy, DummyCollection);
			SourceFilter.AddModuleFilterForTest(sourceNkFilter);
			AssertNotNull("Source Filter", SourceFilter["Nk Source"]);
			AssertEquals("Should be added source filter", sourceNkFilter, SourceFilter["Nk Source"]);
			sourceNkFilter.Property = "ABC";
			sourceNkFilter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.NotStartsWith;
			sourceNkFilter.IsActive = true;

			ModuleNkFilter destinationNkFilter = new ModuleNkFilter("Nk Destination", DummyGetNkQuery, DummyModuleIDs.Dummy, DummyCollection);
			DestinationFilter.AddModuleFilterForTest(destinationNkFilter);

			AssertNotNull("Destination Filter", DestinationFilter["Nk Destination"]);
			AssertEquals("Should be added destination filter", destinationNkFilter, DestinationFilter["Nk Destination"]);
			AssertEquals(false, destinationNkFilter.IsActive);

			Helper.MapModuleFilters("Nk Source", "Nk Destination");

			AssertEquals(true, destinationNkFilter.IsActive);
			AssertEquals("ABC", destinationNkFilter.Property);
			AssertEquals(ModuleNkFilter.ComparisonConstants.NotStartsWith, destinationNkFilter.ComparisonOperator);
		}

		#endregion

		#region TestMapModuleFlagsFilters

		public void TestMapModuleFlagsFilters()
		{
			string[] flagNames = new[] { "A", "B", "C" };
			GetFlagsQuery[] queries = new GetFlagsQuery[] { DummyGetFlagsQuery, DummyGetFlagsQuery, DummyGetFlagsQuery };

			ModuleFlagsFilter sourceFilter = new ModuleFlagsFilter("Source", flagNames, queries);
			SourceFilter.AddModuleFilterForTest(sourceFilter);

			AssertNotNull("Source Filter", SourceFilter["Source"]);
			AssertEquals("Should be added source filter", sourceFilter, SourceFilter["Source"]);

			sourceFilter.Property0 = true;
			sourceFilter.Property1 = true;
			sourceFilter.Property2 = true;
			sourceFilter.Property3 = true;
			sourceFilter.Property4 = true;
			sourceFilter.Property5 = true;
			sourceFilter.Property6 = true;
			sourceFilter.Property7 = true;
			sourceFilter.Property8 = true;
			sourceFilter.Property9 = true;

			sourceFilter.IsActive = true;

			ModuleFlagsFilter destinationFilter = new ModuleFlagsFilter("Destination", flagNames, queries);
			DestinationFilter.AddModuleFilterForTest(destinationFilter);

			AssertNotNull("Destination Filter", DestinationFilter["Destination"]);
			AssertEquals("Should be added destination filter", destinationFilter, DestinationFilter["Destination"]);

			Assert("Destination filter should not be active by default", !destinationFilter.IsActive);

			Assert("Default value of Property0 is false", !destinationFilter.Property0);
			Assert("Default value of Property1 is false", !destinationFilter.Property1);
			Assert("Default value of Property2 is false", !destinationFilter.Property2);
			Assert("Default value of Property3 is false", !destinationFilter.Property3);
			Assert("Default value of Property4 is false", !destinationFilter.Property4);
			Assert("Default value of Property5 is false", !destinationFilter.Property5);
			Assert("Default value of Property6 is false", !destinationFilter.Property6);
			Assert("Default value of Property7 is false", !destinationFilter.Property7);
			Assert("Default value of Property8 is false", !destinationFilter.Property8);
			Assert("Default value of Property9 is false", !destinationFilter.Property9);

			Helper.MapModuleFilters("Source", "Destination");

			Assert("Destination filter should be active", destinationFilter.IsActive);
			AssertEquals("Property0", sourceFilter.Property0, destinationFilter.Property0);
			AssertEquals("Property1", sourceFilter.Property1, destinationFilter.Property1);
			AssertEquals("Property2", sourceFilter.Property2, destinationFilter.Property2);
			AssertEquals("Property3", sourceFilter.Property3, destinationFilter.Property3);
			AssertEquals("Property4", sourceFilter.Property4, destinationFilter.Property4);
			AssertEquals("Property5", sourceFilter.Property5, destinationFilter.Property5);
			AssertEquals("Property6", sourceFilter.Property6, destinationFilter.Property6);
			AssertEquals("Property7", sourceFilter.Property7, destinationFilter.Property7);
			AssertEquals("Property8", sourceFilter.Property8, destinationFilter.Property8);
			AssertEquals("Property9", sourceFilter.Property9, destinationFilter.Property9);
		}

		#endregion

		#region TestMapModuleGuidFilters

		public void TestMapModuleGuidFilters()
		{
			ModuleGuidFilter sourceGuidFilter = new ModuleGuidFilter("Guid Source", DummyModuleIDs.Dummy, DummyGetGuidQuery, DummyCollection);
			SourceFilter.AddModuleFilterForTest(sourceGuidFilter);

			AssertNotNull("Source Filter", SourceFilter["Guid Source"]);
			AssertEquals("Should be added source filter", sourceGuidFilter, SourceFilter["Guid Source"]);

			sourceGuidFilter.Property = ZGuid.NewZGuid();
			sourceGuidFilter.IsActive = true;

			ModuleGuidFilter destinationGuidFilter = new ModuleGuidFilter("Guid Destination", DummyModuleIDs.Dummy, DummyGetGuidQuery, DummyCollection);
			DestinationFilter.AddModuleFilterForTest(destinationGuidFilter);

			AssertNotNull("Destination Filter", DestinationFilter["Guid Destination"]);
			AssertEquals("Should be added destination filter", destinationGuidFilter, DestinationFilter["Guid Destination"]);

			Assert("Destination filter should not be active by default", !destinationGuidFilter.IsActive);

			Helper.MapModuleFilters("Guid Source", "Guid Destination");

			Assert("Destination filter should be active", destinationGuidFilter.IsActive);
			AssertEquals(sourceGuidFilter.Property, destinationGuidFilter.Property);
		}

		#endregion

		#region TestMapModuleGuidsFilters

		public void TestMapModuleGuidsFilters()
		{
			ModuleGuidsFilter sourceGuidsFilter = new ModuleGuidsFilter("Guid Source", DummyModuleIDs.Dummy, DummyGetGuidsQuery, DummyCollection, DummyCollection);
			SourceFilter.AddModuleFilterForTest(sourceGuidsFilter);
			AssertNotNull("Source Filter", SourceFilter["Guid Source"]);
			AssertEquals("Should be added source filter", sourceGuidsFilter, SourceFilter["Guid Source"]);
			sourceGuidsFilter.Property1 = ZGuid.NewZGuid();
			sourceGuidsFilter.Property2 = ZGuid.NewZGuid();
			sourceGuidsFilter.IsActive = true;

			ModuleGuidsFilter destinationGuidsFilter = new ModuleGuidsFilter("Guid Destination", DummyModuleIDs.Dummy, DummyGetGuidsQuery, DummyCollection, DummyCollection);
			DestinationFilter.AddModuleFilterForTest(destinationGuidsFilter);

			AssertNotNull("Destination Filter", DestinationFilter["Guid Destination"]);
			AssertEquals("Should be added destination filter", destinationGuidsFilter, DestinationFilter["Guid Destination"]);
			AssertEquals(false, destinationGuidsFilter.IsActive);

			Helper.MapModuleFilters("Guid Source", "Guid Destination");

			AssertEquals(true, destinationGuidsFilter.IsActive);
			AssertEquals(sourceGuidsFilter.Property1, destinationGuidsFilter.Property1);
			AssertEquals(sourceGuidsFilter.Property2, destinationGuidsFilter.Property2);
		}

		#endregion

		#region TestMapModuleGuidsFiltersSwapped

		public void TestMapModuleGuidsFiltersSwapped()
		{
			ModuleGuidsFilter sourceGuidsFilter = new ModuleGuidsFilter("Guid Source", DummyModuleIDs.Dummy, DummyGetGuidsQuery, DummyCollection, DummyCollection);
			SourceFilter.AddModuleFilterForTest(sourceGuidsFilter);
			AssertNotNull("Source Filter", SourceFilter["Guid Source"]);
			AssertEquals("Should be added source filter", sourceGuidsFilter, SourceFilter["Guid Source"]);
			sourceGuidsFilter.Property1 = ZGuid.NewZGuid();
			sourceGuidsFilter.Property2 = ZGuid.NewZGuid();
			sourceGuidsFilter.IsActive = true;

			ModuleGuidsFilter destinationGuidsFilter = new ModuleGuidsFilter("Guid Destination", DummyModuleIDs.Dummy, DummyGetGuidsQuery, DummyCollection, DummyCollection);
			DestinationFilter.AddModuleFilterForTest(destinationGuidsFilter);

			AssertNotNull("Destination Filter", DestinationFilter["Guid Destination"]);
			AssertEquals("Should be added destination filter", destinationGuidsFilter, DestinationFilter["Guid Destination"]);
			AssertEquals(false, destinationGuidsFilter.IsActive);

			Helper.MapModuleGuidsFiltersSwapped("Guid Source", "Guid Destination");

			AssertEquals(true, destinationGuidsFilter.IsActive);
			AssertEquals(sourceGuidsFilter.Property2, destinationGuidsFilter.Property1);
			AssertEquals(sourceGuidsFilter.Property1, destinationGuidsFilter.Property2);
		}

		#endregion

		#region TestMapModuleLocationFilters

		public void TestMapModuleLocationFilters()
		{
			ModuleLocationFilter sourceLocationFilter = new ModuleLocationFilter("Location Source", DummyGetCodeQuery, DummyLocationCollection, DummyLocationCollection);
			SourceFilter.AddModuleFilterForTest(sourceLocationFilter);
			AssertNotNull("Source Filter", SourceFilter["Location Source"]);
			AssertEquals("Should be added source filter", sourceLocationFilter, SourceFilter["Location Source"]);
			sourceLocationFilter.Property1 = "AUSYD";
			sourceLocationFilter.Property2 = "HKHKG";
			sourceLocationFilter.IsActive = true;

			ModuleLocationFilter destinationLocationFilter = new ModuleLocationFilter("Location Destination", DummyGetCodeQuery, DummyLocationCollection, DummyLocationCollection);
			DestinationFilter.AddModuleFilterForTest(destinationLocationFilter);

			AssertNotNull("Destination Filter", DestinationFilter["Location Destination"]);
			AssertEquals("Should be added destination filter", destinationLocationFilter, DestinationFilter["Location Destination"]);
			AssertEquals(false, destinationLocationFilter.IsActive);

			Helper.MapModuleFilters("Location Source", "Location Destination");

			AssertEquals(true, destinationLocationFilter.IsActive);
			AssertEquals("AUSYD", destinationLocationFilter.Property1);
			AssertEquals("HKHKG", destinationLocationFilter.Property2);
		}

		#endregion

		#endregion

		#region Helper Workflow filters tests

		#region TestMapWorkflowModuleFilters

		public void TestMapWorkflowModuleFilters()
		{
			ZDateTime now = ZDateTime.Now;
			WorkflowModuleFilter sourceLastCompletedMilestoneFilter = new WorkflowModuleFilter("Last Completed Milestone", typeof(DummyBusinessObject), WorkflowModuleFilterTypes.MilestoneLastCompleted);
			SourceFilter.AddModuleFilterForTest(sourceLastCompletedMilestoneFilter);
			AssertNotNull("Source Filter", SourceFilter["Last Completed Milestone"]);
			AssertEquals("Should be added source filter", sourceLastCompletedMilestoneFilter, SourceFilter["Last Completed Milestone"]);
			sourceLastCompletedMilestoneFilter.Property1 = now.AddDays(-5);
			sourceLastCompletedMilestoneFilter.Property2 = now.AddDays(7);
			sourceLastCompletedMilestoneFilter.PropertySearch = "foo";
			sourceLastCompletedMilestoneFilter.IsActive = true;

			WorkflowModuleFilter destinationLastCompletedMilestoneFilter = new WorkflowModuleFilter("Last Completed Milestone", typeof(DummyBusinessObject), WorkflowModuleFilterTypes.MilestoneLastCompleted);
			DestinationFilter.AddModuleFilterForTest(destinationLastCompletedMilestoneFilter);

			AssertNotNull("Destination Filter", DestinationFilter["Last Completed Milestone"]);
			AssertEquals("Should be added destination filter", destinationLastCompletedMilestoneFilter, DestinationFilter["Last Completed Milestone"]);
			AssertEquals(false, destinationLastCompletedMilestoneFilter.IsActive);

			Helper.MapModuleFilters("Last Completed Milestone");

			AssertEquals(true, destinationLastCompletedMilestoneFilter.IsActive);
			AssertEquals(sourceLastCompletedMilestoneFilter.Property1, destinationLastCompletedMilestoneFilter.Property1);
			AssertEquals(sourceLastCompletedMilestoneFilter.Property2, destinationLastCompletedMilestoneFilter.Property2);
			AssertEquals(sourceLastCompletedMilestoneFilter.PropertySearch, destinationLastCompletedMilestoneFilter.PropertySearch);
		}

		#endregion

		#region TestMapWorkflowModuleTextFilters

		public void TestMapWorkflowModuleTextFilters()
		{
			WorkflowModuleTextFilter sourceMilestoneCompletedFilter = new WorkflowModuleTextFilter("Milestone Completed", DummyGetTextQuery, DummyCollection, typeof(DummyBusinessObject));
			SourceFilter.AddModuleFilterForTest(sourceMilestoneCompletedFilter);
			AssertNotNull("Source Filter", SourceFilter["Milestone Completed"]);
			AssertEquals("Should be added source filter", sourceMilestoneCompletedFilter, SourceFilter["Milestone Completed"]);
			sourceMilestoneCompletedFilter.Property = "some string";
			sourceMilestoneCompletedFilter.IsActive = true;

			WorkflowModuleTextFilter destinationMilestoneCompletedFilter = new WorkflowModuleTextFilter("Milestone Completed", DummyGetTextQuery, DummyCollection, typeof(DummyBusinessObject));
			DestinationFilter.AddModuleFilterForTest(destinationMilestoneCompletedFilter);

			AssertNotNull("Destination Filter", DestinationFilter["Milestone Completed"]);
			AssertEquals("Should be added destination filter", destinationMilestoneCompletedFilter, DestinationFilter["Milestone Completed"]);
			AssertEquals(false, destinationMilestoneCompletedFilter.IsActive);
			AssertEquals(false, destinationMilestoneCompletedFilter.IsActive);

			Helper.MapModuleFilters("Milestone Completed");

			AssertEquals(true, destinationMilestoneCompletedFilter.IsActive);
			AssertEquals("some string", destinationMilestoneCompletedFilter.Property);
		}

		#endregion

		#region TestMapModuleFilterGeneric

		public void TestMapModuleFilterGeneric()
		{
			var now = ZDateTime.Now;
			var sourceFilter = new ModuleFilterSource("Last Completed Milestone", typeof(DummyBusinessObject), WorkflowModuleFilterTypes.MilestoneLastCompleted);
			SourceFilter.AddModuleFilterForTest(sourceFilter);
			AssertNotNull("Source Filter", SourceFilter["Last Completed Milestone"]);
			AssertEquals("Should be added source filter", sourceFilter, SourceFilter["Last Completed Milestone"]);
			sourceFilter.Property1 = now.AddDays(-5);
			sourceFilter.Property2 = now.AddDays(7);
			sourceFilter.PropertySearch = "foo";
			sourceFilter.IsActive = true;

			var destinationFilter = new ModuleFilterDestination("Last Completed Milestone", typeof(DummyBusinessObject), WorkflowModuleFilterTypes.MilestoneLastCompleted);
			DestinationFilter.AddModuleFilterForTest(destinationFilter);

			Helper.MapModuleFilters<WorkflowModuleFilter>("Last Completed Milestone");

			CombineAssertions(() =>
			{
				AssertEquals(true, destinationFilter.IsActive);
				AssertEquals(sourceFilter.Property1, destinationFilter.Property1);
				AssertEquals(sourceFilter.Property2, destinationFilter.Property2);
				AssertEquals(sourceFilter.PropertySearch, destinationFilter.PropertySearch);
			});
		}

		class ModuleFilterSource : WorkflowModuleFilter
		{
			public ModuleFilterSource(ZString description, Type bizObjType, WorkflowModuleFilterTypes filterType)
				: base(description, bizObjType, filterType)
			{
			}
		}

		class ModuleFilterDestination : WorkflowModuleFilter
		{
			public ModuleFilterDestination(ZString description, Type bizObjType, WorkflowModuleFilterTypes filterType)
				: base(description, bizObjType, filterType)
			{
			}
		}

		#endregion

		#endregion

		#region Implementation

		public ZQuery DummyGetTextQueryWithOperator(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery();
		}

		public ZQuery DummyGetTextAndNkQuery(SQLComparisonOperator textValueComparisonOperator, ZString textValue, ZString nK)
		{
			return new ZQuery();
		}

		public ZQuery DummyGetNkQuery(ZString nK)
		{
			return new ZQuery();
		}

		public ZQuery DummyGetFlagsQuery(ZBool value)
		{
			return new ZQuery();
		}

		public ZQuery DummyGetGuidQuery(ZGuid value)
		{
			return new ZQuery();
		}

		public ZQuery DummyGetGuidsQuery(ZGuid value1, ZGuid value2)
		{
			return new ZQuery();
		}

		public ZQuery DummyGetCodeQuery(ZString value1, ZString value2)
		{
			return new ZQuery();
		}

		public ZDBOnlySubQuery DummyGetTextQuery(ZString value)
		{
			return new ZDBOnlySubQuery(typeof(DummyBusinessObject), CargoWise.Schema.Schema.GenericGuidSchemaColumn);
		}

		public IBusinessObjectCollection DummyCollection
		{
			get { return new DummyBusinessObjectCollection(Factory); }
		}

		public LocationCollection DummyLocationCollection
		{
			get { return new LocationCollection(Factory); }
		}

		protected override void SetUp()
		{
			base.SetUp();

			SourceFilter = new DummyFilterStripBusinessObject();
			DestinationFilter = new DummyFilterStripBusinessObject();
			Helper = new FilterStripBOMappingHelper(SourceFilter, DestinationFilter);
		}

		DummyFilterStripBusinessObject SourceFilter;
		DummyFilterStripBusinessObject DestinationFilter;
		FilterStripBOMappingHelper Helper;

		#endregion
	}
}
