using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	class ServiceTaskDataTransferHighWaterMarkRegistryItemEditor : DataTransferRegistryItemEditor
	{
		public ServiceTaskDataTransferHighWaterMarkRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new ServiceTaskDataTransferHighWaterMarkRegistryControl();
		}
	}
}
