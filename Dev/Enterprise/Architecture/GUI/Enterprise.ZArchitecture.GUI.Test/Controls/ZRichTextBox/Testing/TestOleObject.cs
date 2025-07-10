using System;
using System.Runtime.InteropServices.ComTypes;
using CargoWise.Interop;

namespace Enterprise.ZArchitecture.GUI.RichEdit.Testing
{
#if !WINZOR
	sealed class TestOleObject : UnsafeNativeMethods.IRunnableObject, UnsafeNativeMethods.IOleObject
	{
		public TestOleObject()
			: this(Guid.Empty)
		{
		}

		public TestOleObject(Guid runningClass)
		{
			this.RunningClass = runningClass;
		}

		public Guid RunningClass;

		#region IRunnableObject Members

		public int SetContainedObject(bool fContained)
		{
			return 0;
		}

		public int GetRunningClass(ref Guid lpClsid)
		{
			lpClsid = RunningClass;
			return 0;
		}

		public bool WasRun;
		public int Run(IntPtr lpbc)
		{
			WasRun = true;
			IsRunning = true;
			return 0;
		}

		public bool IsRunning;
		bool UnsafeNativeMethods.IRunnableObject.IsRunning()
		{
			return this.IsRunning;
		}

		public int LockRunning(bool fLock, bool fLastUnlockCloses)
		{
			return 0;
		}

		#endregion

		#region IOleObject Members

		public int Advise(UnsafeNativeMethods.IAdviseSink pAdvSink, out int cookie)
		{
			// TODO:  Add TestOleObject.Advise implementation
			cookie = 0;
			return 0;
		}

		public UnsafeNativeMethods.IOleClientSite GetClientSite()
		{
			// TODO:  Add TestOleObject.GetClientSite implementation
			return null;
		}

		public int DoVerb(int iVerb, IntPtr lpmsg, UnsafeNativeMethods.IOleClientSite pActiveSite, int lindex, IntPtr hwndParent, NativeMethods.COMRECT lprcPosRect)
		{
			// TODO:  Add TestOleObject.DoVerb implementation
			return 0;
		}

		public int InitFromData(IDataObject pDataObject, int fCreation, int dwReserved)
		{
			// TODO:  Add TestOleObject.InitFromData implementation
			return 0;
		}

		public int SetClientSite(UnsafeNativeMethods.IOleClientSite pClientSite)
		{
			// TODO:  Add TestOleObject.SetClientSite implementation
			return 0;
		}

		public int Unadvise(int dwConnection)
		{
			// TODO:  Add TestOleObject.Unadvise implementation
			return 0;
		}

		public int EnumVerbs(out UnsafeNativeMethods.IEnumOLEVERB e)
		{
			// TODO:  Add TestOleObject.EnumVerbs implementation
			e = null;
			return 0;
		}

		public int EnumAdvise(out IEnumSTATDATA e)
		{
			// TODO:  Add TestOleObject.EnumAdvise implementation
			e = null;
			return 0;
		}

		public int GetUserClassID(out Guid pClsid)
		{
			// TODO:  Add TestOleObject.GetUserClassID implementation
			pClsid = new Guid();
			return 0;
		}

		public int SetMoniker(int dwWhichMoniker, object pmk)
		{
			// TODO:  Add TestOleObject.SetMoniker implementation
			return 0;
		}

		public int Close(int dwSaveOption)
		{
			// TODO:  Add TestOleObject.Close implementation
			return 0;
		}

		public int GetMiscStatus(int dwAspect, out int misc)
		{
			// TODO:  Add TestOleObject.GetMiscStatus implementation
			misc = 0;
			return 0;
		}

		public int SetHostNames(string szContainerApp, string szContainerObj)
		{
			// TODO:  Add TestOleObject.SetHostNames implementation
			return 0;
		}

		public int GetUserType(int dwFormOfType, out string userType)
		{
			// TODO:  Add TestOleObject.GetUserType implementation
			userType = null;
			return 0;
		}

		public int SetColorScheme(NativeMethods.tagLOGPALETTE pLogpal)
		{
			// TODO:  Add TestOleObject.SetColorScheme implementation
			return 0;
		}

		public int GetClipboardData(int dwReserved, out IDataObject data)
		{
			// TODO:  Add TestOleObject.GetClipboardData implementation
			data = null;
			return 0;
		}

		public int IsUpToDate()
		{
			// TODO:  Add TestOleObject.IsUpToDate implementation
			return 0;
		}

		public int SetExtent(int dwDrawAspect, NativeMethods.tagSIZEL pSizel)
		{
			// TODO:  Add TestOleObject.SetExtent implementation
			return 0;
		}

		public int GetMoniker(int dwAssign, int dwWhichMoniker, out IMoniker moniker)
		{
			// TODO:  Add TestOleObject.GetMoniker implementation
			moniker = null;
			return 0;
		}

		public int OleUpdate()
		{
			// TODO:  Add TestOleObject.OleUpdate implementation
			return 0;
		}

		public int GetExtent(int dwDrawAspect, out NativeMethods.tagSIZEL pSizel)
		{
			// TODO:  Add TestOleObject.GetExtent implementation
			pSizel = null;
			return 0;
		}

		#endregion
	}
#endif
}
