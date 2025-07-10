using System;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.ComponentModel
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class ModuleIDAttribute : SingleMetaDataAttribute
	{
		public ModuleIDAttribute(ModuleId moduleId, bool universalCopyMenuIgnoreModuleId = false)
			: base(ZMetaDataTypes.ModuleId)
		{
			this.ModuleId = moduleId;
			this.ModuleIdName = moduleId.ToString();
			this.UniversalCopyMenuIgnoreModuleId = universalCopyMenuIgnoreModuleId;
		}

		public ModuleIDAttribute(string moduleId, bool universalCopyMenuIgnoreModuleId = false)
			: base(ZMetaDataTypes.ModuleId)
		{
			this.ModuleIdName = moduleId;
			this.UniversalCopyMenuIgnoreModuleId = universalCopyMenuIgnoreModuleId;
		}

		public ModuleIdentifier ModuleIdentifier
		{
			get
			{
				var clientHook = Enterprise.ZArchitecture.Modules.ClientHookLoader.Instance.ClientHook;
				var result = ModuleIDs.NotAssigned;

				if (clientHook != null)
				{
					result = clientHook.ModuleOverrides.GetRegisteredIdentifierByName(ModuleIdName);
					if (result == null || result == ModuleIDs.NotAssigned)
					{
						if (clientHook.NewClientModules != null && Array.Exists(clientHook.NewClientModules, module => module.ID.Name == ModuleIdName))
						{
							result = Array.Find(clientHook.NewClientModules, module => module.ID.Name == ModuleIdName).ID;
						}
					}
				}

				if (result == null || result == ModuleIDs.NotAssigned)
				{
					result = ModuleIDs.AllExcludingClientModules.FirstOrDefault(module => module.Name == ModuleIdName);
				}

				return result;
			}
		}

		public string ModuleIdName { get; private set; }
		public Enum ModuleId { get; private set; }
		public bool UniversalCopyMenuIgnoreModuleId { get; private set; }

		public override bool ProvidesMetaDataValue(string metaDataTypeId)
		{
			return metaDataTypeId == ZMetaDataTypes.ModuleId && (ModuleIdName != null || !string.IsNullOrEmpty(ModuleIdName));
		}

		public override object GetMetaDataValue(string metaDataTypeId)
		{
			return ModuleIdentifier;
		}

		public override bool ProvidesMetaDataMember(string metaDataTypeId)
		{
			return false;
		}

		public override string GetMetaDataMember(string metaDataTypeId)
		{
			return null;
		}
	}
}
