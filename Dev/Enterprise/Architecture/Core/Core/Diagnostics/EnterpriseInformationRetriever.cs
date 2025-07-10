using System;
using System.Globalization;
using System.Text;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Integration.Licensing;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	#region SuppressResourceStringsCheckRegion
	public class EnterpriseInformationRetriever
	{
		public string VersionNumber
		{
			get { return ReleaseInfo.Instance.VersionNumber.ToString(); }
		}

		public string ComplianceVersionNumber => RawDataRegistry.Instance.ComplianceVersionNumber.Value;

		public string AzureApplicationClientId => ObjectFactory.Get<ISystemDataRegistry>().AzureApplicationClientId;

		public string VersionDate
		{
			get { return ReleaseInfo.Instance.ExeDate.ToString("dd-MMM-yyyy h:mm:ss tt"); } // This is the format we want to show in the Help/About screen.
		}

		public string VersionDateForReport
		{
			get { return ReleaseInfo.Instance.ExeDate.ToString("dd-MMM-yy hh:mm", CultureInfo.InvariantCulture); } // This is the format we want to show in the reports.
		}

		public string DBVersionNumber
		{
			get
			{
				if (dBVersionNumber == null)
				{
					DataRegistry registry = EnvProxy.Instance.Registry;
					string schemaVersion = registry.DatabaseMajorSchemaVersion.ToString() + '.' + registry.DatabaseMinorSchemaVersion.ToString();
					string scriptVersion = registry.DatabaseMajorScriptVersion.ToString() + '.' + registry.DatabaseMinorScriptVersion.ToString();
					string transformationVersion = registry.DatabaseMajorTransformationVersion.ToString() + "." + registry.DatabaseMinorTransformationVersion.ToString();
					string dataVersion = registry.DatabaseSystemDataVersionMajor.ToString() + "." + registry.DatabaseSystemDataVersionMinor.ToString();
					var clrVersion = FormattableString.Invariant($"{registry.DatabaseMajorClrAssembliesVersion}.{registry.DatabaseMinorClrAssembliesVersion}");
					dBVersionNumber = string.Format("Schema = {0}\r\nScript = {1}\r\nData = {2}\r\nTransformation = {3}\r\nCLR Assemblies = {4}", schemaVersion, scriptVersion, dataVersion, transformationVersion, clrVersion);
				}
				return dBVersionNumber;
			}
		}
		string dBVersionNumber;

		public bool TerminalServerMode
			=> terminalService.IsRemoteAppSession;

		readonly TerminalService terminalService = ObjectFactory.Get<TerminalService>();

		public string DBServerName
		{
			get { return Db.ServerName; }
		}

		public string DBDatabaseName
		{
			get { return Db.DatabaseName; }
		}

		public string CurrentCompanyName
		{
			get { return EnvProxy.Instance.CurrentCompany.Name; }
		}

		public string CurrentCompanyCountry
		{
			get { return EnvProxy.Instance.CurrentCompany.Country.Description; }
		}

		public string SystemDocumentsVersion
		{
			get
			{
				Type docDataFileType = Type.GetType("Enterprise.DbUpgrader.Data.DocumentsDataFile, Enterprise.DbUpgrader.Data.Documents");
				if (docDataFileType != null)
				{
					object docDataFile = docDataFileType.GetConstructor(System.Type.EmptyTypes).Invoke(null);
					return docDataFileType.GetProperty("VersionInDatabase").GetGetMethod().Invoke(docDataFile, null).ToString();
				}
				return "";
			}
		}

		public string ClientDocumentsName
		{
			get { return EnvProxy.Instance.Registry.ClientDocumentName; }
		}

		public string ClientDocumentsVersion
		{
			get { return EnvProxy.Instance.Registry.ClientDocumentVersion.ToString(); }
		}

		public string FrameworkVersion
		{
			get { return System.Environment.Version.ToString(); }
		}

		public string Release
		{
			get { return ReleaseInfo.Instance.ReleaseDisplayText; }
		}

		public string LicenceCode
		{
			get
			{
				return EnvProxy.Instance.CurrentCompany.GetLicenceCode(" - ");
			}
		}

		public string SqlServerVersion
		{
			get
			{
				StringBuilder sqlServerVersion = new StringBuilder();

				sqlServerVersion.Append(Db.Connection.ServerVersionNumber.ToString());
				sqlServerVersion.Append(" (x64)");
				sqlServerVersion.AppendLine();
				sqlServerVersion.AppendLine(Db.Connection.ServerVersionNumber.FormalSqlServerGeneration);

				sqlServerVersion.Append(Db.Connection.ServerEditionText);

				return sqlServerVersion.ToString();
			}
		}

		public string SystemLicenceType
		{
			get
			{
				return DatabaseTypes.GetDescriptionFromCode(RegKey.DatabaseType);
			}
		}

		public string DatabaseSecurityMode
		{
			get
			{
				return IsDatabaseSecurityModeOpenAccordingToRegistrationKey() ?
					DatabaseSecurityModePairList.Descriptions.OpenMode :
					DatabaseSecurityModePairList.Descriptions.Locked;
			}
		}

		public bool IsDatabaseSecurityModeOpenAccordingToRegistrationKey()
		{
			string dbSecurityCode = RegKey.DbSecurityMode;
			return dbSecurityCode == DatabaseSecurityModePairList.Codes.OpenMode;
		}

		public override string ToString()
		{
			var builder = new StringBuilder();

			builder.Append("Release: ");
			builder.AppendLine(Release);
			builder.Append("License Code: ");
			builder.AppendLine(LicenceCode);
			builder.Append("Version Number: ");
			builder.AppendLine(VersionNumber);
			builder.Append("EXE Date: ");
			builder.AppendLine(VersionDate);
			builder.Append("DB Version: ");
			builder.AppendLine(DBVersionNumber);
			builder.Append("DB Server Name: ");
			builder.AppendLine(DBServerName);
			builder.Append("DB Database Name: ");
			builder.AppendLine(DBDatabaseName);
			builder.Append("Terminal Server Mode: ");
			builder.AppendLine((TerminalServerMode ? "Yes, " + (EnvProxy.IsRDSInstalled ? "With" : "Without") + (terminalService.IsCitrixICA ? "Citrix Services" : " RD Services") : "No"));
			builder.Append("Company Name: ");
			builder.AppendLine(CurrentCompanyName);
			builder.Append("Country: ");
			builder.AppendLine(CurrentCompanyCountry);
			builder.Append("System Type: ");
			builder.AppendLine(SystemLicenceType);
			builder.Append("System Documents Version: ");
			builder.AppendLine(SystemDocumentsVersion);
			builder.Append("Client Documents Name: ");
			builder.AppendLine(ClientDocumentsName);
			builder.Append("Client Documents Version: ");
			builder.AppendLine(ClientDocumentsVersion);
			builder.Append(".NET Framework Version: ");
			builder.AppendLine(FrameworkVersion);
			builder.Append("SQL Server Version: ");
			builder.AppendLine(SqlServerVersion);
			builder.Append("Database Security Mode: ");
			builder.AppendLine(DatabaseSecurityMode);
			builder.Append("Application Id: ");
			builder.AppendLine(AzureApplicationClientId);

			var complianceVersion = ComplianceVersionNumber;
			if (!string.IsNullOrEmpty(complianceVersion))
			{
				builder.Append("Compliance Version : ");
				builder.AppendLine(complianceVersion);
			}

			if (Globals.IsWinzor && Globals.WinzorClientIpAddress != null)
			{
				builder.Append("Client IP Address: ");
				builder.AppendLine(Globals.WinzorClientIpAddress.ToString());
			}

			return builder.ToString();
		}

		#region Implementation

		IProductRegistrationKey RegKey
		{
			get { return regKey ?? (regKey = ObjectFactory.Get<IProductRegistration>().Key); }
		}
		IProductRegistrationKey regKey;

		DatabaseTypes DatabaseTypes
		{
			get { return databaseTypes ?? (databaseTypes = new DatabaseTypes()); }
		}
		DatabaseTypes databaseTypes;

		#endregion
	}
	#endregion
}
