using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class PerformanceReportingMetricCollection : RegistryBusinessObjectCollectionTemplate, IDocDataMetricRanking
	{
		#region Index

		public new PerformanceReportingMetric this[int index]
		{
			get { return (PerformanceReportingMetric)(Elements[index]); }
		}

		public PerformanceReportingMetric this[string metricCode]
		{
			get { return Elements.Cast<PerformanceReportingMetric>().FirstOrDefault(m => m.MetricCode == metricCode); }
		}

		#endregion

		#region AddNew

		public PerformanceReportingMetric AddNew(string metricCode)
		{
			var metric = (PerformanceReportingMetric)base.AddNew();
			metric.MetricCode = metricCode;
			metric.MetricCategories.InitializeDefault(metricCode);
			return metric;
		}

		#endregion

		#region GetDefault

		public static PerformanceReportingMetricCollection GetDefault()
		{
			var result = new PerformanceReportingMetricCollection();

			result.AddNew(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip);
			result.AddNew(PerformanceReportingMetricType.Codes.OutboundTotalOrderCycleTimeEnteredToReleased);
			result.AddNew(PerformanceReportingMetricType.Codes.OutboundOperationalOrderCycleTimePickedToFinalized);
			result.AddNew(PerformanceReportingMetricType.Codes.OutboundFillRateOrder);
			result.AddNew(PerformanceReportingMetricType.Codes.OutboundFillRateLine);
			result.AddNew(PerformanceReportingMetricType.Codes.OutboundBackOrdersAsAPercentOfTotalOrders);
			result.AddNew(PerformanceReportingMetricType.Codes.OutboundOrdersPerHour);
			result.AddNew(PerformanceReportingMetricType.Codes.OutboundOrderLinesPerHour);
			result.AddNew(PerformanceReportingMetricType.Codes.OutboundPicksPerHour);
			result.AddNew(PerformanceReportingMetricType.Codes.OutboundPickLinesPerHour);

			result.AddNew(PerformanceReportingMetricType.Codes.InboundDockToStock);
			result.AddNew(PerformanceReportingMetricType.Codes.InboundEarlyReceipts);
			result.AddNew(PerformanceReportingMetricType.Codes.InboundOnTimeReceipts);
			result.AddNew(PerformanceReportingMetricType.Codes.InboundLateReceipts);
			result.AddNew(PerformanceReportingMetricType.Codes.InboundDamageFreeReceiptsJob);
			result.AddNew(PerformanceReportingMetricType.Codes.InboundDamageFreeReceiptsLine);
			result.AddNew(PerformanceReportingMetricType.Codes.InboundReceiveJobsPerHour);
			result.AddNew(PerformanceReportingMetricType.Codes.InboundReceiveLinesPerHour);

			return result;
		}

		#endregion

		#region GetMetricRanking

		public ZString GetMetricRanking(ZString metricCode, ZDecimal metricValue, bool isPercentage = false)
		{
			var metricRanking = GetPerformanceCategory(metricCode, metricValue, isPercentage);
			return metricRanking != null ? metricRanking.CategoryName : null;
		}

		PerformanceReportingMetricCategory GetPerformanceCategory(string metricCode, decimal value, bool isPercentage)
		{
			PerformanceReportingMetricCategory result = null;

			var metric = this[metricCode];
			if (metric != null)
			{
				var orderedCategories = metric.MetricCategories.Cast<PerformanceReportingMetricCategory>();
				var adjustedValue = isPercentage ? value * 100 : value;

				foreach (var category in orderedCategories)
				{
					if (adjustedValue >= category.Value)
					{
						result = category;
						break;
					}
				}

				if (result == null)
				{
					result = orderedCategories.LastOrDefault();
				}
			}

			return result;
		}

		#endregion

		#region GetMetricTargetRanking

		public ZString GetMetricTargetRanking(ZString metricCode)
		{
			var target = GetTargetCategory(metricCode);
			return target != null ? target.CategoryName : ZString.Empty;
		}

		#endregion

		#region GetMetricTargetRange

		public ZString GetMetricTargetRange(ZString metricCode)
		{
			var targetRange = GetMetricTargetRangeResult(metricCode);
			var result = ZString.Empty;

			if (targetRange.Min.IsEmpty && !targetRange.Max.IsEmpty)
			{
				result = ZString.Format("< {0}", targetRange.Max);
			}
			else if (!targetRange.Min.IsEmpty && targetRange.Max.IsEmpty)
			{
				result = ZString.Format(">= {0}", targetRange.Min);
			}
			else if (!targetRange.Min.IsEmpty && !targetRange.Max.IsEmpty)
			{
				result = ZString.Format(">= {0} & < {1}", targetRange.Min, targetRange.Max);
			}

			return result;
		}

		public ZDecimal GetMetricTargetRangeMin(ZString metricCode)
		{
			return GetMetricTargetRangeResult(metricCode).Min;
		}

		public ZDecimal GetMetricTargetRangeMax(ZString metricCode)
		{
			return GetMetricTargetRangeResult(metricCode).Max;
		}

		TargetRangeResult GetMetricTargetRangeResult(ZString metricCode)
		{
			var result = new TargetRangeResult();

			var metric = this[metricCode];
			if (metric != null)
			{
				var target = GetTargetCategory(metricCode);
				if (target != null)
				{
					var list = metric.MetricCategories.Cast<PerformanceReportingMetricCategory>().ToList();
					var index = list.FindIndex(c => c == target);
					var isFirst = index == 0;
					var isLast = index == list.Count - 1;

					result.Min = isLast ? 0 : target.Value;
					result.Max = isLast ? target.Value : isFirst ? 0 : list[index - 1].Value;
				}
			}

			return result;
		}

		#region Implementation

		PerformanceReportingMetricCategory GetTargetCategory(ZString metricCode)
		{
			PerformanceReportingMetricCategory result = null;

			var metric = this[metricCode];
			if (metric != null)
			{
				result = metric.MetricCategories.TargetCategory;
			}

			return result;
		}

		class TargetRangeResult
		{
			public ZDecimal Min { get; set; }
			public ZDecimal Max { get; set; }
		}

		#endregion

		#endregion

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PerformanceReportingMetric();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PerformanceReportingMetricCollection();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion
	}
}
