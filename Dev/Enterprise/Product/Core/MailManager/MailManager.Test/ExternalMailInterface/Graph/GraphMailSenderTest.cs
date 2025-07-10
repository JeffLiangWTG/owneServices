using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.Integration;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Moq;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class GraphMailSenderTest : TestCaseWithFactory
	{
		public void TestDeleteEmailFromDraftsFolderIfFailsToSend()
		{
			var mailItem = NewMailItemWithRawMIMEString();
			var httpProvider = GetMockHttpProviderWithSend(mailItem);

			var logger = new LoggerForTest();
			var authResult = TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult("Id", EmailType.Outgoing);
			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(authResult));
			using (ObjectFactory.Substitute(helper.Object))
			using (var sender = new GraphMailSender(GetOAuth2ConfigurationForTest(cachedToken: new byte[] { 1, 2 }, identifier: "Idx"), "testc@test.com", logger))
			{
				var client = new GraphServiceClientForSenderTest(authResult.GetAuthenticationProvider(), httpProvider);
				sender.GraphServiceClientForTest = client;

				sender.Send(mailItem);
				AssertCollectionContains($@"Graph Service Error occurring when sending email with subject 'PRA=1STOPLIVE,1,EDIAL'.
MessageFromAddress: 174267@qq.com
MessageToAddress: <1-stoppra@editest.net.au>
Error Message: Code: ErrorSendAsDenied
Message: The user account which was used to submit this request does not have the right to send mail as the specified sending account., Cannot submit message.

Will try to deliver the email again using the system email address as the From sender:   From: testc@test.com.", logger.LogEntries);

				AssertEquals("Delete mail from draft if failed to send.", true, client.IsDeletedExecuted);
				AssertNull("No Developer Errors should be reported", ErrorReporter.LastExceptionReported);

				try
				{
					var serviceClient = new GraphServiceClientForSenderTest(authResult.GetAuthenticationProvider(), httpProvider);
					serviceClient.ExceptionForDelete = new InvalidOperationException("Jerry Test");

					sender.GraphServiceClientForTest = serviceClient;
					sender.Send(mailItem);

					AssertCollectionContains("GraphMailSender_DeleteDraftEmailError: Failed to delete draft email. Email Subject: PRA=1STOPLIVE,1,EDIAL", logger.LogEntries);
				}
				finally
				{
					ErrorReporter.Clear();
				}
			}
		}

		public void TestFailedToAuthenticateExceptionThrownWhenErrorAcquiringToken()
		{
			var helper1 = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper1.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Throws(new MsalUiRequiredException("test", "No account or login hint was passed to the AcquireTokenSilent call. "));

			using (Env.Registry.RawRegistry.Ms365OAuth2TokenForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Ms365OAuth2Token { Identifier = "Id", Token = new byte[] { 1, 2 }, User = "email@test.com" }))
			using (ObjectFactory.Substitute(helper1.Object))
			using (var sender = new GraphMailSender(GetOAuth2ConfigurationForTest()))
			{
				var mailItem = Factory.NewWithValidTestData<MailItem>();
				AssertExceptionThrown<FailedToAuthenticateException>(() => sender.Send(mailItem));
			}

			var helper2 = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper2.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Throws(new MsalServiceException("test", "TenantID / ApplicationID / AppSecret is incorrect. "));
			using (ObjectFactory.Substitute(helper2.Object))
			using (var sender = new GraphMailSender(GetOAuth2ConfigurationForTest(false)))
			{
				var mailItem = Factory.NewWithValidTestData<MailItem>();
				AssertExceptionThrown<FailedToAuthenticateException>(() => sender.Send(mailItem));
			}
		}

		public void TestExceptionThrownWhenMailItemIsInvalid()
		{
			using (var sender = new GraphMailSender(GetOAuth2ConfigurationForTest()))
			{
				AssertExceptionThrown<NullReferenceException>(() => sender.Send(null));
			}
		}

		public void TestNoExceptionThrownWhenCreatingEmailWithAttachmentInBody()
		{
			var authResult = TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult("Id", EmailType.Outgoing);
			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(
				Task.FromResult(authResult));

			var mailItem = NewMailItemWithRawMIMEString();

			var httpProvider = new Mock<IHttpProvider>();
			httpProvider.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>())).Returns(Task.FromResult(new HttpResponseMessage
			{
				StatusCode = System.Net.HttpStatusCode.Created,
				Content = new StringContent(ValidMessageResultJsonString, Encoding.UTF8, "application/json")
			}));

			using (Env.Registry.RawRegistry.Ms365OAuth2TokenForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Ms365OAuth2Token { Identifier = "Id", Token = new byte[] { 1, 2 }, User = "email@test.com" }))
			using (ObjectFactory.Substitute(helper.Object))
			using (var sender = new GraphMailSender(GetOAuth2ConfigurationForTest()))
			{
				sender.GraphServiceClientForTest = new GraphServiceClientForSenderTest(authResult.GetAuthenticationProvider(), httpProvider.Object);
				AssertNoExceptionThrown(() => sender.Send(mailItem));
			}

			var authResult2 = TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult();
			var helper2 = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper2.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(authResult2));
			using (ObjectFactory.Substitute(helper2.Object))
			using (var sender = new GraphMailSender(GetOAuth2ConfigurationForTest(false)))
			using (RawDataRegistry.Instance.MailboxEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test@email.com"))
			{
				sender.GraphServiceClientForTest = new GraphServiceClientForSenderTest(authResult2.GetAuthenticationProvider(), httpProvider.Object);
				AssertNoExceptionThrown(() => sender.Send(mailItem));
			}
		}

		public void TestCreateLogForUserWhenEmailAccountHaveNoSendAsRight()
		{
			var mailItem = NewMailItemWithRawMIMEString();
			var httpProvider = GetMockHttpProviderWithSend(mailItem);

			var logger = new LoggerForTest();
			var authResult = TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult("Id", EmailType.Outgoing);
			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(
				Task.FromResult(authResult));
			using (ObjectFactory.Substitute(helper.Object))
			using (var sender = new GraphMailSender(GetOAuth2ConfigurationForTest(cachedToken: new byte[] { 1, 2 }, identifier: "Idx"), senderAddress: "testc@test.com", logger: logger))
			{
				sender.GraphServiceClientForTest = new GraphServiceClientForSenderTest(authResult.GetAuthenticationProvider(), httpProvider);

				sender.Send(mailItem);
				AssertCollectionContains($@"Graph Service Error occurring when sending email with subject 'PRA=1STOPLIVE,1,EDIAL'.
MessageFromAddress: 174267@qq.com
MessageToAddress: <1-stoppra@editest.net.au>
Error Message: Code: ErrorSendAsDenied
Message: The user account which was used to submit this request does not have the right to send mail as the specified sending account., Cannot submit message.

Will try to deliver the email again using the system email address as the From sender:   From: testc@test.com.", logger.LogEntries);
			}

			logger = new LoggerForTest();
			var authResult2 = TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult();
			var helper2 = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper2.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(authResult2));
			using (ObjectFactory.Substitute(helper2.Object))
			using (var sender = new GraphMailSender(GetOAuth2ConfigurationForTest(false, new byte[] { 1, 2 }), "test@email.com", logger))
			{
				sender.GraphServiceClientForTest = new GraphServiceClientForSenderTest(authResult2.GetAuthenticationProvider(), httpProvider);

				sender.Send(mailItem);
				AssertCollectionContains($@"Graph Service Error occurring when sending email with subject 'PRA=1STOPLIVE,1,EDIAL'.
MessageFromAddress: 174267@qq.com
MessageToAddress: <1-stoppra@editest.net.au>
Error Message: Code: ErrorSendAsDenied
Message: The user account which was used to submit this request does not have the right to send mail as the specified sending account., Cannot submit message.

Will try to deliver the email again using the system email address as the From sender:   From: test@email.com.", logger.LogEntries);
			}
		}

		public void TestEmailContentWithMultibyteCharacters()
		{
			var actualContent = "";
			var mailItem = NewMailItemWithRawMIMEString();
			mailItem.MI_Body = "Multibyte characters: “ῦ–ῤ”";

			var authResult = TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult("Id", EmailType.Outgoing);
			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(authResult));

			var httpProvider = new Mock<IHttpProvider>();
			httpProvider.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>()))
				.Callback((HttpRequestMessage m) =>
				{
					var result = m.Content.ReadAsStringAsync().GetResultByAwaiter();
					var resultData = Convert.FromBase64String(result);
					actualContent = Encoding.UTF8.GetString(resultData);
				})
				.Returns(Task.FromResult(new HttpResponseMessage
				{
					StatusCode = System.Net.HttpStatusCode.Created,
					Content = new StringContent(ValidMessageResultJsonString, Encoding.UTF8, "application/json")
				}));

			using (Env.Registry.RawRegistry.Ms365OAuth2TokenForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Ms365OAuth2Token { Identifier = "Id", Token = new byte[] { 1, 2 }, User = "email@test.com" }))
			using (ObjectFactory.Substitute(helper.Object))
			using (var sender = new GraphMailSender(GetOAuth2ConfigurationForTest()))
			{
				sender.GraphServiceClientForTest = new GraphServiceClientForSenderTest(authResult.GetAuthenticationProvider(), httpProvider.Object);
				AssertNoExceptionThrown(() => sender.Send(mailItem));
			}

			AssertContains("Multibyte characters: “ῦ–ῤ”", actualContent);
		}

		[DeveloperOnlyTest]
		public void TestFailedToSendMessageExceptionThrownWhenRequestFailed()
		{
			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(
				Task.FromResult(TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult("Id", EmailType.Outgoing)));

			using (ObjectFactory.Substitute(helper.Object))
			using (var sender = new GraphMailSender(GetOAuth2ConfigurationForTest(cachedToken: new byte[] { 1, 2 }, identifier: "Id")))
			{
				var mailItem = Factory.NewWithValidTestData<MailItem>();
				mailItem.MI_From = "from@test.cargowise.com";
				mailItem.AddRecipientForUserCommunication("to@test.cargowise.com");
				try
				{
					sender.Send(mailItem);
				}
				catch (Exception e)
				{
					AssertType<FailedToSendMessageException>(e);
					AssertType<ServiceException>(e.InnerException);
					AssertContains(@"Code: InvalidAuthenticationToken
Message: CompactToken parsing failed with error code: 80049217", e.InnerException.Message);
				}
			}

			var authResult2 = TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult();
			var helper2 = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper2.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(authResult2));
			using (ObjectFactory.Substitute(helper2.Object))
			using (var sender = new GraphMailSender(GetOAuth2ConfigurationForTest(false, new byte[] { 1, 2 }), "test@email.com"))
			{
				var mailItem = Factory.NewWithValidTestData<MailItem>();
				mailItem.MI_From = "from@test.cargowise.com";
				mailItem.AddRecipientForUserCommunication("to@test.cargowise.com");
				try
				{
					sender.Send(mailItem);
				}
				catch (Exception e)
				{
					AssertType<FailedToSendMessageException>(e);
					AssertType<ServiceException>(e.InnerException);
					AssertContains(@"Code: InvalidAuthenticationToken
Message: CompactToken parsing failed with error code: 80049217", e.InnerException.Message);
				}
			}
		}

		[DeveloperOnlyTest]
		public void TestGetSendingInfo()
		{
			var helper = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(
				Task.FromResult(TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult("Id", EmailType.Outgoing)));

			using (ObjectFactory.Substitute(helper.Object))
			using (var sender = new GraphMailSender(GetOAuth2ConfigurationForTest(cachedToken: new byte[] { 1, 2 }, identifier: "Id")))
			{
				var mailItem = Factory.NewWithValidTestData<MailItem>();
				mailItem.MI_From = "from@test.cargowise.com";
				mailItem.AddRecipientForUserCommunication("to@test.cargowise.com");

				try
				{
					sender.Send(mailItem);
				}
				catch (Exception) { }
				AssertEquals("with Graph API From: Id", sender.GetSendingInfo());
			}

			var authResult2 = TestMs365OAuth2AuthenticationHelper.GetAuthenticationResult();
			var helper2 = new Mock<IMs365OAuth2AuthenticationHelper>();
			helper2.Setup(h => h.AcquireTokenAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(authResult2));
			using (ObjectFactory.Substitute(helper2.Object))
			using (var sender = new GraphMailSender(GetOAuth2ConfigurationForTest(false, new byte[] { 1, 2 }), "test@email.com"))
			{
				var mailItem = Factory.NewWithValidTestData<MailItem>();
				mailItem.MI_From = "from@test.cargowise.com";
				mailItem.AddRecipientForUserCommunication("to@test.cargowise.com");

				try
				{
					sender.Send(mailItem);
				}
				catch (Exception) { }
				AssertEquals("with Graph API (client secret)", sender.GetSendingInfo());
			}
		}

		Ms365OAuth2Configuration GetOAuth2ConfigurationForTest(bool useUserToken = true, byte[] cachedToken = null, string identifier = null)
		{
			var permissionType = useUserToken ? Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_GraphAPI : Ms365OAuth2Configuration.Ms365OAuth2PermissionType.ApplicationPermission_GraphAPI;
			var oAuth2Configuration = new Ms365OAuth2Configuration(
				tenantId: Env.Registry.Ms365OAuth2TenantId,
				applicationId: Env.Registry.Ms365ApplicationIdForOutgoing,
				permissionType: permissionType,
				cachedToken: cachedToken ?? [],
				tokenSaveAction: b => { },
				identifier: identifier);
			return oAuth2Configuration;
		}

		#region test utils
		IHttpProvider GetMockHttpProviderWithSend(MailItem mailItem)
		{
			var httpProvider = new Mock<IHttpProvider> { CallBase = true };
			httpProvider.Setup(m =>
				m.SendAsync(It.IsAny<HttpRequestMessage>()))
				.Returns((HttpRequestMessage m) =>
				{
					return Task.FromResult(new HttpResponseMessage
					{
						StatusCode = System.Net.HttpStatusCode.Created,
						Content = new StringContent(
							IsHttpContentEqualsToMIMEString(m.Content, mailItem.RawMIMEString) ? InvalidMessageResultJsonString : InvalidMessageResultJsonString.Replace("UniqueValidMessageID", "InvalidMessageID"),
							Encoding.UTF8,
							"application/json")
					});
				});

			return httpProvider.Object;
		}

		MailItem NewMailItemWithRawMIMEString()
		{
			var mailItem = Factory.New<MailItem>();
			mailItem.RawMIMEString = RawMIMEString;
			return mailItem;
		}

		bool IsHttpContentEqualsToMIMEString(HttpContent content, string rawMIMEString)
		{
			var stringContent = content.ReadAsStringAsync().Result;
			stringContent = Encoding.UTF8.GetString(Convert.FromBase64String(stringContent));
			return stringContent.Equals(rawMIMEString);
		}

		#endregion

		#region Const Message string

		const string RawMIMEString = @"Mime-Version: 1.0
Content-Transfer-Encoding: base64
Content-Type: application/edifact; name=54399700.txt
Content-Disposition: attachment; filename=54399700.txt
From: 174267@qq.com
To: 1-stoppra@editest.net.au
Subject: PRA=1STOPLIVE,1,EDIAL
Importance: HIGH
X-Priority: 1

VU5BOisuPyAnVU5CK1VOT0M6MytFRElBTCsxU1RPUCsyMzAxMDY6MjAwOSsxJ1VOSCsxK0lGVEVS
QTpEOjk4QjpSVDpFTkVUNTQnQkdNK0VSQStXVExIQVkrOStBUSdEVE0rMTM3OjIwMjMwMTA2MTkz
NDI2OjIwNCdOQUQrTVIrQVNMUEInTkFEK01TK1dUTEhBWSdDVEErSUMrOkNBUkdPV0lTRSBPTkUg
U1VQUE9SVCdDT00rMS1TVE9QUFJBQEVESS5ORVQuQVU6RU0nUkZGK0JOOjEyNTg3NDQ4NDU4NCdS
RkYrRVJOOkNPTi1DMDAwMDEzMTgtTUFFVTI1MjQxNTQnVERUKzEwKyszJ1REVCsyMCs2NTQrMSsr
TVNLKysrOTE0MzI0NTo6OkFERUxBSURFIEVYUFJFU1MnTE9DKzkrQVVTWUQrQVNMUEInTE9DKzEx
K1VTTEFYJ0xPQys3K1VTTEFYJ0VRRCtDTitNQUVVMjUyNDE1NCsyMkcwKysyKzUnSEFOKzo6OkdF
TidOQUQrQ1orQ0FSR09XSVNFIC0gQVVTVFJBTElBIChIL08pJ01FQStBQUUrRytLR006ODgyMSdN
RUErQUFFK1ZHTStLR006ODgyMSdTRUwrMjI1NDc4K0FCKzEnRlRYK0FBWSsrU00xOlZHTSsyMDIz
MDEwNjExMzNVVEM6QU5MIENPTlRBSU5FUiBMSU5FIFBUWSBMVEQ7Q09MTElOUyBTVFJFRVQ7TUVM
Qk9VUk5FO0FVOkNBUkdPV0lTRSBPTkUgU1VQUE9SVDtDQVJHT1dJU0UgLSBBVVNUUkFMSUEgKEgv
Tyk7O0NIUklTLkFRVUlOT0BDQVJHT1dJU0UuQ09NOkNBUkdPV0lTRSBPTkUgU1VQUE9SVCdGVFgr
Wk8xKysrR0VORVJBTCdSRkYrQUFFOkFBQUM2NFRFQydVTlQrMjQrMSdVTlorMSsxJw==";

		const string InvalidMessageResultJsonString = @"{
    ""@odata.context"": ""https://graph.microsoft.com/v1.0/$metadata#users('34d9e9ff-617d-4d50-b374-756cecaa4637')/messages/$entity"",
    ""@odata.etag"": ""W/\""CQAAABYAAAAqZbWB4YZ3TYXgaOIqE2n1AACA1Dwx\"""",
    ""id"": ""UniqueValidMessageID"",
    ""body"": {
        ""contentType"": ""text"",
        ""content"": """"
    },
    ""sender"": {
        ""emailAddress"": {
            ""name"": ""eason_tang2022_1@163.com"",
            ""address"": ""eason_tang2022_1@163.com""
        }
    },
    ""from"": {
        ""emailAddress"": {
            ""name"": ""174267@qq.com"",
            ""address"": ""174267@qq.com""
        }
    },
    ""toRecipients"": [
        {
            ""emailAddress"": {
                ""name"": ""1-stoppra@edi.net.au"",
                ""address"": ""1-stoppra@edi.net.au""
            }
        }
    ],
    ""ccRecipients"": [],
    ""bccRecipients"": [],
    ""replyTo"": [],
    ""flag"": {
        ""flagStatus"": ""notFlagged""
    }
}";

		const string ValidMessageResultJsonString = @"{
    ""@odata.context"": ""https://graph.microsoft.com/v1.0/$metadata#users('34d9e9ff-617d-4d50-b374-756cecaa4637')/messages/$entity"",
    ""@odata.etag"": ""W/\""CQAAABYAAAAqZbWB4YZ3TYXgaOIqE2n1AACA1Dwx\"""",
    ""id"": ""AAMkAGU2YjAwMTEzLWNlOTMtNDU0Ni04MTgzLTliM2ZmYjFhZmFlOQBGAAAAAAAezy5mVB_mT5UDcUZ4-3-LBwAqZbWB4YZ3TYXgaOIqE2n1AAAAAAEPAAAqZbWB4YZ3TYXgaOIqE2n1AACA6tU_AAA="",
    ""createdDateTime"": ""2023-01-09T02:27:33Z"",
    ""lastModifiedDateTime"": ""2023-01-09T02:27:34Z"",
    ""changeKey"": ""CQAAABYAAAAqZbWB4YZ3TYXgaOIqE2n1AACA1Dwx"",
    ""categories"": [],
    ""receivedDateTime"": ""2023-01-09T02:27:34Z"",
    ""sentDateTime"": ""2023-01-09T02:27:34Z"",
    ""hasAttachments"": true,
    ""internetMessageId"": ""<SG2PR01MB2380AE48564F2B3953B048B4BEFE9@SG2PR01MB2380.apcprd01.prod.exchangelabs.com>"",
    ""subject"": ""PRA=1STOPLIVE,1,EDIAL"",
    ""bodyPreview"": """",
    ""importance"": ""high"",
    ""parentFolderId"": ""AAMkAGU2YjAwMTEzLWNlOTMtNDU0Ni04MTgzLTliM2ZmYjFhZmFlOQAuAAAAAAAezy5mVB_mT5UDcUZ4-3-LAQAqZbWB4YZ3TYXgaOIqE2n1AAAAAAEPAAA="",
    ""conversationId"": ""AAQkAGU2YjAwMTEzLWNlOTMtNDU0Ni04MTgzLTliM2ZmYjFhZmFlOQAQAC0WJ6KqSAFPnIxdciXc3A0="",
    ""conversationIndex"": ""AQHZI9HuLRYnoqpIAU+cjF1yJdzcDQ=="",
    ""isDeliveryReceiptRequested"": null,
    ""isReadReceiptRequested"": false,
    ""isRead"": true,
    ""isDraft"": true,
    ""webLink"": ""https://outlook.office365.com/owa/?ItemID=AAMkAGU2YjAwMTEzLWNlOTMtNDU0Ni04MTgzLTliM2ZmYjFhZmFlOQBGAAAAAAAezy5mVB%2BmT5UDcUZ4%2F3%2FLBwAqZbWB4YZ3TYXgaOIqE2n1AAAAAAEPAAAqZbWB4YZ3TYXgaOIqE2n1AACA6tU%2BAAA%3D&exvsurl=1&viewmodel=ReadMessageItem"",
    ""inferenceClassification"": ""focused"",
    ""body"": {
        ""contentType"": ""text"",
        ""content"": """"
    },
    ""sender"": {
        ""emailAddress"": {
            ""name"": ""eason_tang2022_1@163.com"",
            ""address"": ""eason_tang2022_1@163.com""
        }
    },
    ""from"": {
        ""emailAddress"": {
            ""name"": ""eason_tang2022_1@163.com"",
            ""address"": ""eason_tang2022_1@163.com""
        }
    },
    ""toRecipients"": [
        {
            ""emailAddress"": {
                ""name"": ""1-stoppra@edi.net.au"",
                ""address"": ""1-stoppra@edi.net.au""
            }
        }
    ],
    ""ccRecipients"": [],
    ""bccRecipients"": [],
    ""replyTo"": [],
    ""flag"": {
        ""flagStatus"": ""notFlagged""
    },
    ""attachments"": [
        {
            ""@odata.type"": ""#microsoft.graph.fileAttachment"",
            ""@odata.mediaContentType"": ""application/edifact"",
            ""id"": ""AAMkAGU2YjAwMTEzLWNlOTMtNDU0Ni04MTgzLTliM2ZmYjFhZmFlOQBGAAAAAAAezy5mVB_mT5UDcUZ4-3-LBwAqZbWB4YZ3TYXgaOIqE2n1AAAAAAEPAAAqZbWB4YZ3TYXgaOIqE2n1AACA6tU_AAABEgAQAD-OBlMKsbNNrvv-NhlHMcs="",
            ""lastModifiedDateTime"": ""2023-01-09T02:27:33Z"",
            ""name"": ""54399700.txt"",
            ""contentType"": ""application/edifact"",
            ""size"": 1053,
            ""isInline"": false,
            ""contentId"": ""C9E78C6C1D69304FB0601A056CF67E8F@apcprd01.prod.exchangelabs.com"",
            ""contentLocation"": null,
            ""contentBytes"": ""VU5BOisuPyAnVU5CK1VOT0M6MytFRElBTCsxU1RPUCsyMzAxMDY6MjAwOSsxJ1VOSCsxK0lGVEVSQTpEOjk4QjpSVDpFTkVUNTQnQkdNK0VSQStXVExIQVkrOStBUSdEVE0rMTM3OjIwMjMwMTA2MTkzNDI2OjIwNCdOQUQrTVIrQVNMUEInTkFEK01TK1dUTEhBWSdDVEErSUMrOkNBUkdPV0lTRSBPTkUgU1VQUE9SVCdDT00rMS1TVE9QUFJBQEVESS5ORVQuQVU6RU0nUkZGK0JOOjEyNTg3NDQ4NDU4NCdSRkYrRVJOOkNPTi1DMDAwMDEzMTgtTUFFVTI1MjQxNTQnVERUKzEwKyszJ1REVCsyMCs2NTQrMSsrTVNLKysrOTE0MzI0NTo6OkFERUxBSURFIEVYUFJFU1MnTE9DKzkrQVVTWUQrQVNMUEInTE9DKzExK1VTTEFYJ0xPQys3K1VTTEFYJ0VRRCtDTitNQUVVMjUyNDE1NCsyMkcwKysyKzUnSEFOKzo6OkdFTidOQUQrQ1orQ0FSR09XSVNFIC0gQVVTVFJBTElBIChIL08pJ01FQStBQUUrRytLR006ODgyMSdNRUErQUFFK1ZHTStLR006ODgyMSdTRUwrMjI1NDc4K0FCKzEnRlRYK0FBWSsrU00xOlZHTSsyMDIzMDEwNjExMzNVVEM6QU5MIENPTlRBSU5FUiBMSU5FIFBUWSBMVEQ7Q09MTElOUyBTVFJFRVQ7TUVMQk9VUk5FO0FVOkNBUkdPV0lTRSBPTkUgU1VQUE9SVDtDQVJHT1dJU0UgLSBBVVNUUkFMSUEgKEgvTyk7O0NIUklTLkFRVUlOT0BDQVJHT1dJU0UuQ09NOkNBUkdPV0lTRSBPTkUgU1VQUE9SVCdGVFgrWk8xKysrR0VORVJBTCdSRkYrQUFFOkFBQUM2NFRFQydVTlQrMjQrMSdVTlorMSsxJw==""
        }
    ]
}";

		#endregion
	}
}
