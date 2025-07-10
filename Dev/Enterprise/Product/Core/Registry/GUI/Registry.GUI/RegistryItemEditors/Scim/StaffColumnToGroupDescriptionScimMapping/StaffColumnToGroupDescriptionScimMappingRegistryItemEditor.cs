using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class StaffColumnToGroupDescriptionScimMappingRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public StaffColumnToGroupDescriptionScimMappingRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new StaffColumnToGroupDescriptionScimMappingControl();
		}

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			base.EnableEditorPaneCore(editorPane, enabled);
			((StaffColumnToGroupDescriptionScimMappingControl)editorPane).ReadOnly = !enabled;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
