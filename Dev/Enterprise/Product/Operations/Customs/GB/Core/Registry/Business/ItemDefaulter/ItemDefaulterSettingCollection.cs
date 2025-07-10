using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.GB.Registry.XmlSerializers")]
	public class ItemDefaulterSettingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ItemDefaulterSettingCollection()
			: base()
		{
		}

		public ItemDefaulterSettingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ItemDefaulterSetting this[int i]
		{
			get { return (ItemDefaulterSetting)Elements[i]; }
		}

		public new ItemDefaulterSetting AddNew()
		{
			return (ItemDefaulterSetting)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ItemDefaulterSettingCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ItemDefaulterSetting(CurrentFallbackLevel, CurrentFactory);
		}
	}
}
