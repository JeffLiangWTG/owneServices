using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class HouseBillOfLadingTermsAndConditionsCollectionRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public HouseBillOfLadingTermsAndConditionsCollectionRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new HouseBillOfLadingTermsAndConditionsCollectionControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
