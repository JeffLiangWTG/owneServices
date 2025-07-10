using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JobClosureConfigurationCollection : RegistryBusinessObjectCollectionTemplate, IRegistrySettingCollection
	{
		public new JobClosureConfiguration this[int i]
		{
			get { return (JobClosureConfiguration)Elements[i]; }
		}

		public new JobClosureConfiguration AddNew()
		{
			return (JobClosureConfiguration)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JobClosureConfigurationCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new JobClosureConfiguration();
		}
	}
}

