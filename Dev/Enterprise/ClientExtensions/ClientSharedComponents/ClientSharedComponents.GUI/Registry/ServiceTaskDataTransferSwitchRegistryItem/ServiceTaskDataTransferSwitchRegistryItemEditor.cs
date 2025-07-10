using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	class ServiceTaskDataTransferSwitchRegistryItemEditor : DataTransferSwitchRegistryItemEditor
	{
		public ServiceTaskDataTransferSwitchRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new ServiceTaskDataTransferSwitchRegistryControl();
		}
	}
}
