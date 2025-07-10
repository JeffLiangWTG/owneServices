using System;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.Registry
{
	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.WebSecurityMappingRegistryEditor, ZClientEDI")]
	public class WebSecurityMappingRegistryEditorInfo : IRegistryEditorInfo
	{
		public WebSecurityMappingRegistryEditorInfo()
		{
		}

		public Type BaseDataTypeToBeEdited
		{
			get { return typeof(WebSecurityMappingCollection); }
		}
	}
}

