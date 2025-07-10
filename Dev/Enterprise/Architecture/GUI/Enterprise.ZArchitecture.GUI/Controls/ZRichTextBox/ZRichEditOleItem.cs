using System;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Interop;
using CargoWise.Common.Testing;
using CargoWise.Interop;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.RichEdit
{
	public class ZRichEditOleItem : IDisposable
	{
		public ZRichEditOleItem(UnsafeNativeMethods.REOBJECT reObject)
		{
			if (reObject.poleobj != IntPtr.Zero)
			{
				fOleObject = Marshal.GetObjectForIUnknown(reObject.poleobj) as UnsafeNativeMethods.IOleObject;
				fOleCache = Marshal.GetObjectForIUnknown(reObject.poleobj) as UnsafeNativeMethods.IOleCache;
				fRunnableObject = Marshal.GetObjectForIUnknown(reObject.poleobj) as UnsafeNativeMethods.IRunnableObject;
				CursorPosition = reObject.cp;
				Marshal.Release(reObject.poleobj);
			}
			if (reObject.pstg != IntPtr.Zero)
			{
				fStorage = (UnsafeNativeMethods.IStorage)Marshal.GetObjectForIUnknown(reObject.pstg);
				Marshal.Release(reObject.pstg);
			}

			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		public Guid GetUserClassID()
		{
			Guid result;
			fOleObject.GetUserClassID(out result);
			return result;
		}

		public Guid GetRunnableObjectClsID()
		{
			var result = Guid.Empty;
			if (fOleObject is UnsafeNativeMethods.IRunnableObject)
			{
				var runnableObj = (UnsafeNativeMethods.IRunnableObject)fOleObject;
				runnableObj.GetRunningClass(ref result);
			}
			return result;
		}

		public virtual bool IsRunning()
		{
			return fRunnableObject != null && fRunnableObject.IsRunning();
		}

		/// <summary>
		/// 'Runs' the object, that is, loads the dll into the process or loads the exe file out of process (eg. Excel.exe).
		/// </summary>
		/// <returns>true if the object was run successfully and should have CloseOleObject on it when finished.</returns>
		public virtual bool RunObjectIfPossible()
		{
			var result = false;
			if (fRunnableObject != null)
			{
				try
				{
					var wasRunning = fRunnableObject.IsRunning();
					fRunnableObject.Run(IntPtr.Zero);
					if (!wasRunning)
					{
						result = true;
					}
				}
				catch (COMException ex)
				{
					if (ex.ErrorCode != HResult.E_ABORT &&
						ex.ErrorCode != HResult.OLE_E_STATIC &&
						ex.ErrorCode != HResult.RPC_E_SERVERCALL_RETRYLATER)
					{
						throw new COMException(ex.Message + " HResult: " + ex.ErrorCode + " ClassID: " + GetRunnableObjectClsID(), ex);
					}
				}
				catch (OutOfMemoryException) // not really an out of memory exception..
				{
					if (Globals.IsTest)
					{
						throw;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Closes the ole object after a call to RunObjectIfPossible.
		/// </summary>
		public virtual void CloseOleObject()
		{
			if (fOleObject != null)
			{
				fOleObject.Close(0);
			}
		}

		public DataObject NewDataObject()
		{
			return new DataObject(fOleObject);
		}

		public readonly int CursorPosition;

		#region IDisposable Members

		public void Dispose()
		{
			DisposableLeakListener.Instance.UnRegisterDisposable(this);

			if (fOleObject != null)
			{
				try
				{
					if (fOleObject != null)
					{
						Marshal.ReleaseComObject(fOleObject);
					}

					if (fOleCache != null)
					{
						Marshal.ReleaseComObject(fOleCache);
					}

					if (fRunnableObject != null)
					{
						Marshal.ReleaseComObject(fRunnableObject);
					}

					if (fStorage != null)
					{
						Marshal.ReleaseComObject(fStorage);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
		}

		#endregion

		#region Implementation

		readonly UnsafeNativeMethods.IOleObject fOleObject;
		readonly UnsafeNativeMethods.IOleCache fOleCache;
		readonly UnsafeNativeMethods.IRunnableObject fRunnableObject;
		readonly UnsafeNativeMethods.IStorage fStorage;

		protected Metafile GetIconicMetafileFromFile(string fileName)
		{
			var hMetaPict = UnsafeNativeMethods.OleGetIconOfFile(fileName, true);
			var pMetaFilePict = UnsafeNativeMethods.GlobalLock(hMetaPict);
			NativeMethods.METAFILEPICT metaFilePict;
			try
			{
				metaFilePict = (NativeMethods.METAFILEPICT)Marshal.PtrToStructure(pMetaFilePict, typeof(NativeMethods.METAFILEPICT));
			}
			finally
			{
				UnsafeNativeMethods.GlobalUnlock(hMetaPict);
			}
			return ZMetaFileUtil.ConvertMetaPictToDNetMetaFile(metaFilePict);
		}

		protected Metafile GetIconicMetafileFromOleCache()
		{
			var data = fOleObject as System.Runtime.InteropServices.ComTypes.IDataObject;

			// set up the FORMATETC which describes a metafile
			FORMATETC formatEtc;
			unchecked
			{
				formatEtc.cfFormat = (short)DataFormats.GetFormat(DataFormats.MetafilePict).Id;
			}
			formatEtc.ptd = IntPtr.Zero;
			formatEtc.dwAspect = DVASPECT.DVASPECT_ICON;
			formatEtc.lindex = -1;
			formatEtc.tymed = TYMED.TYMED_MFPICT;

			// get the metafile into an STGMEDIUM
			STGMEDIUM stgMedium;
			data.GetData(ref formatEtc, out stgMedium);

			// Pull out the METAFILEPICT which contains a handle to a windows Metafile (this is not an enhanced metafile)
			NativeMethods.METAFILEPICT metaFilePict;
			var pMetaFilePict = UnsafeNativeMethods.GlobalLock(stgMedium.unionmember);
			try
			{
				metaFilePict = (NativeMethods.METAFILEPICT)Marshal.PtrToStructure(pMetaFilePict, typeof(NativeMethods.METAFILEPICT));
			}
			finally
			{
				UnsafeNativeMethods.GlobalUnlock(stgMedium.unionmember);
			}
			return ZMetaFileUtil.ConvertMetaPictToDNetMetaFile(metaFilePict);
		}

		#endregion
	}
}
