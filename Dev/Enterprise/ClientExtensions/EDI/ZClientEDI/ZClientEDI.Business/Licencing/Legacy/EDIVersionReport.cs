using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business.UserAccountReport;
using Enterprise.ZArchitecture.Core;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class EDIVersionReport
	{
		#region Construction / Extraction of Xml Data

		public EDIVersionReport(ZString xmlData)
		{
			ExtractVersionInfo(xmlData);
		}

		void ExtractVersionInfo(ZString xmlData)
		{
			try
			{
				using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlData)))
				using (XmlTextReader xmlParser = new XmlTextReader(stream))
				{
					while (xmlParser.Read())
					{
						HandleOrgPK(xmlParser);
						HandleDatabaseNumber(xmlParser);
						HandleEnterpriseCode(xmlParser);
						HandleCompanyCode(xmlParser);
						HandlePhysicalServerID(xmlParser);
						HandleDBServerName(xmlParser);
						HandleDBName(xmlParser);
						HandleCurrentVersion(xmlParser);
						HandleCurrentDate(xmlParser);
						HandleCurrentRelease(xmlParser);
						HandlePreferredUpgradeMethod(xmlParser);
						HandleDBServerSecurityMode(xmlParser);
						HandlePublicEmailAddressForUpdate(xmlParser);
						HandleSQLServerName(xmlParser);
						HandleSQLServerInstanceName(xmlParser);
						HandleServerSID(xmlParser);
						HandlePOP3(xmlParser);
						HandleSMTP(xmlParser);
						HandleDatabaseFileNameList(xmlParser);
						HandleLicenceKeyList(xmlParser);
						HandleCompanyList(xmlParser);
						HandleBranchList(xmlParser);
						HandleStaffList(xmlParser);
						HandleDatabaseBackupPath(xmlParser);
						HandleEncryptedSystemExpirationKey(xmlParser);
						HandleEncryptedRegistrationKey(xmlParser);
						HandleDocEnginePrintingStats(xmlParser);
						HandleSqlServerVersionDetails(xmlParser);
						HandleAdditionalDatabaseSystemInfoList(xmlParser);
						HandleLicenceUsage(xmlParser);
						HandleOutboundEAdaptorUrl(xmlParser);
						HandleNextRunTimeUtcUPG(xmlParser);
						HandleNextRunTimeUtcMUG(xmlParser);
						HandleScheduleStateUPG(xmlParser);
						HandleScheduleStateMUG(xmlParser);
						HandleTokenAuthenticationEnabled(xmlParser);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string message = xmlData.IsEmpty ? "xmlData is empty." : string.Format(CultureInfo.CurrentCulture, "xmlData is not valid.\r\n\r\nxmlData:\r\n\r\n{0}", xmlData);
				throw new InvalidOperationException(message, ex);
			}
		}

		#region Node Handlers

		void HandleOrgPK(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.OrganisationPK)
			{
				OrganisationPK = new ZGuid(xmlParser.ReadString());
			}
		}

		void HandleDatabaseNumber(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.DatabaseNumber)
			{
				if (int.TryParse(xmlParser.ReadString(), out var num))
				{
					DatabaseNumber = num;
				}
				else
				{
					DatabaseNumber = 0;
				}
			}
		}

		void HandleEnterpriseCode(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.EnterpriseCode)
			{
				EnterpriseCode = xmlParser.ReadString();
			}
		}

		void HandleCompanyCode(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.CompanyCode)
			{
				CompanyCode = xmlParser.ReadString();
			}
		}

		void HandlePhysicalServerID(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.PhysicalServerID)
			{
				PhysicalServerID = xmlParser.ReadString();
			}
		}

		void HandleDBServerName(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.DBServerName)
			{
				DBServerName = xmlParser.ReadString();
			}
		}

		void HandleDBName(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.DBName)
			{
				DBName = xmlParser.ReadString();
			}
		}

		void HandleCurrentVersion(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.CurrentVersion)
			{
				CurrentVersion = xmlParser.ReadString();
			}
		}

		void HandleCurrentDate(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.CurrentDate)
			{
				ZDateTime parsedDate = ParseDate(xmlParser);
				if (parsedDate != ZDateTime.Empty)
				{
					CurrentDate = parsedDate;
				}
			}
		}

		void HandleCurrentRelease(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.CurrentRelease)
			{
				CurrentRelease = xmlParser.ReadString();
			}
		}

		void HandlePreferredUpgradeMethod(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.PreferredUpgradeMethod)
			{
				PreferredUpgradeMethod = xmlParser.ReadString();
			}
		}

		void HandleDBServerSecurityMode(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.DBServerSecurityMode)
			{
				DBServerSecurityMode = xmlParser.ReadString();
			}
		}

		void HandlePublicEmailAddressForUpdate(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.PublicEmailAddressForUpdate)
			{
				InternalPOP3EmailAddress = xmlParser.ReadString(); // Deprecated PublicEmailAddressForUpdate XML Tag. Remains for backwards compatibility
			}
		}

		void HandleSQLServerName(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.SQLServerName)
			{
				SQLServerName = xmlParser.ReadString();
			}
		}

		void HandleSQLServerInstanceName(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.SQLServerInstanceName)
			{
				SQLServerInstanceName = xmlParser.ReadString();
			}
		}

		void HandleServerSID(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.ServerSID)
			{
				ServerSID = new ZGuid(xmlParser.ReadString());
			}
		}

		void HandlePOP3(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.POP3)
			{
				do
				{
					xmlParser.Read();
					switch (xmlParser.Name)
					{
						case ElementNames.MailboxEmailAddress:
							InternalPOP3EmailAddress = xmlParser.ReadString();
							break;
						case ElementNames.UserName:
							InternalPOP3UserName = xmlParser.ReadString();
							break;
						case ElementNames.MailServer:
							InternalPOP3MailServer = xmlParser.ReadString();
							break;
						case ElementNames.Port:
							InternalMailServerPort = ZInt.Parse(xmlParser.ReadString());
							break;
					}
				}
				while (xmlParser.Name != ElementNames.POP3);
			}
		}

		void HandleSMTP(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.SMTP)
			{
				do
				{
					xmlParser.Read();
					switch (xmlParser.Name)
					{
						case ElementNames.MailServer:
							InternalSMTPMailServer = xmlParser.ReadString();
							break;
						case ElementNames.Port:
							InternalSMTPPort = ZInt.Parse(xmlParser.ReadString());
							break;
					}
				}
				while (xmlParser.Name != ElementNames.SMTP);
			}
		}

		void HandleDatabaseFileNameList(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.DBFileNameList)
			{
				do
				{
					xmlParser.Read();
					if (xmlParser.Name == ElementNames.DBFileName)
					{
						LogAndDataFiles.Add(xmlParser.ReadString());
					}
				}
				while (xmlParser.Name != ElementNames.DBFileNameList);
			}
		}

		void HandleLicenceKeyList(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.LicenceKeyList)
			{
				do
				{
					xmlParser.Read();
					if (xmlParser.Name == ElementNames.LicenceKey)
					{
						LicenceKeys.Add(xmlParser.ReadString());
					}
				}
				while (xmlParser.Name != ElementNames.LicenceKeyList);
			}
		}

		void HandleCompanyList(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.CompanyList)
			{
				do
				{
					xmlParser.Read();
					if (xmlParser.Name == ElementNames.Company)
					{
						var co = new EdiCompanyReport();
						co.Parse(xmlParser);
						CompanyList.Add(co);
					}
				}
				while (xmlParser.Name != ElementNames.CompanyList);
			}
		}

		void HandleBranchList(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.BranchList)
			{
				do
				{
					xmlParser.Read();
					if (xmlParser.Name == ElementNames.Branch)
					{
						var branchReport = new BranchReport();
						branchReport.Parse(xmlParser);
						BranchList.Add(branchReport);
					}
				}
				while (xmlParser.Name != ElementNames.BranchList);
			}
		}

		void HandleStaffList(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.StaffList)
			{
				do
				{
					xmlParser.Read();
					if (xmlParser.Name == ElementNames.Staff)
					{
						var staffReport = new StaffReport();
						staffReport.Parse(xmlParser);
						StaffList.Add(staffReport);
					}
					else if (xmlParser.Name == ElementNames.IsFullStaffList)
					{
						IsFullStaffList = XmlConvert.ToBoolean(xmlParser.ReadString());
					}
				}
				while (xmlParser.Name != ElementNames.StaffList);
			}
		}

		void HandleDatabaseBackupPath(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.DatabaseBackupPath)
			{
				DatabaseBackupPath = xmlParser.ReadString();
			}
		}

		void HandleEncryptedSystemExpirationKey(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.EncryptedSystemExpirationKey)
			{
				EncryptedSystemExpirationKey = xmlParser.ReadString();
			}
		}

		void HandleEncryptedRegistrationKey(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.EncryptedRegistrationKey)
			{
				EncryptedRegistrationKey = xmlParser.ReadString();
			}
		}

		void HandleDocEnginePrintingStats(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.DocEngineStats)
			{
				do
				{
					xmlParser.Read();
					if (xmlParser.Name == ElementNames.DocEngineActivePrintersCount)
					{
						ActivePrintersCount = ZInt.Parse(xmlParser.ReadString());
					}
				}
				while (xmlParser.Name != ElementNames.DocEngineStats);
			}
		}

		void HandleSqlServerVersionDetails(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.SqlServerVersionDetails)
			{
				xmlParser.ReadStartElement();
				xmlParser.ReadElementString(); // leave this here to support on xml structure
				string sqlServerEditionText = xmlParser.ReadElementString();
				switch (sqlServerEditionText)
				{
					// Check for old enum values (changed in WI00020517)
					case "Enterprise":
					case "Developer": SqlServerEdition = DbConnection.SqlServerEdition.EnterpriseDeveloper; break;
					case "Standard": SqlServerEdition = DbConnection.SqlServerEdition.StandardWorkgroup; break;
					case "Desktop": SqlServerEdition = DbConnection.SqlServerEdition.Express; break;
					default:
						SqlServerEdition = (DbConnection.SqlServerEdition)Enum.Parse(typeof(DbConnection.SqlServerEdition), sqlServerEditionText, true);
						break;
				}
				var xmlVersionString = xmlParser.ReadElementString();
				SqlServerVersion = SqlServerVersionNumber.SupportedVersions.Any(x => x.Generation.Name.Equals(xmlVersionString, StringComparison.OrdinalIgnoreCase))
					? xmlVersionString
					: "Other";

				SqlServerFullVersionText = xmlParser.ReadElementString();
				xmlParser.ReadEndElement();
			}
		}

		void HandleAdditionalDatabaseSystemInfoList(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.AdditionalDatabaseSystemInfoList)
			{
				do
				{
					xmlParser.Read();
					if (xmlParser.Name == ElementNames.AdditionalDatabaseSystemInfo)
					{
						AdditionalDatabaseSystemInfoList.Add(xmlParser.ReadString());
					}
				}
				while (xmlParser.Name != ElementNames.AdditionalDatabaseSystemInfoList);
			}
		}

		void HandleLicenceUsage(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.LicenceUsage)
			{
				LicenceUsage = xmlParser.ReadString();
			}
		}

		void HandleOutboundEAdaptorUrl(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.OutboundEAdaptorUrl)
			{
				OutboundEAdaptorUrl = xmlParser.ReadString();
			}
		}

		void HandleNextRunTimeUtcUPG(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.NextRunTimeUtcUPG)
			{
				ZDateTime parsedDate = ParseDate(xmlParser);
				if (parsedDate != ZDateTime.Empty)
				{
					NextRunTimeUtcUPG = parsedDate;
				}
			}
		}

		void HandleNextRunTimeUtcMUG(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.NextRunTimeUtcMUG)
			{
				ZDateTime parsedDate = ParseDate(xmlParser);
				if (parsedDate != ZDateTime.Empty)
				{
					NextRunTimeUtcMUG = parsedDate;
				}
			}
		}

		ZDateTime ParseDate(XmlTextReader xmlParser)
		{
			ZDateTime parsedDate = ZDateTime.Empty;
			var s = xmlParser.ReadString();
			if (DateTime.TryParseExact(s, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
			{
				parsedDate = result;
			}
			else
			{
				if (ZDateTime.TryParseISO8601Date(s, out var d))
				{
					parsedDate = d;
				}
			}
			return parsedDate;
		}

		void HandleScheduleStateUPG(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.ScheduleStateUPG)
			{
				ScheduleStateUPG = xmlParser.ReadString();
			}
		}

		void HandleScheduleStateMUG(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.ScheduleStateMUG)
			{
				ScheduleStateMUG = xmlParser.ReadString();
			}
		}

		void HandleTokenAuthenticationEnabled(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.TokenAuthenticationEnabled
				&& ZBool.TryParse(xmlParser.ReadString(), out var enabled))
			{
				TokenAuthenticationEnabled = enabled;
			}
		}

		#endregion

		#endregion

		#region Version Report Properties

		public ZGuid OrganisationPK
		{
			get { return organisationPK; }
			private set
			{
				organisationPK = value;
				OrganisationPKSpecified = true;
			}
		}
		ZGuid organisationPK;

		public bool OrganisationPKSpecified { get; private set; }

		public int DatabaseNumber
		{
			get { return databaseNumber; }
			private set
			{
				databaseNumber = value;
				DatabaseNumberSpecified = true;
			}
		}
		int databaseNumber;

		public bool DatabaseNumberSpecified { get; private set; }

		public ZString EnterpriseCode
		{
			get { return enterpriseCode; }
			private set
			{
				enterpriseCode = value;
				EnterpriseCodeSpecified = true;
			}
		}
		ZString enterpriseCode;
		public bool EnterpriseCodeSpecified { get; private set; }

		public ZString CompanyCode
		{
			get { return companyCode; }
			private set
			{
				companyCode = value;
				CompanyCodeSpecified = true;
			}
		}
		ZString companyCode;
		public bool CompanyCodeSpecified { get; private set; }

		public ZString PhysicalServerID
		{
			get { return physicalServerID; }
			private set
			{
				physicalServerID = value;
				PhysicalServerIDSpecified = true;
			}
		}
		ZString physicalServerID;
		public bool PhysicalServerIDSpecified { get; private set; }

		public ZString DBServerName
		{
			get { return dbServerName; }
			private set
			{
				dbServerName = value;
				DBServerNameSpecified = true;
			}
		}
		ZString dbServerName;
		public bool DBServerNameSpecified { get; private set; }

		public ZString DBName
		{
			get { return dbName; }
			private set
			{
				dbName = value;
				DBNameSpecified = true;
			}
		}
		ZString dbName;
		public bool DBNameSpecified { get; private set; }

		public ZString CurrentVersion
		{
			get { return currentVersion; }
			private set
			{
				currentVersion = value;
				CurrentVersionSpecified = true;
			}
		}
		ZString currentVersion;
		public bool CurrentVersionSpecified { get; private set; }

		public ZString CurrentRelease
		{
			get { return currentRelease; }
			private set
			{
				currentRelease = value;
				CurrentReleaseSpecified = true;
			}
		}
		ZString currentRelease;
		public bool CurrentReleaseSpecified { get; private set; }

		public ZDateTime CurrentDate
		{
			get;
			private set;
		}

		public ZString PreferredUpgradeMethod
		{
			get { return preferredUpgradeMethod; }
			private set
			{
				preferredUpgradeMethod = value;
				PreferredUpgradeMethodSpecified = true;
			}
		}
		ZString preferredUpgradeMethod;
		public bool PreferredUpgradeMethodSpecified { get; private set; }

		public ZString DBServerSecurityMode
		{
			get { return dbServerSecurityMode; }
			internal set
			{
				dbServerSecurityMode = value;
				DBServerSecurityModeSpecified = true;
			}
		}
		ZString dbServerSecurityMode;
		public bool DBServerSecurityModeSpecified { get; private set; }

		public ZString SQLServerName
		{
			get { return sqlServerName; }
			private set
			{
				sqlServerName = value;
				SQLServerNameSpecified = true;
			}
		}
		ZString sqlServerName;
		public bool SQLServerNameSpecified { get; private set; }

		public ZString SQLServerInstanceName
		{
			get { return sqlServerInstanceName; }
			private set
			{
				sqlServerInstanceName = value;
				SQLServerInstanceNameSpecified = true;
			}
		}
		ZString sqlServerInstanceName;
		public bool SQLServerInstanceNameSpecified { get; private set; }

		public ZGuid ServerSID
		{
			get { return serverSID; }
			internal set
			{
				serverSID = value;
				ServerSIDSpecified = true;
			}
		}
		ZGuid serverSID;
		public bool ServerSIDSpecified { get; private set; }

		public ZString InternalPOP3EmailAddress
		{
			get { return internalPOP3EmailAddress; }
			internal set
			{
				internalPOP3EmailAddress = value;
				InternalPOP3EmailAddressSpecified = true;
			}
		}
		ZString internalPOP3EmailAddress;
		public bool InternalPOP3EmailAddressSpecified { get; private set; }

		public ZString InternalPOP3UserName
		{
			get { return internalPOP3UserName; }
			internal set
			{
				internalPOP3UserName = value;
				InternalPOP3UserNameSpecified = true;
			}
		}
		ZString internalPOP3UserName;
		public bool InternalPOP3UserNameSpecified { get; private set; }

		public ZString InternalPOP3MailServer
		{
			get { return internalPOP3MailServer; }
			internal set
			{
				internalPOP3MailServer = value;
				InternalPOP3MailServerSpecified = true;
			}
		}
		ZString internalPOP3MailServer;
		public bool InternalPOP3MailServerSpecified { get; private set; }

		public ZInt InternalMailServerPort
		{
			get { return internalMailServerPort; }
			internal set
			{
				internalMailServerPort = value;
				InternalMailServerPortSpecified = true;
			}
		}
		ZInt internalMailServerPort;
		public bool InternalMailServerPortSpecified { get; private set; }

		public ZString InternalSMTPMailServer
		{
			get { return internalSMTPMailServer; }
			internal set
			{
				internalSMTPMailServer = value;
				InternalSMTPMailServerSpecified = true;
			}
		}
		ZString internalSMTPMailServer;
		public bool InternalSMTPMailServerSpecified { get; private set; }

		public ZInt InternalSMTPPort
		{
			get { return internalSMTPPort; }
			internal set
			{
				internalSMTPPort = value;
				InternalSMTPPortSpecified = true;
			}
		}
		ZInt internalSMTPPort;
		public bool InternalSMTPPortSpecified { get; private set; }

		public List<String> AdditionalDatabaseSystemInfoList
		{
			get { return additionalDatabaseSystemInfoList ?? (additionalDatabaseSystemInfoList = new List<string>()); }
		}
		List<String> additionalDatabaseSystemInfoList;

		public string LicenceUsage
		{
			get;
			internal set;
		}

		public StringCollectionX LogAndDataFiles
		{
			get
			{
				if (fLogAndDataFiles == null)
				{
					fLogAndDataFiles = new StringCollectionX();
				}

				return fLogAndDataFiles;
			}
		}
		StringCollectionX fLogAndDataFiles;

		public StringCollectionX LicenceKeys
		{
			get
			{
				if (fLicenceKeys == null)
				{
					fLicenceKeys = new StringCollectionX();
				}

				return fLicenceKeys;
			}
		}
		StringCollectionX fLicenceKeys;

		public ZString DatabaseBackupPath
		{
			get { return databaseBackupPath; }
			private set
			{
				databaseBackupPath = value;
				DatabaseBackupPathSpecified = true;
			}
		}
		ZString databaseBackupPath;
		public bool DatabaseBackupPathSpecified { get; private set; }

		public List<EdiCompanyReport> CompanyList
		{
			get { return companyList ?? (companyList = new List<EdiCompanyReport>()); }
		}
		List<EdiCompanyReport> companyList;

		public List<BranchReport> BranchList
		{
			get { return branchList ?? (branchList = new List<BranchReport>()); }
		}
		List<BranchReport> branchList;

		public List<StaffReport> StaffList
		{
			get { return staffList ?? (staffList = new List<StaffReport>()); }
		}
		List<StaffReport> staffList;

		public ZBool IsFullStaffList { get; private set; }

		public ZString EncryptedSystemExpirationKey
		{
			get { return encryptedSystemExpirationKey; }
			internal set
			{
				encryptedSystemExpirationKey = value;
				EncryptedSystemExpirationKeySpecified = true;
			}
		}
		ZString encryptedSystemExpirationKey;
		public bool EncryptedSystemExpirationKeySpecified { get; private set; }

		public ZString EncryptedRegistrationKey
		{
			get { return encryptedRegistrationKey; }
			private set
			{
				encryptedRegistrationKey = value;
				EncryptedRegistrationKeySpecified = true;
			}
		}
		ZString encryptedRegistrationKey;
		public bool EncryptedRegistrationKeySpecified { get; private set; }

		public ZInt ActivePrintersCount
		{
			get { return activePrintersCount; }
			private set
			{
				activePrintersCount = value;
				ActivePrintersCountSpecified = true;
			}
		}
		ZInt activePrintersCount;
		public bool ActivePrintersCountSpecified { get; private set; }

		public DbConnection.SqlServerEdition SqlServerEdition
		{
			get { return sqlServerEdition; }
			private set
			{
				sqlServerEdition = value;
				SqlServerEditionSpecified = true;
			}
		}
		public DbConnection.SqlServerEdition sqlServerEdition;
		public bool SqlServerEditionSpecified { get; private set; }

		public ZString SqlServerFullVersionText
		{
			get { return sqlServerFullVersionText; }
			private set
			{
				sqlServerFullVersionText = value;
				SqlServerFullVersionTextSpecified = true;
			}
		}
		ZString sqlServerFullVersionText;
		public bool SqlServerFullVersionTextSpecified { get; private set; }

		public string SqlServerVersion
		{
			get { return sqlServerVersion; }
			private set
			{
				sqlServerVersion = value;
				SqlServerVersionSpecified = true;
			}
		}
		string sqlServerVersion;
		public bool SqlServerVersionSpecified { get; private set; }

		public ZString OutboundEAdaptorUrl
		{
			get { return outboundEAdaptorUrl; }
			private set
			{
				outboundEAdaptorUrl = value;
				OutboundEAdaptorUrlSpecified = true;
			}
		}
		ZString outboundEAdaptorUrl;
		public bool OutboundEAdaptorUrlSpecified { get; private set; }

		public ZDateTime NextRunTimeUtcUPG
		{
			get { return nextRunTimeUtcUPG; }
			private set
			{
				nextRunTimeUtcUPG = value;
				NextRunTimeUtcUPGSpecified = true;
			}
		}
		ZDateTime nextRunTimeUtcUPG;
		public bool NextRunTimeUtcUPGSpecified { get; private set; }

		public ZDateTime NextRunTimeUtcMUG
		{
			get { return nextRunTimeUtcMUG; }
			private set
			{
				nextRunTimeUtcMUG = value;
				NextRunTimeUtcMUGSpecified = true;
			}
		}
		ZDateTime nextRunTimeUtcMUG;
		public bool NextRunTimeUtcMUGSpecified { get; private set; }

		public ZString ScheduleStateUPG
		{
			get { return scheduleStateUPG; }
			private set
			{
				scheduleStateUPG = value;
				ScheduleStateUPGSpecified = true;
			}
		}
		ZString scheduleStateUPG;
		public bool ScheduleStateUPGSpecified { get; private set; }

		public ZString ScheduleStateMUG
		{
			get { return scheduleStateMUG; }
			private set
			{
				scheduleStateMUG = value;
				ScheduleStateMUGSpecified = true;
			}
		}
		ZString scheduleStateMUG;
		public bool ScheduleStateMUGSpecified { get; private set; }

		public ZBool TokenAuthenticationEnabled
		{
			get => tokenAuthenticationEnabled;
			set
			{
				tokenAuthenticationEnabled = value;
				TokenAuthenticationEnabledSpecified = true;
			}
		}
		ZBool tokenAuthenticationEnabled;
		public bool TokenAuthenticationEnabledSpecified { get; private set; }

		#endregion

		#region Company

		public class EdiCompanyReport
		{
			// Only used internally by the CurrentVersionReportProcessor
			public Guid OrgPKThatGeneratedThisLicence { get; set; }

			public string Code { get; set; }
			public string Name { get; set; }
			public bool IsActive { get; set; }
			public Guid PK { get; set; }
			public string CountryCode { get; set; }
			public string CurrencyCode { get; set; }
			public string Address1 { get; set; }
			public string Address2 { get; set; }
			public string City { get; set; }
			public string PostCode { get; set; }
			public string State { get; set; }
			public string Unloco { get; set; }
			public string Phone { get; set; }
			public string BusinessRegNo { get; set; }
			public string BusinessRegNo2 { get; set; }
			public string CustomsRegistrationNo { get; set; }
			public string WebAddress { get; set; }
			public string Email { get; set; }
			public bool IsGSTRegistered { get; set; }
			public bool IsGSTCashBasis { get; set; }
			public bool IsWHTRegistered { get; set; }
			public bool IsWHTCashBasis { get; set; }
			public bool IsReciprocal { get; set; }

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
			internal void Parse(XmlReader xmlParser)
			{
				do
				{
					xmlParser.Read();
					switch (xmlParser.Name)
					{
						case CompanyNames.Code: Code = xmlParser.ReadString(); break;
						case CompanyNames.Name: Name = xmlParser.ReadString(); break;
						case CompanyNames.CountryCode: CountryCode = xmlParser.ReadString(); break;
						case CompanyNames.CurrencyCode: CurrencyCode = xmlParser.ReadString(); break;
						case CompanyNames.Address1: Address1 = xmlParser.ReadString(); break;
						case CompanyNames.Address2: Address2 = xmlParser.ReadString(); break;
						case CompanyNames.City: City = xmlParser.ReadString(); break;
						case CompanyNames.State: State = xmlParser.ReadString(); break;
						case CompanyNames.Unloco: Unloco = xmlParser.ReadString(); break;
						case CompanyNames.PostCode: PostCode = xmlParser.ReadString(); break;
						case CompanyNames.Phone: Phone = xmlParser.ReadString(); break;
						case CompanyNames.BusinessRegNo: BusinessRegNo = xmlParser.ReadString(); break;
						case CompanyNames.BusinessRegNo2: BusinessRegNo2 = xmlParser.ReadString(); break;
						case CompanyNames.CustomsRegistrationNo: CustomsRegistrationNo = xmlParser.ReadString(); break;
						case CompanyNames.WebAddress: WebAddress = xmlParser.ReadString(); break;
						case CompanyNames.Email: Email = xmlParser.ReadString(); break;

						case CompanyNames.IsActive: IsActive = ReadBool(xmlParser); break;
						case CompanyNames.IsGSTRegistered: IsGSTRegistered = ReadBool(xmlParser); break;
						case CompanyNames.IsGSTCashBasis: IsGSTCashBasis = ReadBool(xmlParser); break;
						case CompanyNames.IsWHTRegistered: IsWHTRegistered = ReadBool(xmlParser); break;
						case CompanyNames.IsWHTCashBasis: IsWHTCashBasis = ReadBool(xmlParser); break;
						case CompanyNames.IsReciprocal: IsReciprocal = ReadBool(xmlParser); break;

						case CompanyNames.PK: PK = SafeReadGuid(xmlParser); break;

						default: break;
					}
				}
				while (xmlParser.Name != ElementNames.Company);
			}

			static Guid SafeReadGuid(XmlReader xmlParser)
			{
				try
				{
					return new ZGuid(xmlParser.ReadString()).ToGuid();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					return Guid.Empty;
				}
			}

			static bool ReadBool(XmlReader xmlParser)
			{
				return XmlConvert.ToBoolean(xmlParser.ReadString());
			}

			static class CompanyNames
			{
				public const string Code = "Code";
				public const string Name = "Name";
				public const string IsActive = "IsActive";
				public const string PK = "PK";
				public const string CountryCode = "CountryCode";
				public const string CurrencyCode = "CurrencyCode";
				public const string Address1 = "Address1";
				public const string Address2 = "Address2";
				public const string City = "City";
				public const string PostCode = "PostCode";
				public const string State = "State";
				public const string Unloco = "Unloco";
				public const string Phone = "Phone";
				public const string BusinessRegNo = "BusinessRegNo";
				public const string BusinessRegNo2 = "BusinessRegNo2";
				public const string CustomsRegistrationNo = "CustomsRegistrationNo";
				public const string WebAddress = "WebAddress";
				public const string Email = "Email";
				public const string IsGSTRegistered = "IsGSTRegistered";
				public const string IsGSTCashBasis = "IsGSTCashBasis";
				public const string IsWHTRegistered = "IsWHTRegistered";
				public const string IsWHTCashBasis = "IsWHTCashBasis";
				public const string IsReciprocal = "IsReciprocal";
			}
		}

		#endregion

		#region For Test
#if DEBUG
		public EDIVersionReport(ZGuid organisationPK, ZString enterpriseCode, ZString companyCode, ZString physicalServerID,
			ZString dbServerName, ZString dbName, ZString currentVersion, ZString releaseRing, ZDateTime currentDate, List<String> additionalInfoList = null,
			string licenceUsage = null, string outboundeAdapterUrl = null, ZDateTime? nextRunTimeUtcUPG = null, ZDateTime? nextRunTimeUtcMUG = null, string scheduleStateUPG = null, string scheduleStateMUG = null, bool tokenAuthenticationEnabled = false)
		{
			this.OrganisationPK = organisationPK;
			this.EnterpriseCode = enterpriseCode;
			this.CompanyCode = companyCode;
			this.PhysicalServerID = physicalServerID;
			this.DBServerName = dbServerName;
			this.DBName = dbName;
			this.ServerSID = new ZGuid(LegacyServerSid);
			this.CurrentVersion = currentVersion;
			this.CurrentDate = currentDate;
			this.CurrentRelease = ReleaseInfo.GetReleaseDisplayText(releaseRing, new VersionNumber(currentVersion));
			this.PreferredUpgradeMethod = "DEF";
			this.DBServerSecurityMode = DatabaseSecurityModePairList.Codes.ExOpen;
			var sqlNames = ExtractServerAndInstanceFromDatabase(DBServerName);
			this.SQLServerName = sqlNames[0];
			this.SQLServerInstanceName = sqlNames[1];
			this.InternalPOP3EmailAddress = Env.Registry.MailboxEmailAddress;
			this.InternalPOP3UserName = Env.Registry.MailboxUserName;
			this.InternalPOP3MailServer = Env.Registry.MailServer;
			this.InternalMailServerPort = Env.Registry.MailServerPort;
			this.InternalSMTPMailServer = Env.Registry.SMTPServer;
			this.InternalSMTPPort = Env.Registry.SMTPPort;
			GetAllLogAndDataFilesForAllRelatedDatabases(LogAndDataFiles);
			this.DatabaseBackupPath = Env.Registry.BackupDirectoryPath;
			this.EncryptedSystemExpirationKey = SystemRegistrationKey.Current.ToEncryptedKeyString();
			this.ActivePrintersCount = 1;
			this.additionalDatabaseSystemInfoList = additionalInfoList;
			this.LicenceUsage = licenceUsage;
			this.OutboundEAdaptorUrl = outboundeAdapterUrl;
			if (nextRunTimeUtcUPG != null)
			{
				this.NextRunTimeUtcUPG = (ZDateTime)nextRunTimeUtcUPG;
			}
			if (nextRunTimeUtcMUG != null)
			{
				this.NextRunTimeUtcMUG = (ZDateTime)nextRunTimeUtcMUG;
			}
			this.ScheduleStateUPG = scheduleStateUPG;
			this.ScheduleStateMUG = scheduleStateMUG;
			this.TokenAuthenticationEnabled = tokenAuthenticationEnabled;

			var lic = new LegacyLicence();
			lic.Company.EnterpriseCode = enterpriseCode;
			lic.Company.Code = companyCode;
			lic.Company.PhysicalServerID = physicalServerID;
			lic.Core.LicenceType = LicenceTypes.Codes.PUR;
			lic.Core.UserLimit = 999;
			lic.Accountant.LicenceType = LicenceTypes.Codes.PUR;
			lic.Accountant.UserLimit = 999;
			this.LicenceKeys.Add(lic.ToEncryptedKeyString());
		}

		public const string LegacyServerSid = "D5A7166B-5101-452E-8F11-C0C0762BD759";

		public static EDIVersionReport CreateForTest(Guid orgPk, ZDateTime currentDate)
		{
			return new EDIVersionReport(orgPk, "ABC", "SYD", "123", "NTSQLSRV\\TESTDB", "OdysseyTest", "1.1.1000.10000", ReleaseRings.Codes.GPR, currentDate);
		}

		string[] ExtractServerAndInstanceFromDatabase(string fullyQualifiedDatabaseName)
		{
			string[] result;

			if (fullyQualifiedDatabaseName.IndexOf('\\') > -1)
			{
				result = fullyQualifiedDatabaseName.Split('\\');
			}
			else
			{
				result = new string[] { fullyQualifiedDatabaseName, "" };
			}

			return result;
		}

		#region System Information

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "Test data only")]
		void GetAllLogAndDataFilesForAllRelatedDatabases(StringCollectionX logAndDataFiles)
		{
			logAndDataFiles.Add(@"C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY.MDF");
			logAndDataFiles.Add(@"C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY_LOG.LDF");
			logAndDataFiles.Add(@"C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY_SD001_DATA.MDF");
			logAndDataFiles.Add(@"C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY_SD001_LOG.LDF");
		}

		#endregion

		#region GenerateLegacyXmlForTest

		public string GenerateLegacyXmlForTest()
		{
			using (MemoryStream stream = new MemoryStream())
			using (XmlTextWriter writer = new XmlTextWriter(stream, new UTF8Encoding()))
			{
				WriteXMLHeader(writer);
				WriteXMLBody(writer);
				WriteXMLFooter(writer);
				writer.Flush();
				return Encoding.UTF8.GetString(stream.ToArray());
			}
		}

		public void WriteXMLHeader(XmlTextWriter writer)
		{
			writer.Formatting = Formatting.Indented;
			writer.WriteStartDocument();
		}

		public void WriteXMLBody(XmlTextWriter writer)
		{
			writer.WriteStartElement(ElementNames.VersionReport);
			writer.WriteElementString(ElementNames.OrganisationPK, OrganisationPK.ToString().ToUpper(CultureInfo.InvariantCulture));
			writer.WriteElementString(ElementNames.EnterpriseCode, EnterpriseCode);
			writer.WriteElementString(ElementNames.CompanyCode, CompanyCode);
			writer.WriteElementString(ElementNames.PhysicalServerID, PhysicalServerID);
			writer.WriteElementString(ElementNames.DBServerName, DBServerName);
			writer.WriteElementString(ElementNames.DBName, DBName);
			writer.WriteElementString(ElementNames.CurrentVersion, CurrentVersion);
			writer.WriteElementString(ElementNames.CurrentDate, CurrentDate.ToString().ToUpper(CultureInfo.InvariantCulture));
			writer.WriteElementString(ElementNames.CurrentRelease, CurrentRelease);
			writer.WriteElementString(ElementNames.PreferredUpgradeMethod, PreferredUpgradeMethod);
			writer.WriteElementString(ElementNames.DBServerSecurityMode, DBServerSecurityMode);
			writer.WriteElementString(ElementNames.SQLServerName, SQLServerName);
			writer.WriteElementString(ElementNames.SQLServerInstanceName, SQLServerInstanceName);
			writer.WriteElementString(ElementNames.ServerSID, ServerSID.ToString().ToUpper(CultureInfo.InvariantCulture));
			WritePOP3Details(writer);
			WriteSMTPDetails(writer);
			WriteDBFileNameList(writer);
			WriteLicenceKeys(writer);
			writer.WriteElementString(ElementNames.DatabaseBackupPath, DatabaseBackupPath);
			writer.WriteElementString(ElementNames.EncryptedSystemExpirationKey, EncryptedSystemExpirationKey);
			WriteDocEngineStats(writer);
			WriteSqlServerVersionDetails(writer);
			WriteAdditionalDatabaseSystemInfoList(writer);
			WriteLicenceUsage(writer);
			writer.WriteElementString(ElementNames.OutboundEAdaptorUrl, OutboundEAdaptorUrl);
			writer.WriteElementString(ElementNames.NextRunTimeUtcUPG, NextRunTimeUtcUPG.ToString().ToUpper(CultureInfo.InvariantCulture));
			writer.WriteElementString(ElementNames.NextRunTimeUtcMUG, NextRunTimeUtcMUG.ToString().ToUpper(CultureInfo.InvariantCulture));
			writer.WriteElementString(ElementNames.ScheduleStateUPG, ScheduleStateUPG);
			writer.WriteElementString(ElementNames.ScheduleStateMUG, ScheduleStateMUG);
			writer.WriteElementString(ElementNames.TokenAuthenticationEnabled, TokenAuthenticationEnabled.ToString());
		}

		public void WriteXMLFooter(XmlTextWriter writer)
		{
			writer.WriteEndElement();
			writer.WriteEndDocument();
		}

		void WritePOP3Details(XmlTextWriter writer)
		{
			writer.WriteStartElement(ElementNames.POP3);
			writer.WriteElementString(ElementNames.MailboxEmailAddress, InternalPOP3EmailAddress);
			writer.WriteElementString(ElementNames.UserName, InternalPOP3UserName);
			writer.WriteElementString(ElementNames.MailServer, InternalPOP3MailServer);
			writer.WriteElementString(ElementNames.Port, InternalMailServerPort.ToString());
			writer.WriteEndElement();
		}

		void WriteSMTPDetails(XmlTextWriter writer)
		{
			writer.WriteStartElement(ElementNames.SMTP);
			writer.WriteElementString(ElementNames.MailServer, InternalSMTPMailServer);
			writer.WriteElementString(ElementNames.Port, InternalSMTPPort.ToString());
			writer.WriteEndElement();
		}

		void WriteDBFileNameList(XmlTextWriter writer)
		{
			if (LogAndDataFiles.Count > 0)
			{
				writer.WriteStartElement(ElementNames.DBFileNameList);
				foreach (string databaseFileName in LogAndDataFiles)
				{
					writer.WriteElementString(ElementNames.DBFileName, databaseFileName);
				}
				writer.WriteEndElement();
			}
		}

		void WriteLicenceKeys(XmlTextWriter writer)
		{
			if (LicenceKeys.Count > 0)
			{
				writer.WriteStartElement(ElementNames.LicenceKeyList);
				foreach (string companyLicence in LicenceKeys)
				{
					writer.WriteElementString(ElementNames.LicenceKey, companyLicence);
				}
				writer.WriteEndElement();
			}
		}

		void WriteDocEngineStats(XmlTextWriter writer)
		{
			writer.WriteStartElement(ElementNames.DocEngineStats);
			writer.WriteElementString(ElementNames.DocEngineActivePrintersCount, ActivePrintersCount.ToString());
			writer.WriteEndElement();
		}

		void WriteSqlServerVersionDetails(XmlTextWriter writer)
		{
			DbConnection connection = Db.Connection;
			writer.WriteStartElement(ElementNames.SqlServerVersionDetails);
			writer.WriteElementString(ElementNames.SqlServerCpuArchitecture, "X64");
			writer.WriteElementString(ElementNames.SqlServerEdition, connection.ServerEdition.ToString());
			writer.WriteElementString(ElementNames.SqlServerVersion, connection.ServerVersionNumber.SqlServerGeneration);
			writer.WriteElementString(ElementNames.SqlServerFullVersionText, connection.ServerFullVersionText);
			writer.WriteEndElement();
		}

		void WriteAdditionalDatabaseSystemInfoList(XmlTextWriter writer)
		{
			if (AdditionalDatabaseSystemInfoList.Count > 0)
			{
				writer.WriteStartElement(ElementNames.AdditionalDatabaseSystemInfoList);
				foreach (string info in AdditionalDatabaseSystemInfoList)
				{
					writer.WriteElementString(ElementNames.AdditionalDatabaseSystemInfo, info);
				}
				writer.WriteEndElement();
			}
		}

		void WriteLicenceUsage(XmlTextWriter writer)
		{
			if (LicenceUsage != null)
			{
				writer.WriteElementString(ElementNames.LicenceUsage, LicenceUsage);
			}
		}

		#endregion

#endif
		#endregion

		#region Element Names

		static class ElementNames
		{
			public const string VersionReport = "VersionReport";
			public const string OrganisationPK = "OrganisationPK";
			public const string DatabaseNumber = "DatabaseNumber";
			public const string EnterpriseCode = "EnterpriseCode";
			public const string CompanyCode = "CompanyCode";
			public const string PhysicalServerID = "PhysicalServerID";
			public const string DBServerName = "DBServerName";
			public const string DBName = "DBName";
			public const string CurrentVersion = "CurrentVersion";
			public const string CurrentDate = "CurrentDate";
			public const string CurrentRelease = "CurrentRelease";
			public const string PreferredUpgradeMethod = "PreferredUpgradeMethod";
			public const string DBServerSecurityMode = "DBServerSecurityMode";
			public const string PublicEmailAddressForUpdate = "PublicEmailAddressForUpdate";
			public const string SQLServerName = "SQLServerName";
			public const string SQLServerInstanceName = "SQLServerInstanceName";
			public const string ServerSID = "ServerSID";
			public const string POP3 = "POP3";
			public const string SMTP = "SMTP";
			public const string UserName = "UserName";
			public const string MailServer = "MailServer";
			public const string MailboxEmailAddress = "EmailAddress";
			public const string Port = "Port";
			public const string DBFileNameList = "DBFileNameList";
			public const string DBFileName = "DBFileName";
			public const string LicenceKeyList = "LicenceKeyList";
			public const string LicenceKey = "LicenceKey";
			public const string DatabaseBackupPath = "DatabaseBackupPath";
			public const string EncryptedSystemExpirationKey = "SystemHealthData";
			public const string EncryptedRegistrationKey = "RegistrationData";
			public const string DocEngineStats = "DocEngine";
			public const string DocEngineActivePrintersCount = "ActivePrintersCount";
			public const string SqlServerVersionDetails = "SqlServerVersionDetails";
			public const string SqlServerCpuArchitecture = "SqlServerCpuArchitecture";
			public const string SqlServerEdition = "SqlServerEdition";
			public const string SqlServerFullVersionText = "SqlServerFullVersionText";
			public const string SqlServerVersion = "SqlServerVersion";
			public const string AdditionalDatabaseSystemInfoList = "AdditionalDatabaseSystemInfoList";
			public const string AdditionalDatabaseSystemInfo = "SystemInfo";
			public const string LicenceUsage = "LicenceUsage";
			public const string CompanyList = "CompanyList";
			public const string Company = "Company";
			public const string BranchList = "BranchList";
			public const string Branch = "Branch";
			public const string StaffList = "StaffList";
			public const string Staff = "Staff";
			public const string IsFullStaffList = "IsFullStaffList";
			public const string OutboundEAdaptorUrl = "OutboundEAdaptorUrl";
			public const string NextRunTimeUtcUPG = "NextRunTimeUtcUPG";
			public const string NextRunTimeUtcMUG = "NextRunTimeUtcMUG";
			public const string ScheduleStateUPG = "ScheduleStateUPG";
			public const string ScheduleStateMUG = "ScheduleStateMUG";
			public const string TokenAuthenticationEnabled = "TokenAuthenticationEnabled";
		}

		#endregion
	}
}

