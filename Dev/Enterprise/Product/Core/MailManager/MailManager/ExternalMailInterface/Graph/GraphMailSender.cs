using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Core;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using MimeKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public class GraphMailSender : IMailSender
	{
		public GraphMailSender(Ms365OAuth2Configuration oAuth2Configuration, string senderAddress = null, ILogger logger = null)
		{
			this.oAuth2Configuration = oAuth2Configuration;
			this.senderAddress = senderAddress;
			this.logger = logger;
#if DEBUG
			this.logger?.Log(LogType.Debug, $"{nameof(GraphMailSender)} constructed."); // To test logger does be passed here.
#endif
		}

		public string GetSendingInfo() => string.IsNullOrEmpty(senderProfile) ? (NoResString)"Empty From Address,Graph Sender" : senderProfile;

		public void Dispose() { }

		public void Send(MailItem mailItem)
		{
			lock (sendMutex)
			{
				var retriesMax = 3;
				for (var retries = 0; retries < retriesMax; ++retries)
				{
					try
					{
						SendMimeMessage(mailItem);
						return;
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						if (retries == retriesMax - 1)
						{
							throw;
						}
					}
				}
			}
		}

		#region SendMimeMessage

		void SendMimeMessage(MailItem mailItem)
		{
			var message = mailItem.BuildMimeMessage(shouldFillAttachments: false);

			string GetCommonLogInfo()
			{
				return $@"Graph Service Error occurring when sending email with subject '{mailItem.MI_Subject}'.
MessageFromAddress: {mailItem.MI_From}
MessageToAddress: {mailItem.AllRecipients}";
			}

			try
			{
				SendMimeMessageInternal(mailItem, message);
			}
			catch (ServiceException serviceException)
			{
				try
				{
					var messageFromAddress = message.From.OfType<MailboxAddress>().First().Address;
					if (string.IsNullOrEmpty(senderAddress) || messageFromAddress.Equals(senderAddress, StringComparison.InvariantCultureIgnoreCase))
					{
						throw; // If message from address is same with graph from address, no need another try.
					}
					logger?.Log(LogType.Warning, FormattableString.Invariant($"{GetCommonLogInfo()}\r\nError Message: {serviceException.Message}\r\nWill try to deliver the email again using the system email address as the From sender:   From: {senderAddress}."));

					message.From.Clear();
					message.From.Add(InternetAddress.Parse(senderAddress));
					SendMimeMessageInternal(mailItem, message);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					logger?.Log(LogType.Warning, FormattableString.Invariant($"{GetCommonLogInfo()}\r\nError Message: {e.Message}"));
					HandleMailException(e);
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				HandleMailException(e);
			}
		}

		void HandleMailException(Exception exception)
		{
			if (exception is MsalUiRequiredException || exception.InnerException is MsalUiRequiredException ||
				exception is MsalServiceException || exception.InnerException is MsalServiceException)
			{
				ExceptionHandlingHelper.ThrowFailedToAuthenticateException(exception);
			}

			ExceptionHandlingHelper.ThrowFailedToSendMessageException(exception);
		}

		#endregion

		#region SendMimeMessageInternal

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void SendMimeMessageInternal(MailItem mailItem, MimeMessage message)
		{
			var content = Convert.ToBase64String(message.GetData());
			var authResult = Ms365OAuth2AuthenticationHelper.GetOAuth2AuthenticationResult(oAuth2Configuration);
			var username = authResult?.Account?.Username;
			var senderInfo = username == null ? "(client secret)" : "From: " + username;
			senderProfile = $"with Graph API {senderInfo}";

			var graphClient = authResult.GetGraphServiceClient();
#if DEBUG
			if (ZArchitecture.Environment.Globals.IsTest && GraphServiceClientForTest != null)
			{
				graphClient = GraphServiceClientForTest;
			}
#endif

			#region Create message in draft mail box

			var userRequestBuilder = GraphUtils.GetUserRequestBuilder(graphClient, oAuth2Configuration, senderAddress);
			var createEmailUrl = userRequestBuilder.Messages.RequestUrl;
			var request = new HttpRequestMessage(HttpMethod.Post, createEmailUrl)
			{
				Content = new StringContent(content, Encoding.UTF8, "text/plain"),
			};
			graphClient.AuthenticationProvider.AuthenticateRequestAsync(request).GetResultByAwaiter();
			var httpResult = graphClient.HttpProvider.SendAsync(request).GetResultByAwaiter();

			var createdMessage = JsonConvert.DeserializeObject<JObject>(httpResult.Content.ReadAsStringAsync().GetResultByAwaiter());
			var createdMessageId = createdMessage["id"]?.ToString();

			#endregion
			try
			{
				#region Upload attachments

				//In graph API there is a batch way to combining multiple requests into a single JSON object (see https://docs.microsoft.com/en-us/graph/json-batching), but considering the limit of POST request body (1MB by default), we may get 413（Request Entity Too Large）if we use batch API to add attachments.
				//There may have a potential throttling issue with multiple instances of OMS. See outlook section in https://docs.microsoft.com/en-us/graph/throttling-limits.

				var attachments = mailItem.MailAttachments.OfType<MailAttachment>();
				foreach (var attachment in attachments)
				{
					if (IsAttachmentSizeExceedGraphLimit(attachment.MA_Data.Length))
					{
						throw new NotSupportedException("attachment size can not be larger than 150MB, see https://docs.microsoft.com/en-us/graph/outlook-large-attachments?tabs=csharp for details.");
					}
					if (mailItem.AttachmentIsReferredToInHTMLBody(attachment, out var attachmentFileName))
					{
						AttachInlineAttachments(attachment, createdMessageId, attachmentFileName, userRequestBuilder);
					}
					else
					{
						AttachFileAttachments(attachment, createdMessageId, userRequestBuilder);
					}
				}

				#endregion

				#region Send mail in draft folder

				userRequestBuilder.Messages[createdMessageId].Send().Request().PostAsync().GetResultByAwaiter();

				#endregion
			}
			catch
			{
				try
				{
					// Delete the new email form the drafts folder if it fails to send.
					userRequestBuilder.MailFolders.Drafts.Messages[createdMessageId].Request().DeleteAsync().GetResultByAwaiter();
				}
				catch (Exception ex)
				{
					logger?.Log(LogType.Warning, FormattableString.Invariant($"GraphMailSender_DeleteDraftEmailError: Failed to delete draft email. Email Subject: {mailItem.MI_Subject}"), ex);
				}

				throw;
			}
		}

		void AttachFileAttachments(MailAttachment mailAttachment, string messageIdAttachedTo, IUserRequestBuilder userRequestBuilder)
		{
			if (IsLargeSizeAttachment(mailAttachment.MA_Data.Length))
			{
				var attachmentItem = new AttachmentItem
				{
					AttachmentType = AttachmentType.File,
					Name = mailAttachment.MA_FileName,
					Size = mailAttachment.MA_Data.Length,
				};

				UploadAttachmentWithUploadSession(attachmentItem, messageIdAttachedTo, mailAttachment.MA_Data, userRequestBuilder);
			}
			else
			{
				var attachmentItem = new FileAttachment
				{
					Name = mailAttachment.MA_FileName,
					ContentBytes = mailAttachment.MA_Data
				};

				userRequestBuilder.Messages[messageIdAttachedTo].Attachments.Request().AddAsync(attachmentItem).GetResultByAwaiter();
			}
		}

		void AttachInlineAttachments(MailAttachment mailAttachment, string messageIdAttachedTo, string contentId, IUserRequestBuilder userRequestBuilder)
		{
			if (IsLargeSizeAttachment(mailAttachment.MA_Data.Length))
			{
				var attachmentItem = new AttachmentItem
				{
					AttachmentType = AttachmentType.File,
					Name = mailAttachment.MA_FileName,
					Size = mailAttachment.MA_Data.Length,
					IsInline = true,
					ContentId = contentId
				};

				UploadAttachmentWithUploadSession(attachmentItem, messageIdAttachedTo, mailAttachment.MA_Data, userRequestBuilder);
			}
			else
			{
				var attachmentItem = new FileAttachment
				{
					Name = mailAttachment.MA_FileName,
					IsInline = true,
					ContentId = contentId,
					ContentBytes = mailAttachment.MA_Data
				};
				userRequestBuilder.Messages[messageIdAttachedTo].Attachments.Request().AddAsync(attachmentItem).GetResultByAwaiter();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void UploadAttachmentWithUploadSession(AttachmentItem attachmentItem, string messageIdAttachedTo, ZBlob data, IUserRequestBuilder userRequestBuilder)
		{
			var uploadSession = userRequestBuilder.Messages[messageIdAttachedTo].Attachments
				.CreateUploadSession(attachmentItem)
				.Request()
				.PostAsync().GetResultByAwaiter();

			var uploadUrl = uploadSession.UploadUrl;
			var nextExpectedRange = uploadSession.NextExpectedRanges?.FirstOrDefault();

			using (var httpClient = new HttpClient())
			{
				HttpResponseMessage httpResult = null;
				while (!string.IsNullOrEmpty(nextExpectedRange))
				{
					var startIndex = nextExpectedRange == "0-" ? 0 : int.Parse(nextExpectedRange);
					var endIndex = Convert.ToInt32(Math.Min(startIndex + UploadBufferSize, attachmentItem.Size.Value)) - 1;
					var contentRange = $"bytes {startIndex}-{endIndex}/{attachmentItem.Size}";

					var content = new StreamContent(new MemoryStream(data, startIndex, endIndex - startIndex + 1));

					content.Headers.Add("Content-Type", "application/octet-stream");
					content.Headers.Add("Content-Range", contentRange);

					httpResult = httpClient.PutAsync(uploadUrl, content).GetResultByAwaiter(continueOnCapturedContext: true);
					uploadSession = JsonConvert.DeserializeObject<UploadSession>(httpResult.Content.ReadAsStringAsync().GetResultByAwaiter());

					nextExpectedRange = uploadSession?.NextExpectedRanges?.FirstOrDefault();
				}
			}
		}

		#endregion

		readonly Ms365OAuth2Configuration oAuth2Configuration;
		readonly string senderAddress;

		// we can only upload attachment less than 150 MB. https://docs.microsoft.com/en-us/graph/outlook-large-attachments?tabs=csharp
		bool IsAttachmentSizeExceedGraphLimit(double attachmentSizeInBytes) => attachmentSizeInBytes >= 150 * 1024 * 1024;

		// If an attachment is larger than 3MB, we should regard it as a large attachment. https://docs.microsoft.com/en-us/graph/outlook-large-attachments?tabs=csharp
		bool IsLargeSizeAttachment(double attachmentSizeInBytes) => attachmentSizeInBytes >= 3.0 * 1024 * 1024;

		//Set 2MB as a block size based on demo in https://docs.microsoft.com/en-us/graph/outlook-large-attachments?tabs=csharp
		readonly long UploadBufferSize = 2 * 1024 * 1024;

		string senderProfile;

		internal readonly ILogger logger;

		static readonly object sendMutex = new object();

#if DEBUG

		#region ForTestOnly

		internal GraphServiceClient GraphServiceClientForTest { get; set; }

		#endregion

#endif
	}
}
