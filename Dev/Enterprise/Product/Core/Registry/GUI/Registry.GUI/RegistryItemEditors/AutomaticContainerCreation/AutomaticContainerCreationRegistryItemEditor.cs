using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class AutomaticContainerCreationRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public AutomaticContainerCreationRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new AutomaticContainerCreationControl();

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			base.EnableEditorPaneCore(editorPane, enabled);
			((AutomaticContainerCreationControl)editorPane).ReadOnly = !enabled;
		}
	}
}
