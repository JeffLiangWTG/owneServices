using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business;

[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
public class SystemDefinableRegistryImageCollection : RegistryImageCollection
{
	public new SystemDefinableRegistryImage this[int i]
	{
		get { return (SystemDefinableRegistryImage)Elements[i]; }
	}

	public new SystemDefinableRegistryImage AddNew()
	{
		return (SystemDefinableRegistryImage)base.AddNew();
	}

	protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
	{
		return new SystemDefinableRegistryImageCollection();
	}

	protected override BusinessObject CreateNonPersistentBusinessObject()
	{
		return new SystemDefinableRegistryImage();
	}

	public SystemDefinableRegistryImage FindByCodeAndSystemDefined(string code, bool systemDefined)
	{
		return Elements
			.Cast<SystemDefinableRegistryImage>()
			.FirstOrDefault(x => x.Code.EqualsIgnoringCase(code) && x.SystemDefined == systemDefined);
	}
}
