using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.JP.Common
{
	public static class IStmALogExtensions
	{
		public static class Reference
		{
			public const string SendingWithOverriddenValues = "Sending with overridden values|EDIMessage Number={0}";
		}

		public static void AddEventWithReference(this IStmALogProvider logProvider, Event eventType, ZBlob messageData)
		{
			var reference = JPMessageUtils.ConvertMessageToString(JPMessageUtils.ExtractHeader(messageData));
			AddEventWithReference(logProvider, eventType, reference);
		}

		public static void AddEventWithReference(this IStmALogProvider logProvider, Event eventType, ZString reference)
		{
			logProvider.Logs.AddNew(eventType, reference);
		}

		public static void AddEventIgnoringMsgNum(this IStmALogProvider logProvider, Event eventType, EDIMessage naccsMessage)
		{
			if (naccsMessage.EM_MessageNum.IsEmpty)
			{
				naccsMessage.EM_MessageNumInfo.ValueChanged += (s, e) =>
				{
					AddEventWithReference(logProvider, eventType, naccsMessage.EM_MessageNum);
				};
			}
			else
			{
				AddEventWithReference(logProvider, eventType, naccsMessage.EM_MessageNum);
			}
		}
	}
}
