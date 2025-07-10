using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TIP.Testing
{
	[TestedType(typeof(OrgPartRelationRegistryBusinessObject))]
	public class OrgPartRelationRegistryBusinessObjectTest : RegistryBusinessObjectTemplateTestCase<OrgPartRelationRegistryBusinessObject>
	{
		public void TestSetDefaults()
		{
			AssertEquals(OrgPartRelation.RelationshipTypes.Owner, OrgProdRelation.RelationshipType);
		}

		public void TestRelationshipType()
		{
			OrgProdRelation.RelationshipType = "ABC";
			AssertEquals("RelationshipType should have errors", true, OrgProdRelation.RelationshipTypeInfo.HasErrors());
			OrgProdRelation.RelationshipType = OrgPartRelation.RelationshipTypes.Owner;
			AssertEquals("RelationshipType should not have errors", false, OrgProdRelation.RelationshipTypeInfo.HasErrors());
		}

		public void TestOrgName()
		{
			Assert("Org Name should be empty", OrgProdRelation.OrgName.IsEmpty);
			OrgProdRelation.OrgHeaderPK = Org.PK;
			AssertEquals("Org Name should be empty", org.OH_FullName, OrgProdRelation.OrgName);
		}

		public void TestOrgHeaderPK()
		{
			OrgPartRelationRegistryBusinessObjectCollection collection = new OrgPartRelationRegistryBusinessObjectCollection();
			OrgPartRelationRegistryBusinessObject obj1 = (OrgPartRelationRegistryBusinessObject)collection.AddNew();
			OrgPartRelationRegistryBusinessObject obj2 = (OrgPartRelationRegistryBusinessObject)collection.AddNew();
			obj1.OrgHeaderPK = Org.PK;
			obj1.RelationshipType = OrgPartRelation.RelationshipTypes.Owner;
			obj2.OrgHeaderPK = Org.PK;
			obj2.RelationshipType = OrgPartRelation.RelationshipTypes.Owner;
			AssertEquals("Duplicate pair in list", true, obj2.OrgHeaderPKInfo.HasError("This organisation-product relationship pair already exists."));
		}

		public void TestEquals()
		{
			var obj1 = new OrgPartRelationRegistryBusinessObject()
			{ OrgHeaderPK = Org.PK, RelationshipType = OrgPartRelation.RelationshipTypes.Supplier };
			var obj2 = new OrgPartRelationRegistryBusinessObject()
			{ OrgHeaderPK = Org.PK, RelationshipType = OrgPartRelation.RelationshipTypes.Owner };
			var obj3 = obj1;
			AssertNotEquals("obj1 is not equal to obj2", obj1.Equals(obj2));
			AssertNotEquals("obj1 is not equal to obj2", obj1.GetHashCode().Equals(obj2.GetHashCode()));
			Assert("obj3 is equal to obj1", obj1.Equals(obj3));
			Assert("obj3 is equal to obj1", obj1.GetHashCode().Equals(obj3.GetHashCode()));
		}

		OrgPartRelationRegistryBusinessObject OrgProdRelation
		{
			get
			{
				return orgProdRelation ?? (orgProdRelation = new OrgPartRelationRegistryBusinessObject());
			}
		}

		OrgPartRelationRegistryBusinessObject orgProdRelation;
		OrgHeader Org
		{
			get
			{
				return org ?? (org = Factory.LoadTop1<OrgHeader>(new ZQuery()));
			}
		}

		OrgHeader org;
		#region Overrides
		protected override OrgPartRelationRegistryBusinessObject GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override OrgPartRelationRegistryBusinessObject GetBusinessObjectToSerialise()
		{
			OrgPartRelationRegistryBusinessObject result = new OrgPartRelationRegistryBusinessObject();
			result.OrgHeaderPK = new ZGuid("C3F842EF-3BE5-448C-BED3-0017B232C624");
			result.RelationshipType = OrgPartRelation.RelationshipTypes.Owner;
			return result;
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
		#endregion
	}
}
