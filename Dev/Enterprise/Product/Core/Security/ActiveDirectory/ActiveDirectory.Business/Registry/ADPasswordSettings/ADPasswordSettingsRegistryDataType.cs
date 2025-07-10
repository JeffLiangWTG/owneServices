using Enterprise.Registry.Business;

namespace Enterprise.Security.ActiveDirectory
{
	[RegistryEditor("Enterprise.Security.ActiveDirectory.GUI.ADPasswordSettingsRegistryEditor, Enterprise.Security.ActiveDirectory.GUI")]
	public class ADPasswordSettingsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ADPasswordSettingsRegistryBusinessObject>
	{
		public ADPasswordSettingsRegistryDataType(ADPasswordSettingsRegistryBusinessObject defaultValue) : base(defaultValue)
		{
		}
	}
}
