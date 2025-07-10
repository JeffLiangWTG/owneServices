using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.Business.Testing
{
	public class FeatureControlExtensionsTest : TestCaseWithFactory
	{
		[TestDate(2024, 8, 1)]
		public void TestLoadFromDatabase()
		{
			var featureControl = FeatureControlExtensions.LoadFromDatabase(DateTime.MinValue, ZGuid.Empty);
			AssertEquals(@"<?xml version=""1.0""?>
<FeatureControl xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <TimestampUtc>0001-01-01T00:00:00</TimestampUtc>
</FeatureControl>", featureControl.ToXmlString());
			featureControl = FeatureControlExtensions.LoadFromDatabase(ZDateTime.Empty, ZGuid.Empty);
			AssertEquals(@"<?xml version=""1.0""?>
<FeatureControl xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <TimestampUtc>0001-01-01T00:00:00</TimestampUtc>
</FeatureControl>", featureControl.ToXmlString());

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "some org";
			org.OH_Code = "code";

			var trustedSystem = Factory.New<EdiTrustedSystem>();
			trustedSystem.ETS_Product = "CW1";
			trustedSystem.ETS_SystemID = "1024";

			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_OH = org.PK;
			licenceEnterprise.LE_EnterpriseCode = "BLA";

			var licenceDatabase = Factory.New<LicenceDatabase>();
			licenceDatabase.LD_LE = licenceEnterprise.PK;
			licenceDatabase.LD_DatabaseNumber = 1024;
			licenceDatabase.LD_ServerCode = "123";
			licenceDatabase.LD_Product = "CW1";
			licenceDatabase.LD_TenantID = "1024";
			licenceDatabase.LD_OH_WebAccessOrg = org.PK;
			licenceDatabase.LD_ETS_TrustedSystem = trustedSystem.PK;
			Factory.Save();

			var featureSet = Factory.New<FeatureControlSet>();
			featureSet.FCS_ProductName = "CargeWise Next";

			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			header.FCM_FeatureControlCode = "AUTHLOGIN";
			var rule1 = header.FeatureControlRules.AddNew();
			rule1.IsGlobalRule = true;
			rule1.FCR_Description = "rule global - 1 - AUTHLOGIN";
			rule1.FCR_StartDateUtc = new ZDateTime(2001, 1, 1);
			rule1.FCR_EndDateUtc = new ZDateTime(2002, 1, 1);
			rule1.FCR_Parameters = "<a>global - 1 - AUTHLOGIN</a>";

			var rule2 = header.FeatureControlRules.AddNew();
			rule2.IsGlobalRule = true;
			rule2.FCR_Description = "rule global - 2 - AUTHLOGIN";
			rule2.FCR_StartDateUtc = new ZDateTime(2003, 1, 1);
			rule2.FCR_EndDateUtc = new ZDateTime(2004, 1, 1);
			rule2.FCR_Parameters = "<a>rule global - 2 - AUTHLOGIN</a>";
			rule2.FCR_IsActive = false;

			var rule3 = header.FeatureControlRules.AddNew();
			rule3.IsGlobalRule = false;
			rule3.FCR_Description = "AUTHLOGIN - rule client - 1";
			rule3.FCR_StartDateUtc = new ZDateTime(2005, 1, 1);
			rule3.FCR_EndDateUtc = new ZDateTime(2006, 1, 1);
			rule3.FCR_Parameters = "<a>AUTHLOGIN - rule client - 1</a>";
			rule3.FCR_IsActive = false;

			var pivot1 = Factory.New<FeatureControlRuleLicenceDatabasePivot>();
			pivot1.FCD_FCR_FeatureControlRule = rule3.PK;
			pivot1.FCD_LD_LicenceDatabase = licenceDatabase.PK;

			var rule4 = header.FeatureControlRules.AddNew();
			rule4.IsGlobalRule = false;
			rule4.FCR_Description = "AUTHLOGIN - rule client - 2";
			rule4.FCR_StartDateUtc = new ZDateTime(2007, 1, 1);
			rule4.FCR_EndDateUtc = new ZDateTime(2008, 1, 1);
			rule4.FCR_Parameters = "<a>AUTHLOGIN - rule client - 2</a>";

			var pivot2 = Factory.New<FeatureControlRuleLicenceDatabasePivot>();
			pivot2.FCD_FCR_FeatureControlRule = rule4.PK;
			pivot2.FCD_LD_LicenceDatabase = licenceDatabase.PK;

			var rule5 = header.FeatureControlRules.AddNew();
			rule5.IsGlobalRule = false;
			rule5.FCR_Description = "AUTHLOGIN - rule client - 3";
			rule5.FCR_StartDateUtc = new ZDateTime(2009, 1, 1);
			rule5.FCR_EndDateUtc = new ZDateTime(2010, 1, 1);
			rule5.FCR_Parameters = "<a>AUTHLOGIN - rule client - 3</a>";

			var setRule1 = header.FeatureControlRules.AddNew();
			setRule1.IsFeatureSetRule = true;
			setRule1.FCR_FCS_FeatureSet = featureSet.PK; 
			setRule1.FCR_Description = "AUTHLOGIN - rule feature set - 1";
			setRule1.FCR_StartDateUtc = new ZDateTime(2009, 1, 1);
			setRule1.FCR_EndDateUtc = new ZDateTime(2010, 1, 1);
			setRule1.FCR_Parameters = "<a>AUTHLOGIN - rule feature set - 1</a>";
			licenceDatabase.LD_FCS_FeatureSet = featureSet.PK;

			var header2 = Factory.NewWithValidTestData<FeatureControlHeader>();
			header2.FCM_FeatureControlCode = "EXPORTCSV";
			var rule6 = header2.FeatureControlRules.AddNew();
			rule6.IsGlobalRule = true;
			rule6.FCR_Description = "EXPORTCSV - rule global - 1";
			rule6.FCR_StartDateUtc = new ZDateTime(2011, 1, 1);
			rule6.FCR_Parameters = "<a>EXPORTCSV - rule global - 1</a>";

			var rule7 = header2.FeatureControlRules.AddNew();
			rule7.IsGlobalRule = false;
			rule7.FCR_Description = "EXPORTCSV - rule client - 1";
			rule7.FCR_StartDateUtc = new ZDateTime(2013, 1, 1);
			rule7.FCR_EndDateUtc = new ZDateTime(2014, 1, 1);
			rule7.FCR_UseGlobalParameters = true;

			var pivot3 = Factory.New<FeatureControlRuleLicenceDatabasePivot>();
			pivot3.FCD_FCR_FeatureControlRule = rule7.PK;
			pivot3.FCD_LD_LicenceDatabase = licenceDatabase.PK;

			Factory.Save();

			featureControl = FeatureControlExtensions.LoadFromDatabase(DateTime.MinValue, licenceDatabase.PK);
			AssertEquals(@"<?xml version=""1.0""?>
<FeatureControl xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <TimestampUtc>2024-08-01T00:00:00Z</TimestampUtc>
  <Rules>
    <Rule>
      <FCM_FeatureControlCode>AUTHLOGIN</FCM_FeatureControlCode>
      <FCR_RuleType>CLI</FCR_RuleType>
      <FCR_StartDateUtc>2007-01-01T00:00:00Z</FCR_StartDateUtc>
      <FCR_EndDateUtc>2008-01-01T00:00:00Z</FCR_EndDateUtc>
      <FCR_Parameters>&lt;a&gt;AUTHLOGIN - rule client - 2&lt;/a&gt;</FCR_Parameters>
    </Rule>
    <Rule>
      <FCM_FeatureControlCode>AUTHLOGIN</FCM_FeatureControlCode>
      <FCR_RuleType>GLB</FCR_RuleType>
      <FCR_StartDateUtc>2001-01-01T00:00:00Z</FCR_StartDateUtc>
      <FCR_EndDateUtc>2002-01-01T00:00:00Z</FCR_EndDateUtc>
      <FCR_Parameters>&lt;a&gt;global - 1 - AUTHLOGIN&lt;/a&gt;</FCR_Parameters>
    </Rule>
    <Rule>
      <FCM_FeatureControlCode>AUTHLOGIN</FCM_FeatureControlCode>
      <FCR_RuleType>GLB</FCR_RuleType>
      <FCR_StartDateUtc>2009-01-01T00:00:00Z</FCR_StartDateUtc>
      <FCR_EndDateUtc>2010-01-01T00:00:00Z</FCR_EndDateUtc>
      <FCR_Parameters>&lt;a&gt;AUTHLOGIN - rule feature set - 1&lt;/a&gt;</FCR_Parameters>
    </Rule>
    <Rule>
      <FCM_FeatureControlCode>EXPORTCSV</FCM_FeatureControlCode>
      <FCR_RuleType>CLI</FCR_RuleType>
      <FCR_StartDateUtc>2013-01-01T00:00:00Z</FCR_StartDateUtc>
      <FCR_EndDateUtc>2014-01-01T00:00:00Z</FCR_EndDateUtc>
      <FCR_Parameters>&lt;a&gt;EXPORTCSV - rule global - 1&lt;/a&gt;</FCR_Parameters>
    </Rule>
    <Rule>
      <FCM_FeatureControlCode>EXPORTCSV</FCM_FeatureControlCode>
      <FCR_RuleType>GLB</FCR_RuleType>
      <FCR_StartDateUtc>2011-01-01T00:00:00Z</FCR_StartDateUtc>
      <FCR_Parameters>&lt;a&gt;EXPORTCSV - rule global - 1&lt;/a&gt;</FCR_Parameters>
    </Rule>
  </Rules>
</FeatureControl>", featureControl.ToXmlString());

			featureControl = FeatureControlExtensions.LoadFromDatabase(featureControl.TimestampUtc, licenceDatabase.PK);
			AssertEquals(@"<?xml version=""1.0""?>
<FeatureControl xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <TimestampUtc>2024-08-01T00:00:00Z</TimestampUtc>
</FeatureControl>", featureControl.ToXmlString());
		}

		[TestDate(2024, 8, 1)]
		public void TestLoadFromDatabaseShouldUpdateTimestampWhenFeatureSetHasBeenUpdated()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "some org";
			org.OH_Code = "code";

			var trustedSystem = Factory.New<EdiTrustedSystem>();
			trustedSystem.ETS_Product = "CW1";
			trustedSystem.ETS_SystemID = "1024";

			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_OH = org.PK;
			licenceEnterprise.LE_EnterpriseCode = "BLA";

			var licenceDatabase = Factory.New<LicenceDatabase>();
			licenceDatabase.LD_LE = licenceEnterprise.PK;
			licenceDatabase.LD_DatabaseNumber = 1024;
			licenceDatabase.LD_ServerCode = "123";
			licenceDatabase.LD_Product = "CW1";
			licenceDatabase.LD_TenantID = "1024";
			licenceDatabase.LD_OH_WebAccessOrg = org.PK;
			licenceDatabase.LD_ETS_TrustedSystem = trustedSystem.PK;
			Factory.Save();

			var featureSet = Factory.New<FeatureControlSet>();
			featureSet.FCS_ProductName = "CargeWise Next";

			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			header.FCM_FeatureControlCode = "AUTHLOGIN";
			var rule1 = header.FeatureControlRules.AddNew();
			rule1.FCR_RuleType = FeatureControlRuleTypeList.Codes.FeatureSet;
			rule1.FCR_Description = "rule global - 1 - AUTHLOGIN";
			rule1.FCR_StartDateUtc = new ZDateTime(2001, 1, 1);
			rule1.FCR_EndDateUtc = new ZDateTime(2002, 1, 1);
			rule1.FCR_Parameters = "<a>global - 1 - AUTHLOGIN</a>";
			rule1.FCR_FCS_FeatureSet = featureSet.PK;
			licenceDatabase.LD_FCS_FeatureSet = featureSet.PK;

			Factory.Save();

			var originalfeatureControlLoaded = FeatureControlExtensions.LoadFromDatabase(DateTime.MinValue, licenceDatabase.PK);
			AssertEquals("Precondition", @"<?xml version=""1.0""?>
<FeatureControl xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <TimestampUtc>2024-08-01T00:00:00Z</TimestampUtc>
  <Rules>
    <Rule>
      <FCM_FeatureControlCode>AUTHLOGIN</FCM_FeatureControlCode>
      <FCR_RuleType>GLB</FCR_RuleType>
      <FCR_StartDateUtc>2001-01-01T00:00:00Z</FCR_StartDateUtc>
      <FCR_EndDateUtc>2002-01-01T00:00:00Z</FCR_EndDateUtc>
      <FCR_Parameters>&lt;a&gt;global - 1 - AUTHLOGIN&lt;/a&gt;</FCR_Parameters>
    </Rule>
  </Rules>
</FeatureControl>", originalfeatureControlLoaded.ToXmlString());

			TestDateAttribute.AddDays(1);
			var licenceDatabase2 = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase2.LD_FCS_FeatureSet = featureSet.PK;
			Factory.Save();

			var secondFeatureControlLoaded = FeatureControlExtensions.LoadFromDatabase(originalfeatureControlLoaded.TimestampUtc, licenceDatabase.PK);
			AssertEquals("Should have updated timestamp to time that Licence Database was attached to feature set", @"<?xml version=""1.0""?>
<FeatureControl xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <TimestampUtc>2024-08-02T00:00:00Z</TimestampUtc>
  <Rules>
    <Rule>
      <FCM_FeatureControlCode>AUTHLOGIN</FCM_FeatureControlCode>
      <FCR_RuleType>GLB</FCR_RuleType>
      <FCR_StartDateUtc>2001-01-01T00:00:00Z</FCR_StartDateUtc>
      <FCR_EndDateUtc>2002-01-01T00:00:00Z</FCR_EndDateUtc>
      <FCR_Parameters>&lt;a&gt;global - 1 - AUTHLOGIN&lt;/a&gt;</FCR_Parameters>
    </Rule>
  </Rules>
</FeatureControl>", secondFeatureControlLoaded.ToXmlString());
		}
	}
}
