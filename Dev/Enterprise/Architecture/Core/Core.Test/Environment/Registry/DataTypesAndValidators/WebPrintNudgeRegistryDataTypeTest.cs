using System.Text;
using System.Text.Json;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(WebPrintNudgeRegistryDataType))]
	sealed class WebPrintNudgeRegistryDataTypeTest : RegistryDataTypeTestCase<WebPrintNudgeRegistryDataType>
	{
		public void TestSerialization()
		{
			var nudgeBefore = new WebPrintNudge { EnableIPAddress = true };
			var dataType = new WebPrintNudgeRegistryDataType(nudgeBefore);

			var nudgeDuring = dataType.Serialise(nudgeBefore);
			var nudgeAfter = dataType.Deserialise(nudgeDuring);

			AssertValuesEqual("Deserialized data should match the data before serialization.", nudgeBefore, nudgeAfter);
		}

		#region Implementation

		protected override WebPrintNudgeRegistryDataType GetNewDataType()
		{
			return new WebPrintNudgeRegistryDataType(new WebPrintNudge { EnableIPAddress = true });
		}

		protected override void AssertValuesEqual(string message, object lhs, object rhs)
		{
			var lhsNudge = (WebPrintNudge)lhs;
			var rhsNudge = (WebPrintNudge)rhs;

			AssertEquals(message, lhsNudge?.EnableIPAddress, rhsNudge?.EnableIPAddress);
			AssertEquals(message, lhsNudge?.ChangingToUrlAddressDateTimeUtc, rhsNudge?.ChangingToUrlAddressDateTimeUtc);
			AssertEquals(message, lhsNudge?.SwtichBackToIPAddressIntervalInHours, rhsNudge?.SwtichBackToIPAddressIntervalInHours);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var nudge1 = new WebPrintNudge { EnableIPAddress = true };
			var nudge2 = new WebPrintNudge { EnableIPAddress = false, ChangingToUrlAddressDateTimeUtc = new System.DateTime(2022, 06, 13, 11, 15, 15), SwtichBackToIPAddressIntervalInHours = 24 };

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(nudge1,Encoding.Unicode.GetBytes(JsonSerializer.Serialize(nudge1))),
				new ValidSampleAndBinaryValueInDB(nudge2,Encoding.Unicode.GetBytes(JsonSerializer.Serialize(nudge2))),
			};
		}

		#endregion
	}
}
