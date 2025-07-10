using System;
using System.Reflection;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class HybridDocumentBrand : ClientAndAgentBrandingBusinessObject
	{
		public HybridDocumentBrand()
		{
		}

		public HybridDocumentBrand(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HybridDocumentBrand(fallbackLevel, factory);
		}

		protected override FallbackLevel CurrentFallbackLevelCore
		{
			get { return base.CurrentFallbackLevelCore; }
			set
			{
				base.CurrentFallbackLevelCore = value;
				ClearCodeList();
			}
		}

		internal bool IsAgentBranded
		{
			get
			{
				bool result = true; // By default the registry is Agent Branded

				if (CurrentFallbackLevel != null)
				{
					RegistryItemProposedValueAccessor retriever = new RegistryItemProposedValueAccessor(DocumentsDataRegistry.Instance.HBLAndHAWBBrandingOption, CurrentFallbackLevel);
					result = ((string)retriever.GetFallBackValue().Value == HBLAndHAWBBrandingOptionEditorInfo.AgentBranded);
				}

				return result;
			}
		}

		protected override IRegistryItem BrandingOptionRegistryItem
		{
			get { return IsAgentBranded ? DocumentsDataRegistry.Instance.EnableAgentBranding : DocumentsDataRegistry.Instance.EnableClientBranding; }
		}

		protected override string BrandingOptionTitle
		{
			get { return IsAgentBranded ? (NoResString)"Agent Branding" : (NoResString)"Client Branding"; }
		}

		protected override CodeDescriptionPairList GetNewCodeList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			if (IsAgentBranded)
			{
				result = new CodeDescriptionPairList(Env.Registry.AgentCategoryList);
			}
			else
			{
				Assembly assembly = Assembly.Load("Enterprise.MasterFiles.Business");

				Type orgCodeListsType = assembly.GetType("Enterprise.MasterFiles.Business.OrgCodeLists");
				object orgCodeLists = Activator.CreateInstance(orgCodeListsType);

				MethodInfo info = orgCodeListsType.GetMethod("DefaultCompanyTariffLevels_List", BindingFlags.Public | BindingFlags.Instance);
				result = (CodeDescriptionPairList)info.Invoke(orgCodeLists, new object[] { CurrentFactory });
			}

			return result;
		}
	}
}
