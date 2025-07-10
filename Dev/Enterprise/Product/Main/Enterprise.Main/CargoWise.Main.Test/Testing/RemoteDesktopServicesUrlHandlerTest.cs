using System;
using System.IO;
using System.Text;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Testing;

namespace Enterprise.URLHandler.Testing
{
	sealed class RemoteDesktopServicesUrlHandlerTest : RemoteDesktopServicesTest
	{
		public void TestExecuteUrl()
		{
			const string testUrl = "edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=Organisation&BusinessEntityPK=53bea36d-a4fd-4d39-9061-1e0ebfd4eea5&Hash=%2bGM6J1DnGfSGjrNkn6Q5ceTsEKWaGoypM";
			var handler = new RemoteDesktopServicesEnterpriseUrlHandlerForTest();
			Assert("Url should be executed successfully", ExecuteUrl(handler, testUrl, waitForEnterpriseToStart: true));
			AssertEquals("Should stop looking for other handlers when url executed successfully", 1, handler.HandleCalls);
			AssertEquals("Should pass url to the message handler indicating wait for enterprise to start", testUrl + "&Wait=True", handler.LastUrl);
		}

		public void TestShouldKeepLookingForGoodHandlerWhenHandlerWhichNotAbleToHandleUrlIsFound()
		{
			var handler = new RemoteDesktopServicesEnterpriseUrlHandlerForTest();
			handler.HandleOverride = () =>
			{
				switch (handler.HandleCalls)
				{
					case 1:
						throw new EnterpriseUrlHandlerException(string.Empty);
					case 2:
						return false;
					case 3:
						return true;
				}
				return true;
			};

			AssertEquals("Good url handler found", true, ExecuteUrl(handler, string.Empty, waitForEnterpriseToStart: false));
			AssertEquals("Should go through all channels in order to find the good handler", handler.HandleCalls, 3);
			AssertEquals("Should NOT indicate that should wait for enterprise to start in url", string.Empty, handler.LastUrl);
		}

		public void TestShouldIgnoreAllExceptionsIfGoodHandlerIsFoundEvenIfReturnsFalse()
		{
			var handler = new RemoteDesktopServicesEnterpriseUrlHandlerForTest();
			handler.HandleOverride = () =>
			{
				switch (handler.HandleCalls)
				{
					case 1:
						throw new NotSupportedLicenceKeyException(string.Empty);
					case 3:
						throw new EnterpriseUrlHandlerException(ExceptionMessage);
				}
				return false;
			};

			AssertEquals("Good url handler found but it cannot handle url provided", false, ExecuteUrl(handler, string.Empty, false));
			AssertEquals("Should go through all channels in attempt to find better handler", handler.HandleCalls, 3);
		}

		public void TestAuthenticationExceptionsShouldBeIgnoredIfOtherHandlerFound()
		{
			var handler = new RemoteDesktopServicesEnterpriseUrlHandlerForTest();
			handler.HandleOverride = () =>
			{
				switch (handler.HandleCalls)
				{
					case 1:
						throw new NotSupportedLicenceKeyException(string.Empty);
					case 2:
						throw new EnterpriseUrlHandlerException(ExceptionMessage);
					case 3:
						throw new InvalidUrlCredentialsException(string.Empty);
				}
				return true;
			};

			var exception = AssertExceptionThrown<RDPRemoteException>(() => ExecuteUrl(handler, string.Empty, false));
			AssertEquals("Should go through all channels in order to find good handler", handler.HandleCalls, 3);
			AssertEquals("Should ignore handlers running for other licence or user but should report error from found one", nameof(EnterpriseUrlHandlerException), exception.RemoteExceptionType);
			AssertEquals("Should ignore handlers running for other licence or user but should report error from found one", ExceptionMessage, exception.RemoteExceptionMessage);
		}

		public void TestShouldReportLastUrlHandlerExceptionIfNoGoodHandlerFound()
		{
			var handler = new RemoteDesktopServicesEnterpriseUrlHandlerForTest();
			handler.HandleOverride = () =>
			{
				switch (handler.HandleCalls)
				{
					case 1:
						throw new EnterpriseUrlHandlerException("First exception");
					case 2:
						throw new EnterpriseUrlHandlerException(ExceptionMessage);
					case 3:
						throw new InvalidUrlCredentialsException(string.Empty);
				}
				return true;
			};

			var exception = AssertExceptionThrown<RDPRemoteException>(() => ExecuteUrl(handler, string.Empty, false));
			AssertEquals(nameof(EnterpriseUrlHandlerException), exception.RemoteExceptionType);

			AssertEquals("Should go through all channels in order to find good handler", handler.HandleCalls, 3);
			AssertEquals("Should should report last url handler error ignoring authentication exceptions", ExceptionMessage, exception.RemoteExceptionMessage);
		}

		public void TestExceptionShouldBeRethrownWhenNoOtherHandlerFound()
		{
			AssertExceptionShouldBeRethrownWhenNoOtherHandlerFound(m => new NotSupportedLicenceKeyException(m));
			AssertExceptionShouldBeRethrownWhenNoOtherHandlerFound(m => new InvalidUrlCredentialsException(m));
			AssertExceptionShouldBeRethrownWhenNoOtherHandlerFound(m => new EnterpriseUrlHandlerException(m));
		}

		void AssertExceptionShouldBeRethrownWhenNoOtherHandlerFound<TException>(Func<string, TException> newExceptionToThrow)
			where TException : EnterpriseUrlHandlerException
		{
			Func<bool> handle = () => { throw newExceptionToThrow(ExceptionMessage); };
			var handler = new RemoteDesktopServicesEnterpriseUrlHandlerForTest { HandleOverride = handle };
			var exception = AssertExceptionThrown<RDPRemoteException>(() => ExecuteUrl(handler, string.Empty, false));
			AssertEquals("Exception should be re-thrown as no good url handler found", typeof(TException).Name, exception.RemoteExceptionType);
			AssertEquals("Exception should be re-thrown as no good url handler found", ExceptionMessage, exception.RemoteExceptionMessage);
		}

		bool ExecuteUrl(IMessageHandler handler, string url, bool waitForEnterpriseToStart)
		{
			MessageHandlers.Register(EnterpriseChannelMessageTypes.EdiEntUrl, handler);
			var urlHandlerService = new EnterpriseUrlHandlerClient().GetUrlHandlerService(ClientProcessId);
			AssertEquals("EDIEDIDAT", urlHandlerService.LicenceKeyIdentifier);
			return urlHandlerService.ExecuteUrl(url, waitForEnterpriseToStart);
		}

		protected override void AssertNoClientProcessError()
		{ }

		class RemoteDesktopServicesEnterpriseUrlHandlerForTest : MessageHandlerWithReturn<bool>
		{
			protected override bool DoHandle(IEnterpriseChannel channel, Stream messageData)
			{
				HandleCalls++;
				using (var reader = new StreamReader(messageData, Encoding.UTF8))
				{
					LastUrl = reader.ReadToEnd();
				}
				return HandleOverride == null || HandleOverride();
			}

			public int HandleCalls { get; private set; }
			public Func<bool> HandleOverride { private get; set; }
			public string LastUrl { get; private set; }
		}

		const string ExceptionMessage = "Exception for test";

		protected override int GetChannelNumber()
		{
			return 3;
		}
	}
}
