using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class CustomsReferenceNumberTypesRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public CustomsReferenceNumberTypesRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new CustomsReferenceNumberTypesRegistryControl();
		}

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			((CustomsReferenceNumberTypesRegistryControl)editorPane).ReadOnly = !enabled;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
