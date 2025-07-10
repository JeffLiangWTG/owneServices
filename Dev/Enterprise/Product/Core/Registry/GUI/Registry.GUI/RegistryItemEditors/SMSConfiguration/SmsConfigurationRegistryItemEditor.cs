using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class ServerUsernamePasswordConfigurationRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public ServerUsernamePasswordConfigurationRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new ServerUsernamePasswordConfigurationControl();
		}
	}
}
