using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ProductAreaSourceModuleMappingCollection : RegistryBusinessObjectCollectionTemplate<ProductAreaSourceModuleMapping>
	{
		public ProductAreaSourceModuleMappingCollection()
			: base(null, null)
		{
		}

		public ProductAreaSourceModuleMapping AddNew(string code, string productArea)
		{
			var result = AddNew();
			result.Code = code;
			result.ProductArea = productArea;
			return result;
		}

		public ProductAreaSourceModuleMapping AddIfNotExists(string code, string productArea)
		{
			if (!ContainsCode(code))
			{
				return AddNew(code, productArea);
			}

			return null;
		}

		public ProductAreaSourceModuleMapping GetSourceModuleMapping(string sourceModuleCode)
		{
			foreach (ProductAreaSourceModuleMapping mapping in this)
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
			return new ProductAreaSourceModuleMappingCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ProductAreaSourceModuleMapping();
		}

		#endregion
	}
}

