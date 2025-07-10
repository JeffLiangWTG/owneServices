using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class AgentDocumentBrandCollection : DocumentBrandingCollection
	{
		public AgentDocumentBrandCollection()
		{
		}

		public AgentDocumentBrandCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new AgentDocumentBrand this[int i]
		{
			get { return (AgentDocumentBrand)Elements[i]; }
		}

		public new AgentDocumentBrand AddNew()
		{
			return (AgentDocumentBrand)base.AddNew();
		}

		public new AgentDocumentBrand FindByCode(string code)
		{
			return (AgentDocumentBrand)base.FindByCode(code);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AgentDocumentBrandCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AgentDocumentBrand(CurrentFallbackLevel, CurrentFactory);
		}
	}
}
