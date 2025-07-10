using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.PAVE.MENT.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.GUI.Test
{
	[TestedType(typeof(SeriesFilterBusinessObject))]
	public class SeriesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestSeriesFilterBizo_TestStaffFilter()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "DNT";
			var system = Factory.NewWithValidTestData<BMSystem>();
			var component = Factory.NewWithValidTestData<BMComponent>();
			component.FC_FS_System = system.PK;
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "AAA";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "BBB";
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "ABC";
			var query = MENTTestHelper.CreateQuery(Factory, "CODY");

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 1, ZDateTime.BrettsBirthday, group.PK, component.PK, "1", staff1.GS_Code);
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.BrettsBirthday, group.PK, component.PK, "1", staff2.GS_Code);
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 3, ZDateTime.BrettsBirthday, group.PK, component.PK, "1", staff3.GS_Code);

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = (ModuleNkFilter)filterBizo["Staff Code"];
			filter.IsActive = true;
			filter.Property = staff1.GS_Code;
			AssertResults(filterBizo, 1);

			filter.Property = "BAD";
			AssertResults(filterBizo);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.AddTextFilterStrip("Code", "A");

			AssertResults(filterBizo, 1, 3);
		}

		public void TestSeriesFilterBizo_AllStaffFromReleaseGroupFilter()
		{
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ADO", "Adom");
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FAR", "Farness");

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.Staff.AddRange(staff1);
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.Staff.AddRange(staff1, staff2);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var releaseGroup1 = BMSTestHelper.CreateReleaseGroup(system, group1);
			var releaseGroup2 = BMSTestHelper.CreateReleaseGroup(system, group2);
			var query = MENTTestHelper.CreateQuery(Factory, "SYRUP");

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 1, ZDateTime.BrettsBirthday, group1.PK, buffer.PK, "1", staff1.GS_Code);
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.BrettsBirthday, group2.PK, buffer.PK, "1", staff1.GS_Code);
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 3, ZDateTime.BrettsBirthday, group2.PK, buffer.PK, "1", staff2.GS_Code);

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBizo["Staff Release Group"];
			filter.IsActive = true;

			filter.Property = group1.PK;
			AssertResults("Rows come from extactions for staff 1", filterBizo, 1, 2);

			filter.Property = group2.PK;
			AssertResults("All rows match because both staff are in the release group", filterBizo, 1, 2, 3);

			filter.Property = ZGuid.NewZGuid();
			AssertResults("No rows because we entered dummy data", filterBizo);
		}

		public void TestSeriesFilterBizo_TestReleaseGroupFilter()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "DNT";
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "AAA";
			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			group3.GG_Code = "ABC";
			var system = Factory.NewWithValidTestData<BMSystem>();
			var component = Factory.NewWithValidTestData<BMComponent>();
			component.FC_FS_System = system.PK;
			var staff = Factory.New<GlbStaff>();
			var query = MENTTestHelper.CreateQuery(Factory, "CODY");

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 1, ZDateTime.BrettsBirthday, group1.PK, component.PK, "1", staff.GS_Code);
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.BrettsBirthday, group2.PK, component.PK, "1", staff.GS_Code);
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 3, ZDateTime.BrettsBirthday, group3.PK, component.PK, "1", staff.GS_Code);

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBizo["Release Group"];
			filter.IsActive = true;
			filter.Property = group1.PK;

			AssertResults(filterBizo, 1);

			filter.Property = ZGuid.NewZGuid();
			AssertResults(filterBizo);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.AddTextFilterStrip("Code", "A");

			AssertResults(filterBizo, 2, 3);
		}

		public void TestSeriesFilterBizo_TestDateFilter()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "DNT";
			var system = Factory.NewWithValidTestData<BMSystem>();
			var component = Factory.NewWithValidTestData<BMComponent>();
			component.FC_FS_System = system.PK;
			var staff = Factory.New<GlbStaff>();
			var query = MENTTestHelper.CreateQuery(Factory, "CODY");

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 1, ZDateTime.Now.AddDays(-3), group.PK, component.PK, "1", staff.GS_Code);
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.Now.AddDays(-10), group.PK, component.PK, "1", staff.GS_Code);

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = (ModuleDateFilter)filterBizo["Time recorded"];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last7Days;
			AssertResults(filterBizo, 1);

			filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last14Days;
			AssertResults(filterBizo, 1, 2);

			filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Tomorrow;
			AssertResults(filterBizo);
		}

		public void TestSeriesFilterBizo_TestAttributeFilter()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "DNT";
			var system = Factory.NewWithValidTestData<BMSystem>();
			var component1 = Factory.NewWithValidTestData<BMComponent>();
			component1.FC_FS_System = system.PK;
			var staff = Factory.New<GlbStaff>();
			var query = MENTTestHelper.CreateQuery(Factory, "CODY");

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 1, ZDateTime.BrettsBirthday, group.PK, component1.PK, "1", staff.GS_Code);
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.BrettsBirthday, group.PK, component1.PK, "2", staff.GS_Code);

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizo["Attribute Value"];
			filter.IsActive = true;
			filter.Property = "1";
			AssertResults(filterBizo, 1);

			filter.Property = "0";
			AssertResults(filterBizo);
		}

		public void TestCurrentComponentFilter()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "DNT";
			var system = Factory.NewWithValidTestData<BMSystem>();
			var component1 = BMSTestHelper.CreateBucket(system, "AAA");
			var component2 = BMSTestHelper.CreateBucket(system, "BBB");
			var component3 = BMSTestHelper.CreateBucket(system, "ABC");
			var staff = Factory.New<GlbStaff>();
			var query = MENTTestHelper.CreateQuery(Factory, "CODY");

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 1, ZDateTime.BrettsBirthday, group.PK, component1.PK, "1", staff.GS_Code);
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.BrettsBirthday, group.PK, component2.PK, "1", staff.GS_Code);
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 3, ZDateTime.BrettsBirthday, group.PK, component3.PK, "1", staff.GS_Code);

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBizo.FilterStrips.AddNew("Current Component").CurrentModuleFilter;

			filter.Property = component1.PK;
			AssertResults(filterBizo, 1);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.AddTextFilterStrip("Name", "A");

			AssertResults(filterBizo, 1, 3);
		}

		#region Implementation

		static void AssertResults(string message, IFilterStripBusinessObject filterBizo, params decimal[] expectedValues)
		{
			var results = new List<decimal>();

			using (var command = Db.Connection.Command("SELECT MAS_AgedScoreValue FROM dbo.MENTAgedScoreMetric WHERE " + filterBizo.Filter.LiteralTextSqlFormatted))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					results.Add(reader.GetDecimal(0));
				}
			}

			AssertContainsExactElementsInAnyOrder(message, expectedValues, results);
		}

		static void AssertResults(IFilterStripBusinessObject filterBizo, params decimal[] expectedValues)
		{
			AssertResults(null, filterBizo, expectedValues);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new SeriesFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion

	}
}
