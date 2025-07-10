using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common.Collections;
using CargoWise.Interop;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.RichEdit
{
	internal class ZComRichEditOleInterfaceLocator : IDisposable
	{
		protected ZComRichEditOleInterfaceLocator(RichTextBox richEdit)
		{
			this.richEditRef = new WeakReference(richEdit);
		}

		public static ZComRichEditOleInterfaceLocator GetInstance(RichTextBox richEdit)
		{
			return GetInstance(richEdit, typeof(ZComRichEditOleInterfaceLocator));
		}

		protected static ZComRichEditOleInterfaceLocator GetInstance(RichTextBox richEdit, Type locatorType)
		{
			var result = Instances[richEdit];
			if (result == null)
			{
				var constructor = locatorType.GetConstructor(
					BindingFlags.Instance | BindingFlags.NonPublic,
					null,
					new Type[] { typeof(RichTextBox) },
					null);
				result = (ZComRichEditOleInterfaceLocator)constructor.Invoke(new object[] { richEdit });
				Instances[richEdit] = result;
			}
			result.fReferenceCount++;
			return result;
		}

		public RichTextBox RichEdit
		{
			get { return richEditRef.Target as RichTextBox; }
		}

		public UnsafeNativeMethods.IRichEditOle RichEditOle
		{
			get
			{
				if (richEditOle == null)
				{
					richEditOle = FindIRichEditOle(RichEdit);
					if (richEditOle == null)
					{
						throw new InvalidOperationException("Could not find the IRichEditOle interface. You probably havn't create the control handle yet.");
					}
				}
				return richEditOle;
			}
		}

		UnsafeNativeMethods.IRichEditOle richEditOle;
		readonly WeakReference richEditRef;

		#region IDisposable Members

		public void Dispose()
		{
			fReferenceCount--;
			if (fReferenceCount == 0)
			{
				Instances.Remove(RichEdit);
				if (richEditOle != null)
				{
					Marshal.ReleaseComObject(richEditOle);
					richEditOle = null;
				}
			}
		}

		#endregion

		#region Implementation

		const int WM_USER = 0x0400;
		const int EM_GETOLEINTERFACE = WM_USER + 60;

		protected int fReferenceCount;

		static WeakReferencedKeyDictionary<RichTextBox, ZComRichEditOleInterfaceLocator> Instances
		{
			get { return instances ?? (instances = new WeakReferencedKeyDictionary<RichTextBox, ZComRichEditOleInterfaceLocator>()); }
		}
		[ThreadStatic]
		static WeakReferencedKeyDictionary<RichTextBox, ZComRichEditOleInterfaceLocator> instances;

		/// <summary>
		/// Get the IRichEditOle interface for this RichTextBox.
		/// </summary>
		public static UnsafeNativeMethods.IRichEditOle FindIRichEditOle(RichTextBox richEdit)
		{
			var richEditUnkPtr = IntPtr.Zero;
			var richEditPtr = IntPtr.Zero;

			// alloc a pointer to hold the return value for EM_GETOLEINTERFACE
			var memoryPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(IntPtr)));

			// clear the pointer
			Marshal.WriteIntPtr(memoryPtr, IntPtr.Zero);
			try
			{
				if (UnsafeNativeMethods.SendMessage(new HandleRef(new object(), richEdit.Handle), EM_GETOLEINTERFACE, IntPtr.Zero, memoryPtr) !=
					IntPtr.Zero)
				{
					richEditUnkPtr = Marshal.ReadIntPtr(memoryPtr);
				}
				else
				{
					Globals.Message.ShowDeveloperErrorOnce("EM_GETOLEINTERFACE failed", "EM_GETOLEINTERFACE failed.", "");
				}

				// query for the IRichEditOle interface
				var richEditOleGuid = new Guid("00020D00-0000-0000-C000-000000000046");
				Marshal.QueryInterface(richEditUnkPtr, ref richEditOleGuid, out richEditPtr);

				// wrap it in the C# interface for IRichEditOle
				var result = Marshal.GetTypedObjectForIUnknown(richEditPtr, typeof(UnsafeNativeMethods.IRichEditOle));
				return (UnsafeNativeMethods.IRichEditOle)result;
			}
			finally
			{
				// free the ptr memory
				Marshal.FreeCoTaskMem(memoryPtr);
				ReleaseIfNotNull(richEditUnkPtr);
				ReleaseIfNotNull(richEditPtr);
			}
		}

		static void ReleaseIfNotNull(IntPtr comObjPtrToRelease)
		{
			if (comObjPtrToRelease != IntPtr.Zero)
			{
				Marshal.Release(comObjPtrToRelease);
			}
		}

		#endregion
	}
}
