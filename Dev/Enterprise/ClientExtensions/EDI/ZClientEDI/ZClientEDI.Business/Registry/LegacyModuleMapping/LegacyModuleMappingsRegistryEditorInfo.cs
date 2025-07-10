using System;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.Registry
{
	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.LegacyModuleMappingsRegistryEditor, ZClientEDI")]
	public class LegacyModuleMappingsRegistryEditorInfo : IRegistryEditorInfo
	{
		public LegacyModuleMappingsRegistryEditorInfo(ZString moduleMappingCaption)
		{
			ModuleMappingCaption = moduleMappingCaption;
		}

		public readonly ZString ModuleMappingCaption;

		public Type BaseDataTypeToBeEdited
		{
			get { return typeof(LegacyModuleMappingCollection); }
		}
	}
}

