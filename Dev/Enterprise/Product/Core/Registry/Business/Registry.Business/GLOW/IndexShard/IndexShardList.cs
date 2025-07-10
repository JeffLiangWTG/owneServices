using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class IndexShardList : RegistryBusinessObjectCollectionTemplate
	{
		public new IndexShard this[int i] => (IndexShard)Elements[i];

		public new IndexShard AddNew()
		{
			return (IndexShard)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new IndexShard() { IsOverridden = true };
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new IndexShardList();
		}
	}
}
