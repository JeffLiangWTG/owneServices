using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.MasterFiles.Business.VersionInfo;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests
{
	[TestedType(typeof(UpgradeDownloaderTask))]
	sealed class UpgradeDownloaderTaskTest : ServiceTaskTestCase<UpgradeDownloaderTask>
	{
		public void TestRunTaskWithNoUpgrades()
		{
			UpgradeDownloaderTaskForTesting task = new UpgradeDownloaderTaskForTesting(Array.Empty<bool>(), new bool[] { false });
			TestServiceLogger log = InitialiseAndRunTaskSchedule(task);

			AssertEquals(2, log.Count);
			AssertEquals(true, log[0].Equals("Information|Checking a package to download..."));
			AssertEquals(true, log[1].Equals("Information|No package to download."));
		}

		public void TestRunTaskWithSuccessfulUpgrade()
		{
			var item = GetVersionInfoItem(TimeSpan.FromMinutes(-100), 200, out var fileName);
			Factory.Save();

			var task = new UpgradeDownloaderTaskForTesting(new bool[] { false, true }, new bool[] { false, true, true, true });
			var log = InitialiseAndRunTaskSchedule(task);

			AssertEquals(5, log.Count);
			AssertEquals(true, log[0].Equals("Information|Checking a package to download..."));
			AssertEquals(true, log[1].Equals($"Information|Downloading package {fileName}..."));
			AssertEquals(true, log[2].Equals($"Information|Downloading package {fileName}, downloaded 0 bytes out of -1. Status: Unknown"));
			AssertEquals(true, log[3].Equals("Information|Package downloaded, importing package to the database..."));
			AssertEquals(true, log[4].Equals($"Information|Upgrade {fileName} was imported and is ready to be applied"));

			item.Reload();
			AssertEquals(EDIMessage.Status.ProcessedOK, item.EM_Status);
		}

		public void TestDoesReportAnyError()
		{
			// Arrange
			_ = GetVersionInfoItem(TimeSpan.FromMinutes(-100), 200, out _);
			Factory.Save();

			var task = new UpgradeDownloaderTaskForTesting(
				downloadCompletedResponses: new [] { false, true },
				downloadInProcessResponses: new [] { false, true, true, true });

			// Act
			_ = InitialiseAndRunTaskSchedule(task);

			// Assert
			AssertEquals(nameof(ErrorReporter.LastMessageReported), string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestServiceTaskCanRunInAnyBranch()
		{
			Assert(GetHostedServiceAttributes().All(x => x.CanRunInAnyBranch));
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						SystemMessageList.Descriptions.UpgradeDownload,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.SYS,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + SystemMessageList.Codes.UpgradeDownload),
				};
			}
		}

		EDIMessage GetVersionInfoItem(TimeSpan timeSpan, int patch, out string fileName)
		{
			ZDateTime newTime = ZDateTime.Now.Add(timeSpan);
			var versionInfo = new PackageVersionInfo(newTime, 1, 2, 3, patch);

			string xml =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
				"<UpgradeDownload>" + System.Environment.NewLine +
				"  <ExeVersionDate>" + newTime.ToLongTimeString().ToUpper() + "</ExeVersionDate>" + System.Environment.NewLine +
				"  <MajorVersion>1</MajorVersion>" + System.Environment.NewLine +
				"  <MinorVersion>2</MinorVersion>" + System.Environment.NewLine +
				"  <Release>3</Release>" + System.Environment.NewLine +
				"  <Patch>" + patch + "</Patch>" + System.Environment.NewLine +
				"  <Comment>Comment</Comment>" + System.Environment.NewLine +
				"  <ForceDownload>Y</ForceDownload>" + System.Environment.NewLine +
				"  <PackageURL>http://www.cargowise.com/ftpmirror/" + versionInfo.PackageFileName + "</PackageURL>" + System.Environment.NewLine +
				"</UpgradeDownload>";

			var interchange = SystemMessage.CreateInterchange(Factory, xml);
			var result = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			result.EM_SystemCreateTimeUtc = newTime;
			result.EM_MessageSubType = SystemMessageList.Codes.UpgradeDownload;
			fileName = versionInfo.PackageFileName;
			return result;
		}
	}
}
