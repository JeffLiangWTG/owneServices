using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.GlobalChargeCode.Testing
{
	using CargoWise.EntityFramework.Testing;

	internal class AccGlobalChargeCodeMapPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLedgerTypes()
		{
			AccGlobalChargeCodeMapPivotLookups lookup = new AccGlobalChargeCodeMapPivotLookups(null);
			AssertEquals(2, lookup.LedgerTypes.Count);
			AssertEquals(ZArchitecture.Core.LedgerTypes.AccountsPayable, lookup.LedgerTypes[0].Code);
			AssertEquals(ZArchitecture.Core.LedgerTypes.AccountsReceivable, lookup.LedgerTypes[1].Code);
		}

		public void TestGlobalChargeCodeMapsOrganization()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			GlobalChargeCodeMap globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCode.YG_OH = org1.PK;
			globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCode.YG_OH = org2.PK;
			Factory.Save();
			GlobalChargeCodeMapPivotOrganizationCollection collection = new GlobalChargeCodeMapPivotOrganizationCollection(Factory);
			collection.OrganisationPK = org2.PK;
			AccGlobalChargeCodeMapPivotLookups lookup = new AccGlobalChargeCodeMapPivotLookups(collection.AddNew());
			GlobalChargeCodeMapOrganizationCollection globalChargeCodeCollection = lookup.GlobalChargeCodeMapsOrganization;
			globalChargeCodeCollection.Load();
			AssertEquals(1, globalChargeCodeCollection.Count);
			AssertEquals(globalChargeCode, globalChargeCodeCollection[0]);
		}

		public void TestGlobalChargeCodeMapsIntercompany()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			GlobalChargeCodeMap globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCode.YG_OH = org1.PK;
			globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCode.YG_OH = org2.PK;
			globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			Factory.Save();
			GlobalChargeCodeMapPivotIntercompanyCollection collection = new GlobalChargeCodeMapPivotIntercompanyCollection(Factory);
			AccGlobalChargeCodeMapPivotLookups lookup = new AccGlobalChargeCodeMapPivotLookups(collection.AddNew());
			GlobalChargeCodeMapIntercompanyCollection globalChargeCodeCollection = lookup.GlobalChargeCodeMapsIntercompany;
			globalChargeCodeCollection.Load();
			AssertEquals(1, globalChargeCodeCollection.Count);
			AssertEquals(globalChargeCode, globalChargeCodeCollection[0]);
		}
	}
}