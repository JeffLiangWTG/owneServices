using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GlobalChargeCode.Testing
{
	[TestedType(typeof(GlobalChargeCodeMapPivotIntercompanyWithLocalClientOverrideCollection))]
	public class GlobalChargeCodeMapPivotIntercompanyWithLocalClientOverrideCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlobalChargeCodeMapPivotIntercompanyWithLocalClientOverrideCollection(Factory);
		}

		public void TestCreateRelationshipFilter()
		{
			var currentCompanyChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK));

			var localClient1 = Factory.NewWithValidTestData<OrgHeader>();
			var localClient2 = Factory.NewWithValidTestData<OrgHeader>();

			var globalChargeCodeMap1 = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			var validPivot1 = globalChargeCodeMap1.PivotWithOverrideLocalClientCollection.AddNew();
			validPivot1.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			validPivot1.YP_AC = currentCompanyChargeCode.PK;
			validPivot1.YP_OH_LocalClientOverride = localClient1.PK;

			var globalChargeCodeMap2 = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			var validPivot2 = globalChargeCodeMap2.PivotWithOverrideLocalClientCollection.AddNew();
			validPivot2.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			validPivot2.YP_AC = currentCompanyChargeCode.PK;
			validPivot2.YP_OH_LocalClientOverride = localClient2.PK;

			var pivotWithoutLocalClient = globalChargeCodeMap1.PivotWithoutOverrideLocalClientCollection.AddNew();
			pivotWithoutLocalClient.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			pivotWithoutLocalClient.YP_AC = currentCompanyChargeCode.PK;
			pivotWithoutLocalClient.YP_OH_LocalClientOverride = ZGuid.Empty;

			Factory.Save();

			var collection = new GlobalChargeCodeMapPivotIntercompanyWithLocalClientOverrideCollection(Factory);
			collection.Load();
			AssertEquals("expect all pivots with YP_OH_LocalClientOverride set", 2, collection.Count);
			Assert(collection.Contains(validPivot1.PK));
			Assert(collection.Contains(validPivot2.PK));

			collection = new GlobalChargeCodeMapPivotIntercompanyWithLocalClientOverrideCollection(Factory, globalChargeCodeMap1.PK);
			collection.Load();
			AssertEquals("expect all pivots for global charge code map which has YP_OH_LocalClientOverride set", 1, collection.Count);
			Assert(collection.Contains(validPivot1.PK));
		}
	}
}
