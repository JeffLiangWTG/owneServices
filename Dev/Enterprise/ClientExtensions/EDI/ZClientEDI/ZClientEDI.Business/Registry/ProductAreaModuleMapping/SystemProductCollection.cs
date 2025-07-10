using System;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CustomerService.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class SystemProductCollection : RegistryBusinessObjectCollectionTemplate<SystemProduct>
	{
		public ModuleListType ProductCriticality { get; set; }

		public SystemProductCollection()
			: this(ModuleListType.Unspecified)
		{
		}

		public SystemProductCollection(ModuleListType productCriticality)
			: this(null, null)
		{
			this.ProductCriticality = productCriticality;
		}

		public SystemProductCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public SystemProduct AddNew(ZString code, ZString description, bool isProductReadOnly)
		{
			SystemProduct result = AddNew();
			result.Code = code;
			result.Description = description;
			result.IsProductReadOnly = isProductReadOnly;
			result.Enabled = true;
			result.ProductCriticality = ProductCriticality;
			return result;
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var collection = new SystemProductCollection(fallbackLevel, factory);
			collection.ProductCriticality = ProductCriticality;
			return collection;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var product = new SystemProduct(CurrentFallbackLevel, CurrentFactory);
			product.ProductCriticality = ProductCriticality;
			return product;
		}

		#endregion

		public CodeDescriptionPairList GetModuleList(string productCode, string productArea = "", bool excludeInternal = false, bool excludeDisabled = false)
		{
			var product = GetProductByCode(productCode);
			var factory = CurrentFactory ?? (CurrentFactory = new BusinessObjectFactory());

			return product != null
				? product.ModuleMappings.GetModuleList(factory, ProductCriticality, productCode, productArea, excludeInternal, excludeDisabled)
				: new CodeDescriptionPairList();
		}

		public CodeDescriptionPairList GetFullModuleList(bool excludeInternal = false, bool excludeDisabled = false)
		{
			var result = new CodeDescriptionPairList();
			foreach (SystemProduct product in this)
			{
				if (product.Enabled)
				{
					var modules = product.ModuleMappings.GetFullModuleList(string.Empty, excludeInternal, excludeDisabled);
					foreach (CodeDescriptionPair module in modules)
					{
						result.AddPair(module.Code, FormattableString.Invariant($"[{product.Description}] {module.Description}"));
					}
				}
			}
			return result;
		}

		public SystemProduct GetProductByCode(string productCode)
		{
			return this.Cast<SystemProduct>().FirstOrDefault(p => p.Code.EqualsIgnoringCase(productCode));
		}

		public ProductAreaModuleMapping GetMapping(string productCode, string code)
		{
			var product = GetProductByCode(productCode);

			return product != null
				? product.ModuleMappings.GetMapping(code)
				: null;
		}

		public ProductAreaModuleMapping GetMapping(string moduleCode)
		{
			if (moduleCode != null) {
				foreach (SystemProduct product in this)
				{
					if (product.Enabled)
					{
						if (product.ModuleMappings.GetMapping(moduleCode) != null)
						{
							return product.ModuleMappings.GetMapping(moduleCode);
						}
					}
				}
			}
			return null;
		}

		public ZString GetDescriptionFromCode(string productCode, string code)
		{
			var product = GetProductByCode(productCode);

			return product != null
				? product.ModuleMappings.GetDescriptionFromCode(code)
				: ZString.Empty;
		}

		public ServiceTypeProductAreaModuleMapping GetServiceTypeMapping(string productCode, string moduleCode)
		{
			var product = GetProductByCode(productCode);

			return product != null
				? product.ServiceTypeModuleMappings.GetMapping(moduleCode)
				: null;
		}
	}
}

