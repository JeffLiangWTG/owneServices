using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class FTPDestinationOverrideRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public FTPDestinationOverrideRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new FTPDestinationOverrideUserControl();
		}
	}
}
