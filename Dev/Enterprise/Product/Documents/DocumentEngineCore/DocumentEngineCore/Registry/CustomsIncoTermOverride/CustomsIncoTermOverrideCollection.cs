using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class CustomsIncoTermOverrideCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new CustomsIncoTermOverride this[int index]
		{
			get { return (CustomsIncoTermOverride)Elements[index]; }
		}

		public new CustomsIncoTermOverride AddNew()
		{
			return (CustomsIncoTermOverride)base.AddNew();
		}

		public CustomsIncoTermOverride AddNew(ZString customsCode, ZString internationalCode)
		{
			CustomsIncoTermOverride result = AddNew();
			result.CustomsCode = customsCode;
			result.InternationalCode = internationalCode;
			return result;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CustomsIncoTermOverrideCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CustomsIncoTermOverride();
		}
	}
}
