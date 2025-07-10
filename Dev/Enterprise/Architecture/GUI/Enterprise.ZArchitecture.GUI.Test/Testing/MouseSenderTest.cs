using System;
using System.Windows.Forms;
using CargoWise.Interop;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing;

sealed class MouseSenderTest : TestCase
{
	public void TestSendMessage_BUTTONDOWN_ShouldTriggerMouseDown()
	{
		using var form = new ZForm();
		var args = default(MouseEventArgs);
		form.MouseDown += (sender, e) => args = e;

		MouseSender.SendMessage(form, form.Handle, WindowsMessage.WM_LBUTTONDOWN, 0, makeLParam(100, 50));

		CombineAssertions(() =>
		{
			AssertNotNull("MouseDown should have been raised", args);
			AssertEquals("MouseDown Button should be Left", MouseButtons.Left, args.Button);
			AssertEquals("MouseDown Clicks should be 1", 1, args.Clicks);
			AssertEquals("MouseDown X should be 100", 100, args.X);
			AssertEquals("MouseDown Y should be 50", 50, args.Y);
		});
	}

	public void TestSendMessage_BUTTONUP_ShouldTriggerMouseUp()
	{
		using var form = new ZForm();
		var args = default(MouseEventArgs);
		form.MouseUp += (sender, e) => args = e;

		MouseSender.SendMessage(form, form.Handle, WindowsMessage.WM_LBUTTONUP, 0, makeLParam(-100, -50));

		CombineAssertions(() =>
		{
			AssertNotNull("MouseUp should have been raised", args);
			AssertEquals("MouseUp Button should be Left", MouseButtons.Left, args.Button);
			AssertEquals("MouseUp Clicks should be 1", 1, args.Clicks);
			AssertEquals("MouseUp X should be -100", -100, args.X);
			AssertEquals("MouseUp Y should be -50", -50, args.Y);
		});
	}

	public void TestPostMessage_BUTTONDOWN_ShouldTriggerMouseDown_AfterDoEvents()
	{
		using var form = new ZForm();
		var args = default(MouseEventArgs);
		form.MouseDown += (sender, e) => args = e;

		MouseSender.PostMessage(form, form.Handle, WindowsMessage.WM_LBUTTONDOWN, 0, makeLParam(100, 50));
		Application.DoEvents();

		CombineAssertions(() =>
		{
			AssertNotNull("MouseDown should have been raised", args);
			AssertEquals("MouseDown Button should be Left", MouseButtons.Left, args.Button);
			AssertEquals("MouseDown Clicks should be 1", 1, args.Clicks);
			AssertEquals("MouseDown X should be 100", 100, args.X);
			AssertEquals("MouseDown Y should be 50", 50, args.Y);
		});
	}

	public void TestPostMessage_BUTTONUP_ShouldTriggerMouseUp_AfterDoEvents()
	{
		using var form = new ZForm();
		var args = default(MouseEventArgs);
		form.MouseUp += (sender, e) => args = e;

		MouseSender.PostMessage(form, form.Handle, WindowsMessage.WM_LBUTTONUP, 0, makeLParam(100, 50));
		Application.DoEvents();

		CombineAssertions(() =>
		{
			AssertNotNull("MouseUp should have been raised", args);
			AssertEquals("MouseUp Button should be Left", MouseButtons.Left, args.Button);
			AssertEquals("MouseUp Clicks should be 1", 1, args.Clicks);
			AssertEquals("MouseUp X should be 100", 100, args.X);
			AssertEquals("MouseUp Y should be 50", 50, args.Y);
		});
	}

	IntPtr makeLParam(int x, int y) => new IntPtr((y << 16) | (x & 0xFFFF));
}
