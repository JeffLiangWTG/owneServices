using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ChargeCodeForPricingPageSectionsConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ChargeCodeForPricingPageSectionsConfigurationCollection()
			: this(null, null)
		{
		}

		public ChargeCodeForPricingPageSectionsConfigurationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
			=> new ChargeCodeForPricingPageSectionsConfiguration(CurrentFallbackLevel, CurrentFactory);

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new ChargeCodeForPricingPageSectionsConfigurationCollection(fallbackLevel, factory);

		public new ChargeCodeForPricingPageSectionsConfiguration this[int i]
			=> (ChargeCodeForPricingPageSectionsConfiguration)Elements[i];

		public new ChargeCodeForPricingPageSectionsConfiguration AddNew()
			=> (ChargeCodeForPricingPageSectionsConfiguration)base.AddNew();
	}
}
