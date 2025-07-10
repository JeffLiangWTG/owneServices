using System.Windows.Forms;

namespace Enterprise.Registry.GUI.Testing
{
	abstract class MilestoneEventUpdatesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((MilestoneEventUpdatesRegistryControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
