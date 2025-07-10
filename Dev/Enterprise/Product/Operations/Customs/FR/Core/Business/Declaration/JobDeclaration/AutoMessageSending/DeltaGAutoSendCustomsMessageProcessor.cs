using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Interfaces;
using Enterprise.Customs.FR.Business.MessageSending;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaGAutoSendCustomsMessageProcessor : AutoSendCustomsMessageProcessor
	{
		public DeltaGAutoSendCustomsMessageProcessor(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override ZBool SendCustomsMessageCore(INotifications notifications, Customs.Business.CusEntryHeader entryHeader)
		{
			var sent = false;
			var errorCollector = new EU.Business.ErrorCollector();

			var messageSendingObject = new DeltaGJobDeclarationMessageSendingObject((CusEntryHeader)entryHeader);

			if (messageSendingObject.ShouldSend)
			{
				var rule = AutoSendCustomsMessageRules.FirstOrDefault(x => x.CanSendMessage((CusEntryHeader)entryHeader));

				if (rule != null)
				{
					var sender = new DeltaGMessageSender(messageSendingObject, errorCollector)
					{
						MessageDecorator = message => message.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(10),
						IsAutoSendCustomsMessageProcessor = true
					};
					messageSendingObject.MessageType = rule.newMessageType;

					var result = sender.SendAndThrowExceptionIfAny();
					if (errorCollector.ErrorCount == 0)
					{
						sent = true;
					}
					else
					{
						notifications.Add(NotificationType.Error, result);
					}
				}
			}
			return sent;
		}

		protected override IEnumerable<IAutoSendCustomsMessageRule> AutoSendCustomsMessages
		{
			get
			{
				yield return new SendEAVMessageWhenStatusIs055();
				yield return new SendVALMessageWhenStatusIsEmpty();
			}
		}
	}
}
