using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.Business.Testing
{
	[TestedType(typeof(FeatureControlRule))]
	public class FeatureControlRuleTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			var ruleGlobal = header.FeatureControlRules.AddNew();
			ruleGlobal.FCR_Description = "GlobalRule1";
			ruleGlobal.IsGlobalRule = true;
			ruleGlobal.FCR_Parameters = "{\"key1\":\"value1\"}";
			ruleGlobal.FCR_StartDateUtc = new ZDateTime(2024, 1, 1);
			return ruleGlobal;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		public void TestRuleType()
		{
			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			var rule = header.FeatureControlRules.AddNew();

			rule.IsGlobalRule = false;
			rule.FCR_UseGlobalParameters = true;
			AssertEquals(true, rule.FCR_UseGlobalParameters);
			AssertEquals("CLI", rule.FCR_RuleType);

			rule.IsGlobalRule = true;
			AssertEquals(false, rule.FCR_UseGlobalParameters);
			AssertEquals("GLB", rule.FCR_RuleType);

			rule.IsFeatureSetRule = true;
			AssertEquals("FCS", rule.FCR_RuleType);

			var featureSet = Factory.NewWithValidTestData<FeatureControlSet>();
			rule.FCR_FCS_FeatureSet = featureSet.PK;
			AssertNotNull(rule.FeatureSet);

			rule.IsFeatureSetRule = false;
			AssertEquals(ZGuid.Empty, rule.FCR_FCS_FeatureSet);
			AssertNull(rule.FeatureSet);
		}

		public void TestParameters()
		{
			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			var ruleGlobal = header.FeatureControlRules.AddNew();
			ruleGlobal.IsGlobalRule = true;
			ruleGlobal.FCR_Parameters = "{\"key1\":\"value1\"}";
			var ruleClient = header.FeatureControlRules.AddNew();
			ruleClient.IsGlobalRule = false;
			ruleClient.FCR_Parameters = "{\"key2\":\"value2\"}";

			AssertEquals(true, ruleGlobal.FCR_UseGlobalParameters_ReadOnly);
			AssertEquals(false, ruleGlobal.FCR_Parameters_ReadOnly);

			AssertEquals(false, ruleClient.FCR_UseGlobalParameters_ReadOnly);
			AssertEquals(false, ruleClient.FCR_Parameters_ReadOnly);

			var shouldCancel = false;
			ruleClient.BeforeParametersOverwrite += (_, e) =>
			{
				e.Cancel = shouldCancel;
			};

			shouldCancel = true;
			ruleClient.FCR_UseGlobalParameters = true;
			AssertEquals(false, ruleClient.FCR_UseGlobalParameters);
			AssertEquals(false, ruleClient.FCR_UseGlobalParameters_ReadOnly);
			AssertEquals(false, ruleClient.FCR_Parameters_ReadOnly);

			shouldCancel = false;
			ruleClient.FCR_UseGlobalParameters = true;
			AssertEquals(true, ruleClient.FCR_UseGlobalParameters);
			AssertEquals("{\"key1\":\"value1\"}", ruleClient.FCR_Parameters);
			AssertEquals(false, ruleClient.FCR_UseGlobalParameters_ReadOnly);
			AssertEquals(true, ruleClient.FCR_Parameters_ReadOnly);

			ruleGlobal.IsGlobalRule = false;
			AssertEquals("", ruleClient.FCR_Parameters);
		}

		public void TestCanDelete()
		{
			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			var ruleGlobal = header.FeatureControlRules.AddNew();
			ruleGlobal.IsGlobalRule = true;
			ruleGlobal.FCR_Parameters = "{\"key1\":\"value1\"}";
			AssertEquals(true, ruleGlobal.CanDelete);

			var ruleClient = header.FeatureControlRules.AddNew();
			ruleClient.IsGlobalRule = false;
			ruleClient.FCR_UseGlobalParameters = true;
			AssertEquals(true, ruleClient.CanDelete);
			AssertEquals(false, ruleGlobal.CanDelete);
			AssertEquals("This global rule cannot be deleted because its parameters are currently in use by other rules.", ruleGlobal.ReasonForNotAbleToDelete.ToString());
		}

		public void TestDatabaseCount()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LD_ServerCode = "DB1";
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			db2.LD_ServerCode = "DB2";

			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			var rule1 = header.FeatureControlRules.AddNew();
			var pivot = rule1.LicenceDatabasePivots.AddNew();
			pivot.FCD_LD_LicenceDatabase = db1.PK;
			AssertEquals(1, rule1.DatabaseCount);

			var rule2 = header.FeatureControlRules.AddNew();
			AssertEquals(0, rule2.DatabaseCount);
		}

		public void TestCanAttachLicenceDatabase()
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
			var message = "";
			AssertEquals(true, rule2.CanAttachLicenceDatabase(db2, out message));
			AssertNull(message);
			AssertEquals(false, rule2.CanAttachLicenceDatabase(db1, out message));
			AssertEquals("License Database 100 for client TEST_ORG1 is already attached to an active rule", message);

			rule1.FCR_IsActive = false;
			AssertEquals(true, rule2.CanAttachLicenceDatabase(db1, out message));
			AssertNull(message);
			AssertEquals(true, rule2.CanAttachLicenceDatabase(db2, out message));
			AssertNull(message);

			rule1.FCR_IsActive = true;
			rule2.FCR_IsActive = false;
			AssertEquals(true, rule2.CanAttachLicenceDatabase(db1, out message));
			AssertNull(message);
			AssertEquals(true, rule2.CanAttachLicenceDatabase(db2, out message));
			AssertNull(message);
		}

		[TestDate(2024, 1, 1)]
		public void TestDelete()
		{
			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			header.FCM_SystemLastEditTimeUtc = new ZDateTime(2000, 1, 1);
			var rule1 = header.FeatureControlRules.AddNew();
			rule1.Delete();
			AssertEquals(new ZDateTime(2024, 1, 1), header.FCM_SystemLastEditTimeUtc);

			AssertNoExceptionThrown(() =>
			{
				var rule2 = Factory.New<FeatureControlRule>();
				rule2.Delete();
			});
		}

		public void TestDeleteDbPivotsOnSavingIfNotClientRule()
		{
			var header = Factory.NewWithValidTestData<FeatureControlHeader>();

			var rule1 = header.FeatureControlRules.AddNew();
			rule1.FCR_Description = "rule1";
			rule1.FCR_StartDateUtc = new ZDateTime(2025, 1, 1);
			var pivot1 = rule1.LicenceDatabasePivots.AddNew();
			pivot1.FCD_LD_LicenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>().PK;

			var rule2 = header.FeatureControlRules.AddNew();
			rule1.FCR_Description = "rule2";
			rule2.FCR_StartDateUtc = new ZDateTime(2025, 1, 1);
			var pivot2 = rule2.LicenceDatabasePivots.AddNew();
			pivot2.FCD_LD_LicenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>().PK;

			Factory.Save();

			AssertEquals(1, rule1.LicenceDatabasePivots.Count);
			AssertEquals(1, rule2.LicenceDatabasePivots.Count);

			rule1.IsGlobalRule = true;
			Factory.Save();
			rule1.LicenceDatabasePivots.Reload(true);
			AssertEquals(0, rule1.LicenceDatabasePivots.Count);

			rule2.IsFeatureSetRule = true;
			rule2.FCR_FCS_FeatureSet = Factory.NewWithValidTestData<FeatureControlSet>().PK;
			Factory.Save();
			rule2.LicenceDatabasePivots.Reload(true);
			AssertEquals(0, rule2.LicenceDatabasePivots.Count);
		}

		public void TestAudit()
		{
			var obj = Factory.NewWithValidTestData<FeatureControlRule>();
			Assert(!obj.IsAutoLogged);
			AssertNotNull(obj.RelatedAuditChildren.Single(x => x.KeyColumn == FeatureControlRuleLicenceDatabasePivotSchema.FCD_FCR_FeatureControlRule && x.InfoColumn == null));
		}
	}

	[TestedType(typeof(FeatureControlRule))]
	public class FeatureControlRuleAuditParentTest : AuditParentTest<FeatureControlRule>
	{
		protected override FeatureControlRule NewTestAuditParent()
		{
			return Factory.New<FeatureControlRule>();
		}
	}
}
