using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.TGE.Business
{
	[XmlSerializerAssembly("ZClientTGE.XmlSerializers")]
	public class TGEEventRegistryBusinessObjectCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new TGEEventRegistryBusinessObject this[int i]
		{
			get { return (TGEEventRegistryBusinessObject)Elements[i]; }
		}

		public new TGEEventRegistryBusinessObject AddNew()
		{
			return (TGEEventRegistryBusinessObject)base.AddNew();
		}

		public bool ContainsCode(ZString code)
		{
			bool result = false;
			foreach (TGEEventRegistryBusinessObject element in Elements)
			{
				if (element.Code == code)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TGEEventRegistryBusinessObjectCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TGEEventRegistryBusinessObject();
		}
	}
}
