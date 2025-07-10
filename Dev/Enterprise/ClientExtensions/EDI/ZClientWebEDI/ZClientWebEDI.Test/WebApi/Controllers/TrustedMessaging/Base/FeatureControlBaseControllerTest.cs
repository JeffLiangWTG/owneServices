using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Licensing.Billing.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class FeatureControlBaseControllerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		[TestDate(2024, 1, 1)]
		public void TestGetFeatureControlRuleCore()
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

			var databaseInAnotherSession = (new BusinessObjectFactory() { RefreshEnabled = false }).Load<LicenceDatabase>(licenceDatabase.PK);
			databaseInAnotherSession.LD_ReportedHostDBName = "SH0WIxxxxxxxxV5";
			databaseInAnotherSession.LD_LastHeartbeat = ZDateTime.UtcNow.AddMinutes(-1);
			databaseInAnotherSession.Factory.Save();

			//the client ts equals server ts
			var rsp = CallController(trustedSystem, "CW1", "1024", DateTime.MinValue);
			AssertEquals(DateTime.MinValue, rsp.ruleTimestamp);
			AssertEquals(DateTimeKind.Utc, rsp.ruleTimestamp.Kind);
			AssertEquals(null, rsp.ruleContent);
			AssertEquals(false, licenceDatabase.LD_FeatureControlRuleLastSyncUtc.IsEmpty);
			AssertEquals(true, licenceDatabase.LD_FeatureControlRuleLastSyncContent.IsEmpty);
			licenceDatabase.LD_FeatureControlRuleLastSyncUtc = ZDateTime.Empty;

			//the client ts does not equal server ts
			rsp = CallController(trustedSystem, "CW1", "1024", DateTime.MaxValue);
			AssertEquals(DateTime.MinValue, rsp.ruleTimestamp);
			AssertEquals(DateTimeKind.Utc, rsp.ruleTimestamp.Kind);
			AssertEquals(@"<?xml version=""1.0""?>
<FeatureControl xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <TimestampUtc>0001-01-01T00:00:00</TimestampUtc>
</FeatureControl>", rsp.ruleContent);
			AssertEquals(true, licenceDatabase.LD_FeatureControlRuleLastSyncUtc.IsEmpty);
			AssertEquals(rsp.ruleContent, licenceDatabase.LD_FeatureControlRuleLastSyncContent);

			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			header.FCM_FeatureControlCode = "ACCEPCFTR";
			var rule1 = header.FeatureControlRules.AddNew();
			rule1.IsGlobalRule = true;
			rule1.FCR_Description = "rule global - 1 - ACCEPCFTR";
			rule1.FCR_StartDateUtc = new ZDateTime(2001, 1, 1);
			rule1.FCR_EndDateUtc = new ZDateTime(2002, 1, 1);
			rule1.FCR_Parameters = "<a>global - 1 - ACCEPCFTR</a>";

			var rule2 = header.FeatureControlRules.AddNew();
			rule2.IsGlobalRule = true;
			rule2.FCR_Description = "rule global - 2 - ACCEPCFTR";
			rule2.FCR_StartDateUtc = new ZDateTime(2003, 1, 1);
			rule2.FCR_EndDateUtc = new ZDateTime(2004, 1, 1);
			rule2.FCR_Parameters = "<a>rule global - 2 - ACCEPCFTR</a>";
			rule2.FCR_IsActive = false;

			var rule3 = header.FeatureControlRules.AddNew();
			rule3.IsGlobalRule = false;
			rule3.FCR_Description = "ACCEPCFTR - rule client - 1";
			rule3.FCR_StartDateUtc = new ZDateTime(2005, 1, 1);
			rule3.FCR_EndDateUtc = new ZDateTime(2006, 1, 1);
			rule3.FCR_Parameters = "<a>ACCEPCFTR - rule client - 1</a>";
			rule3.FCR_IsActive = false;

			var pivot1 = Factory.New<FeatureControlRuleLicenceDatabasePivot>();
			pivot1.FCD_FCR_FeatureControlRule = rule3.PK;
			pivot1.FCD_LD_LicenceDatabase = licenceDatabase.PK;

			var rule4 = header.FeatureControlRules.AddNew();
			rule4.IsGlobalRule = false;
			rule4.FCR_Description = "ACCEPCFTR - rule client - 2";
			rule4.FCR_StartDateUtc = new ZDateTime(2007, 1, 1);
			rule4.FCR_EndDateUtc = new ZDateTime(2008, 1, 1);
			rule4.FCR_Parameters = "<a>ACCEPCFTR - rule client - 2</a>";

			var pivot2 = Factory.New<FeatureControlRuleLicenceDatabasePivot>();
			pivot2.FCD_FCR_FeatureControlRule = rule4.PK;
			pivot2.FCD_LD_LicenceDatabase = licenceDatabase.PK;

			var rule5 = header.FeatureControlRules.AddNew();
			rule5.IsGlobalRule = false;
			rule5.FCR_Description = "ACCEPCFTR - rule client - 3";
			rule5.FCR_StartDateUtc = new ZDateTime(2009, 1, 1);
			rule5.FCR_EndDateUtc = new ZDateTime(2010, 1, 1);
			rule5.FCR_Parameters = "<a>ACCEPCFTR - rule client - 3</a>";

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

			var ruleXml =
@"<?xml version=""1.0""?>
<FeatureControl xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <TimestampUtc>2024-01-01T00:00:00Z</TimestampUtc>
  <Rules>
    <Rule>
      <FCM_FeatureControlCode>ACCEPCFTR</FCM_FeatureControlCode>
      <FCR_RuleType>CLI</FCR_RuleType>
      <FCR_StartDateUtc>2007-01-01T00:00:00Z</FCR_StartDateUtc>
      <FCR_EndDateUtc>2008-01-01T00:00:00Z</FCR_EndDateUtc>
      <FCR_Parameters>&lt;a&gt;ACCEPCFTR - rule client - 2&lt;/a&gt;</FCR_Parameters>
    </Rule>
    <Rule>
      <FCM_FeatureControlCode>ACCEPCFTR</FCM_FeatureControlCode>
      <FCR_RuleType>GLB</FCR_RuleType>
      <FCR_StartDateUtc>2001-01-01T00:00:00Z</FCR_StartDateUtc>
      <FCR_EndDateUtc>2002-01-01T00:00:00Z</FCR_EndDateUtc>
      <FCR_Parameters>&lt;a&gt;global - 1 - ACCEPCFTR&lt;/a&gt;</FCR_Parameters>
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
</FeatureControl>";

			rsp = CallController(trustedSystem, "CW1", "1024", DateTime.MinValue);
			AssertEquals(new DateTime(2024, 1, 1), rsp.ruleTimestamp);
			AssertContains(ruleXml, rsp.ruleContent);
			AssertEquals(ruleXml, licenceDatabase.LD_FeatureControlRuleLastSyncContent);
			AssertEquals(true, licenceDatabase.LD_FeatureControlRuleLastSyncUtc.IsEmpty);

			var featureControlStorageProvider = new WritableFeatureControlStorageProvider();
			var featureControlRepository = new FeatureControlRuleRepository(featureControlStorageProvider, null);
			featureControlRepository.SaveFeatureControlRuleContent(rsp.ruleContentRaw);

			var timeProvider = new FeatureControlTestHelper.FeatureControlManagerDateTimeProvider();
			var featureControlManager = new FeatureControlManager(featureControlStorageProvider, null, timeProvider);
			timeProvider.CurrentUtcDateTimeOverride = new DateTime(2011, 1, 1);
			var featureData = featureControlManager.GetFeatureData("CR5RESWIZ");
			AssertNotNull("Should load rule since it's the start date", featureData);
			AssertEquals("CR5RESWIZ", featureData.Code);
			AssertEquals("<a>CR5RESWIZ - rule global - 1</a>", featureData.Parameter);

			rsp = CallController(trustedSystem, "CW1", "1024", DateTime.MaxValue);
			AssertEquals(new DateTime(2024, 1, 1), rsp.ruleTimestamp);
			AssertContains(ruleXml, rsp.ruleContent);
			AssertEquals(ruleXml, licenceDatabase.LD_FeatureControlRuleLastSyncContent);
			AssertEquals(true, licenceDatabase.LD_FeatureControlRuleLastSyncUtc.IsEmpty);

			rsp = CallController(trustedSystem, "CW1", "1024", new DateTime(2024, 1, 1));
			AssertEquals(new DateTime(2024, 1, 1), rsp.ruleTimestamp);
			AssertEquals(null, rsp.ruleContent);
			AssertEquals(ruleXml, licenceDatabase.LD_FeatureControlRuleLastSyncContent);
			AssertEquals(false, licenceDatabase.LD_FeatureControlRuleLastSyncUtc.IsEmpty);

			Db.Connection.ExecuteNonQuery($"UPDATE FeatureControlRule SET FCR_Parameters = '<a>CR5RESWIZ - rule global - 1 - (updated)</a>', FCR_SystemLastEditTimeUtc = '2024-01-01 00:00:01', FCR_SystemLastEditUser = 'E' WHERE FCR_PK = '{rule6.PK}'; ");
			rsp = CallController(trustedSystem, "CW1", "1024", new DateTime(2024, 1, 1));
			AssertEquals(new DateTime(2024, 1, 1, 0, 0, 1), rsp.ruleTimestamp);
			var newRuleXml = ruleXml.Replace("<FCR_Parameters>&lt;a&gt;CR5RESWIZ - rule global - 1&lt;/a&gt;</FCR_Parameters>",
				"<FCR_Parameters>&lt;a&gt;CR5RESWIZ - rule global - 1 - (updated)&lt;/a&gt;</FCR_Parameters>");
			newRuleXml = newRuleXml.Replace("<TimestampUtc>2024-01-01T00:00:00Z</TimestampUtc>", "<TimestampUtc>2024-01-01T00:00:01Z</TimestampUtc>");
			AssertContains(newRuleXml, rsp.ruleContent);
			AssertEquals(newRuleXml, licenceDatabase.LD_FeatureControlRuleLastSyncContent);

			rsp = CallController(trustedSystem, "CW1", "1024", new DateTime(2024, 1, 1, 0, 0, 1));
			AssertEquals(new DateTime(2024, 1, 1, 0, 0, 1), rsp.ruleTimestamp);
			AssertEquals(null, rsp.ruleContent);
			AssertEquals(newRuleXml, licenceDatabase.LD_FeatureControlRuleLastSyncContent);
		}

		(DateTime ruleTimestamp, byte[] ruleContentRaw, string ruleContent) CallController(EdiTrustedSystem trustedSystem, string product, string systemId, DateTime clientRuleTimestap)
		{
			var logger = new NLogWrapperForTest(GetType());
			var controller = CreateController(logger);
			var context = BuildContext(controller, trustedSystem, product, systemId, "", clientRuleTimestap);
			controller.GetFeatureControlRuleCore_Exposed(context);
			var rsp = context.ResponseInfo;

			if (rsp.RuleTimestampUtc != DateTime.MinValue)
			{
				AssertEquals(DateTimeKind.Utc, rsp.RuleTimestampUtc.Kind);
			}

			return (rsp.RuleTimestampUtc, rsp.RuleContent, rsp.RuleContent == null ? null : DecompressString(rsp.RuleContent));
		}

		public static string DecompressString(byte[] compressedBytes)
		{
			using (var ms = new MemoryStream(compressedBytes))
			{
				using (var zipStream = new GZipStream(ms, CompressionMode.Decompress))
				{
					using (var resultStream = new MemoryStream())
					{
						zipStream.CopyTo(resultStream);
						return Encoding.UTF8.GetString(resultStream.ToArray());
					}
				}
			}
		}

		TrustedContext<FeatureControlRequest, FeatureControlResponse> BuildContext(TrustedController controller,
			EdiTrustedSystem trustedSystem, string product, string systemId, string tenantId, DateTime ruleTimestamp)
		{
			var request = new FeatureControlRequest()
			{
				Product = product,
				SystemId = systemId,
				TenantId = tenantId,
				InfoExpires = ZDateTime.UtcNow.AddMinutes(5).ToDateTime(),
				RuleTimestampUtc = ruleTimestamp
			};

			return new TrustedContextForTest<FeatureControlRequest, FeatureControlResponse>(product, systemId, request, trustedSystem, controller) { Success = true };
		}

		FeatureControlBaseControllerForTest CreateController(NLogWrapper logger)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://unit-testing/api/FeatureControl/Rule");
			requestMessage.Content = new StringContent("{ }", Encoding.UTF8, "application/json");
			var controller = new FeatureControlBaseControllerForTest(logger);
			controller.Request = requestMessage;
			return controller;
		}

		public class FeatureControlBaseControllerForTest : FeatureControlBaseController
		{
			public FeatureControlBaseControllerForTest() : base()
			{
			}

			public FeatureControlBaseControllerForTest(NLogWrapper logger) : base(logger)
			{
			}

			public void GetFeatureControlRuleCore_Exposed(TrustedContext<FeatureControlRequest, FeatureControlResponse> context)
				=> base.GetFeatureControlRuleCore(context);
		}
	}
}
