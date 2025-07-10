using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class TransportModeCombinationBufferTimeRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public TransportModeCombinationBufferTimeRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new TransportModeCombinationBufferTimeRegistryControl();
		}

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
