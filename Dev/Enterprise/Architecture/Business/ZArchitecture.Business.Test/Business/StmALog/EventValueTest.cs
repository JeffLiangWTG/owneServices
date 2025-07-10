using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class EventValueTest : TestCase
	{
		public void TestCreateWithTypeAndTimeOnly()
		{
			Event shellEvent = new Event("XXX", (NoResString)"", ZGuid.NewZGuid());
			EventValue eventValue = new EventValue(shellEvent, new ZDateTimeOffset());

			AssertEquals(shellEvent, eventValue.EventType);
		}

		public void TestFullValidFields()
		{
			Event shellEvent = new Event("XXX", (NoResString)"YYY", ZGuid.NewZGuid());
			DummyPropagationSettings shellPropagationSettings = new DummyPropagationSettings(true);
			String testReference = "testReference";
			ImmutableDictionary<string, string> shellParameters = new Dictionary<string, string>
			{
				{ "placeholder_one", "placeholder_value_one" },
				{ "placeholder_two", "placeholder_value_two" },
			}.ToImmutableDictionary();

			EventValue testEventValue = new EventValue(shellEvent,
														 reference: testReference,
														 parameters: shellParameters,
														 propagationSettings: shellPropagationSettings);
			AssertEquals(shellEvent.Code, testEventValue.Code);
			AssertEquals(shellEvent.Description, testEventValue.Description);
			AssertEquals(shellEvent.PK, testEventValue.PK);
			AssertEquals(testReference, testEventValue.Reference);
			AssertEquals(shellParameters.Count, testEventValue.Parameters.Count);
			foreach (KeyValuePair<string, string> item in shellParameters)
			{
				Assert(testEventValue.Parameters.Contains(item));
			}
			AssertEquals(shellPropagationSettings.PropagateOnParameterChange, testEventValue.PropagationSettings.PropagateOnParameterChange);
		}

		public void TestDefaultValues()
		{
			EventValue testEventValue = new EventValue(new Event("XXX", (NoResString)"YYY", ZGuid.NewZGuid()));
			AssertNotEquals(ZGuid.Empty, testEventValue.PK);
			AssertEquals(false, testEventValue.DeferFiringWorkflow);
			AssertEquals(false, testEventValue.IsEstimate);
			AssertEquals(ZDateTimeOffset.Empty, testEventValue.EventTime);
			AssertEquals(ImmutableDictionary<string, string>.Empty, testEventValue.Parameters);
			AssertEquals("", testEventValue.Reference);
			AssertEquals(new DefaultPropagationSettings().PropagateOnParameterChange, testEventValue.PropagationSettings.PropagateOnParameterChange);
		}

		public void TestLocationParameterChangesEventTime_Offset()
		{
			//With daylight saving
			var dt = new DateTime(2023, 3, 1);
			var ev = new EventValue(AutoEvents.CustomisableEvent00, eventTime: new ZDateTimeOffset(dt, TimeSpan.FromHours(0)), parameters: new Dictionary<string, string>
			{
				{ Constants.EventReferenceParameters.Codes.Location, "AUSYD" }
			});
			AssertEquals(new ZDateTimeOffset(dt, TimeSpan.FromHours(11)), ev.EventTime);

			//Without daylight saving
			dt = new DateTime(2023, 5, 1);
			ev = new EventValue(AutoEvents.CustomisableEvent00, eventTime: new ZDateTimeOffset(dt, TimeSpan.FromHours(2)), parameters: new Dictionary<string, string>
			{
				{ Constants.EventReferenceParameters.Codes.Location, "AUSYD" }
			});
			AssertEquals(new ZDateTimeOffset(dt, TimeSpan.FromHours(10)), ev.EventTime);

			//Brisbane
			dt = new DateTime(2023, 3, 1);
			ev = new EventValue(AutoEvents.CustomisableEvent00, eventTime: new ZDateTimeOffset(dt, TimeSpan.FromHours(1)), parameters: new Dictionary<string, string>
			{
				{ Constants.EventReferenceParameters.Codes.Location, "AUBNE" }
			});
			AssertEquals(new ZDateTimeOffset(dt, TimeSpan.FromHours(10)), ev.EventTime);
		}

		[TestTimeZoneUNLOCO("AUBNE")]
		public void TestInvalidLocationParameterDoesNotChangeOffset()
		{
			var dt = new ZDateTime(2023, 3, 1);
			var ev = new EventValue(AutoEvents.CustomisableEvent00, eventTime: dt.ToOffset(), parameters: new Dictionary<string, string>
			{
				{ Constants.EventReferenceParameters.Codes.Location, "XXXXX" }
			});
			AssertEquals(new ZDateTimeOffset(dt, TimeSpan.FromHours(10)), ev.EventTime);
		}

		public void TestInvalidLocationParameterDoesNotChangeOffset2()
		{
			var dt = new ZDateTime(2023, 3, 1);
			var ev = new EventValue(AutoEvents.CustomisableEvent00, eventTime: new ZDateTimeOffset(dt, TimeSpan.FromHours(5)), parameters: new Dictionary<string, string>
			{
				{ Constants.EventReferenceParameters.Codes.Location, "XXXXX" }
			});
			AssertEquals(new ZDateTimeOffset(dt, TimeSpan.FromHours(5)), ev.EventTime);
		}
	}
}
