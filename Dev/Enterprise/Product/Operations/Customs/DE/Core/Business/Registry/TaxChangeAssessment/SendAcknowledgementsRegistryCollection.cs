using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.DE.Business.XmlSerializers")]
	public class SendAcknowledgementsRegistryCollection : RegistryBusinessObjectCollectionTemplate
	{
		public SendAcknowledgementsRegistryCollection()
			: base()
		{
		}

		public SendAcknowledgementsRegistryCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new SendAcknowledgementsRegistry this[int i] => (SendAcknowledgementsRegistry)Elements[i];

		public new SendAcknowledgementsRegistry AddNew() => (SendAcknowledgementsRegistry)base.AddNew();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new SendAcknowledgementsRegistry(CurrentFallbackLevel, CurrentFactory);

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new SendAcknowledgementsRegistryCollection(fallbackLevel, factory);

		protected override bool AllowNewCore => true;

		protected override bool AllowRemoveCore => true;
	}
}
