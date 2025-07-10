using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.EConversation.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.EConversation.Testing
{
	sealed class ConversationTextBoxTest : TestCaseWithFactory
	{
#if !WINZOR
		[GuiTest]
		public void TestMouseWheelMessage()
		{
			const int WM_MOUSEWHEEL = 0x020A;

			var dummyBizO = Factory.New<DummyBusinessObject>();
			using (var form = new ZFormForTest(dummyBizO))
			{
				var textbox = new ConversationTextBoxForTest();
				form.Controls.Add(textbox);
				form.Show();
				Application.DoEvents();

				form.MessageReceived.Clear();
				var msg = new Message() { HWnd = textbox.Handle, Msg = WM_MOUSEWHEEL, Result = new IntPtr(1) };
				textbox.ScrollBars = RichTextBoxScrollBars.None;
				textbox.WndProc_Exposed(ref msg);
				Application.DoEvents();

				AssertEquals(msg.Result, IntPtr.Zero);
				AssertEquals(true, form.MessageReceived.Any(x => x.Msg == WM_MOUSEWHEEL));

				form.MessageReceived.Clear();
				msg = new Message() { HWnd = textbox.Handle, Msg = WM_MOUSEWHEEL, Result = new IntPtr(1) };
				textbox.ScrollBars = RichTextBoxScrollBars.Vertical;
				textbox.WndProc_Exposed(ref msg);
				Application.DoEvents();
				AssertEquals(false, form.MessageReceived.Any(x => x.Msg == WM_MOUSEWHEEL));
			}
		}

		class ConversationTextBoxForTest : ConversationTextBox
		{
			public void WndProc_Exposed(ref Message m)
			{
				base.WndProc(ref m);
			}
		}

		class ZFormForTest : ZForm
		{
			public ZFormForTest(object dataSource) : base(dataSource)
			{
			}

			protected override void WndProc(ref Message m)
			{
				base.WndProc(ref m);
				MessageReceived.Add(m);
			}

			public readonly List<Message> MessageReceived = new List<Message>();
		}
#endif
	}
}
