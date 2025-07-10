using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.BE.Business;

[XmlSerializerAssembly("Enterprise.Customs.BE.Business.XmlSerializers")]
public class MessageVersionRegistryCollection : RegistryBusinessObjectCollectionTemplate
{
	public MessageVersionRegistryCollection()
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
				defaultCollection = new MessageVersionRegistryCollection
				{
					new MessageVersionRegistry { DomainCode = MessageVersionRegistry.NCTSP5DomainCode, TargetSystemName = MessageVersionRegistry.NCTSP5DefaultTarget },
					new MessageVersionRegistry { DomainCode = MessageVersionRegistry.IDMSDomainCode, TargetSystemName = MessageVersionRegistry.IDMSDefaultTarget },
					new MessageVersionRegistry { DomainCode = MessageVersionRegistry.AESDomainCode, TargetSystemName = MessageVersionRegistry.AESDefaultTarget },
					new MessageVersionRegistry { DomainCode = MessageVersionRegistry.PNTSDomainCode, TargetSystemName = MessageVersionRegistry.PNTSDefaultTarget },
					new MessageVersionRegistry { DomainCode = MessageVersionRegistry.TSDDomainCode, TargetSystemName = MessageVersionRegistry.TSDDefaultTarget },
					new MessageVersionRegistry { DomainCode = MessageVersionRegistry.RENDomainCode, TargetSystemName = MessageVersionRegistry.RENDefaultTarget },
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
