using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using UniversalEventDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Universal
{
	internal static class UniversalDataEventExtensions
	{
		public static UniversalEventDataObject AddToContextCollection(
			this UniversalEventDataObject eventDataObject,
			string typeName,
			string contextValue)
		{
			if (eventDataObject.ContextCollection == null)
			{
				eventDataObject.ContextCollection = new List<Context>();
			}

			var contextType = new ContextType()
			{
				Type = typeName
			};

			eventDataObject.ContextCollection.Add(new Context()
			{
				Type = contextType,
				Value = contextValue
			});

			return eventDataObject;
		}

		public static UniversalEventDataObject SetRejectedEventType(
			this UniversalEventDataObject eventDataObject,
			string companyCode,
			string messageSubType)
		{
			eventDataObject.EventType = AutoEvents.InterchangeRejectedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = companyCode,
				Reason = (NoResString)"This is the rejection reason from eHub.", // This is a test value
				MessageSubType = messageSubType,
			};

			return eventDataObject;
		}

		public static UniversalEventDataObject SetAcknowledgedEventType(
			this UniversalEventDataObject eventDataObject,
			string companyCode,
			string messageSubType)
		{
			eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = companyCode,
				MessageSubType = messageSubType,
			};

			return eventDataObject;
		}
	}
}
