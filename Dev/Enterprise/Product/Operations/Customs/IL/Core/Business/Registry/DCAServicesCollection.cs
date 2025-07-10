using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IL.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.IL.Business.XmlSerializers")]
	[XmlRoot("Services")]
	public class DCAServicesCollection : RegistryBusinessObjectCollectionTemplate
	{
		public DCAServicesCollection() { }

		public DCAServicesCollection(DCAParameters parameter, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
			this.parameter = parameter;
		}

		public DCAParameters Parameter
		{
			get { return parameter; }
		}
		DCAParameters parameter;

		public new DCAService this[int i]
		{
			get { return (DCAService)Elements[i]; }
		}

		public new DCAService AddNew()
		{
			return (DCAService)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DCAServicesCollection(Parameter, fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new DCAService(CurrentFallbackLevel, CurrentFactory);

		public DCAServicesCollection Clone(DCAParameters parameter, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = (DCAServicesCollection)Clone(fallbackLevel, factory);
			result.parameter = parameter;
			return result;
		}
	}
}
