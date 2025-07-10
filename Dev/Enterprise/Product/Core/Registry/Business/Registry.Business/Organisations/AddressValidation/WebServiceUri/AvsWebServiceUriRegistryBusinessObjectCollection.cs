using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AvsWebServiceUriRegistryBusinessObjectCollection : CodeDescriptionBoolCollection
	{
		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		public AvsWebServiceUriRegistryBusinessObject Primary => GetServiceUriFromType(AvsWebServiceUriType.Primary);

		public AvsWebServiceUriRegistryBusinessObject Secondary => GetServiceUriFromType(AvsWebServiceUriType.Secondary);

		public AvsWebServiceUriRegistryBusinessObject Background => GetServiceUriFromType(AvsWebServiceUriType.Background);

		public new AvsWebServiceUriRegistryBusinessObject AddNew()
		{
			return (AvsWebServiceUriRegistryBusinessObject)base.AddNew();
		}

		public new AvsWebServiceUriRegistryBusinessObject this[int i]
		{
			get { return (AvsWebServiceUriRegistryBusinessObject)base[i]; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AvsWebServiceUriRegistryBusinessObject();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new AvsWebServiceUriRegistryBusinessObjectCollection();
		}

		AvsWebServiceUriRegistryBusinessObject GetServiceUriFromType(string type)
		{
			var result = default(AvsWebServiceUriRegistryBusinessObject);

			var uriBizOs = this.OfType<AvsWebServiceUriRegistryBusinessObject>();
			if (uriBizOs.Count(x => x.Type == type) == 1)
			{
				result = uriBizOs.Single(x => x.Type == type);
			}

			return result;
		}
	}
}
