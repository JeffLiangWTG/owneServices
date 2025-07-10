using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GlobalChargeCode.Testing
{
	[TestedType(typeof(GlobalChargeCodeMapPivotOrganization))]
	internal class GlobalChargeCodeMapPivotOrganizationTest : GlobalChargeCodeMapPivotTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			GlobalChargeCodeMapOrganization globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			return globalChargeCode.PivotCollection.AddNew();
		}

		public override void TestFetchStrategy()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "TEST1";
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Code = "TEST2";
			GlobalChargeCodeMapOrganization globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCode.YG_OH = org1.PK;
			GlobalChargeCodeMapPivotOrganization pivot = globalChargeCode.PivotCollection.AddNew();
			pivot.YP_AC = chargeCode1.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			pivot = globalChargeCode.PivotCollection.AddNew();
			pivot.YP_AC = chargeCode2.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCode.YG_OH = org1.PK;
			pivot = globalChargeCode.PivotCollection.AddNew();
			pivot.YP_AC = chargeCode1.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			pivot = globalChargeCode.PivotCollection.AddNew();
			pivot.YP_AC = chargeCode2.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GlobalChargeCodeMapPivotOrganizationCollection globalChargeCodePivotCollectionInNewFactory = new GlobalChargeCodeMapPivotOrganizationCollection(newFactory);
			globalChargeCodePivotCollectionInNewFactory.Load();
			AccChargeCode chargeCode = globalChargeCodePivotCollectionInNewFactory[0].ChargeCode;
			GlobalChargeCodeMap chargeCodeMap = globalChargeCodePivotCollectionInNewFactory[0].GlobalChargeCodeMap;
			int dBHints = newFactory.DatabaseLoadCount;
			chargeCode = globalChargeCodePivotCollectionInNewFactory[1].ChargeCode;
			chargeCodeMap = globalChargeCodePivotCollectionInNewFactory[1].GlobalChargeCodeMap;
			chargeCode = globalChargeCodePivotCollectionInNewFactory[2].ChargeCode;
			chargeCodeMap = globalChargeCodePivotCollectionInNewFactory[2].GlobalChargeCodeMap;
			chargeCode = globalChargeCodePivotCollectionInNewFactory[3].ChargeCode;
			chargeCodeMap = globalChargeCodePivotCollectionInNewFactory[3].GlobalChargeCodeMap;
			AssertEquals(dBHints, newFactory.DatabaseLoadCount);
		}

		public void TestGlobalChargeCodeMap()
		{
			GlobalChargeCodeMapOrganization globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			GlobalChargeCodeMapPivotOrganization pivot = globalChargeCode.PivotCollection.AddNew();
			AssertEquals(globalChargeCode, pivot.GlobalChargeCodeMap);
		}

		public void TestParentCollection()
		{
			GlobalChargeCodeMapOrganization globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			GlobalChargeCodeMapPivotOrganization pivot = globalChargeCode.PivotCollection.AddNew();
			AssertEquals(globalChargeCode.PivotCollection, pivot.ParentCollection);
		}
	}
}
