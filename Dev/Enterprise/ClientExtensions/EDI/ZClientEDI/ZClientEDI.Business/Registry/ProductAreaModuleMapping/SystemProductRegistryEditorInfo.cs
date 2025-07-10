using System;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.CustomerService.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Registry
{
	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.SystemProductRegistryEditor, ZClientEDI")]
	public class SystemProductRegistryEditorInfo : IRegistryEditorInfo
	{
		public SystemProductRegistryEditorInfo(ModuleListType moduleListType, ZBool isSourceModuleMappingsVisible)
		{
			ModuleListType = moduleListType;
			IsSourceModuleMappingsVisible = isSourceModuleMappingsVisible;
		}

		public readonly ModuleListType ModuleListType;
		public readonly ZBool IsSourceModuleMappingsVisible;

		public ZString ModuleCaption
		{
			get { return EnumExtensions.GetCaption(ModuleListType); }
		}

		public Type BaseDataTypeToBeEdited
		{
			get { return typeof(SystemProductCollection); }
		}
	}
}
