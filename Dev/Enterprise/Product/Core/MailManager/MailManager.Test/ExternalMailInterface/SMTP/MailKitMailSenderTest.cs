using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.Integration;
using Enterprise.MailManager.MailFilters.Testing;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Identity.Client;
using Moq;
using NUnit.Framework;
using Rnwood.SmtpServer;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class MailKitMailSenderTest : TestCaseWithFactory
	{
		public void TestFailedToAuthenticateExceptionThrownWhenErrorAcquiringToken()
		{
			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Throws(new MsalUiRequiredException("test", "No account or login hint was passed to the AcquireTokenSilent call. "));

			(var mailServerConfiguration, _, var oAuth2Configuration) = GetConfigurationForTest();
			var smtp = GetMockSmtpClientImpl();

			using (ObjectFactory.Substitute(helper.Object))
			using (var sender = new MailKitMailSender(mailServerConfiguration, oAuth2Configuration) { SmtpClientImpl = smtp })
			{
				AssertExceptionThrown<FailedToAuthenticateException>(() => sender.Connect());
			}
		}

		public void TestGmailFailedToAuthenticateExceptionThrownWhenErrorAcquiringToken()
		{
			var helper = new Mock<IGmailOAuth2AuthenticationHelper>();
			helper.Setup(h => h.AcquireTokenSilentlyAsync(It.IsAny<CancellationToken>())).Throws(new Exception( "No account or login hint was passed to the AcquireTokenSilent call. "));

			(var mailServerConfiguration, _, var oAuth2Configuration) = GetGmailConfigurationForTest();
			var smtp = GetMockSmtpClientImpl();

			using (ObjectFactory.Substitute(helper.Object))
			using (var sender = new MailKitMailSender(mailServerConfiguration, oAuth2Configuration) { SmtpClientImpl = smtp })
			{
				AssertExceptionThrown<FailedToAuthenticateException>(() => sender.Connect());
			}
		}

		public void TestGetSendingServer()
		{
			(var mailServerConfiguration, var userPasswordAuthConfiguration, _) = GetConfigurationForTest();
			var smtp = GetMockSmtpClientImpl(() => { });
			using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration) { SmtpClientImpl = smtp })
			{
				sender.Connect();
				AssertEquals("From: user Server: localhost", sender.GetSendingInfo());
			}
		}

		public void TestUseOAuth2()
		{
			var (mailServerConfiguration, userPasswordAuthConfiguration, oAuth2Configuration) = GetConfigurationForTest();

			var acquireTokenCalled = false;
			var oAuthAuthenticateCalled = false;
			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(new Func<CancellationToken, Task<Microsoft.Identity.Client.AuthenticationResult>>(c =>
			{
				acquireTokenCalled = true;
				return Task.FromResult(TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult(((Ms365OAuth2Configuration)oAuth2Configuration).Identifier, ((Ms365OAuth2Configuration)oAuth2Configuration).ApplicationId));
			}));

			var smtp = GetMockSmtpClientImpl(() => oAuthAuthenticateCalled = true);

			using (ObjectFactory.Substitute(helper.Object))
			using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration) { SmtpClientImpl = smtp })
			{
				sender.Connect();
				Assert(!acquireTokenCalled);
				Assert(!oAuthAuthenticateCalled);
				AssertEquals("From: user Server: localhost", sender.GetSendingInfo());
			}

			using (ObjectFactory.Substitute(helper.Object))
			using (var sender = new MailKitMailSender(mailServerConfiguration, oAuth2Configuration) { SmtpClientImpl = smtp })
			{
				sender.Connect();
				Assert(acquireTokenCalled);
				Assert(oAuthAuthenticateCalled);
				AssertEquals("From: Id90D4AFAA-75D8-47F1-A16C-07347C798C68 Server: localhost with OAuth2", sender.GetSendingInfo());
			}
		}

		internal static (SmtpConfiguration mailServerConfiguration, UserPasswordAuthConfiguration userPasswordAuthConfiguration, IOAuth2Configuration oAuth2Configuration) GetConfigurationForTest(
			string server = "localhost",
			int port = 0,
			string secureConnectionTypes = "",
			string ehloDomain = null,
			string userName = "user",
			string password = "password")
		{
			var smtpConfiguration = new SmtpConfiguration(server, port, secureConnectionTypes, ehloDomain);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(userName, password);
			var oAuth2Configuration = new Ms365OAuth2Configuration(tenantId: "19D6C9DE-4A85-4A42-A018-12F84FEF5BE3",
													 applicationId: "90D4AFAA-75D8-47F1-A16C-07347C798C68",
													 permissionType: Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook,
													 cachedToken: Array.Empty<byte>(),
													 identifier: "Id");
			return (smtpConfiguration, userPasswordAuthConfiguration, oAuth2Configuration);
		}

		internal static (SmtpConfiguration mailServerConfiguration, UserPasswordAuthConfiguration userPasswordAuthConfiguration, IOAuth2Configuration oAuth2Configuration) GetGmailConfigurationForTest(
			string server = "localhost",
			int port = 0,
			string secureConnectionTypes = "",
			string ehloDomain = null,
			string userName = "user",
			string password = "password")
		{
			var smtpConfiguration = new SmtpConfiguration(server, port, secureConnectionTypes, ehloDomain);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(userName, password);
			var oAuth2Configuration = new GmailOAuth2Configuration(null, null);
			return (smtpConfiguration, userPasswordAuthConfiguration, oAuth2Configuration);
		}

		internal static ISmtpClientImpl GetMockSmtpClientImpl(Action authenticationCalledAction = null)
		{
			var smtp = new Mock<ISmtpClientImpl>();
			smtp.Setup(s => s.IsConnected).Returns(false);
			smtp.Setup(s => s.Connect(It.IsAny<SmtpConfiguration>()));
			smtp.Setup(s => s.Capabilities).Returns(SmtpCapabilities.Authentication);
			smtp.Setup(s => s.Authenticate(It.Is<SaslMechanism>(x => x.Credentials.UserName == "Id" + "90D4AFAA-75D8-47F1-A16C-07347C798C68"), It.IsAny<CancellationToken>())).Callback<SaslMechanism, CancellationToken>(
				(x, y) =>
				{
					authenticationCalledAction?.Invoke();
				});
			smtp.Object.Authenticate("user", "password");
			return smtp.Object;
		}

		public void TestMessageIsOnlySentOnceWhenFromAddressIsSameWithAuthenticationAddress()
		{
			var email = "justin.chen@email.com";
			QueueEmailWithEqualsSign(email);

			var (mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest(port: Env.Registry.SMTPPort, userName: Env.Registry.SMTPUsername, password: Env.Registry.SMTPPassword);
			var server = new ServerSupportingAuthLogin(mailServerConfiguration.Port, userPasswordAuthConfiguration.UserName, userPasswordAuthConfiguration.Password);
			server.Start();
			try
			{
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration, email) { SmtpClientImpl = new TestSmtpClientImplWithSpecifiedSmtpFrom(true, email) })
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					var exception = AssertExceptionThrown<FailedToSendMessageException>(() => sender.Send(Factory.LoadTop1<MailItem>(new ZQuery())));
					AssertType<SmtpCommandException>(exception.InnerException);
					AssertEquals(3, ((TestSmtpClientImplWithSpecifiedSmtpFrom)sender.SmtpClientImpl).SendCalledCount);
				}
			}
			finally
			{
				server.Stop();
			}
		}

		public void TestMessageIsSentWithSpecifiedSmtpFrom()
		{
			var email = "justin.chen@email.com";
			AssertSendSuccessfully(email, new TestSmtpClientImplWithSpecifiedSmtpFrom(senderAddress: email));
		}

		public void TestSend()
		{
			AssertSendSuccessfully("Default@edi.com.au");
		}

		void AssertSendSuccessfully(string expectedFromAddress, ISmtpClientImpl smtpClient = null)
		{
			QueueEmailWithEqualsSign();
			var (mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest(port: Env.Registry.SMTPPort, userName: Env.Registry.SMTPUsername, password: Env.Registry.SMTPPassword);
			var authServer = new ServerSupportingAuthLogin(mailServerConfiguration.Port, userPasswordAuthConfiguration.UserName, userPasswordAuthConfiguration.Password);
			ISession session = null;
			var sessionCompletedWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
			authServer.SessionCompleted += (sender, eventArgs) =>
			{
				session = eventArgs.Session;
				sessionCompletedWaitHandle.Set();
			};
			authServer.Start();
			try
			{
				var logger = new LoggerForTest();

				List<RejectedRecipientInfo> rejectedRecipients;
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration, expectedFromAddress, logger))
				{
					if (smtpClient != null)
					{
						sender.SmtpClientImpl = smtpClient;
					}
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					sender.Send(Factory.LoadTop1<MailItem>(new ZQuery()));

					rejectedRecipients = sender.SmtpClientImpl.RejectedRecipients;
				}

				sessionCompletedWaitHandle.WaitOne(5000);
				AssertNotNull(session);
				AssertEquals(0, rejectedRecipients.Count);
				AssertEquals(1, session.Messages.Count);
				AssertEquals(expectedFromAddress, session.Messages[0].From);
				AssertEquals(2, session.Messages[0].To.Length);
				AssertEquals("enterprisecmr@acsedi.edi.net.au", session.Messages[0].To[0]);
				AssertEquals("cc@acsedi.edi.net.au", session.Messages[0].To[1]);

				if (smtpClient != null)
				{
					AssertCollectionContains($@"SMTP Command Error occurring when sending email with subject 'hello'.
MessageFromAddress: ""Developer"" <Default@edi.com.au>
MessageToAddress: <enterprisecmr@acsedi.edi.net.au>;<cc@acsedi.edi.net.au>
RejectedRecipients: Address:justin.chen@email.com,ErrorCode:530,ErrorMessage:blah blah
Error Message: SendAdDeniedException
Will try to deliver the email again using the system email address as the From sender:   From: justin.chen@email.com.", logger.LogEntries);
					AssertEquals(2, ((TestSmtpClientImplWithSpecifiedSmtpFrom)smtpClient).SendCalledCount);
				}
			}
			finally
			{
				authServer.Stop();
			}
		}

		[ExpectExceptionMessage(typeof(FailedToSendMessageException), "We do not relay non-local mail, sorry.")]
		public void TestFailToSend()
		{
			QueueEmailWithEqualsSign();
			var (mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest(port: Env.Registry.SMTPPort, userName: Env.Registry.SMTPUsername, password: Env.Registry.SMTPPassword);
			var server = new ServerSupportingAuthLogin(mailServerConfiguration.Port, userPasswordAuthConfiguration.UserName, userPasswordAuthConfiguration.Password);
			server.MessageCompleted += (sender, eventArgs) =>
			{
				throw new SmtpServerException(new Rnwood.SmtpServer.SmtpResponse(553, "We do not relay non-local mail, sorry."));
			};
			server.Start();
			try
			{
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					sender.Send(Factory.LoadTop1<MailItem>(new ZQuery()));
				}
			}
			finally
			{
				server.Stop();
			}
		}

		[ExpectExceptionMessage(typeof(FailedToSendMessageException), "We do not relay non-local mail, sorry.")]
		public void TestFailToSendWithOverriddenConfiguration()
		{
			QueueEmailWithEqualsSign();
			var mail = Factory.New<MailItem>();
			mail.MI_From = "from@test.cargowise.com";
			mail.AddRecipientForUserCommunication("to@test.cargowise.com");
			mail.MI_Subject = "success";
			mail.MI_Body = "This mail should succeed";
			mail.MI_BusinessEntityID = Guid.NewGuid().ToString();

			var (mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest(port: Env.Registry.SMTPPort, userName: Env.Registry.SMTPUsername, password: Env.Registry.SMTPPassword);
			var server = new ServerSupportingAuthLogin(mailServerConfiguration.Port, userPasswordAuthConfiguration.UserName, userPasswordAuthConfiguration.Password);
			server.MessageCompleted += (sender, eventArgs) =>
			{
				throw new SmtpServerException(new Rnwood.SmtpServer.SmtpResponse(553, "We do not relay non-local mail, sorry."));
			};
			server.Start();
			try
			{
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					sender.Send(mail);
				}
			}
			finally
			{
				server.Stop();
			}
		}

		public void TestFailToSendWithInvalidSenderAddress()
		{
			QueueEmailWithEqualsSign();
			var mail = Factory.New<MailItem>();
			mail.MI_From = "from@test.cargowise.com";
			mail.AddRecipientForUserCommunication("to@test.cargowise.com");
			mail.MI_Subject = "success";
			mail.MI_Body = "This mail should succeed";
			mail.MI_BusinessEntityID = Guid.NewGuid().ToString();

			var (mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest(port: Env.Registry.SMTPPort, userName: Env.Registry.SMTPUsername, password: Env.Registry.SMTPPassword);
			var server = new ServerSupportingAuthLogin(mailServerConfiguration.Port, userPasswordAuthConfiguration.UserName, userPasswordAuthConfiguration.Password);
			server.Start();
			try
			{
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration, "invalid sender address"))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					AssertExceptionThrown<FailedToSendMessageException>("exception thrown", "Unexpected 's' token at offset 8", () => sender.Send(mail));
				}
			}
			finally
			{
				server.Stop();
			}
		}

		[ExpectException(typeof(FailedToConnectException))]
		public void TestServerDoesNotRespond()
		{
			QueueEmailWithEqualsSign();
			var (mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest();
			using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
				sender.Send(Factory.LoadTop1<MailItem>(new ZQuery()));
			}
		}

		[ExpectException(typeof(FailedToConnectException))]
		public void TestServerDoesNotRespondWithOverriddenConfiguration()
		{
			QueueEmailWithEqualsSign();
			(var mailServerConfiguration, var userPasswordAuthConfiguration, _) = GetConfigurationForTest();
			using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
				var mail = Factory.New<MailItem>();
				mail.MI_From = "from@test.cargowise.com";
				mail.AddRecipientForUserCommunication("to@test.cargowise.com");
				mail.MI_Subject = "success";
				mail.MI_Body = "This mail should succeed";
				mail.MI_BusinessEntityID = Guid.NewGuid().ToString();

				sender.Send(mail);
			}
		}

		public void TestAuthLogin()
		{
			var mail = Factory.New<MailItem>();
			mail.MI_From = "from@test.cargowise.com";
			mail.AddRecipientForUserCommunication("to@test.cargowise.com");
			mail.MI_Subject = "test";
			mail.MI_Body = "This is a test";

			var (mailServerConfiguration, userPasswordAuthConfiguration, oAuth2Configuration) = GetConfigurationForTest(port: Env.Registry.SMTPPort);
			var server = new DefaultServer(mailServerConfiguration.Port);
			server.Start();
			try
			{
				using (var sender = new MailKitMailSender(mailServerConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					AssertNoExceptionThrown("Should be no exception thrown when username & password are empty", delegate
					{
						sender.Send(mail);
					});
				}

				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					AssertExceptionThrown(typeof(SmtpConfigurationException), "AUTH LOGIN is not supported by localhost but AUTH LOGIN credentials have been configured", delegate
					{
						sender.Send(mail);
					});
				}
			}
			finally
			{
				server.Stop();
			}

			var authServer = new ServerSupportingAuthLogin(mailServerConfiguration.Port, userPasswordAuthConfiguration.UserName, userPasswordAuthConfiguration.Password);
			authServer.Start();
			try
			{
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					AssertNoExceptionThrown(delegate
					{
						sender.Send(mail);
					});
				}
			}
			finally
			{
				authServer.Stop();
			}

			authServer = new ServerSupportingAuthLogin(mailServerConfiguration.Port, userPasswordAuthConfiguration.UserName, "badpassword");
			authServer.Start();
			try
			{
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					AssertExceptionThrown(typeof(FailedToAuthenticateException), delegate
					{
						sender.Send(mail);
					});
				}
			}
			finally
			{
				authServer.Stop();
			}
		}

		public void TestAuthLoginWithOverriddenConfiguration()
		{
			var mail = Factory.New<MailItem>();
			mail.MI_From = "from@test.cargowise.com";
			mail.AddRecipientForUserCommunication("to@test.cargowise.com");
			mail.MI_Subject = "test";
			mail.MI_Body = "This is a test";
			mail.MI_BusinessEntityID = Guid.NewGuid().ToString();

			var (mailServerConfiguration, userPasswordAuthConfiguration, oAuth2Configuration) = GetConfigurationForTest(port: Env.Registry.SMTPPort);
			var server = new DefaultServer(mailServerConfiguration.Port);
			server.Start();
			try
			{
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					AssertExceptionThrown(typeof(SmtpConfigurationException), "AUTH LOGIN is not supported by localhost but AUTH LOGIN credentials have been configured", delegate
					{
						sender.Send(mail);
					});
				}
			}
			finally
			{
				server.Stop();
			}

			var authServer = new ServerSupportingAuthLogin(mailServerConfiguration.Port, userPasswordAuthConfiguration.UserName, userPasswordAuthConfiguration.Password);
			authServer.Start();
			try
			{
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					AssertNoExceptionThrown(delegate
					{
						sender.Send(mail);
					});
				}
			}
			finally
			{
				authServer.Stop();
			}

			authServer = new ServerSupportingAuthLogin(mailServerConfiguration.Port, userPasswordAuthConfiguration.UserName, "badpassword");
			authServer.Start();
			try
			{
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					AssertExceptionThrown(typeof(FailedToAuthenticateException), delegate
					{
						sender.Send(mail);
					});
				}
			}
			finally
			{
				authServer.Stop();
			}
		}

		public void TestConnectionResetAfterError()
		{
			var failMail = Factory.New<MailItem>();
			failMail.MI_From = "from@test.cargowise.com";
			failMail.MI_Subject = "fail";
			failMail.MI_Body = "This mail should fail because it has not recipient";

			var successMail = Factory.New<MailItem>();
			successMail.MI_From = "from@test.cargowise.com";
			successMail.AddRecipientForUserCommunication("to@test.cargowise.com");
			successMail.MI_Subject = "success";
			successMail.MI_Body = "This mail should succeed";

			var (mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest(port: Env.Registry.SMTPPort, userName: "username", password: "password");
			var server = new ServerSupportingAuthLogin(mailServerConfiguration.Port, userPasswordAuthConfiguration.UserName, userPasswordAuthConfiguration.Password);

			server.MessageCompleted += (source, args) =>
			{
				if (args.Message.To.Length == 0)
				{
					throw new SmtpServerException(new Rnwood.SmtpServer.SmtpResponse(StandardSmtpResponseCode.BadSequenceOfCommands,
						"No RCPT TO specified"));
				}
				for (int i = 0; i < args.Message.Session.Messages.Count; i++)
				{
					if (args.Message.Session.Messages[i].To.Length == 0)
					{
						throw new SmtpServerException(new Rnwood.SmtpServer.SmtpResponse(StandardSmtpResponseCode.BadSequenceOfCommands,
							"RCPT TO already specified"));
					}
				}
			};
			server.Start();
			try
			{
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					AssertExceptionThrown("MailKit.dll throws ArgumentException if recipient does not exist", typeof(FailedToSendMessageException), delegate
					{
						sender.Send(failMail);
					});

					AssertNoExceptionThrown(delegate
					{
						sender.Send(successMail);
					});
				}
			}
			finally
			{
				server.Stop();
			}
		}

		public void TestSendWithOverriddenConfiguration()
		{
			var mail = Factory.New<MailItem>();
			mail.MI_From = "from@test.cargowise.com";
			mail.AddRecipientForUserCommunication("to@test.cargowise.com");
			mail.MI_Subject = "success";
			mail.MI_Body = "This mail should succeed";
			mail.MI_BusinessEntityID = Guid.NewGuid().ToString();

			var (mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest(port: Env.Registry.SMTPPort, userName: "username", password: "password");
			var server = new ServerSupportingAuthLogin(mailServerConfiguration.Port, userPasswordAuthConfiguration.UserName, userPasswordAuthConfiguration.Password);
			server.Start();
			try
			{
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					AssertNoExceptionThrown(() => sender.Send(mail));
				}
			}
			finally
			{
				server.Stop();
			}
		}

		public void TestSmtpEhloDomain()
		{
			DoTestSmtpEhloDomain("Test2.com");
			DoTestSmtpEhloDomain("");
		}

		void DoTestSmtpEhloDomain(string ehloDomain)
		{
			var (mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest(port: Env.Registry.SMTPPort, userName: "username", password: "password", ehloDomain: ehloDomain);
			var server = new ServerForTestEhloDomain(mailServerConfiguration.Port, userPasswordAuthConfiguration.UserName, userPasswordAuthConfiguration.Password);
			server.Start();

			try
			{
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					var mail = Factory.New<MailItem>();
					mail.MI_From = "tester <from@test.cargowise.com>";
					mail.AddRecipientForUserCommunication("to@test.cargowise.com");
					mail.MI_Subject = "success";
					mail.MI_Body = "This mail should succeed";
					sender.Send(mail);

					for (var i = 1; i < 5000; i++)
					{
						if (ServerForTestEhloDomain.MessageReceivedFlag)
						{
							break;
						}

						Thread.Sleep(1);
					}

					if (string.IsNullOrWhiteSpace(ehloDomain))
					{
						AssertContains($"EHLO {System.Environment.MachineName}", ServerForTestEhloDomain.SessionLog);
					}
					else
					{
						AssertContains("EHLO " + ehloDomain, ServerForTestEhloDomain.SessionLog);
					}
				}
			}
			finally
			{
				server.Stop();
			}
		}

		[TestRequiresAdministrativePrivileges("Installs an X509 Certificate")]
		public void TestTLSWithAuth()
		{
			// This is like gmail using TLS
			var port = Env.Registry.SMTPPort;
			var userName = "username";
			var password = "password";

			QueueEmailWithEqualsSign();
			var (mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest(port: port, userName: userName, password: password);

			var server = new ServerSupportingAuthLogin(
				portNumber: mailServerConfiguration.Port,
				username: userPasswordAuthConfiguration.UserName,
				password: userPasswordAuthConfiguration.Password,
				requiresSecureConnection: true);

			server.Start();
			try
			{
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					AssertExceptionThrown(typeof(SmtpConfigurationException), delegate
					{
						sender.Send(Factory.LoadTop1<MailItem>(new ZQuery()));
					});
				}

				(mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest(port: port, secureConnectionTypes: ZArchitecture.Core.SecureConnectionTypes.TLS, userName: userName, password: password);

				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					sender.SmtpClientImpl.SslProtocols = SslProtocols.None;
					sender.SmtpClientImpl.ServerCertificateValidationCallback = (s, certificate, chain, errors) => true;
					AssertNoExceptionThrown(delegate
					{
						sender.Send(Factory.LoadTop1<MailItem>(new ZQuery()));
					});
				}
			}
			finally
			{
				server.Stop();
			}
		}

		[TestRequiresAdministrativePrivileges("Installs an X509 Certificate")]
		public void TestSSLWithAuth()
		{
			// This is like gmail using SSL

			var port = Env.Registry.SMTPPort;
			var userName = "username";
			var password = "password";

			QueueEmailWithEqualsSign();
			var (mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest(port: port, userName: userName, password: password);

			var server = new ServerSupportingAuthLogin(
				portNumber: mailServerConfiguration.Port,
				username: userPasswordAuthConfiguration.UserName,
				password: userPasswordAuthConfiguration.Password,
				requiresSecureConnection: true,
				sslCertificate: ServerCertificate.GetSSLCertificate());

			server.Start();
			try
			{
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					AssertExceptionThrown(typeof(FailedToConnectException), delegate
					{
						sender.SmtpClientImpl.Timeout = 3000;
						sender.Send(Factory.LoadTop1<MailItem>(new ZQuery()));
					});
				}

				(mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest(port: port, secureConnectionTypes: ZArchitecture.Core.SecureConnectionTypes.SSL, userName: userName, password: password);
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					AssertNoExceptionThrown(delegate
					{
						sender.SmtpClientImpl.Timeout = 3000;
						sender.SmtpClientImpl.ServerCertificateValidationCallback = (s, certificate, chain, errors) => true;
						sender.Send(Factory.LoadTop1<MailItem>(new ZQuery()));
					});
				}
			}
			finally
			{
				server.Stop();
			}
		}

		[TestRequiresAdministrativePrivileges("Installs an X509 Certificate")]
		public void TestTLSWithAuthWithOverriddenConfiguration()
		{
			// This is like gmail using TLS

			var port = Env.Registry.SMTPPort;
			var userName = "username";
			var password = "password";

			QueueEmailWithEqualsSign();

			var mail = Factory.LoadTop1<MailItem>(new ZQuery());
			mail.MI_BusinessEntityID = Guid.NewGuid().ToString();
			Factory.Save();

			var (mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest(port: port, userName: userName, password: password);

			var server = new ServerSupportingAuthLogin(
				portNumber: mailServerConfiguration.Port,
				username: userPasswordAuthConfiguration.UserName,
				password: userPasswordAuthConfiguration.Password,
				requiresSecureConnection: true);

			server.Start();
			try
			{
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					AssertExceptionThrown(typeof(SmtpConfigurationException), delegate
					{
						sender.Send(Factory.LoadTop1<MailItem>(new ZQuery()));
					});
				}

				(mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest(port: port, secureConnectionTypes: ZArchitecture.Core.SecureConnectionTypes.TLS, userName: userName, password: password);
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					sender.SmtpClientImpl.SslProtocols = SslProtocols.None;
					sender.SmtpClientImpl.ServerCertificateValidationCallback = (s, certificate, chain, errors) => true;

					AssertNoExceptionThrown(delegate
					{
						sender.Send(Factory.LoadTop1<MailItem>(new ZQuery()));
					});
				}
			}
			finally
			{
				server.Stop();
			}
		}

		[TestRequiresAdministrativePrivileges("Installs an X509 Certificate")]
		public void TestSSLWithAuthWithOverriddenConfiguration()
		{
			// This is like gmail using SSL

			var port = Env.Registry.SMTPPort;
			var userName = "username";
			var password = "password";

			QueueEmailWithEqualsSign();

			var mail = Factory.LoadTop1<MailItem>(new ZQuery());
			mail.MI_BusinessEntityID = Guid.NewGuid().ToString();
			Factory.Save();

			var (mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest(port: port, userName: userName, password: password);

			var server = new ServerSupportingAuthLogin(
				portNumber: mailServerConfiguration.Port,
				username: userPasswordAuthConfiguration.UserName,
				password: userPasswordAuthConfiguration.Password,
				requiresSecureConnection: true,
				sslCertificate: ServerCertificate.GetSSLCertificate());

			server.Start();
			try
			{
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					AssertExceptionThrown(typeof(FailedToConnectException), delegate
					{
						sender.SmtpClientImpl.Timeout = 3000;
						sender.Send(Factory.LoadTop1<MailItem>(new ZQuery()));
					});
				}

				(mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest(port: port, secureConnectionTypes: ZArchitecture.Core.SecureConnectionTypes.SSL, userName: userName, password: password);
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					AssertNoExceptionThrown(delegate
					{
						sender.SmtpClientImpl.Timeout = 3000;
						sender.SmtpClientImpl.ServerCertificateValidationCallback = (s, certificate, chain, errors) => true;
						sender.Send(Factory.LoadTop1<MailItem>(new ZQuery()));
					});
				}
			}
			finally
			{
				server.Stop();
			}
		}

		void QueueEmailWithEqualsSign(string fromAddress = "")
		{
			fromAddress = string.IsNullOrEmpty(fromAddress) ? "Default@edi.com.au" : fromAddress;
			TestCaseHelper.ClearTable(Enterprise.ZArchitecture.Schema.MailDBItemsSchema.Constants.TableName);
			var rawEmail = $"From: \"Developer\" <{fromAddress}>\r\nTo: enterprisecmr@acsedi.edi.net.au\r\nCc: cc@acsedi.edi.net.au\r\nSubject: hello\r\n\r\n=\r\n";
			((OutgoingMailCreator)Env.OutgoingMailManager).CreateAndSaveMIMERaw(rawEmail, TimeSpan.Zero);
		}

		protected override void SetUp()
		{
			base.SetUp();

			firstUnusedPort = FindUnusedPort(25); //	Start scanning available port from 25 to give a wider range in case of firewall blockage
			Env.Registry.SMTPServer = "localhost";
			Env.Registry.SMTPPort = firstUnusedPort;
		}

		int firstUnusedPort;

		static int FindUnusedPort(int startScanAt)
		{
			for (var i = startScanAt; i < startScanAt + 100; i++)
			{
				using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
				{
					try
					{
						var localhost = IPAddress.Parse("127.0.0.1");
						var endPoint = new IPEndPoint(localhost, i);
						socket.Connect(endPoint);
					}
					catch (SocketException ex)
					{
						if (ex.ErrorCode == 10061)
						{
							return i;
						}
					}
				}
			}

			return -1;
		}

		public void TestMailKitConfigIsLoggedWhenConnect()
		{
			var logger = new LoggerForTest();

			QueueEmailWithEqualsSign();

			var senderAddress = "testemail@edi.com.au";
			var (mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest(port: Env.Registry.SMTPPort, userName: "testuser", password: "testpassword");
			var server = new ServerSupportingAuthLogin(mailServerConfiguration.Port, userPasswordAuthConfiguration.UserName, userPasswordAuthConfiguration.Password);
			server.Start();
			try
			{
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration, senderAddress, logger: logger, useMailKitSMTPProtocolLogging: true))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					AssertNoExceptionThrown(() => sender.Send(Factory.LoadTop1<MailItem>(new ZQuery())));

					Assert("Should have more than two logs", logger.LogEntries.Count() > 2);
					AssertEquals(string.Format("MailKitMailSender Connect Smtp -- Host = {0} Port = {1} User = {2} SenderAddress = {3} SecureConnectionType = {4}",
							mailServerConfiguration.Server, mailServerConfiguration.Port, userPasswordAuthConfiguration.UserName, senderAddress, mailServerConfiguration.SecureConnectionType)
						, logger.LogEntries.ToArray()[1]);
				}

				var mockLoggerStream = new MockLoggerStream(logger);

				using (ObjectFactory.Substitute<LoggerStream>(mockLoggerStream))
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration, logger: logger, useMailKitSMTPProtocolLogging: true))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					mockLoggerStream.writeCalled = false;
					AssertNoExceptionThrown(() => sender.Send(Factory.LoadTop1<MailItem>(new ZQuery())));
					AssertEquals("LoggerStream Write should be called", true, mockLoggerStream.writeCalled);
				}
			}
			finally
			{
				server.Stop();
			}
		}

		public void TestMailKitNoLoggerProvidedForConnect()
		{
			QueueEmailWithEqualsSign();

			var senderAddress = "testemail@edi.com.au";
			var (mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest(port: Env.Registry.SMTPPort, userName: "testuser", password: "testpassword");

			var server = new ServerSupportingAuthLogin(mailServerConfiguration.Port, userPasswordAuthConfiguration.UserName, userPasswordAuthConfiguration.Password);
			server.Start();
			try
			{
				using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration, senderAddress))
				{
					MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
					AssertNoExceptionThrown(() => sender.Send(Factory.LoadTop1<MailItem>(new ZQuery())));
				}
			}
			finally
			{
				server.Stop();
			}
		}

		public void TestUseMailKitSMTPProtocolLogging()
		{
			var (mailServerConfiguration, userPasswordAuthConfiguration, _) = GetConfigurationForTest();
			using (var mks = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				Assert("When protocol logging is disabled, MailKit should use NullProtocolLogger", ((SmtpClientImpl)mks.SmtpClientImpl).ProtocolLogger is NullProtocolLogger);
			}

			var dummy = new DummyLogger();
			using (var mks = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration, logger: dummy, useMailKitSMTPProtocolLogging: true))
			{
				Assert("When protocol logging is enabled, MailKit should use ProtocolLogger", ((SmtpClientImpl)mks.SmtpClientImpl).ProtocolLogger is ProtocolLogger);
				var loggerStream = ((ProtocolLogger)((SmtpClientImpl)mks.SmtpClientImpl).ProtocolLogger).Stream;
				Assert("ProtocolLogger's stream should be of type LoggerStream", loggerStream is LoggerStream);
			}
		}

		public void TestWarningLogAddedIfRetryCloseFailed()
		{
			var logger = new LoggerForTest();
			var senderAddress = "testemail@edi.com.au";

			(var mailServerConfiguration, var userPasswordAuthConfiguration, _) = GetConfigurationForTest();

			var smtp = new Mock<ISmtpClientImpl>();
			smtp.Setup(s => s.Connect(It.IsAny<SmtpConfiguration>())).Throws(new FailedToConnectException("Test: connect failed."));
			smtp.Setup(s => s.Close()).Throws(new Exception("Test: close failed."));

			using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration, senderAddress, logger) { SmtpClientImpl = smtp.Object })
			{
				AssertExceptionThrown(typeof(FailedToConnectException), delegate
				{
					sender.Connect();
				});

				AssertNotNull(sender.logger);
				AssertContains("Close connection failed.", logger.LogEntries.ToArray()[1]);
				//Avoid exception throwed in sender.Dispose().
				sender.SmtpClientImpl = null;
			}
		}
	}
}
