using System;
using System.Collections.Generic;
using AppDomainWrappers.Net;
using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Test.Utilities;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static System.FormattableString;
using static CargoWise.BrandManager.BrandingFactory;

namespace Enterprise.Startup.Testing
{
	sealed class SetupDbConnectionTest : AbstractApplicationStartupTaskTest<SetupDbConnection>
	{
#if !WINZOR

		[ExpectNoExceptions]
		public void TestTryToConnectToNonExistentDatabase()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var domainData = new Dictionary<string, object>
			{
				{ "ServerName", Db.ServerName },
				{ "DatabaseName", Db.DatabaseName + "NonExistent" }
			};

			using (var appDomainWrapper = new AppDomainWrapper("TestTryToConnectToNonExistentDatabase"))
			{
				appDomainWrapper.RunActionInAppDomain(() =>
				{
					Db.InitializeDatabaseDetails(
						(string)AppDomain.CurrentDomain.GetData("ServerName"),
						(string)AppDomain.CurrentDomain.GetData("DatabaseName"));

					TestingState.Setup();

					var setupDbConnection = new SetupDbConnection();
					setupDbConnection.Execute(new ApplicationArguments(new string[] { Db.ServerName, Db.DatabaseName }));
					AssertStartsWith("Connect to database error message", string.Format(@"Failed to connect to database {1}\{0}.", Db.DatabaseName, Db.ServerName), UnitTestUserNotification.Instance.LastMessage.Text);
				}, domainData);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		[ExpectNoExceptions]
		public void TestTryToConnectToNonExistentServer()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var domainData = new Dictionary<string, object>
			{
				{ "ServerName", Db.ServerName + "-NonExistent" },
				{ "DatabaseName", "TheDatabase" }
			};

			using (var appDomainWrapper = new AppDomainWrapper("TestTryToConnectToNonExistentServer"))
			{
				appDomainWrapper.RunActionInAppDomain(() =>
				{
					Db.InitializeDatabaseDetails(
						(string)AppDomain.CurrentDomain.GetData("ServerName"),
						(string)AppDomain.CurrentDomain.GetData("DatabaseName"));

					Db.Connection.OpenRetries = 0;
					TestingState.Setup();

					var setupDbConnection = new SetupDbConnection();
					setupDbConnection.Execute(new ApplicationArguments(new string[] { Db.ServerName, Db.DatabaseName }));

					AssertStartsWith("Connect to database error message", "Failed to connect to database", UnitTestUserNotification.Instance.LastMessage.Text);
				}, domainData);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		[GuiTest]
		[ExpectNoExceptions]
		public void TestOutdatedDatabaseVersionPopup()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var domainData = new Dictionary<string, object>
			{
				{ "ServerName", Db.ServerName },
				{ "DatabaseName", Db.DatabaseName }
			};

			using (var appDomainWrapper = new AppDomainWrapper("TestOutdatedDatabaseVersionPopup"))
			{
				appDomainWrapper.RunActionInAppDomain(() =>
				{
					Db.InitializeDatabaseDetails(
						(string)AppDomain.CurrentDomain.GetData("ServerName"),
						(string)AppDomain.CurrentDomain.GetData("DatabaseName"));

					Db.Connection.OpenRetries = 0;
					TestingState.Setup();
					Configure(BrandingType.CargoWiseOne);
					DesignModeFinder.SetIsDesigningForTest(true);

					var setupDbConnection = new SetupDbConnection();

					using (setupDbConnection.OverrideIsBelowMinSupportedSqlMajorVersion((serverProduct) => true))
					{
						const string firstErrorMessage = "Critical {0} Upgrade to MS {1}";
						var errorContext = SqlCutOverHelper.UpgradeToSupportedSqlVersionAction;
						ZFormModaliser.LastFormShownDialogForTest = null;
						setupDbConnection.Execute(new ApplicationArguments(new[] { Db.ServerName, Db.DatabaseName }));
						var unsupportedSqlVersionRichText = setupDbConnection.FormatMessage_ExposedForTest(firstErrorMessage, errorContext, SqlServerVersionNumber.SqlMinimumSupportedGenerationEdition);

						AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
						AssertEquals(typeof(RichTextEmailDisplayZForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
						AssertStartsWith("", Invariant($"Critical {Enterprise.Core.Constants.ProductName} Upgrade to MS {SqlServerVersionNumber.SqlMinimumSupportedGenerationEdition}"), unsupportedSqlVersionRichText.Trim());
						foreach (var line in SqlCutOverHelper.UpgradeToSupportedSqlVersionAction.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
						{
							AssertContains(line, unsupportedSqlVersionRichText);
						}
					}
				}, domainData);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}
#endif

		public override int DefaultErrorExitCode => ExitCodes.SetupDbConnectionError;
	}
}
