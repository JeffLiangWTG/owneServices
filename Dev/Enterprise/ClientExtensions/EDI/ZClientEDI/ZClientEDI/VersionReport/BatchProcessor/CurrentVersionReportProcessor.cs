using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Licensing;
using CargoWise.Licensing.Registration;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.BatchProcessor;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Integration;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.VersionReport;
using Enterprise.ProductRegistration.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.VersionReporting.BatchProcessor
{
	class CurrentVersionReportProcessor : VersionReportProcessor
	{
		public CurrentVersionReportProcessor(ILogger serviceLogger, bool receivedViaEhub)
			: base(serviceLogger)
		{
			this.ReceivedViaEhub = receivedViaEhub;
		}

		internal bool ReceivedViaEhub { get; private set; }

		protected override void ProcessVersionReport(EDIVersionReport report)
		{
			LogInfo(report, CreateLogMesssage(report));
			LicenceDatabase database = GetLicenceDatabase(report);

			if (Is201504Report(report))
			{
				Handle201504Report(database, report);
			}
			else
			{
				HandleLegacyReport(database, report);
				HandleLicenceUsage(report.LicenceUsage);
			}
		}

		bool Is201504Report(EDIVersionReport report)
		{
			return report.DatabaseNumberSpecified;
		}

		void Handle201504Report(LicenceDatabase database, EDIVersionReport report)
		{
			if (database == null)
			{
				LogInfo(report, "no matching database found - ignoring");
				return;
			}

			if (report.EncryptedRegistrationKey.IsEmpty)
			{
				// unregistered system - so service tasks won't run - we won't be sent a report - should never happen
				LogInfo(report, "no registration key - ignoring");
				return;
			}

			var regKey = Decode(report.EncryptedRegistrationKey);
			var passwordHash = CargoWise.eHub.Common.SHA512Encryptor.Encrypt(database.LD_DatabaseNumber.ToString() + regKey.Password);
			if (passwordHash != database.LD_Password)
			{
				LogInfo(report, "password incorrect - ignoring");
				return;
			}

			UpdateExeVersion(database, report);
			UpdateLocalDataFromReportCommon(database, report);
			UpdateLicenceExpiry(database, regKey.ExpiryDate);
			UpdateLastReport(database, report, ZDateTime.Now);
			UpdateClientCompanyList(database, report.CurrentDate, report);
			UpdateClientBranchList(database, report);
			UpdateClientStaffList(database, report);
			UpdateOutboundEAdaptorUrl(database, report);
			UpdateReportedDBNames(database, report);
			UpdateUpgradeScheduleInfo(database, report);
			UpdateTokenAuthenticationEnabled(database, report);
			database.Factory.Save();
			LogInfo(report, "processed OK");

			// Only handle usage if the registration is correct.
			HandleLicenceUsage(report.LicenceUsage);
		}

		void HandleLegacyReport(LicenceDatabase database, EDIVersionReport report)
		{
			if (database != null)
			{
				if (UpdateExeVersion(database, report) || database.ShouldUpdateFromHeartbeat)
				{
					ISystemRegistrationKey key = !report.EncryptedSystemExpirationKey.IsEmpty
						? SystemRegistrationKey.NewFromEncryptedXmlKey(report.EncryptedSystemExpirationKey)
						: null;

					ZDateTime reportTime = ZDateTime.Now;
					ZDateTime lastHeartbeat = reportTime;

					UpdateLocalDataFromLegacyReport(database, report, key);

					string reasonForNotUpdating;
					if (ShouldUpdateExpiryDateOnClient(database, report, key, out reasonForNotUpdating))
					{
						database.SendUpdateForSystemExpiry(report.CompanyCode);
						lastHeartbeat = ZDateTime.Empty;
						database.RequestVersionReportAsConfirmationFromLegacySystem();

						LogInfo(report, "sent new system expiry via " + (database.VersionCanReceiveSystemMessageRDU == Enterprise.Customs.Business.TriState.True ? "eHub" : "email"));
					}
					else
					{
						LogInfo(report, "no expiry update - " + reasonForNotUpdating);
					}

					UpdateLastReport(database, report, lastHeartbeat);
					ProcessLicenceKeys(database, report, reportTime);
					UpdateOutboundEAdaptorUrl(database, report);
					UpdateUpgradeScheduleInfo(database, report);
					database.Factory.Save();
				}
				else
				{
					string msg = string.Format("not processed - too soon since last report ({0})",
						database.LD_LastHeartbeat.ToDateTime().ToString("dd-MMM-yyyy HH:mm:ss"));
					LogInfo(report, msg);
				}
			}
		}

		void UpdateLastReport(LicenceDatabase database, EDIVersionReport report, ZDateTime now)
		{
			// Prevent logging if nothing has changed except last dates (which always change).
			// This code must be after all other changes to the record.
			if (report.CurrentVersionSpecified)
			{
				database.LD_CurrentVersionLastReportUtc = report.CurrentDate;
			}
			database.LD_LastHeartbeat = now;
		}

		protected virtual void HandleLicenceUsage(string licenceUsage)
		{
			if (!string.IsNullOrEmpty(licenceUsage))
			{
				var usageProcessor = new LicenceUsageProcessor(ServiceLogger);
				usageProcessor.Process(LicenceUsageProcessor.DecodeCompressedEncrypted(licenceUsage));
			}
		}

		void UpdateClientCompanyList(LicenceDatabase database, ZDateTime reportTimeUtc, EDIVersionReport report)
		{
			if (report.CompanyList.Count > 0)
			{
				try
				{
					ClientCompany.Sync(new BusinessObjectFactory() { RefreshEnabled = false }, database, reportTimeUtc, report.CompanyList);
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}

					ServiceLogger?.Log(LogType.Error, "ClientCompany.Sync", ex);
					ErrorReporter.ReportOnce("Unhandled Exception in ClientCompany.Sync", ex.Message, ex);
				}
			}
		}

		void UpdateClientBranchList(LicenceDatabase database, EDIVersionReport report)
		{
			if (database.LD_LicenceType == DatabaseTypes.Codes.Production && report.BranchList.Count > 0)
			{
				try
				{
					var factory = new BusinessObjectFactory() { RefreshEnabled = false };
					ClientBranch.Sync(factory, database, report.BranchList);
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}

					if (ServiceLogger != null)
					{
						ServiceLogger.Log(LogType.Error, "ClientBranch.Sync", ex);
					}
					var message = "ClientBranch.Sync() on database " + database.LD_DatabaseNumber;
					ErrorReporter.ReportOnce("ClientBranch.Sync", message, ex);
				}
			}
		}

		void UpdateClientStaffList(LicenceDatabase database, EDIVersionReport report)
		{
			if (report.StaffList.Count > 0)
			{
				try
				{
					if (database.LD_LicenceType == DatabaseTypes.Codes.Production)
					{
						ClientStaff.Sync(database.PK, report.StaffList);
						VersionReportContactImportHelper.ImportContacts(database.PK, report.StaffList, report.IsFullStaffList);
						CustomerUserAccountSyncHelper.LinkUserAccountsToClientStaff(database.PK, report.StaffList);
					}
					else
					{
						CustomerUserAccountSyncHelper.ImportDeactivationOfUserAccounts(database.PK, report.StaffList);
					}
				}
				catch (Exception ex)
				{
					if (ServiceLogger != null)
					{
						ServiceLogger.Log(LogType.Error, "CurrentVersionReportProcessor.UpdateClientStaffList", ex);
					}
					var message = "CurrentVersionReportProcessor.UpdateClientStaffList() on database " + database.LD_DatabaseNumber;
					ErrorReporter.ReportOnce("ClientStaff.UpdateClientStaffList", message, ex);

					if (ex.IsCriticalException())
					{
						throw;
					}
				}
			}
		}

		protected override string GetReportId()
		{
			return "CurrentVersionReport";
		}

		RegistrationKey Decode(string encryptedRegKey)
		{
			string xml = TwoWayEncoder.NewWithStandardInitialisationVector().Decrypt(encryptedRegKey);
			return RegistrationKeyUtility.DeserializeIfAuthentic(xml);
		}

		void LogInfo(EDIVersionReport report, string msg)
		{
			if (ServiceLogger != null)
			{
				string text;
				if (report.DatabaseNumberSpecified)
				{
					text = string.Format("Report for {0}-{1} [DB# {2}] {3}",
						report.EnterpriseCode, report.PhysicalServerID, report.DatabaseNumber,
						msg);
				}
				else
				{
					text = string.Format("Report for {0}-{1}-{2} {3}",
					report.EnterpriseCode, report.CompanyCode, report.PhysicalServerID,
					msg);
				}
				ServiceLogger.Log(LogType.Information, text);
			}
		}

		string CreateLogMesssage(EDIVersionReport report)
		{
			string result = string.Format("for SQL Server [{0}] database [{1}] has been received",
				report.SQLServerName + ((!string.IsNullOrEmpty(report.SQLServerInstanceName)) ? ((string)report.SQLServerInstanceName) : ""), report.DBName);

			return result;
		}

		#region Should Update Expiry Date On Client

		protected bool ShouldUpdateExpiryDateOnClient(LicenceDatabase database, EDIVersionReport report, ISystemRegistrationKey key, out string reasonForNotUpdating)
		{
			bool result = false;
			reasonForNotUpdating = string.Empty;

			if (!IsLicenceNeedingExpiryUpdate(database, key) && LicenceHasCurrentBillingTimeZoneInfo(key))
			{
				reasonForNotUpdating = "Licence expiry doesn't need updating (" + key.SystemExpiryDate.ToShortDateString() + ") and has current billing timezone info";
			}
			else if (!database.LD_IsActive)
			{
				reasonForNotUpdating = "Database inactive";
			}
			else if (database.LD_LicenceType.IsEmpty)
			{
				reasonForNotUpdating = "Database type is blank";
			}
			else if (database.LD_HostServerName.IsEmpty)
			{
				reasonForNotUpdating = "Database HostServerName is blank";
			}
			else if (database.LD_HostDBName.IsEmpty)
			{
				reasonForNotUpdating = "Database HostDBName is blank";
			}
			else if (database.LD_HostServerSID.IsEmpty)
			{
				reasonForNotUpdating = "Database Server SID is blank";
			}
			else if (database.LD_HostServerSID != report.ServerSID)
			{
				reasonForNotUpdating = "Database Server SID does not match report SID";
			}
			else if (database.VersionCanReceiveSystemMessageRDU == Customs.Business.TriState.True && !ReceivedViaEhub)
			{
				reasonForNotUpdating = "EHub is not working - Database supports eHub heartbeats, but report received via email";
			}
			else if (!IsReportedDbSecurityModeAcceptable(database, report))
			{
				reasonForNotUpdating = "Reported DB security invalid";
			}
			else
			{
				result = true;
			}

			return result;
		}

		/// <summary>
		/// Licence not yet initialised OR expires within 30 days (or expires AFTER 60 days = sanity check)
		/// </summary>
		bool IsLicenceNeedingExpiryUpdate(LicenceDatabase database, ISystemRegistrationKey key)
		{
			if (key == null)
			{
				return true;
			}

			if (database.LD_ManualLicenceExpiry.IsEmpty)
			{
				return database.LD_LicenceExpiry == ZDateTime.Empty
					|| (ZDateTime)database.LD_LicenceExpiryInfo.OriginalValue == ZDateTime.Empty
					|| key.SystemExpiryDate.Date <= ZDateTime.Today.AddDays(Licences.DefaultNumberOfDaysBeforeUpdateSystemKey)
					|| key.SystemExpiryDate.Date > ZDateTime.Today.AddDays(Licences.DefaultLicenceGracePeriodInDays);
			}
			else
			{
				return database.LD_ManualLicenceExpiry != key.SystemExpiryDate;
			}
		}

		bool LicenceHasCurrentBillingTimeZoneInfo(ISystemRegistrationKey key)
		{
			var info = new BillingTimeZoneInfo(ZDateTime.UtcNow.ToDateTime());
			return key.CurrentBillingTimeZoneUtcOffset == info.CurrentBillingTimeZoneUtcOffset
				&& key.NextBillingTimeZoneUtcOffset == info.NextBillingTimeZoneUtcOffset
				&& key.NextUtcOffsetEffectiveTimeUtc == info.NextUtcOffsetEffectiveTimeUtc;
		}

		/// <summary>
		/// Reported DB Security Mode matches our records
		/// or Reported DB Security Mode = LOCKED
		/// or Client is allowed to be in Open Mode
		/// or Client is running ediEnterprise (Version LT 2.0.0.0)
		/// or Client is HOSTED by us - EDIDataRegistry.Instance.DatabaseHostedLocations.Value.GetBoolFromCode(LD_HostedLocation)
		/// </summary>
		bool IsReportedDbSecurityModeAcceptable(LicenceDatabase database, EDIVersionReport report)
		{
			return report.DBServerSecurityMode == database.LD_DBServerSecurityMode
				|| report.DBServerSecurityMode == DatabaseSecurityModePairList.Codes.Locked
				|| database.LD_DBServerSecurityMode == DatabaseSecurityModePairList.Codes.OpenMode
				|| new VersionNumber(report.CurrentVersion) < FirstVersionToEnforceDbSecurity
				|| !(database.LD_HostedLocation.IsEmpty || database.LD_HostedLocation == Enterprise.Core.Constants.LicenceConstants.NotHostedWithCargoWise)
			;
		}

		protected virtual VersionNumber FirstVersionToEnforceDbSecurity
		{
			get { return firstVersionToEnforceDbSecurity; }
		}
		readonly VersionNumber firstVersionToEnforceDbSecurity = new VersionNumber(2, 0, 0, 0);

		#endregion

		#region Update Database Information

		void LogAndUpdate(string changedFieldTitle, ZPropertyInfo propertyInfo, IZType reportedValue)
		{
			IZType existingValue = propertyInfo.Value;
			if (reportedValue is ZString && ((ZString)reportedValue).Length > propertyInfo.MaxLength)
			{
				reportedValue = ((ZString)reportedValue).Substring(0, propertyInfo.MaxLength);
			}
			if (!existingValue.Equals(reportedValue))
			{
				propertyInfo.Value = reportedValue;
			}
		}

		void UpdateLocalDataFromLegacyReport(LicenceDatabase database, EDIVersionReport report, ISystemRegistrationKey key)
		{
			if (database.LD_HostServerSID.IsEmpty // Updating for the first time.
				|| (// SID has changed, but DB server, instance and name the same
					!database.LD_HostServerSID.IsEmpty
					&& database.LD_HostServerSID != report.ServerSID
					&& database.LD_HostServerName == report.DBServerName
					&& database.LD_HostDBInstance == report.SQLServerInstanceName
					&& database.LD_HostDBName == report.DBName
					)

				|| database.LD_HostedLocation != Enterprise.Core.Constants.LicenceConstants.NotHostedWithCargoWise)
			{
				UpdateHostServerSID(database, report);
				UpdateDatabaseServerName(database, report);
				UpdateDatabaseInstanceName(database, report);
				UpdateDatabaseName(database, report);
			}

			UpdateLocalDataFromReportCommon(database, report);
			if (key != null)
			{
				UpdateLicenceExpiry(database, key.SystemExpiryDate);
			}
		}

		void UpdateLocalDataFromReportCommon(LicenceDatabase database, EDIVersionReport report)
		{
			UpdatePOP3Details(database, report);
			UpdateSMTPDetails(database, report);
			UpdateDatabaseLocationsAndPaths(database, report);
			UpdateIntegratedPrintingStatistics(database, report);
			UpdateSqlServerVersionDetails(database, report);
			UpdateLicenceDatabaseWithAdditionalSystemInfo(database, report);
		}

		#region Initial Configuration

		protected void UpdateLicenceExpiry(LicenceDatabase database, DateTime expiryDate)
		{
			ZDateTime reportDate = new ZDateTime(expiryDate);
			if (reportDate.IsValidSmallDateTime)
			{
				ZDateTime expiryDateAsSmallDateTime = new ZDateTime(expiryDate.Year, expiryDate.Month, expiryDate.Day, expiryDate.Hour, expiryDate.Minute, 0, DateTimeKind.Local);
				LogAndUpdate("Licence Expiry Date", database.LD_LicenceExpiryInfo, expiryDateAsSmallDateTime);
			}
		}

		void UpdateDatabaseServerName(LicenceDatabase database, EDIVersionReport report)
		{
			if (database.LD_HostServerName != report.DBServerName)
			{
				database.LD_HostServerName = report.DBServerName;
			}
		}

		void UpdateHostServerSID(LicenceDatabase database, EDIVersionReport report)
		{
			if (report.ServerSIDSpecified && database.LD_HostServerSID != report.ServerSID)
			{
				database.LD_HostServerSID = report.ServerSID;
			}
		}

		void UpdateDatabaseName(LicenceDatabase database, EDIVersionReport report)
		{
			if (report.DBNameSpecified && database.LD_HostDBName != report.DBName)
			{
				database.LD_HostDBName = report.DBName;
			}
		}

		void UpdateDatabaseInstanceName(LicenceDatabase database, EDIVersionReport report)
		{
			if (report.SQLServerInstanceNameSpecified && database.LD_HostDBInstance != report.SQLServerInstanceName)
			{
				database.LD_HostDBInstance = report.SQLServerInstanceName;
			}
		}

		#endregion

		#region Common Updates

		void UpdateIntegratedPrintingStatistics(LicenceDatabase database, EDIVersionReport report)
		{
			if (report.ActivePrintersCountSpecified)
			{
				LogAndUpdate("Number of Active Print Queues", database.LD_NoOfActivePrintQueuesInfo, (ZShort)report.ActivePrintersCount);
			}
		}

		bool UpdateOutboundEAdaptorUrl(LicenceDatabase database, EDIVersionReport report)
		{
			bool modified = false;
			if (report.OutboundEAdaptorUrlSpecified)
			{
				var currentUrl = database.LD_OutboundEAdaptorUrl;
				var newUrl = report.OutboundEAdaptorUrl.SubstringSafe(0, LicenceDatabase.Schema.LD_OutboundEAdaptorUrlMaxLength);
				if (string.IsNullOrEmpty(currentUrl) || currentUrl != newUrl)
				{
					database.LD_OutboundEAdaptorUrl = newUrl;
					modified = true;
				}
			}

			return modified;
		}

		bool UpdateExeVersion(LicenceDatabase database, EDIVersionReport report)
		{
			bool modified = false;
			if (report.CurrentVersionSpecified)
			{
				var currentBuild = database.CurrentVersion;
				if (currentBuild == null || currentBuild.ExeVersion != report.CurrentVersion)
				{
					ReleaseBuild build = GetReleaseBuild(report.CurrentVersion);
					if ((build != null && (currentBuild == null || build.PK != currentBuild.PK))
						|| (build == null && currentBuild != null))
					{
						modified = true;
						database.LD_CurrentVersionFirstReportUtc = report.CurrentDate;
						database.LD_HL_CurrentRunningVersion = (build != null) ? build.PK : ZGuid.Empty;
						if (NeedUpdateSentVersion(build, database.SentVersion))
						{
							database.LD_HL_CurrentSentVersion = (build != null) ? build.PK : ZGuid.Empty;
						}
					}
				}
			}
			return modified;
		}

		bool NeedUpdateSentVersion(ReleaseBuild currentVersion,ReleaseBuild sentVersion)
		{
			return currentVersion != null && (sentVersion == null || currentVersion.VersionNumber > sentVersion.VersionNumber);
		}

		void UpdatePOP3Details(LicenceDatabase database, EDIVersionReport report)
		{
			if (!report.InternalPOP3EmailAddress.IsEmpty)
			{
				LogAndUpdate("POP3 Email Address", database.LD_PublicEmailAddressForUpdateInfo, report.InternalPOP3EmailAddress);
			}

			if (report.InternalPOP3MailServerSpecified)
			{
				LogAndUpdate("POP3 Mail Server", database.LD_InternalPop3EmailAddressInfo, report.InternalPOP3MailServer); // The LD_InternalPop3EmailAddress is actually the mail server name - this DB field is badly named.
				LogAndUpdate("POP3 User Name", database.LD_InternalPop3UserNameInfo, report.InternalPOP3UserName);
				LogAndUpdate("POP3 Port", database.LD_InternalPop3PortInfo, report.InternalMailServerPort);
			}
		}

		void UpdateSMTPDetails(LicenceDatabase database, EDIVersionReport report)
		{
			if (report.InternalSMTPMailServerSpecified)
			{
				LogAndUpdate("SMTP Mail Server", database.LD_InternalSmtpEmailAddressInfo, report.InternalSMTPMailServer);// The LD_InternalSMTPEmailAddress is actually the mail server name - this DB field is badly named.
				LogAndUpdate("SMTP Port", database.LD_InternalSmtpPortInfo, report.InternalSMTPPort);
			}
		}

		void UpdateDatabaseLocationsAndPaths(LicenceDatabase database, EDIVersionReport report)
		{
			if (report.LogAndDataFiles.Count > 0)
			{
				StringBuilder builder = new StringBuilder();
				builder.AppendLine("Database Backup Path: ");
				builder.AppendLine(report.DatabaseBackupPath);
				builder.AppendLine();
				builder.AppendLine("Database File Locations: ");
				foreach (string file in report.LogAndDataFiles)
				{
					builder.Append("-  ");
					builder.AppendLine(file);
				}
				database.LD_DatabaseFilePathDetail = builder.ToString();
			}
		}

		void UpdateSqlServerVersionDetails(LicenceDatabase database, EDIVersionReport report)
		{
			if (!report.SqlServerFullVersionText.IsEmpty)
			{
				LogAndUpdate("SQL Server Version", database.LD_SQLVersionInfo, (ZString)report.SqlServerVersion);
				LogAndUpdate("SQL Server Edition", database.LD_SQLEditionInfo, SqlServerVersionDetailsConverter.ToCode(report.SqlServerEdition));
				LogAndUpdate("SQL Server Full Version Text", database.LD_SQLVerStringInfo, report.SqlServerFullVersionText);
			}
		}

		void UpdateReportedDBNames(LicenceDatabase database, EDIVersionReport report)
		{
			database.LD_ReportedHostDBName = report.DBName;
			database.LD_ReportedHostDBInstance = report.SQLServerInstanceName;
			database.LD_ReportedHostServerName = report.DBServerName;
		}

		void UpdateUpgradeScheduleInfo(LicenceDatabase database, EDIVersionReport report)
		{
			if (report.NextRunTimeUtcUPGSpecified)
			{
				database.LD_NextRunTimeUtcUPG = report.NextRunTimeUtcUPG;
			}
			if (report.NextRunTimeUtcMUGSpecified)
			{
				database.LD_NextRunTimeUtcMUG = report.NextRunTimeUtcMUG;
			}
			if (report.ScheduleStateUPGSpecified)
			{
				database.LD_ScheduleStateUPG = report.ScheduleStateUPG;
			}
			if (report.ScheduleStateMUGSpecified)
			{
				database.LD_ScheduleStateMUG = report.ScheduleStateMUG;
			}
		}

		void UpdateTokenAuthenticationEnabled(LicenceDatabase database, EDIVersionReport report)
		{
			if (report.TokenAuthenticationEnabledSpecified)
			{
				database.LD_TokenAuthenticationEnabled = report.TokenAuthenticationEnabled;
				database.LicEnterprise.LE_TokenAuthenticationEnabled = database.LicEnterprise.Databases
					.Cast<LicenceDatabase>().Any(x => x.LD_TokenAuthenticationEnabled);
			}
		}

		#endregion

		#region Additional Database System Info

		void UpdateLicenceDatabaseWithAdditionalSystemInfo(LicenceDatabase database, EDIVersionReport report)
		{
			var list = GetSystemInfoList(report);
			if (list.Count > 0)
			{
				database.LD_OSName = list.GetDescriptionFromCode(VersionReport.OSNameKey);
				database.LD_OSVersion = list.GetDescriptionFromCode(VersionReport.OSVersionKey);
				database.LD_SystemManufacturer = list.GetDescriptionFromCode(VersionReport.SystemManufacturerKey);
				UpdateBIOSDate(database, list);
				UpdateTotalPhysicalMemory(database, list);
				UpdateNoOfProcessors(database, list);
				UpdateProcessorInfo(database, list);
				UpdateIsVirtualMachine(database, list, report);
				UpdateConfigData(database, list);
			}
		}

		void UpdateConfigData(LicenceDatabase database, CodeDescriptionPairList list)
		{
			if (list.ContainsCode(VersionReport.MAXDOP))
			{
				using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
				using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings() { ConformanceLevel = ConformanceLevel.Fragment, Indent = true }))
				{
					xmlWriter.WriteStartElement("DbConfig");
					foreach (VersionReport.DBCONFIG_INFO_FIELDS key in Enum.GetValues(typeof(VersionReport.DBCONFIG_INFO_FIELDS)))
					{
						if (key == VersionReport.DBCONFIG_INFO_FIELDS.TraceFlags)
						{
							xmlWriter.WriteStartElement(VersionReport.DbConfigFieldKeys[key]);
							foreach (var trace in list.GetDescriptionFromCode(VersionReport.DbConfigFieldKeys[key]).Split(';'))
							{
								xmlWriter.WriteElementString("TraceFlag", trace);
							}
							xmlWriter.WriteEndElement();
						}
						else
						{
							xmlWriter.WriteElementString(VersionReport.DbConfigFieldKeys[key], list.GetDescriptionFromCode(VersionReport.DbConfigFieldKeys[key]));
						}
					}
					xmlWriter.WriteEndElement();
					xmlWriter.Close();
					database.LD_DatabaseConfig = stringWriter.ToString();
				}
			}
		}

		void UpdateBIOSDate(LicenceDatabase database, CodeDescriptionPairList list)
		{
			if (list.ContainsCode(VersionReport.BIOSVersionKey))
			{
				int separatorIndex = list[VersionReport.BIOSVersionKey].Description.LastIndexOf(',');
				if (separatorIndex < 0)
				{
					separatorIndex = list[VersionReport.BIOSVersionKey].Description.LastIndexOf('-');
				}

				if (separatorIndex > -1)
				{
					string biosDateText = list[VersionReport.BIOSVersionKey].Description.Substring(separatorIndex + 1, list[VersionReport.BIOSVersionKey].Description.Length - separatorIndex - 1).Trim();

					ZDateTime parseResult = ZDateTime.Empty;
					database.LD_BIOSDate = ZDate.Empty;

					foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures)) //Date pattern for bios release date is irrelevant to system locale... need to go through all possible formats
					{
						ZDateTime.TryParseExact(biosDateText, out parseResult, culture.DateTimeFormat.ShortDatePattern);
						if (parseResult.IsValid)
						{
							database.LD_BIOSDate = parseResult.Date;
							break;
						}
					}
				}
			}
		}

		void UpdateTotalPhysicalMemory(LicenceDatabase database, CodeDescriptionPairList list)
		{
			if (list.ContainsCode(VersionReport.TotalPhysicalMemoryKey))
			{
				ZInt totalPhysicalMemory = 0;
				ZInt.TryParse(list[VersionReport.TotalPhysicalMemoryKey].Description.Replace("MB", "").Replace(",", "").Trim(), out totalPhysicalMemory);
				if (totalPhysicalMemory > 0)
				{
					database.LD_TotalPhysicalMemoryMB = totalPhysicalMemory;
				}
			}
		}

		void UpdateNoOfProcessors(LicenceDatabase database, CodeDescriptionPairList list)
		{
			if (list.ContainsCode(VersionReport.ProcessorsKey) && !string.IsNullOrEmpty(list[VersionReport.ProcessorsKey].Description))
			{
				string noOfProcessorsText = list[VersionReport.ProcessorsKey].Description.Split(' ')[0].Trim();
				ZInt noOfProcessors = 0;
				ZInt.TryParse(noOfProcessorsText, out noOfProcessors);
				if (noOfProcessors > 0)
				{
					database.LD_NoOfProcessorCores = noOfProcessors;
				}
			}
		}

		void UpdateProcessorInfo(LicenceDatabase database, CodeDescriptionPairList list)
		{
			if (list.ContainsCode(VersionReport.ProcessorTypeKey) && !string.IsNullOrEmpty(list[VersionReport.ProcessorTypeKey].Description))
			{
				string[] processorDetails = list[VersionReport.ProcessorTypeKey].Description.Split('~');
				database.LD_ProcessorType = processorDetails[0];

				if (processorDetails.Length > 1)
				{
					ZDecimal processorSpeed = 0;
					ZDecimal.TryParse(processorDetails[1].Replace("Mhz", "").Trim(), out processorSpeed);
					if (processorSpeed > database.LD_ProcessorSpeedMHz              //The number returned by systeminfo is current speed not maximum speed. So only update when returning larger number.
						&& processorSpeed > 0m && processorSpeed <= 999999.999m)    //The number should be in a reasonable range.
					{
						database.LD_ProcessorSpeedMHz = processorSpeed;
					}
				}
			}
		}

		void UpdateIsVirtualMachine(LicenceDatabase database, CodeDescriptionPairList list, EDIVersionReport report)
		{
			database.LD_VirtualMachineDetected = false;

			if (report.SqlServerFullVersionText.IndexOf(VersionReport.HypervisorKey) != -1)
			{
				database.LD_VirtualMachineDetected = true;
				return;
			}

			if (list.ContainsCode(VersionReport.SystemModelKey)
				&& EDIDataRegistry.Instance.VirtualMachineDetectionKeywords.Value.Count > 0)
			{
				IEnumerable<string> virtualMachineKeywords = EDIDataRegistry.Instance.VirtualMachineDetectionKeywords.Value.Cast<ICodeDescription>().Select(p => p.Code);
				foreach (var keyword in virtualMachineKeywords)
				{
					if (list[VersionReport.SystemModelKey].Description.IndexOf(keyword, 0, StringComparison.OrdinalIgnoreCase) != -1)
					{
						database.LD_VirtualMachineDetected = true;
						return;
					}
				}
			}
		}

		CodeDescriptionPairList GetSystemInfoList(EDIVersionReport report)
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			string parentLevelKey = string.Empty;
			foreach (string value in report.AdditionalDatabaseSystemInfoList)
			{
				int index = value.IndexOf(':');
				string code = index > -1 ? value.Substring(0, index).Trim() : value;
				string description = index > -1 ? value.Substring(index + 1, value.Length - index - 1).Trim() : string.Empty;

				if (!code.StartsWith("["))
				{
					parentLevelKey = code;
				}
				else
				{
					code = parentLevelKey + code;
				}

				list.AddPair(code, description);
			}
			return list;
		}

		#endregion

		#endregion

		#region System and Licence Discrepencies

		void ProcessLicenceKeys(LicenceDatabase database, EDIVersionReport report, ZDateTime reportTime)
		{
			var allLicences = new List<LegacyLicence>();
			foreach (string encryptedLicenceKey in report.LicenceKeys)
			{
				if (string.IsNullOrEmpty(encryptedLicenceKey))
				{
					continue;
				}

				var clientLicence = new LegacyLicence(encryptedLicenceKey);
				LicenceHeader ediLicHeader = database.LicHeadersForAllCompanies.FindById(clientLicence.Company.PhysicalServerID, clientLicence.Company.Code);
				allLicences.Add(clientLicence);

				if (ediLicHeader == null)
				{
				}
				else if (ediLicHeader.IsCargoWiseInstallation)
				{
					// We don't care about internal system discrepancies
					ediLicHeader.LA_LastLicenceCheckInSync = true;
					ediLicHeader.LA_LastLicenceSyncCheck = ZDateTime.Empty;
					ediLicHeader.DiscrepancyText = ZString.Empty;
					ediLicHeader.LA_LastDiscrepancyChange = ZDateTime.Empty;
					ediLicHeader.DisableAutoLog = true;
				}
				else
				{
					var ediLicence = new LegacyLicence(ediLicHeader.GenerateLicenceKey());

					LicenceComparer comparer = new LicenceComparer(clientLicence, ediLicence, false);
					ediLicHeader.LA_LastLicenceCheckInSync = comparer.AreLicencesTheSame;

					string tabbedDiscrepancyText = comparer.GetDescriptionOfDifferences();
					string untabbedDiscrepancyText = tabbedDiscrepancyText.Replace("\t", " ");
					if (ediLicHeader.DiscrepancyText != untabbedDiscrepancyText)
					{
						ediLicHeader.DiscrepancyText = untabbedDiscrepancyText;
						ediLicHeader.LA_LastDiscrepancyChange = reportTime;
					}

					// Prevent logging if nothing has changed except LA_LastLicenceSyncCheck (which always changes).
					// This code must be after all other changes to the record.
					if (!ediLicHeader.HasChanges)
					{
						ediLicHeader.DisableAutoLog = true;
					}
					ediLicHeader.LA_LastLicenceSyncCheck = reportTime;
				}
			}

			if (report.CompanyList.Count == 0 && allLicences.Count > 0)
			{
				// prefetch
				Factory.Load<RefCountry>(new ZQuery(RefCountrySchema.PK, allLicences.Select(x => x.Company.CountryPK)));

				foreach (var clientLicence in allLicences)
				{
					report.CompanyList.Add(AsCompanyReport(database, clientLicence.Company));
				}

				UpdateClientCompanyList(database, reportTime, report);
			}
		}

		EDIVersionReport.EdiCompanyReport AsCompanyReport(LicenceDatabase database, LicenceCompanyDetails co)
		{
			var result = new EDIVersionReport.EdiCompanyReport();

			result.OrgPKThatGeneratedThisLicence = co.OrgPKThatGeneratedThisLicence;

			result.Address1 = co.Address1;
			result.Address2 = co.Address2;
			result.BusinessRegNo = co.BusinessRegNo;
			result.BusinessRegNo2 = co.BusinessRegNo2;
			result.City = co.City;
			result.Code = co.Code;
			var country = database.Factory.Load<RefCountry>(co.CountryPK);
			if (country != null)
			{
				result.CountryCode = country.RN_Code;
			}
			result.Email = co.Email;
			result.IsGSTCashBasis = co.IsGSTCashBasis;
			result.IsGSTRegistered = co.IsGSTRegistered;
			result.IsReciprocal = co.IsReciprocal;
			result.IsWHTCashBasis = co.IsWHTCashBasis;
			result.IsWHTRegistered = co.IsWHTRegistered;
			result.Name = co.Name;
			result.Phone = co.Phone;
			result.PostCode = co.PostCode;
			result.State = co.State;
			result.WebAddress = co.WebAddress;
			result.CurrencyCode = co.LocalCurrencyCode;
			result.IsActive = true; // we don't know so assume it is

			return result;
		}

		#endregion
	}
}
