using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GlobalChargeCode.Testing
{
	[TestedType(typeof(GlobalChargeCodeMapIntercompanyCollection))]
	public class GlobalChargeCodeMapIntercompanyCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCreateRelationshipFilter()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			globalChargeCode.YG_OH = org.PK;
			Factory.Save();
			GlobalChargeCodeMapIntercompanyCollection collection = new GlobalChargeCodeMapIntercompanyCollection(Factory);
			collection.Load();
			AssertEquals(1, collection.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlobalChargeCodeMapIntercompanyCollection(Factory);
		}
	}
}
