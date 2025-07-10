using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Interop.DataObjects;
using Enterprise.RemoteDesktopServices.Server;
using Moq;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	internal class MicrosoftOffice365ObjectHandlerTest : TestCase
	{
		public void TestHandlerIsNotBlocked()
		{
			using (var handler = new MicrosoftOffice365ObjectHandlerForTest())
			{
				var mockDataObject = new Mock<ZDataObjectMicrosoftOffice365>();
				var taskCompleteHandle = new ManualResetEvent(false);
				mockDataObject
					.Setup(x => x.CreateDataObject())
					.Returns(() =>
					{
						Task.Delay(TimeSpan.FromMilliseconds(1)).GetAwaiter().GetResult();
						taskCompleteHandle.Set();
						return null;
					});
				mockDataObject.Object.AccessToken = "TokenForTest";
				mockDataObject.Object.Format = ZDataObjectMicrosoftOffice365.DragType.OneDriveFile;
				var testTask = new Task(() =>
				{
					handler.HandleExposed(null, mockDataObject.Object);
				});
				testTask.Start();
				AssertEquals(expected: 0, WaitHandle.WaitAny(new WaitHandle[] { taskCompleteHandle }, TimeSpan.FromSeconds(3)));
				Assert(testTask.Wait(TimeSpan.FromSeconds(1)));
			}
		}

		class MicrosoftOffice365ObjectHandlerForTest : MicrosoftOffice365ObjectHandler, IDisposable
		{
			public void HandleExposed(IEnterpriseChannel channel, ZDataObjectMicrosoftOffice365 message)
			{
				base.Handle(channel, message);
			}

			protected override Form GetActiveForm(string message)
			{
				return testForm;
			}
			readonly Form testForm = new Form() { Text = "MicrosoftOffice365ObjectHandlerForTest" };

			public void Dispose()
			{
				testForm.Dispose();
			}
		}
	}
}
