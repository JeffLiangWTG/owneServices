using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GlobalChargeCode.Testing
{
	[TestedType(typeof(GlobalChargeCodeMapPivotIntercompanyCollection))]
	public class GlobalChargeCodeMapPivotIntercompanyCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlobalChargeCodeMapPivotIntercompanyCollection(Factory);
		}

		public void TestCreateRelationshipFilter()
		{
			var currentCompanyChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK));
			var anotherCompanyChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var globalChargeCodeValid = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			var validPivot = globalChargeCodeValid.PivotWithoutOverrideLocalClientCollection.AddNew();
			validPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			validPivot.YP_AC = currentCompanyChargeCode.PK;

			var globalChargeCodeValid2 = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			var validPivot2 = globalChargeCodeValid2.PivotWithoutOverrideLocalClientCollection.AddNew();
			validPivot2.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			validPivot2.YP_AC = currentCompanyChargeCode.PK;

			var globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			var invalidPivot = globalChargeCode.PivotWithoutOverrideLocalClientCollection.AddNew();
			invalidPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			invalidPivot.YP_AC = anotherCompanyChargeCode.PK;

			var globalChargeCodeOrganization = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCodeOrganization.YG_OH = org.PK;
			var validOrgPivot = globalChargeCodeOrganization.PivotCollection.AddNew();
			validOrgPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			validOrgPivot.YP_AC = currentCompanyChargeCode.PK;

			Factory.Save();

			var collection = new GlobalChargeCodeMapPivotIntercompanyCollection(Factory);
			collection.Load();
			AssertEquals(2, collection.Count);
			Assert(collection.Contains(validPivot.PK));
			Assert(collection.Contains(validPivot2.PK));

			collection = new GlobalChargeCodeMapPivotIntercompanyCollection(Factory, globalChargeCodeValid.PK);
			collection.Load();
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(validPivot.PK));
		}

		public void TestSetDefaultsForNewChild()
		{
			GlobalChargeCodeMapIntercompany globalChargeCodeValid = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			GlobalChargeCodeMapPivotIntercompanyCollection collection = new GlobalChargeCodeMapPivotIntercompanyCollection(Factory);
			collection.AddNew();
			AssertEquals(ZGuid.Empty, collection[0].YP_YG);
			collection = new GlobalChargeCodeMapPivotIntercompanyCollection(Factory, globalChargeCodeValid.PK);
			collection.AddNew();
			AssertEquals(globalChargeCodeValid.PK, collection[0].YP_YG);
		}

		public void TestGetCodeOfAPChargeCode()
		{
			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "TEST1";
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Code = "TEST2";
			GlobalChargeCodeMapPivotIntercompanyCollection collection = new GlobalChargeCodeMapPivotIntercompanyWithLocalClientOverrideCollection(Factory);
			GlobalChargeCodeMapPivotIntercompany pivot = collection.AddNew();
			pivot.YP_AC = chargeCode1.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			pivot = collection.AddNew();
			pivot.YP_AC = chargeCode2.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			pivot = collection.AddNew();
			pivot.YP_AC = chargeCode1.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			AssertEquals("TEST1, TEST2", collection.GetCodeOfAPChargeCode());
		}

		public void TestGetCodesOfARChargeCodes()
		{
			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "TEST1";
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Code = "TEST2";
			GlobalChargeCodeMapPivotIntercompanyCollection collection = new GlobalChargeCodeMapPivotIntercompanyWithLocalClientOverrideCollection(Factory);
			GlobalChargeCodeMapPivotIntercompany pivot = collection.AddNew();
			pivot.YP_AC = chargeCode1.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			pivot = collection.AddNew();
			pivot.YP_AC = chargeCode2.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			pivot = collection.AddNew();
			pivot.YP_AC = chargeCode1.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			AssertEquals("TEST1, TEST2", collection.GetCodesOfARChargeCodes());
		}
	}
}
