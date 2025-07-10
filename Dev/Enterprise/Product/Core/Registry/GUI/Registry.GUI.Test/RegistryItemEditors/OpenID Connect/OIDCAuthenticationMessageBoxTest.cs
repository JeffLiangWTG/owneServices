using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using WTG.OpenIDConnect.Login;

namespace Enterprise.Registry.GUI
{
	sealed class OIDCAuthenticationMessageBoxTest : TestCase
	{
		/// <summary>
		/// Dummy interface to ease the mock of the protected methods of <see cref="OIDCAuthenticationMessageBox"/>
		/// </summary>
		public interface IProtectedMock
		{
			Task<T> PerformAction<T>(Func<CancellationToken, T> innFunc);
			void Dispose(bool disposing);
		}

		public class OIDCAuthenticationMessageBoxFixture : OIDCAuthenticationMessageBox
		{
			public OIDCAuthenticationMessageBoxFixture(ContainerControl parentControl) : base(parentControl) { }
			public Task<T> CallBasePerformAction<T>(Func<CancellationToken, T> innFunc) => base.PerformAction(innFunc);
			protected override Task<T> PerformAction<T>(Func<CancellationToken, T> innFunc) => Task.FromResult(innFunc(CancellationToken));
			public CancellationToken CancellationToken => CancellationTokenSource.Token;
			public bool ActiveFlag { get; set; }
			public void RunOnClosedEvent() => OnClosed(EventArgs.Empty);
		}

		public void TestConstructor()
		{
			using (var parentControl = new ContainerControl())
			{
				using (var testObject = new OIDCAuthenticationMessageBox(parentControl))
				{
					AssertEquals(parentControl, testObject.ParentControl);
				}
				AssertEquals(false, parentControl.IsDisposed);
			}
		}

		public void TestInitialisation()
		{
			using (var testObject = new OIDCAuthenticationMessageBox(null))
			{
				AssertEquals("Verifying OpenID Connect Settings. In order to verify, you need to login as a controller user in the web pop up window, click \"Cancel\" to cancel the verification process.", testObject.Message);
				testObject.Show();
				var cancelButton = testObject.FindSingleOrDefault((ZButton button) => button.Text == "Cancel");
				AssertNotNull(cancelButton);
				AssertEquals(true, cancelButton.Enabled);
				AssertEquals(true, cancelButton.Visible);
				var otherButtons = testObject.FindAll((ZButton button) => button.Visible && (button != cancelButton));
				AssertContainsExactElementsInExactOrder(Array.Empty<ZButton>(), otherButtons);
			}
		}

		public void TestPerformLoginCallsPerformActionAndTakeOwnership()
		{
			var factory = new BusinessObjectFactory();
			AssertEquals(true, factory.IsOwnedByCurrentThread);
			factory.RelinquishThreadOwnership();
			AssertEquals(false, factory.IsOwnedByCurrentThread);

			var userMock = new Mock<IUser>(MockBehavior.Strict);
			userMock.As<IFactoryProvider>().SetupGet(m => m.Factory).Returns(factory);

			var mock = new Mock<OIDCAuthenticationMessageBox>(null) { CallBase = true, };
			var protectedMock = mock.Protected().As<IProtectedMock>();
			using (var testObject = mock.Object)
			{
				mock.Reset(); // so we do not need to worry about calls done in the ZMessageBox constructor
				protectedMock
					.Setup(m => m.PerformAction(It.IsAny<Func<CancellationToken, LoginAuthenticationInfo>>()))
					.Returns(Task.FromResult(LoginAuthenticationInfo.NewSuccessfulLogin(userMock.Object)));
				var oidcConfig = new OIDCConfig();
				var task = testObject.PerformLogin(oidcConfig);
				task.Wait();
				var result = task.Result;
				AssertEquals(LoginAuthenticationInfo.Status.OK, result.State);
				AssertSame(userMock.Object, result.User);
				protectedMock.Verify(m => m.PerformAction(It.IsAny<Func<CancellationToken, LoginAuthenticationInfo>>()), Times.Once());
				mock.VerifyNoOtherCalls();
				AssertEquals(true, factory.IsOwnedByCurrentThread);
			}
			protectedMock.Verify(m => m.Dispose(true), Times.Once());
			userMock.As<IFactoryProvider>().VerifyGet(m => m.Factory, Times.Once());
			userMock.VerifyNoOtherCalls();
		}

		public void TestVerifyOidcConfigCallsPerformAction()
		{
			var mock = new Mock<OIDCAuthenticationMessageBox>(null) { CallBase = true, };
			var protectedMock = mock.Protected().As<IProtectedMock>();
			using (var testObject = mock.Object)
			{
				mock.Reset(); // so we do not need to worry about calls done in the ZMessageBox constructor
				protectedMock
					.Setup(m => m.PerformAction(It.IsAny<Func<CancellationToken, string>>()))
					.Returns(Task.FromResult("Test Verify"));
				var oidcConfig = new OIDCConfig();
				var task = testObject.VerifyOidcConfig(oidcConfig, "myDomain");
				task.Wait();
				var result = task.Result;
				AssertEquals("Test Verify", result);
				protectedMock.Verify(m => m.PerformAction(It.IsAny<Func<CancellationToken, string>>()), Times.Once());
				mock.VerifyNoOtherCalls();
			}
			protectedMock.Verify(m => m.Dispose(true), Times.Once());
		}

		public void TestPerformLoginDoCallLogin()
		{
			var loginRequestMessages = new List<OIDCLoginRequestMessage>();
			var oidcConfig = new OIDCConfig
			{
				AuthorityURL = "https://localhost/",
				ClientIdentifier = Guid.NewGuid().ToString(),
				OIDCServerType = OIDCServerTypes.Generic,
			};
			var mockOIDCLoginServer = SetupMockOidcLoginServer(loginRequestMessages);
			CancellationToken expectedCancellationToken;
			using (var testObject = new OIDCAuthenticationMessageBoxFixture(null))
			using (ObjectFactory.Substitute(mockOIDCLoginServer.Object))
			{
				expectedCancellationToken = testObject.CancellationToken;
				var result = testObject.PerformLogin(oidcConfig).Result;
				CombineAssertions(() =>
				{
					AssertEquals(LoginAuthenticationInfo.Status.Failure, result.State);
					AssertEquals("Authentication operation was canceled, please try again.", result.FailureMessage);
					AssertEquals(string.Empty, result.ExtendedErrorInformation);
				});
			}
			mockOIDCLoginServer.Verify(m => m.LoginLocal(It.IsAny<OIDCLoginRequestMessage>(), WebUrlLauncher.Launch, expectedCancellationToken, null), Times.Once());
			AssertEquals(1, loginRequestMessages.Count);
			var loginRequestMessage = loginRequestMessages.First();
			CombineAssertions(() =>
			{
				AssertEquals(oidcConfig.AuthorityURL, loginRequestMessage.Authority);
				AssertEquals(oidcConfig.ClientIdentifier, loginRequestMessage.ClientID);
				AssertEquals(OIDCLoginRequestMessage.OIDCServer.Generic, loginRequestMessage.ServerType);
			});
			mockOIDCLoginServer.VerifyNoOtherCalls();
		}

		public void TestVerifyOidcConfigDoCallLogin()
		{
			var loginRequestMessages = new List<OIDCLoginRequestMessage>();
			var oidcConfig = new OIDCConfig
			{
				AuthorityURL = "https://localhost/",
				ClientIdentifier = Guid.NewGuid().ToString(),
				OIDCServerType = OIDCServerTypes.Azure,
			};
			var domain = Guid.NewGuid().ToString();
			var mockOIDCLoginServer = SetupMockOidcLoginServer(loginRequestMessages);
			CancellationToken expectedCancellationToken;
			using (var testObject = new OIDCAuthenticationMessageBoxFixture(null))
			using (ObjectFactory.Substitute(mockOIDCLoginServer.Object))
			{
				expectedCancellationToken = testObject.CancellationToken;
				var result = testObject.VerifyOidcConfig(oidcConfig, domain).Result;
				AssertEquals("Authentication operation was canceled, please try again.", result);
			}
			mockOIDCLoginServer.Verify(m => m.LoginLocal(It.IsAny<OIDCLoginRequestMessage>(), WebUrlLauncher.Launch, expectedCancellationToken, null), Times.Once());
			AssertEquals(1, loginRequestMessages.Count);
			var loginRequestMessage = loginRequestMessages.First();
			CombineAssertions(() =>
			{
				AssertEquals(oidcConfig.AuthorityURL, loginRequestMessage.Authority);
				AssertEquals(oidcConfig.ClientIdentifier, loginRequestMessage.ClientID);
				AssertEquals(OIDCLoginRequestMessage.OIDCServer.Azure, loginRequestMessage.ServerType);
				AssertEquals(domain, loginRequestMessage.DomainHint);
			});
			mockOIDCLoginServer.VerifyNoOtherCalls();
		}

		public void TestPerformActionCanReturnResultAndCloseDialog()
		{
			using (var form = new ZForm())
			using (var testObject = new OIDCAuthenticationMessageBoxFixture(form))
			{
				form.Show();
				var taskResult = testObject.CallBasePerformAction((cancellationToken) =>
				{
					cancellationToken.ThrowIfCancellationRequested();
					testObject.ActiveFlag = true;
					return Task.FromResult(cancellationToken);
				});
				while (!taskResult.IsCompleted)
				{
					Application.DoEvents();
				}
				CombineAssertions(() =>
				{
					AssertEquals("Active flag should be set", true, testObject.ActiveFlag);
					AssertEquals("Dialog should be closed", true, testObject.IsDisposed);
					AssertSame("Dialog should have been displayed", testObject, ZFormModaliser.LastFormShownDialogForTest);
				});
			}
		}

		public void TestPerformActionCanReturnAsyncResultAndCloseDialog()
		{
			TestPerformAction(shouldCancel: false);
		}

		public void TestPerformActionCanBeCancelled()
		{
			TestPerformAction(shouldCancel: true);
		}

		void TestPerformAction(bool shouldCancel)
		{
			using (var form = new ZForm())
			using (var testObject = new OIDCAuthenticationMessageBoxFixture(form))
			using (ZFormModaliser.SetTemporaryDelegateToCallBeforeShowingFormsOrDialogs((obj) => SetActiveFlagOrCloseDialogWhenShown(obj, closeDialog: shouldCancel)))
			{
				form.Show();
				var taskResult = testObject.CallBasePerformAction(cancellationToken =>
				{
					return SpinWait.SpinUntil(() => testObject.ActiveFlag || cancellationToken.IsCancellationRequested, 2000);
				});
				while (!taskResult.IsCompleted)
				{
					Application.DoEvents();
				}
				CombineAssertions(() =>
				{
					AssertEquals("Condition was not met before the delay has expired", true, taskResult.Result);
					AssertEquals("Dialog should be closed", true, testObject.IsDisposed);
					AssertSame("Dialog should have been displayed", testObject, ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals("Active flag not set as expected", !shouldCancel, testObject.ActiveFlag);
				});
			}
		}

		public void TestStartPosition()
		{
			using (var form = new ZForm())
			using (var testObject = new OIDCAuthenticationMessageBoxFixture(form))
			{
				AssertEquals(FormStartPosition.WindowsDefaultLocation, testObject.StartPosition);
			}
		}

		void SetActiveFlagOrCloseDialogWhenShown(object form, bool closeDialog)
		{
			if (form is OIDCAuthenticationMessageBoxFixture messageBox)
			{
				Thread.Sleep(500);
				messageBox.CancellationToken.ThrowIfCancellationRequested();
				if (closeDialog)
				{   // simulate that the dialog is closed by user
					messageBox.RunOnClosedEvent();
				}
				if (!messageBox.CancellationToken.IsCancellationRequested)
				{
					messageBox.ActiveFlag = true;
				}
			}
			else
			{   // let the assertion framework raise an error
				AssertType<OIDCAuthenticationMessageBoxFixture>(form);
			}
		}

		static Mock<IOIDCLoginServer> SetupMockOidcLoginServer(List<OIDCLoginRequestMessage> loginRequestMessages)
		{
			var mockOIDCLoginServer = new Mock<IOIDCLoginServer>(MockBehavior.Strict);
			mockOIDCLoginServer.SetupGet(m => m.IsSupported).Returns(true);
			mockOIDCLoginServer
				.Setup(m => m.LoginLocal(Capture.In(loginRequestMessages), It.IsAny<OIDCWebLauncher>(),
					It.IsAny<CancellationToken>(), It.IsAny<OIDCLoginFactory>()))
				.Returns((OIDCLoginRequestMessage loginRequest, OIDCWebLauncher webLauncher, CancellationToken cancellationToken,
						OIDCLoginFactory oidcLoginFactory) =>
					BuilDummyResponseMessage(loginRequest, webLauncher, oidcLoginFactory, cancellationToken));
			return mockOIDCLoginServer;
		}

		static OIDCLoginResponseMessage BuilDummyResponseMessage(OIDCLoginRequestMessage request, OIDCWebLauncher webLauncher, OIDCLoginFactory oidcLoginFactory, CancellationToken cancellationToken)
		{
			return OIDCLoginResponseMessage.CreateFailedResponse(OIDCLoginResponseMessage.ErrorType.OperationCanceled, "Operation cancelled", "Operation cancelled for test");
		}
	}
}
