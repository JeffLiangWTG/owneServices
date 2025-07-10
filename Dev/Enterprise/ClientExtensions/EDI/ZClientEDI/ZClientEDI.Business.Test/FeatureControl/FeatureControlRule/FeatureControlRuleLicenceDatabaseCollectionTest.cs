using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.Business.Testing
{
	[TestedType(typeof(FeatureControlRuleLicenceDatabaseCollection))]
	public class FeatureControlRuleLicenceDatabaseCollectionTest : ActiveBusinessObjectCollectionTestCase<FeatureControlRuleLicenceDatabaseCollection>
	{
		protected override FeatureControlRuleLicenceDatabaseCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			var ruleGlobal = header.FeatureControlRules.AddNew();
			ruleGlobal.IsGlobalRule = true;
			ruleGlobal.FCR_Parameters = "{\"key1\":\"value1\"}";
			return new FeatureControlRuleLicenceDatabaseCollection(ruleGlobal, new ZQuery());
		}
		public void TestGetExtraNotification()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TEST_ORG1";
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LD_ServerCode = "DB1";
			db1.LD_DatabaseNumber = 100;
			db1.LD_OH_WebAccessOrg = org.PK;
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			db2.LD_ServerCode = "DB2";

			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			var rule1 = header.FeatureControlRules.AddNew();
			var pivot = rule1.LicenceDatabasePivots.AddNew();
			pivot.FCD_LD_LicenceDatabase = db1.PK;

			var rule2 = header.FeatureControlRules.AddNew();
			var collection = new FeatureControlRuleLicenceDatabaseCollection(rule2, new ZQuery());
			var provider = collection as IFilterModuleExtraNotificationProvider;
			var notification = provider.GetExtraNotification(db1);
			AssertEquals(CargoWise.ComponentModel.NotificationType.Error, notification.Type);
			AssertEquals("License Database 100 for client TEST_ORG1 is already attached to an active rule", notification.Message);
			notification = provider.GetExtraNotification(db2);
			AssertNull(notification);
		}
	}
}
