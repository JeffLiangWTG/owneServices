using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class RelatedModuleFiltersHelperGuiTest : TestCaseWithFactory
	{
		public void TestHasFilterStrips()
		{
			var filterBizo = (FilterStripBusinessObject)ObjectFactory.Get("ProcessTasks_FilterStripBusinessObject");

			var filter = filterBizo.SaveLayout("new layout");

			AssertEquals(false, RelatedModuleFiltersHelper.HasFilterStrips(filter, ModuleIDs.ProcessTasks));

			var strip = filterBizo.FilterStrips.AddNew("Description");
			((ModuleTextFilter)strip.CurrentModuleFilter).Property = "Whatever";
			filter = filterBizo.SaveLayout("new layout");

			AssertEquals(true, RelatedModuleFiltersHelper.HasFilterStrips(filter, ModuleIDs.ProcessTasks));
		}

		public void TestGetNewFilterBusinessObjectWithOverridenModuleId_ShouldNotSaveLayout_WhenSettingModuleType()
		{
			using (Db.Connection.TrackExecutedCommands())
			{
				var filterBizo = RelatedModuleFiltersHelper.GetNewFilterBusinessObjectWithOverridenModuleId(DummyModuleIDs.Dummy, DummyModuleIDs.DummyThatHitsFilterBizoOnDispose);
				AssertNotNull(filterBizo);
				AssertEquals("There should be no references to StmModuleFilter in the list of commands, since this would imply that the module's layout is being saved unnecessarily",
					0, Db.Connection.ExecutedCommands.Count(c => c.Contains(StmModuleFilterSchema.Constants.TableName)));
			}
		}

		ZDBOnlyQuery GetParameterizedFilterQuery(DummyFilterBusinessObject dummyBizO, StmModuleFilter layout)
		{
			var parameters = new ZSqlParameterCollection();
			var sql = RelatedModuleFiltersHelper.GetFilterQueryParameterized(layout, parameters);

			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT {0} FROM dbo.{1} WHERE {2}",
				"Z0_Description",
				DummyBizoSchema.Constants.TableName,
				sql);

			var query = new ZDBOnlyQuery(dummyBizO.QueryObjectType);
			query.AddFilterAndZSQLParameterCollection(sqlText, parameters, JoinCondition.And);

			return query;
		}

		public void TestParameterizedFilterQuery()
		{
			var dummyBizO = new DummyFilterBusinessObject();

			var subStrip = dummyBizO.FilterStrips.AddNew("Z0_Description");
			var subFilter = (ModuleTextFilter)subStrip.CurrentModuleFilter;
			subFilter.IsActive = true;
			subFilter.Property = "Ooh eee ooh ah ah, ting tang walla walla bing bang";
			subFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

			var subStrip2 = dummyBizO.FilterStrips.AddNew("Z0_Code");
			var subFilter2 = (ModuleTextFilter)subStrip2.CurrentModuleFilter;
			subFilter2.IsActive = true;
			subFilter2.Property = "OOH";
			subFilter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

			AssertEquals("Precondition: subfilter dummy business object must contain 2 filters before adding to the parameterization query", 2, dummyBizO.ActiveModuleFilters.Count);

			var layout = Factory.New<StmModuleFilter>();
			var layoutPKString = layout.PK.ToString().Replace("-", string.Empty);
			dummyBizO.FillLayoutValues(layout, DummyModuleIDs.Dummy);

			var query = GetParameterizedFilterQuery(dummyBizO, layout);

			var expectedLiteral = "SELECT Z0_Description FROM dbo.DummyBizo WHERE Z0_Description = 'Ooh eee ooh ah ah, ting tang walla walla bing bang' and Z0_Code = 'OOH'";
			var expectedParameterized = $"SELECT Z0_Description FROM dbo.DummyBizo WHERE Z0_Description = @{layoutPKString}1 and Z0_Code = @{layoutPKString}0";

			AssertContains(expectedLiteral, query.LiteralTextADO);
			AssertEquals("Expect the query to have parameterised text", expectedParameterized, query.ParameterisedText.ParameterisedQueryText);
			AssertEquals("Parameter value equals subFilter property", subFilter.Property, query.ParameterisedText.Parameters[1].ValueForSql);

			var previousQueryText = query.ParameterisedText.ParameterisedQueryText;

			query = GetParameterizedFilterQuery(dummyBizO, layout);
			AssertEquals("The parameterised query generated on this run should be the same as the previous run", previousQueryText, query.ParameterisedText.ParameterisedQueryText);
		}
	}
}
