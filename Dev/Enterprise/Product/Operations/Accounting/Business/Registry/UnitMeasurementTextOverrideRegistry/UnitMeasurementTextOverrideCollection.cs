using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class UnitMeasurementTextOverrideCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new UnitMeasurementTextOverride this[int i]
		{
			get { return (UnitMeasurementTextOverride)base[i]; }
		}

		public new UnitMeasurementTextOverride AddNew()
		{
			return (UnitMeasurementTextOverride)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new UnitMeasurementTextOverride();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new UnitMeasurementTextOverrideCollection();
		}
	}
}
