using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.FeatureControl.Business.Testing
{
	public class FeatureControlRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckFCR_Description()
		{
			RuleClient2.FCR_Description = RuleClient.FCR_Description;
			AssertHasError(RuleClient2.FCR_DescriptionInfo, "The rule description must be unique.");
			RuleClient2.FCR_Description = "";
			AssertHasError(RuleClient2.FCR_DescriptionInfo, "Please enter a Description.");
			RuleClient2.FCR_Description = "new rules";
			AssertNoErrors(RuleClient2.FCR_DescriptionInfo);
		}

		public void TestParameters()
		{
			RuleGlobal.FCR_Parameters = new string('c', 1024 * 1024);
			AssertHasError(RuleGlobal.FCR_ParametersInfo, "'c' is an invalid start of a value. LineNumber: 0 | BytePositionInLine: 0.");
			RuleGlobal.FCR_Parameters = new string('c', 1024 * 1024) + 1;
			AssertHasError(RuleGlobal.FCR_ParametersInfo, "The amount of data in the parameters field must not exceed 1MB. Please adjust and try again.");
			RuleGlobal.FCR_Parameters = "{\"key1\":\"value1\"}";
			AssertNoErrors(RuleGlobal.FCR_ParametersInfo);
			RuleGlobal.FCR_Parameters = "";
			AssertNoErrors(RuleGlobal.FCR_ParametersInfo);

			RuleClient2.FCR_Parameters = "x";
			RuleClient2.FCR_Parameters = "";
			AssertEquals(true, RuleClient2.FCR_UseGlobalParameters);
			AssertNoErrors(RuleClient2.FCR_ParametersInfo);

			RuleGlobal.FCR_IsActive = false;
			RuleClient2.Validation.ValidateFCR_Parameters();
			AssertNoErrors(RuleClient2.FCR_ParametersInfo);
		}

		public void TestCheckFCR_EndDateUtc()
		{
			RuleGlobal.FCR_EndDateUtc = RuleGlobal.FCR_StartDateUtc.AddDays(-1);
			AssertHasError(RuleGlobal.FCR_EndDateUtcInfo, "The end date cannot be earlier than the start date.");
			RuleGlobal.FCR_EndDateUtc = ZDate.Empty;
			AssertNoErrors(RuleGlobal.FCR_EndDateUtcInfo);
		}

		public void TestCheckDateRange()
		{
			RuleGlobal.FCR_StartDateUtc = ZDateTime.UtcNow.Date.AddYears(20);
			RuleGlobal.FCR_EndDateUtc = ZDateTime.UtcNow.Date.AddYears(21);
			AssertNoErrors(RuleGlobal.FCR_StartDateUtcInfo);
			AssertNoErrors(RuleGlobal.FCR_EndDateUtcInfo);

			RuleGlobal.FCR_StartDateUtc = ZDateTime.UtcNow.Date.AddYears(-20);
			RuleGlobal.FCR_EndDateUtc = ZDateTime.UtcNow.Date.AddYears(-19);
			AssertNoErrors(RuleGlobal.FCR_StartDateUtcInfo);
			AssertNoErrors(RuleGlobal.FCR_EndDateUtcInfo);
		}

		public void TestGlobalRule()
		{
			RuleGlobal.FCR_UseGlobalParameters = true;
			AssertHasError(RuleGlobal.FCR_UseGlobalParametersInfo, "The global rule is unable to assign this flag.");
			RuleGlobal.FCR_UseGlobalParameters = false;
			RuleGlobal.FCR_Parameters = "{\"key1\":\"value1\"}";

			RuleGlobal.FCR_IsActive = false;
			AssertNoErrors(RuleGlobal.FCR_IsActiveInfo);
			RuleGlobal.IsGlobalRule = false;
			AssertHasError(RuleGlobal.IsGlobalRuleInfo, "This global rule cannot be changed because its parameters are currently in use by other rules.");

			RuleClient.FCR_UseGlobalParameters = true;
			AssertHasError(RuleClient.FCR_UseGlobalParametersInfo, "There is no global control rule.");

			RuleClient.FCR_UseGlobalParameters = false;
			RuleGlobal.FCR_IsActive = true;
			RuleGlobal.IsGlobalRule = true;

			RuleClient.IsGlobalRule = true;
			AssertHasError(RuleClient.IsGlobalRuleInfo, "Only one global control rule is allowed.");
			RuleClient.IsGlobalRule = false;

			var featureSet1 = Factory.NewWithValidTestData<FeatureControlSet>();
			featureSet1.FCS_ProductName = "Test Product";
			var featureSetRule1 = Header.FeatureControlRules.AddNew();
			featureSetRule1.FCR_RuleType = FeatureControlRuleTypeList.Codes.FeatureSet;
			featureSetRule1.FCR_FCS_FeatureSet = featureSet1.PK;
			featureSetRule1.FCR_StartDateUtc = new ZDateTime(2024, 1, 1);
			RuleClient.IsGlobalRule = true;
			AssertHasError(RuleClient.IsGlobalRuleInfo, "Active global control rule is not allowed because there are active feature set rules.");

			featureSetRule1.FCR_IsActive = false;
			RuleClient.IsGlobalRule = true;
			AssertNoErrors(RuleGlobal.IsGlobalRuleInfo);

			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LD_ServerCode = "DB1";
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			db2.LD_ServerCode = "DB2";
			RuleClient.IsGlobalRule = false;
			var pivot1 = RuleGlobal.LicenceDatabasePivots.AddNew();
			pivot1.FCD_LD_LicenceDatabase = db1.PK;
			var pivot2 = RuleClient.LicenceDatabasePivots.AddNew();
			pivot2.FCD_LD_LicenceDatabase = db2.PK;
			Header.RunPreSaveValidation();
			AssertHasError(RuleGlobal.FCR_RuleTypeInfo, "A global rule cannot have license databases attached.");
			AssertNoErrors(RuleClient.FCR_RuleTypeInfo);
		}

		public void TestFeatureSet_UniqueFeatureSet()
		{
			var featureSet1 = Factory.NewWithValidTestData<FeatureControlSet>();
			featureSet1.FCS_ProductName = "Test Product";

			var rule1 = Header.FeatureControlRules.AddNew();
			rule1.FCR_RuleType = FeatureControlRuleTypeList.Codes.FeatureSet;
			rule1.FCR_FCS_FeatureSet = featureSet1.PK;
			rule1.FCR_StartDateUtc = new ZDateTime(2024, 1, 1);

			rule1.RunPreSaveValidation();
			AssertNoErrors(rule1.FCR_FCS_FeatureSetInfo);

			Factory.Save();

			var rule2 = Header.FeatureControlRules.AddNew();
			rule2.FCR_RuleType = FeatureControlRuleTypeList.Codes.FeatureSet;
			rule2.FCR_FCS_FeatureSet = featureSet1.PK;
			rule2.FCR_StartDateUtc = new ZDateTime(2024, 1, 1);
			rule2.RunPreSaveValidation();
			AssertHasError(rule2.FCR_FCS_FeatureSetInfo, "A rule with same feature code already exists in feature set Test Product.");

			var featureSet2 = Factory.NewWithValidTestData<FeatureControlSet>();
			featureSet2.FCS_ProductName = "New Product";
			rule2.FCR_FCS_FeatureSet = featureSet2.PK;
			rule2.RunPreSaveValidation();
			AssertNoErrors(rule2.FCR_FCS_FeatureSetInfo);
		}

		public void TestFeatureSet_RuleType()
		{
			var featureSet1 = Factory.NewWithValidTestData<FeatureControlSet>();
			featureSet1.FCS_ProductName = "Test Product";

			RuleGlobal.FCR_IsActive = false;

			var rule1 = Header.FeatureControlRules.AddNew();
			rule1.FCR_RuleType = FeatureControlRuleTypeList.Codes.FeatureSet;
			rule1.FCR_FCS_FeatureSet = ZGuid.Empty;

			rule1.RunPreSaveValidation();
			AssertHasError(rule1.FCR_FCS_FeatureSetInfo, "Feature set is required.");

			rule1.FCR_FCS_FeatureSet = featureSet1.PK;
			rule1.RunPreSaveValidation();
			AssertNoErrors(rule1.FCR_FCS_FeatureSetInfo);

			rule1.FCR_RuleType = FeatureControlRuleTypeList.Codes.Client;
			rule1.FCR_FCS_FeatureSet = featureSet1.PK;
			rule1.RunPreSaveValidation();
			AssertHasError(rule1.FCR_FCS_FeatureSetInfo, "Feature set is not allowed.");

			rule1.FCR_FCS_FeatureSet = ZGuid.Empty;
			rule1.RunPreSaveValidation();
			AssertNoErrors(rule1.FCR_FCS_FeatureSetInfo);

			rule1.FCR_RuleType = FeatureControlRuleTypeList.Codes.FeatureSet;
			rule1.FCR_FCS_FeatureSet = featureSet1.PK;
			rule1.RunPreSaveValidation();
			AssertNoErrors(rule1.FCR_FCS_FeatureSetInfo);

			rule1.FCR_RuleType = FeatureControlRuleTypeList.Codes.Global;
			rule1.FCR_FCS_FeatureSet = ZGuid.Empty;
			rule1.FCR_RuleType = FeatureControlRuleTypeList.Codes.FeatureSet;
			rule1.RunPreSaveValidation();
			AssertHasError(rule1.FCR_FCS_FeatureSetInfo, "Feature set is required.");

			RuleGlobal.FCR_IsActive = true;
			rule1.RunPreSaveValidation();
			AssertHasError(rule1.FCR_RuleTypeInfo, "Active feature set rule is not allowed because there is active global rule.");

			RuleGlobal.FCR_IsActive = false;
			rule1.FCR_FCS_FeatureSet = featureSet1.PK;
			rule1.RunPreSaveValidation();
			AssertNoErrors(rule1.FCR_RuleTypeInfo);

			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LD_ServerCode = "DB1";
			var pivot1 = rule1.LicenceDatabasePivots.AddNew();
			pivot1.FCD_LD_LicenceDatabase = db1.PK;
			rule1.RunPreSaveValidation();
			AssertHasError(rule1.FCR_RuleTypeInfo, "A feature set rule cannot have license databases attached.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			Header = Factory.NewWithValidTestData<FeatureControlHeader>();
			RuleGlobal = Header.FeatureControlRules.AddNew();
			RuleGlobal.FCR_Description = "GlobalRule1";
			RuleGlobal.IsGlobalRule = true;
			RuleGlobal.FCR_Parameters = "{\"key1\":\"value1\"}";
			RuleGlobal.FCR_StartDateUtc = new ZDateTime(2024, 1, 1);
			RuleClient = Header.FeatureControlRules.AddNew();
			RuleClient.FCR_Description = "Rule1";
			RuleClient.IsGlobalRule = false;
			RuleClient.FCR_Parameters = "{\"key2\":\"value2\"}";
			RuleClient.FCR_StartDateUtc = new ZDateTime(2025, 1, 1);
			RuleClient2 = Header.FeatureControlRules.AddNew();
			RuleClient2.FCR_Description = "Rule2";
			RuleClient2.IsGlobalRule = false;
			RuleClient2.FCR_UseGlobalParameters = true;
			RuleClient2.FCR_StartDateUtc = new ZDateTime(2026, 1, 1);
			Factory.Save();
		}

		FeatureControlHeader Header;
		FeatureControlRule RuleGlobal;
		FeatureControlRule RuleClient;
		FeatureControlRule RuleClient2;
	}
}
