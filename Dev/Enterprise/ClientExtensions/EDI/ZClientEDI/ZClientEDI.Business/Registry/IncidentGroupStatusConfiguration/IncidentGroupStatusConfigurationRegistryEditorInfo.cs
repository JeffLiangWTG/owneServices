using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.Registry.Business
{
	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.IncidentGroupStatusConfigurationRegistryEditor, ZClientEDI")]
	public class IncidentGroupStatusConfigurationRegistryEditorInfo : IRegistryEditorInfo
	{
		public Type BaseDataTypeToBeEdited => typeof(IncidentGroupTypeCollection);
	}
}
