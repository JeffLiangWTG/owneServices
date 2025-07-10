using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class IndexDurationList : RegistryBusinessObjectCollectionTemplate
	{
		public new IndexDuration this[int i] => (IndexDuration)Elements[i];

		public IndexDurationList() { }

		public new IndexDuration AddNew()
		{
			return (IndexDuration)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new IndexDuration();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new IndexDurationList();
		}
	}
}
