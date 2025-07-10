using System;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.Registry
{
	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.SourceModulesRegistryEditor, ZClientEDI")]
	public class SourceModulesRegistryEditorInfo : IRegistryEditorInfo
	{
		public SourceModulesRegistryEditorInfo()
		{
		}

		public Type BaseDataTypeToBeEdited
		{
			get { return typeof(SourceModuleCollection); }
		}
	}
}

