using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	public class ExceptionXmlTest : TestCaseWithXmlDoc
	{
		public void TestOccurrenceID()
		{
			const string exceptionId1 = "E00000001-EDI-DAT";
			const string exceptionId2 = "E00000002-EDI-DAT";
			string occurrence1 = CreateOccurrenceXml(ZDateTime.Today, new ZDateTime(2005, 6, 8, 10, 56, 08), exceptionId1);
			string occurrence2 = CreateOccurrenceXml(ZDateTime.Today, new ZDateTime(2005, 6, 8, 10, 56, 08), exceptionId2);

			var logs = new ScalableHelpErrorLogCollection(Factory);
			var log = new ExceptionXml(occurrence1).Process(logs, false);
			AssertEquals("Log Occurrence Count", 1, log.Occurrences.Count);
			AssertEquals(exceptionId1, log.Occurrences[0].HO_ExceptionID);

			var log1 = new ExceptionXml(occurrence2).Process(logs, false);
			AssertEquals(log, log1);
			log.Occurrences.Load();
			AssertEquals("Log Occurrence Count", 2, log.Occurrences.Count);
			AssertEquals(exceptionId2, log.Occurrences[1].HO_ExceptionID);
		}

		protected override void SetUp()
		{
			base.SetUp();
			{
				EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = new ZDateTime(2000, 1, 1, 0, 0, 0);
				var thresholdCollection = new IssueWorkItemCreationThresholdCollection();
				var threshold = thresholdCollection.AddNew();
				threshold.IssueOccurrenceThreshold = 1;
				threshold.ThresholdTimespan = 1;
				EDIDataRegistry.Instance.IssueWorkItemCreationThresholdClientVisible.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, thresholdCollection);
			}
		}

		#region TestAccurateExceptionSource

		class ExceptionInnerClass
		{
			public class ExceptionInnerMostClass
			{
				public static void ExceptionFunction(int level)
				{
					string str = null;

					if (level == 0)
					{
						if (str.Length > 0)
						{
							return;
						}
					}

					ExceptionFunction(level - 1);
				}
			}
		}

		void AddIssueForTest()
		{
			TestCaseHelper.ClearTable("HelpErrorLogOccurrence");
			TestCaseHelper.ClearTable("HelpErrorLogKey");
			TestCaseHelper.ClearTable("HelpErrorLog");

			ExceptionReportBuilder testAttachmentBuilder = null;

			try
			{
				ExceptionInnerClass.ExceptionInnerMostClass.ExceptionFunction(3);
			}
			catch (Exception ex)
			{
				var reportArgs = new ExceptionReportArgs(ex, "Test Error Id", "Test Key add line no", "Test Error add line no");
				reportArgs.SessionId = new Guid("15F3F612-6749-4F45-92E5-713ED6D863E9");
				reportArgs.Sequence = 5678;
				testAttachmentBuilder = new ExceptionReportBuilder(reportArgs);
			}

			string attachment = testAttachmentBuilder.GenerateReport();

			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			var errorLogs = new ScalableHelpErrorLogCollection(factory);

			new ExceptionXml(attachment).Process(errorLogs, true);
		}

		public void TestAccurateExceptionSource()
		{
			AddIssueForTest();

			var xmlString = (string)Db.Connection.ExecuteScalar("SELECT TOP 1 dbo.ClrUncompressAsString(HO_CompressedXMLData) FROM dbo.HelpErrorLogOccurrence");
			var logs = new ScalableHelpErrorLogCollection(Factory);
			var xml = new ExceptionXml(xmlString);
			var log = xml.Process(logs, shouldSave: false);
			AssertEquals("ExceptionSource is accurate now.", "NUnitCore.dll", log.HE_ExceptionSource);
		}

		#endregion

		public void TestExceptionIDCanBeLongerThan9Digits()
		{
			const string exceptionId = "E00000001-EDI-DAT";
			var occurrenceXml = CreateOccurrenceXml(ZDateTime.Today, new ZDateTime(2005, 6, 8, 10, 56, 08), exceptionId);
			var logs = new ScalableHelpErrorLogCollection(Factory);
			var log = new ExceptionXml(occurrenceXml).Process(logs, shouldSave: false);
			AssertEquals("Full exception ID should be retrieved", exceptionId, log.Occurrences[0].HO_ExceptionID);
		}

		public void TestExceptionIDCanBe19Digits()
		{
			const string exceptionId = "R123456789012345678";
			var occurrenceXml = CreateOccurrenceXml(ZDateTime.Today, new ZDateTime(2005, 6, 8, 10, 56, 08), exceptionId);
			var logs = new ScalableHelpErrorLogCollection(Factory);
			var log = new ExceptionXml(occurrenceXml).Process(logs, shouldSave: false);
			AssertEquals("Full exception ID should be retrieved", exceptionId, log.Occurrences[0].HO_ExceptionID);
		}

		public void TestProcess_IsClientVisible()
		{
			string xml = File.ReadAllText(ClientVisibleTestFile);
			var logs = new ScalableHelpErrorLogCollection(Factory);
			var log = new ExceptionXml(xml).Process(logs, false);

			AssertEquals(ZBool.True, log.HE_IsClientVisible);
		}

		public void TestProcess_NotIsClientVisible()
		{
			string xml = File.ReadAllText(SilentExceptionTestFile);
			var logs = new ScalableHelpErrorLogCollection(Factory);
			var log = new ExceptionXml(xml).Process(logs, false);

			AssertEquals(ZBool.False, log.HE_IsClientVisible);
		}

		public void TestProcessWithGuid()
		{
			var logs = new ScalableHelpErrorLogCollection(Factory);

			var xmlWithKey = File.ReadAllText(SmallCallstackTestFileWithKey);
			var log = new ExceptionXml(xmlWithKey).Process(logs, false);

			var xmlWithGuidKey = File.ReadAllText(SmallCallstackTestFileWithGuidKey);
			var log1 = new ExceptionXml(xmlWithGuidKey).Process(logs, false);

			AssertNotEquals("GUID should not be removed from key", log, log1);
			AssertEquals(1, log.Keys.Count);
			AssertEquals(1, log1.Keys.Count);
			log.Occurrences.Load();
			log1.Occurrences.Load();
			AssertEquals(1, log.Occurrences.Count);
			AssertEquals(1, log1.Occurrences.Count);
		}

		public void TestProcess_WithMatchingLog()
		{
			var xmlWithoutKey = File.ReadAllText(SmallCallstackTestFile);
			var xmlWithKey = File.ReadAllText(SmallCallstackTestFileWithKey);

			var logs = new ScalableHelpErrorLogCollection(Factory);

			var log = new ExceptionXml(xmlWithoutKey).Process(logs, false);
			Factory.Save();
			AssertEquals("log.Keys.Count", 1, log.Keys.Count);
			AssertEquals("log.Keys[0].HK_Key", "at Dummy.ForTest", log.Keys[0].HK_Key);
			AssertEquals("log.Occurrences.Count", 1, log.Occurrences.Count);

			var log1 = new ExceptionXml(xmlWithKey).Process(logs, false);
			Factory.Save();
			AssertEquals("log.Keys.Count", 1, log.Keys.Count);
			AssertEquals("log1.Keys.Count", 1, log1.Keys.Count);
			AssertEquals("log.Keys[0].HK_Key", "at Dummy.ForTest", log.Keys[0].HK_Key);
			AssertEquals("log1.Keys[0].HK_Key", "Some Key", log1.Keys[0].HK_Key);
			AssertEquals("log.Occurrences.Count", 1, log.Occurrences.Count);
			AssertEquals("log1.Occurrences.Count", 1, log1.Occurrences.Count);

			var logb = new ExceptionXml(xmlWithoutKey).Process(logs, false);
			Factory.Save();
			AssertEquals("Logs", log, logb);
			AssertEquals("log.Keys.Count", 1, log.Keys.Count);
			AssertEquals("log1.Keys.Count", 1, log1.Keys.Count);
			AssertEquals("log.Keys[0].HK_Key", "at Dummy.ForTest", log.Keys[0].HK_Key);
			AssertEquals("log1.Keys[0].HK_Key", "Some Key", log1.Keys[0].HK_Key);
			log.Occurrences.Load();
			log1.Occurrences.Load();
			AssertEquals("log.Occurrences.Count", 2, log.Occurrences.Count);
			AssertEquals("log1.Occurrences.Count", 1, log1.Occurrences.Count);

			var log1b = new ExceptionXml(xmlWithKey).Process(logs, false);
			Factory.Save();
			AssertEquals("Logs", log1, log1b);
			AssertEquals("Logs[0].Keys.Count", 1, log.Keys.Count);
			AssertEquals("log1.Keys.Count", 1, log1.Keys.Count);
			AssertEquals("Logs[0].Keys[0].HK_Key", "at Dummy.ForTest", log.Keys[0].HK_Key);
			AssertEquals("log1.Keys[0].HK_Key", "Some Key", log1.Keys[0].HK_Key);
			log.Occurrences.Load();
			log1.Occurrences.Load();
			AssertEquals("Logs[0].Occurrences.Count", 2, log.Occurrences.Count);
			AssertEquals("log1.Occurrences.Count", 2, log1.Occurrences.Count);
		}

		public void TestProcessWithMatchingLicenceHeader()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_FullName = "PEANUT BUTTER CORPORATION";
			org.CreateAndLoadLicenceForOrg();

			var database = org.LicCompany.LicDatabases.AddNew();
			database.LD_HostServerName = "PEANUT";
			database.LD_HostDBName = "BUTTER";

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "AAA";
			clientCompany.LCC_LD = database.PK;
			clientCompany.LCC_OH = org.PK;

			Factory.Save();

			var xmlData = @"
<EDI_Exception_Report>
	<Company>Peanut Butter Corporation</Company>
	<DBServerName>PEANUT</DBServerName>
	<ExceptionDetails>
		<EnvironmentInfo>
			<DatabaseInfo>
				<MainDatabaseName>BUTTER</MainDatabaseName>
			</DatabaseInfo>
		</EnvironmentInfo>
	</ExceptionDetails>
</EDI_Exception_Report>";

			var xml = new ExceptionXml(xmlData);
			var errorLogs = new ScalableHelpErrorLogCollection(Factory);
			var log = xml.Process(errorLogs, true);
			AssertEquals("log.Occurrences.Count", 1, log.Occurrences.Count);
			AssertEquals("log.Occurrences[0].HO_LD", database.PK, log.Occurrences[0].HO_LD);
			AssertEquals("log.RelatedIncidents.Count", 0, log.RelatedIncidents.Count);

			log.Logs.AddNew(Events.AutoMatchDone);
			log.RelatedWorkItems.AddNew();
			log.HE_FixedDate = ZDateTime.UtcNow; // normally, all related work items should be closed for Fixed Date to be populated
			Factory.Save();
			var logb = xml.Process(errorLogs, true);
			Factory.Save();
			AssertEquals("errorLogs.Count", log, logb);
			var err = new BusinessObjectFactory().Load<EdiHelpErrorLog>(log.PK);
			AssertEquals("errorLogs[0].Occurrences.Count", 2, err.Occurrences.Count);
			AssertEquals("errorLogs[0].RelatedIncidents.Count", 1, err.RelatedIncidents.Count);
			AssertEquals("errorLogs[0].RelatedIncidents[0].IM_LCC", clientCompany.PK, err.RelatedIncidents[0].IM_LCC);

			var logc = xml.Process(errorLogs, true);
			Factory.Save();
			err = new BusinessObjectFactory().Load<EdiHelpErrorLog>(log.PK);
			AssertEquals("errorLogs.Count", log, logc);
			AssertEquals("errorLogs[0].Occurrences.Count", 3, err.Occurrences.Count);
			AssertEquals("errorLogs[0].RelatedIncidents.Count", 1, err.RelatedIncidents.Count);
		}

		public void TestProcessWithMatchingClientCompany()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_FullName = "PEANUT BUTTER CORPORATION";
			org.CreateAndLoadLicenceForOrg();
			org.LicenceEnterpriseCode = "DDD";
			org.LicCompany.LC_CompanyCode = "AAA";

			var database = org.LicCompany.LicDatabases.AddNew();
			database.LD_ServerCode = "HST";
			var licHeader = org.LicCompany.GetHeader(database);

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "AAA";
			clientCompany.LCC_LD = database.PK;

			Factory.Save();

			var xmlData = @"
<EDI_Exception_Report>
	<Company>Peanut Butter Corporation</Company>
	<DBServerName>PEANUT</DBServerName>
	<LicenceEnterpriseCode>DDD</LicenceEnterpriseCode>
	<LicenceCompanyCode>AAA</LicenceCompanyCode>
	<LicenceServerCode>HST</LicenceServerCode>
	<ExceptionDetails>
		<EnvironmentInfo>
			<DatabaseInfo>
				<MainDatabaseName>BUTTER</MainDatabaseName>
			</DatabaseInfo>
		</EnvironmentInfo>
	</ExceptionDetails>
</EDI_Exception_Report>";

			var xml = new ExceptionXml(xmlData);
			var errorLogs = new ScalableHelpErrorLogCollection(Factory);
			var log = xml.Process(errorLogs, true);
			AssertEquals("log.Occurrences.Count", 1, log.Occurrences.Count);
			AssertEquals("log.Occurrences[0].HO_LD", database.PK, log.Occurrences[0].HO_LD);
			AssertEquals("log.Occurrences[0].HO_LCC", clientCompany.PK, log.Occurrences[0].HO_LCC);
		}

		public void TestUserLoginName()
		{
			var xml = new ExceptionXml("<EDI_Exception_Report><LoginName>Bob</LoginName></EDI_Exception_Report>");
			AssertEquals("UserLoginName", "Bob", xml.UserLoginName);
		}

		public void TestProcess_ShouldTryMatchingAgainstLicenceCode()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicEnterprise.LE_EnterpriseCode = "EDI";
			org.LicCompany.LC_CompanyCode = "EDI";
			org.LicCompany.LicDatabases.AddNew().LD_ServerCode = "DAT";
			Factory.Save();

			var ex = new Exception("Exception");
			var reportArgs = new ExceptionReportArgs(ex, "Test Error Id", "Test Key", "Test Error");
			var attachmentBuilder = new ExceptionReportBuilder(reportArgs);
			var xmlData = attachmentBuilder.GenerateReport();
			var exceptionXml = new ExceptionXml(xmlData);
			var errorLogs = new ScalableHelpErrorLogCollection(Factory);
			var log = exceptionXml.Process(errorLogs, false);
			AssertEquals(1, log.Occurrences.Count);
			AssertEquals(org.LicCompany.LicDatabases[0], log.Occurrences[0].Database);
		}

		public void TestProcess_DoNotProcessIssuesFromOldSystem()
		{
			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = new ZDateTime(2009, 9, 4, 0, 0, 0);

			var xml = File.ReadAllText(ClientVisibleTestFile);
			var logs = new ScalableHelpErrorLogCollection(Factory);
			var log = new ExceptionXml(xml).Process(logs, false);
			AssertNull(log);

			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = new ZDateTime(2000, 9, 4, 0, 0, 0);
			log = new ExceptionXml(xml).Process(logs, false);
			AssertNotNull(log);
		}

		public void TestProcess_HtmlWithDtd()
		{
			var xml = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\"> <html></html>";
			var logs = new ScalableHelpErrorLogCollection(Factory);
			AssertNoExceptionThrown(() => new ExceptionXml(xml));
		}

		public void TestProcessGlowWindowsCEReport()
		{
			var xml = File.ReadAllText(SampleGlowWindowsCEFile);
			var logs = new ScalableHelpErrorLogCollection(Factory);
			var log = new ExceptionXml(xml).Process(logs, false);
			AssertEquals("CargoWise One CE Mobility (Glow)", log.HE_ExceptionSource);
			AssertEquals("File or assembly name 'OpenNETCF.Net, Version=2.3.12004.0, Culture=neutral, PublicKeyToken=E60DBEA84BB431B7', or one of its dependencies, was not found.", log.HE_ExceptionMessage);
		}

		public void TestProcessGlowWeb_StripsBaseURLAndVersionFromLogKey()
		{
			var xml = File.ReadAllText(SampleGlowWebFileWithFullStackTraceData);
			var logs = new ScalableHelpErrorLogCollection(Factory);

			var exceptionXml = new ExceptionXml(xml);
			var log = exceptionXml.Process(logs, false);

			var expectedLogKey = @"Object.e.implementation.createException@JS/Desktop/main.js?v=<VERSION>:23:10951
Object.e.implementation.run@JS/Desktop/main.js?v=<VERSION>:23:10826
e@JS/Desktop/main.js?v=<VERSION>:23:10679
g@JS/Desktop/main.js?v=<VERSION>:23:22166
Function.h.forPromise@JS/Desktop/main.js?v=<VERSION>:23:23193
Object.t.reject@JS/Desktop/main.js?v=<VERSION>:23:21296
{anonymous}(
o@JS/Desktop/main.js?v=<VERSION>:7:9264
{anonymous}(
m.when@JS/Desktop/main.js?v=<VERSION>:7:4913
Object.e.implementation.createException@JS/Desktop/main.js?v=<VERSION>:23:10951
Object.e.implementation.run@JS/Desktop/main.js?v=<VERSION>:23:10826
e@JS/Desktop/main.js?v=<VERSION>:23:10679
g@JS/Desktop/main.js?v=<VERSION>:23:22166
Object.e.then.e.pipe@JS/Desktop/main.js?v=<VERSION>:23:21747
i.load@JS/Desktop/main.js?v=<VERSION>:21:21641
i.read@JS/Desktop/main.js?v=<VERSION>:21:22012
l@JS/Desktop/main.js?v=<VERSION>:4:21280
Function.d [as peek]@JS/Desktop/main.js?v=<VERSION>:4:21898
Function.s.load@JS/Desktop/main.js?v=<VERSION>:21:21320
Object.e.implementation.createException@JS/Desktop/main.js?v=<VERSION>:23:10951
Object.e.implementation.run@JS/Desktop/main.js?v=<VERSION>:23:10826
e@JS/Desktop/main.js?v=<VERSION>:23:10679
g@JS/Desktop/main.js?v=<VERSION>:23:22166
Function.h.forPromise@JS/Desktop/main.js?v=<VERSION>:23:23193
Object.e.rejectWith@JS/Desktop/main.js?v=<VERSION>:23:21484
{anonymous}(
d@JS/Desktop/main.js?v=<VERSION>:1:16770
Object.f.fireWith@JS/Desktop/main.js?v=<VERSION>:1:17550";

			AssertEquals("Final key should not include the server path, or the software version.", expectedLogKey.Trim(), exceptionXml.KeyFields.LogKey.Trim());
			AssertContains("Raw stack trace should still include the server path and software version", "Object.e.implementation.createException@https://uatglowtitanplay.corporate.cargowise.com/Portals/JS/Desktop/main.js?v=15.8.10.6:23:10951", log.Occurrences[0].HO_XMLData);
		}

		public void TestProcessGlowWeb_StripsBaseURLAndVersionFromLogKey2()
		{
			var xml = File.ReadAllText(SampleGlowWebFileWithFullStackTraceData2);
			var logs = new ScalableHelpErrorLogCollection(Factory);

			var exceptionXml = new ExceptionXml(xml);
			var log = exceptionXml.Process(logs, false);

			var expectedLogKey = @"Message: Unable to process binding ""ifnot: function (
Message: Unable to process binding ""if: function (
Message: e.map is not a function
at JS/Shared/Services/DocumentService.js?v=<VERSION>:1:2218
at JS/Desktop/main.js?v=<VERSION>:1:18106
at d (
at Object.p.add [as done] (
at Array.<anonymous> (
at Function.Y.extend.each (
at JS/Desktop/main.js?v=<VERSION>:1:18024
at Function.e.Deferred (
at Object.i.then (
at Object.e.then.e.pipe (";

			AssertEquals("Final key should not include the server path, or the software version.", expectedLogKey.Trim(), exceptionXml.KeyFields.LogKey.Trim());
			AssertContains("Raw stack trace should still include the server path and software version", "at https://uatglowtitanplay.corporate.cargowise.com/Portals/JS/Shared/Services/DocumentService.js?v=15.10.3.0:1:2218", log.Occurrences[0].HO_XMLData);
		}

		public void TestInvalidXmlHasInvalidXmlAsKey()
		{
			var xml = new ExceptionXml("SomeInvalidXMLRubbishDoDa");
			xml.PopulateFromXML();
			Assert(!xml.IsValid);
			AssertEquals("InvalidXml", xml.KeyFields.LogKey);
			AssertEquals(typeof(XmlException).ToString(), xml.KeyFields.Type);
			AssertEquals("Container for all issues with invalid XML", xml.KeyFields.Source);
			AssertEquals("Container for all issues with invalid XML", xml.KeyFields.Message);
		}

		public void TestNullXml()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new ExceptionXml(null); });
		}

		public void TestEmptyXml()
		{
			const string Source = "System.Xml";
			const string Message = "Root element is missing.";
			AssertInvalidXml(new ExceptionXml(""), typeof(XmlException), Source, Message, "");
		}

		public void TestInvalidXml()
		{
			const string XmlData = "<unclosed-tag>";
			const string Source = "System.Xml";
			const string Message = "Unexpected end of file has occurred. The following elements are not closed: unclosed-tag. Line 1, position 15.";
			var xml = new ExceptionXml(XmlData);
			AssertInvalidXml(xml, typeof(XmlException), Source, Message, XmlData);

			var logs = NewLogs;
			xml.Process(logs, true);
			AssertInvalidXml(xml, typeof(XmlException), "Container for all issues with invalid XML", "Container for all issues with invalid XML", XmlData);
		}

		public void TestValidXmlWithDBServerNameOverMaxLength()
		{
			var xml = new ValidXml();
			xml.SingleNode("DBServerName").InnerXml = "DBServerName that is very long".PadRight(500, 'X');
			xml.Process(NewLogs, true);
			AssertEquals("Errors to Cargowise", 0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestValidXml()
		{
			var logs = NewLogs;
			var xml = new ValidXml();
			AssertEquals("IsValid", true, xml.IsValid);
			xml.AssertProcess(logs, 1);

			xml = new ValidXml();
			xml.SingleNode("TimeOfException").InnerText = xml.ExceptionTimeForOccurrence2.ToString("u");
			xml.AssertProcess(logs, 2);
		}

		public void TestProcessEmptyExceptionTime()
		{
			ValidXml xml = new ValidXml();
			xml.SingleNode("TimeOfException").InnerText = "";
			xml.Process(NewLogs, true);
			Assert("TimeOfException should not be valid", !xml.ExceptionTime.IsValid);

			var log = xml.LogLoadedFromDatabase();
			AssertEquals("Occurrences count", 1, log.Occurrences.Count);
			Assert("Occurrence ExceptionDateTime should not be valid", !log.Occurrences[0].HO_ExceptionDateTime.IsValid);
			Assert("Log FirstProcessed should be valid", log.HE_FirstProcessed.IsValid);
		}

		public void TestProcessUtcExceptionTime()
		{
			ValidXml xml = new ValidXml();
			xml.SingleNode("TimeOfException").InnerText = xml.ExceptionTimeForOccurrence2.ToString("u");
			xml.Process(NewLogs, true);

			AssertEquals(xml.ExceptionTimeForOccurrence2, xml.ExceptionTime);
			AssertEquals(DateTimeKind.Utc, xml.ExceptionTime.Kind);
		}

		public void TestProcessOldUtcExceptionTimeInXml()
		{
			var xmlData = @"
<EDI_Exception_Report>
	<Company>Peanut Butter Corporation</Company>
	<DBServerName>PEANUT</DBServerName>
	<TimeOfException>2023-11-29T03:23:55.1850122Z</TimeOfException>
	<ExceptionDetails>
		<EnvironmentInfo>
			<DatabaseInfo>
				<MainDatabaseName>BUTTER</MainDatabaseName>
			</DatabaseInfo>
		</EnvironmentInfo>
	</ExceptionDetails>
</EDI_Exception_Report>";
			var xml = new ExceptionXml(xmlData);
			var errorLogs = new ScalableHelpErrorLogCollection(Factory);
			xml.Process(errorLogs, false, true);

			AssertEquals("2023-11-29 03:23:55Z", xml.ExceptionTime.ToString("u"));
		}

		public void TestProcessOldLocalExceptionTimeWithTimeZoneOffsetInXml()
		{
			var xmlData = @"
<EDI_Exception_Report>
	<Company>Peanut Butter Corporation</Company>
	<DBServerName>PEANUT</DBServerName>
	<TimeOfException>2023-11-29T13:23:55.1850122+10:00</TimeOfException>
	<ExceptionDetails>
		<EnvironmentInfo>
			<DatabaseInfo>
				<MainDatabaseName>BUTTER</MainDatabaseName>
			</DatabaseInfo>
		</EnvironmentInfo>
	</ExceptionDetails>
</EDI_Exception_Report>";
			var xml = new ExceptionXml(xmlData);
			var errorLogs = new ScalableHelpErrorLogCollection(Factory);
			xml.Process(errorLogs, false, true);

			AssertEquals("2023-11-29 03:23:55Z", xml.ExceptionTime.ToString("u"));
		}

		public void TestProcessOldLocalExceptionTimeWithNoTimeZoneOffsetInXml()
		{
			var xmlData = @"
<EDI_Exception_Report>
	<Company>Peanut Butter Corporation</Company>
	<DBServerName>PEANUT</DBServerName>
	<TimeOfException>2023-11-29T13:23:55.185</TimeOfException>
	<ExceptionDetails>
		<EnvironmentInfo>
			<DatabaseInfo>
				<MainDatabaseName>BUTTER</MainDatabaseName>
			</DatabaseInfo>
		</EnvironmentInfo>
	</ExceptionDetails>
</EDI_Exception_Report>";
			var xml = new ExceptionXml(xmlData);
			var errorLogs = new ScalableHelpErrorLogCollection(Factory);
			xml.Process(errorLogs, false, true);

			var cultureInfo = new CultureInfo("");
			cultureInfo.DateTimeFormat.DateSeparator = "-";
			cultureInfo.DateTimeFormat.TimeSeparator = ":";
			cultureInfo.DateTimeFormat.ShortDatePattern = "dd/MMM/yy";
			cultureInfo.DateTimeFormat.ShortTimePattern = "hh:mm:ss";
			DateTime.TryParse("2023-11-29T13:23:55.185", cultureInfo, DateTimeStyles.None, out DateTime d);
			AssertEquals(d.ToUniversalTime().ToString("u"), xml.ExceptionTime.ToString("u"));
		}

		public void TestProcessVeryBigType()
		{
			AssertVeryBigValue("ExceptionType");
		}

		public void TestProcessVeryBigSource()
		{
			AssertVeryBigValue("Source");
		}

		public void TestProcessVeryBigMessage()
		{
			AssertVeryBigValue("Message");
		}

		public void TestProcessVeryBigCallStack()
		{
			AssertVeryBigValue("StackTrace/Call");
		}

		public void TestCallStackText()
		{
			AssertEquals("CallStack with Exception Processing Calls removed", ExpectedCallStack, new ValidXml().CallStackText());
		}

		public void TestErrorReportID()
		{
			var exceptionXml = new ExceptionXml(ErrorReportIDOnlyText);
			var logs = NewLogs;

			var log = exceptionXml.Process(logs, shouldSave: false);
			AssertEquals("Should have one occurence", 1, log.Occurrences.Count);

			var occurence = log.Occurrences[0];
			AssertEquals("R123456789012345678", occurence.HO_ExceptionID);
		}

		public void TestErrorReportIDWhenEnterpriseNumberFountainFails()
		{
			var exceptionXml = new ExceptionXml(ErrorReportIDOnlyText_EnterpriseNumberFountainFailed);
			var logs = NewLogs;

			var log = exceptionXml.Process(logs, shouldSave: false);
			AssertEquals("Should have one occurence", 1, log.Occurrences.Count);

			var occurence = log.Occurrences[0];
			AssertEquals("Unknown", occurence.HO_ExceptionID); // YSM: Temporary reversion to old (untested) behaviour
		}

		public void TestSessionId()
		{
			var id1 = "791fa99e-96c2-43dc-8185-ba38961d45cb";
			var id2 = "ef6b4159-966f-4034-a068-101e1c2e41d4";

			AssertSessionID(id1, null, id1, "HO_SessionID should match SessionId from XML");
			AssertSessionID(null, id2, id2, "If there is no SessionId, then we should fall back to SessionID");
			AssertSessionID(id1, id2, id1, "Should prefer SessionId to SessionID. There should be no problem with both in the same report");
			AssertSessionID(null, null, Guid.Empty.ToString(), "If there is neither then it should be empty");

			void AssertSessionID(string sessionId, string sessionID, string expected, string message)
			{
				var xmlText = ErrorReportSessionId(sessionId, sessionID);
				var exceptionXml = new ExceptionXml(xmlText);
				var log = exceptionXml.Process(NewLogs, shouldSave: false);
				var result = log.Occurrences.Cast<HelpErrorLogOccurrence>().Single();

				AssertEquals(message, expected, result.HO_SessionID.ToString());
			}
		}

		public void TestFromTestRigAndTestRigOriginParsedCorrectly()
		{
			// Arrange
			var xml = new ValidXml(WithTestRigOrigin);

			// Act
			xml.PopulateFromXML();

			// Assert
			AssertEquals("FromTestRig is set correctly", true, xml.fromTestRigExposed);
			AssertEquals("TestRigOrigin is set correctly", "WI00761180", xml.testRigOriginExposed);
		}

		public void TestProcess_WithIssueLogHavingNonEnglishCharacters()
		{
			var xml = File.ReadAllText(SampleNonEnglishMessageExceptionXML);
			var logs = new ScalableHelpErrorLogCollection(Factory);
			var log = new ExceptionXml(xml).Process(logs, false);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var savedLog = newFactory.Load<EdiHelpErrorLog>(log.PK);

			AssertEquals("Message", "无法从传输连接中读取数据: 你的主机中的软件中止了一个已建立的连接。。", savedLog.HE_ExceptionMessage);
			AssertEquals("log.Occurrences.Count", 1, savedLog.Occurrences.Count);
		}

		public void TestProcess_WithIssueLogHavingNonEnglishCharactersWIShouldGetCretedProperly()
		{
			InitializeSettingForClientVisible();
			var logs = new ScalableHelpErrorLogCollection(Factory);
			var mockAssignment = new Mock<IIssueAssignmentCalculator>();
			mockAssignment.Setup(o => o.GetAssignment(It.IsAny<EdiHelpErrorLog>(), It.IsAny<DataFormatter>())).Returns(new IssueAssignment("PRD", "ARE", "MOD"));
			var today = ZDateTime.Today;
			var issue = new ExceptionXml(CreateOccurrenceXml(today, today, "E00000001-EDI-DAT", "无法从传输连接中读取数据: 你的主机中的软件中止了一个已建立的连接。。", "key1")) { IssueAssignmentCalculator = mockAssignment.Object };
			var log = issue.Process(logs, false);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var savedLog = newFactory.Load<EdiHelpErrorLog>(log.PK);

			AssertEquals("Message", "无法从传输连接中读取数据: 你的主机中的软件中止了一个已建立的连接。。", savedLog.HE_ExceptionMessage);
			AssertEquals("log.Occurrences.Count", 1, savedLog.Occurrences.Count);

			Assert(savedLog.HasWorkItems);
			var firstWorkItem = savedLog.RelatedWorkItems.SingleOrDefault() as WorkItem;
			AssertEquals("无法从传输连接中读取数据: 你的主机中的软件中止了一个已建立的连接。。", firstWorkItem.WKI_Summary);
		}
		void InitializeSettingForClientVisible()
		{
			var thresholdCollection = new IssueWorkItemCreationThresholdCollection();
			var threshold = thresholdCollection.AddNew();
			threshold.IssueOccurrenceThreshold = 1;
			threshold.ThresholdTimespan = 7;
			EDIDataRegistry.Instance.IssueWorkItemCreationThresholdClientVisible.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, thresholdCollection);
		}

		protected void AssertVeryBigValue(string propertyName)
		{
			int beforeCount = Factory.GetDatabaseCount(typeof(EdiHelpErrorLog));

			var xml = new ValidXml();
			xml.SingleNode("ExceptionDetails/" + propertyName).InnerXml = propertyName + " that is very long".PadRight(99999, 'X');
			xml.Process(NewLogs, true);
			AssertEquals("Exception should exist in DB after it has been processed", beforeCount + 1, Factory.GetDatabaseCount(typeof(EdiHelpErrorLog)));
		}

		protected void AssertInvalidXml(ExceptionXml xml, Type exceptionType, string source, string message, string xmlData)
		{
			AssertEquals("IsValid", false, xml.IsValid);

			Assert("TimeOfException", xml.ExceptionTime.IsValid);
			AssertEquals("Type", exceptionType.ToString(), xml.KeyFields.Type);
			AssertEquals("Source", source, xml.KeyFields.Source);
			AssertEquals("Message", message, xml.KeyFields.Message);
			AssertEquals("Stack", "", xml.KeyFields.CallStack);
			AssertEquals("XmlData", xmlData, xml.XmlData);
		}

		protected IHelpErrorLogCollection NewLogs
		{
			get { return new ScalableHelpErrorLogCollection(Factory); }
		}

		const string ExpectedCallStack = "at Enterprise.Startup.TestRig.AnotherStackFrame2(\r\n" +
				"at Enterprise.Startup.TestRig.AnotherStackFrame1(\r\n" +
				"at Enterprise.Startup.TestRig.ThrowExceptionButton_Click(\r\n";

		#region class ValidXml

		internal class ValidXml : ExceptionXml
		{
			public ValidXml()
				: base(ValidXmlText)
			{
			}

			public ValidXml(string xmlString)
				: base(xmlString)
			{
			}

			public void AssertProcess(IHelpErrorLogCollection logs, int occurrenceCount)
			{
				Process(logs, true, true);
				logs.Save();

				ZDateTime occurrenceExceptionTime = (occurrenceCount == 1) ? Values.ExceptionTime : ExceptionTimeForOccurrence2;

				AssertEquals("XML Type", Values.ExceptionType, KeyFields.Type);
				AssertEquals("XML Source", Values.Source, KeyFields.Source);
				AssertEquals("XML Message", Values.Message.TrimEnd(), KeyFields.Message);

				string line1, line2, line3;
				using (StringReader reader = new StringReader(KeyFields.CallStack))
				{
					line1 = reader.ReadLine();
					line2 = reader.ReadLine();
					line3 = reader.ReadLine();
					AssertNull("Only 3 Lines", reader.ReadLine());
				}

				AssertEquals("XML CallStack start", line1, "at Enterprise.Startup.TestRig.AnotherStackFrame2(");
				AssertEquals("XML CallStack start", line2, "at Enterprise.Startup.TestRig.AnotherStackFrame1(");
				AssertEquals("XML CallStack start", line3, "at Enterprise.Startup.TestRig.ThrowExceptionButton_Click(");

				AssertEquals("XML ServerName", Values.DBServerName, serverName);
				AssertEquals("XML Company", Values.Company, company);
				Assert("XML ExceptionTime", occurrenceExceptionTime.ToString("u") == ExceptionTime.ToString("u"));
				AssertEquals("XML VersionNumber", Values.VersionNumber, versionNumber);
				AssertEquals("XML ExeCreationTime", Values.ExeCreationTime.ToUniversalTime(), exeDate);

				AssertEquals("XML TestRigOrigin", Values.TestRigOrigin, testRigOrigin);
				AssertEquals("FromTestRig set correctly from XML", Values.FromTestRig, fromTestRig);

				EdiHelpErrorLog log = LogLoadedFromDatabase();
				AssertEquals("ExceptionType", Values.ExceptionType, log.HE_ExceptionType);
				AssertEquals("ExceptionSource", Values.Source, log.HE_ExceptionSource);
				AssertEquals("ExceptionMessage", Values.ExceptionMessage, KeyFields.Messages[0]);
				AssertEquals("Message", Values.Message.TrimEnd(), log.HE_ExceptionMessage);
				Assert("FirstReported", Values.ExceptionTime.ToString("u") == log.HE_FirstReported.ToString("u"));
				Assert("FirstProcessed", ZDateTime.UtcNow > log.HE_FirstProcessed && log.HE_FirstProcessed > ZDateTime.UtcNow.AddHours(-1));
				AssertEquals("FailCount", occurrenceCount, log.HE_FailCount);
				AssertEquals("Occurrences count", occurrenceCount, log.Occurrences.Count);

				HelpErrorLogOccurrence occurrence = log.Occurrences[occurrenceCount - 1];
				AssertEquals("ServerName", Values.DBServerName, occurrence.HO_ServerName);
				AssertEquals("Company", Values.Company, occurrence.HO_Company);
				AssertWithin2Minutes("ExceptionDateTime", occurrenceExceptionTime, occurrence.HO_ExceptionDateTime);
				AssertWithin2Minutes("ExeDateTime", Values.ExeCreationTime.ToUniversalTime(), occurrence.HO_EXEDateTime);
				AssertEquals("VersionNumber", Values.VersionNumber, occurrence.HO_VersionNumber);
				AssertEquals("HO_SessionID", Values.SessionId, occurrence.HO_SessionID);
				AssertEquals("HO_Sequence", Values.Sequence, occurrence.HO_Sequence);
			}

			public EdiHelpErrorLog LogLoadedFromDatabase()
			{
				ZQuery occurrenceQuery = new ZQuery();
				occurrenceQuery.AddToFilter(HelpErrorLogOccurrenceSchema.HO_Company, Values.Company);
				HelpErrorLogOccurrence[] occurrences = new BusinessObjectFactory().Load<HelpErrorLogOccurrence>(occurrenceQuery);
				Assert("Occurrences count", occurrences.Length == 1 || occurrences.Length == 2);

				ZQuery logQuery = new ZQuery();
				logQuery.AddToFilter(HelpErrorLogSchema.PK, occurrences[0].HO_HE);

				EdiHelpErrorLog[] logs = new BusinessObjectFactory().Load<EdiHelpErrorLog>(logQuery);
				AssertEquals("ErrorLog count", 1, logs.Length);
				return logs[0];
			}

			protected void AssertWithin2Minutes(string message, ZDateTime expected, ZDateTime actual)
			{
				Assert(message + " should be valid", actual.IsValid);
				int secondsDifference = (expected - actual).Duration().Seconds;
				Assert(message + " should be within 120 seconds of " + expected + " but was " + actual, secondsDifference <= 120);
			}

			static string ValidXmlText
			{
				get
				{
					using (StringWriter writer = new StringWriter())
					using (XmlTextWriter xtw = new XmlTextWriter(writer))
					{
						xtw.WriteStartElement("EDI_Exception_Report");

						xtw.WriteElementString("DBServerName", Values.DBServerName);
						xtw.WriteElementString("Company", Values.Company);
						xtw.WriteElementString("TimeOfException", Values.ExceptionTime.ToString("u"));
						xtw.WriteElementString("SessionId", Values.SessionId.ToString());
						xtw.WriteElementString("Sequence", Values.Sequence.ToString());
						xtw.WriteElementString("VersionNumber", Values.VersionNumber);
						xtw.WriteElementString("ExeCreationTime", Values.ExeCreationTime.ToString("u"));
						xtw.WriteElementString("ExceptionMessage", Values.ExceptionMessage);

						xtw.WriteStartElement("ExceptionDetails");

						xtw.WriteElementString("ExceptionType", Values.ExceptionType);
						xtw.WriteElementString("Message", Values.Message);
						xtw.WriteElementString("Source", Values.Source);

						xtw.WriteStartElement("StackTrace");

						xtw.WriteElementString("Calls", Values.Calls.Length.ToString());

						foreach (string call in Values.Calls)
						{
							xtw.WriteElementString("Call", call);
						}

						xtw.WriteEndElement();
						xtw.WriteEndElement();
						xtw.WriteEndElement();

						return writer.ToString();
					}
				}
			}

			public ZDateTime ExceptionTimeForOccurrence2
			{
				get { return exceptionTimeForOccurrence2; }
			}

			readonly ZDateTime exceptionTimeForOccurrence2 = Values.ExceptionTime.AddDays(1);

			internal bool fromTestRigExposed => fromTestRig;
			internal string testRigOriginExposed => testRigOrigin;

			#region class Values

			protected class Values
			{
				public const string DBServerName = "192.168.0.1 My server";
				public const string Company = "Test Company & Sons";
				public static readonly DateTime ExceptionTime = DateTime.Today.AddDays(-7).ToUniversalTime();
				public static Guid SessionId = Guid.NewGuid();
				public static int Sequence = 123;
				public static readonly string VersionNumber = "1.2.3.4";
				public static readonly DateTime ExeCreationTime = DateTime.Today.AddDays(-7).ToUniversalTime();

				public const string ExceptionType = "TestType";
				public const string ExceptionMessage = "Test Exception Message";
				public const string Message = "Test Message with <complicated> & 'annoying' \"Special\" characters and a trailing space ";
				public const string Source = "Test Source";

				public const bool FromTestRig = false;
				public const string TestRigOrigin = "";

				public static readonly string[] Calls = new string[]
				{
						"at System.Environment.GetStackTrace(Exception e) ",
						"at System.Environment.GetStackTrace(Exception e) ",
						"at System.Environment.get_StackTrace() ",
						"at Enterprise.ZArchitecture.Core.ExceptionFullTracer.CaptureTraceToReporter(Exception ex) ",
						"at Enterprise.ZArchitecture.Core.ExceptionDetails.WriteFullReport(XmlTextWriter Xtw) ",
						"at Enterprise.ZArchitecture.Core.ExceptionAttachmentBuilder.GenerateAttachment() ",
						"at Enterprise.ZArchitecture.Core.ExceptionEmailBuilder.Build() ",
						"at Enterprise.ZArchitecture.Core.BaseExceptionReporter.SendReport(IReportsException Reporter, ExceptionReportArgs ReportArgs) ",
						"at Enterprise.ZArchitecture.Core.BaseExceptionReporter.ReportSilently(String Message, Exception E) ",
						"at Enterprise.ZArchitecture.Core.BaseExceptionReporter.ReportDeveloperException(String Message, Exception E) ",
						"at Enterprise.ZArchitecture.Environment.UserNotification.ShowDeveloperException(String Message, Exception E) ",
						"at Enterprise.ZArchitecture.Environment.UserNotificationBase.ShowDeveloperErrorAlways(String Message, String Caption) ",
						"at Enterprise.ZArchitecture.ErrorReporter.ReportOnce(String Message) ",
						@"at Enterprise.Startup.TestRig.AnotherStackFrame2(Object sender, EventArgs e) in Q:\Enterprise\Enterprise\Startup\GuiTest.cs:line 2882",
						@"at Enterprise.Startup.TestRig.AnotherStackFrame1(Object sender, EventArgs e) in Q:\Enterprise\Enterprise\Startup\GuiTest.cs:line 2882",
						@"at Enterprise.Startup.TestRig.AnotherStackFrame1(Object sender, EventArgs e) in Q:\Enterprise\Enterprise\Startup\GuiTest.cs:line 2882",
						@"at Enterprise.Startup.TestRig.ThrowExceptionButton_Click(Object sender, EventArgs e) in Q:\Enterprise\Enterprise\Startup\GuiTest.cs:line 2882",
						"at System.Windows.Forms.Control.OnClick(EventArgs e)",
						"at System.Windows.Forms.Button.OnClick(EventArgs e)",
						"at System.Windows.Forms.Button.WndProc(Message& m)",
						"at System.Windows.Forms.ControlNativeWindow.OnMessage(Message& m)"
				};
			}

			#endregion
		}

		#endregion

		static string ErrorReportIDOnlyText
		{
			get
			{
				using (StringWriter writer = new StringWriter())
				using (XmlTextWriter xtw = new XmlTextWriter(writer))
				{
					xtw.WriteStartElement("EDI_Exception_Report");

					xtw.WriteElementString("ErrorReportID", "R123456789012345678");

					xtw.WriteEndElement();

					return writer.ToString();
				}
			}
		}

		static string ErrorReportIDOnlyText_EnterpriseNumberFountainFailed
		{
			get
			{
				using (var writer = new StringWriter())
				using (var xtw = new XmlTextWriter(writer))
				{
					xtw.WriteStartElement("EDI_Exception_Report");

					xtw.WriteElementString("ErrorReportID", "ReportIDFailed-EDI-DAT");

					xtw.WriteEndElement();

					return writer.ToString();
				}
			}
		}

		static string ErrorReportSessionId(string sessionIdInExpectedCase, string sessionIdWithRandomCase)
		{
			using (var writer = new StringWriter())
			using (var xtw = new XmlTextWriter(writer))
			{
				xtw.WriteStartElement("EDI_Exception_Report");
				if (!sessionIdInExpectedCase.IsNullOrEmpty())
				{
					xtw.WriteElementString("SessionId", sessionIdInExpectedCase);
				}
				if (!sessionIdWithRandomCase.IsNullOrEmpty())
				{
					xtw.WriteElementString("SeSsIoNiD", sessionIdWithRandomCase);
				}

				xtw.WriteEndElement();

				return writer.ToString();
			}
		}

		static string WithTestRigOrigin
		{
			get
			{
				using (StringWriter writer = new StringWriter())
				using (XmlTextWriter xtw = new XmlTextWriter(writer))
				{
					xtw.WriteStartElement("EDI_Exception_Report");

					xtw.WriteElementString("TestRigOrigin", "WI00761180");

					xtw.WriteEndElement();

					return writer.ToString();
				}
			}
		}
	}

	class ExceptionXmlTestWithIssueNotifier : TestCaseWithXmlDoc
	{
		public void TestInnerExceptionDescriptionReturned()
		{
			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = new ZDateTime(2000, 1, 1, 0, 0, 0);
			var xml = File.ReadAllText(SampleTestFile);
			var logs = new ScalableHelpErrorLogCollection(Factory);
			var log = new ExceptionXml(xml).Process(logs, false);
			AssertEquals("System.Data.RowNotInTableException", log.HE_ExceptionType);
			AssertEquals("System.Data", log.HE_ExceptionSource);
			AssertEquals("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.", log.HE_ExceptionMessage);
		}

		[TestDate(2005, 5, 5)]
		public void TestProcess_ExeDateBeforeFixedDate_ShouldNotAttach()
		{
			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = new ZDateTime(2000, 1, 1, 0, 0, 0);
			var thresholdCollection = new IssueWorkItemCreationThresholdCollection();
			var threshold = thresholdCollection.AddNew();
			threshold.IssueOccurrenceThreshold = 1;
			threshold.ThresholdTimespan = 30;
			EDIDataRegistry.Instance.IssueWorkItemCreationThresholdClientVisible.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, thresholdCollection);

			var log = Factory.New<EdiHelpErrorLog>();

			var issueOccurrence1 = Factory.New<HelpErrorLogOccurrence>();
			var issueOccurrence1XML = new ExceptionXml(CreateOccurrenceXml(today.AddDays(-7), today.AddDays(-20), "E00000001-EDI-DAT", "machineKey1"));
			issueOccurrence1.HO_XMLData = issueOccurrence1XML.ToString();
			log.Occurrences.Add(issueOccurrence1);
			Factory.Save();
			var firstLog = issueOccurrence1XML.Process(logs, true);
			Assert("Precondition: There should be a new work item attached to the issue.", firstLog.HasWorkItems);
			Assert("Precondition: The new log should have a blank fix date.", firstLog.HE_FixedDate.IsEmpty);

			firstLog.RelatedWorkItems.RemoveAndDeleteAll();
			Factory.Save();
			Assert("Precondition: log has no work items attached", !firstLog.HasWorkItems);

			var fixDate = today.AddDays(-5);
			firstLog.HE_FixedDate = fixDate;
			Factory.Save();
			var firstLogInNewFactory = new BusinessObjectFactory().Load<EdiHelpErrorLog>(firstLog.PK);
			Assert("Precondition", firstLogInNewFactory.HE_FixedDate == fixDate);

			var issueOccurrence2 = Factory.New<HelpErrorLogOccurrence>();
			var issueOccurrence2XML = new ExceptionXml(CreateOccurrenceXml(today.AddDays(-7), today.AddDays(-26), "E00000001-EDI-DAT", "machineKey1"));
			issueOccurrence2.HO_XMLData = issueOccurrence2XML.ToString();
			log.Occurrences.Add(issueOccurrence2);
			Factory.Save();
			var secondLog = issueOccurrence2XML.Process(logs, true);
			Assert("Precondition: secondLog.FirstOccurrence.HO_ExceptionDateTime < firstLogInNewFactory.HE_FixedDate", secondLog.FirstOccurrence.HO_ExceptionDateTime < firstLogInNewFactory.HE_FixedDate);
			Assert("There should not be a new work item attached to the issue.", !secondLog.HasWorkItems);
		}

		public void TestProcess_AddAutoReOpenedIssue()
		{
			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = new ZDateTime(2000, 1, 1, 0, 0, 0);

			var xml = CreateOccurrenceXml(today.AddDays(-11), new ZDateTime(2005, 6, 8, 10, 56, 08), "E00000001-EDI-DAT");
			var log = new ExceptionXml(xml).Process(logs, false);
			AssertLogOccurrenceCount(log, 1);
			log.HE_FixedDate = today.AddDays(-11); // ensure that it is marked fixed and therefore should be reopened, this step is tested by TestProcess_AutoClose_AutoReOpen

			xml = CreateOccurrenceXml(today.AddDays(-1), new ZDateTime(2005, 6, 8, 10, 56, 08), "E00000002-EDI-DAT");
			log = new ExceptionXml(xml).Process(logs, false);

			AssertLogOccurrenceCount(log, 2);
			Assert("Log on registry date should be reopened, no longer fixed", log.HE_FixedDate.IsEmpty);
			AssertHasEvent(log, "Log should be should log signifying auto-reopened", Events.IncidentReopened);

			xml = CreateOccurrenceXml(today, new ZDateTime(2005, 6, 8, 10, 56, 08), "E00000003-EDI-DAT");
			log = new ExceptionXml(xml).Process(logs, false);

			AssertLogOccurrenceCount(log, 3);
			Assert("Still not fixed", log.HE_FixedDate.IsEmpty);
		}

		[TestDate(2005, 5, 5)]
		public void TestProcess_IssueCreationThresholds_TestThreshold()
		{
			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = new ZDateTime(2000, 1, 1, 0, 0, 0);
			var thresholdCollection = new IssueWorkItemCreationThresholdCollection();
			var threshold = thresholdCollection.AddNew();
			threshold.IssueOccurrenceThreshold = 2;
			threshold.ThresholdTimespan = 1;
			EDIDataRegistry.Instance.IssueWorkItemCreationThresholdClientVisible.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, thresholdCollection);

			var issueOccurrence1XML = new ExceptionXml(CreateOccurrenceXml(today, new ZDateTime(2005, 6, 8, 10, 56, 08), "E00000001-EDI-DAT", "machineKey1"));
			var firstLog = issueOccurrence1XML.Process(logs, false);
			firstLog.Factory.Save();
			Assert("There should not be a new work item attached to the issue.", !firstLog.HasWorkItems);

			var issueOccurrence2XML = new ExceptionXml(CreateOccurrenceXml(today, new ZDateTime(2005, 6, 8, 10, 56, 08), "E00000002-EDI-DAT", "machineKey1"));
			var secondLog = issueOccurrence2XML.Process(logs, false);
			secondLog.Factory.Save();
			Assert("The second log must create a new work item", secondLog.HasWorkItems);
		}

		[TestDate(2005, 5, 5)]
		public void TestProcess_IssueCreationThresholds_AppliesToOccurrencesOnOneIssue()
		{
			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = new ZDateTime(2000, 1, 1, 0, 0, 0);
			var thresholdCollection = new IssueWorkItemCreationThresholdCollection();
			var threshold = thresholdCollection.AddNew();
			threshold.IssueOccurrenceThreshold = 2;
			threshold.ThresholdTimespan = 1;
			EDIDataRegistry.Instance.IssueWorkItemCreationThresholdClientVisible.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, thresholdCollection);

			var issueOccurrence1XML = new ExceptionXml(CreateOccurrenceXml(today, new ZDateTime(2005, 6, 8, 10, 56, 08), "E00000001-EDI-DAT", "message", "key one"));
			var firstLog = issueOccurrence1XML.Process(logs, false);
			firstLog.Factory.Save();
			Assert("There should not be a new work item attached to the issue.", !firstLog.HasWorkItems);

			var issueOccurrence2XML = new ExceptionXml(CreateOccurrenceXml(today, new ZDateTime(2005, 6, 8, 10, 56, 08), "E00000002-EDI-DAT", "message", "key two"));
			var secondLog = issueOccurrence2XML.Process(logs, false);
			secondLog.Factory.Save();
			Assert("There should not be a new work item attached to the issue.", !secondLog.HasWorkItems);
		}

		[TestDate(2005, 5, 5)]
		public void TestProcess_IssueCreationThresholds_TestTimespan()
		{
			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = new ZDateTime(2000, 1, 1, 0, 0, 0);
			var thresholdCollection = new IssueWorkItemCreationThresholdCollection();
			var threshold = thresholdCollection.AddNew();
			threshold.IssueOccurrenceThreshold = 2;
			threshold.ThresholdTimespan = 2;
			EDIDataRegistry.Instance.IssueWorkItemCreationThresholdClientVisible.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, thresholdCollection);

			var issueOccurrence1XML = new ExceptionXml(CreateOccurrenceXml(today.AddDays(-7), today.AddDays(-70), "E00000001-EDI-DAT", "machineKey1"));
			var firstLog = issueOccurrence1XML.Process(logs, false);
			firstLog.Factory.Save();
			Assert("There should not be a new work item attached to the issue.", !firstLog.HasWorkItems);

			var issueOccurrence2XML = new ExceptionXml(CreateOccurrenceXml(today, new ZDateTime(2005, 5, 5, 10, 56, 08), "E00000001-EDI-DAT", "machineKey1"));
			var secondLog = issueOccurrence2XML.Process(logs, false);
			secondLog.Factory.Save();
			Assert("There should not be a new work item attached to the issue.", !secondLog.HasWorkItems);

			var issueOccurrence3XML = new ExceptionXml(CreateOccurrenceXml(today, new ZDateTime(2005, 5, 5, 10, 56, 08), "E00000001-EDI-DAT", "machineKey1"));
			var thirdLog = issueOccurrence3XML.Process(logs, false);
			thirdLog.Factory.Save();
			Assert("There should be a new work item attached to the issue.", thirdLog.HasWorkItems);
		}

		[TestDate(2018, 01, 30)]
		public void TestProcess_IssueCreationThresholds_TestLatestGP1Release()
		{
			var build = Factory.New<ReleaseBuild>();
			build.HL_Product = ProductTypes.Codes.Enterprise;
			build.HL_MajorVersion = 17;
			build.HL_MinorVersion = 11;
			build.HL_Release = 10;
			build.HL_Patch = 879;
			build.HL_ReleaseStatus = ReleaseRings.Codes.GP1;
			build.HL_ExeVersionDate = new ZDateTime(2018, 1, 30, 5, 5, 5);
			build.HL_Superceded = false;

			var build2 = Factory.New<ReleaseBuild>();
			build2.HL_Product = ProductTypes.Codes.Enterprise;
			build2.HL_MajorVersion = 17;
			build2.HL_MinorVersion = 11;
			build2.HL_Release = 10;
			build2.HL_Patch = 800;
			build2.HL_ReleaseStatus = ReleaseRings.Codes.GP1;
			build2.HL_ExeVersionDate = new ZDateTime(2018, 1, 28, 5, 5, 5);
			build2.HL_Superceded = false;

			var thresholdCollection = new IssueWorkItemCreationThresholdCollection();
			var threshold = thresholdCollection.AddNew();
			threshold.IssueOccurrenceThreshold = 2;
			threshold.ThresholdTimespan = 2;
			EDIDataRegistry.Instance.IssueWorkItemCreationThresholdClientVisible.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, thresholdCollection);
			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = new ZDateTime(2017, 1, 1, 0, 0, 0);

			var issueOccurrenceXML = new ExceptionXml(CreateOccurrenceXml(today.AddDays(-35), today, "E00000001-EDI-DAT", "machineKey1"));
			var versionElement = issueOccurrenceXML.CreateElement("VersionNumber");
			versionElement.InnerText = build2.HL_MajorVersion.ToString() + "." + build2.HL_MinorVersion.ToString() + "." + build2.HL_Release + "." + build2.HL_Patch;
			issueOccurrenceXML.SelectSingleNode("EDI_Exception_Report").InsertAfter(versionElement, issueOccurrenceXML.SelectSingleNode("EDI_Exception_Report/ExeCreationTime"));
			var firstLog = issueOccurrenceXML.Process(logs, false);

			Factory.Save();

			var issueOccurrenceXML2 = new ExceptionXml(CreateOccurrenceXml(today.AddDays(-35), today, "E00000001-EDI-DAT", "machineKey1"));
			var versionElement2 = issueOccurrenceXML2.CreateElement("VersionNumber");
			versionElement2.InnerText = build2.HL_MajorVersion.ToString() + "." + build2.HL_MinorVersion.ToString() + "." + build2.HL_Release + "." + build2.HL_Patch;
			issueOccurrenceXML2.SelectSingleNode("EDI_Exception_Report").InsertAfter(versionElement2, issueOccurrenceXML2.SelectSingleNode("EDI_Exception_Report/ExeCreationTime"));
			var secondLog = issueOccurrenceXML2.Process(logs, false);
			Assert("There should be a new work item attached to the issue.", secondLog.HasWorkItems);
		}

		public void TestProcess_IssueCreationThresholds_TestNullExeDate()
		{
			var thresholdCollection = new IssueWorkItemCreationThresholdCollection();
			var threshold = thresholdCollection.AddNew();
			threshold.IssueOccurrenceThreshold = 2;
			threshold.ThresholdTimespan = 2;
			EDIDataRegistry.Instance.IssueWorkItemCreationThresholdClientVisible.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, thresholdCollection);

			var issueOccurrence1XML = new ExceptionXml(CreateOccurrenceXml(today, today, "E00000001-EDI-DAT", "machineKey1"));
			issueOccurrence1XML.SelectSingleNode("EDI_Exception_Report/ExeCreationTime").InnerText = string.Empty;
			var firstLog = issueOccurrence1XML.Process(logs, false);
			firstLog.Factory.Save();
			Assert("There should not be a new work item attached to the issue.", !firstLog.HasWorkItems);

			var issueOccurrence2XML = new ExceptionXml(CreateOccurrenceXml(today, today, "E00000001-EDI-DAT", "machineKey1"));
			issueOccurrence2XML.SelectSingleNode("EDI_Exception_Report/ExeCreationTime").InnerText = string.Empty;
			var secondLog = issueOccurrence2XML.Process(logs, false);
			secondLog.Factory.Save();
			Assert("There should be a new work item attached to the issue.", secondLog.HasWorkItems);
		}

		[TestDate(2005, 5, 5)]
		public void TestProcess_IssueCreationThresholds_MessageGrouping()
		{
			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = new ZDateTime(2000, 1, 1, 0, 0, 0);
			var thresholdCollection = new IssueWorkItemCreationThresholdCollection();
			var threshold = thresholdCollection.AddNew();
			threshold.IssueOccurrenceThreshold = 2;
			threshold.ThresholdTimespan = 1;
			EDIDataRegistry.Instance.IssueWorkItemCreationThresholdClientVisible.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, thresholdCollection);

			var issueOccurrence1XML = new ExceptionXml(CreateOccurrenceXml(today.AddDays(-7), new ZDateTime(2005, 5, 5, 10, 56, 08), "E00000001-EDI-DAT", "The ducks fly south in the summer", "machineKey1"));
			var firstLog = issueOccurrence1XML.Process(logs, false);
			firstLog.Factory.Save();
			Assert("There should not be a new work item attached to the issue.", !firstLog.HasWorkItems);

			var issueOccurrence2XML = new ExceptionXml(CreateOccurrenceXml(today, new ZDateTime(2005, 5, 5, 10, 56, 08), "E00000001-EDI-DAT", "The bevers build the strongest dams in the north", "machineKey1"));
			var secondLog = issueOccurrence2XML.Process(logs, false);
			secondLog.Factory.Save();
			Assert("There should not be a new work item attached to the issue.", secondLog.HasWorkItems);

			var issueOccurrence3XML = new ExceptionXml(CreateOccurrenceXml(today, new ZDateTime(2005, 5, 5, 10, 56, 08), "E00000001-EDI-DAT", "The bevers build the strongest dams in the north", "machineKey1"));
			var thirdLog = issueOccurrence3XML.Process(logs, false);
			thirdLog.Factory.Save();
			Assert("There should be a new work item attached to the issue.", thirdLog.HasWorkItems);
		}

		[TestDate(2005, 5, 5)]
		public void TestProcess_IssueCreationThresholds_NonClientVisibleThreshold()
		{
			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = new ZDateTime(2000, 1, 1, 0, 0, 0);
			var thresholdCollection = new IssueWorkItemCreationThresholdCollection();
			var threshold = thresholdCollection.AddNew();
			threshold.IssueOccurrenceThreshold = 2;
			threshold.ThresholdTimespan = 1;
			EDIDataRegistry.Instance.IssueWorkItemCreationThresholdClientVisible.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, thresholdCollection);

			thresholdCollection = new IssueWorkItemCreationThresholdCollection();
			threshold = thresholdCollection.AddNew();
			threshold.IssueOccurrenceThreshold = 3;
			threshold.ThresholdTimespan = 1;
			EDIDataRegistry.Instance.IssueWorkItemCreationThresholdNonClientVisible.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, thresholdCollection);

			var issueOccurrenceXML = new ExceptionXml(File.ReadAllText(SilentExceptionTestFile));
			var firstLog = issueOccurrenceXML.Process(logs, false);
			firstLog.Factory.Save();
			Assert("There should not be a new work item attached to the issue.", !firstLog.HasWorkItems);

			issueOccurrenceXML = new ExceptionXml(File.ReadAllText(SilentExceptionTestFile));
			var secondLog = issueOccurrenceXML.Process(logs, false);
			secondLog.Factory.Save();
			Assert("There should not be a new work item attached to the issue.", !firstLog.HasWorkItems);

			issueOccurrenceXML = new ExceptionXml(File.ReadAllText(SilentExceptionTestFile));
			var thirdLog = issueOccurrenceXML.Process(logs, false);
			thirdLog.Factory.Save();
			Assert("The third log must create a new work item", secondLog.HasWorkItems);
		}

		public void TestProcess_ExistingIssueShouldNotCreateNewWorkItem()
		{
			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = new ZDateTime(2000, 1, 1, 0, 0, 0);

			var issue = new ExceptionXml(CreateOccurrenceXml(today, today, "E00000001-EDI-DAT", "machineKey1"));

			var firstLog = issue.Process(logs, true);
			Assert("There should be a new work item attached to the issue.", firstLog.HasWorkItems);
			AssertEquals("There should be a new work item attached to the issue.", 1, firstLog.RelatedWorkItems.Count);

			var secondLog = issue.Process(logs, true);
			AssertEquals("There should not be a new work item attached to the issue.", 1, secondLog.RelatedWorkItems.Count);
		}

		public void TestProcess_AddNewAutoAssignedIssueToExistingWorkItem()
		{
			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = new ZDateTime(2000, 1, 1, 0, 0, 0);

			string xml = CreateOccurrenceXml(today, today, "E00000001-EDI-DAT", "Error message", Guid.NewGuid().ToSqlGuid());
			var firstLog = new ExceptionXml(xml).Process(logs, true);
			Assert("There should be a new work item attached to the issue.", firstLog.HasWorkItems);
			var wkItem1 = (NewWorkItem)firstLog.RelatedWorkItems.First();
			AssertEquals(firstLog.ExceptionMessageFirstLine, wkItem1.WKI_Summary);

			xml = CreateOccurrenceXml(today, today, "E00000002-EDI-DAT", "Error message", Guid.NewGuid().ToSqlGuid());
			var secondLog = new ExceptionXml(xml).Process(logs, true);
			Assert("There should be a work item attached to the issue.", secondLog.HasWorkItems);
			AssertEquals(wkItem1, secondLog.RelatedWorkItems.First());
		}

		public void TestProcess_DoesNotAddNewAutoAssignedIssueToExistingWorkItemIfIsNullReference()
		{
			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = new ZDateTime(2000, 1, 1, 0, 0, 0);

			var xml = CreateOccurrenceXml(today, today, "E00000001-EDI-DAT", "Error message", Guid.NewGuid().ToSqlGuid(), typeof(NullReferenceException).FullName);
			var firstLog = new ExceptionXml(xml).Process(logs, true);
			Assert("There should be a new work item attached to the issue.", firstLog.HasWorkItems);
			var wkItem1 = (NewWorkItem)firstLog.RelatedWorkItems.First();
			AssertEquals(firstLog.ExceptionMessageFirstLine, wkItem1.WKI_Summary);

			xml = CreateOccurrenceXml(today, today, "E00000002-EDI-DAT", "Error message", Guid.NewGuid().ToSqlGuid(), typeof(NullReferenceException).FullName);
			var secondLog = new ExceptionXml(xml).Process(logs, true);
			Assert("There should be a work item attached to the issue.", secondLog.HasWorkItems);
			AssertNotEquals(wkItem1, secondLog.RelatedWorkItems.First());
		}

		public void TestAutoAssigningIssue_ShouldNotAssignToCancelledWorkItem()
		{
			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = new ZDateTime(2000, 1, 1, 0, 0, 0);

			var issue = new ExceptionXml(CreateOccurrenceXml(today, today, "E00000001-EDI-DAT", "machineKey1"));
			var firstLog = issue.Process(logs, true);
			var firstWorkitem = firstLog.RelatedWorkItems.SingleOrDefault() as WorkItem;
			AssertNotNull("There should be a new work item attached to the first issue.", firstWorkitem);

			Factory.Save();

			firstWorkitem.Cancel();
			firstLog.HE_FixedDate = firstLog.HE_FixedDate.AddDays(-2);

			var secondLog = issue.Process(logs, true);
			AssertEquals("The second log should have 2 related work items.", 2, secondLog.RelatedWorkItems.Count);
			var secondWorkItem = secondLog.RelatedWorkItems.Single(x => x.PK != firstWorkitem.PK) as WorkItem;
			AssertNotNull("The second log should have created a new work item because the first work item was cancelled.", secondWorkItem);
			Assert("There should be a work item attached to the issue.", secondLog.HasWorkItems);

			var thirdLog = issue.Process(logs, true);
			AssertEquals("The third log should have 2 related work items.", 2, thirdLog.RelatedWorkItems.Count);
		}

		public void TestIssueAssignmentCalculatorUsed()
		{
			var mockAssignment = new Mock<IIssueAssignmentCalculator>();
			mockAssignment.Setup(o => o.GetAssignment(It.IsAny<EdiHelpErrorLog>(), It.IsAny<DataFormatter>())).Returns(new IssueAssignment("PRD", "ARE", "MOD"));
			var issue = new ExceptionXml(CreateOccurrenceXml(today, today, "E00000001-EDI-DAT")) { IssueAssignmentCalculator = mockAssignment.Object };
			var log = issue.Process(logs, true);
			var workItem = log.RelatedWorkItems.SingleOrDefault() as WorkItem;
			AssertEquals("PRD", workItem.WKI_WorkItemType);
			AssertEquals("ARE", workItem.WKI_WorkItemArea);
			AssertEquals("MOD", workItem.WKI_ActivityType);
		}

		public void TestRealIssueAssignmentCalculatorHasAccessToNewOccurrence()
		{
			AssignmentTestHelper.AddStackLineCount(Factory, "Assembly.dll", "Enterprise.ZArchitecture.Business.AutoStmALog.set_SL_Reference(ZString value)", 1);
			AssignmentTestHelper.AddPublishedAssembly("Assembly.dll", "$/Code");
			AssignmentTestHelper.AddSourceTreeResponsibility("$/Code", "PRD", "ARE", "MOD");
			Factory.Save();
			var issue = new ExceptionXml(CreateOccurrenceXml(today, today, "E00000001-EDI-DAT", "The exception message"));
			var log = issue.Process(logs, false);
			var workItem = log.RelatedWorkItems.SingleOrDefault() as WorkItem;
			AssertEquals("PRD", workItem.WKI_WorkItemType);
			AssertEquals("ARE", workItem.WKI_WorkItemArea);
			AssertEquals("MOD", workItem.WKI_ActivityType);
		}

		public void TestIssueWithSameErrorAssignedToSameWIForMatchingModule()
		{
			var mockAssignment = new Mock<IIssueAssignmentCalculator>();
			mockAssignment.Setup(o => o.GetAssignment(It.IsAny<EdiHelpErrorLog>(), It.IsAny<DataFormatter>())).Returns(new IssueAssignment("PRD", "ARE", "MOD"));
			var issue = new ExceptionXml(CreateOccurrenceXml(today, today, "E00000001-EDI-DAT", "Error message", "key1")) { IssueAssignmentCalculator = mockAssignment.Object };
			var log = issue.Process(logs, true);
			var firstWorkItem = log.RelatedWorkItems.SingleOrDefault() as WorkItem;

			issue = new ExceptionXml(CreateOccurrenceXml(today, today, "E00000002-EDI-DAT", "Error message", "key2")) { IssueAssignmentCalculator = mockAssignment.Object };
			log = issue.Process(logs, true);
			var secondWorkItem = log.RelatedWorkItems.SingleOrDefault() as WorkItem;

			AssertEquals(firstWorkItem, secondWorkItem);
		}

		public void TestIssueWithSameErrorNotAssignedToSameWIForNonMatchingModule()
		{
			var mockAssignment = new Mock<IIssueAssignmentCalculator>();
			mockAssignment.Setup(o => o.GetAssignment(It.IsAny<EdiHelpErrorLog>(), It.IsAny<DataFormatter>())).Returns(new IssueAssignment("PRD", "ARE", "MOD"));
			var issue = new ExceptionXml(CreateOccurrenceXml(today, today, "E00000001-EDI-DAT", "Error message", "key1")) { IssueAssignmentCalculator = mockAssignment.Object };
			var log = issue.Process(logs, true);
			var firstWorkItem = log.RelatedWorkItems.SingleOrDefault() as WorkItem;

			mockAssignment = new Mock<IIssueAssignmentCalculator>();
			mockAssignment.Setup(o => o.GetAssignment(It.IsAny<EdiHelpErrorLog>(), It.IsAny<DataFormatter>())).Returns(new IssueAssignment("PRD", "TWO", "TWO"));
			issue = new ExceptionXml(CreateOccurrenceXml(today, today, "E00000002-EDI-DAT", "Error message", "key2")) { IssueAssignmentCalculator = mockAssignment.Object };
			log = issue.Process(logs, true);
			var secondWorkItem = log.RelatedWorkItems.SingleOrDefault() as WorkItem;

			AssertNotEquals(firstWorkItem, secondWorkItem);

			AssertEquals("PRD", firstWorkItem.WKI_WorkItemType);
			AssertEquals("ARE", firstWorkItem.WKI_WorkItemArea);
			AssertEquals("MOD", firstWorkItem.WKI_ActivityType);

			AssertEquals("PRD", secondWorkItem.WKI_WorkItemType);
			AssertEquals("TWO", secondWorkItem.WKI_WorkItemArea);
			AssertEquals("TWO", secondWorkItem.WKI_ActivityType);
		}

		public void TestIssueWIWhenNoAssignmentCalculated()
		{
			using (EDIDataRegistry.Instance.FallbackWorkItemCriteriaRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "AAA/BBB/CCC"))
			{
				var issue = new ExceptionXml(CreateOccurrenceXml(today, today, "E00000001-EDI-DAT"));
				var log = issue.Process(logs, true);
				var workItem = log.RelatedWorkItems.SingleOrDefault() as WorkItem;
				var fallbackLog = workItem.Logs.GetAllLogs().Cast<StmALog>().First(e => e.SL_SE_NKEvent == Events.EditedARecord.Code && e.SL_Reference.ToString() == "Created from issue, assigned with fallback criteria: AAA/BBB/CCC");
				AssertEquals("AAA", workItem.WKI_WorkItemType);
				AssertEquals("BBB", workItem.WKI_WorkItemArea);
				AssertEquals("CCC", workItem.WKI_ActivityType);
				AssertEquals(NewWorkItemLookups.WorkItemTypeConstants.IssueFix, workItem.WKI_ActivitySubtype);
				AssertEquals(ReleaseRings.Codes.GPR, workItem.WKI_Priority);
				AssertNotNull(fallbackLog);
			}
		}

		[TestDate(2017, 11, 30, 14, 26, 01)]
		public void TestIssueAttachedToWorkItemClosedAfterLastExeDate()
		{
			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = new ZDateTime(2000, 1, 1, 0, 0, 0);

			var xml = CreateOccurrenceXml(new ZDateTime(2017, 11, 20), ZDateTime.Now, "E00000001-EDI-DAT", "Error message", Guid.NewGuid().ToSqlGuid());
			var firstLog = new ExceptionXml(xml).Process(logs, true);
			var workItem = (NewWorkItem)firstLog.RelatedWorkItems.Single();
			Factory.Save();

			workItem.WorkflowItems.Tasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);
			TestDateAttribute.AddSeconds(1); // Forces edits to task status to propagate to workflows and raise the JCL event.
			Factory.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItem.WKI_Status);
			AssertEquals(TestDateAttribute.Date, workItem.JobCloseDateUtc);

			xml = CreateOccurrenceXml(new ZDateTime(2017, 11, 20), ZDateTime.Now, "E00000002-EDI-DAT", "Error message", Guid.NewGuid().ToSqlGuid());
			var secondLog = new ExceptionXml(xml).Process(logs, true);
			Assert("There should be a work item attached to the issue.", secondLog.HasWorkItems);
			AssertEquals(workItem, secondLog.RelatedWorkItems.Single());
		}

		[TestDate(2017, 11, 30, 14, 26, 01)]
		public void TestIssueNotAttachedToWorkItemClosedBeforeLastExeDate()
		{
			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = new ZDateTime(2000, 1, 1, 0, 0, 0);

			var xml = CreateOccurrenceXml(new ZDateTime(2017, 11, 20), ZDateTime.Now, "E00000001-EDI-DAT", "Error message", Guid.NewGuid().ToSqlGuid());
			var firstLog = new ExceptionXml(xml).Process(logs, true);
			var workItem = (NewWorkItem)firstLog.RelatedWorkItems.Single();
			Factory.Save();

			workItem.WorkflowItems.Tasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);
			TestDateAttribute.AddSeconds(1); // Forces edits to task status to propagate to workflows and raise the JCL event.
			Factory.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItem.WKI_Status);
			AssertEquals(TestDateAttribute.Date, workItem.JobCloseDateUtc);

			xml = CreateOccurrenceXml(TestDateAttribute.Date.AddDays(1), ZDateTime.Now, "E00000002-EDI-DAT", "Error message", Guid.NewGuid().ToSqlGuid());
			var secondLog = new ExceptionXml(xml).Process(logs, true);
			Assert("There should be a work item attached to the issue.", secondLog.HasWorkItems);
			AssertNotEquals(workItem, secondLog.RelatedWorkItems.Single());
		}

		public void TestWorkItemCreatedForUpgradeFailueIssueRegardlessOfThreshold()
		{
			var thresholdCollection = new IssueWorkItemCreationThresholdCollection();
			var threshold = thresholdCollection.AddNew();
			threshold.IssueOccurrenceThreshold = 5;
			threshold.ThresholdTimespan = 1;
			EDIDataRegistry.Instance.IssueWorkItemCreationThresholdClientVisible.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, thresholdCollection);

			var issue = new ExceptionXml(CreateOccurrenceXml(today, today, "E00000001-EDI-DAT", exceptionDescription: "Failed to upgrade database"));
			var log = issue.Process(logs, true);
			var workItem = log.RelatedWorkItems.SingleOrDefault() as WorkItem;
			AssertNotNull(workItem);
			AssertContains("Upgrade Failure", workItem.WKI_Summary);
		}

		public void TestWorkItemNotCreatedForUpgradeFailueIssueOnOldExe()
		{
			var thresholdCollection = new IssueWorkItemCreationThresholdCollection();
			var threshold = thresholdCollection.AddNew();
			threshold.IssueOccurrenceThreshold = 5;
			threshold.ThresholdTimespan = 1;
			EDIDataRegistry.Instance.IssueWorkItemCreationThresholdClientVisible.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, thresholdCollection);

			var issue = new ExceptionXml(CreateOccurrenceXml(today, today.AddYears(-1), "E00000001-EDI-DAT", exceptionDescription: "Failed to upgrade database"));
			var log = issue.Process(logs, true);
			AssertEquals(0, log.RelatedWorkItems.Count);
		}

		public void TestThresholdConsidersUnsavedOccurrencesInFactory()
		{
			var thresholdCollection = new IssueWorkItemCreationThresholdCollection();
			var threshold = thresholdCollection.AddNew();
			threshold.IssueOccurrenceThreshold = 2;
			threshold.ThresholdTimespan = 1;
			EDIDataRegistry.Instance.IssueWorkItemCreationThresholdClientVisible.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, thresholdCollection);

			var issueOccurrence1XML = new ExceptionXml(CreateOccurrenceXml(today, today, "E00000001-EDI-DAT", "machineKey1"));
			var firstLog = issueOccurrence1XML.Process(logs, false);
			Assert("There should not be a new work item attached to the issue.", !firstLog.HasWorkItems);

			var issueOccurrence2XML = new ExceptionXml(CreateOccurrenceXml(today, today, "E00000002-EDI-DAT", "machineKey1"));
			var secondLog = issueOccurrence2XML.Process(logs, false);
			Assert("The second log must create a new work item", secondLog.HasWorkItems);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = new ZDateTime(2000, 1, 1, 0, 0, 0);
			var thresholdCollection = new IssueWorkItemCreationThresholdCollection();
			var threshold = thresholdCollection.AddNew();
			threshold.IssueOccurrenceThreshold = 1;
			threshold.ThresholdTimespan = 1;
			EDIDataRegistry.Instance.IssueWorkItemCreationThresholdClientVisible.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, thresholdCollection);

			today = ZDateTime.Today;
			logs = new ScalableHelpErrorLogCollection(Factory);

			defaultRegistryValue = EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager;

			earlierDate = ZDateTime.Today.AddDays(-10);
			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = earlierDate;

			EDIDataRegistry.Instance.StackLineCountNumberOfImportedLogs = 100;

			// Create workflow template for issue fix WIs so the check for existing attached WIs can find non-completed ones and therefore not keep making new WIs.
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode, subType4: NewWorkItemLookups.WorkItemTypeConstants.IssueFix);
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
			BMSTestHelper.CreateTask(template, templateWorkflow);

			Factory.Save();
		}

		IBMTestHelper BMSTestHelper { get; } = ObjectFactory.Get<IBMTestHelper>();

		protected override void TearDown()
		{
			base.TearDown();
			EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager = defaultRegistryValue;
		}

		void AssertLogOccurrenceCount(EdiHelpErrorLog log, int expected)
		{
			log.Occurrences.Load();
			AssertEquals("Log Occurrence Count", expected, log.Occurrences.Count);
		}

		void AssertHasEvent(EdiHelpErrorLog log, string message, Event @event)
		{
			AssertNotNull(message, log.Logs.MostRecentLogByEventTime(@event));
		}

		ZDateTime defaultRegistryValue;
		ZDateTime earlierDate;
		IHelpErrorLogCollection logs;
		ZDateTime today;

		#endregion
	}
}
