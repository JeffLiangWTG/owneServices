using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	class DpsWebServiceItemRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;

		public DpsWebServiceItemRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new DpsWebServiceItemControl();

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			base.EnableEditorPaneCore(editorPane, enabled);

			((RegistryZUserControl)editorPane).ReadOnly = !enabled;
		}
	}
}
