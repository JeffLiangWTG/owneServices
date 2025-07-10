using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business.UserAccountReport;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public static class CustomerUserAccountSyncHelper
	{
		public static void LinkUserAccountsToClientStaff(ZGuid databasePK, List<StaffReport> staffReports)
		{
			var mutex = CreateAndLockImportMutex(databasePK, maxTries: 3, millisecondsBetweenTries: 5000);
			if (mutex == null)
			{
				return;
			}

			using (mutex)
			{
				foreach (var staffsByRange in staffReports.Select((x, i) => new { Index = i % ImportBatchSize, Staff = x })
							.GroupBy(x => x.Index))
				{
					for (var retryCounter = 1; retryCounter <= MaxRetries; retryCounter++)
					{
						try
						{
							LinkUserAccountsToClientStaffCore(databasePK, staffsByRange.Select(x => x.Staff));
							break;
						}
						catch (ZSaveException ex)
						{
							if (retryCounter == MaxRetries)
							{
								ErrorReporter.ReportOnce("CustomerUserAccountSyncHelper.ImportUserAccounts", ex);
							}
						}
					}
				}
			}
		}

		static void LinkUserAccountsToClientStaffCore(ZGuid databasePK, IEnumerable<StaffReport> reports)
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var userAccountQuery = new ZQuery(EdiCustomerUserAccountSchema.EUA_LD, databasePK)
					.AddToFilter(EdiCustomerUserAccountSchema.EUA_UserID, reports.Select(x => x.Code))
					.AddToFilter(EdiCustomerUserAccountSchema.EUA_LS, null);
			var userAccounts = factory.Load<EdiCustomerUserAccount>(userAccountQuery).ToDictionary(x => x.EUA_UserID);

			if (userAccounts.Count > 0)
			{
				var clientStaffQuery = new ZQuery(ClientStaffSchema.LS_LD, databasePK)
					.AddToFilter(ClientStaffSchema.LS_Code, userAccounts.Select(x => x.Value.EUA_UserID));
				foreach (var clientStaff in factory.Load<ClientStaff>(clientStaffQuery))
				{
					userAccounts[clientStaff.LS_Code].EUA_LS = clientStaff.PK;
				}

				factory.Save();
			}
		}

		public static void ImportDeactivationOfUserAccounts(ZGuid databasePK, List<StaffReport> staffReports)
		{
			var orgPK = new BusinessObjectFactory().Load<LicenceDatabase>(databasePK)?.LD_OH_WebAccessOrg ?? ZGuid.Empty;
			if (orgPK.IsEmpty)
			{
				return;
			}

			var mutex = CreateAndLockImportMutex(orgPK, maxTries: 3, millisecondsBetweenTries: 5000);
			if (mutex == null)
			{
				return;
			}

			using (mutex)
			{
				foreach (var staffsByRange in staffReports.Where(x => !x.IsActive).Select((x, i) => new { Index = i % ImportBatchSize, Staff = x })
					.GroupBy(x => x.Index))
				{
					for (var retryCounter = 1; retryCounter <= MaxRetries; retryCounter++)
					{
						try
						{
							DeactivateExistingImportedUserAccounts(databasePK, staffsByRange.Select(x => x.Staff));
							break;
						}
						catch (ZSaveException ex)
						{
							if (retryCounter == MaxRetries)
							{
								ErrorReporter.ReportOnce("CustomerUserAccountSyncHelper.ImportUserAccounts", ex);
							}
						}
					}
				}
			}
		}

		static void DeactivateExistingImportedUserAccounts(ZGuid databasePK, IEnumerable<StaffReport> reports)
		{
			var utcNow = ZDateTime.UtcNow;
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var userAccountQuery = new ZQuery(EdiCustomerUserAccountSchema.EUA_LD, databasePK)
				.AddToFilter(EdiCustomerUserAccountSchema.EUA_UserID, reports.Select(x => x.Code));
			var userAccounts = factory.Load<EdiCustomerUserAccount>(userAccountQuery).ToDictionary(x => x.EUA_UserID);

			foreach (var report in reports)
			{
				if (userAccounts.ContainsKey(report.Code))
				{
					var userAccount = userAccounts[report.Code];
					if (userAccount.EUA_IsActive)
					{
						userAccount.EUA_IsActive = false;
						userAccount.EUA_SystemVerifiedDateUtc = utcNow;
					}
				}
			}

			if (userAccounts.Any(x => x.Value.HasChanges))
			{
				factory.Save();
			}
		}

		#region Mutex

		static readonly MutexID CreateCustomerUserAccountMutexID = new MutexID("CustomerUserAccountSyncHelper", "Ensure User Account only be created once.");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		static ZGlobalMutex CreateAndLockImportMutex(ZGuid orgPk, int maxTries, int millisecondsBetweenTries)
		{
			ZGlobalMutex result = null;
			int failureCount = 0;
			do
			{
				var mutex = new ZGlobalMutex(CreateCustomerUserAccountMutexID, orgPk.ToString());
				try
				{
					if (mutex.Lock())
					{
						result = mutex;
						mutex = null;
						break;
					}
				}
				catch (SqlLockLostException)
				{
				}
				finally
				{
					if (mutex != null)
					{
						try
						{
							((IDisposable)mutex).Dispose();
						}
						catch (Exception ex) when (!ex.IsCriticalException()) { }
					}
				}

				failureCount++;
				if (failureCount < maxTries)
				{
					Thread.Sleep(millisecondsBetweenTries);
				}
			}
			while (failureCount < maxTries);

			return result;
		}

		#endregion Mutex

		const int ImportBatchSize = 100;
		const int MaxRetries = 3;
	}
}
