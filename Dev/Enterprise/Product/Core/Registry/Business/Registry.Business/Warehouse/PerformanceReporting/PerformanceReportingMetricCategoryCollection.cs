using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[XmlRoot("MetricCategories")]
	public class PerformanceReportingMetricCategoryCollection : RegistryBusinessObjectCollectionTemplate, ICodeDescriptionPairList
	{
		public PerformanceReportingMetricCategoryCollection()
			: this(null)
		{
		}

		public PerformanceReportingMetricCategoryCollection(PerformanceReportingMetric parent)
		{
			this.Parent = parent;
		}

		readonly PerformanceReportingMetric Parent;

		#region AddNew

		public new PerformanceReportingMetricCategory AddNew()
		{
			return (PerformanceReportingMetricCategory)base.AddNew();
		}

		#endregion

		#region IsDuplicateSetting

		public bool IsDuplicateSetting(PerformanceReportingMetricCategory categoryToCheck)
		{
			return Elements.Cast<PerformanceReportingMetricCategory>().Any(c => c != categoryToCheck && c.EnglishCategoryName == categoryToCheck.EnglishCategoryName);
		}

		#endregion

		#region OnAdded

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			var category = bizOAdded as PerformanceReportingMetricCategory;
			if (category != null)
			{
				category.Parent = Parent;
				category.Operator = "<";

				if (Count > 1)
				{
					this[Count - 2].Operator = ">=";
				}
			}
		}

		#endregion

		#region OnRemoved

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			var category = bizO as PerformanceReportingMetricCategory;
			if (category != null)
			{
				if (Count > 0)
				{
					this[Count - 1].Operator = "<";
				}
			}
		}

		#endregion

		#region Default

		public void InitializeDefault(string metricCode)
		{
			if (metricCode == PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip)
			{
				AddNew(PerformanceReportingMetricCategory.TypeBestInClass, 99.98);
				AddNew(PerformanceReportingMetricCategory.TypeAdvantage, 99.2);
				AddNew(PerformanceReportingMetricCategory.TypeTypical, 98);
				AddNew(PerformanceReportingMetricCategory.TypeDisadvantage, 93);
				AddNew(PerformanceReportingMetricCategory.TypeMajorOpportunity, 93);
			}
			else if (metricCode == PerformanceReportingMetricType.Codes.OutboundTotalOrderCycleTimeEnteredToReleased)
			{
				AddNew(PerformanceReportingMetricCategory.TypeMajorOpportunity, 48);
				AddNew(PerformanceReportingMetricCategory.TypeDisadvantage, 24);
				AddNew(PerformanceReportingMetricCategory.TypeTypical, 13.2);
				AddNew(PerformanceReportingMetricCategory.TypeAdvantage, 5.8);
				AddNew(PerformanceReportingMetricCategory.TypeBestInClass, 5.8);
			}
			else if (metricCode == PerformanceReportingMetricType.Codes.OutboundFillRateOrder)
			{
				AddNew(PerformanceReportingMetricCategory.TypeBestInClass, 99.7);
				AddNew(PerformanceReportingMetricCategory.TypeAdvantage, 98.29);
				AddNew(PerformanceReportingMetricCategory.TypeTypical, 96.97);
				AddNew(PerformanceReportingMetricCategory.TypeDisadvantage, 92);
				AddNew(PerformanceReportingMetricCategory.TypeMajorOpportunity, 92);
			}
			else if (metricCode == PerformanceReportingMetricType.Codes.OutboundFillRateLine)
			{
				AddNew(PerformanceReportingMetricCategory.TypeBestInClass, 99.7);
				AddNew(PerformanceReportingMetricCategory.TypeAdvantage, 98.6);
				AddNew(PerformanceReportingMetricCategory.TypeTypical, 97.5);
				AddNew(PerformanceReportingMetricCategory.TypeDisadvantage, 94.34);
				AddNew(PerformanceReportingMetricCategory.TypeMajorOpportunity, 94.34);
			}
			else if (metricCode == PerformanceReportingMetricType.Codes.OutboundBackOrdersAsAPercentOfTotalOrders)
			{
				AddNew(PerformanceReportingMetricCategory.TypeMajorOpportunity, 9.74);
				AddNew(PerformanceReportingMetricCategory.TypeDisadvantage, 3);
				AddNew(PerformanceReportingMetricCategory.TypeTypical, 1);
				AddNew(PerformanceReportingMetricCategory.TypeAdvantage, 0.084);
				AddNew(PerformanceReportingMetricCategory.TypeBestInClass, 0.084);
			}
			else if (metricCode == PerformanceReportingMetricType.Codes.InboundDockToStock)
			{
				AddNew(PerformanceReportingMetricCategory.TypeMajorOpportunity, 34.4);
				AddNew(PerformanceReportingMetricCategory.TypeDisadvantage, 16);
				AddNew(PerformanceReportingMetricCategory.TypeTypical, 7);
				AddNew(PerformanceReportingMetricCategory.TypeAdvantage, 4);
				AddNew(PerformanceReportingMetricCategory.TypeBestInClass, 4);
			}
			else if (metricCode == PerformanceReportingMetricType.Codes.InboundOnTimeReceipts)
			{
				AddNew(PerformanceReportingMetricCategory.TypeBestInClass, 97.3);
				AddNew(PerformanceReportingMetricCategory.TypeAdvantage, 95);
				AddNew(PerformanceReportingMetricCategory.TypeTypical, 90);
				AddNew(PerformanceReportingMetricCategory.TypeDisadvantage, 84);
				AddNew(PerformanceReportingMetricCategory.TypeMajorOpportunity, 84);
			}
			else
			{
				AddNew(PerformanceReportingMetricCategory.TypeBestInClass, 0);
				AddNew(PerformanceReportingMetricCategory.TypeAdvantage, 0);
				AddNew(PerformanceReportingMetricCategory.TypeTypical, 0);
				AddNew(PerformanceReportingMetricCategory.TypeDisadvantage, 0);
				AddNew(PerformanceReportingMetricCategory.TypeMajorOpportunity, 0);
			}
		}

		void AddNew(ZString categoryName, ZDecimal value)
		{
			var category = this.AddNew();
			using (category.GetValidationSuspender())
			{
				category.EnglishCategoryName = categoryName;
				category.Value = value;
				category.IsTarget = PerformanceReportingMetricCategory.TypeTypical.Equals(categoryName);
			}
		}

		#endregion

		#region GetClone

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PerformanceReportingMetricCategoryCollection(Parent);
		}

		#endregion

		#region CreateNonPersistentBusinessObject

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PerformanceReportingMetricCategory();
		}

		#endregion

		#region Index

		public new PerformanceReportingMetricCategory this[int i]
		{
			get { return (PerformanceReportingMetricCategory)Elements[i]; }
		}

		public PerformanceReportingMetricCategory this[string categoryName]
		{
			get { return this.Cast<PerformanceReportingMetricCategory>().FirstOrDefault(c => c.CategoryName == categoryName); }
		}

		#endregion

		#region TargetCategory

		public PerformanceReportingMetricCategory TargetCategory
		{
			get { return this.Cast<PerformanceReportingMetricCategory>().FirstOrDefault(c => c.IsTarget); }
		}

		#endregion

		#region ICodeDescriptionPairList Members

		bool ICodeDescriptionPairList.ContainsCode(object code)
		{
			return this.Cast<PerformanceReportingMetricCategory>().Any(c => c.CategoryName == code.ToString());
		}

		string ICodeDescriptionPairList.GetDescriptionFromCode(string code)
		{
			return FindBoxListProvider.DescriptionFromCode(code);
		}

		#endregion
	}
}
