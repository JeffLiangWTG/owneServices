using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(ServiceLevelRegistryItemEditor))]
	public class ServiceLevelRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ServiceLevelRegistryItemEditor((ServiceLevelRegistryDataType)RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ServiceLevelRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ServiceLevelRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ServiceLevelRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			ServiceLevelRegistryBusinessObjectCollection setupCollection = new ServiceLevelRegistryBusinessObjectCollection();
			ServiceLevelRegistryBusinessObject setup = setupCollection.AddNew();
			setup.ServiceLevel = "STD";

			return new object[] { setupCollection };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			ServiceLevelRegistryBusinessObjectCollection collection1 = (ServiceLevelRegistryBusinessObjectCollection)setValue;
			ServiceLevelRegistryBusinessObjectCollection collection2 = (ServiceLevelRegistryBusinessObjectCollection)getValue;

			AssertEquals("GetValueFromEditorPane().Count", collection1.Count, collection2.Count);

			for (int i = 0; i < collection1.Count; ++i)
			{
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].ServiceLevel", i.ToString()), collection1[i].ServiceLevel, collection2[i].ServiceLevel);
			}
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
