using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.DE.Business.XmlSerializers")]
	public class MessageVersionRegistryCollection : RegistryBusinessObjectCollectionTemplate
	{
		public MessageVersionRegistryCollection()
			: base()
		{
		}

		public MessageVersionRegistryCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public MessageVersionRegistryCollection DefaultCollection
		{
			get
			{
				if (defaultCollection == null)
				{
					defaultCollection = new MessageVersionRegistryCollection()
					{
						new MessageVersionRegistry() { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = MessageVersionRegistry.AtlasDefaultVersionNumber },
						new MessageVersionRegistry() { SystemCode = MessageVersionRegistry.AESSystemCode, VersionNumber = MessageVersionRegistry.AESDefaultVersionNumber },
						new MessageVersionRegistry() { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = MessageVersionRegistry.EmcsDefaultVersionNumber }
					};
				}
				return defaultCollection;
			}
		}
		MessageVersionRegistryCollection defaultCollection;

		public new MessageVersionRegistry this[int i] => (MessageVersionRegistry)Elements[i];

		public new MessageVersionRegistry AddNew() => (MessageVersionRegistry)base.AddNew();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new MessageVersionRegistry(CurrentFallbackLevel, CurrentFactory);

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new MessageVersionRegistryCollection(fallbackLevel, factory);

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
