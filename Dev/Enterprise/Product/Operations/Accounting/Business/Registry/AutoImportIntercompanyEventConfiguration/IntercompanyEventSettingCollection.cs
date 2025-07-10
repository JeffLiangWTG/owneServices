using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class IntercompanyEventSettingCollection : RegistryBusinessObjectCollectionTemplate, IRegistrySettingCollection
	{
		public new IntercompanyEventSetting this[int i]
		{
			get { return (IntercompanyEventSetting)Elements[i]; }
		}

		public new IntercompanyEventSetting AddNew()
		{
			return (IntercompanyEventSetting)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new IntercompanyEventSetting();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new IntercompanyEventSettingCollection();
		}
	}
}
