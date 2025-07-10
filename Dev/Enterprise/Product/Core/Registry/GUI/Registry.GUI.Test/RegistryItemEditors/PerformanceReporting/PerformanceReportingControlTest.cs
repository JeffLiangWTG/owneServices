using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(PerformanceReportingControl))]
	sealed class PerformanceReportingControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			var metricCollection = new PerformanceReportingMetricCollection();
			metricCollection.AddNew("ISDO");
			return metricCollection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((PerformanceReportingControl)control).CategoryGrid.ReadOnly;
		}

		protected override RegistryZUserControl GetNewControl()
		{
			var control = new PerformanceReportingControl();

			// select an item in the parent grid to populate the child grid
			control.BindingSource.DataSourceChanged += delegate
			{
				if (control.MetricGrid.List != null && control.MetricGrid.List.Count > 0)
				{
					control.MetricGrid.Select(0);
				}
			};
			return control;
		}
	}
}
