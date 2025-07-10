using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TIP.Testing
{
	[TestedType(typeof(OrgPartRelationRegistryBusinessObjectCollection))]
	public class OrgPartRelationRegistryBusinessObjectCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<OrgPartRelationRegistryBusinessObjectCollection>
	{
		public void TestPairOccurrencesNoInList()
		{
			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgPartRelationRegistryBusinessObjectCollection collection = new OrgPartRelationRegistryBusinessObjectCollection();
			OrgPartRelationRegistryBusinessObject element1 = (OrgPartRelationRegistryBusinessObject)collection.AddNew();
			element1.OrgHeaderPK = org.PK;
			element1.RelationshipType = OrgPartRelation.RelationshipTypes.Owner;
			OrgPartRelationRegistryBusinessObject element2 = (OrgPartRelationRegistryBusinessObject)collection.AddNew();
			element2.OrgHeaderPK = org.PK;
			element2.RelationshipType = OrgPartRelation.RelationshipTypes.Owner;
			AssertEquals("Organisation added twice to the list, same relationship", 2, collection.GetRelationshipCountInList(org.PK, "OWN"));
		}

		protected override OrgPartRelationRegistryBusinessObjectCollection GetCollectionToTest()
		{
			return new OrgPartRelationRegistryBusinessObjectCollection();
		}

		protected override bool RequiresFactory
		{
			get
			{
				return true;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrgPartRelationRegistryBusinessObject();
		}
	}
}
