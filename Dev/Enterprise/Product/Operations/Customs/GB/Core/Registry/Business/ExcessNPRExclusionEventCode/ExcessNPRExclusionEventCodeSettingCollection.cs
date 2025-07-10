using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.GB.Registry.XmlSerializers")]
	public class ExcessNPRExclusionEventCodeSettingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ExcessNPRExclusionEventCodeSettingCollection()
			: base()
		{
		}

		public ExcessNPRExclusionEventCodeSettingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ExcessNPRExclusionEventCodeSetting this[int i]
		{
			get { return (ExcessNPRExclusionEventCodeSetting)Elements[i]; }
		}

		public new ExcessNPRExclusionEventCodeSetting AddNew()
		{
			return (ExcessNPRExclusionEventCodeSetting)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ExcessNPRExclusionEventCodeSettingCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ExcessNPRExclusionEventCodeSetting(CurrentFallbackLevel, CurrentFactory);
		}
	}
}
