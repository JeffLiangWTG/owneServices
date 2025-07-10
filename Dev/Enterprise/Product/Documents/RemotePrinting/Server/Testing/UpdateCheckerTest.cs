using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.RemotePrinting.Server.RPSCore;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing
{
	class UpdateCheckerTest : TestCase
	{
		public void TestRequirementsNoClientInfoProvided()
		{
			AssertCheckUpdateRequirements("10.0", "4.7",
				OldVersion, null, null, null,
				IntermediateVersion, IntermediateInstall, null, null);
		}

		public void TestRequirementsNoClientInfoProvided_IntermediateVersion()
		{
			AssertCheckUpdateRequirements("10.0", "4.7",
				IntermediateVersion, null, null, null,
				CurrentVersion, CurrentInstall, null, $"Remote Print Client {IntermediateVersion} on machine  did not provide system details. Request details: [HttpContext is null]");
		}

		public void TestRequirementsMatching()
		{
			AssertCheckUpdateRequirements("10.0", "4.7",
				SlightlyOldVersion, "10.0", "4.8", "M1",
				CurrentVersion, CurrentInstall, null, null);
		}

		public void TestRequirementsMatchingDescriptiveVersion()
		{
			AssertCheckUpdateRequirements("10.0", "4.7",
				SlightlyOldVersion, "Microsoft Windows NT 10.0", "4.8 or better", "M1",
				CurrentVersion, CurrentInstall, null, null);
		}

		public void TestRequirementsNotMatchingOS()
		{
			AssertCheckUpdateRequirements("10.0", "4.7",
				SlightlyOldVersion, "9.0", "4.8", "M1",
				UpdateChecker.UpdateErrorVersion, null, "New Remote Printing Client installation requires Windows of version 10.0 or newer to be installed on client machine. Current version : 9.0.", null);
		}

		public void TestRequirementsNotMatchingDotNet()
		{
			AssertCheckUpdateRequirements("10.0", "4.7",
				SlightlyOldVersion, "10.0", "4.6", "M1",
				UpdateChecker.UpdateErrorVersion, null, "New Remote Printing Client installation requires .Net Framework of version 4.7 or newer to be installed on client machine. Current version : 4.6.", null);
		}

		public void TestIncorrectVersion()
		{
			AssertCheckUpdateRequirements("10.0", "4.7",
				SlightlyOldVersion, "Microsoft Windows NT A.B", "4.8", "M1",
				UpdateChecker.UpdateErrorVersion, null, "Could not parse Windows version on Remote Printing Client machine: A.B. Input string was not in a correct format.", null);
		}

		public void TestNullVersion()
		{
			AssertCheckUpdateRequirements("10.0", "4.7",
				null, null, null, null,
				IntermediateVersion, IntermediateInstall, null, null);
		}

		void AssertCheckUpdateRequirements(string requiredOSVersion, string requiredDotNetVersion,
			string clientVersion, string clientOSVersion, string clientDotNetVersion, string clientMachineName,
			string expectedVersion, string expectedInstall, string expectedErrorMessage, string expectedErrorReport, bool reportMissingClientInfo = true)
		{
			var folder = TempForTest.TempPath;

			var currentVersionFileName = Path.Combine(folder, CurrentInstall + ".version");
			File.WriteAllText(currentVersionFileName, CurrentVersion);
			var intermediateVersionFileName = Path.Combine(folder, IntermediateInstall + ".version");
			File.WriteAllText(intermediateVersionFileName, IntermediateVersion);

			try
			{
				var notifications = new NotificationsForTest();

				var updateChecker = new UpdateChecker(folder, CurrentInstall, "http://print",
					new ClientRequirements { OSMinVersion = requiredOSVersion, DotNetMinVersion = requiredDotNetVersion, IntermediateInstallationFile = IntermediateInstall },
					notifications);
				var clientUpdate = updateChecker.CheckUpdate(new ClientInfo { ClientVersion = clientVersion, OSVersion = clientOSVersion, DotNetVersion = clientDotNetVersion, MachineName = clientMachineName }, null, reportMissingClientInfo);

				AssertEquals(expectedVersion, clientUpdate.Version);

				if (expectedInstall != null)
				{
					AssertEquals($"http://print/{expectedInstall}.msi", clientUpdate.Link);
					AssertEquals(0, notifications.Notifications.Count);
				}

				if (expectedErrorMessage != null)
				{
					if (expectedInstall == null)
					{
						AssertEquals(expectedErrorMessage, clientUpdate.Link);
					}

					AssertEquals(1, notifications.Notifications.Count);
					AssertEquals(NotificationType.Error, notifications.Notifications[0].Type);
					AssertEquals(expectedErrorMessage, notifications.Notifications[0].Message);
				}

				if (expectedErrorReport != null)
				{
					AssertEquals(expectedErrorReport, ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			}
			finally
			{
				DeleteIfExists(currentVersionFileName);
				DeleteIfExists(intermediateVersionFileName);
			}
		}

		public void TestCheckUpdateHandleExceptions()
		{
			var notifications = new NotificationsForTest();

			var updateChecker = new UpdateCheckerForTest(TempForTest.TempPath, "ERROR", "http://print",
				new ClientRequirements { OSMinVersion = "10.0", DotNetMinVersion = "4.7.2", IntermediateInstallationFile = IntermediateInstall },
				notifications);
			var clientUpdate = updateChecker.CheckUpdate(new ClientInfo { ClientVersion = SlightlyOldVersion, OSVersion = "10.0", DotNetVersion = "4.7.2", MachineName = "M1" });

			AssertEquals(UpdateChecker.UpdateErrorVersion, clientUpdate.Version);

			var expectedMessage = $@"Server exception while checking update.
{typeof(ApplicationException).FullName}: Exception in GetVersion
   at Enterprise.RemotePrinting.Server.Testing.UpdateCheckerForTest.GetVersion(";

			AssertStartsWith("Should include correct error details", expectedMessage, clientUpdate.Link);
		}

		const string CurrentVersion = "1.10.0";
		const string IntermediateVersion = "1.5.0";
		const string OldVersion = "1.1.0";
		const string SlightlyOldVersion = "1.9.0";

		const string CurrentInstall = "current";
		const string IntermediateInstall = "intermediate";
	}

	class NotificationsForTest : INotifications
	{
		public IList<INotification> Notifications { get; } = new List<INotification>();

		public void Add(INotification notification)
		{
			Notifications.Add(notification);
		}
	}

	class UpdateCheckerForTest : UpdateChecker
	{
		public UpdateCheckerForTest(string folder, string installation, string url, ClientRequirements clientRequirements, INotifications notifications)
			: base(folder, installation, url, clientRequirements, notifications)
		{
		}

		protected override string GetVersion(string installationFile)
		{
			if (installationFile.Equals("ERROR", StringComparison.OrdinalIgnoreCase))
			{
				throw new ApplicationException("Exception in GetVersion");
			}

			return base.GetVersion(installationFile);
		}
	}
}
