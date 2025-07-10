using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentScanning.Business
{
#if DEBUG
	public
#endif
	class DocDbManagerSemaphore : ISemaphoreType
	{
		#region ISemaphoreType Members

		string ISemaphoreType.LockInfo
		{
			get { return "DocumentDbManager"; }
		}

		int ISemaphoreType.MaxConcurrentHandles
		{
			get { return 1; }
		}

		string ISemaphoreType.Category
		{
			get { return "DCM"; }
		}

		#endregion

		public static string GetUserHoldingSemaphore(ISemaphoreHandle semaphoreHandle, BusinessObjectFactory factory)
		{
			string userName = Res.GetString("bf0d2589-318f-4c40-91be-bceb00b83ae0", "another user");
			ISemaphoreInfo[] activeSemaphores = EnvProxy.Instance.SemaphoreProvider.GetActiveSemaphoreHandles(semaphoreHandle.Semaphore);

			if (activeSemaphores.Length > 0)
			{
				GlbStaff staff = factory.Load<GlbStaff>(activeSemaphores[0].OwnerSession.UserPk);

				if (staff != null)
				{
					userName = staff.GS_LoginName;
				}
			}

			return userName;
		}
	}
}
