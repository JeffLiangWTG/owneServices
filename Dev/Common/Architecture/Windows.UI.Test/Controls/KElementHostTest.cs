using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Interop;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class KElementHostTest : TestCase
	{
		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestWndProcNoExceptionThrowWhenHandleAbnormalMessage()
		{
			using (var form = new KForm { Size = new Size(400, 300) })
			using (var elementHost = new KElementHost { Location = new Point(10, 10), Size = new Size(200, 200) })
			{
				form.Size = new Size(400, 400000); 
				form.Controls.Add(elementHost);
				form.Show();

				var mwp = new KForm.WindowPos { x = 10, y = 10, cx = 400, cy = 400, flags = 0, hwnd = elementHost.Handle, hwndInsertAfter = form.Handle };
				var ptrWindowPos = Marshal.AllocHGlobal(Marshal.SizeOf(mwp));
				Marshal.StructureToPtr(mwp, ptrWindowPos, false);
				var message = Message.Create((IntPtr)new HandleRef(elementHost, elementHost.Handle), WindowsMessage.WM_WINDOWPOSCHANGING, IntPtr.Zero, ptrWindowPos);
				UnsafeNativeMethods.SendMessage(new HandleRef(elementHost, elementHost.Handle), WindowsMessage.WM_WINDOWPOSCHANGING, IntPtr.Zero, ptrWindowPos);
				Application.DoEvents();

				AssertNull(ErrorReporter.LastExceptionReported);

				const uint negativeSizeValue = 0xFFD8FFD8; //-40, -40
				var ptrSize = new IntPtr(negativeSizeValue);
				UnsafeNativeMethods.SendMessage(new HandleRef(elementHost, elementHost.Handle), WindowsMessage.WM_SIZE, IntPtr.Zero, ptrSize);
				Application.DoEvents();

				AssertNull(ErrorReporter.LastExceptionReported);
			}
		}
	}
}
