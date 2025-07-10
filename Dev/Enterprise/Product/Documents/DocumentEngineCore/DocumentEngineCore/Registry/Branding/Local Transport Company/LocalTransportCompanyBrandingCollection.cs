using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class LocalTransportCompanyBrandingCollection : RegistryBusinessObjectCollection
	{
		public LocalTransportCompanyBrandingCollection() { }

		public LocalTransportCompanyBrandingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public new LocalTransportCompanyBranding this[int index]
		{
			get { return (LocalTransportCompanyBranding)Elements[index]; }
		}

		public new LocalTransportCompanyBranding AddNew()
		{
			return (LocalTransportCompanyBranding)base.AddNew();
		}

		#region Overrides

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var branding = child as LocalTransportCompanyBranding;

			if (branding != null)
			{
				branding.LabelName = LabelNames.DeliveryLabel;
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new LocalTransportCompanyBranding(CurrentFallbackLevel, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new LocalTransportCompanyBrandingCollection(fallbackLevel, factory);
		}

		#endregion
	}
}
