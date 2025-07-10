using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.AU.Declaration.Business.XmlSerializers")]
	public class DefaultPremiseIDCollection : RegistryBusinessObjectCollectionTemplate
	{
		public DefaultPremiseIDCollection()
		{
		}

		public DefaultPremiseIDCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new DefaultPremiseID this[int index]
		{
			get { return (DefaultPremiseID)Elements[index]; }
		}

		public new DefaultPremiseID AddNew()
		{
			return (DefaultPremiseID)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultPremiseIDCollection(factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DefaultPremiseID(CurrentFactory);
		}
	}
}
