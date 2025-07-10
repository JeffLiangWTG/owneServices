using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.RemoteDesktopServices.Server;
using Moq;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	[GuiTest]
	class StartDropHandlerTest : RemoteDesktopServicesTest
	{
		public void TestDragDropTrackingInfo()
		{
			var handler = new StartDropHandlerForTest();
#if NETFRAMEWORK
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(WindowCaptionUtils.Base64EncodeCaptionAndHandle("(MockFormText)", (UIntPtr)123456))))
#else
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(WindowCaptionUtils.Base64EncodeCaptionAndHandle("(MockFormText)", 123456))))
#endif
			{
				handler.Handle(null, stream);
			}
			AssertContains(
				@"##################################
New Hwnd Detected.
	Hwnd: 123456
	Text: (MockFormText)
Cannot find waitingActiveForm because form text is empty.
Show selection form.
Save form to cache. Handle: 123456.
Final result:",
				EnterpriseChannel.Instance.DragDropTrackingInfoForm.GetTextBoxContent());
		}

		public void TestAccessingNullFormFromCache()
		{
			var handler = new StartDropHandlerForTest();
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("TestCase|||TestCase")))
			{
				handler.Handle(null, stream);
			}
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("TestCase|||TestCase")))
			{
				AssertNoExceptionThrown(() => handler.Handle(null, stream));
			}
		}

		public void TestFindFormMatchingText()
		{
			const string captionUnicode = @"编辑公司信息 - DKVIKLIESBJE / VIKING LIFE - SAVING EQUIPMENT A/ S - 生产系统 - Branch: FR - Bordeau - Company: Geodis Freight Forwarding";
			const string captionAnsi = @"Edit Organization - DKVIKLIESBJE / VIKING LIFE - SAVING EQUIPMENT A/ S - Production System - Branch: FR - Bordeau - Company: Geodis Freight Forwarding";
			var enterpriseChannelMock = new Mock<IEnterpriseChannel>();
			var handler = new StartDropHandler();

			var isCitrix = false;
			var terminalServiceMock = new Mock<TerminalService>();
			ObjectFactory.Substitute(terminalServiceMock.Object);

			foreach (var caption in new[] { captionUnicode, captionAnsi })
			{
				isCitrix = !isCitrix;
				terminalServiceMock.Invocations.Clear();
				terminalServiceMock.Setup(x => x.IsCitrixICA).Returns(isCitrix);

				foreach (var textLength in new[] { 64, caption.Length })
				{
					var title = isCitrix ? $@"{caption.Substring(0, textLength)} - \\Remote" : caption;
					Test(title);
				}
			}

			void Test(string title)
			{
				using (var testForm = new TestForm(title))
				{
					testForm.Show();
					Form activeForm = null;

					using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(WindowCaptionUtils.Base64EncodeCaptionAndHandle(title, (UIntPtr)(int)testForm.Handle))))
					{
						handler.Handle(enterpriseChannelMock.Object, stream);
						activeForm = StartDropHandler.UseWaitingActiveForm();
					}

					AssertNotNull(activeForm);
					AssertEquals(testForm, activeForm);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			EnterpriseChannel.Instance.ShowDragDropTrackingInfoForm();
		}

		protected override void TearDown()
		{
			EnterpriseChannel.Instance.DragDropTrackingInfoForm.Dispose();
			base.TearDown();
		}

		sealed class TestForm : Form
		{
			public TestForm(string caption)
			{
				ClientSize = new System.Drawing.Size(800, 450);
				Text = caption;
			}
		}
	}
}
