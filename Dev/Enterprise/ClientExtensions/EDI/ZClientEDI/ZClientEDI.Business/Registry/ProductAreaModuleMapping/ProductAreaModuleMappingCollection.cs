using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.CustomerService.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ProductAreaModuleMappingCollection : RegistryBusinessObjectCollectionTemplate<ProductAreaModuleMapping>
	{
		public ProductAreaModuleMappingCollection()
			: this(null, null) { }

		public ProductAreaModuleMappingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public ProductAreaModuleMapping AddNew(ZString moduleCode, ZString moduleDescription, ZString productArea, bool isModuleReadOnly, bool isInternal = false, bool isEnabled = true)
		{
			ProductAreaModuleMapping result = AddNew();
			result.ModuleCode = moduleCode;
			result.ModuleDescription = moduleDescription;
			result.ProductArea = productArea;
			result.IsModuleReadOnly = isModuleReadOnly;
			result.IsInternal = isInternal;
			result.IsEnabled = isEnabled;
			return result;
		}

		public ProductAreaModuleMapping AddIfNotExists(ZString moduleCode, ZString moduleDescription, ZString productArea, bool isModuleReadOnly)
		{
			if (!ContainsCode(moduleCode))
			{
				return AddNew(moduleCode, moduleDescription, productArea, isModuleReadOnly);
			}

			return null;
		}

		#region GetModuleList

		public CodeDescriptionPairList GetFullModuleList(string productArea = "", bool excludeInternal = false, bool excludeDisabled = false)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			var sourceModules = EDIDataRegistry.Instance.SourceModules.Value;
			foreach (ProductAreaModuleMapping mapping in this)
			{
				if ((string.IsNullOrEmpty(productArea)
					|| string.Equals(mapping.ProductArea, productArea, StringComparison.OrdinalIgnoreCase)
					|| mapping.SourceModuleMappings.Cast<ProductAreaSourceModuleMapping>().Any(x => string.Equals(x.ProductArea, productArea, StringComparison.OrdinalIgnoreCase)))
					&& !(excludeInternal && mapping.IsInternal) && !(excludeDisabled && !mapping.IsEnabled))
				{
					result.AddPair(mapping.ModuleCode, mapping.ModuleDescriptionMultilingual);
				}
			}
			return result;
		}

		public CodeDescriptionPairList GetModuleList(BusinessObjectFactory factory, ModuleListType moduleListType, string product, string productArea, bool excludeInternal = false, bool excludeDisabled = false)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			var overrideableSourceModuleCodes = GetOverrideableSourceModuleCodes(factory, moduleListType, product);
			foreach (ProductAreaModuleMapping mapping in this)
			{
				if ((string.IsNullOrEmpty(productArea)
					|| string.Equals(mapping.ProductArea, productArea, StringComparison.OrdinalIgnoreCase)
					|| mapping.SourceModuleMappings.Cast<ProductAreaSourceModuleMapping>()
						.Any(x => string.Equals(x.ProductArea, productArea, StringComparison.OrdinalIgnoreCase) && overrideableSourceModuleCodes.Contains(x.Code)))
					&& !(excludeInternal && mapping.IsInternal) && !(excludeDisabled && !mapping.IsEnabled))
				{
					result.AddPair(mapping.ModuleCode, mapping.ModuleDescriptionMultilingual);
				}
			}
			return result;
		}

		HashSet<ZString> GetOverrideableSourceModuleCodes(BusinessObjectFactory factory, ModuleListType moduleListType, string product)
		{
			return factory.GetCachedValue($"ProductAreaModuleMappingCollection.GetOverrideableSourceModuleCodes_{moduleListType}_{product}",
				() =>
				{
					return EDIDataRegistry.Instance.SourceModules.Value.Cast<SourceModule>()
							.Where(x => (x.ModuleListType == moduleListType || moduleListType == ModuleListType.Unspecified) && string.Equals(x.Product, product, StringComparison.OrdinalIgnoreCase) && x.IsSelectableForOverride)
							.Select(x => x.Code).Distinct().ToHashSet();
				});
		}

		#endregion

		#region ContainsCode

		public ZBool ContainsCode(string code)
		{
			return GetMapping(code) != null;
		}

		public ProductAreaModuleMapping GetMapping(string code)
		{
			foreach (ProductAreaModuleMapping mapping in this)
			{
				if (string.Equals(mapping.ModuleCode, code, StringComparison.OrdinalIgnoreCase))
				{
					return mapping;
				}
			}

			return null;
		}

		#endregion

		#region GetCodeFromDescription

		public ZString GetCodeFromDescription(string description)
		{
			foreach (ProductAreaModuleMapping mapping in this)
			{
				if (string.Equals(mapping.ModuleDescription, description, StringComparison.OrdinalIgnoreCase))
				{
					return mapping.ModuleCode;
				}
			}

			return ZString.Empty;
		}

		#endregion

		#region GetDescriptionFromCode

		public ZString GetDescriptionFromCode(string code)
		{
			foreach (ProductAreaModuleMapping mapping in this)
			{
				if (string.Equals(mapping.ModuleCode, code, StringComparison.OrdinalIgnoreCase))
				{
					return mapping.ModuleDescription;
				}
			}

			return ZString.Empty;
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ProductAreaModuleMappingCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ProductAreaModuleMapping(CurrentFallbackLevel, CurrentFactory);
		}

		#endregion
	}
}

