using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.BE.Business;

[XmlSerializerAssembly("Enterprise.Customs.BE.Business.XmlSerializers")]
public class CustomsRegistryCollection : RegistryBusinessObjectCollectionTemplate
{
	public CustomsRegistryCollection()
	{
	}

	public CustomsRegistryCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(fallbackLevel, factory)
	{
	}

	public CustomsRegistryCollection DefaultCollection => defaultCollection ?? (defaultCollection = new CustomsRegistryCollection());

	CustomsRegistryCollection defaultCollection;

	public new CustomsRegistry this[int i] => (CustomsRegistry)Elements[i];

	public new CustomsRegistry AddNew() => (CustomsRegistry)base.AddNew();

	protected override BusinessObject CreateNonPersistentBusinessObject() => new CustomsRegistry(CurrentFallbackLevel, CurrentFactory);

	protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new CustomsRegistryCollection(fallbackLevel, factory);

	protected override bool AllowNewCore => true;

	protected override bool AllowRemoveCore => true;
}
