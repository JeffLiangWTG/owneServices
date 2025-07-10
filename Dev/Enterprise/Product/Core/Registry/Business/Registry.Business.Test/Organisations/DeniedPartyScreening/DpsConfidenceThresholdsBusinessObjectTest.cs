using System.IO;
using System.Xml;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DpsConfidenceThresholdsBusinessObject))]
	sealed class DpsConfidenceThresholdsBusinessObjectTest : RegistryBusinessObjectTemplateTestCase<DpsConfidenceThresholdsBusinessObject>
	{
		#region GetDefaults

		public void TestGetDefaultThresholds()
		{
			var defaultThresholds = new DpsConfidenceThresholdsBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals(DeniedPartyConstants.MatchScores.Medium, defaultThresholds.MediumThreshold);
				AssertEquals(DeniedPartyConstants.MatchScores.High, defaultThresholds.HighThreshold);
			});
		}

		#endregion

		#region Serialization

		public void TestXmlSerialization()
		{
			string result;
			var defaultThresholds = new DpsConfidenceThresholdsBusinessObject();

			var expectedSerializedXml = $"<?xml version=\"1.0\" encoding=\"utf-16\"?><DpsConfidenceThresholdsBusinessObject><MediumThreshold>{DeniedPartyConstants.MatchScores.Medium}</MediumThreshold><HighThreshold>{DeniedPartyConstants.MatchScores.High}</HighThreshold></DpsConfidenceThresholdsBusinessObject>";
			using (StringWriter stringWriter = new StringWriter())
			using (XmlTextWriter writer = new XmlTextWriter(stringWriter))
			{
				Serializer.Serialize(writer, defaultThresholds);
				result = stringWriter.ToString();
			}

			AssertEquals(expectedSerializedXml, result);
		}

		public void TestXmlDeserialization()
		{
			DpsConfidenceThresholdsBusinessObject deserializedBizo;
			var defaultThresholds = new DpsConfidenceThresholdsBusinessObject();

			var xmlToDeserialize = $"<?xml version=\"1.0\" encoding=\"utf-16\"?><DpsConfidenceThresholdsBusinessObject><MediumThreshold>{DeniedPartyConstants.MatchScores.Medium}</MediumThreshold><HighThreshold>{DeniedPartyConstants.MatchScores.High}</HighThreshold></DpsConfidenceThresholdsBusinessObject>";
			using (StringReader stringReader = new StringReader(xmlToDeserialize))
			using (XmlTextReader reader = new XmlTextReader(stringReader))
			{
				deserializedBizo = (DpsConfidenceThresholdsBusinessObject)Serializer.Deserialize(reader);
			}

			CombineAssertions(() =>
			{
				AssertEquals("Should deserialize to default values", defaultThresholds, deserializedBizo);
				AssertNoErrors(deserializedBizo.HighThresholdInfo);
				AssertNoErrors(deserializedBizo.MediumThresholdInfo);
			});
		}

		#endregion

		#region Equals

		public void TestEquals()
		{
			var defaultThresholds = new DpsConfidenceThresholdsBusinessObject();
			var thresholds1 = new DpsConfidenceThresholdsBusinessObject { MediumThreshold = 75, HighThreshold = 85 };
			var thresholds2 = new DpsConfidenceThresholdsBusinessObject { MediumThreshold = 85, HighThreshold = 75 };
			var thresholds3 = new DpsConfidenceThresholdsBusinessObject { MediumThreshold = DeniedPartyConstants.MatchScores.Medium, HighThreshold = DeniedPartyConstants.MatchScores.High };
			var thresholds4 = new DpsConfidenceThresholdsBusinessObject { MediumThreshold = 75, HighThreshold = 85 };

			CombineAssertions(() =>
			{
				AssertEquals(defaultThresholds, thresholds3);
				AssertEquals(thresholds3, defaultThresholds);

				AssertNotEquals(defaultThresholds, thresholds1);
				AssertNotEquals(defaultThresholds, thresholds2);
				AssertNotEquals(defaultThresholds, thresholds4);

				AssertEquals(thresholds1, thresholds4);
				AssertNotEquals(thresholds1, thresholds2);
			});
		}

		public void TestGetHashCode()
		{
			var defaultThresholds = new DpsConfidenceThresholdsBusinessObject();
			AssertEquals(DeniedPartyConstants.MatchScores.Medium.GetHashCode() ^ DeniedPartyConstants.MatchScores.High.GetHashCode(), defaultThresholds.GetHashCode());
		}

		#endregion

		#region Implementation

		protected override DpsConfidenceThresholdsBusinessObject GetBusinessObjectToClone() => new DpsConfidenceThresholdsBusinessObject();

		protected override DpsConfidenceThresholdsBusinessObject GetBusinessObjectToSerialise() => new DpsConfidenceThresholdsBusinessObject();

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		ZXmlSerializer Serializer => serializer ?? (serializer = ZXmlSerializer.New(typeof(DpsConfidenceThresholdsBusinessObject)));
		ZXmlSerializer serializer;

		#endregion
	}
}
