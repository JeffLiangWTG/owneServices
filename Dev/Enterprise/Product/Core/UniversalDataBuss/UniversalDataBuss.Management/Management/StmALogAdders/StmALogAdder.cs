using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Event = Enterprise.ZArchitecture.Business.Event;
using EventDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.UniversalDataBuss.Management
{
	internal class StmALogAdder
	{
		internal StmALogAdder(Event eventType,
			EventDataObject eventDataObject,
			IEventDataContextManager dataContextManager,
			UniversalObjectFactory factory,
			ISimpleLogger logger)
		{
			this.eventType = Argument.NotNull(eventType, "Event eventType");
			this.EventDataObject = eventDataObject;
			this.dataContextManager = dataContextManager;
			this.factory = factory;
			this.logger = logger;
		}

		readonly Event eventType;
		public readonly EventDataObject EventDataObject;
		readonly IEventDataContextManager dataContextManager;
		readonly UniversalObjectFactory factory;
		readonly ISimpleLogger logger;

		internal void AddNewLogToParent(BusinessObject logParent)
		{
			AddNewLogToParent((IStmALogParent)logParent);
		}

		internal void AddNewLogToParent(IStmALogParent logParent)
		{
			var newEvent = AddNewLogToParentCore(logParent);
			factory.RecordEndOfEveryRead(newEvent);
		}

		protected virtual StmALog AddNewLogToParentCore(IStmALogParent logParent)
		{
			EventValue eventInfo;
			if (EventDataObject != null)
			{
				var parameters = EventParameters.GetEventParameters(EventDataObject.EventParameters, EventDataObject.EventReference);

				ConvertFlightDateToISO8601ShortDateString(parameters);
				ConvertLocationFrom3LettersCodeTo5LettersCode(parameters, logParent.Factory);

				var eventTime = EventDataObject.EventTime.GetValueOrDefault();
				var eventTimeZDateTimeOffset = eventTime.IsZDateTime ? eventTime.ToZDateTimeOffsetWithTheGivenOffsetIfNoneIsPresent(ZDateTimeOffset.Now.Offset) : eventTime.ToZDateTimeOffset();

				eventInfo = new EventValue(eventType,
					isEstimate: EventDataObject.IsEstimate.GetValueOrDefault(),
					eventTime: eventTimeZDateTimeOffset,
					reference: EventDataObject.EventReference.GetValueOrDefault(),
					parameters: parameters);
			}
			else
			{
				eventInfo = new EventValue(eventType);
			}

			var eventTransformer = this.dataContextManager as IEventTransformer;
			if (eventTransformer != null)
			{
				eventInfo = eventTransformer.Transform(eventInfo, EventDataObject, logParent);
			}

			return logParent.Logs.AddNew(eventInfo);
		}

		static void ConvertLocationFrom3LettersCodeTo5LettersCode(IDictionary<string, string> parameters, BusinessObjectFactory factory)
		{
			if (parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Location, out var loc) && loc.Length == 3)
			{
				parameters[Constants.EventReferenceParameters.Codes.Location] = factory.GetUNLocoFromSuppliedLocationCode(loc);
			}
		}

		void ConvertFlightDateToISO8601ShortDateString(IDictionary<string, string> parameters)
		{
			var flightDateFound = false;
			if (EventDataObject.EventParameters != null)
			{
				var val = EventDataObject.EventParameters.GetEventParameter(Constants.EventReferenceParameters.Codes
					.FlightDate);
				if (val != null && val is ZDateTime dateTime)
				{
					parameters[Constants.EventReferenceParameters.Codes.FlightDate] = dateTime.ToISO8601ShortDateString();
					flightDateFound = true;
				}
			}

			if (!flightDateFound && parameters.TryGetValue(Constants.EventReferenceParameters.Codes.FlightDate, out var dateStr))
			{
				var date = new ZDateTimeParser().TryParseAndValidate(dateStr, typeof(ZDateTime), "FlightDate", logger);
				if (date != null && date is ZDateTime dateTime2)
				{
					parameters[Constants.EventReferenceParameters.Codes.FlightDate] = dateTime2.ToISO8601ShortDateString();
				}
			}
		}
	}
}
