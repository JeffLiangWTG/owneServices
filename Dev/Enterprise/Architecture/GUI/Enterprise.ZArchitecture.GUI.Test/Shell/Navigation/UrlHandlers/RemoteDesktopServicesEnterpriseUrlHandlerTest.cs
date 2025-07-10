#if !WINZOR
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Server;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class RemoteDesktopServicesEnterpriseUrlHandlerTest : TestCaseWithDummy
	{
		[GuiTest]
		public void TestDoHandleInOtherThreadWhenShouldWaitForEnterpriseToStart()
		{
			using (SetIsUrlAuthenticationSupported())
			{
				AssertDoHandleInOtherThreadWhenShouldWaitForEnterpriseToStart(waitForEnterpriseToStart: true);
			}
		}

		[GuiTest]
		public void TestShouldWaitForEnterpriseToStartForOldLegacyClient()
		{
			AssertDoHandleInOtherThreadWhenShouldWaitForEnterpriseToStart(waitForEnterpriseToStart: false);
		}

		void AssertDoHandleInOtherThreadWhenShouldWaitForEnterpriseToStart(bool waitForEnterpriseToStart)
		{
			var mockChannel = new MockChannel();
			using (EnterpriseUrlHandlerTest.ClearDispatcher())
			{
				var url = GetUrl();
				ThreadPool.QueueUserWorkItem(_ => ExecuteUrl(mockChannel, url, waitForEnterpriseToStart));
				mockChannel.Result.AsyncWaitHandle.WaitOne(200);
				AssertEquals("Still waiting for app to start", false, EnterpriseUrlHandlerTest.CheckIfDummyFormOpen());
			}

			mockChannel.WaitForResult();
			var urlExecuted = (bool)mockChannel.Result.AsyncState && EnterpriseUrlHandlerTest.CheckIfDummyFormOpen();
			Assert("After waiting for the main form to open, the url should be executed", urlExecuted);
		}

		public void TestDoHandleWhenShouldNotWaitForEnterpriseToStart()
		{
			using (SetIsUrlAuthenticationSupported())
			using (EnterpriseUrlHandlerTest.ClearDispatcher())
			{
				var mockChannel = new MockChannel();
				ExecuteUrl(mockChannel, GetUrl(), waitForEnterpriseToStart: false);
				mockChannel.WaitForResult();
				AssertEquals($"{Enterprise.Core.Constants.ProductName} cannot handle your request, it may be opening or closing at this time. Try again later.", ((RDPRemoteException)mockChannel.Result.AsyncState).RemoteExceptionMessage);
			}
		}

		public void TestShouldSendExceptionBackToNewClient()
		{
			using (SetIsUrlAuthenticationSupported())
			{
				var mockChannel = new MockChannel();
				ExecuteUrl(mockChannel, InvalidUrl, waitForEnterpriseToStart: false);
				mockChannel.WaitForResult();
				Assert("Should send exception back to new client which supports url authentication", mockChannel.Result.AsyncState is Exception);
			}
		}

		public void TestShouldNotSendExceptionsBackToOldLegacyClient()
		{
			var mockChannel = new MockChannel();
			ExecuteUrl(mockChannel, InvalidUrl, waitForEnterpriseToStart: false);
			mockChannel.WaitForResult();
			AssertEquals("Should not send exception back to old legacy client", false, mockChannel.Result.AsyncState);
		}

		static IDisposable SetIsUrlAuthenticationSupported()
		{
			var supportedMessageTypes = new[] { EnterpriseChannelMessageTypes.UrlAuthenticationRequired };
			InitializationMessageHandler.RemoteInitializationMessage = new InitializationMessage(supportedMessageTypes, null);
			Assert("Pre-Condition: url authentication should be supported", InitializationMessageHandler.IsUrlAuthenticationSupported);

			return new DisposableAction(() => InitializationMessageHandler.RemoteInitializationMessage = null);
		}

		static void ExecuteUrl(IEnterpriseChannel channel, string url, bool waitForEnterpriseToStart)
		{
			var messageData = Encoding.UTF8.GetBytes(waitForEnterpriseToStart ? url + "&Wait=True" : url);
			messageData = EnterpriseChannel.ConstructMessage(EnterpriseChannelMessageTypes.EdiEntUrl, Guid.NewGuid(), messageData);
			using (var stream = new MemoryStream(messageData, 4, messageData.Length - 4))
			{
				new RemoteDesktopServicesEnterpriseUrlHandler().Handle(channel, stream);
			}
		}

		string GetUrl()
		{
			Dummy.Factory.Save();
			return ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK);
		}

		const string InvalidUrl = "edient:LicenceCode=InvalidURL";

		#region MockChannel

		class MockChannel : MessageChannel
		{
			internal MockChannel()
			{
				Result = new AsyncResult(typeof(bool));
			}

			public override bool Send(byte[] data)
			{
				using (var stream = new MemoryStream(data, 20, data.Length - 20))
				{
					Result.Complete(stream);
				}
				return true;
			}

			public override bool IsConnected
			{
				get { return true; }
			}

			internal void WaitForResult()
			{
				while (!Result.AsyncWaitHandle.WaitOne(100))
				{
					Application.DoEvents();
				}
			}

			internal AsyncResult Result { get; private set; }

			protected override void OnErrorDuringSendingReturnCallbackMessage(string returnTypeInfo, Exception unexpectedException)
			{
				throw new NotImplementedException();
			}

			protected override void RecordMessageHandlerException(Exception exception, string message)
			{
				throw new NotImplementedException();
			}
		}

		#endregion
	}
}
#endif
