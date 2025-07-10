using Enterprise.Registry.Business;

namespace Enterprise.Customs.CA.Registry
{
	[RegistryEditor("Enterprise.Customs.CA.GUI.DefaultFreightPercentagesRegistryItemEditor, Enterprise.Customs.CA.GUI")]
	public class DefaultFreightPercentagesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DefaultFreightPercentageCollection>
	{
		public DefaultFreightPercentagesRegistryDataType()
		{
		}
	}
}
