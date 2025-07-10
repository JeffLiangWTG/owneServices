using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GlobalChargeCode.Testing
{
	[TestedType(typeof(GlobalChargeCodeMapPivotOrganizationCollection))]
	public class GlobalChargeCodeMapPivotOrganizationCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlobalChargeCodeMapPivotOrganizationCollection(Factory);
		}

		public void TestCreateRelationshipFilter()
		{
			var currentCompanyChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK));
			var anotherCompanyChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var globalChargeCodeValid = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCodeValid.YG_OH = org.PK;
			var validPivot = globalChargeCodeValid.PivotCollection.AddNew();
			validPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			validPivot.YP_AC = currentCompanyChargeCode.PK;

			var globalChargeCodeValid2 = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCodeValid2.YG_OH = org.PK;
			var validPivot2 = globalChargeCodeValid2.PivotCollection.AddNew();
			validPivot2.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			validPivot2.YP_AC = currentCompanyChargeCode.PK;

			var globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCode.YG_OH = org.PK;
			var invalidPivot = globalChargeCode.PivotCollection.AddNew();
			invalidPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			invalidPivot.YP_AC = anotherCompanyChargeCode.PK;

			var globalChargeCodeIntercompany = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			var validIntPivot = globalChargeCodeIntercompany.PivotCollection.AddNew();
			validIntPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			validIntPivot.YP_AC = currentCompanyChargeCode.PK;

			Factory.Save();

			var collection = new GlobalChargeCodeMapPivotOrganizationCollection(Factory);
			collection.Load();
			AssertEquals(2, collection.Count);
			Assert(collection.Contains(validPivot.PK));
			Assert(collection.Contains(validPivot2.PK));

			collection = new GlobalChargeCodeMapPivotOrganizationCollection(Factory, globalChargeCodeValid.PK);
			collection.Load();
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(validPivot.PK));
		}

		public void TestSetDefaultsForNewChild()
		{
			GlobalChargeCodeMapOrganization globalChargeCodeValid = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			GlobalChargeCodeMapPivotOrganizationCollection collection = new GlobalChargeCodeMapPivotOrganizationCollection(Factory);
			collection.AddNew();
			AssertEquals(ZGuid.Empty, collection[0].YP_YG);
			collection = new GlobalChargeCodeMapPivotOrganizationCollection(Factory, globalChargeCodeValid.PK);
			collection.AddNew();
			AssertEquals(globalChargeCodeValid.PK, collection[0].YP_YG);
		}

		public void TestGetCodeOfAPChargeCode()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TEST1";
			GlobalChargeCodeMapPivotOrganizationCollection collection = new GlobalChargeCodeMapPivotOrganizationCollection(Factory);
			GlobalChargeCodeMapPivotOrganization pivot = collection.AddNew();
			pivot.YP_AC = chargeCode.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			AssertEquals("TEST1", collection.GetCodeOfAPChargeCode());
		}

		public void TestGetCodesOfARChargeCodes()
		{
			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "TEST1";
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Code = "TEST2";
			GlobalChargeCodeMapPivotOrganizationCollection collection = new GlobalChargeCodeMapPivotOrganizationCollection(Factory);
			GlobalChargeCodeMapPivotOrganization pivot = collection.AddNew();
			pivot.YP_AC = chargeCode1.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			pivot = collection.AddNew();
			pivot.YP_AC = chargeCode2.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			AssertEquals("TEST1, TEST2", collection.GetCodesOfARChargeCodes());
		}
	}
}
