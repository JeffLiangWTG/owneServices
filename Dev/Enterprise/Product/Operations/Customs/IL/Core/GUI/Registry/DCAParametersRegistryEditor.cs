using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IL.GUI
{
	public class DCAParametersRegistryEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public DCAParametersRegistryEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled) => ((DCAParametersControl)editorPane).ReadOnly = !enabled;

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new DCAParametersControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
