using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	class Event_IXmlEventValueObjectTest : TestCaseWithFactory
	{
		public void TestEventContextTypes_IsKeptInSnycWith_IXmlEventValueObjectContextValueList()
		{
			CombineAssertions(delegate
			{
				foreach (var propertyInfo in typeof(IXmlEventValueObjectContextValueList).GetProperties())
				{
					if (typeof(IZType).IsAssignableFrom(propertyInfo.PropertyType))
					{
						Assert("Constant with matching name [" + propertyInfo.Name + "] must exist on enum Event.ContextTypes.", Enum.IsDefined(typeof(Event.ContextTypes), propertyInfo.Name));
					}
				}

				foreach (Event.ContextTypes enumConstant in Enum.GetValues(typeof(Event.ContextTypes)))
				{
					var contextPropertyName = $"{enumConstant.ToString()}{(enumConstant == Event.ContextTypes.ContainerNumber || enumConstant == Event.ContextTypes.ULDIdentification ? "s" : "")}";
					AssertNotNull($"ZTyped Property with matching name [{contextPropertyName}] must exist on IXmlEventValueObjectContextValueList.", typeof(IXmlEventValueObjectContextValueList).GetProperty(contextPropertyName));
				}
			});
		}

		public void TestEventAsIXmlEventValueObjectWorksWithRealValues()
		{
			var universalEvent = new Event();
			universalEvent.CreatedTime = new ZDateTimeOffset(2011, 4, 2);
			universalEvent.EventReference = "LEGEND";
			universalEvent.EventTime = new ZDateTimeOffset(2011, 6, 3);
			universalEvent.EventType = "CCC";
			universalEvent.IsEstimate = ZBool.True;
			universalEvent.IsCancelled = ZBool.False;

			var contextCollection = universalEvent.ContextCollection = new List<Context>();
			contextCollection.Add(new Context() { Type = "DestinationIATAAirportCode", Value = "AIA" });
			contextCollection.Add(new Context() { Type = "FlightDate", Value = "2010-04-05" });
			contextCollection.Add(new Context() { Type = "FlightNumber", Value = "QF565" });
			contextCollection.Add(new Context() { Type = "IATAAirportCode", Value = "JFK" });
			contextCollection.Add(new Context() { Type = "IATACarrierCode", Value = "QF" });
			contextCollection.Add(new Context() { Type = "IsPartial", Value = "false" });
			contextCollection.Add(new Context() { Type = "MAWBDestinationIATAAirportCode", Value = "LKR" });
			contextCollection.Add(new Context() { Type = "MAWBNumber", Value = "081-54821939" });
			contextCollection.Add(new Context() { Type = "MAWBOriginIATAAirportCode", Value = "SYD" });
			contextCollection.Add(new Context() { Type = "MessageIsPartial", Value = "true" });
			contextCollection.Add(new Context() { Type = "OriginIATAAirportCode", Value = "DFD" });
			contextCollection.Add(new Context() { Type = "SourceEventCode", Value = "VOILA" });
			contextCollection.Add(new Context() { Type = "ULDIdentification", Value = "UL5218" });
			contextCollection.Add(new Context() { Type = "ULDIdentification", Value = "UL5219" });
			contextCollection.Add(new Context() { Type = new ContextType() { Type = "AMS", Description = "AMS Number" }, Value = "134FREGT" });
			contextCollection.Add(new Context() { Type = "LegDestinationTerminalCode", Value = "C1F-123" });

			var eventData = universalEvent as IXmlEventValueObject;
			var contexts = eventData.Context;
			CombineAssertions(delegate
			{
				AssertEquals("eventData.EventReference", "LEGEND", eventData.EventReference);
				AssertEquals("eventData.EventTime", new ZDateTimeOffset(2011, 6, 3), eventData.EventTime);
				AssertEquals("eventData.EventType", "CCC", eventData.EventType);
				AssertEquals("eventData.IsEstimate", ZBool.True, eventData.IsEstimate);
				AssertEquals("eventData.IsCancelled", ZBool.False, eventData.IsCancelled);
				AssertNotNull("eventData.Context", contexts);
			});

			CombineAssertions(delegate
			{
				AssertEquals("contexts.DestinationIATAAirportCode", "AIA", contexts.DestinationIATAAirportCode);
				AssertEquals("contexts.FlightDate", new ZDateTime(2010, 4, 5), contexts.FlightDate);
				AssertEquals("contexts.FlightNumber", "QF565", contexts.FlightNumber);
				AssertEquals("contexts.IATAAirportCode", "JFK", contexts.IATAAirportCode);
				AssertEquals("contexts.IATACarrierCode", "QF", contexts.IATACarrierCode);
				AssertEquals("contexts.IsPartial", ZBool.False, contexts.IsPartial);
				AssertEquals("contexts.MAWBDestinationIATAAirportCode", "LKR", contexts.MAWBDestinationIATAAirportCode);
				AssertEquals("contexts.MAWBNumber", "081-54821939", contexts.MAWBNumber);
				AssertEquals("contexts.MAWBOriginIATAAirportCode", "SYD", contexts.MAWBOriginIATAAirportCode);
				AssertEquals("contexts.MessageIsPartial", ZBool.True, contexts.MessageIsPartial);
				AssertEquals("contexts.OriginIATAAirportCode", "DFD", contexts.OriginIATAAirportCode);
				AssertEquals("contexts.SourceEventCode", "VOILA", contexts.SourceEventCode);
				AssertContainsExactElementsInAnyOrder("contexts.ULDIdentifications", new List<ZString> { "UL5218", "UL5219" }, contexts.ULDIdentifications);
				AssertEquals("AdditionalReference", "134FREGT", contexts.Values.Single(reference => reference.Key.Type == "AMS" && reference.Key.Description == "AMS Number").Value);
				AssertEquals("LegDestinationTerminalCode", "C1F-123", contexts.LegDestinationTerminalCode);
				AssertEquals("contexts.Values.Count", 16, contexts.Values.Count());
			});
		}

		public void TestEventAsIXmlEventValueObjectDoesntThrowNullRefExceptionsOnAnEmptyEvent()
		{
			IXmlEventValueObject eventData = new Event();
			var contexts = eventData.Context;
			CombineAssertions(delegate
			{
				AssertEquals("eventData.CreatedTime", ZDateTimeOffset.Empty, eventData.CreatedTime);
				AssertEquals("eventData.EventReference", ZString.Empty, eventData.EventReference);
				AssertEquals("eventData.EventTime", ZDateTimeOffset.Empty, eventData.EventTime);
				AssertEquals("eventData.EventType", ZString.Empty, eventData.EventType);
				AssertEquals("eventData.IsEstimate", ZBool.False, eventData.IsEstimate);
				AssertEquals("eventData.IsCancelled", ZBool.False, eventData.IsCancelled);
				AssertNotNull("eventData.Context", contexts);
			});

			CombineAssertions(delegate
			{
				AssertEquals("contexts.DestinationIATAAirportCode", ZString.Empty, contexts.DestinationIATAAirportCode);
				AssertEquals("contexts.FlightDate", ZDateTime.Empty, contexts.FlightDate);
				AssertEquals("contexts.FlightNumber", null, contexts.FlightNumber);
				AssertEquals("contexts.IATAAirportCode", ZString.Empty, contexts.IATAAirportCode);
				AssertEquals("contexts.IATACarrierCode", ZString.Empty, contexts.IATACarrierCode);
				AssertEquals("contexts.IsPartial", ZBool.False, contexts.IsPartial);
				AssertEquals("contexts.MAWBDestinationIATAAirportCode", ZString.Empty, contexts.MAWBDestinationIATAAirportCode);
				AssertEquals("contexts.MAWBNumber", ZString.Empty, contexts.MAWBNumber);
				AssertEquals("contexts.MAWBOriginIATAAirportCode", ZString.Empty, contexts.MAWBOriginIATAAirportCode);
				AssertEquals("contexts.MessageIsPartial", ZBool.False, contexts.MessageIsPartial);
				AssertEquals("contexts.OriginIATAAirportCode", ZString.Empty, contexts.OriginIATAAirportCode);
				AssertEquals("contexts.SourceEventCode", ZString.Empty, contexts.SourceEventCode);
				AssertEquals("contexts.ULDIdentifications", null, contexts.ULDIdentifications);
				AssertEquals("contexts.Values.Count", 0, contexts.Values.Count());
			});
		}
	}
}
