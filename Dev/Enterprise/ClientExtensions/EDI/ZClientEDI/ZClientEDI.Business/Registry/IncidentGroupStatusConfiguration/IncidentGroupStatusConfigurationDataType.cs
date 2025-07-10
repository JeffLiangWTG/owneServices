using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.Registry.Business
{
	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.IncidentGroupStatusConfigurationRegistryEditor, ZClientEDI")]
	public class IncidentGroupStatusConfigurationDataType : NonPersistentBusinessObjectRegistryDataType<IncidentGroupTypeCollection>
	{
		public IncidentGroupStatusConfigurationDataType() : base()
		{
		}
	}
}
