using System.Text;
using System.Text.Json;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(WebPrintNudgeSuspendingRegistryDataType))]
	sealed class WebPrintNudgeSuspendingRegistryDataTypeTest : RegistryDataTypeTestCase<WebPrintNudgeSuspendingRegistryDataType>
	{
		public void TestSerialization()
		{
			var nudgeSuspendingBefore = new WebPrintNudgeSuspending();
			var dataType = new WebPrintNudgeSuspendingRegistryDataType(nudgeSuspendingBefore);

			var nudgeSuspendingDuring = dataType.Serialise(nudgeSuspendingBefore);
			var nudgeSuspendingAfter = dataType.Deserialise(nudgeSuspendingDuring);

			AssertValuesEqual("Deserialized data should match the data before serialization.", nudgeSuspendingBefore, nudgeSuspendingAfter);
		}

		#region Implementation

		protected override WebPrintNudgeSuspendingRegistryDataType GetNewDataType()
		{
			return new WebPrintNudgeSuspendingRegistryDataType(new WebPrintNudgeSuspending());
		}

		protected override void AssertValuesEqual(string message, object lhs, object rhs)
		{
			var lhsNudge = (WebPrintNudgeSuspending)lhs;
			var rhsNudge = (WebPrintNudgeSuspending)rhs;

			AssertEquals(message, lhsNudge?.MaxErrorsInMinutes, rhsNudge?.MaxErrorsInMinutes);
			AssertEquals(message, lhsNudge?.IntervalMinutes, rhsNudge?.IntervalMinutes);
			AssertEquals(message, lhsNudge?.SuspendMinutes, rhsNudge?.SuspendMinutes);
			AssertEquals(message, lhsNudge?.MaxErrorsInHours, rhsNudge?.MaxErrorsInHours);
			AssertEquals(message, lhsNudge?.IntervalHours, rhsNudge?.IntervalHours);
			AssertEquals(message, lhsNudge?.SuspendHours, rhsNudge?.SuspendHours);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var nudgeSuspending1 = new WebPrintNudgeSuspending();
			var nudgeSuspending2 = new WebPrintNudgeSuspending
			{
				MaxErrorsInMinutes = 3,
				IntervalMinutes = 15,
				SuspendMinutes = 10,
				MaxErrorsInHours = 10,
				IntervalHours = 1,
				SuspendHours = 1
			};

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(nudgeSuspending1, Encoding.Unicode.GetBytes(JsonSerializer.Serialize(nudgeSuspending1))),
				new ValidSampleAndBinaryValueInDB(nudgeSuspending2, Encoding.Unicode.GetBytes(JsonSerializer.Serialize(nudgeSuspending2))),
			};
		}

		#endregion
	}
}
