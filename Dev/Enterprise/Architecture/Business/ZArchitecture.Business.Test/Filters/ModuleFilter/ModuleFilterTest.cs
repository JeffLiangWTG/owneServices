using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleFilter))]
	sealed class ModuleFilterTest : ModuleFilterTestCase<ModuleFilter>
	{
		#region Test Constructors

		[ExpectExceptionMessage(typeof(ArgumentException), "DummyModuleFilter description cannot be empty.")]
		public void TestConstructionWithEmptyDescriptionThrowsException()
		{
			new DummyModuleFilter("", DummyBizoSchema.Z0_Code);
		}

		[ExpectExceptionMessage(typeof(NullReferenceException), "DummyModuleFilter (moo) filterColumn cannot be null.")]
		public void TestConstructionWithNullFilterColumnThrowsException()
		{
			new DummyModuleFilter("moo", (SchemaStringColumn)null);
		}

		[ExpectExceptionMessage(typeof(NullReferenceException), "DummyModuleFilter (moo) queryDelegate cannot be null.")]
		public void TestConstructionWithNullQueryDelegateThrowsException()
		{
			new DummyModuleFilter("moo", (Delegate)null);
		}

		#endregion

		#region TestIsActive

		public void TestIsActive()
		{
			AssertEquals("Filters should not be active on construction.", false, Filter.IsActive);

			var isActiveChangedFired = false;
			Filter.IsActiveChanged += delegate
			{
				isActiveChangedFired = true;
			};

			Filter.IsActive = true;

			AssertEquals("Setting IsActive should fire the IsActiveChanged event.", true, isActiveChangedFired);
		}

		public void TestIsActiveCallsClearWhenFalse()
		{
			var mock = new Mock<ModuleFilter>((ZString)"moo", DummyBizoSchema.Z0_Code) { CallBase = true };
			var filter = mock.Object;

			filter.IsActive = true;
			filter.OrCategory = FilterOrCategory.Red;
			mock.Protected().Verify("ClearCore", Times.Never());

			filter.IsActive = false;
			AssertEquals(FilterOrCategory.None, filter.OrCategory);
			mock.Protected().Verify("ClearCore", Times.Once());
		}

		[ExpectNoExceptions]
		public void TestIXmlSerializableImplementation()
		{
			var mock = new Mock<ModuleFilter>((ZString)"moo", DummyBizoSchema.Z0_Code) { CallBase = true };
			IXmlSerializable filter = mock.Object;

			using (XmlTextReader reader = new XmlTextReader(new StringReader("")))
			using (XmlTextWriter writer = new XmlTextWriter(new StringWriter()))
			{
				filter.ReadXml(reader);
				mock.Protected().Verify("DeserializePropertiesFromXml", Times.Once(), reader);

				filter.WriteXml(writer);
				mock.Protected().Verify("SerializePropertiesToXml", Times.Once(), writer);
			}
		}

		public void TestIsActivePreservesMandatorySecurityFilterOrCategory()
		{
			var someMandatorySecurityFilter = new ModuleTextFilter("Some Mandatory Security Filter", DummyBizoSchema.Z0_NVarChar) { Property = "U1" };
			someMandatorySecurityFilter.IsActive = true;
			someMandatorySecurityFilter.IsMandatorySecurityFilter = true;
			someMandatorySecurityFilter.OrCategory = FilterOrCategory.MandatoryFilterOrCategory;
			someMandatorySecurityFilter.IsActive = false;
			AssertEquals(FilterOrCategory.MandatoryFilterOrCategory, someMandatorySecurityFilter.OrCategory);
		}

		#endregion

		#region TestIsPublishedOnWeb

		public void TestIsPublishedOnWeb()
		{
			AssertEquals("Filters should be web published by default.", true, Filter.IsPublishedOnWeb);

			Filter.IsPublishedOnWeb = false;
			AssertEquals(false, Filter.IsPublishedOnWeb);

			Filter.IsPublishedOnWeb = true;
			AssertEquals(true, Filter.IsPublishedOnWeb);
		}

		#endregion

		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			Assert("Nothing to test for the abstract ModuleFilter class.", true);
		}

		#endregion

		public void TestFilterMaxLength()
		{
			var filterBizO = new DummyFilterStripBusinessObjectForDefaultsTesting();
			var someFilter = ((ModuleTextFilter)filterBizO["someFilter"]);
			AssertEquals(255, someFilter.MaxLength);
		}

		public void TestMaxMaximumLength()
		{
			var filterBizO = new DummyFilterStripBusinessObjectForDefaultsTesting();
			var nVarCharMaxFilter = ((ModuleTextFilter)filterBizO["nVarCharMaxFilter"]);
			AssertEquals(ModuleFilter.MaxMaximumLength, nVarCharMaxFilter.MaxLength);
		}

		#region TestCommonModuleFilterQuery

		public void TestCommonModuleFilterQuery()
		{
			var collection = new ModuleFilterCollection();

			var filter1 = new DummyModuleFilter("Z0_Code", DummyBizoSchema.Z0_Code);
			var filter2 = new DummyModuleFilter("Z0_Description", DummyBizoSchema.Z0_Description);
			filter1.IsCommon = true;
			filter2.IsCommon = true;
			collection.AddFilter(filter1);
			collection.AddFilter(filter2);

			var commonFilter = (DummyModuleFilter)filter1.NewCommonModuleFilter(filter1.Category, collection);
			commonFilter.DummyProperty = "hello";

			AssertEquals("Z0_Code = 'hello' or Z0_Description = 'hello'", commonFilter.Query.LiteralTextADO);
		}

		public void TestCommonModuleFilterQueryMaxLength()
		{
			var collection = new ModuleFilterCollection();

			var filter1 = new DummyModuleFilter("Z0_Code", DummyBizoSchema.Z0_Code);
			var filter2 = new DummyModuleFilter("Z0_Description", DummyBizoSchema.Z0_Description);
			filter1.IsCommon = true;
			filter2.IsCommon = true;
			collection.AddFilter(filter1);
			collection.AddFilter(filter2);

			var commonFilter = (DummyModuleFilter)filter1.NewCommonModuleFilter(filter1.Category, collection);
			commonFilter.DummyProperty = "hello";

			AssertEquals(100, commonFilter.MaxLength);
		}

		public void TestAddFilter_ShouldThrowException_WhenFilterAlreadyExists()
		{
			var collection = new ModuleFilterCollection();

			const string expectedExpMsg = "A filter already exists with the description 'Z0_Code'; filter descriptions must be unique.";
			var filter1 = new DummyModuleFilter("Z0_Code", DummyBizoSchema.Z0_Code);
			var filter2 = new DummyModuleFilter("Z0_Code", DummyBizoSchema.Z0_Code);

			collection.AddFilter(filter1);

			AssertExceptionThrown<ArgumentException>("AddFilter should throw exception on duplicate", expectedExpMsg, () => collection.AddFilter(filter2));
		}

		#endregion

		#region TestShallowClone

		public void TestShallowClone()
		{
			var filter = new DummyModuleFilter("moo", DummyBizoSchema.Z0_Code);
			filter.DummyProperty = "comic book guy";

			var clonedFilter = (DummyModuleFilter)filter.ShallowClone();
			AssertNotEquals(filter, clonedFilter);
			AssertEquals("comic book guy", clonedFilter.DummyProperty);
		}

		#endregion

		#region TestShallowCloneAndClearValues

		public void TestShallowCloneAndClearValues()
		{
			var filter = new DummyModuleFilter("moo", DummyBizoSchema.Z0_Code);
			filter.DummyProperty = "comic book guy";

			var clonedFilter = (DummyModuleFilter)filter.ShallowCloneAndClearValues("oink");
			AssertNotEquals(filter, clonedFilter);
			AssertEquals("comic book guy", clonedFilter.DummyProperty);
			AssertEquals("oink", clonedFilter.Description);
		}

		public void TestShallowCloneAndClearVisibility()
		{
			var filter = new DummyModuleFilter("moo", DummyBizoSchema.Z0_Code);
			filter.Visibility = FilterVisibility.AlwaysVisible;

			var clonedFilter = (DummyModuleFilter)filter.ShallowCloneAndClearValues("oink");
			AssertEquals(FilterVisibility.Visible, clonedFilter.Visibility);
		}

		#endregion

		#region TestCopyTransient

		public void TestCopyTransient()
		{
			var filter1 = new DummyModuleFilter("moo", DummyBizoSchema.Z0_Code);
			var filter2 = new DummyModuleFilter("You", DummyBizoSchema.Z0_Code);
			filter2.CopyTransientProperties(filter1);
			AssertEquals("Description should not be changed", "You", filter2.Description);
		}

		#endregion

		#region TestLocalizedDescription

		public void TestLocalizedDescription()
		{
			const string resourceID = "526e7369-3139-4fc0-893a-48f232ab4f8c";
			var filter = new DummyModuleFilter("moo", DummyBizoSchema.Z0_Code)
			{
				DummyProperty = "comic book guy",
				MultilingualDescription = ResString.GetMultilingualString(resourceID, "moo")
			};

			using (var cache = Res.UseMockData())
			{
				AssertEquals("moo", filter.LocalizedDescription);

				cache.Put(resourceID, new ResourceStringData("", "boo"));

				AssertEquals("boo", filter.LocalizedDescription);
			}
		}

		#endregion

		#region TestExclusiveFiltersValidation

		public void TestExclusiveFiltersValidation()
		{
			var filtersList = new ActiveModuleFiltersProviderForTest();

			var someFilter = new ModuleTextFilter("Some Filter", DummyBizoSchema.Z0_VarCharMax) { Property = "qwerty" };
			filtersList.Filters.Add(someFilter);
			someFilter.IsActive = true;
			someFilter.ActiveModuleFiltersProviderHelper = filtersList;

			var someMandatorySecurityFilter = new ModuleTextFilter("Some Mandatory Security Filter", DummyBizoSchema.Z0_NVarChar) { Property = "U1" };
			filtersList.Filters.Add(someMandatorySecurityFilter);
			someMandatorySecurityFilter.IsActive = true;
			someMandatorySecurityFilter.ActiveModuleFiltersProviderHelper = filtersList;
			someMandatorySecurityFilter.IsMandatorySecurityFilter = true;

			var exclusiveFilter = new ModuleTextFilter("Exclusive Filter", DummyBizoSchema.Z0_Code) { IsExclusiveHelper = true };
			filtersList.Filters.Add(exclusiveFilter);
			exclusiveFilter.IsActive = true;
			exclusiveFilter.ActiveModuleFiltersProviderHelper = filtersList;

			var exclusiveFilter2 = new ModuleTextFilter("Exclusive Filter (2)", DummyBizoSchema.Z0_Code) { IsExclusiveHelper = true };
			filtersList.Filters.Add(exclusiveFilter2);
			exclusiveFilter2.IsActive = true;
			exclusiveFilter2.ActiveModuleFiltersProviderHelper = filtersList;

			AssertEquals("Precondition", false, exclusiveFilter.HasErrors);
			AssertEquals("Precondition", false, someFilter.HasWarnings);
			AssertEquals("Precondition", false, someMandatorySecurityFilter.HasWarnings);

			exclusiveFilter.Property = "abc";

			someFilter.Validation.ValidateAll();
			someMandatorySecurityFilter.Validation.ValidateAll();
			exclusiveFilter.Validation.ValidateAll();

			AssertEquals(false, exclusiveFilter.HasErrors);
			AssertEquals(true, someFilter.HasWarnings);
			AssertEquals(false, someMandatorySecurityFilter.HasWarnings);

			exclusiveFilter2.Property = "bcd";

			someFilter.Validation.ValidateAll();
			someMandatorySecurityFilter.Validation.ValidateAll();
			exclusiveFilter.Validation.ValidateAll();

			AssertEquals(true, exclusiveFilter.HasErrors);
			AssertEquals(true, someFilter.HasWarnings);
			AssertEquals(false, someMandatorySecurityFilter.HasWarnings);

			exclusiveFilter.OrCategory = FilterOrCategory.Red;
			exclusiveFilter2.OrCategory = FilterOrCategory.Red;

			someFilter.Validation.ValidateAll();
			someMandatorySecurityFilter.Validation.ValidateAll();
			exclusiveFilter.Validation.ValidateAll();

			AssertEquals(false, exclusiveFilter.HasErrors);
			AssertEquals(true, someFilter.HasWarnings);
			AssertEquals(false, someMandatorySecurityFilter.HasWarnings);

			exclusiveFilter.Property = ZString.Empty;
			exclusiveFilter2.Property = ZString.Empty;

			someFilter.Validation.ValidateAll();
			someMandatorySecurityFilter.Validation.ValidateAll();
			exclusiveFilter.Validation.ValidateAll();

			AssertEquals(false, exclusiveFilter.HasErrors);
			AssertEquals(false, someFilter.HasWarnings);
			AssertEquals(false, someMandatorySecurityFilter.HasWarnings);
		}

		class ActiveModuleFiltersProviderForTest : IActiveModuleFiltersProvider
		{
			public IEnumerable<ModuleFilter> ActiveModuleFilters
			{
				get { return Filters.ToArray(); }
			}

			public IEnumerable<ModuleFilter> AlwaysAppliedModuleFilters => throw new NotImplementedException();

			public List<ModuleFilter> Filters = new List<ModuleFilter>();
		}

		#endregion

		#region TestQueryCaching

		public void TestQueryCaching()
		{
			var filter = (DummyModuleFilter)Filter;
			filter.DummyProperty = "AAA";
			AssertEquals(0, filter.GetQueryExecutionCount);

			var query1 = filter.Query;
			AssertEquals(1, filter.GetQueryExecutionCount);

			var query2 = filter.Query;
			AssertEquals("The count should not have increased (and the cached query returned instead) because the filter does not have changes, and yet...", 1, filter.GetQueryExecutionCount);
			AssertEquals(query1.LiteralTextSqlFormatted, query2.LiteralTextSqlFormatted);

			filter.DummyProperty = "BBB";
			AssertEquals(1, filter.GetQueryExecutionCount);

			var query3 = filter.Query;
			AssertEquals("The count should increase because there were changes, and yet...", 2, filter.GetQueryExecutionCount);
			AssertNotEquals("A new query should have been generated rather than using the cached one, and yet...", query2.LiteralTextSqlFormatted, query3.LiteralTextSqlFormatted);

			var query4 = filter.Query;
			AssertEquals("The count should not have increased (and the cached query returned instead) because the filter does not have changes, and yet...", 2, filter.GetQueryExecutionCount);
			AssertEquals(query3.LiteralTextSqlFormatted, query4.LiteralTextSqlFormatted);
		}

		public void TestQuery_ShouldReturnCloneOfCachedQuery()
		{
			var filter = (DummyModuleFilter)Filter;
			var query1 = filter.Query;
			var query2 = filter.Query;

			AssertEquals(false, ReferenceEquals(query1, query2));
		}

		#endregion

		#region InvalidateCachedQuery

		public void TestChangeOrCategory_ShouldInvalidateCachedQuery()
		{
			var filterBizo = new DummyFilterBusinessObject();
			var filter = filterBizo.FilterStrips.AddNew("Z0_Description").CurrentModuleFilter;
			filter.CachedQuery_ForTest = new ZQuery();

			filter.OrCategory = FilterOrCategory.BurlyWood;
			AssertNull(filter.CachedQuery_ForTest);
		}

		public void TestChangeGroupOrCategory_ShouldInvalidateCachedQuery()
		{
			var filterBizo = new DummyFilterBusinessObject();
			var filter = filterBizo.FilterStrips.AddNew("Z0_Description").CurrentModuleFilter;
			filter.CachedQuery_ForTest = new ZQuery();

			filter.GroupOrCategory = FilterOrCategory.BurlyWood;
			AssertNull(filter.CachedQuery_ForTest);
		}

		#endregion

		#region TestSetValueFromInitialCode
		public void TestShouldSetValueFromInitialCode()
		{
			var initialProperty = "Property";
			var initialCode = "Code";
			var moduleFilter = new DummyModuleFilter("description", DummyBizoSchema.GenericStringSchemaColumn);
			AssertEquals("Default implementation is false", false, moduleFilter.ShouldSetValueFromInitialCode(initialCode, initialProperty));
		}

		public void TestGetFormattedInitialCode_FilterColumnName()
		{
			var moduleFilter = new DummyModuleFilter("description", DummyBizoSchema.GenericStringSchemaColumn);
			AssertEquals("Default implementation is empty string", ZString.Empty, moduleFilter.GetFormattedInitialCode_FilterColumnName());
		}

		public void TestGetFormattedInitialCode_Prefix()
		{
			var moduleFilter = new DummyModuleFilter("description", DummyBizoSchema.GenericStringSchemaColumn);
			AssertEquals("Default implementation is empty string", ZString.Empty, moduleFilter.GetFormattedInitialCode_Prefix());
		}
		#endregion

		#region TestSelectedFiltersCacheShouldThrowDeveloperExceptionWhenWebServiceOrWeb

		public void TestSelectedFiltersCacheShouldThrowDeveloperExceptionWhenWebServiceOrWeb()
		{
			Globals.IsWebService = false;
			Globals.IsWeb = false;

			AssertNoExceptionThrown(() => { var notCoolButOk = ModuleFilter.SelectedFiltersCache; });

			Globals.IsWebService = true;
			Globals.IsWeb = false;

			AssertExceptionThrown<DeveloperNotificationException>(() => { var notOk = ModuleFilter.SelectedFiltersCache; });

			Globals.IsWebService = false;
			Globals.IsWeb = true;

			AssertExceptionThrown<DeveloperNotificationException>(() => { var notOk = ModuleFilter.SelectedFiltersCache; });
		}

		#endregion

		#region TestSubGroup

		class DummyModuleFilterSubGroupForModuleFilterTest : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return ZQuery.NoResultQuery;
			}
		}

		public void TestSubGroup()
		{
			var subGroup = new DummyModuleFilterSubGroupForModuleFilterTest();
			var moduleTextFilter = new ModuleTextFilter("Z0_Description", DummyBizoSchema.Z0_Description)
			{
				SubGroup = subGroup,
				SqlComparisonOperator = SQLComparisonOperator.Equal
			};
			var moduleGuidFilter = new ModuleGuidFilter("Z0_Guid", DummyModuleIDs.Dummy, DummyBizoSchema.Z0_Guid, new StmNoteNonDependentCollection(Factory))
			{
				SubGroup = subGroup,
				SqlComparisonOperator = SQLComparisonOperator.Equal
			};

			AssertNotEquals(null, moduleTextFilter.SubGroup);
			AssertNotEquals(null, moduleGuidFilter.SubGroup);
		}

		public void TestSubGroupEqualWithBlankQueryDelegate()
		{
			var subGroup = new DummyModuleFilterSubGroupForModuleFilterTest();
			var moduleTextFilter = new ModuleTextFilter("Z0_Description", DummyBizoSchema.Z0_Description)
			{
				SubGroup = subGroup,
				SqlComparisonOperator = SQLComparisonOperator.Equal,
				IsBlankFilterQueryDelegate = () => ZQuery.NoResultQuery
			};
			var moduleGuidFilter = new ModuleGuidFilter("Z0_Guid", DummyModuleIDs.Dummy, DummyBizoSchema.Z0_Guid, new StmNoteNonDependentCollection(Factory))
			{
				SubGroup = subGroup,
				SqlComparisonOperator = SQLComparisonOperator.Equal,
				IsBlankFilterQueryDelegate = () => ZQuery.NoResultQuery
			};

			AssertNotEquals(null, moduleTextFilter.SubGroup);
			AssertNotEquals(null, moduleGuidFilter.SubGroup);
		}

		public void TestSubGroupIsBlankWithoutBlankQueryDelegate()
		{
			var subGroup = new DummyModuleFilterSubGroupForModuleFilterTest();
			var moduleTextFilter = new ModuleTextFilter("Z0_Description", DummyBizoSchema.Z0_Description)
			{
				SubGroup = subGroup,
				SqlComparisonOperator = SQLComparisonOperator.IsBlank
			};
			var moduleGuidFilter = new ModuleGuidFilter("Z0_Guid", DummyModuleIDs.Dummy, DummyBizoSchema.Z0_Guid, new StmNoteNonDependentCollection(Factory))
			{
				SubGroup = subGroup,
				SqlComparisonOperator = SQLComparisonOperator.IsBlank
			};

			AssertNotEquals(null, moduleTextFilter.SubGroup);
			AssertNotEquals(null, moduleGuidFilter.SubGroup);
		}

		public void TestSubGroupIsBlankWithBlankQueryDelegate()
		{
			var subGroup = new DummyModuleFilterSubGroupForModuleFilterTest();
			var moduleTextFilter = new ModuleTextFilter("Z0_Description", DummyBizoSchema.Z0_Description)
			{
				SubGroup = subGroup,
				SqlComparisonOperator = SQLComparisonOperator.IsBlank,
				IsBlankFilterQueryDelegate = () => ZQuery.NoResultQuery
			};
			var moduleGuidFilter = new ModuleGuidFilter("Z0_Guid", DummyModuleIDs.Dummy, DummyBizoSchema.Z0_Guid, new StmNoteNonDependentCollection(Factory))
			{
				SubGroup = subGroup,
				SqlComparisonOperator = SQLComparisonOperator.IsBlank,
				IsBlankFilterQueryDelegate = () => ZQuery.NoResultQuery
			};

			AssertEquals(null, moduleTextFilter.SubGroup);
			AssertEquals(null, moduleGuidFilter.SubGroup);
		}

		#endregion

		#region TestDescriptionWithoutInstanceNumber

		public void TestGetDescriptionWithoutInstanceNumber_WhenIsDuplicateDefault()
		{
			AssertDescriptionWithoutInstanceNumberEquals("Should not truncate", "Description", "Description");
			AssertDescriptionWithoutInstanceNumberEquals("Should not truncate", "Description (details)", "Description (details)");
			AssertDescriptionWithoutInstanceNumberEquals("Should truncate", "Description (1)", "Description");
			AssertDescriptionWithoutInstanceNumberEquals("Should truncate", "Description (14445)", "Description");
			AssertDescriptionWithoutInstanceNumberEquals("Should truncate end only", "(1) Description (1)", "(1) Description");
			AssertDescriptionWithoutInstanceNumberEquals("Should truncate only number", "Description (details) (14445)", "Description (details)");
		}

		void AssertDescriptionWithoutInstanceNumberEquals(string message, string description, string expectedTruncatedDescription)
		{
			var filter = new DummyModuleFilter(description, DummyBizoSchema.Z0_Code);
			filter.IsDuplicateDefault = true;

			AssertEquals(message, expectedTruncatedDescription, filter.DescriptionWithoutInstanceNumber);
		}

		public void TestGetDescriptionWithoutInstanceNumber_WhenIsNotDuplicateDefault()
		{
			var filter = new DummyModuleFilter("Filter Description (1)", DummyBizoSchema.Z0_Code);
			filter.IsDuplicateDefault = false;

			AssertEquals("Should not truncate", "Filter Description (1)", filter.DescriptionWithoutInstanceNumber);
		}

		#endregion

		#region TestLogWhenDefaultFilterMissed

		public void TestMessageWhenLoadDefaultFilterWithParentModule()
		{
			ErrorReporter.Clear();

			var module = new DummyModuleHelper();
			module.LimitedColumns = new DummyZLimitedColumnsProvider();
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new DummyModuleCodeFilter((ZString)"moo", DummyBizoSchema.Z0_Description, new StmNoteNonDependentCollection(Factory), DummyBizoSchema.Z0_Code, new StmNoteNonDependentCollection(Factory));
			filterStripBizO.ParentModule = module;

			filterStripBizO.AddModuleFilterForTest(filter);
			_ = filterStripBizO.SaveLayout("savedFilter");

			var expectedMessage = "ParentModule: 'Enterprise.ZArchitecture.Business.Testing.ModuleFilterTest+DummyModuleHelper', CodeSchemaColumn Exists: 'False', DescriptionSchemaColumn Exists: 'False', alwaysAppliedFilters Count: '0', fModuleFilters Count: '0'";
			AssertEquals(expectedMessage, filterStripBizO.DefaultFilterLoadMessage);
		}

		class DummyZLimitedColumnsProvider : IZLimitedColumnsProvider
		{
			public SchemaColumn CodeSchemaColumn { get; set; }

			public SchemaColumn DescriptionSchemaColumn { get; set; }
		}

		class DummyModuleCodeFilter : ModuleCodeFilter
		{
			public DummyModuleCodeFilter(FilterCategory category, ModuleFilterCollection parentCollection) : base(category, parentCollection)
			{
			}

			public DummyModuleCodeFilter(ZString description, SchemaStringColumn filterColumn1, IBusinessObjectCollection list1, SchemaStringColumn filterColumn2, IBusinessObjectCollection list2)
				: base(description, filterColumn1, list1, filterColumn2, list2)
			{
			}

			protected override FilterCategory DefaultCategory
			{
				get { return FilterCategories.Other; }
			}
			protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			{
				throw new NotImplementedException();
			}
		}

		class DummyModuleHelper : IZModuleHelper
		{
			public IZLimitedColumnsProvider LimitedColumns { get; set; }
			public ModuleIdentifier ParentModuleID { get; set; }

			public ModuleIdentifier ID => new ClientModuleIdentifier(TestClientModuleId.ClientModuleID, (NoResString)"ClientModuleID");

			public bool SupportsWorkflow => throw new NotImplementedException();

			public bool AllowNew => throw new NotImplementedException();

			public void Dispose()
			{
				throw new NotImplementedException();
			}

			public SecurityCheckpoint[] GetSecurityCheckpointForPopups()
			{
				return null;
			}
		}

		#endregion

		#region Implementation

		protected override ModuleFilter GetNewModuleFilter()
		{
			return new DummyModuleFilter("moo", DummyBizoSchema.Z0_Code);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		#endregion
	}
}
