using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Moq;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class GraphServiceClientForSenderTest : GraphServiceClient
	{
		public GraphServiceClientForSenderTest(IAuthenticationProvider authenticationProvider, IHttpProvider httpProvider = null) : base(authenticationProvider, httpProvider)
		{
			var authenticationProvider1 = new Mock<IAuthenticationProvider>();
			authenticationProvider1.Setup(a => a.AuthenticateRequestAsync(It.IsAny<HttpRequestMessage>())).Returns(Task.CompletedTask);
		}

		public bool IsDeletedExecuted { get; private set; }

		IUserRequestBuilder me;
		public override IUserRequestBuilder Me
		{
			get
			{
				if (me != null)
				{
					return me;
				}

				var messageRequestBuilder = MessageRequestBuilder();

				var userMessagesCollectionRequestBuilder = new Mock<IUserMessagesCollectionRequestBuilder>();
				userMessagesCollectionRequestBuilder.Setup(m => m.RequestUrl).Returns("https://graph.microsoft.com/me/messages");

				AddError(userMessagesCollectionRequestBuilder, messageRequestBuilder);

				var mailFolders = GetMailFolders(messageRequestBuilder);

				var mock = new Mock<IUserRequestBuilder>();
				me = mock.Object;
				mock.Setup(u => u.Messages).Returns(userMessagesCollectionRequestBuilder.Object);
				mock.Setup(u => u.MailFolders).Returns(mailFolders);

				return me;
			}
		}

		public override IGraphServiceUsersCollectionRequestBuilder Users
		{
			get
			{
				var messageRequestBuilder = MessageRequestBuilder();

				var userMessagesCollectionRequestBuilder = new Mock<IUserMessagesCollectionRequestBuilder>();
				userMessagesCollectionRequestBuilder.Setup(m => m.RequestUrl).Returns("https://graph.microsoft.com/v1.0/users/test@email.com/messages");

				AddError(userMessagesCollectionRequestBuilder, messageRequestBuilder);

				var mailFolders = GetMailFolders(messageRequestBuilder);

				var userRequestBuilder = new Mock<IUserRequestBuilder>();
				userRequestBuilder.Setup(u => u.RequestUrl).Returns("https://graph.microsoft.com/v1.0/users/test@email.com");
				userRequestBuilder.Setup(u => u.Messages).Returns(userMessagesCollectionRequestBuilder.Object);
				userRequestBuilder.Setup(u => u.MailFolders).Returns(mailFolders);

				var users = new Mock<IGraphServiceUsersCollectionRequestBuilder>();
				users.Setup(u => u[It.IsAny<string>()]).Returns(userRequestBuilder.Object);

				return users.Object;
			}
		}

		public Exception ExceptionForDelete { get; set; }

		IUserMailFoldersCollectionRequestBuilder GetMailFolders(IMessageRequestBuilder messageRequestBuilder)
		{
			var mailFolderMessages = new Mock<IMailFolderMessagesCollectionRequestBuilder>();
			mailFolderMessages.Setup(m => m[It.IsAny<string>()]).Returns(messageRequestBuilder);

			var mailFolder = new Mock<IMailFolderRequestBuilder>();
			mailFolder.Setup(m => m.Messages).Returns(mailFolderMessages.Object);

			var mailFolders = new Mock<IUserMailFoldersCollectionRequestBuilder>();
			mailFolders.Setup(m => m.Drafts).Returns(mailFolder.Object);

			return mailFolders.Object;
		}

		void AddError(Mock<IUserMessagesCollectionRequestBuilder> userMessagesCollectionRequestBuilder, IMessageRequestBuilder messageRequestBuilder)
		{
			var err = new Error();
			err.Code = "ErrorSendAsDenied";
			err.Message = "The user account which was used to submit this request does not have the right to send mail as the specified sending account., Cannot submit message.";

			userMessagesCollectionRequestBuilder.Setup(m => m[It.Is<string>(s => s.Equals("UniqueValidMessageID"))]).Throws(new ServiceException(err));
			userMessagesCollectionRequestBuilder.Setup(m => m[It.Is<string>(s => !s.Equals("UniqueValidMessageID"))]).Returns(messageRequestBuilder);
		}

		IMessageRequestBuilder MessageRequestBuilder()
		{
			#region Message Send Request Post Mock

			var sendRequest = new Mock<IMessageSendRequest>();
			sendRequest.Setup(s => s.PostAsync(default)).Returns(Task.CompletedTask);

			var sendRequestBuilder = new Mock<IMessageSendRequestBuilder>();
			sendRequestBuilder.Setup(s => s.Request(default)).Returns(sendRequest.Object);

			#endregion

			#region MIME Message

			Stream mimeStream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

			var contentRequestBuilder = new Mock<IMessageContentRequestBuilder>();
			contentRequestBuilder.Setup(c => c.Request(default).GetAsync(default, default)).Returns(Task.FromResult(mimeStream));

			var messageRequestBuilder = new Mock<IMessageRequestBuilder>();
			messageRequestBuilder.Setup(m => m.Content).Returns(contentRequestBuilder.Object);
			messageRequestBuilder.Setup(m => m.Send()).Returns(sendRequestBuilder.Object);

			var messageRequest = new Mock<IMessageRequest>();
			if (ExceptionForDelete != null)
			{
				messageRequest.Setup(m => m.DeleteAsync(default)).Throws(ExceptionForDelete);
			}
			else
			{
				messageRequest.Setup(m => m.DeleteAsync(default)).Returns(new Func<CancellationToken, Task>(c =>
				{
					IsDeletedExecuted = true;
					return Task.CompletedTask;
				}));
			}

			messageRequestBuilder.Setup(m => m.Request()).Returns(messageRequest.Object);

			#endregion

			return messageRequestBuilder.Object;
		}
	}
}
