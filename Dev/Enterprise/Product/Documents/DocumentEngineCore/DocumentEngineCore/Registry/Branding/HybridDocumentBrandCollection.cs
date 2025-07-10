using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class HybridDocumentBrandCollection : ClientAndAgentBrandingCollection
	{
		public HybridDocumentBrandCollection()
		{
		}

		public HybridDocumentBrandCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new HybridDocumentBrand this[int i]
		{
			get { return (HybridDocumentBrand)Elements[i]; }
		}

		public new HybridDocumentBrand AddNew()
		{
			return (HybridDocumentBrand)base.AddNew();
		}

		public new HybridDocumentBrand FindByCode(string code)
		{
			return (HybridDocumentBrand)base.FindByCode(code);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HybridDocumentBrandCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new HybridDocumentBrand(CurrentFallbackLevel, CurrentFactory);
		}
	}
}
