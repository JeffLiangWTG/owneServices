using Enterprise.Registry.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[RegistryEditor("Enterprise.Customs.AU.Declaration.GUI.DefaultPremiseIDsRegistryItemEditor, Enterprise.Customs.AU.Declaration.GUI")]
	public class DefaultPremiseIDsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DefaultPremiseIDCollection>
	{
		public DefaultPremiseIDsRegistryDataType()
		{
		}
	}
}
