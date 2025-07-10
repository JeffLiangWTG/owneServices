using System;
using System.Runtime.InteropServices;
using CargoWise.Interop;
using Enterprise.ZArchitecture.GUI.RichEdit.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.RichEdit
{
	sealed class ZRichEditOleItemTest : TestCase
	{
#if !WINZOR
		public const string TestObjectToInsert = "Enterprise\\Enterprise.sln";

		public void TestRunObjectIfPossible()
		{
			var runnableObject = new TestOleObject();
			var reObject = new UnsafeNativeMethods.REOBJECT();

			IntPtr pOleObject;
			var oleObjectGuid = typeof(UnsafeNativeMethods.IOleObject).GUID;
			Marshal.QueryInterface(Marshal.GetIUnknownForObject(runnableObject), ref oleObjectGuid, out pOleObject);
			reObject.poleobj = pOleObject;
			using (var oleItem = new ZRichEditOleItem(reObject))
			{
				runnableObject.IsRunning = true;
				runnableObject.WasRun = false;
				AssertEquals("Should return false because the object is already running", false, oleItem.RunObjectIfPossible());
				AssertEquals(
					"Object should have had Run call on it anyway to make it run (don't know why it needs to be run when it is already in the 'running' state but to get its DataObject this is required)",
					true, runnableObject.WasRun);

				runnableObject.IsRunning = false;
				runnableObject.WasRun = false;
				AssertEquals("Should return true because the object wasn't already running and needs to be run again", true, oleItem.RunObjectIfPossible());
				AssertEquals(
					"Object should have had Run called on it",
					true, runnableObject.WasRun);
			}
		}
#endif
	}
}
