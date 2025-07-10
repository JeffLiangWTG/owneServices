using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.CustomerService;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class ModuleListBuilder : IModuleListBuilder
	{
		public static class Codes
		{
			public const string All = "ALL";
		}

		public static class Descriptions
		{
			public const string All = "All Items";
		}

		public CodeDescriptionPairList Build(ModuleListType moduleListType, ZString product, ZString productArea)
		{
			return BuildCore(moduleListType, product, productArea);
		}

		public CodeDescriptionPairList Build(ModuleListType moduleListType, ZString product, ZString productArea, bool excludeInternal, bool excludeDisabled)
		{
			return BuildCore(moduleListType, product, productArea, excludeInternal, excludeDisabled);
		}

		public IEnumerable<Tuple<string, string, string>> BuildWithCriticality(ModuleListType moduleListType, ZString product, ZString productArea, bool excludeInternal, bool excludeDisabled)
		{
			var moduleList = new List<Tuple<string, string, string>>();
			var basicList = BuildBasicList(moduleListType, product, productArea, excludeInternal, excludeDisabled);

			foreach (CodeDescriptionPair codeDescriptionPair in basicList)
			{
				moduleList.Add(new Tuple<string, string, string>(string.Empty, codeDescriptionPair.Code, codeDescriptionPair.Description));
			}

			if (moduleListType == ModuleListType.Unspecified || moduleListType == ModuleListType.Cr8)
			{
				var cr8List = BuildCr8List(moduleListType, product, productArea, excludeInternal, excludeDisabled);

				foreach (CodeDescriptionPair codeDescriptionPair in cr8List)
				{
					moduleList.Add(new Tuple<string, string, string>(CriticalityCodes.CR8_ComplianceRequirement, codeDescriptionPair.Code, codeDescriptionPair.Description));
				}
			}

			if (moduleListType == ModuleListType.Unspecified || moduleListType == ModuleListType.Cr9)
			{
				var cr9List = BuildCr9List(moduleListType, product, productArea, excludeInternal, excludeDisabled);

				foreach (CodeDescriptionPair codeDescriptionPair in cr9List)
				{
					moduleList.Add(new Tuple<string, string, string>(CriticalityCodes.CR9_CustomerServiceRequest, codeDescriptionPair.Code, codeDescriptionPair.Description));
				}
			}

			moduleList.OrderBy(x => x.Item3);

			return moduleList;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected virtual CodeDescriptionPairList BuildCore(ModuleListType moduleListType, ZString product, ZString productArea, bool excludeInternal = false, bool excludeDisabled = false)
		{
			var list = BuildBasicList(moduleListType, product, productArea, excludeInternal, excludeDisabled);

			if (moduleListType == ModuleListType.Unspecified || moduleListType == ModuleListType.Cr8)
			{
				list.AddRange(BuildCr8List(moduleListType, product, productArea, excludeInternal, excludeDisabled));
			}

			if (moduleListType == ModuleListType.Unspecified || moduleListType == ModuleListType.Cr9)
			{
				list.AddRange(BuildCr9List(moduleListType, product, productArea, excludeInternal, excludeDisabled));
			}

			list.SortByDescription();

			return list;
		}

		CodeDescriptionPairList BuildBasicList(ModuleListType moduleListType, ZString product, ZString productArea, bool excludeInternal, bool excludeDisabled)
		{
			var list = new CodeDescriptionPairList();

			if (ShouldIncludeAllModule(moduleListType, product, productArea))
			{
				list.Add(new CodeDescriptionPair(Codes.All, (NoResString)Descriptions.All));
			}

			if (moduleListType == ModuleListType.Unspecified || moduleListType == ModuleListType.MenuSection)
			{
				if (product.IsEmpty || product == ProductTypes.Codes.Enterprise || product == ProductTypes.Codes.EHub)
				{
					var modules = GetNewEnterpriseMenuSectionListBuilder().Build(ProductTypes.Codes.Enterprise, productArea, excludeInternal, excludeDisabled);
					if (product.IsEmpty)
					{
						foreach (CodeDescriptionPair module in modules)
						{
							list.AddPair(module.Code, FormattableString.Invariant($"[{ProductTypes.Descriptions.EnterpriseCW1}] {module.Description}"));
						}
					}
					else
					{
						list.AddRange(modules);
					}
				}

				if (!product.IsEmpty && (product != ProductTypes.Codes.Enterprise))
				{
					list.AddRange(EDIDataRegistry.Instance.SystemProductMappings.Value.GetModuleList(product, productArea, excludeInternal, excludeDisabled));
				}

				if (product.IsEmpty)
				{
					foreach (SystemProduct systemProduct in EDIDataRegistry.Instance.SystemProductMappings.Value)
					{
						if (systemProduct.Code != ProductTypes.Codes.Enterprise && systemProduct.Code != ProductTypes.Codes.EHub)
						{
							foreach (ProductAreaModuleMapping module in systemProduct.ModuleMappings)
							{
								var excludeInternalConstraintViolated = excludeInternal && module.IsInternal;
								var excludeDisableConstraintViolated = excludeDisabled && !module.IsEnabled;
								if (!excludeInternalConstraintViolated && !excludeDisableConstraintViolated)
								{
									list.Add(new CodeDescriptionPair(module.ModuleCode.ToString(), FormattableString.Invariant($"[{systemProduct.Description}] {module.ModuleDescriptionMultilingual}")));
								}
							}
						}
					}
				}
			}

			return list;
		}

		CodeDescriptionPairList BuildCr8List(ModuleListType moduleListType, ZString product, ZString productArea, bool excludeInternal, bool excludeDisabled)
		{
			if (moduleListType != ModuleListType.Unspecified && moduleListType != ModuleListType.Cr8)
			{
				return new CodeDescriptionPairList();
			}

			if (product.IsEmpty)
			{
				return EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.Value.GetFullModuleList(excludeInternal, excludeDisabled);
			}
			else
			{
				return EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.Value.GetModuleList(product, productArea, excludeInternal, excludeDisabled);
			}
		}

		CodeDescriptionPairList BuildCr9List(ModuleListType moduleListType, ZString product, ZString productArea, bool excludeInternal, bool excludeDisabled)
		{
			if (moduleListType != ModuleListType.Unspecified && moduleListType != ModuleListType.Cr9)
			{
				return new CodeDescriptionPairList();
			}

			if (product.IsEmpty)
			{
				return EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.Value.GetFullModuleList(excludeInternal, excludeDisabled);
			}
			else
			{
				return EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.Value.GetModuleList(product, productArea, excludeInternal, excludeDisabled);
			}
		}

		protected virtual bool ShouldIncludeAllModule(ModuleListType moduleListType, ZString product, ZString productArea)
		{
			return false;
		}

		protected internal virtual IEnterpriseMenuSectionListBuilder GetNewEnterpriseMenuSectionListBuilder()
		{
			return new EnterpriseMenuSectionListBuilder();
		}
	}
}

