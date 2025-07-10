using System;
using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PerformanceReportingRegistryItem))]
	sealed class PerformanceReportingRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<PerformanceReportingMetricCollection>
	{
		protected override StronglyTypedRegistryItem<PerformanceReportingMetricCollection, PerformanceReportingMetricCollection> GetNewRegistryItem()
		{
			return new PerformanceReportingRegistryItem("", null, null, null, RegistryStorageFlags.System, new PerformanceReportingMetricCollection());
		}

		#region TestTranslatable

		public void TestTranslatable()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var registryItem = new PerformanceReportingRegistryItem("", null, null, null, RegistryStorageFlags.System, new PerformanceReportingMetricCollection());
				var metricCollection = registryItem.Value;
				var metric = (PerformanceReportingMetric)metricCollection.AddNew();
				metric.MetricCode = "Test";

				var categories = metric.MetricCategories;
				var category = categories.AddNew();
				category.EnglishCategoryName = "Group 1";
				category.Value = 90;
				category.IsTarget = false;

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, metricCollection);
				var key = ((ResourceString)registryItem.Value[0].MetricCategories[0].CategoryName).ResourceKey;
				mockChs.Put(key, new ResourceStringData(key, "群组一"));
				AssertEquals("群组一", registryItem.Value[0].MetricCategories[0].CategoryName.ToString(Core.SharedConstants.Languages.ChineseSimplified));
			}
		}

		#endregion

		#region TestGetCaptions_NoDuplicates

		public void TestGetCaptions_NoDuplicates()
		{
			var registryItem = new PerformanceReportingRegistryItem("", null, null, null, RegistryStorageFlags.System, new PerformanceReportingMetricCollection());
			var metricCollection = registryItem.Value;
			AddNewPerformanceReportMetric(metricCollection, "T1", "category", 90, false);
			AddNewPerformanceReportMetric(metricCollection, "T2", "category", 90, false);
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, metricCollection);

			var captions = registryItem.GetCaptions(metricCollection);
			AssertEquals(1, captions.Count());
			AssertEquals("category", captions.FirstOrDefault());
		}

		void AddNewPerformanceReportMetric(PerformanceReportingMetricCollection collection, string metricCode, string englishCategoryName, decimal categoryValue, bool categoryIsTarget)
		{
			var metric = (PerformanceReportingMetric)collection.AddNew();
			metric.MetricCode = metricCode;
			var category = metric.MetricCategories.AddNew();
			category.EnglishCategoryName = englishCategoryName;
			category.Value = categoryValue;
			category.IsTarget = categoryIsTarget;
		}

		#endregion
	}
}
