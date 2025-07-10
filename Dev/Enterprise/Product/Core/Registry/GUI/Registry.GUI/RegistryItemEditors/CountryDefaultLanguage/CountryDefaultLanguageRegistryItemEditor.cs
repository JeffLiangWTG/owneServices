using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class CountryDefaultLanguageRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;

		public CountryDefaultLanguageRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new CountryDefaultLanguageControl();
		}
	}
}
