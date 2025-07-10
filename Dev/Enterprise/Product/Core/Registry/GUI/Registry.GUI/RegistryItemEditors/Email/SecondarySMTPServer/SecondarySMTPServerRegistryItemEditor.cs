using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class SecondarySMTPServerRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public SecondarySMTPServerRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new SecondarySMTPServerControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
