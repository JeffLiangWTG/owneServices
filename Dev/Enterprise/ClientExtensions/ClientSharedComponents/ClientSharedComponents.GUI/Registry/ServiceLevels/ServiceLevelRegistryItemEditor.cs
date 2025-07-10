using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	public class ServiceLevelRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public ServiceLevelRegistryItemEditor(ServiceLevelRegistryDataType dataType, FallbackLevel currentFallbackLevel, BusinessObjectFactory factory)
			: base(dataType, currentFallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new ServiceLevelRegistryControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
