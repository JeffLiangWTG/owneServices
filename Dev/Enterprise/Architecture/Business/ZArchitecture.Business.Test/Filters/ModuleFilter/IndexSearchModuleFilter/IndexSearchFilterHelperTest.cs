using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.Testing;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(IndexSearchFilterHelper))]
	sealed class IndexSearchFilterHelperTest : TestCase
	{
		public void TestGetIndexSearchModuleFilterFromSearchField()
		{
			var field = new SearchField("moo", "Des", typeof(string));
			var filter = IndexSearchFilterHelper.GetIndexSearchModuleFilterFromSearchField(field)[0];
			Assert(filter is IndexSearchModuleTextFilter);

			field = new SearchField("moo", "Des", typeof(int));
			filter = IndexSearchFilterHelper.GetIndexSearchModuleFilterFromSearchField(field)[0];
			Assert(filter is IndexSearchModuleNumberRangeFilter);

			field = new SearchField("moo", "Des", typeof(bool));
			filter = IndexSearchFilterHelper.GetIndexSearchModuleFilterFromSearchField(field)[0];
			Assert(filter is IndexSearchModuleFlagsFilter);
		}

		public void TestGetIndexSearchModuleFilterFromSearchField_AuditField()
		{
			var createTimeField = new SearchField("CREATETIME", "Des", typeof(string));
			AssertEquals("CreateTime should belong to Audit Information category",
				FilterCategories.AuditInformation,
				IndexSearchFilterHelper.GetIndexSearchModuleFilterFromSearchField(createTimeField)[0].Category);

			var createUserField = new SearchField("CREATEUSER", "Des", typeof(string));
			AssertEquals("CreateUser should belong to Audit Information category",
				FilterCategories.AuditInformation,
				IndexSearchFilterHelper.GetIndexSearchModuleFilterFromSearchField(createUserField)[0].Category);

			var lastEditTimeField = new SearchField("LASTEDITTIME", "Des", typeof(string));
			AssertEquals("LastEditTime should belong to Audit Information category",
				FilterCategories.AuditInformation,
				IndexSearchFilterHelper.GetIndexSearchModuleFilterFromSearchField(lastEditTimeField)[0].Category);

			var lastEditUserField = new SearchField("LASTEDITUSER", "Des", typeof(string));
			AssertEquals("LastEditUser should belong to Audit Information category",
				FilterCategories.AuditInformation,
				IndexSearchFilterHelper.GetIndexSearchModuleFilterFromSearchField(lastEditUserField)[0].Category);
		}

		public void TestGetIndexSearchModuleFilterFromSearchField_SearchFieldWithRuleLookup()
		{
			var ruleLookup = new SearchFieldRuleLookup(Guid.NewGuid());
			var field = new SearchField("moo", "Des", typeof(string), false, false, 0, ruleLookup: ruleLookup);
			var filter = IndexSearchFilterHelper.GetIndexSearchModuleFilterFromSearchField(field)[0];
			Assert(filter is IndexSearchModuleTextFilter);
			Assert(filter.QueryDelegate != null);
		}

		public void TestGetIndexSearchModuleFilterFromSearchField_SearchFieldWithEntityLookup()
		{
			var factory = new BusinessObjectFactory();
			var entityLookup = new SearchFieldEntityLookup("IDummy", "IDummy", "GlbStaff", "Code", "IsActive");
			var field = new SearchField("moo", "Des", typeof(Guid), false, false, 0, entityLookup);
			var filter = IndexSearchFilterHelper.GetIndexSearchModuleFilterFromSearchField(field, factory)[0];
			Assert(filter is IndexSearchModuleGuidFilter);
		}

		public void TestGetIndexSearchModuleFilterFromSearchField_OverideFilter()
		{
			var factory = new BusinessObjectFactory();
			var field = new SearchField("FLAG", "Des", typeof(string));
			var filterExpected = new IndexSearchModuleFlagsFilter(field, FilterCategories.StatusAndFlags);
			var fieldOverride = new SearchFieldOverride(field, filterExpected);

			var filter = IndexSearchFilterHelper.GetIndexSearchModuleFilterFromSearchField(fieldOverride, factory)[0];

			AssertEquals(filterExpected, filter);
		}

		public void TestGetIndexSearchModuleFilterFromSearchField_OverideCategory()
		{
			var factory = new BusinessObjectFactory();
			var field = new SearchField("FLAG", "Des", typeof(string));
			var fieldOverride = new SearchFieldOverride(field, FilterCategories.Locations);

			var filter = IndexSearchFilterHelper.GetIndexSearchModuleFilterFromSearchField(fieldOverride, factory)[0];

			AssertEquals(typeof(IndexSearchModuleTextFilter), filter.GetType());
			AssertEquals(FilterCategories.Locations, filter.Category);
		}

		public void TestShouldReportErrorWhenGuidFilterCannotBeCreated()
		{
			var factory = new BusinessObjectFactory();
			var entityLookup = new SearchFieldEntityLookup("IDummy", "IDummy", "NotExistsTableName", "Code", "IsActive");
			var field = new SearchField("moo", "Des", typeof(Guid), false, false, 0, entityLookup);
			var filters = IndexSearchFilterHelper.GetIndexSearchModuleFilterFromSearchField(field, factory);
			Assert(filters.IsNullOrEmpty());
			Assert("Should report: Failed to create index filter with lookup", ErrorReporter.LastMessageReported.Contains("Search Field Name: moo, tableName: NotExistsTableName, Description: Des"));
			ErrorReporter.Clear();
		}

		public void TestGetQueryResult()
		{
			using (new GlowIndexQueryEngineMock())
			{
				var queries = new List<IGlowQuery>() { new EmptyQuery() };
				var resultCollection = IndexSearchFilterHelper.GetQueryResult(queries, "IDummyBusinessObject");
				var query = IndexSearchFilterHelper.ConvertResultToZQuery(resultCollection, "IDummyBusinessObject");
				AssertNotNull(query);
				AssertContains(ZGuid.BrettsGuid.ToString(), query.LiteralTextADO, true);
			}
		}

		public void TestConvertResultToZQuery()
		{
			var resultCollection = new GlowIndexQueryEngineMock().Results;
			var query = IndexSearchFilterHelper.ConvertResultToZQuery(resultCollection, "IDummyBusinessObject");

			AssertNotNull(query);
			AssertContains(ZGuid.BrettsGuid.ToString(), query.LiteralTextADO, true);
		}

		public void TestGetSearchFields()
		{
			using (new GlowIndexQueryEngineMock())
			{
				var collection = IndexSearchFilterHelper.GetSearchFields(DummyModuleIDs.Dummy);
				AssertEquals(collection.Status, GlowIndexQueryStatus.Success);
				AssertEquals(2, collection.Value.Length);
				AssertEquals("CODE", collection.Value[0].FieldName);
				AssertEquals("NAME", collection.Value[1].FieldName);
			}
		}

		public void TestGetIndexSearchModuleFilterFromSearchField_SearchFieldWithNkLookup()
		{
			var factory = new BusinessObjectFactory();
			var entityLookup = new SearchFieldEntityLookup("IDummy", "IDummy", "GlbStaff", "Code", "IsActive");
			var field = new SearchField("moo", "Des", typeof(string), false, false, 0, entityLookup);
			var filter = IndexSearchFilterHelper.GetIndexSearchModuleFilterFromSearchField(field, factory)[0];
			Assert(filter is IndexSearchModuleNKFilter);
		}

		public void TestLookupFromGlowCanGetCorrectModuleId()
		{
			var factory = new BusinessObjectFactory();
			var entityLookup = new SearchFieldEntityLookup("IDummy", "IDummy", "RefServiceLevel", "Code", "IsActive");
			var field = new SearchField("moo", "Des", typeof(string), false, false, 0, entityLookup);
			var filter = IndexSearchFilterHelper.GetIndexSearchModuleFilterFromSearchField(field, factory)[0];

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			Assert(!ErrorReporter.HasBeenReported("Failed to create index filter with lookup"));
			Assert(filter is IndexSearchModuleNKFilter);
		}
	}
}
