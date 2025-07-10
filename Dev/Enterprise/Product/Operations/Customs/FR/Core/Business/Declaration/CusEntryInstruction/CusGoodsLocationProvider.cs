using System;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.FR.Business.Declaration
{
	partial class CusEntryInstruction : ICusGoodsLocationProvider
	{
		protected override EU.Business.CusGoodsLocation GetGoodsLocation()
		{
			var goodsLocation = Customs.Business.CusGoodsLocation.Load<EU.Business.CusGoodsLocation>(this, Customs.Business.CusGoodsLocationUseList.Codes.EntryInstruction, reLoadExistingRows: true);
			if (goodsLocation == null && LockGoodsLocationManagementMutex())
			{
				goodsLocation = Customs.Business.CusGoodsLocation.New<EU.Business.CusGoodsLocation>(this, Customs.Business.CusGoodsLocationUseList.Codes.EntryInstruction);
			}
			return goodsLocation;
		}

		#region Mutex for GoodsLocation

		public void UnlockGoodsLocationManagementMutex()
		{
			if (mutex != null && mutex.IsLocked && mutex.HasLock)
			{
				mutex.Unlock();
			}
		}

		ZGlobalMutex GoodsLocationManagementMutex => mutex ?? (mutex = new ZGlobalMutex(MutexIDs.GoodsLocationManagement, PK.ToString()));
		ZGlobalMutex mutex;

		bool LockGoodsLocationManagementMutex()
		{
			var result = true;
			if (IsInDatabase)
			{
				if (GoodsLocationManagementMutex.IsLocked)
				{
					result = GoodsLocationManagementMutex.HasLock;
				}
				else
				{
					try
					{
						result = GoodsLocationManagementMutex.Lock();
						if (result && HasLoadedGoodsLocation && GoodsLocation is CusGoodsLocation goodsLocation && !goodsLocation.IsDeleted)
						{
							goodsLocation.ReloadSafe();
							goodsLocation.RefreshBinding();
						}
					}
					catch (Exception)
					{
						DisposeMutex();
						throw;
					}
				}
			}
			return result;
		}

		public void DisposeMutex()
		{
			if (mutex != null)
			{
				((IDisposable)mutex).Dispose();
				mutex = null;
			}
		}

		#endregion
	}
}
