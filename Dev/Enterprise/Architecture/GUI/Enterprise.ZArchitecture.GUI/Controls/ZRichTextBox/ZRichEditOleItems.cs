using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Interop;

namespace Enterprise.ZArchitecture.GUI.RichEdit
{
	public class ZRichEditOleItems : IDisposable
	{
		internal ZRichEditOleItems(RichTextBox richEdit)
		{
			this.RichEdit = richEdit;
			this.RichEditOleLocator = ZComRichEditOleInterfaceLocator.GetInstance(richEdit);
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		/// <summary>
		/// Get the selected OLE object or null if there is none selected.
		/// </summary>
		public ZRichEditOleItem GetSelectedOleItem()
		{
			return GetOleItemAt(RichEdit.SelectionStart);
		}

		public ZRichEditOleItem GetOleItemJustInserted()
		{
			return GetOleItemAt(RichEdit.SelectionStart > 0 ? RichEdit.SelectionStart - 1 : RichEdit.SelectionStart);
		}

		/// <summary>
		/// Get the OLE object at the given cursor location. Null is returned if this is not possible.
		/// </summary>
		public ZRichEditOleItem GetOleItemAt(int charIndex)
		{
			var reObject = new UnsafeNativeMethods.REOBJECT();
			reObject.cbStruct = Marshal.SizeOf(typeof(UnsafeNativeMethods.REOBJECT));

			using (new RichTextBoxSelectionPreserver(RichEdit))
			{
				try
				{
					// the cursor location must be set like this otherwise the object doesn't get retrieved!
					RichEdit.SelectionStart = charIndex;
					RichEdit.SelectionLength = 1;

					reObject.cp = charIndex;
					RichEditOleLocator.RichEditOle.GetObject(
						-1, ref reObject,
						UnsafeNativeMethods.RichTextEditGetObjectOptions.REO_GETOBJ_POLEOBJ |
						UnsafeNativeMethods.RichTextEditGetObjectOptions.REO_GETOBJ_PSTG);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					// this is the only way to see if GetObject() has failed
				}
			}

			return reObject.poleobj == IntPtr.Zero ? null : new ZRichEditOleItem(reObject);
		}

		public ZRichEditOleItem GetOleItemAtPoint(Point p)
		{
			ZRichEditOleItem result = null;
			var reObject = new UnsafeNativeMethods.REOBJECT();
			reObject.cbStruct = Marshal.SizeOf(typeof(UnsafeNativeMethods.REOBJECT));

			for (var i = 0; i < int.MaxValue; i++)
			{
				try
				{
					RichEditOleLocator.RichEditOle.GetObject(
						i, ref reObject,
						UnsafeNativeMethods.RichTextEditGetObjectOptions.REO_GETOBJ_POLEOBJ |
						UnsafeNativeMethods.RichTextEditGetObjectOptions.REO_GETOBJ_PSTG);
				}
				catch (ArgumentException)
				{
					break; // GetObjectCount always returns zero so this is the only way!
				}
				var charPos = RichEdit.GetPositionFromCharIndex(reObject.cp);

				if (p.X > charPos.X - 100 &&
					p.X < charPos.X + 100 &&
					p.Y > charPos.Y - 100 &&
					p.Y < charPos.Y + 100)
				{
					result = (reObject.poleobj == IntPtr.Zero ? null : new ZRichEditOleItem(reObject));
					break;
				}
			}

			return result;
		}

		#region IDisposable Members

		public void Dispose()
		{
			RichEditOleLocator.Dispose();
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

		#endregion

		readonly RichTextBox RichEdit;
		readonly ZComRichEditOleInterfaceLocator RichEditOleLocator;
	}
}
