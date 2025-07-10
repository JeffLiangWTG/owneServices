using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GlobalChargeCode.Testing
{
	[TestedType(typeof(GlobalChargeCodeMapIntercompany))]
	internal class GlobalChargeCodeMapIntercompanyTest : GlobalChargeCodeMapTest
	{
		public void TestYG_APChargeCode()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TEST1";
			GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			GlobalChargeCodeMapPivotIntercompany pivot = globalChargeCode.PivotWithoutOverrideLocalClientCollection.AddNew();
			pivot.YP_AC = chargeCode.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			Factory.Save();
			AssertEquals("TEST1", globalChargeCode.YG_APChargeCode);
		}

		public void TestYG_ARChargeCode()
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
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "TEST1", "TEST2" }, globalChargeCode.YG_ARChargeCodes.Split(", "));
		}

		public void TestPivotCollection()
		{
			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			GlobalChargeCodeMapPivotIntercompanyCollection pivotCollection = new GlobalChargeCodeMapPivotIntercompanyCollection(Factory, globalChargeCode.PK);
			GlobalChargeCodeMapPivotIntercompany pivot = pivotCollection.AddNew();
			pivot.YP_AC = chargeCode1.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			pivot = pivotCollection.AddNew();
			pivot.YP_AC = chargeCode1.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			Factory.Save();
			AssertEquals(2, globalChargeCode.PivotCollection.Count);
		}

		public void TestPivotWithAndWithoutOverrideLocalClientCollection()
		{
			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			GlobalChargeCodeMapPivotIntercompany pivot1 = globalChargeCode.PivotWithOverrideLocalClientCollection.AddNew();
			pivot1.YP_AC = chargeCode1.PK;
			pivot1.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			pivot1.YP_OH_LocalClientOverride = Factory.NewWithValidTestData<OrgHeader>().PK;
			GlobalChargeCodeMapPivotIntercompany pivot2 = globalChargeCode.PivotWithoutOverrideLocalClientCollection.AddNew();
			pivot2.YP_AC = chargeCode2.PK;
			pivot2.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			Factory.Save();
			AssertEquals(1, globalChargeCode.PivotWithOverrideLocalClientCollection.Count);
			Assert(globalChargeCode.PivotWithOverrideLocalClientCollection.Contains(pivot1.PK));
			AssertEquals(1, globalChargeCode.PivotWithoutOverrideLocalClientCollection.Count);
			Assert(globalChargeCode.PivotWithoutOverrideLocalClientCollection.Contains(pivot2.PK));
			AssertEquals(2, globalChargeCode.PivotCollection.Count);
			Assert(globalChargeCode.PivotCollection.Contains(pivot1.PK));
			Assert(globalChargeCode.PivotCollection.Contains(pivot2.PK));
		}

		public void TestYG_HasLocalClientOverride()
		{
			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			GlobalChargeCodeMapPivotIntercompanyCollection pivotCollection = new GlobalChargeCodeMapPivotIntercompanyCollection(Factory, globalChargeCode.PK);
			GlobalChargeCodeMapPivotIntercompany pivot1 = pivotCollection.AddNew();
			pivot1.YP_AC = chargeCode1.PK;
			pivot1.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			Factory.Save();
			Assert(!globalChargeCode.YG_HasLocalClientOverride);
			pivot1.YP_OH_LocalClientOverride = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var globalChargeCodeReload = newFactory.Load<GlobalChargeCodeMapIntercompany>(globalChargeCode.PK);
			Assert(globalChargeCodeReload.YG_HasLocalClientOverride);
		}
	}
}
