using System;
using Enterprise.Integration;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(PerformanceReportingRegistryItemEditor))]
	sealed class PerformanceReportingRegistryItemEditorTest : Enterprise.Registry.GUI.Testing.RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new PerformanceReportingRegistryItem("", null, null, null, RegistryStorageFlags.System, PerformanceReportingMetricCollection.GetDefault());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new PerformanceReportingRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(PerformanceReportingControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new PerformanceReportingMetricCollection() };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((PerformanceReportingControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
