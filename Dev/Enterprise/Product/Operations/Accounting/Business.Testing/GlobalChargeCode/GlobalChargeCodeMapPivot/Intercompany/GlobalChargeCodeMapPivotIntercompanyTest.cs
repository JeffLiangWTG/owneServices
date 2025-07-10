using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GlobalChargeCode.Testing
{
	[TestedType(typeof(GlobalChargeCodeMapPivotIntercompany))]
	internal class GlobalChargeCodeMapPivotIntercompanyTest : GlobalChargeCodeMapPivotTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			return globalChargeCode.PivotCollection.AddNew();
		}

		public override void TestFetchStrategy()
		{
			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "TEST1";
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Code = "TEST2";
			GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			GlobalChargeCodeMapPivotIntercompany pivot = globalChargeCode.PivotWithoutOverrideLocalClientCollection.AddNew();
			pivot.YP_AC = chargeCode1.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			pivot = globalChargeCode.PivotWithoutOverrideLocalClientCollection.AddNew();
			pivot.YP_AC = chargeCode2.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			pivot = globalChargeCode.PivotWithoutOverrideLocalClientCollection.AddNew();
			pivot.YP_AC = chargeCode1.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			pivot = globalChargeCode.PivotWithoutOverrideLocalClientCollection.AddNew();
			pivot.YP_AC = chargeCode2.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GlobalChargeCodeMapPivotIntercompanyCollection globalChargeCodePivotCollectionInNewFactory = new GlobalChargeCodeMapPivotIntercompanyCollection(newFactory);
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
			GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			GlobalChargeCodeMapPivotIntercompany pivot = globalChargeCode.PivotCollection.AddNew();
			AssertEquals(globalChargeCode, pivot.GlobalChargeCodeMap);
		}

		public void TestParentCollection()
		{
			GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			GlobalChargeCodeMapPivotIntercompany pivot = globalChargeCode.PivotCollection.AddNew();
			AssertEquals(globalChargeCode.PivotCollection, pivot.ParentCollection);
		}
	}
}
