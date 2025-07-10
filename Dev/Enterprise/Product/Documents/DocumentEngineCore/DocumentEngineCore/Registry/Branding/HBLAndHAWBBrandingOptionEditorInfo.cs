using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentEngineCore.Registry
{
	[RegistryEditor("Enterprise.DocumentEngineCore.GUI.Registry.HBLAndHAWBBrandingOptionRegistryItemEditor, Enterprise.DocumentEngineCore.GUI")]
	public class HBLAndHAWBBrandingOptionEditorInfo : IRegistryEditorInfo
	{
		public const string AgentBranded = "Agent-branded";
		public const string ClientBranded = "Client-branded";

		#region IRegistryEditorInfo Members

		public Type BaseDataTypeToBeEdited
		{
			get { return typeof(string); }
		}

		#endregion
	}
}
