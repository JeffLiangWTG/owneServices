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
	public class AgentDocumentBrand : DocumentBrandingBusinessObject
	{
		public AgentDocumentBrand()
		{
		}

		public AgentDocumentBrand(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AgentDocumentBrand(fallbackLevel, factory);
		}

		protected override string BrandingOptionTitle
		{
			get { return (NoResString)"Agent Branding"; }
		}

		protected override IRegistryItem BrandingOptionRegistryItem
		{
			get { return DocumentsDataRegistry.Instance.EnableAgentBranding; }
		}

		protected override CodeDescriptionPairList GetNewCodeList()
		{
			return new CodeDescriptionPairList(Env.Registry.AgentCategoryList);
		}
	}
}
