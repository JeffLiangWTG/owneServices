using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	public class PerformanceReportingRegistryItem : TranslatableRegistryItem<PerformanceReportingMetricCollection, PerformanceReportingMetricCollection>
	{
		public PerformanceReportingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, PerformanceReportingMetricCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new PerformanceReportingRegistryDataType(), storage, defaultValue))
		{
		}

		#region Convert

		protected override PerformanceReportingMetricCollection Convert(PerformanceReportingMetricCollection value)
		{
			foreach (PerformanceReportingMetric item in value)
			{
				foreach (PerformanceReportingMetricCategory category in item.MetricCategories)
				{
					category.CategoryName = GetMultilingualString(category.EnglishCategoryName);
				}
			}
			return value;
		}

		#endregion

		#region TranslatableRegistryItem Members

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get { return System.Array.Empty<ResourceString>(); }
		}

		public override IEnumerable<string> GetCaptions(PerformanceReportingMetricCollection value)
		{
			return value.Cast<PerformanceReportingMetric>()
					.SelectMany(item => item.MetricCategories)
					.Cast<PerformanceReportingMetricCategory>()
					.Select(c => c.CategoryName.ToString()).Distinct();
		}

		public override bool IsTranslatable
		{
			get { return true; }
		}

		public override int MaxLength
		{
			get { return PerformanceReportingMetricCategory.MaxCategoryNameLength; }
		}

		#endregion
	}

	[RegistryEditor("Enterprise.Registry.GUI.PerformanceReportingRegistryItemEditor, Enterprise.Registry.GUI")]
	class PerformanceReportingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<PerformanceReportingMetricCollection>
	{
	}
}
