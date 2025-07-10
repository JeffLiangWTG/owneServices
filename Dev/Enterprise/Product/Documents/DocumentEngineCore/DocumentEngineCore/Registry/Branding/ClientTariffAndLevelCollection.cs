using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class ClientTariffAndLevelCollection : DocumentBrandingCollection
	{
		public ClientTariffAndLevelCollection()
		{
		}

		public ClientTariffAndLevelCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ClientTariffAndLevel this[int i]
		{
			get { return (ClientTariffAndLevel)Elements[i]; }
		}

		public new ClientTariffAndLevel AddNew()
		{
			return (ClientTariffAndLevel)base.AddNew();
		}

		public new ClientTariffAndLevel FindByCode(string code)
		{
			return (ClientTariffAndLevel)base.FindByCode(code);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ClientTariffAndLevelCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ClientTariffAndLevel(CurrentFallbackLevel, CurrentFactory);
		}
	}
}
