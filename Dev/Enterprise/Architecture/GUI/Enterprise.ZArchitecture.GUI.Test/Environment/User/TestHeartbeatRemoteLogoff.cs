using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TestHeartbeatRemoteLogoff : TestCase
	{
		[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "testing SqlConnection")]
		public void TestRemoteLogoff_IgnoreAllExceptions()
		{
			var environmentMock = new Mock<IEnvironment>();
			var envMock = new Mock<IEnv>();

			environmentMock.Setup(x => x.CurrentUser).Callback(() =>
			{
				using (var connection = new SqlConnection("Server=[_MOCK_SERVER_];Connection Timeout=1")) // We want to simulate method call failing with SqlException.
				{
					connection.Open();
				}
			});

			envMock.Setup(x => x.Instance).Returns(environmentMock.Object);
			using (EnvProxy.SetTemporaryEnvForTest(envMock.Object))
			{
				AssertNoExceptionThrown(() =>
				{
					try
					{
						var helper = new HeartbeatRemoteLogoff();
						helper.OnRemoteLogoff();
					}
					catch (Exception exception) when (exception is InvalidOperationException && exception.Message == "Calling Application.Exit()")
					{
						// expected for Globals.IsTest
					}
				});
			}
		}

		public void TestRemoteLogoffCtorEnvHavingNoCurrentUser()
		{
			var expectedMessage = $"Someone on another machine has logged in and has forced this application to exit.\r\nA user can be only logged in from one computer or terminal session at a time.";
			var environmentMock = new Mock<IEnvironment>();
			var envMock = new Mock<IEnv>();

			environmentMock.Setup(x => x.CurrentUser).Returns<IUser>(null);
			envMock.Setup(x => x.Instance).Returns(environmentMock.Object);
			using (EnvProxy.SetTemporaryEnvForTest(envMock.Object))
			{
				AssertNoExceptionThrown(() =>
				{
					try
					{
						var helper = new HeartbeatRemoteLogoff();

						helper.OnRemoteLogoff();
						Application.DoEvents();

						CombineAssertions(() =>
						{
							using (var dialog = ZFormModaliser.LastFormShownDialogForTest)
							{
								AssertType(typeof(ZMessageBox), dialog);
								var messageBox = (ZMessageBox)dialog;

								AssertEquals("messageBox.Text", "Remote Log Off", messageBox.Text);
								AssertMultilineASCIIEquals("messageBox.Message", expectedMessage, messageBox.MessageMultilingual);
							}
						});
					}
					catch (Exception exception) when (exception is NullReferenceException)
					{
						throw;
					}
					catch (Exception exception) when (exception is InvalidOperationException && exception.Message == "Calling Application.Exit()")
					{
					}
				});
			}
		}

		public void TestRemoteLogoffCtorEnvWithCurrentUser()
		{
			var expectedMessage = $"Someone on another machine has logged in as \"Bob Tran\" and has forced this application to exit.\r\nA user can be only logged in from one computer or terminal session at a time.";
			var environmentMock = new Mock<IEnvironment>();
			var envMock = new Mock<IEnv>();
			var userMock = new Mock<IUser>();

			userMock.Setup(x => x.FullName).Returns("Bob Tran");
			environmentMock.Setup(x => x.CurrentUser).Returns(userMock.Object);
			envMock.Setup(x => x.Instance).Returns(environmentMock.Object);

			Exception expectedException = null;
			using (EnvProxy.SetTemporaryEnvForTest(envMock.Object))
			{
				try
				{
					var helper = new HeartbeatRemoteLogoff();
					helper.OnRemoteLogoff();
					Application.DoEvents();
				}
				catch (Exception exception)
				{
					expectedException = exception;
				}

				using (var dialog = ZFormModaliser.LastFormShownDialogForTest)
				{
					CombineAssertions(() =>
					{
						AssertNotNull(expectedException);
						Assert(expectedException is InvalidOperationException);
						AssertEquals("Calling Application.Exit()", expectedException?.Message);
						AssertNotEquals(nameof(TestRemoteLogoffHandledUnhandledException), expectedException?.Message);

						AssertType(typeof(SelfLogoffForm), dialog);
						var selfLogoffForm = (SelfLogoffForm)dialog;
						AssertMultilineASCIIEquals("selfLogoffForm.Message is incorrect.", expectedMessage, selfLogoffForm.Message);
					});
				}
			}
		}

		public void TestRemoteLogoffHandledUnhandledException()
		{
			var expectedMessage = $"Someone on another machine has logged in and has forced this application to exit.\r\nA user can be only logged in from one computer or terminal session at a time.";

			var helper = new HeartbeatRemoteLogoff();
			var environmentMock = new Mock<IEnvironment>();
			var envMock = new Mock<IEnv>();
			environmentMock.Setup(x => x.CurrentUser).Returns<IUser>(null);
			envMock.Setup(x => x.Instance).Returns(environmentMock.Object);

			Exception expectedException = null;
			using (EnvProxy.SetTemporaryEnvForTest(envMock.Object))
			{
				try
				{
					helper.OnRemoteLogoff();
					Application.DoEvents();
				}
				catch (Exception exception)
				{
					expectedException = exception;
				}
			}

			CombineAssertions(() =>
			{
				AssertNotNull(expectedException);
				Assert(expectedException is InvalidOperationException);
				AssertEquals("Calling Application.Exit()", expectedException?.Message);
				AssertNotEquals(nameof(TestRemoteLogoffHandledUnhandledException), expectedException?.Message);

				using (var dialog = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertType(typeof(ZMessageBox), dialog);
					var messageBox = (ZMessageBox)dialog;

					AssertEquals("messageBox.Text", "Remote Log Off", messageBox.Text);
					AssertMultilineASCIIEquals("messageBox.Message", expectedMessage, messageBox.MessageMultilingual);
				}
			});
		}

		[TestTimeZoneUNLOCO("USCHI")]
		public void TestLocalTimeZoneInMessage()
		{
#if WINZOR
			var form = new Form();
			form.Show();
#endif
			var helper = new HeartbeatRemoteLogoff();
			var utc = new DateTime(2014, 9, 17, 12, 22, 00);
			helper.OnRemoteUpgradeLogoff(utc, () => false);
			Application.DoEvents();

			var message = ZFormModaliser.LastFormShownDialogForTest as ZMessageBox;
			AssertNotEquals("form shown should be a messageBox", message, null);
			AssertEquals("Message should be in correct TimeZone", string.Format("The system is scheduled to upgrade on {0}. Please save all work in progress and log out. Any unsaved changes will be lost.", utc.AddHours(-5)), message.MessageMultilingual.ToString());
		}

		public void TestMessageClosesWhenNoMore()
		{
#if WINZOR
			var form = new Form();
			form.Show();
#endif
			var helper = new HeartbeatRemoteLogoff();
			var utc = new DateTime(2014, 9, 17, 12, 22, 00);
			helper.OnRemoteUpgradeLogoff(utc, () => false);
			Application.DoEvents();

			var message = ZFormModaliser.LastFormShownDialogForTest as ZMessageBox;
			AssertNotEquals("form shown should be a messageBox", message, null);
			AssertCollectionNotContains("Message should close when no more future updates", message, Application.OpenForms.OfType<ZMessageBox>());
		}

		protected override void SetUp()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
			base.SetUp();
		}
	}
}
