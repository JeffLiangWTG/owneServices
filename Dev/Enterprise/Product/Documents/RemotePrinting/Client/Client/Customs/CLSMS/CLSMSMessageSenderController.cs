using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using CargoWise.eHub.Common.Extensions;
using Enterprise.xTMessaging.Shared;
using Xware.Xt.Grpc.Application;

namespace Enterprise.RemotePrinting.Client
{
	public class CLSMSMessageSenderController : CustomsMessageController
	{
		public CLSMSMessageSenderController(string localMachineName, CancellationToken cancellationToken) : base(localMachineName, cancellationToken)
		{
		}

		protected override CustomseHubClientSettingManager CreateNewSettingManager(string machineName)
		{
			return new CLSMSClientApplicationSettingManager(machineName, WebServiceClient);
		}

		protected override int ProcessCore(ICustomseHubClientSetting setting)
		{
			int numberOfMessages = 0;

			if (setting is ICLSMSClientApplicationSetting clSetting)
			{
				var msgClientProvider = CLSMSHelper.GetMsgClientProvider(clSetting);
				var receiveHandler = new CLReceiveHandler(clSetting.SendFolder, this);

				using (var connector = new DirectxTConnector(msgClientProvider, new CLDirectxTMessagingConfig(), new XTLogger(OnShowInformation), null, receiveHandler))
				{
					connector.InitializeIfNeeded();
					connector.Receive();
				}
			}

			return numberOfMessages;
		}
	}

	public class CLReceiveHandler : IReceiveHandler
	{
		public CLReceiveHandler(string folderPath, INotifications notifications)
		{
			this.folderPath = folderPath;
			this.notifications = notifications;
			this.fileFormatChecker = new MessageFileFormatChecker(notifications);
		}

		readonly string folderPath;
		readonly INotifications notifications;
		readonly MessageFileFormatChecker fileFormatChecker;

		public IDictionary<MsgIdUri, MessageHandlingResult> MessageProcessingResults;

		IDictionary<MsgIdUri, MessageHandlingResult> IReceiveHandler.MessageProcessingResults => MessageProcessingResults;

		public void HandleReceivedMessageBatch(ICollection<MsgIdUri> msgIds, GetMessageMetaDataForHandling getMessageMetaDataForHandling, GetMsgData getMsgData, LoadReplyIntoMemory loadReplyIntoMemory)
		{
			MessageProcessingResults = new Dictionary<MsgIdUri, MessageHandlingResult>();

			foreach (var msgId in msgIds)
			{
				var result = false;
				var metaDataDictionary = getMessageMetaDataForHandling(msgId);
				var fileName = "";
				if (metaDataDictionary.TryGetValue("custom.FileName", out fileName))
				{
					using (var msgData = getMsgData(new GetMsgDataMessage { Id = msgId }))
					{
						var resultStream = loadReplyIntoMemory(msgData);

						if (fileFormatChecker.CheckIsXMLDocument(resultStream))
						{
							var newFileName = Path.Combine(folderPath, fileName);
							if (!File.Exists(newFileName))
							{
								WriteToLocalAsUTF8Xml(resultStream, newFileName);
								result = true;
								ShowNotificationMessage($"Message sent successfully, message id: {msgId}, file name: {newFileName}.");
							}
							else
							{
								ShowNotificationMessage($"When write xml messsage to file into folder:{folderPath}, file already exists, file Name: {newFileName}.");
							}
						}
					}
				}
				else
				{
					ShowNotificationMessage($"The message lost file name, message id: {msgId}.");
				}

				if (!MessageProcessingResults.TryGetValue(msgId, out var receiveProcessingResult))
				{
					MessageProcessingResults.Add(msgId, new MessageHandlingResult(result ? xTMessaging.Shared.Constants.MessageHandlingResultOperation.Success : xTMessaging.Shared.Constants.MessageHandlingResultOperation.Error, 0));
				}
			}

			void ShowNotificationMessage(string message)
			{
				notifications?.AddMessage(message);
			}
		}

		void WriteToLocalAsUTF8Xml(Stream stream, string fileName)
		{
			stream.SeekBegin();
			var document = XDocument.Load(stream);
			using (var sw = new StreamWriter(fileName, false, new UTF8Encoding(false)))
			{
				document.Save(sw);
			}
		}
	}
}
