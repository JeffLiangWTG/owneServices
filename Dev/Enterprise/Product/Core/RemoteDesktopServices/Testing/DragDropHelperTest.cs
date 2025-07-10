using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.RemoteDesktopServices.Server;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	internal class DragDropHelperTest : RemoteDesktopServicesTest
	{
		[ExpectNoExceptions]
		public void TestDecodeLagacyMessageWhenFormatException()
		{
			// Arrange
			const string testMessage = "(NotBase64)|||NotImportant";
			EnterpriseChannel.Instance.ShowDragDropTrackingInfoForm();
			using (Form form = new Form())
			using (var logForm = EnterpriseChannel.Instance.DragDropTrackingInfoForm)
			{
				var mockHelper = new Mock<DragDropHelper>();
				mockHelper
					.Protected()
					.Setup<Form>("GetCurrentActiveForm")
					.Returns(form);

				using (ErrorReporter.SetTemporaryInstanceForTest(Mock.Of<IErrorReporter>()))
				{
					// Act
					_ = mockHelper.Object.HandleRemoteWindowTitleAndHandleMessage(testMessage, false, false);
				}

				// Assert
				AssertContains($"Hwnd: NotImportant\r\n	Text: (NotBase64)", logForm.GetTextBoxContent());
			}
		}
	}
}
