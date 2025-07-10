using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CustomerService.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ServiceTypeProductAreaModuleMappingCollection : RegistryBusinessObjectCollectionTemplate<ServiceTypeProductAreaModuleMapping>
	{
		public string ProductCode { get; set; }
		public ModuleListType ProductCriticality { get; set; }

		public ServiceTypeProductAreaModuleMappingCollection()
			: this(null, ModuleListType.Unspecified)
		{
		}

		public ServiceTypeProductAreaModuleMappingCollection(string productCode, ModuleListType productCriticality)
			: this(null, null)
		{
			ProductCode = productCode;
			ProductCriticality = productCriticality;
		}

		public ServiceTypeProductAreaModuleMappingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public ServiceTypeProductAreaModuleMapping AddNew(ZString moduleCode, ZString moduleDescription, ZString productArea, bool isInternal = false, bool isEnabled = true)
		{
			var result = AddNew();
			result.ModuleCode = moduleCode;
			result.ModuleDescription = moduleDescription;
			result.ProductArea = productArea;
			result.IsInternal = isInternal;
			result.IsEnabled = isEnabled;
			return result;
		}

		public ServiceTypeProductAreaModuleMapping AddIfNotExists(ZString moduleCode, ZString moduleDescription, ZString productArea)
		{
			if (!ContainsCode(moduleCode))
			{
				return AddNew(moduleCode, moduleDescription, productArea);
			}

			return null;
		}

		#region GetModuleList

		public CodeDescriptionPairList GetModuleList(string productArea = "", bool excludeInternal = false, bool excludeDisabled = false)
		{
			var result = new CodeDescriptionPairList();
			foreach (ServiceTypeProductAreaModuleMapping mapping in this)
			{
				if ((string.IsNullOrEmpty(productArea)
					|| string.Equals(mapping.ProductArea, productArea, StringComparison.OrdinalIgnoreCase)
					|| mapping.ServiceTypeMappings.Cast<ServiceTypeProductAreaModuleMapping>().Any(x => string.Equals(x.ProductArea, productArea, StringComparison.OrdinalIgnoreCase)))
					&& !(excludeInternal && mapping.IsInternal) && !(excludeDisabled && !mapping.IsEnabled))
				{
					result.AddPair(mapping.ModuleCode, mapping.ModuleDescription);
				}
			}
			return result;
		}

		#endregion

		#region ContainsCode

		public ZBool ContainsCode(string code)
		{
			return GetMapping(code) != null;
		}

		public ServiceTypeProductAreaModuleMapping GetMapping(string code)
		{
			foreach (ServiceTypeProductAreaModuleMapping mapping in this)
			{
				if (string.Equals(mapping.ModuleCode, code, StringComparison.OrdinalIgnoreCase))
				{
					return mapping;
				}
			}

			return null;
		}

		#endregion

		protected override bool RunPreSaveValidationCore()
		{
			var result = base.RunPreSaveValidationCore();
			if (result)
			{
				result = ValidateDuplicates();
			}
			return result;
		}

		bool ValidateDuplicates()
		{
			var validationList = new List<string>();
			foreach (var item in this)
			{
				if (!item.ProductArea.IsEmpty && !item.ModuleCode.IsEmpty)
				{
					var value = item.ProductArea + "-" + item.ModuleCode;
					if (validationList.Contains(value))
					{
						var errorMessage = Res.GetString("968bf18e-bfd7-4fa2-8c30-6906bf6132a3", "Product '{0}' already have a module code '{1}'", item.ProductArea, item.ModuleCode);
						item.ProductAreaInfo.AddError(errorMessage);
						item.ModuleCodeInfo.AddError(errorMessage);
					}
					else
					{
						validationList.Add(value);
					}
				}
			}
			return true;
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ServiceTypeProductAreaModuleMappingCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ServiceTypeProductAreaModuleMapping(CurrentFallbackLevel, CurrentFactory, ProductCode, ProductCriticality);
		}

		#endregion
	}
}

