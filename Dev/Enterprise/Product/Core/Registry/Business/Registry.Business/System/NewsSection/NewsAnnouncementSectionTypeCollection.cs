using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class NewsAnnouncementSectionTypeCollection : RegistryBusinessObjectCollectionTemplate<NewsAnnouncementSectionType>
	{
		public NewsAnnouncementSectionTypeCollection()
			: base(null, null)
		{
		}

		public NewsAnnouncementSectionTypeCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new NewsAnnouncementSectionTypeCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new NewsAnnouncementSectionType(CurrentFallbackLevel, CurrentFactory);
		}

		protected override bool AllowNewCore => true;

		protected override bool AllowRemoveCore => true;
	}
}
