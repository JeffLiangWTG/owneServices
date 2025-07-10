using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class SubscriptionRulesRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public SubscriptionRulesRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new SubscriptionRulesRegistryControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
