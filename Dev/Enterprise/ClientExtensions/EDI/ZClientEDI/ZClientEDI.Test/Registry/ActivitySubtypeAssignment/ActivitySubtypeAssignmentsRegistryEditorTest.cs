using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(ActivitySubtypeAssignmentsRegistryEditor))]
	public class ActivitySubtypeAssignmentsRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ActivitySubtypeAssignmentsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new ActivitySubtypeAssignmentsRegistryEditor(new ActivitySubtypeAssignmentsRegistryDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ActivitySubtypeAssignmentsControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			ActivitySubtypeAssignmentCollection collection = new ActivitySubtypeAssignmentCollection();
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
			return !((ActivitySubtypeAssignmentsControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
