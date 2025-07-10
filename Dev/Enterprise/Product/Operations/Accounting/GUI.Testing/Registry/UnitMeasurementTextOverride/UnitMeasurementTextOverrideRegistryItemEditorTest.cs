using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(UnitMeasurementTextOverrideRegistryItemEditor))]
	public class UnitMeasurementTextOverrideRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new UnitMeasurementTextOverrideRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(UnitMeasurementTextOverrideControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new UnitMeasurementTextOverrideCollection();
			var unitMeasurementTextOverride = collection.AddNew();
			unitMeasurementTextOverride.UnitMeasurement = "KG";
			unitMeasurementTextOverride.TextOverride = "KG";

			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((UnitMeasurementTextOverrideControl)editorPane).ReadOnly;
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new UnitMeasurementTextOverrideRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
