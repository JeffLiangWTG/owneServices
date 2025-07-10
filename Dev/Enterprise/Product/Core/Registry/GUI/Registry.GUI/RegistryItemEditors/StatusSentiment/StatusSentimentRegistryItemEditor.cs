using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class StatusSentimentRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public StatusSentimentRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
					: base(dataType, fallbackLevel, factory)
		{ }

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new StatusSentimentControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
