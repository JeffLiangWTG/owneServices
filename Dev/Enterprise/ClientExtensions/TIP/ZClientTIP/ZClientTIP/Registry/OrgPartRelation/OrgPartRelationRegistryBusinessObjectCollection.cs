using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.TIP
{
	[XmlSerializerAssembly("ZClientTIP.XmlSerializers")]
	public class OrgPartRelationRegistryBusinessObjectCollection : RegistryBusinessObjectCollectionTemplate
	{
		public OrgPartRelationRegistryBusinessObjectCollection()
		{
		}

		public OrgPartRelationRegistryBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new OrgPartRelationRegistryBusinessObject this[int index]
		{
			get { return (OrgPartRelationRegistryBusinessObject)base[index]; }
		}

		public int GetRelationshipCountInList(ZGuid orgPK, ZString relationType)
		{
			int result = 0;
			foreach (OrgPartRelationRegistryBusinessObject currentElement in Elements)
			{
				if (currentElement.OrgHeaderPK == orgPK
					&& currentElement.RelationshipType == relationType)
				{
					result++;
				}
			}
			return result;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrgPartRelationRegistryBusinessObjectCollection(factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrgPartRelationRegistryBusinessObject(CurrentFactory);
		}
	}
}
