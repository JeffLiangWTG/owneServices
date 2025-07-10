using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class ExitControlMessageSendingObjectParent : EU.ExitControl.Business.ExitControlMessageSendingObjectParent
	{
		public ExitControlMessageSendingObjectParent(EU.ExitControl.Business.CusExitHeader cusExitHeader) : base(cusExitHeader)
		{
		}

		public int SendAndSaveMessages()
		{
			var messagesSent = 0;

			SelectedSendingObjects.Cast<ExitControlMessageSendingObject>()
				.Select(action => GetMessageSender(action))
				.Where(sender => sender != null)
				.ForEach(sender =>
				{
					sender.Send();
					messagesSent++;
				});
			if (messagesSent > 0)
			{
				try
				{
					Factory.Save();
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
					messagesSent = 0;
				}
			}

			return messagesSent;
		}

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

		readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new[]
		{
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.Type), true, 80),
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.TransportID), true, 200),
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.Location), true, 200),
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.DateTime), true, 200),
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.ExitOffice), true, 80),
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.MRN_LRN), true, 80),
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.CustomsStatus), true, 80),
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.MessageStatus), true, 80)
		};

		static ExitControlMessageSender GetMessageSender(ExitControlMessageSendingObject action)
		{
			switch (action.Type)
			{
				case DEExitReportTypeList.Codes.Anticipation:
					return new EXTANTMessageSender(action);

				case DEExitReportTypeList.Codes.Presentation:
					return new EXTPREMessageSender(action);

				case DEExitReportTypeList.Codes.Transfer:
					return new EXTINFMessageSender(action);

				case DEExitReportTypeList.Codes.ExitNotification:
					return new EXTNOTMessageSender(action);

				default:
					return null;
			}
		}
	}
}
