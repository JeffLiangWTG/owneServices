using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class EnableComplianceWiseRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;

		public EnableComplianceWiseRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new EnableComplianceWiseRegistryControl();
		}

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			base.EnableEditorPaneCore(editorPane, enabled);
			((RegistryZUserControl)editorPane).ReadOnly = !enabled;
		}
	}
}
