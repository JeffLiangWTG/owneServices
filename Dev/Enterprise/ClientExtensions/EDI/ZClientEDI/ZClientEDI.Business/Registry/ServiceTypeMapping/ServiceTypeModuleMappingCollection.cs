using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ServiceTypeModuleMappingCollection : RegistryBusinessObjectCollectionTemplate<ServiceTypeModuleMapping>
	{
		public ServiceTypeModuleMappingCollection()
			: base(null, null)
		{
		}

		public ServiceTypeModuleMapping AddNew(string code)
		{
			var result = AddNew();
			result.Code = code;
			return result;
		}

		public ServiceTypeModuleMapping AddIfNotExists(string code)
		{
			if (!ContainsCode(code))
			{
				return AddNew(code);
			}

			return null;
		}

		ServiceTypeModuleMapping GetSourceModuleMapping(string sourceModuleCode)
		{
			foreach (ServiceTypeModuleMapping mapping in this)
			{
				if (string.Equals(mapping.Code, sourceModuleCode, StringComparison.OrdinalIgnoreCase))
				{
					return mapping;
				}
			}

			return null;
		}

		ZBool ContainsCode(string code)
		{
			return GetSourceModuleMapping(code) != null;
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ServiceTypeModuleMappingCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ServiceTypeModuleMapping();
		}

		#endregion
	}
}

