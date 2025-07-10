using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class PeriodReopenLevelsCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new PeriodReopenLevels this[int x]
		{
			get { return (PeriodReopenLevels)base[x]; }
		}

		public new PeriodReopenLevels AddNew()
		{
			return (PeriodReopenLevels)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PeriodReopenLevelsCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PeriodReopenLevels();
		}
	}
}
