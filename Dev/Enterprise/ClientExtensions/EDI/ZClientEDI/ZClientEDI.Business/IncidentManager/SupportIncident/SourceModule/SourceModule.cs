using System;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Core.Modules;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SourceModule : AutoSourceModule
	{
		public SourceModule()
		{
			ModuleTypeCode = nameof(ModuleListType.MenuSection);

			base.IsSelectableForOverride = true;
			base.IsSearchable = true;
		}

		public static string GetPath(IMainFormModule mainFormModule)
		{
			var parentSection = mainFormModule.ParentSection;
			var parentCategory = parentSection.ParentCategory;
			return string.Format(CultureInfo.CurrentCulture, "{0} > {1} >", parentCategory.UnresolvedDisplayTextWithoutAmpersand, parentSection.UnresolvedDisplayTextWithoutAmpersand);
		}

		public static bool IsModuleTypeCompatible(ModuleListType lookupModuleType, ModuleListType sourceModuleType)
		{
			if (lookupModuleType == ModuleListType.MenuSection)
			{
				return sourceModuleType == ModuleListType.MenuSection || sourceModuleType == ModuleListType.DetectedMenuItem;
			}
			else
			{
				return lookupModuleType == sourceModuleType;
			}
		}

		#region Properties

		#region Product

		[List("ProductList")]
		public override ZString Product
		{
			get { return base.Product; }
			set { base.Product = value; }
		}

		public bool Product_ReadOnly => IsDetectedMenuItem;

		#endregion

		#region ModuleListType

		public ModuleListType ModuleListType
		{
			get
			{
				ModuleListType result;
				if (!Enum.TryParse(ModuleTypeCode, out result))
				{
					result = ModuleListType.Unspecified;
				}

				return result;
			}
			set { ModuleTypeCode = value.ToString(); }
		}

		[List("ModuleListTypeDescriptions")]
		[MaxLength(50)]
		public ZString ModuleListTypeDescription
		{
			get { return ModuleListTypeDescriptions.GetCodeFromDescription(ModuleTypeCode); }
			set { ModuleTypeCode = ModuleListTypeDescriptions.GetDescriptionFromCode(value); }
		}

		public bool ModuleListTypeDescription_ReadOnly => IsDetectedMenuItem;

		public ZWrappedPropertyInfo ModuleListTypeDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ModuleListTypeDescription), x => ModuleTypeCodeInfo); }
		}

		#endregion

		#region DefaultModule

		[List("ModulesList")]
		public ZString DefaultModule
		{
			get { return base.DefaultMenuSection; }
			set { base.DefaultMenuSection = value; }
		}

		public ZPropertyInfo DefaultModuleInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DefaultModule), x => DefaultMenuSectionInfo); }
		}

		public ZString DefaultModuleDescription
		{
			get { return MenuSectionList.GetDescriptionFromCode(DefaultModule); }
		}

		#endregion

		public bool Code_ReadOnly => IsDetectedMenuItem;

		public bool Description_ReadOnly => IsDetectedMenuItem;

		public bool Path_ReadOnly => IsDetectedMenuItem;

		#endregion

		#region Lookups

		#region ModuleListTypeDescriptions

		public CodeDescriptionPairList ModuleListTypeDescriptions
		{
			get
			{
				if (moduleListTypes == null)
				{
					moduleListTypes = new CodeDescriptionPairList();
					foreach (var moduleListType in (ModuleListType[])Enum.GetValues(typeof(ModuleListType)))
					{
						if (IsDetectedMenuItem)
						{
							if (moduleListType == ModuleListType.DetectedMenuItem)
							{
								moduleListTypes.AddPair(EnumExtensions.GetCaption(moduleListType),
									moduleListType.ToString());
							}
						}
						else if (moduleListType != ModuleListType.Unspecified && moduleListType != ModuleListType.DetectedMenuItem)
						{
							moduleListTypes.AddPair(EnumExtensions.GetCaption(moduleListType),
								moduleListType.ToString());
						}
					}
				}

				return moduleListTypes;
			}
		}
		CodeDescriptionPairList moduleListTypes;

		#endregion

		#region ModulesList

		public ICodeDescriptionPairList ModulesList
		{
			get
			{
				switch (ModuleListType)
				{
					case ModuleListType.MenuSection:
					case ModuleListType.DetectedMenuItem:
						return MenuSectionList;

					case ModuleListType.Cr8:
						return Cr8ModuleList;

					case ModuleListType.Cr9:
						return Cr9ModuleList;

					default:
						return new CodeDescriptionPairList();
				}
			}
		}

		public ICodeDescriptionPairList MenuSectionList
		{
			get
			{
				return EDIDataRegistry.Instance.SystemProductMappings.Value.GetModuleList(Product,
					excludeDisabled: true);
			}
		}

		public ICodeDescriptionPairList Cr8ModuleList
		{
			get
			{
				return EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.Value.GetModuleList(Product,
					excludeDisabled: true);
			}
		}

		public ICodeDescriptionPairList Cr9ModuleList
		{
			get
			{
				return EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.Value.GetModuleList(Product,
					excludeDisabled: true);
			}
		}

		#endregion

		#region ProductList

		CodeDescriptionPairList productList;

		public ICodeDescriptionPairList ProductList
		{
			get
			{
				if (productList == null)
				{
					productList = new CodeDescriptionPairList();
					foreach (SystemProduct product in EDIDataRegistry.Instance.SystemProductMappings.Value)
					{
						productList.Add(new CodeDescriptionPair(product.Code.ToString(), product.Description));
					}
				}

				return productList;
			}
		}

		#endregion

		#endregion

		#region Validation

		public override void ValidateProduct()
		{
			base.ValidateProduct();
			ListValidation.ErrorIfInvalidCode(ProductInfo, ProductList);
		}

		public override void ValidateCode()
		{
			base.ValidateCode();
			MandatoryValidation.CheckEntered(CodeInfo);
		}

		public override void ValidateDescription()
		{
			base.ValidateDescription();
			MandatoryValidation.CheckEntered(DescriptionInfo);
		}

		public override void ValidateModuleTypeCode()
		{
			base.ValidateModuleTypeCode();
			MandatoryValidation.CheckEntered(ModuleTypeCodeInfo);
		}

		public override void ValidateDefaultMenuSection()
		{
			base.ValidateDefaultMenuSection();
			ProductAreaModuleMapping mapping;
			switch (ModuleListType)
			{
				case ModuleListType.MenuSection:
				case ModuleListType.DetectedMenuItem:
					mapping = EDIDataRegistry.Instance.SystemProductMappings.Value.GetMapping(DefaultMenuSection);
					break;
				case ModuleListType.Cr8:
					mapping =
						EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.Value.GetMapping(DefaultMenuSection);
					break;
				case ModuleListType.Cr9:
					mapping =
						EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.Value.GetMapping(DefaultMenuSection);
					break;
				default:
					mapping = null;
					break;
			}

			if (mapping != null && mapping.IsInternal)
			{
				DefaultMenuSectionInfo.AddWarning("Section code is internal only");
			}
		}

		#endregion

		bool IsDetectedMenuItem => ModuleListType == ModuleListType.DetectedMenuItem;
	}
}

