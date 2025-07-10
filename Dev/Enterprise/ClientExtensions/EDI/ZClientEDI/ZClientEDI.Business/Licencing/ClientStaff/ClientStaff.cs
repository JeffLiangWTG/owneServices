using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business.UserAccountReport;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class ClientStaff : AutoClientStaff
	{
		public ClientStaff(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("Database")]
		public override ZGuid LS_LD
		{
			get => base.LS_LD;
			set => base.LS_LD = value;
		}

		[EmailAddress]
		public override ZString LS_Email
		{
			get => base.LS_Email;
			set => base.LS_Email = value;
		}

		public LicenceDatabase Database => Factory.Load<LicenceDatabase>(LS_LD);

		#endregion

		#region Sync

		static readonly MutexID ImportClientStaffMutexID = new MutexID("ClientStaff", "Ensure same client staff only be imported once.");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public static bool Sync(ZGuid databasePK, List<StaffReport> staffReports, int maxTries = 3, int millisecondsBetweenTries = 5000)
		{
			if (staffReports.Count > 0)
			{
				var mutex = CreateAndLockImportMutex(databasePK, maxTries, millisecondsBetweenTries);
				if (mutex == null)
				{
					return false;
				}

				var factory = new BusinessObjectFactory() { RefreshEnabled = false };
				var databaseInSyncFactory = factory.Load<LicenceDatabase>(databasePK);
				using (mutex)
				{
					SyncInternal(factory, databaseInSyncFactory, staffReports);
				}
			}

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		static ZGlobalMutex CreateAndLockImportMutex(ZGuid databasePk, int maxTries, int millisecondsBetweenTries)
		{
			ZGlobalMutex result = null;
			int failureCount = 0;
			do
			{
				var mutex = new ZGlobalMutex(ImportClientStaffMutexID, databasePk.ToString());
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

		static void SyncInternal(BusinessObjectFactory factory, LicenceDatabase database, List<StaffReport> staffReports)
		{
			bool hasChanges = false;

			var query = new ZQuery(ClientStaffSchema.LS_LD, database.PK);
			query.AddToFilter(ClientStaffSchema.LS_Code, SQLComparisonOperator.NotEqual, string.Empty);
			var clientStaffByCode = factory.Load<ClientStaff>(query).ToDictionary(x => (string)x.LS_Code, StringComparer.OrdinalIgnoreCase);

			foreach (var report in staffReports)
			{
				if (!clientStaffByCode.TryGetValue(report.Code, out ClientStaff clientStaff))
				{
					clientStaff = factory.New<ClientStaff>();
					clientStaff.LS_LD = database.PK;
				}

				PopulateFromVersionReport(clientStaff, report);
				hasChanges |= clientStaff.HasChanges;
			}

			if (hasChanges)
			{
				factory.Save();
			}
		}

		static void PopulateFromVersionReport(ClientStaff staff, StaffReport report)
		{
			staff.LS_Code = report.Code;
			staff.LS_FullName = report.Name;
			staff.LS_Email = report.EmailAddress;
			staff.LS_IsActive = report.IsActive;
		}

		#endregion
	}
}

