using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.CustomerService.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class LegacyModuleMappingCollection : RegistryBusinessObjectCollectionTemplate<LegacyModuleMapping>
	{
		public LegacyModuleMappingCollection()
			: this(ModuleListType.MenuSection)
		{
		}

		public LegacyModuleMappingCollection(ModuleListType legacyModuleType)
			: base(null, null)
		{
			LegacyModuleType = legacyModuleType;
		}

		public readonly ModuleListType LegacyModuleType;

		public LegacyModuleMapping AddNew(string code, string description, string criticalityMapping, string moduleMapping, string countryMapping = "")
		{
			var mapping = AddNew();
			mapping.Code = code;
			mapping.Description = description;
			mapping.CriticalityMapping = criticalityMapping;
			mapping.ModuleMapping = moduleMapping;
			mapping.CountryMapping = countryMapping;

			return mapping;
		}

		public LegacyModuleMapping GetMapping(string code)
		{
			foreach (LegacyModuleMapping mapping in this)
			{
				if (string.Equals(mapping.Code, code, StringComparison.OrdinalIgnoreCase))
				{
					return mapping;
				}
			}

			return null;
		}

		public ICodeDescriptionPairList GetLegacyModules()
		{
			var result = new CodeDescriptionPairList();
			foreach (LegacyModuleMapping mapping in this)
			{
				result.AddPair(mapping.Code, mapping.Description);
			}

			return result;
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new LegacyModuleMappingCollection(LegacyModuleType);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new LegacyModuleMapping(LegacyModuleType);
		}

		#endregion
	}
}

