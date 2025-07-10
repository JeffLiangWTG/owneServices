using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.Business.Testing
{
	[TestedType(typeof(FeatureControlExportBizObj))]
	public class FeatureControlExportBizObjTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new FeatureControlExportBizObj();
		}

		public void TestExport()
		{
			var obj = new FeatureControlExportBizObj();
			AssertEquals(ZString.Empty, obj.RuleContent);
			obj.Export();
			AssertRuleContent(@"<?xml version=""1.0""?>
<FeatureControl xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <TimestampUtc>0001-01-01T00:00:00</TimestampUtc>
</FeatureControl>", obj.RuleContent);

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

			using (ObjectCache.OverrideDateTimeProvider(new FeatureControlDateTimeProvider() { CurrentUtcDateTimeOverride = new DateTime(2024, 8, 1, 0, 0, 0, DateTimeKind.Utc) }))
			{
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

				var header2 = Factory.NewWithValidTestData<FeatureControlHeader>();
				header2.FCM_FeatureControlCode = "CR5RESWIZ";
				var rule6 = header2.FeatureControlRules.AddNew();
				rule6.IsGlobalRule = true;
				rule6.FCR_Description = "CR5RESWIZ - rule global - 1";
				rule6.FCR_StartDateUtc = new ZDateTime(2011, 1, 1);
				rule6.FCR_Parameters = "<a>CR5RESWIZ - rule global - 1</a>";

				var rule7 = header2.FeatureControlRules.AddNew();
				rule7.IsGlobalRule = false;
				rule7.FCR_Description = "CR5RESWIZ - rule client - 1";
				rule7.FCR_StartDateUtc = new ZDateTime(2013, 1, 1);
				rule7.FCR_EndDateUtc = new ZDateTime(2014, 1, 1);
				rule7.FCR_UseGlobalParameters = true;

				var pivot3 = Factory.New<FeatureControlRuleLicenceDatabasePivot>();
				pivot3.FCD_FCR_FeatureControlRule = rule7.PK;
				pivot3.FCD_LD_LicenceDatabase = licenceDatabase.PK;
				Factory.Save();
			}

			obj.Export();
			AssertRuleContent(@"<?xml version=""1.0""?>
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
    <Rule>
      <FCM_FeatureControlCode>CR5RESWIZ</FCM_FeatureControlCode>
      <FCR_RuleType>GLB</FCR_RuleType>
      <FCR_StartDateUtc>2011-01-01T00:00:00Z</FCR_StartDateUtc>
      <FCR_Parameters>&lt;a&gt;CR5RESWIZ - rule global - 1&lt;/a&gt;</FCR_Parameters>
    </Rule>
  </Rules>
</FeatureControl>", obj.RuleContent);

			obj.DatabasePk = licenceDatabase.PK;
			obj.Export();
			AssertRuleContent(@"<?xml version=""1.0""?>
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
      <FCM_FeatureControlCode>CR5RESWIZ</FCM_FeatureControlCode>
      <FCR_RuleType>CLI</FCR_RuleType>
      <FCR_StartDateUtc>2013-01-01T00:00:00Z</FCR_StartDateUtc>
      <FCR_EndDateUtc>2014-01-01T00:00:00Z</FCR_EndDateUtc>
      <FCR_Parameters>&lt;a&gt;CR5RESWIZ - rule global - 1&lt;/a&gt;</FCR_Parameters>
    </Rule>
    <Rule>
      <FCM_FeatureControlCode>CR5RESWIZ</FCM_FeatureControlCode>
      <FCR_RuleType>GLB</FCR_RuleType>
      <FCR_StartDateUtc>2011-01-01T00:00:00Z</FCR_StartDateUtc>
      <FCR_Parameters>&lt;a&gt;CR5RESWIZ - rule global - 1&lt;/a&gt;</FCR_Parameters>
    </Rule>
  </Rules>
</FeatureControl>", obj.RuleContent);

			var timeProvider = new FeatureControlManagerDateTimeProvider();
			var featureControlManager = new FeatureControlManager(ObjectFactory.Get<IReadOnlyFeatureControlStorage>(), null, timeProvider);
			using (ObjectFactory.Substitute<IFeatureControlManager>(featureControlManager))
			{
				timeProvider.CurrentUtcDateTimeOverride = ZDateTime.UtcNow.ToDateTime();
				AssertGetFeatureData(timeProvider, "CR5RESWIZ", null);
				SetRegistryValue(obj.RuleContent);

				timeProvider.CurrentUtcDateTimeOverride = ZDateTime.UtcNow.AddHours(30).ToDateTime();
				AssertGetFeatureData(timeProvider, "CR5RESWIZ", "<a>CR5RESWIZ - rule global - 1</a>");

				timeProvider.CurrentUtcDateTimeOverride = new DateTime(2001, 01, 03);
				//Expecting null as feature code is not active
				AssertGetFeatureData(timeProvider, "AUTHLOGIN", null);

				obj.AllowAllFeatureStages = true;
				obj.Export();
				AssertRuleContent(@"<?xml version=""1.0""?>
<FeatureControl xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <TimestampUtc>2024-08-01T00:00:00Z</TimestampUtc>
  <AllowAllFeatureStages>true</AllowAllFeatureStages>
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
      <FCM_FeatureControlCode>CR5RESWIZ</FCM_FeatureControlCode>
      <FCR_RuleType>CLI</FCR_RuleType>
      <FCR_StartDateUtc>2013-01-01T00:00:00Z</FCR_StartDateUtc>
      <FCR_EndDateUtc>2014-01-01T00:00:00Z</FCR_EndDateUtc>
      <FCR_Parameters>&lt;a&gt;CR5RESWIZ - rule global - 1&lt;/a&gt;</FCR_Parameters>
    </Rule>
    <Rule>
      <FCM_FeatureControlCode>CR5RESWIZ</FCM_FeatureControlCode>
      <FCR_RuleType>GLB</FCR_RuleType>
      <FCR_StartDateUtc>2011-01-01T00:00:00Z</FCR_StartDateUtc>
      <FCR_Parameters>&lt;a&gt;CR5RESWIZ - rule global - 1&lt;/a&gt;</FCR_Parameters>
    </Rule>
  </Rules>
</FeatureControl>", obj.RuleContent);
				SetRegistryValue(obj.RuleContent);
				timeProvider.CurrentUtcDateTimeOverride = new DateTime(2001, 01, 04);
				//Should return the feature data as all feature codes are allowed
				AssertGetFeatureData(timeProvider, "AUTHLOGIN", "<a>global - 1 - AUTHLOGIN</a>");
			}
		}

		void AssertGetFeatureData(TimeProvider timeProvider, string featureCode, string parameterExpected)
		{
			var data = new FeatureControlManager(ObjectFactory.Get<IReadOnlyFeatureControlStorage>(), null, timeProvider).GetFeatureData(featureCode);
			if (parameterExpected == null)
			{
				AssertNull(data);
			}
			else
			{
				AssertEquals(parameterExpected, data.Parameter);
			}
		}

		void AssertRuleContent(string xmlExpected, string ruleContentBase64String)
		{
			using var ms = new MemoryStream(Convert.FromBase64String(ruleContentBase64String));
			using var zipStream = new GZipStream(ms, CompressionMode.Decompress);
			using (var resultStream = new MemoryStream())
			{
				zipStream.CopyTo(resultStream);
				var xml = Encoding.UTF8.GetString(resultStream.ToArray());
				AssertEquals(xmlExpected, xml);
			}
		}

		void SetRegistryValue(string value)
			=> WebDataRegistry.Instance.FeatureControlRuleContent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
	}

	class FeatureControlDateTimeProvider : Environment.DateTimeProvider
	{
		public DateTime CurrentUtcDateTimeOverride { get; set; }
		public override DateTime CurrentUtcDateTime => CurrentUtcDateTimeOverride;
	}

	class FeatureControlManagerDateTimeProvider : TimeProvider
	{
		public DateTime CurrentUtcDateTimeOverride { get; set; }

		public override DateTimeOffset GetUtcNow()
		{
			return CurrentUtcDateTimeOverride;
		}
	}
}
