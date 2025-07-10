using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	class EventsVisibilityRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public EventsVisibilityRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new EventsVisibilityRegistryControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
