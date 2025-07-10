using System.Text;
using System.Text.Json;
using Enterprise.ZArchitecture.Core.Environment.Registry.DataTypesAndValidators;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(DeleteExpiredRatesRegistryDataType))]
	sealed class DeleteExpiredRatesRegistryDataTypeTest : RegistryDataTypeTestCase<DeleteExpiredRatesRegistryDataType>
	{
		public void TestSerialization()
		{
			var deleteExpiredRatesBefore = new DeleteExpiredRates { ExpiredRatesPeriodInYears = 1, BatchSize = 100 };
			var dataType = new DeleteExpiredRatesRegistryDataType(deleteExpiredRatesBefore);

			var deleteExpiredRatesDuring = dataType.Serialise(deleteExpiredRatesBefore);
			var deleteExpiredRatesAfter = dataType.Deserialise(deleteExpiredRatesDuring);

			AssertValuesEqual("Deserialized data should match the data before serialization.", deleteExpiredRatesBefore, deleteExpiredRatesAfter);
		}

		#region Implementation

		protected override DeleteExpiredRatesRegistryDataType GetNewDataType()
		{
			return new DeleteExpiredRatesRegistryDataType(new DeleteExpiredRates { ExpiredRatesPeriodInYears = 1, BatchSize = 100 });
		}

		protected override void AssertValuesEqual(string message, object lhs, object rhs)
		{
			var lhsRates = (DeleteExpiredRates)lhs;
			var rhsRates = (DeleteExpiredRates)rhs;

			AssertEquals(message, lhsRates?.ExpiredRatesPeriodInYears, rhsRates?.ExpiredRatesPeriodInYears);
			AssertEquals(message, lhsRates?.BatchSize, rhsRates?.BatchSize);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var deleteExpiredRates = new DeleteExpiredRates { ExpiredRatesPeriodInYears = 1, BatchSize = 100 };
			var deleteExpiredRates2 = new DeleteExpiredRates { ExpiredRatesPeriodInYears = 0, BatchSize = 500 };

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(deleteExpiredRates,Encoding.Unicode.GetBytes(JsonSerializer.Serialize(deleteExpiredRates))),
				new ValidSampleAndBinaryValueInDB(deleteExpiredRates2,Encoding.Unicode.GetBytes(JsonSerializer.Serialize(deleteExpiredRates2))),
			};
		}

		#endregion
	}
}
