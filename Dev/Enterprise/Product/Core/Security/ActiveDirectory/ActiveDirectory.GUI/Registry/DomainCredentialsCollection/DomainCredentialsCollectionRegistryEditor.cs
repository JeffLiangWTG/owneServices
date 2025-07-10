using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.ActiveDirectory.GUI
{
	public class DomainCredentialsCollectionRegistryEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public DomainCredentialsCollectionRegistryEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new DomainCredentialsCollectionRegistryControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			base.EnableEditorPaneCore(editorPane, enabled);

			((RegistryZUserControl)editorPane).ReadOnly = !enabled;
		}
	}
}

