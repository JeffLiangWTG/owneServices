using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class ModuleFilterCollectionTest : TestCaseWithFactory
	{
		#region TestShallowClone

		IDisposable ValidationError(ModuleTextFilter filter)
		{
			var oldValidation = filter.PropertyValidation;
			filter.PropertyValidation = info =>
			{
				info.AddError("Test");
			};

			return new DisposableAction(() =>
			{
				filter.PropertyValidation = oldValidation;
			});
		}

		public void TestShallowClone()
		{
			var textFilter = new ModuleTextFilter("bart", DummyBizoSchema.Z0_Description);
			textFilter.IsCommon = true; // prove we don't clone the auto-created common module filter
			textFilter.Property = "simpson";
			textFilter.IsActive = true;
			using (ValidationError(textFilter))
			{
				textFilter.Validation.ValidateAll();
			}

			Collection.AddFilter(textFilter);
			AssertHasErrors(textFilter.PropertyInfo);

			var numberFilter = Collection.AddNumberRangeFilter("homer", DummyBizoSchema.Z0_Decimal);
			numberFilter.Property1 = 19;
			numberFilter.Property2 = 29;

			var collectionClone = Collection.ShallowClone();
			var textFilterClone = (ModuleTextFilter)collectionClone["bart"];
			var numberFilterClone = (ModuleNumberRangeFilter)collectionClone["homer"];

			AssertNotEquals(textFilter, textFilterClone);
			AssertNotEquals(numberFilter, numberFilterClone);
			AssertNotEquals("simpson", textFilterClone.Property);
			AssertEquals(false, textFilterClone.HasErrors);
			AssertEquals(19M, numberFilterClone.Property1);
			AssertEquals(29M, numberFilterClone.Property2);
			AssertEquals(false, textFilterClone.IsActive);
			AssertEquals(false, numberFilterClone.IsActive);
		}

		#endregion

		#region AddAttributeFilters

		[ExpectNoExceptions]
		public void TestAddAttributeFilters()
		{
			var filters = new ModuleFilterCollection();

			var attributes = new CustomAttribute[]
							 {
								new CustomAttribute("1Bstring", DummyBizoSchema.Z0_VarCharMax, (NoResString)"1Astring"),
								new CustomAttribute("", DummyBizoSchema.Z0_Bool, (NoResString)"1Bstring"),
								new CustomAttribute("2Adate", DummyBizoSchema.Z0_Date, (NoResString)"2 A Date"),
								new CustomAttribute("3Bbool", DummyBizoSchema.Z0_Bool, (NoResString)"3ABool"),
								new CustomAttribute("4Bnum", DummyBizoSchema.Z0_Money, (NoResString)"4ANum"),
								new CustomAttribute("5Bstring", DummyBizoSchema.Z0_Description, (NoResString)"5AString"),
							 };

			filters.AddAttributeFilters(attributes, (x, y) => new ZQuery(new ZQuery(DummyDependentBizoSchema.ZD1_Number, 1), x));
			AssertNull(filters[""]);
			AssertNull(filters["1Astring"]);
			AssertNull(filters["3Abool"]);
			AssertNull(filters["4Anum"]);
			AssertNull(filters["5Astring"]);

			AssertEquals(((ModuleTextFilter)filters["1Bstring"]).MaxLength, ModuleFilter.MaxMaximumLength);
			AssertEquals(((ModuleTextFilter)filters["5Bstring"]).MaxLength, DummyBizoSchema.Z0_Description.MaxLength);

			((ModuleTextFilter)filters["1Bstring"]).Property = "text1";
			((ModuleDateFilter)filters["2Adate"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filters["2Adate"]).Property1 = new ZDateTime(2000, 6, 5);
			((ModuleDateFilter)filters["2Adate"]).Property2 = new ZDateTime(2010, 6, 5);
			((ModuleFlagsFilter)filters["3Bbool"]).Property0 = true;
			((ModuleNumberRangeFilter)filters["4Bnum"]).Property1 = 0.1;
			((ModuleNumberRangeFilter)filters["4Bnum"]).Property2 = 1.1;
			((ModuleTextFilter)filters["5Bstring"]).Property = "text2";
			((ModuleTextFilter)filters["Any Text Attribute"]).Property = "any text";

			var modules = new List<ModuleFilter> { filters["1Bstring"], filters["2Adate"], filters["3Bbool"], filters["4Bnum"], filters["5Bstring"] };
			var query = filters.GetFilterQuery(modules);
			AssertEquals("(ZD1_Number = 1 and Z0_VarCharMax like 'text1%') and (ZD1_Number = 1 and (Z0_Date >= #2000-06-05 00:00:00.000# and Z0_Date < #2010-06-06 00:00:00.000#)) and (ZD1_Number = 1 and Z0_Bool = 1) and (ZD1_Number = 1 and (Z0_Money >= 0.1 and Z0_Money <= 1.1)) and (ZD1_Number = 1 and Z0_Description like 'text2%')", query.LiteralTextADO);

			query = filters.GetFilterQuery(new List<ModuleFilter> { filters["Any Text Attribute"] });
			AssertEquals("ZD1_Number = 1 and Z0_VarCharMax like 'any text%' or (ZD1_Number = 1 and Z0_Description like 'any text%')", query.LiteralTextADO);

			filters = new ModuleFilterCollection();
			filters.AddAttributeFilters(attributes, (x, y) => new ZQuery(new ZQuery(DummyDependentBizoSchema.ZD1_Number, 1), x), FilterCategories.GetOrCreateFilterCategory((NoResString)"Test Attribute Search"));
			((ModuleTextFilter)filters["Any Test Text Attribute"]).Property = "any test text";
			query = filters.GetFilterQuery(new List<ModuleFilter> { filters["Any Test Text Attribute"] });
			AssertEquals("ZD1_Number = 1 and Z0_VarCharMax like 'any test text%' or (ZD1_Number = 1 and Z0_Description like 'any test text%')", query.LiteralTextADO);
		}

		public void TestAddFlagsFilter()
		{
			var filters = new ModuleFilterCollection();
			filters.AddFlagsFilter("andFlags",
				new string[] { "Flag1", "Flag2" },
				new GetFlagsQuery[] { delegate { return new ZQuery(DummyBizoSchema.Z0_Bool, true); }, delegate { return new ZQuery(DummyBizoSchema.Z0_Bool, false); } });

			filters.AddFlagsFilter("orFlags",
				new string[] { "Flag1", "Flag2" },
				new GetFlagsQuery[] { delegate { return new ZQuery(DummyBizoSchema.Z0_Bool, true); }, delegate { return new ZQuery(DummyBizoSchema.Z0_Bool, false); } },
				JoinCondition.Or);

			((ModuleFlagsFilter)filters["andFlags"])["Flag1"] = true;
			((ModuleFlagsFilter)filters["andFlags"])["Flag2"] = true;
			((ModuleFlagsFilter)filters["orFlags"])["Flag1"] = true;
			((ModuleFlagsFilter)filters["orFlags"])["Flag2"] = true;
			var query = filters.GetFilterQuery(new ModuleFilter[] { filters["andFlags"], filters["orFlags"] });
			var expectedQuery = new ZQuery(
				new ZQuery(new ZQuery(DummyBizoSchema.Z0_Bool, true), new ZQuery(DummyBizoSchema.Z0_Bool, false)),
				new ZQuery(new ZQuery(DummyBizoSchema.Z0_Bool, true), JoinCondition.Or, new ZQuery(DummyBizoSchema.Z0_Bool, false))
				);
			AssertEquals(expectedQuery.LiteralTextADO, query.LiteralTextADO);
		}

		[ExpectNoExceptions]
		public void TestAnyTextAttributeFilter()
		{
			var attributes = new[]
			{
				new CustomAttribute("Code", DummyBizoSchema.Z0_Code, (NoResString)"Code"),
				new CustomAttribute("Description", DummyBizoSchema.Z0_Description, (NoResString)"Description")
			};
			AssertAnyTextAttributeFilter(attributes, (x, y) => x, null);
			AssertAnyTextAttributeFilter(attributes, null, new SubDummyProcessor());
		}

		void AssertAnyTextAttributeFilter(CustomAttribute[] attributes, ModuleFilterCollection.AttributeFilterQuery getFilter, ModuleFilterSubGroup subGroup)
		{
			var filters = new ModuleFilterCollection();
			if (getFilter != null)
			{
				filters.AddAttributeFilters(attributes, getFilter);
			}
			else
			{
				filters.AddAttributeFilters(attributes, FilterCategories.AttributeSearch, subGroup);
			}

			var anyTextAttributeFilter = (ModuleTextFilter)filters["Any Text Attribute"];
			AssertNotNull(anyTextAttributeFilter);
			AssertEquals(DummyBizoSchema.Z0_Description.MaxLength, anyTextAttributeFilter.MaxLength);

			anyTextAttributeFilter.Property = "Blablablabla";
			AssertNotEquals("Z0_Code like 'Blablablabla%' \r\nOR\r\nZ0_Description like 'Blablablabla%'\r\n", anyTextAttributeFilter.Query.LiteralTextSqlFormatted);
			AssertEquals(@"(
	Z0_Description like 'Blablablabla%' 
	AND
	Z0_Description >= 'Blablablabla' 
	AND
	Z0_Description <= 'Blablablablþ'
)
"
				, anyTextAttributeFilter.Query.LiteralTextSqlFormatted);
		}

		#endregion

		#region TestAddNumberFilterWithoutIsBlankAndIsNotBlank

		public void TestAddNumberFilterWithoutIsBlankAndIsNotBlank()
		{
			var filters = new ModuleFilterCollection();
			var filter1 = filters.AddNumberFilterWithoutIsBlankAndIsNotBlank("TEST1", DummyBizoSchema.Z0_VarCharMax);
			var filter2 = filters.AddNumberFilterWithoutIsBlankAndIsNotBlank("TEST2", (a, b) => new ZQuery());

			Assert("Should not contain 'Is Blank' comparison operator.", !filter1.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank));
			Assert("Should not contain 'Is Blank' comparison operator.", !filter2.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank));
			Assert("Should not contain 'Is Not Blank' comparison operator.", !filter1.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));
			Assert("Should not contain 'Is Not Blank' comparison operator.", !filter2.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));
		}

		#endregion

		#region TestAddDateOffsetFilter

		public void TestAddDateOffsetFilter()
		{
			var filters = new ModuleFilterCollection();
			var filter1 = filters.AddDateFilter("Boat", DummyBizoSchema.Z0_DateTimeOffset);
			var filter2 = filters.AddDateFilter("Love", (DateComparisonOperator a, ZDateTimeOffset b, ZDateTimeOffset c) => new ZQuery(), isNullable: true);
			var filter3 = filters.AddDateFilter("SNOW", DummyBizoSchema.Z0_DateTimeOffset, true);
			var filter4 = filters.AddDateFilter("Song", (DateComparisonOperator a, ZDateTimeOffset b, ZDateTimeOffset c) => new ZQuery(), true, isNullable: true);

			AssertZDateColumnFilter(filter1, DummyBizoSchema.Z0_DateTimeOffset, false);
			AssertZDateColumnFilter(filter3, DummyBizoSchema.Z0_DateTimeOffset, true);

			Assert(filters.Any(f => f.Code == "Boat"));
			Assert(filters.Any(f => f.Code == "Love"));
			Assert(filters.Any(f => f.Code == "SNOW"));
			Assert(filters.Any(f => f.Code == "Song"));
		}

		void AssertZDateColumnFilter(ModuleDateTimeOffsetFilter filter1, SchemaDateTimeOffsetColumn column, bool shouldConvert)
		{
			AssertEquals(false, filter1.IsNull);
			AssertEquals(column, filter1.FilterColumn);
			AssertEquals(shouldConvert, filter1.ConvertFromLocalToUTC);
		}

		#endregion

		#region TestNonWebPublishedFilterAreNotInList

		public void TestNonWebPublishedFilterAreNotInListWhenOnWeb()
		{
			var isWeb = Globals.IsWeb;
			try
			{
				var filters1 = new ModuleFilterCollection();
				var filterOnWeb = filters1.AddTextFilter("Text", DummyBizoSchema.Z0_VarCharMax);
				var filterNotOnWeb = filters1.AddTextFilter("Description", DummyBizoSchema.Z0_Description);
				filterNotOnWeb.IsPublishedOnWeb = false;

				Globals.IsWeb = true;
				AssertCollectionContains(filterOnWeb, filters1.Filter_List);
				AssertCollectionNotContains(filterNotOnWeb, filters1.Filter_List);

				var filters2 = new ModuleFilterCollection();
				filters2.AddFilter(filterOnWeb);
				filters2.AddFilter(filterNotOnWeb);

				Globals.IsWeb = false;
				AssertCollectionContains(filterOnWeb, filters2.Filter_List);
				AssertCollectionContains(filterNotOnWeb, filters2.Filter_List);
			}
			finally
			{
				Globals.IsWeb = isWeb;
			}
		}

		#endregion

		#region TestFilterList_Visible

		public void TestFilterList_Visible()
		{
			var filters = new ModuleFilterCollection();
			var filter1 = filters.AddTextFilter("Text", DummyBizoSchema.Z0_VarCharMax);
			var filter2 = filters.AddTextFilter("Description", DummyBizoSchema.Z0_Description);

			AssertCollectionContains(filter1, filters.Filter_List);
			AssertCollectionContains(filter2, filters.Filter_List);

			filter2.Visible = false;
			filters.ResetFilterList();

			AssertCollectionContains(filter1, filters.Filter_List);
			AssertCollectionNotContains(filter2, filters.Filter_List);
		}

		#endregion

		#region TestIndexerGetsDuplicateFilters

		public void TestIndexerGetsDuplicateFilters()
		{
			var filter = Collection.AddTextFilter("Description", DummyBizoSchema.Z0_Description);
			AssertEquals(filter, Collection["Description"]);
			AssertEquals(filter, Collection.GetVisibleModuleFilterAndDuplicateAndDeactivateIfActive("Description"));

			filter.IsActive = true;
			var duplicateFilter1 = Collection.GetVisibleModuleFilterAndDuplicateAndDeactivateIfActive("Description");
			AssertEquals(duplicateFilter1, Collection["Description (1)"]);

			var duplicateFilter2 = Collection.GetVisibleModuleFilterAndDuplicateAndDeactivateIfActive("Description");
			AssertEquals(duplicateFilter2, Collection["Description (2)"]);
		}

		#endregion

		#region TestEnumerationEnumeratesDuplicateFilters

		public void TestEnumerationEnumeratesDuplicateFilters()
		{
			var filter = Collection.AddTextFilter("Description", DummyBizoSchema.Z0_Description);
			filter.IsActive = true;
			var duplicateFilter = Collection.GetVisibleModuleFilterAndDuplicateAndDeactivateIfActive("Description");

			var enumerationCount = 0;
			var filterFound = false;
			var duplicateFilterFound = false;

			foreach (var enumeratedFilter in Collection)
			{
				enumerationCount++;
				if (enumeratedFilter == filter)
				{
					filterFound = true;
				}

				if (enumeratedFilter == duplicateFilter)
				{
					duplicateFilterFound = true;
				}
			}

			AssertEquals(2, enumerationCount);
			AssertEquals(true, filterFound);
			AssertEquals(true, duplicateFilterFound);
		}

		#endregion

		#region TestGetFilterQuery

		public void TestGetFilterQuery()
		{
			#region Setup

			var collection = new ModuleFilterCollection();

			var filter1 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			var filter2 = new ModuleTextFilter("Description", DummyBizoSchema.Z0_Description);
			var filter3 = new ModuleTextFilter("Text", DummyBizoSchema.Z0_VarCharMax);
			var filter4 = new ModuleTextFilter("Another Text", DummyBizoSchema.Z0_VarCharMax);

			var modules = new List<ModuleFilter>();
			modules.Add(filter1);
			modules.Add(filter2);
			modules.Add(filter3);
			modules.Add(filter4);

			filter1.Property = "Code";
			filter2.Property = "Description 1";
			filter3.Property = "Text 1";
			filter4.Property = "Text 2";

			filter3.OrCategory = FilterOrCategory.Blue;
			filter4.OrCategory = FilterOrCategory.Blue;

			#endregion

			var query = collection.GetFilterQuery(modules);
			AssertEquals("Z0_Code like 'Code%' and Z0_Description like 'Description 1%' and (Z0_VarCharMax like 'Text 1%' or Z0_VarCharMax like 'Text 2%')", query.LiteralTextADO);
		}

		#endregion

		#region TestGetFilterGrouping

		class SubDummyProcessor : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Guid);
				subQuery.AddToFilter(filter);

				var parentQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
				parentQuery.AddSubQuery(DummyBizoSchema.Z0_Guid, DummyBizoSchema.PK, subQuery, JoinCondition.And);

				return parentQuery;
			}
		}

		public void TestGetFilterGrouping()
		{
			var processor = new SubDummyProcessor();
			var collection = new ModuleFilterCollection();

			var filter1 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter1.SubGroup = null;
			filter1.Property = "VAL1";
			filter1.OrCategory = FilterOrCategory.Blue;

			var filter2 = new ModuleTextFilter("Sub Code", DummyBizoSchema.Z0_Code);
			filter2.SubGroup = processor;
			filter2.Property = "SUB1";
			filter2.OrCategory = FilterOrCategory.Blue;

			var filter3 = new ModuleTextFilter("Sub Code", DummyBizoSchema.Z0_Code);
			filter3.SubGroup = processor;
			filter3.Property = "SUB2";
			filter3.OrCategory = FilterOrCategory.Blue;

			var filter4 = new ModuleTextFilter("Other Sub Code", DummyBizoSchema.Z0_FK_Code);
			filter4.SubGroup = processor;
			filter4.Property = "OSUB";
			filter4.OrCategory = FilterOrCategory.None;

			var filter5 = new ModuleTextFilter("Description", DummyBizoSchema.Z0_Description);
			filter5.SubGroup = null;
			filter5.Property = "Blaticus";
			filter5.OrCategory = FilterOrCategory.None;

			var resultQuery = collection.GetFilterQuery(new ModuleFilter[]
			{
				filter1,
				filter2,
				filter3,
				filter4,
				filter5,
			});

			const string expectedQuery =
@"(
	(
		Z0_Description like 'Blaticus%' 
		AND
		Z0_Description >= 'Blaticus' 
		AND
		Z0_Description <= 'Blaticuþ'
	)
	AND
	(
		(
			Z0_Code like 'VAL1%' 
			AND
			Z0_Code >= 'VAL1' 
			AND
			Z0_Code <= 'VALþ'
		)
		OR
		(
			Z0_Guid IN 
			(
				SELECT Z0_PK FROM dbo.DummyBizo WHERE 
				(
					Z0_Code like 'SUB1%' 
					AND
					Z0_Code >= 'SUB1' 
					AND
					Z0_Code <= 'SUBþ'
				)
				OR
				(
					Z0_Code like 'SUB2%' 
					AND
					Z0_Code >= 'SUB2' 
					AND
					Z0_Code <= 'SUBþ'
				)
			)
		)
	)
)
AND
Z0_Guid IN 
(
	SELECT Z0_PK FROM dbo.DummyBizo WHERE 
	(
		Z0_FK_Code like 'OSUB%' 
		AND
		Z0_FK_Code >= 'OSUB' 
		AND
		Z0_FK_Code <= 'OSUþ'
	)
)
";

			AssertMultilineASCIIEquals("", expectedQuery, resultQuery.LiteralTextSqlFormatted);
		}

		#endregion

		#region TestToSortedArrayWithIsExclusiveLast

		public void TestToSortedArrayWithIsExclusiveLast()
		{
			var collection = new ModuleFilterCollection();
			collection.AddTextFilter("Z0_VarCharMax", DummyBizoSchema.Z0_VarCharMax);
			collection.AddTextFilter("Z0_Code", DummyBizoSchema.Z0_Code).IsExclusiveHelper = true;
			collection.AddTextFilter("Z0_Description", DummyBizoSchema.Z0_Description);

			var sortedWithExclusiveLast = collection.ToSortedArrayWithIsExclusiveLast();
			AssertEquals("Z0_VarCharMax", sortedWithExclusiveLast[0].Description);
			AssertEquals("Z0_Description", sortedWithExclusiveLast[1].Description);
			AssertEquals("Z0_Code", sortedWithExclusiveLast[2].Description);
		}

		#endregion

		#region TestAddTextFilterForExactComparison

		public void TestAddTextFilterForExactComparison()
		{
			var textFilter = Collection.AddTextFilterForExactComparison("Description", (o, v) => { return new ZQuery(); });
			AssertEquals(1, textFilter.ComparisonOperator_List.Count);
			AssertEquals(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact, textFilter.ComparisonOperator_List[0].Code);
		}

		#endregion

		#region TestAddFiltersForTranslatableText

		public void TestAddFiltersForTranslatableText()
		{
			var collection = GetNewModuleFilterCollection();
			collection.AddFiltersForTranslatableText("Description", DummyBizoSchema.Z0_Description, typeof(TranslatableDataFieldTestCase.DummyWithTranslatable), (NoResString)"Description");
			AssertEquals(1, collection.Count());
			AssertType(typeof(ModuleTextFilter), collection["Description"]);
			AssertEquals("Description", collection["Description"].MultilingualDescription);
			AssertEquals(DummyBizoSchema.Z0_Description, collection["Description"].FilterColumn);

			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			using (Res.UseMockData())
			{
				collection = GetNewModuleFilterCollection();
				collection.AddFiltersForTranslatableText("Description", DummyBizoSchema.Z0_Description, typeof(TranslatableDataFieldTestCase.DummyWithTranslatable), (NoResString)"Description");
				AssertEquals(2, collection.Count());
				AssertType(typeof(ModuleTextFilter), collection["Description"]);
				AssertEquals("Description (English)", collection["Description"].MultilingualDescription);
				AssertEquals(DummyBizoSchema.Z0_Description, collection["Description"].FilterColumn);

				AssertType(typeof(ModuleTranslatableTextFilter), collection["Description_Local"]);
				AssertEquals("Description (Chinese - Simplified)", collection["Description_Local"].MultilingualDescription);
				AssertEquals(DummyBizoSchema.Z0_Description, collection["Description_Local"].FilterColumn);
				AssertStartsWith("", DummyBizoSchema.Z0_Description.Name, ((ModuleTranslatableTextFilter)collection["Description_Local"]).Customizable.Source.GetKey(null, "Test"));
			}
		}

		#endregion

		#region TestAddTranslatableTextFilter

		public void TestAddTranslatableTextFilter()
		{
			var collection = GetNewModuleFilterCollection();
			collection.AddTranslatableTextFilter("Description", (o, v) => { return new ZQuery(); }, (NoResString)"DescriptionMultiLangualString");
			AssertEquals(1, collection.Count());
			AssertType(typeof(ModuleTextFilter), collection["Description"]);
			AssertEquals("DescriptionMultiLangualString", collection["Description"].MultilingualDescription);
		}

		#endregion

		#region TestAddTranslatableNumberRangeFilter

		public void TestAddTranslatableNumberRangeFilter()
		{
			var collection = GetNewModuleFilterCollection();
			collection.AddTranslatableNumberRangeFilter("Description", (o, v) => { return new ZQuery(); }, (NoResString)"DescriptionMultiLangualString");
			AssertEquals(1, collection.Count());
			AssertType(typeof(ModuleNumberRangeFilter), collection["Description"]);
			AssertEquals("DescriptionMultiLangualString", collection["Description"].MultilingualDescription);
		}

		#endregion

		#region Query Caching

		public void TestAddAndRemoveFilterStrip_ShouldInvalidateCachedQuery()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = (DummyFilterBusinessObject)module.FilterBusinessObject;
				var collection = (DummyModuleFilterCollection)filterBizo.ModuleFilters;
				AssertNull(collection.CachedQuery_ForTest);

				collection.CachedQuery_ForTest = new ZQuery();
				AssertNotNull(collection.CachedQuery_ForTest);
				var filter = filterBizo["Z0_Description"];
				filter.IsActive = true;
				AssertNull(collection.CachedQuery_ForTest);

				collection.CachedQuery_ForTest = new ZQuery();
				AssertNotNull(collection.CachedQuery_ForTest);
				filter.IsActive = false;
				AssertNull(collection.CachedQuery_ForTest);
			}
		}

		public void TestFilterCaching()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = (DummyFilterBusinessObject)module.FilterBusinessObject;
				var collection = (DummyModuleFilterCollection)filterBizo.ModuleFilters;
				AssertEquals(0, collection.GetFilterQueryCoreCount);

				var filter1 = filterBizo.Filter;
				AssertEquals(1, collection.GetFilterQueryCoreCount);

				var filter2 = filterBizo.Filter;
				AssertEquals("No changes have been made since the last call to Filter, so a cached query should have been used, and yet...", 1, collection.GetFilterQueryCoreCount);
				AssertEquals(filter1.LiteralTextSqlFormatted, filter2.LiteralTextSqlFormatted);

				var strip1 = filterBizo.FilterStrips.AddNew("Z0_Description");
				var filter3 = filterBizo.Filter;
				AssertEquals("A filter strip was added, so the query should have been reevaluated, and yet...", 2, collection.GetFilterQueryCoreCount);

				var filter4 = filterBizo.Filter;
				AssertEquals(2, collection.GetFilterQueryCoreCount);

				var moduleFilter = (ModuleTextFilter)filterBizo["Z0_Code"];
				var filter5 = filterBizo.Filter;
				AssertEquals("We got a reference to a module filter but didn't set it to be active, so it wouldn't have added a filter strip, and shouldn't cause a re-evaluation of the query, and yet...", 2, collection.GetFilterQueryCoreCount);

				moduleFilter.IsActive = true;
				var filter6 = filterBizo.Filter;
				AssertEquals("A new filter strip was added using the module filter IsActive method, which should cause the query to be re-evaluated, and yet...", 3, collection.GetFilterQueryCoreCount);

				var filter7 = filterBizo.Filter;
				AssertEquals(3, collection.GetFilterQueryCoreCount);

				moduleFilter.Property = "A";
				var filter8 = filterBizo.Filter;
				AssertEquals("One of the filters has changes, so the query should be re-evaluated, and yet...", 4, collection.GetFilterQueryCoreCount);
				AssertNotEquals(filter7.LiteralTextSqlFormatted, filter8.LiteralTextSqlFormatted);

				var filter9 = filterBizo.Filter;
				AssertEquals(4, collection.GetFilterQueryCoreCount);
				AssertEquals(filter8.LiteralTextSqlFormatted, filter9.LiteralTextSqlFormatted);

				moduleFilter.IsActive = false;
				var filter10 = filterBizo.Filter;
				AssertEquals("The filter strip was removed by setting its module filter to inactive, so the query should have been re-evaluated, and yet...", 5, collection.GetFilterQueryCoreCount);
				AssertNotEquals(filter9.LiteralTextSqlFormatted, filter10.LiteralTextSqlFormatted);

				collection.SupportsQueryCaching = false;
				var filter11 = filterBizo.Filter;
				AssertEquals("Filter caching was disabled, so the query should have been reevaluated even though nothing else has changed, and yet...", 6, collection.GetFilterQueryCoreCount);
				AssertEquals(filter10.LiteralTextSqlFormatted, filter11.LiteralTextSqlFormatted);
			}
		}

		[TestDate(2017, 9, 1)]
		public void TestFilterQueryProperty_AfterCollectionQueryCached_ShouldCauseCollectionQueryToRequireReevaluation()
		{
			var filterBizo = new DummyFilterBusinessObject();
			var filter = filterBizo.AddTextFilterStrip("Z0_Description", "Squanch");
			var fullQuery1 = filterBizo.Filter;

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			filter.Property = "Schwifty";
			var filterQuery = filter.Query; // caches the filter's query
			var fullQuery2 = filterBizo.Filter;

			AssertNotEquals("The individual filter was changed, so the cached query on the ModuleFilterCollection should be considered invalid (and not valid since the individual filter cached a query as well), and yet...", fullQuery1.LiteralTextSqlFormatted, fullQuery2.LiteralTextSqlFormatted);
		}

		public void TestGetFilterQuery_ShouldReturnCloneOfCachedQuery()
		{
			var filterBizo = new DummyFilterBusinessObject();
			var query1 = filterBizo.ModuleFilters.GetFilterQuery(filterBizo.ActiveModuleFiltersForQuery);
			var query2 = filterBizo.ModuleFilters.GetFilterQuery(filterBizo.ActiveModuleFiltersForQuery);

			AssertEquals(false, ReferenceEquals(query1, query2));
		}

		#endregion

		#region AddFilter

		public void TestAddFilter_ShouldFireFilterAddedEvent()
		{
			ModuleFilter filterWhichWasAdded = null;
			var collection = new ModuleFilterCollection();
			collection.FilterAdded += (sender, args) => filterWhichWasAdded = args.AddedFilter;

			var filter = collection.AddTextFilter("Squanch", DummyBizoSchema.Z0_Description);
			AssertEquals(filter, filterWhichWasAdded);
		}

		#endregion

		#region RemoveFilter

		public void TestRemoveFilter()
		{
			var collection = new ModuleFilterCollection();
			var filter1 = collection.AddTextFilter("Description", DummyBizoSchema.Z0_Description);
			var filter2 = collection.AddTextFilter("Key", DummyBizoSchema.Z0_FK_Code);

			AssertEquals(2, collection.Count());
			collection.RemoveFilter(filter1);
			AssertEquals(1, collection.Count());
			AssertEquals(filter2, collection["Key"]);

			collection.RemoveFilter(filter2);
			AssertEquals(0, collection.Count());
		}

		#endregion

		#region Implementation

		protected ModuleFilterCollection Collection
		{
			get { return fCollection ?? (fCollection = GetNewModuleFilterCollection()); }
		}
		ModuleFilterCollection fCollection;

		protected virtual ModuleFilterCollection GetNewModuleFilterCollection()
		{
			return new ModuleFilterCollection();
		}

		#endregion
	}
}
