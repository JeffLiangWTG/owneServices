using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.Licensing.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.VersionReport;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests
{
	sealed class LicenceUsageRequestMessageActionTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			VersionReportBuilderFactory.ClearSent();

			CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, "XX", new ZDateTime(2009, 1, 1, 9, 32, 0), GlbCompany.CurrentCompany.PK, ModuleLicenceType.NON, true);

			CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, "XX", new ZDateTime(2010, 1, 1, 9, 32, 0), GlbCompany.CurrentCompany.PK, ModuleLicenceType.NON, true);
			CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, "XX", new ZDateTime(2010, 2, 1, 9, 32, 0), GlbCompany.CurrentCompany.PK, ModuleLicenceType.NON, true);
			CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, "XX", new ZDateTime(2010, 3, 1, 9, 32, 0), GlbCompany.CurrentCompany.PK, ModuleLicenceType.NON, true);

			CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, "XX", new ZDateTime(2010, 4, 1, 9, 32, 0), GlbCompany.CurrentCompany.PK, ModuleLicenceType.NON, true);

			string requestXml = @"<LicenceUsageRequest>
<DateFrom>2010/01/01</DateFrom>
<DateTo>2010/03/31</DateTo>
<RequestedBy>RIS</RequestedBy>
</LicenceUsageRequest>";

			var interchange = SystemMessage.CreateInterchange(Factory, requestXml);
			var message = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			Factory.Save();
			AssertEquals(0, VersionReportBuilderFactory.SentCount);

			IMessageAction action = new LicenceUsageRequestMessageAction(new BusinessObjectFactoryProvider(Factory));
			var logger = new NotificationBuffer();
			List<ITransactionParticipant> participants;
			action.ExecuteAction(message, logger, out participants);
			AssertEquals("3 months of usage becomes 3 reports", 3, VersionReportBuilderFactory.SentCount);
			LicenceConsumptionLogSchema log = LicenceUsageReportBuilderTest.GetLogFromEncryptedCompressed(VersionReportBuilderFactory.ReportsSent[0].LicenceUsage);
			AssertEquals(new ZDate(2010, 1, 1), log.DateFrom);
			AssertEquals(new ZDate(2010, 1, 31), log.DateTo);
			AssertEquals("RIS", log.RequestedBy);

			log = LicenceUsageReportBuilderTest.GetLogFromEncryptedCompressed(VersionReportBuilderFactory.ReportsSent[1].LicenceUsage);
			AssertEquals(new ZDate(2010, 2, 1), log.DateFrom);
			AssertEquals(new ZDate(2010, 2, 28), log.DateTo);
			AssertEquals("RIS", log.RequestedBy);

			log = LicenceUsageReportBuilderTest.GetLogFromEncryptedCompressed(VersionReportBuilderFactory.ReportsSent[2].LicenceUsage);
			AssertEquals(new ZDate(2010, 3, 1), log.DateFrom);
			AssertEquals(new ZDate(2010, 3, 31), log.DateTo);
			AssertEquals("RIS", log.RequestedBy);
		}

		public void TestProcessInvalidMessage()
		{
			VersionReportBuilderFactory.ClearSent();
			var requestXml = @"<LicenceUsageRequest>
<DateFrom>2010/01/01</DateFrom>
<DateTo>2010/31/03</DateTo>
</LicenceUsageRequest>";

			var interchange = SystemMessage.CreateInterchange(Factory, requestXml);
			var message = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			Factory.Save();
			AssertEquals(0, VersionReportBuilderFactory.SentCount);

			IMessageAction action = new LicenceUsageRequestMessageAction(new BusinessObjectFactoryProvider(Factory));
			var logger = new NotificationBuffer();
			var result = action.ExecuteAction(message, logger, out var participants);
			AssertEquals("ExecuteAction result", true, result);
			AssertEquals("Log", "Invalid Usage Request", logger.AsString.Trim());
		}

		LicenceUsageLog CreateLicenceUsageRecord(ILicenceCheckpoint checkpoint, string userInitials, ZDateTime usageTimeUtc, ZGuid companyPK, ModuleLicenceType licenceType, bool saveFactory)
		{
			LicenceUsageLog log = Factory.New<LicenceUsageLog>();
			log.S7_OpenDateTimeUtc = usageTimeUtc;
			log.S7_FormCaption = checkpoint.Name;
			log.S7_GS_NKUser = userInitials;
			log.S7_ParentID = companyPK;
			log.S7_ControllerID = LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString();
			log.S7_MouseClicks = (int)licenceType;
			if (saveFactory)
			{
				Factory.Save();
			}
			return log;
		}
	}
}
