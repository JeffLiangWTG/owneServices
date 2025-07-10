using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.ExitControl.Business.ExitControlMessageSender;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class ExitControlMessageSender : GenericMessageSender<MessageBuilderData, ExitControlMessageSendingObject>
	{
		public ExitControlMessageSender(ExitControlMessageSendingObjectParent sendingObjectParent)
		{
			this.sendingObjectParent = Argument.NotNull(sendingObjectParent, nameof(sendingObjectParent));
			messagesSent = ZInt.Zero;
			messagesWithCreateFailure = ZInt.Zero;
			messagesWithSendFailure = ZInt.Zero;
		}

		protected readonly ExitControlMessageSendingObjectParent sendingObjectParent;

		public ZInt messagesSent;
		public ZInt messagesWithCreateFailure;
		public ZInt messagesWithSendFailure;

		public override List<MessageBuilderData> GetMessageBuildersData()
		{
			var messageBuilders = new List<MessageBuilderData>();
			var certificateData = sendingObjectParent.CertificateData;
			foreach (var objectToSend in sendingObjectParent.SelectedSendingObjects
				.Cast<ExitControlMessageSendingObject>())
			{
				messageBuilders.AddRange(GetIndividualMessageBuilder(objectToSend, certificateData));
			}

			return messageBuilders;
		}

		protected override List<MessageBuilderData> GetIndividualMessageBuilder(ExitControlMessageSendingObject objectToSend, ICertificateProvider certificateData)
		{
			Argument.NotNull(objectToSend, "objectToSend cannot be null");
			var exitReport = objectToSend.MessagingObject;

			var messageBuilders = new List<MessageBuilderData>();

			AddMessageBuilder(messageBuilders, objectToSend, exitReport, certificateData);

			return messageBuilders;
		}

		void AddMessageBuilder(List<MessageBuilderData> messageBuilders, ExitControlMessageSendingObject objectToSend, CusExitReport exitReport, ICertificateProvider certificateData)
		{
			try
			{
				var builderManager = new ExitControlMessageBuilderManager(objectToSend, certificateData);

				messageBuilders.Add(new MessageBuilderData
				{
					MessageBuilder = builderManager.NewMessageBuilder(),
					CusExitReport = exitReport,
					ObjectToSend = objectToSend,
				});
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				messagesWithSendFailure++;
				ErrorReporter.ReportOnce("ExitControlMessageSender.GetIndividualMessageBuilder", "Exception thrown when trying to create builder to send declaration", ex);
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
			var exitReport = objectToSend.CusExitReport;

			var previousMessageStatus = exitReport?.CER_MessageStatus ?? ZString.Empty;
			var initialMessagesSent = messagesSent;
			try
			{
				var message = TrySendDeclaration(objectToSend);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				messagesWithSendFailure++;
				ErrorReporter.ReportOnce("ExitControlMessageSender.SendIndividualDeclaration", "Exception thrown when trying to send declaration", ex);
			}
			finally
			{
				CleanUpDatabaseAfterFailure(exitReport, previousMessageStatus, initialMessagesSent);
			}
		}

		ESEDIMessage TrySendDeclaration(MessageBuilderData objectToSend)
		{
			var messageBuilderToSend = Argument.NotNull(objectToSend.MessageBuilder, "messageBuilderToSend cannot be null");
			var exitReport = Argument.NotNull(objectToSend.CusExitReport, "exitReport cannot be null");

			var factory = exitReport.Factory;

			var messageCreator = new EDIMessageCreator(messageBuilderToSend, factory);
			var message = messageCreator.CreateMessage();
			message.EM_LinkedObject = exitReport;

			exitReport.CER_MessageStatus = LogicalStatusList.Codes.Sent;
			messagesSent++;

			return message;
		}

		void CleanUpDatabaseAfterFailure(CusExitReport exitReport, ZString previousMessageStatus, ZInt initialMessagesSent)
		{
			if (messagesSent == initialMessagesSent && exitReport != null)
			{
				exitReport.CER_MessageStatus = previousMessageStatus;
			}
		}

		public class MessageBuilderData
		{
			public IMessageBuilderBase MessageBuilder;
			public CusExitReport CusExitReport;
			public ExitControlMessageSendingObject ObjectToSend;
		}
	}
}
