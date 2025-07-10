using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI.RegistryItemEditors.System.ProcessController
{
	public class LoggingMethodsRegistryItemEditor : CodeDescriptionBoolWithExtraBoolRegistryItemEditor
	{
		public LoggingMethodsRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType,
				fallbackLevel,
				factory,
				Res.GetString("{91AA3CF8-CB25-4357-B513-1C247568B38A}", "Read from"),
				Res.GetString("{60A56149-D5D5-420A-9E00-19F9ACCC290A}", "Write to"))
		{
		}
	}
}
