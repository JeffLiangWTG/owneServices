using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Testing
{
	public static class UniversalEventCreator
	{
		public static Event Create(IDataContextDataObject dataContext, string eventCode)
		{
			var eventObject = new Event();
			eventObject.DataContext = dataContext;
			eventObject.EventType = eventCode;
			eventObject.EventTime = ZDateTimeOffset.Now;
			eventObject.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			return eventObject;
		}

		public static void SetAttachedDocumentCollection(Event @event, params AttachedDocument[] docs)
		{
			@event.AttachedDocumentCollection = docs.ToList();
		}
	}
}
