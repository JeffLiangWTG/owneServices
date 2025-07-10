using System.IO;
using System.Linq;
using CargoWise.Definitions;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GlowCompatibility.Test
{
	class TrackingEventsRegistryTests : TestCase
	{
		public void TestEventVisibility()
		{
			var enterpriseValues = WebDataRegistry.Instance.EventVisibility
				.DefaultValue
				.Cast<EventVisibility>()
				.Select(v => v.EventCode.ToString())
				.ToArray();
			var message = "Default event codes are incompatible with CW1. Please update TrackableEvents in Glow.";
			AssertArrayEqualsByElements(message, enterpriseValues, TrackableEvents.DefaultCodes.ToArray());
		}

		public void TestIncludeEstimates()
		{
			var message = "Default IncludeEstimates flag is incompatible with CW1, please update TrackableEvents in Glow.";
			AssertEquals(message,
				WebDataRegistry.Instance.EventIncludeEstimates.DefaultValue,
				TrackableEvents.DefaultOptions.IncludeEstimates);
		}

		public void TestIncludeRelated()
		{
			var message = "Default IncludeRelated flag is incompatible with CW1, please update TrackableEvents in Glow.";
			AssertEquals(message,
				WebDataRegistry.Instance.EventIncludeRelated.DefaultValue,
				TrackableEvents.DefaultOptions.IncludeRelated);
		}
	}

	class TrackingEventRegistryNamesTest : TestCase
	{
		public void TestSameAsCW1()
		{
			AssertEquals(WebDataRegistry.Instance.EventVisibility.Name, TrackableEvents.RegistryNames.EventCodes);
			AssertEquals(WebDataRegistry.Instance.EventIncludeEstimates.Name, TrackableEvents.RegistryNames.IncludeEstimates);
			AssertEquals(WebDataRegistry.Instance.EventIncludeRelated.Name, TrackableEvents.RegistryNames.IncludeRelated);
		}
	}

	class TrackingEventCodeVisiblitySerializerTest : TestCase
	{
		public void TestDeserialize()
		{
			var defaultValue = WebDataRegistry.Instance.EventVisibility.DefaultValue;
			using (var stream = new MemoryStream())
			{
				ZXmlSerializer
					.New(WebDataRegistry.Instance.EventVisibility.DataType.DataType)
					.Serialize(stream, defaultValue);

				var message = "EventVisibilityDeserializer is incompatible with CW1. Please update EventVisibilityDeserializer in Glow.";
				var glowValues = EventVisibilityDeserializer.Deserialise(stream.ToArray()).ToArray();
				var enterpriseValues = WebDataRegistry.Instance.EventVisibility
					.DefaultValue
					.Cast<EventVisibility>()
					.Select(v => v.EventCode.ToString())
					.ToArray();
				AssertArrayEqualsByElements(message, enterpriseValues, glowValues);
			}
		}
	}
}
