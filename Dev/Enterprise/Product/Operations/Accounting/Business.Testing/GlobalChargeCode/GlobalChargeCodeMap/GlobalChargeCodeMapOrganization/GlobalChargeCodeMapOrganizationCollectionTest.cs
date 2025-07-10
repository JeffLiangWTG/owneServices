using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GlobalChargeCode.Testing
{
	[TestedType(typeof(GlobalChargeCodeMapOrganizationCollection))]
	public class GlobalChargeCodeMapOrganizationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCreateRelationshipFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			GlobalChargeCodeMapIntercompany intercompanyGlobalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			GlobalChargeCodeMapOrganization globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCode.YG_OH = org1.PK;
			globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCode.YG_OH = org2.PK;
			Factory.Save();
			GlobalChargeCodeMapOrganizationCollection collection = new GlobalChargeCodeMapOrganizationCollection(Factory);
			collection.Load();
			AssertEquals(2, collection.Count);
			collection = new GlobalChargeCodeMapOrganizationCollection(Factory, org1.PK);
			collection.Load();
			AssertEquals(1, collection.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlobalChargeCodeMapOrganizationCollection(Factory);
		}
	}
}
