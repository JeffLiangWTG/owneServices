using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business.UserAccountReport;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public static class VersionReportContactImportHelper
	{
		public static void ImportContacts(ZGuid databasePK, List<StaffReport> staffReports, bool isFullStaffList)
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var database = factory.Load<LicenceDatabase>(databasePK);
			var org = database.WebAccessOrg;
			if (org == null)
			{
				return;
			}

			var mutex = CreateAndLockImportMutex(org.PK, maxTries: 3, millisecondsBetweenTries: 5000);
			if (mutex == null)
			{
				return;
			}

			using (mutex)
			{
				var lookupFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var dbInLookupFactory = lookupFactory.Load<LicenceDatabase>(database.PK);
				var importer = new VersionReportContactImporter(lookupFactory, dbInLookupFactory);
				importer.ImportFromStaffReports(staffReports);
			}

			if (isFullStaffList && database.LD_StaffFirstReportUtc.IsEmpty)
			{
				AddContactToPersonMergeQueue(database);
				database.LD_StaffFirstReportUtc = ZDateTime.UtcNow;
				factory.Save();
			}
		}

		static void AddContactToPersonMergeQueue(LicenceDatabase database)
		{
			using (var command = Db.Connection.Command("EdiAddContactToPersonMergeQueue"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@DatabasePk", SqlDbType.UniqueIdentifier, database.PK.ToGuid());
				command.ExecuteNonQuery();
			}
		}

		#region Mutex

		static readonly MutexID CreateStaffRelatedContactMutexID = new MutexID("ContactImporter", "Ensure contact for client staff only be created once.");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		static ZGlobalMutex CreateAndLockImportMutex(ZGuid orgPk, int maxTries, int millisecondsBetweenTries)
		{
			ZGlobalMutex result = null;
			int failureCount = 0;
			do
			{
				var mutex = new ZGlobalMutex(CreateStaffRelatedContactMutexID, orgPk.ToString());
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

		#endregion
	}
}
