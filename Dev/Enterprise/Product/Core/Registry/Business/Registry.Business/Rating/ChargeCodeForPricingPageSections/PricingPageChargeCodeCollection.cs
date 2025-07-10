using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[XmlRoot(ElementName = "PricingPageChargeCodes")]
	public class PricingPageChargeCodeCollection : RegistryBusinessObjectCollectionTemplate
	{
		public PricingPageChargeCodeCollection()
		{
		}

		public PricingPageChargeCodeCollection(ChargeCodeForPricingPageSectionsConfiguration parentConfiguration, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
			fParentConfiguration = parentConfiguration;
		}

		public new PricingPageChargeCodeGroup this[int i]
		{
			get { return (PricingPageChargeCodeGroup)Elements[i]; }
		}

		public new PricingPageChargeCodeGroup AddNew()
		{
			return (PricingPageChargeCodeGroup)base.AddNew();
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			var chargeType = bizOAdded as PricingPageChargeCodeGroup;
			if (chargeType != null)
			{
				chargeType.SetParentCollection(this);
			}
		}

		public PricingPageChargeCodeCollection Clone(ChargeCodeForPricingPageSectionsConfiguration parentConfiguration, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = (PricingPageChargeCodeCollection)Clone(fallbackLevel, factory);
			result.fParentConfiguration = parentConfiguration;

			return result;
		}

		public ChargeCodeForPricingPageSectionsConfiguration ParentConfiguration
		{
			get { return fParentConfiguration; }
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PricingPageChargeCodeCollection(null, fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PricingPageChargeCodeGroup(CurrentFallbackLevel, CurrentFactory, this);
		}

		ChargeCodeForPricingPageSectionsConfiguration fParentConfiguration;
	}
}
