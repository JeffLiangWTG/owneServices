using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class WebUrlThemeRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public WebUrlThemeRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new WebUrlThemeControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
