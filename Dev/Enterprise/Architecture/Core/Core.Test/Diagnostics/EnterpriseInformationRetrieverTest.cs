using System;
using System.Net;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class EnterpriseInformationRetrieverTest : NUnit.Framework.TransactionedTestCase
	{
		public void TestDetails()
		{
			AssertDetails(false, false);
		}

		public void TestDetailsWithComplianceVersionNumber()
		{
			var mockISystemDataRegistry = new Mock<ISystemDataRegistry>();
			mockISystemDataRegistry.Setup(registry => registry.AzureApplicationClientId).Returns("103C34F9-2A81-416C-83D0-1DD29B69E7F2");
			using (ObjectFactory.Substitute(mockISystemDataRegistry.Object))
			using (RawDataRegistry.Instance.ComplianceVersionNumber.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "18.20.32"))
			{
				AssertDetails(true, true);
			}
		}

		public void TestDetailsWithWinzorNullIP()
		{
			var originalIsWinzor = Globals.IsWinzor;
			try
			{
				Globals.IsWinzor = true;
				AssertDetails(false, false);
			}
			finally
			{
				Globals.IsWinzor = originalIsWinzor;
			}
		}

		public void TestDetailsWithWinzorWithIP()
		{
			var originalIsWinzor = Globals.IsWinzor;
			try
			{
				Globals.IsWinzor = true;
				Globals.WinzorClientIpAddress = new IPAddress([192, 168, 1, 26]);
				AssertDetails(false, false, "Client IP Address: 192.168.1.26\r\n");
			}
			finally
			{
				Globals.IsWinzor = originalIsWinzor;
			}
		}

		void AssertDetails(bool expectComplianceVersion, bool appIssued, string additionalExpectedData = null)
		{
			var env = EnvProxy.Instance;
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.SystemExpiryDateForTest = env.Time.CurrentLocalDate.AddDays(env.Licence.DefaultLicenceGracePeriodInDays);
			registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			registrationKey.DbSecurityModeForTest = DatabaseSecurityModePairList.Codes.Locked;

			var appClientId = appIssued ? "103C34F9-2A81-416C-83D0-1DD29B69E7F2" : "Not issued";

			var retriever = new EnterpriseInformationRetriever();

			AssertEquals("Release", ReleaseInfo.Instance.ReleaseDisplayText, retriever.Release);
			AssertEquals("DBVersionNumber contains Schema version", true, retriever.DBVersionNumber.Contains("Schema = "));
			AssertEquals("DBVersionNumber contains Script version", true, retriever.DBVersionNumber.Contains("Script = "));
			AssertEquals("DBVersionNumber contains Data version", true, retriever.DBVersionNumber.Contains("Data = "));
			AssertEquals("DBVersionNumber contains Transformation version", true, retriever.DBVersionNumber.Contains("Transformation = "));

			AssertEquals("VersionNumber", ReleaseInfo.Instance.VersionNumber.ToString(), retriever.VersionNumber);
			AssertEquals("ExeDate", ReleaseInfo.Instance.ExeDate.ToString("dd-MMM-yyyy h:mm:ss tt"), retriever.VersionDate);
			AssertEquals("VersionDateForReport", ReleaseInfo.Instance.ExeDate.ToString("dd-MMM-yy hh:mm"), retriever.VersionDateForReport);
			AssertEquals("DBServerName", Db.ServerName, retriever.DBServerName);
			AssertEquals("DBDatabaseName", Db.DatabaseName, retriever.DBDatabaseName);
			AssertEquals("CurrentCompanyCountry", env.CurrentCompany.Country.Description, retriever.CurrentCompanyCountry);
			AssertEquals("CurrentCompanyName", env.CurrentCompany.Name, retriever.CurrentCompanyName);
			AssertEquals("TerminalServerMode", false, retriever.TerminalServerMode);
			AssertEquals("Application Id", appClientId, retriever.AzureApplicationClientId);

			AssertEquals("ClientDocumentsName", env.Registry.ClientDocumentName, retriever.ClientDocumentsName);
			AssertEquals("ClientDocumentsVersion", env.Registry.ClientDocumentVersion.ToString(), retriever.ClientDocumentsVersion);
			AssertEquals("LicenceCode", env.CurrentCompany.GetLicenceCode(" - "), retriever.LicenceCode);
			Assert("SystemDocumentsVersion.Length should not be 0.", retriever.SystemDocumentsVersion.Length > 0);
			Assert("FrameworkVersion.Length should not be 0.", retriever.FrameworkVersion.Length > 0);

			var expectedDetails = "Release: " + retriever.Release +
				"\r\nLicense Code: " + retriever.LicenceCode +
				"\r\nVersion Number: " + retriever.VersionNumber +
				"\r\nEXE Date: " + retriever.VersionDate +
				"\r\nDB Version: " + retriever.DBVersionNumber +
				"\r\nDB Server Name: " + retriever.DBServerName +
				"\r\nDB Database Name: " + retriever.DBDatabaseName +
				"\r\nTerminal Server Mode: " + (retriever.TerminalServerMode ? "Yes, " + (EnvProxy.IsRDSInstalled ? "With" : "Without") + " RD Services" : "No") +
				"\r\nCompany Name: " + retriever.CurrentCompanyName +
				"\r\nCountry: " + retriever.CurrentCompanyCountry +
				"\r\nSystem Type: " + retriever.SystemLicenceType +
				"\r\nSystem Documents Version: " + retriever.SystemDocumentsVersion +
				"\r\nClient Documents Name: " + retriever.ClientDocumentsName +
				"\r\nClient Documents Version: " + retriever.ClientDocumentsVersion +
				"\r\n.NET Framework Version: " + retriever.FrameworkVersion +
				"\r\nSQL Server Version: " + retriever.SqlServerVersion +
				"\r\nDatabase Security Mode: " + retriever.DatabaseSecurityMode +
				"\r\nApplication Id: " + appClientId + "\r\n";

			if (expectComplianceVersion)
			{
				AssertEquals("ComplianceVersionNumber", "18.20.32", retriever.ComplianceVersionNumber);
				expectedDetails += "Compliance Version : 18.20.32\r\n";
			}
			else
			{
				AssertEquals("ComplianceVersionNumber", string.Empty, retriever.ComplianceVersionNumber);
			}

			if (additionalExpectedData != null)
			{
				expectedDetails += additionalExpectedData;
			}

			AssertEquals("ToString()", expectedDetails, retriever.ToString());
		}
	}
}
