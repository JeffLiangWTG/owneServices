using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class JXCMessageProcessorFactory
	{
		virtual
 public JXCMessageProcessor NewProcessor(JXCRecord[] records, INotifications notificationSubscriber)
		{
			JXCMessageProcessor result = null;

			if (records != null && records.Length > 2)
			{
				JXCRecord firstBodyRecord = records[1];
				ZString messageType = firstBodyRecord.LineType;
				switch (messageType)
				{
					case JXCConstants.MessageTypes.MAWB:
						result = new MAWBMessageProcessor(records);
						break;

					case JXCConstants.MessageTypes.DAWB:
						result = new DAWBMessageProcessor(records);
						break;

					case JXCConstants.MessageTypes.CHAB:
						result = new CHABMessageProcessor(records);
						break;

					case JXCConstants.MessageTypes.PSAB:
						result = new PSABMessageProcessor(records);
						break;

					case JXCConstants.MessageTypes.OMAN:
						result = new OMANMessageProcessor(records);
						break;

					case JXCConstants.MessageTypes.PSBL:
						result = new PSBLMessageProcessor(records);
						break;

					case JXCConstants.MessageTypes.COHB:
						result = new COHBMessageProcessor(records);
						break;

					default:
						NotifyIgnoredMessageType(notificationSubscriber, messageType);
						break;
				}
			}
			else
			{
				NotifyInvalidJXCFile(notificationSubscriber);
			}

			return result;
		}

		void NotifyInvalidJXCFile(INotifications notificationSubscriber)
		{
			ErrorNotification error = new ErrorNotification(ErrorType.InvalidFileFormat, "JXC file is invalid");
			notificationSubscriber.Notify(error);
		}

		void NotifyIgnoredMessageType(INotifications notificationSubscriber, ZString messageType)
		{
			string warningMessage = messageType + " message is not supported. The system is ignoring this message.";
			WarningNotification warning = new WarningNotification(warningMessage);
			notificationSubscriber.Notify(warning);
		}
	}
}
