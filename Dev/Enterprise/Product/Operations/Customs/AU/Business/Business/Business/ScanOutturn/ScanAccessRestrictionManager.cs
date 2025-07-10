using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ScanAccessRestrictionManager : IDisposable
	{
		public string LockResult { get; private set; }

		ScanAccessRestrictionManager(IScanMasterBill scanMasterBill)
		{
			hostBO = Argument.NotNull(scanMasterBill, "ScanMasterBill");
		}

		public static ScanAccessRestrictionManager LockScan(IScanMasterBill scanMasterBill)
		{
			var manager = new ScanAccessRestrictionManager(scanMasterBill);
			manager.Lock();
			return manager;
		}

		readonly IScanMasterBill hostBO;
		Dictionary<string, ZGlobalMutex> scanMutexes;

		public void Lock()
		{
			GenerateScanMutexes();
			var errorMessageBuilder = new StringBuilder();

			foreach (var scanMutex in scanMutexes.Values)
			{
				if (!scanMutex.Lock())
				{
					var lockInfo = scanMutex.GetLockInfo();
					var recordID = lockInfo.RecordID;
					string lockObjectTips = recordID.Contains("HouseBill:") ? Res.GetString("6929b81b-e019-47e2-8592-5f41c90ee5a2", "shipment/cargo({0})", recordID) : Res.GetString("07cad14a-12d3-48fd-b5bf-045138ce41e3", "consol({0})", recordID);

					errorMessageBuilder.AppendLine("- " + Res.GetString("36f11f1a-d272-4436-9c7a-bd405874c681", "{0} is scanning {1} on {2} since {3}",
						lockInfo.GetUserWithLock(),
						lockObjectTips,
						lockInfo.HostName,
						EnvProxy.Instance.Time.GetLocalTimeFromUtc(lockInfo.LockStartTime.ToDateTime())));
				}
			}

			if (errorMessageBuilder.Length > 0)
			{
				string errorMessageHeader = Res.GetString("752f340c-bb6a-44d0-bfd2-2a05709c4815", "Below user(s) are in the process of outturn scanning the same record(s) you are attempting to outturn scan. Only one user may perform outturn scanning on the same record at the same time. Please try again later.");
				LockResult = errorMessageHeader + System.Environment.NewLine + errorMessageBuilder.ToString();
			}
		}

		void GenerateScanMutexes()
		{
			scanMutexes = new Dictionary<string, ZGlobalMutex>();

			foreach (var mutexKey in hostBO.GetMutexKeys())
			{
				if (!scanMutexes.ContainsKey(mutexKey))
				{
					scanMutexes.Add(mutexKey, new ZGlobalMutex(MutexIDs.ScanningForOutturn, mutexKey));
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (scanMutexes != null)
				{
					foreach (var scanMutex in scanMutexes.Values)
					{
						if (scanMutex.HasLock)
						{
							scanMutex.Unlock();
						}
					}

					scanMutexes = null;
				}
			}
		}
	}
}
