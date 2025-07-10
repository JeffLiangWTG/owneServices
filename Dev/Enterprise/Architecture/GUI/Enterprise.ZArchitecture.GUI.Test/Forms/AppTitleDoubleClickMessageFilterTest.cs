using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Forms.BorderlessForm;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms
{
	public class AppTitleDoubleClickMessageFilterTest : TestCase
	{
		public void TestHandleMouseDoubleClickCorrectly()
		{
			// Arrange
			var mock = new Mock<IToggleMaximiseForm>();
			var intPtr = new IntPtr(1);
			mock.Setup(x => x.TitleHWnd).Returns(intPtr);

			var appTitleDoubleClickMessageFilter = new AppTitleDoubleClickMessageFilter(mock.Object);
			var message = new Message();
			message.HWnd = intPtr;
			message.Msg = 0x0203; //WM_LBUTTONDBLCLK

			var result = appTitleDoubleClickMessageFilter.PreFilterMessage(ref message);
			Assert("AppTitleDoubleClickMessageFilter should handle mouse left button double click message!", result);
			mock.Verify(x => x.ToggleMaximise(), Times.Once);

			message.HWnd = new IntPtr(2);
			result = appTitleDoubleClickMessageFilter.PreFilterMessage(ref message);
			Assert(!result);
			mock.Verify(x => x.ToggleMaximise(), Times.Once);

			message.HWnd = intPtr;
			message.Msg = 0x0201;
			result = appTitleDoubleClickMessageFilter.PreFilterMessage(ref message);
			Assert(!result);
			mock.Verify(x => x.ToggleMaximise(), Times.Once);
		}

		public void TestRemoveFormReferenceWhenFormDispose()
		{
			var mock = new MockIToggleMaximiseForm();
			var appTitleDoubleClickMessageFilter = new AppTitleDoubleClickMessageFilter(mock);
			mock.Dispose();
			Assert("ToggleMaximiseForm reference should be null after form disposed!", appTitleDoubleClickMessageFilter.ToggleMaximiseForm == null);
		}
		class MockIToggleMaximiseForm : IToggleMaximiseForm
		{
			public IntPtr TitleHWnd { get; set; }

			public event EventHandler Disposed;

			public void Dispose()
			{
				Disposed?.Invoke(this, EventArgs.Empty);
			}

			public void ToggleMaximise()
			{
			}
		}
	}
}
