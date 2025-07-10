using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UserAccountReport;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.UserAccountReporting.BatchProcessor
{
	class UserAccountReportProcessor : IEmailAttachmentProcessor
	{
		public UserAccountReportProcessor(ILogger serviceLogger)
		{
			ServiceLogger = serviceLogger;
		}

		public ILogger ServiceLogger { get; private set; }

		protected BusinessObjectFactory Factory
		{
			get
			{
				return factory ?? (factory = new BusinessObjectFactory());
			}
		}
		BusinessObjectFactory factory;

		public void Process(string xmlData)
		{
			var userAccountReport = new UserAccountReport(xmlData);
			var database = GetLicenceDatabase(userAccountReport);
			if (database == null || !database.LD_IsActive)
			{
				ServiceLogger.Log(LogType.Warning, "The processing database is null or inactive.");
				return;
			}
			UpdateClientBranchList(database, userAccountReport);
			UpdateClientStaffList(database, userAccountReport);
			ServiceLogger.Log(LogType.Information, $"Processing user account report from database {database.LD_DatabaseNumber} complete");
		}

		void UpdateClientBranchList(LicenceDatabase database, UserAccountReport report)
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

		void UpdateClientStaffList(LicenceDatabase database, UserAccountReport report)
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
						ServiceLogger.Log(LogType.Error, "UserAccountReportProcessor.UpdateClientStaffList", ex);
					}
					var message = "UserAccountReportProcessor.UpdateClientStaffList() on database " + database.LD_DatabaseNumber;
					ErrorReporter.ReportOnce("ClientStaff.UpdateClientStaffList", message, ex);

					if (ex.IsCriticalException())
					{
						throw;
					}
				}
			}
		}

		LicenceDatabase GetLicenceDatabase(UserAccountReport report)
		{
			return Factory.LoadTop1<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, report.DatabaseNumber));
		}
	}
}
