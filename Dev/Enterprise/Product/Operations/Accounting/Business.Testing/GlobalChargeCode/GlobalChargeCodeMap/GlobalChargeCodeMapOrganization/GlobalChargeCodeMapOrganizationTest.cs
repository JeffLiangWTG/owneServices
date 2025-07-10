using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GlobalChargeCode.Testing
{
	[TestedType(typeof(GlobalChargeCodeMapOrganization))]
	internal class GlobalChargeCodeMapOrganizationTest : GlobalChargeCodeMapTest
	{
		public void TestFetchStrategy()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			GlobalChargeCodeMapOrganization globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCode.YG_OH = org1.PK;
			globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCode.YG_OH = org2.PK;
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GlobalChargeCodeMapOrganizationCollection globalChargeCodeCollectionInNewFactory = new GlobalChargeCodeMapOrganizationCollection(newFactory);
			globalChargeCodeCollectionInNewFactory.Load();
			OrgHeader orgHeader = globalChargeCodeCollectionInNewFactory[0].Header;
			int dBHints = newFactory.DatabaseLoadCount;
			orgHeader = globalChargeCodeCollectionInNewFactory[1].Header;
			AssertEquals(dBHints, newFactory.DatabaseLoadCount);
		}

		public void TestYG_APChargeCode()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TEST1";
			GlobalChargeCodeMapOrganization globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			GlobalChargeCodeMapPivotOrganization pivot = globalChargeCode.PivotCollection.AddNew();
			pivot.YP_AC = chargeCode.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			AssertEquals("TEST1", globalChargeCode.YG_APChargeCode);
		}

		public void TestYG_ARChargeCode()
		{
			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "TEST1";
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Code = "TEST2";
			GlobalChargeCodeMapOrganization globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			GlobalChargeCodeMapPivotOrganization pivot = globalChargeCode.PivotCollection.AddNew();
			pivot.YP_AC = chargeCode1.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			pivot = globalChargeCode.PivotCollection.AddNew();
			pivot.YP_AC = chargeCode2.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "TEST1", "TEST2" }, globalChargeCode.YG_ARChargeCodes.Split(", "));
		}

		public void TestPivotCollection()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			GlobalChargeCodeMapOrganization globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCode.YG_OH = org1.PK;
			GlobalChargeCodeMapPivotOrganizationCollection pivotCollection = new GlobalChargeCodeMapPivotOrganizationCollection(Factory, globalChargeCode.PK);
			GlobalChargeCodeMapPivotOrganization pivot = pivotCollection.AddNew();
			pivot.YP_AC = chargeCode1.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			pivot = pivotCollection.AddNew();
			pivot.YP_AC = chargeCode1.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			Factory.Save();
			AssertEquals(2, globalChargeCode.PivotCollection.Count);
		}
	}
}
