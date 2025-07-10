using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class SaveProgressFormTest : TestCase
	{
		public void TestFocusIsNotInTextBoxSoWeDoNotSeeCursor()
		{
			using (var form = new SaveProgressForm())
			{
				form.Show();
				AssertEquals(false, form.ProgressTextBox.Focused);
			}
		}

		public void TestUpdateStatus()
		{
			using (var form = new SaveProgressForm())
			{
				form.UpdateStatus("firstStatus", 10);
				AssertEquals("Status text box", "firstStatus", form.ProgressTextBox.Text);
				AssertEquals("Status progress bar", 10, form.ProgressBar.Value);

				form.Show();
				form.UpdateStatus("secondStatus", 50);
				AssertEquals("Status text box", "secondStatus", form.ProgressTextBox.Text);
				AssertEquals("Status progress bar", 50, form.ProgressBar.Value);
			}
		}

		public void TestUserClosingNotPermitted()
		{
			using (var form = new SaveProgressForm())
			{
				form.Show();
				Application.DoEvents();
				form.Close();
				AssertEquals(true, form.Visible);
			}
		}

		public void TestProgressFormTextBox()
		{
			using (var form = new SaveProgressForm())
			{
				form.ProgressTextBox.MouseClick += ProgressTextBox_MouseClick;
				form.Show();
				Application.DoEvents();
				AssertNoExceptionThrown(() => ClickOnPoint(form.Handle, new Point(form.ProgressTextBox.Left + 1, form.ProgressTextBox.Top + 1)));
				Application.DoEvents();
			}
		}

		private void ProgressTextBox_MouseClick(object sender, MouseEventArgs e)
		{
			throw new NotImplementedException();
		}

		#region PerformMouseClick

		[DllImport("user32.dll")]
		static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

		[DllImport("user32.dll")]
		internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

#pragma warning disable 649
		internal struct INPUT
		{
			public UInt32 Type;
			public MOUSEKEYBDHARDWAREINPUT Data;
		}

		[StructLayout(LayoutKind.Explicit)]
		internal struct MOUSEKEYBDHARDWAREINPUT
		{
			[FieldOffset(0)]
			public MOUSEINPUT Mouse;
		}

		internal struct MOUSEINPUT
		{
			public Int32 X;
			public Int32 Y;
			public UInt32 MouseData;
			public UInt32 Flags;
			public UInt32 Time;
			public IntPtr ExtraInfo;
		}

		static void ClickOnPoint(IntPtr wndHandle, Point clientPoint)
		{
			var oldPos = Cursor.Position;

			/// get screen coordinates
			ClientToScreen(wndHandle, ref clientPoint);

			/// set cursor on coords, and press mouse
			Cursor.Position = new Point(clientPoint.X, clientPoint.Y);

			var inputMouseDown = new INPUT
			{
				/// input type mouse
				Type = 0
			};
			inputMouseDown.Data.Mouse.Flags = 0x0002;

			var inputMouseUp = new INPUT
			{
				/// input type mouse
				Type = 0
			};
			inputMouseUp.Data.Mouse.Flags = 0x0004;

			var inputs = new INPUT[] { inputMouseDown, inputMouseUp };
			SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

			/// return mouse 
			Cursor.Position = oldPos;
		}

#pragma warning restore 649

		#endregion
	}
}
