using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business.MessageProcessors
{
	public abstract class ElectronicFormEventLogManager<TResponse> where TResponse : class
	{
		public Event DetermineEventAndLog(TResponse response, EnterpriseBusinessObject enterpriseBusinessObject)
		{
			var eventType = DetermineEventType(response, enterpriseBusinessObject);
			if (eventType == null)
			{
				return null;
			}

			LogEventWithParameters(response, enterpriseBusinessObject, eventType);

			return eventType;
		}

		public void LogEventWithParameters(TResponse response, EnterpriseBusinessObject enterpriseBusinessObject, Event eventType)
		{
			enterpriseBusinessObject.Logs.AddNew(
							eventType: eventType,
							parameters: GetEventParams(enterpriseBusinessObject, eventType, response).ToArray(),
							dateTime: ZDateTimeOffset.Now);
		}

		protected bool IsWithdrawResponse(EnterpriseBusinessObject enterpriseBusinessObject)
		{
			((ILogsInternals)enterpriseBusinessObject.Logs).ReloadFromDB();
			var extraQuery = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"MST={MessageType}");
			var eventMessageWithdrawCancelRequest = enterpriseBusinessObject.Logs.MostRecentLogByEventTime(Events.MessageWithdrawCancelRequest, extraQuery);
			if (eventMessageWithdrawCancelRequest == null)
			{
				return false;
			}

			var eventMessageSent = enterpriseBusinessObject.Logs.MostRecentLogByEventTime(Events.MessageSent, extraQuery);
			return eventMessageWithdrawCancelRequest.EventTimeOffset > eventMessageSent.EventTimeOffset;
		}

		protected abstract string GetReferenceNumberEventParameter(EnterpriseBusinessObject enterpriseBusinessObject);

		protected abstract Event DetermineEventType(TResponse response, EnterpriseBusinessObject enterpriseBusinessObject);

		protected abstract string GetResponseEventParameter(Event eventType, TResponse response);

		protected abstract string MessageType { get; }

		Dictionary<string, string> GetEventParams(EnterpriseBusinessObject enterpriseBusinessObject, Event eventType, TResponse response)
		{
			var dictionary = new Dictionary<string, string>();
			dictionary["DEP"] = ILMessageEventParameter.Department;
			dictionary["MST"] = MessageType;
			if (GetReferenceNumberEventParameter(enterpriseBusinessObject) is string reference

				&& !reference.IsEmpty())
			{
				dictionary["RFN"] = reference;
			}
			if (GetResponseEventParameter(eventType, response) is string paramRes
				&& !paramRes.IsEmpty())
			{
				dictionary["RES"] = paramRes;
			}

			return dictionary;
		}
	}
}
