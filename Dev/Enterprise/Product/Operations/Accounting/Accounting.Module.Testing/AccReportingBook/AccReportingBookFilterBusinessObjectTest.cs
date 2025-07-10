using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccReportingBookFilterBusinessObject))]
	sealed class AccReportingBookFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCodeFilter()
		{
			var filter = (ModuleTextFilter)FilterBO["Code"];
			filter.Property = "1";
			filter.IsActive = true;
			ReportingBookCollection.Load(FilterBO.Filter);
			Assert("Should only contain reportingBook1", ReportingBookCollection.Contains(Book1PK));
			Assert("Should not contain reportingBook2", !ReportingBookCollection.Contains(Book2PK));
			AssertEquals(filter.FilterColumn.MaxLength, filter.MaxLength);
		}

		public void TestGlobalStatusFilter()
		{
			var filter = (ModuleTextFilter)FilterBO["Global Status"];
			var filter_collection = filter.ParentCollections;
			filter.Property = "GLOBAL";
			filter.IsActive = true;
			ReportingBookCollection.Load(FilterBO.Filter);
			Assert("Should only contain reportingBook1", ReportingBookCollection.Contains(Book1PK));
			Assert("Should not contain reportingBook2", !ReportingBookCollection.Contains(Book2PK));

			filter.Property = "NOT GLOBAL";
			filter.IsActive = true;
			ReportingBookCollection.Load(FilterBO.Filter);
			Assert("Should only contain reportingBook2", ReportingBookCollection.Contains(Book2PK));
			Assert("Should not contain reportingBook1", !ReportingBookCollection.Contains(Book1PK));
			AssertEquals(10, filter.MaxLength);
		}

		public void TestPresentationJournalFilter()
		{
			var filter = (ModuleTextFilter)FilterBO["Presentation Journal"];
			filter.Property = "UUU";
			filter.IsActive = true;
			ReportingBookCollection.Load(FilterBO.Filter);
			Assert("Should only contain reportingBook1", ReportingBookCollection.Contains(Book1PK));
			Assert("Should not contain reportingBook2", !ReportingBookCollection.Contains(Book2PK));
		}

		public void TestPresentationJournalFilterWithChildCategory()
		{
			var companyCategorySetList = new GLPresentationJournalCategoryCollection();
			var category1 = companyCategorySetList.AddNew();
			category1.Code = "IOS";
			category1.Description = (NoResString)"Category 1";
			var category2 = companyCategorySetList.AddNew();
			category2.Code = "EOC";
			category2.ParentCode = category1.Code;
			category2.Description = (NoResString)"Category 2";
			var category3 = companyCategorySetList.AddNew();
			category3.Code = "DOC";
			category3.ParentCode = category1.Code;
			category3.Description = (NoResString)"Category 3";
			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, companyCategorySetList);

			var creator = new TestObjectCreator(Factory);
			var reportingBook1 = creator.CreateReportingBook("11", "1", AlternateChart1PK, "IOS", true);
			var reportingBook2 = creator.CreateReportingBook("21", "1", AlternateChart1PK, "EOC", false);
			var reportingBook3 = creator.CreateReportingBook("31", "1", AlternateChart1PK, "DOC", false);
			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO["Presentation Journal"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "C";
			filter.IsActive = true;
			ReportingBookCollection.Load(FilterBO.Filter);
			Assert("Should contain IOS", ReportingBookCollection.Contains(reportingBook1));
			Assert("Should contain EOC", ReportingBookCollection.Contains(reportingBook2));
			Assert("Should contain DOC", ReportingBookCollection.Contains(reportingBook3));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			ReportingBookCollection.Load(FilterBO.Filter);
			Assert("Should not contain IOS", !ReportingBookCollection.Contains(reportingBook1));
			Assert("Should not contain EOC", !ReportingBookCollection.Contains(reportingBook2));
			Assert("Should not contain DOC", !ReportingBookCollection.Contains(reportingBook3));
			Assert("Should contain UUU", ReportingBookCollection.Contains(Book1PK));

			filter.Property = "E";
			ReportingBookCollection.Load(FilterBO.Filter);
			Assert("Should not contain IOS", !ReportingBookCollection.Contains(reportingBook1));
			Assert("Should not contain EOC", !ReportingBookCollection.Contains(reportingBook2));
			Assert("Should contain DOC", ReportingBookCollection.Contains(reportingBook3));
			Assert("Should contain UUU", ReportingBookCollection.Contains(Book1PK));

			filter.Property = "DOC";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			ReportingBookCollection.Load(FilterBO.Filter);
			Assert("Should contain IOS", ReportingBookCollection.Contains(reportingBook1));
			Assert("Should not contain EOC", !ReportingBookCollection.Contains(reportingBook2));
			Assert("Should contain DOC", ReportingBookCollection.Contains(reportingBook3));
			Assert("Should not contain UUU", !ReportingBookCollection.Contains(Book1PK));
		}

		public void TestPresentationJournalFilterInvalidCode()
		{
			var companyCategorySetList = new GLPresentationJournalCategoryCollection();
			var category1 = companyCategorySetList.AddNew();
			category1.Code = "IOS";
			category1.Description = (NoResString)"Category 1";
			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, companyCategorySetList);

			var filter = (ModuleTextFilter)FilterBO["Presentation Journal"];
			filter.Property = "X";
			filter.IsActive = true;

			var warningComparisons = new[]
			{
				SQLComparisonOperator.StartsWith,
				SQLComparisonOperator.Contains,
				SQLComparisonOperator.DoesNotStartWith,
				SQLComparisonOperator.NotContains
			};

			foreach (var comparison in warningComparisons)
			{
				filter.SqlComparisonOperator = comparison;
				filter.Validation.ValidateProperty();
				Assert(filter.PropertyInfo.HasWarning(ListValidation.InvalidCodeMessage.ToString()));
			}

			var errorComparisons = new[]
			{
				SQLComparisonOperator.Equal,
				SQLComparisonOperator.NotEqual,
			};

			foreach (var comparison in errorComparisons)
			{
				filter.SqlComparisonOperator = comparison;
				filter.Validation.ValidateProperty();
				Assert(filter.PropertyInfo.HasError("Enter a valid selection."));
			}
		}

		public void TestAlternateChartFilterFilter()
		{
			var filter = (ModuleGuidFilter)FilterBO["Alternate Chart"];
			filter.Property = AlternateChart1PK;
			filter.IsActive = true;
			ReportingBookCollection.Load(FilterBO.Filter);
			Assert("Should only contain reportingBook1", ReportingBookCollection.Contains(Book1PK));
			Assert("Should not contain reportingBook2", !ReportingBookCollection.Contains(Book2PK));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccReportingBookFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			FilterBO = (AccReportingBookFilterBusinessObject)GetNewFilterStripBusinessObject();
			ReportingBookCollection = new AccReportingBookCollection(Factory);

			CreateData();
		}

		void CreateData()
		{
			var creator = new TestObjectCreator(Factory);
			var chart1 = creator.CreateAlternateChart("1", "4", false, true);
			var chart2 = creator.CreateAlternateChart("2", "4", false, false);
			Factory.Save();

			AlternateChart1PK = chart1.PK;

			var reportingBook1 = creator.CreateReportingBook("1", "1", chart1.PK, "UUU");
			var reportingBook2 = creator.CreateReportingBook("2", "1", chart2.PK, "DDD");
			Factory.Save();

			Book1PK = reportingBook1.PK;
			Book2PK = reportingBook2.PK;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheck();
			result.Add(TableFilter(AccAlternateChartSchema.Constants.TableName, "Global Status"));
			return result;
		}

		AccReportingBookFilterBusinessObject FilterBO;
		AccReportingBookCollection ReportingBookCollection;
		ZGuid Book1PK, Book2PK;
		ZGuid AlternateChart1PK;
	}
}
