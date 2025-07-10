using System.Collections.Generic;
using System.Xml.Linq;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DataTransformation;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(SeaBookingRequestV1ToV2Transformation))]
	[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
	sealed class SeaBookingRequestV1ToV2TransformationTest : XsltDataTransformationTest
	{
		protected override XDocument Source => GetDocument(@"OldSeaBookingRequestData.xml");

		protected override XDocument ExpectedResult => GetDocument(@"NewSeaBookingRequestData.xml");

		protected override IEnumerable<string> ExpectedNotifications
		{
			get
			{
				yield return "Some fields were overridden by you, but are no longer able to remain overridden for compliance purposes. Please check the following fields: Container - TareWeight, Package - Weight, Package - Volume, Package - Packs.";
			}
		}

		public void TestTransformationForMultiplePackLines()
		{
			var source = GetDocument(@"OldSeaBookingRequestDataWithMultiplePackLines.xml");
			var expectedResult = GetDocument(@"NewSeaBookingRequestDataWithMultiplePackLines.xml");

			var transformation = new SeaBookingRequestV1ToV2Transformation();
			var result = transformation.Run(source, new NotificationsHandler());

			AssertNotNull("result", result);
			AssertMultilineASCIIEquals("result",
				expectedResult.ToString(),
				result.ToString());
		}
	}
}
