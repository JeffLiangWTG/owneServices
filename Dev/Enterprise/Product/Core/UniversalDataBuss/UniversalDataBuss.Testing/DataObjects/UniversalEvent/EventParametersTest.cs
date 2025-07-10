namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	using System;
	using CargoWise.EventReference;
	using CargoWise.Types;
	using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
	using NUnit.Framework;

	[TestedType(typeof(EventParameters))]
	class EventParametersTest : DataObjectTestCase<EventParameters>
	{
		public void TestGetCodeByName_ComplexTest()
		{
			AssertEquals("Location", Constants.EventReferenceParameters.Codes.Location, EventParameters.GetCodeByName("Location"));
			AssertEquals("Facility", Constants.EventReferenceParameters.Codes.Facility, EventParameters.GetCodeByName("Facility"));
			AssertEquals("Department", Constants.EventReferenceParameters.Codes.Department, EventParameters.GetCodeByName("Department"));
			AssertEquals("Service", Constants.EventReferenceParameters.Codes.Service, EventParameters.GetCodeByName("Service"));
			AssertEquals("Reason", Constants.EventReferenceParameters.Codes.Reason, EventParameters.GetCodeByName("Reason"));
			AssertEquals("MessageType", Constants.EventReferenceParameters.Codes.MessageType, EventParameters.GetCodeByName("MessageType"));
			AssertEquals("MessageSubType", Constants.EventReferenceParameters.Codes.MessageSubType, EventParameters.GetCodeByName("MessageSubType"));
			AssertEquals("Old", Constants.EventReferenceParameters.Codes.Old, EventParameters.GetCodeByName("Old"));
			AssertEquals("New", Constants.EventReferenceParameters.Codes.New, EventParameters.GetCodeByName("New"));
			AssertEquals("Type", Constants.EventReferenceParameters.Codes.Type, EventParameters.GetCodeByName("Type"));
			AssertEquals("Name", Constants.EventReferenceParameters.Codes.Name, EventParameters.GetCodeByName("Name"));
			AssertEquals("Partial", Constants.EventReferenceParameters.Codes.Partial, EventParameters.GetCodeByName("Partial"));
			AssertEquals("Total", Constants.EventReferenceParameters.Codes.Total, EventParameters.GetCodeByName("Total"));
			AssertEquals("VoyageFlightNumber", Constants.EventReferenceParameters.Codes.VoyageFlightNumber, EventParameters.GetCodeByName("VoyageFlightNumber"));
			AssertEquals("FlightDate", Constants.EventReferenceParameters.Codes.FlightDate, EventParameters.GetCodeByName("FlightDate"));
			AssertEquals("EquipmentReferenceNumber", Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, EventParameters.GetCodeByName("EquipmentReferenceNumber"));
			AssertEquals("CustomsReferenceNumber", Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, EventParameters.GetCodeByName("CustomsReferenceNumber"));
			AssertEquals("ExternalDocumentType", Constants.EventReferenceParameters.Codes.ExternalDocumentType, EventParameters.GetCodeByName("ExternalDocumentType"));
			AssertEquals("ReceiptNumber", Constants.EventReferenceParameters.Codes.ReceiptNumber, EventParameters.GetCodeByName("ReceiptNumber"));
			AssertEquals("ReferenceNumber", Constants.EventReferenceParameters.Codes.ReferenceNumber, EventParameters.GetCodeByName("ReferenceNumber"));
			AssertEquals("RequestNumber", Constants.EventReferenceParameters.Codes.RequestNumber, EventParameters.GetCodeByName("RequestNumber"));
			AssertEquals("Quantity", Constants.EventReferenceParameters.Codes.Quantity, EventParameters.GetCodeByName("Quantity"));
			AssertEquals("Status", Constants.EventReferenceParameters.Codes.Status, EventParameters.GetCodeByName("Status"));
			AssertEquals("Score", Constants.EventReferenceParameters.Codes.Score, EventParameters.GetCodeByName("Score"));
			AssertEquals("EventCode", Constants.EventReferenceParameters.Codes.EventCode, EventParameters.GetCodeByName("EventCode"));
			AssertEquals("From", Constants.EventReferenceParameters.Codes.From, EventParameters.GetCodeByName("From"));

			AssertNull("Unknown name", EventParameters.GetCodeByName("McLaren"));
		}

		public void TestGetPropertyByCode_ComplexTest()
		{
			AssertEquals("Location", "Location", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.Location).Name);
			AssertEquals("Facility", "Facility", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.Facility).Name);
			AssertEquals("Department", "Department", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.Department).Name);
			AssertEquals("Service", "Service", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.Service).Name);
			AssertEquals("Reason", "Reason", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.Reason).Name);
			AssertEquals("MessageType", "MessageType", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.MessageType).Name);
			AssertEquals("MessageSubType", "MessageSubType", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.MessageSubType).Name);
			AssertEquals("Old", "Old", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.Old).Name);
			AssertEquals("New", "New", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.New).Name);
			AssertEquals("Type", "Type", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.Type).Name);
			AssertEquals("Name", "Name", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.Name).Name);
			AssertEquals("Partial", "Partial", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.Partial).Name);
			AssertEquals("Total", "Total", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.Total).Name);
			AssertEquals("VoyageFlightNumber", "VoyageFlightNumber", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.VoyageFlightNumber).Name);
			AssertEquals("FlightDate", "FlightDate", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.FlightDate).Name);
			AssertEquals("EquipmentReferenceNumber", "EquipmentReferenceNumber", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber).Name);
			AssertEquals("CustomsReferenceNumber", "CustomsReferenceNumber", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.CustomsReferenceNumber).Name);
			AssertEquals("ExternalDocumentType", "ExternalDocumentType", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.ExternalDocumentType).Name);
			AssertEquals("ReceiptNumber", "ReceiptNumber", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.ReceiptNumber).Name);
			AssertEquals("ReferenceNumber", "ReferenceNumber", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.ReferenceNumber).Name);
			AssertEquals("RequestNumber", "RequestNumber", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.RequestNumber).Name);
			AssertEquals("Quantity", "Quantity", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.Quantity).Name);
			AssertEquals("Status", "Status", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.Status).Name);
			AssertEquals("Score", "Score", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.Score).Name);
			AssertEquals("EventCode", "EventCode", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.EventCode).Name);
			AssertEquals("From", "From", EventParameters.GetPropertyByCode(Constants.EventReferenceParameters.Codes.From).Name);

			AssertNull("Unknown code", EventParameters.GetPropertyByCode("McLaren"));
		}

		public void TestGetEventParameters()
		{
			var eventParams = new EventParameters();
			var reference = "";
			AssertEquals("Precondition.", 0, EventParameters.GetEventParameters(eventParams).Count);

			var properties = typeof(EventParameters).GetProperties();
			for (int i = 0; i < properties.Length; i++)
			{
				var property = properties[i];
				var code = EventParameters.GetCodeByName(property.Name);

				object uxmlValue;
				string paramValue;

				if (property.PropertyType == typeof(ZInt?))
				{
					uxmlValue = new ZInt?(i);
					paramValue = uxmlValue.ToString();
				}
				else if (property.PropertyType == typeof(ZDateTime?))
				{
					uxmlValue = new ZDateTime?(new ZDateTime(2015, 1, 1));
					paramValue = new ZDateTime(2015, 1, 1).ToISO8601String();
				}
				else if (property.PropertyType == typeof(ZDateTimeOffset?))
				{
					uxmlValue = new ZDateTimeOffset?(new ZDateTimeOffset(2015, 1, 1, 1, 2, 3, 123, TimeSpan.FromHours(8)));
					paramValue = new ZDateTimeOffset(2015, 1, 1, 1, 2, 3, 123, TimeSpan.FromHours(8)).ToISO8601String();
				}
				else
				{
					uxmlValue = new ZString?("ABC" + i);
					paramValue = uxmlValue.ToString();
				}

				property.SetValue(eventParams, uxmlValue);

				var dictionary = EventParameters.GetEventParameters(eventParams);
				AssertEquals(i + 1, dictionary.Count);
				AssertEquals(paramValue, dictionary[code]);
				AssertEquals(paramValue, EventParameters.GetEventParameter(code, eventParams));

				// fallback to reference
				reference = reference + "|" + code + "=DEF" + i;
				dictionary = EventParameters.GetEventParameters(null, reference);
				AssertEquals(i + 1, dictionary.Count);
				AssertEquals("DEF" + i, dictionary[code]);
				AssertEquals("DEF" + i, EventParameters.GetEventParameter(code, null, reference));
			}
		}
	}
}
