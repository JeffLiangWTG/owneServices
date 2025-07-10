using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(ProductAreaAssignmentsRegistryEditor))]
	public class ProductAreaAssignmentsRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ProductAreaAssignmentsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new ProductAreaAssignmentsRegistryEditor(new ProductAreaAssignmentsRegistryDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ProductAreaAssignmentsControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			ProductAreaAssignmentCollection collection = new ProductAreaAssignmentCollection();
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ProductAreaAssignmentsControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
