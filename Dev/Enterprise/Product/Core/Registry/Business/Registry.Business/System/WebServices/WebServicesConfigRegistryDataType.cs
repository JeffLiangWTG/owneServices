namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.WebServicesRegistryGridItemEditor, Enterprise.Registry.GUI")]
	public class WebServicesConfigRegistryDataType : NonPersistentBusinessObjectRegistryDataType<WebServicesConfigCollection>
	{
		public WebServicesConfigRegistryDataType(WebServicesConfigCollection defaultValue)
			: base(defaultValue)
		{
		}
	}
}
