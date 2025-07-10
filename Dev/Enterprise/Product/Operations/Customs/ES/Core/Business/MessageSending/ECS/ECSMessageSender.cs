using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.MessageSending.ECSMessageSender;

namespace Enterprise.Customs.ES.Business.MessageSending
{
	public class ECSMessageSender : GenericMessageSender<MessageBuilderData, ECSExitHeaderMessageSendingObject>
	{
		public ECSMessageSender(ECSExitHeaderMessageSendingObjectParent sendingObjectParent)
		{
			this.sendingObjectParent = Argument.NotNull(sendingObjectParent, nameof(sendingObjectParent));
			messagesSent = ZInt.Zero;
			messagesWithCreateFailure = ZInt.Zero;
			messagesWithSendFailure = ZInt.Zero;
		}
		readonly ECSExitHeaderMessageSendingObjectParent sendingObjectParent;

		public ZInt messagesSent;
		public ZInt messagesWithCreateFailure;
		public ZInt messagesWithSendFailure;

		public override List<MessageBuilderData> GetMessageBuildersData()
		{
			var messageBuilders = new List<MessageBuilderData>();
			var certificateData = sendingObjectParent.CertificateData;
			foreach (var objectToSend in sendingObjectParent.SelectedSendingObjects
				.Cast<ECSExitHeaderMessageSendingObject>())
			{
				messageBuilders.AddRange(GetIndividualMessageBuilder(objectToSend, certificateData));
			}

			return messageBuilders;
		}

		protected override List<MessageBuilderData> GetIndividualMessageBuilder(ECSExitHeaderMessageSendingObject objectToSend, ICertificateProvider certificateData)
		{
			Argument.NotNull(objectToSend, "objectToSend cannot be null");
			var exitDetail = objectToSend.ExitDetail;

			var messageBuilders = new List<MessageBuilderData>();

			AddMessageBuilder(messageBuilders, objectToSend, exitDetail, certificateData);

			return messageBuilders;
		}

		void AddMessageBuilder(List<MessageBuilderData> messageBuilders, ECSExitHeaderMessageSendingObject objectToSend, CusExitDetail exitDetail, ICertificateProvider certificateData)
		{
			try
			{
				var builderManager = new ECSMessageBuilderManager(objectToSend, certificateData);

				messageBuilders.Add(new MessageBuilderData
				{
					MessageBuilder = builderManager.NewMessageBuilder(),
					CusExitDetail = exitDetail
				});
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				messagesWithSendFailure++;
				ErrorReporter.ReportOnce("ECSMessageSender.GetIndividualMessageBuilder", "Exception thrown when trying to create builder to send declaration", ex);
			}
		}

		public void Send(List<MessageBuilderData> messageBuildersToSend)
		{
			foreach (var objectToSend in messageBuildersToSend)
			{
				SendIndividualDeclaration(objectToSend);
			}
		}

		void SendIndividualDeclaration(MessageBuilderData objectToSend)
		{
			var exitDetail = objectToSend.CusExitDetail;

			var previousCEDStatus = exitDetail?.CED_Status ?? ZString.Empty;
			var initialMessagesSent = messagesSent;
			try
			{
				var message = TrySendDeclaration(objectToSend);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				messagesWithSendFailure++;
				ErrorReporter.ReportOnce("ECSMessageSender.SendIndividualDeclaration", "Exception thrown when trying to send declaration", ex);
			}
			finally
			{
				CleanUpDatabaseAfterFailure(exitDetail, previousCEDStatus, initialMessagesSent);
			}
		}

		ESEDIMessage TrySendDeclaration(MessageBuilderData objectToSend)
		{
			var messageBuilderToSend = Argument.NotNull(objectToSend.MessageBuilder, "messageBuilderToSend cannot be null");
			var exitDetail = Argument.NotNull(objectToSend.CusExitDetail, "exitDetail cannot be null");

			var factory = exitDetail.Factory;

			var messageCreator = new EDIMessageCreator(messageBuilderToSend, factory);
			var message = messageCreator.CreateMessage();
			message.EM_LinkedObject = exitDetail;

			exitDetail.CED_Status = MessageStatusList.Codes.AwaitingResponse;
			messagesSent++;

			return message;
		}

		void CleanUpDatabaseAfterFailure(CusExitDetail exitDetail, ZString previousCEDStatus, ZInt initialMessagesSent)
		{
			if (messagesSent == initialMessagesSent && exitDetail != null)
			{
				exitDetail.CED_Status = previousCEDStatus;
			}
		}

		public class MessageBuilderData
		{
			public IMessageBuilderBase MessageBuilder;
			public CusExitDetail CusExitDetail;
		}
	}
}
