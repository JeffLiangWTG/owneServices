using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Res = Enterprise.Messaging.Business.Res;

namespace Enterprise.Messaging.MessageProcessors
{
	public class ResponseMessageAbnormalityReporter
	{
		public delegate bool IsMessageClear();
		public delegate ZString GetMessageDelegate(EDIMessage message);
		public GetMessageDelegate GetMessageOverride;

		public delegate ZString GetReportKeyDelegate(ZString originalKey, EDIMessage message);
		public GetReportKeyDelegate GetReportKeyOverride;

		public void Report(BusinessObject topLevelBizObj, EDIMessage originalMessage, EDIMessage responseMessage, IsMessageClear isMessageClear)
		{
			if (originalMessage != null && isMessageClear != null && topLevelBizObj != null)
			{
				if (IsMessageClearedWhenOriginalMessageWasSendWithErrors(originalMessage, isMessageClear))
				{
					ReportMessageWithAllTheOriginalErrors(topLevelBizObj, originalMessage, responseMessage);
				}
				else if (IsMessageNotClearedWhenItShouldHaveBeenCleared(originalMessage, isMessageClear))
				{
					ReportMessageNotClearedWhenItShouldHaveBeenCleared(topLevelBizObj, originalMessage, responseMessage);
				}
			}
		}

		void ReportMessageNotClearedWhenItShouldHaveBeenCleared(BusinessObject topLevelBizObj, EDIMessage originalMessage, EDIMessage responseMessage)
		{
			string errorMessage = Res.GetString("ba5cd5fd-c33f-4f3f-af9a-c3b078bd633b", "{2}\r\nOriginal Message:\r\n{0}\r\nResponse Message:\r\n{1}",
					GetMessage(originalMessage),
					GetMessage(responseMessage),
					topLevelBizObj.GetType().FullName);
			ErrorReporter.ReportOnce(GetReportKey(NotClearedWhenMessageWasSendWithoutErrorKey, responseMessage), errorMessage);
		}

		public static string NotClearedWhenMessageWasSendWithoutErrorKey
		{
			get { return Res.GetString("9a919173-6a74-4454-a23b-98fad3f8b087", "Not Cleared When Message Was Send Without Error"); }
		}

		void ReportMessageWithAllTheOriginalErrors(BusinessObject topLevelBizObj, EDIMessage originalMessage, EDIMessage responseMessage)
		{
			topLevelBizObj.LoadChildEditableObjects();
			topLevelBizObj.RunPreSaveValidation();
			ZString errorsInOriginalMessage = topLevelBizObj.Notifications.GetMessageErrors().ToUniqueMessageListString();
			if (errorsInOriginalMessage.IsEmpty)
			{
				// This whole error reporter has been invoked in error if the top level bizO has no errors.  Do not report anything. 
				// The error report will send CW nothing useful if all you see is an positive inbound message, and outbound message, and a BLANK list of alleged errors. For example: see Issue 00170996, error E00013677.
				return;
			}

			string errorMessage = Res.GetString("841488de-2857-44c2-af32-8cf221cd7893", "{3}\r\nMessage Errors:\r\n{0}\r\nOriginal Message:\r\n{1}\r\nResponse Message:\r\n{2}",
					errorsInOriginalMessage,
					GetMessage(originalMessage),
					GetMessage(responseMessage),
					topLevelBizObj.GetType().FullName);
			ErrorReporter.ReportOnce(GetReportKey(ClearedWhenMessageWasSendWithErrorKey, responseMessage), errorMessage);
		}

		public static string ClearedWhenMessageWasSendWithErrorKey
		{
			get { return Res.GetString("9077a467-08d0-4c6b-8145-28bdcd5b07bc", "Cleared When Message Was Send With Error"); }
		}

		ZString GetMessage(EDIMessage message)
		{
			return GetMessageOverride != null ? GetMessageOverride(message) : message.EM_MessageInterpretation;
		}

		ZString GetReportKey(ZString originalKey, EDIMessage message)
		{
			return GetReportKeyOverride != null ? GetReportKeyOverride(originalKey, message) : originalKey;
		}

		bool IsMessageNotClearedWhenItShouldHaveBeenCleared(EDIMessage originalMessage, IsMessageClear isMessageClear)
		{
			return (!originalMessage.EM_SendWithMessageErrors && !isMessageClear());
		}

		bool IsMessageClearedWhenOriginalMessageWasSendWithErrors(EDIMessage originalMessage, IsMessageClear isMessageClear)
		{
			return (originalMessage.EM_SendWithMessageErrors && isMessageClear());
		}
	}
}
