using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.IO;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.LogsReport;
using Enterprise.MasterFiles.Business.VersionReport;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests
{
	sealed class LogsRequestMessageActionTest : TestCaseWithFactory
	{
		[TestDate(2010, 2, 10)]
		public void TestSend()
		{
			try
			{
				string xml =
@"<LogsRequest>
<DateFromUtc>2010/02/03</DateFromUtc>
<DateToUtc>2010/02/10</DateToUtc>
<ServiceTaskCode>ABC</ServiceTaskCode>
<IncidentNumber>CS01234567</IncidentNumber>
<HostServerName>LON-SSQL-20B\MSSQLSERVER2</HostServerName>
<HostDBName>ODYSSEYSEIHAM</HostDBName>
<HostConnectionServerName>1234</HostConnectionServerName>
<MaxZipSize>100</MaxZipSize>
</LogsRequest>";

				var interchange = SystemMessage.CreateInterchange(Factory, xml);
				var message = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
				Factory.Save();
				AssertEquals(0, VersionReportBuilderFactory.SentCount);

				GlbCompany.CurrentCompany.GC_Code = "COM";
				var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
				registrationKey.EnterpriseCodeForTest = "ENT";
				registrationKey.ServerCodeForTest = "SRV";

				using (var tempDirectory1 = new TempDirectory())
				{
					string tempDirectory = tempDirectory1.DirectoryName + "\\";
					LogsReportBuilder.LogFilesDirectoriesForTest.Value = new List<string>();
					LogsReportBuilder.LogFilesDirectoriesForTest.Value.Add(tempDirectory);

					File.WriteAllText(Path.Combine(tempDirectory, "ABC_20100203.txt"), "log1");
					File.WriteAllText(Path.Combine(tempDirectory, "ABC_20100204.txt"), "log2");
					File.WriteAllText(Path.Combine(tempDirectory, "ABC_20100206.txt"), "log3");
					File.WriteAllText(Path.Combine(tempDirectory, "ABC_20100210.txt"), "log4");
					File.WriteAllText(Path.Combine(tempDirectory, "ABC_20100202.txt"), "Too early!");
					File.WriteAllText(Path.Combine(tempDirectory, "ABC_20100211.txt"), "Too late!");

					var outgoing = new DebugOnlyOutgoingSystemMessage();
					ObjectFactory.Substitute<IOutgoingSystemMessage>(outgoing);

					IMessageAction action = new LogsRequestMessageAction(new BusinessObjectFactoryProvider(Factory));
					var logger = new NotificationBuffer();
					List<ITransactionParticipant> participants;
					action.ExecuteAction(message, logger, out participants);

					AssertEquals("MessageName", SystemMessageList.Descriptions.LogsReport, DebugOnlyOutgoingSystemMessage.MessageName);
					string bodyText = Encoding.UTF8.GetString(DebugOnlyOutgoingSystemMessage.MessageStream);
					LogsReport report = new LogsReport(bodyText);

					AssertEquals("ABC", report.ServiceTaskCode);
					AssertEquals("CS01234567", report.IncidentNumber);

					ZipExtractor extractor = new ZipExtractor();
					string[] zippedFileNames = extractor.GetFileNames(new MemoryStream(report.LogFilesZip));

					AssertCollectionContains(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100203.txt")), zippedFileNames);
					AssertCollectionContains(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100204.txt")), zippedFileNames);
					AssertCollectionContains(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100206.txt")), zippedFileNames);
					AssertCollectionContains(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100210.txt")), zippedFileNames);
				}
			}
			finally
			{
				DebugOnlyOutgoingSystemMessage.Initialize();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "HYEMELJKW");
		}

		public void TestProcessInvalidMessage()
		{
			VersionReportBuilderFactory.ClearSent();
			var requestXml = @"<LogsRequest>
<DateFrom>2010/01/01</DateFrom>
<DateTo>2010/31/03</DateTo>
</LogsRequest>";

			var interchange = SystemMessage.CreateInterchange(Factory, requestXml);
			var message = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			Factory.Save();
			AssertEquals(0, VersionReportBuilderFactory.SentCount);

			IMessageAction action = new LogsRequestMessageAction(new BusinessObjectFactoryProvider(Factory));
			var logger = new NotificationBuffer();
			var result = action.ExecuteAction(message, logger, out var participants);
			AssertEquals("ExecuteAction result", true, result);
			Assert(logger.AsString.Trim(), logger.AsString.Trim().StartsWith("Invalid Usage Request"));
		}
	}
}
