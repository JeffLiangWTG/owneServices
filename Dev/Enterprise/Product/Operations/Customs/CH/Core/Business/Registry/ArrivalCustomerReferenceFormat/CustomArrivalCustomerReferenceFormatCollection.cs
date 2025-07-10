using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CH.Business;

[XmlSerializerAssembly("Enterprise.Customs.CH.Business.XmlSerializers")]
public sealed class CustomArrivalCustomerReferenceFormatCollection : RegistryBusinessObjectCollectionTemplate
{
	public CustomArrivalCustomerReferenceFormatCollection()
	{
	}

	public CustomArrivalCustomerReferenceFormatCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
	{
	}

	public new CustomArrivalCustomerReferenceFormat this[int index]
	{
		get { return (CustomArrivalCustomerReferenceFormat)base[index]; }
	}

	public new CustomArrivalCustomerReferenceFormat AddNew() => (CustomArrivalCustomerReferenceFormat)base.AddNew();

	protected override BusinessObject CreateNonPersistentBusinessObject()
	{
		return new CustomArrivalCustomerReferenceFormat(CurrentFallbackLevel, CurrentFactory);
	}

	protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
	{
		return new CustomArrivalCustomerReferenceFormatCollection(fallbackLevel, factory);
	}
}
