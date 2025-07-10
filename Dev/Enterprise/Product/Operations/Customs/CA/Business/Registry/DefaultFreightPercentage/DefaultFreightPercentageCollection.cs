using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.CA.Business.XmlSerializers")]
	public class DefaultFreightPercentageCollection : RegistryBusinessObjectCollectionTemplate
	{
		public DefaultFreightPercentageCollection()
		{
		}

		public DefaultFreightPercentageCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new DefaultFreightPercentage this[int index]
		{
			get { return (DefaultFreightPercentage)Elements[index]; }
		}

		public new DefaultFreightPercentage AddNew()
		{
			return (DefaultFreightPercentage)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultFreightPercentageCollection(factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DefaultFreightPercentage(CurrentFactory);
		}
	}
}
