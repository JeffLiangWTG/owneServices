using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class ContainerPenaltyFreeDaysOptionsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public ContainerPenaltyFreeDaysOptionsRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new ContainerPenaltyFreeDaysOptionsRegistryItemControl();
		}

		protected override EditorPaneAnchor Anchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
