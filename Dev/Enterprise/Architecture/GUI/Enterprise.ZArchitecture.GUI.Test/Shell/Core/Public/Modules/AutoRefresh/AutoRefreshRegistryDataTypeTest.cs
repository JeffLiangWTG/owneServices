using System;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.AutoRefresh.Testing
{
	[TestedType(typeof(AutoRefreshRegistryDataType))]
	sealed class AutoRefreshRegistryDataTypeTest : RegistryDataTypeTestCase<AutoRefreshRegistryDataType>
	{
		protected override AutoRefreshRegistryDataType GetNewDataType()
		{
			return new AutoRefreshRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(new AutoRefreshTimeOut(), new byte[] { Convert.ToByte(false), 5 }),
				new ValidSampleAndBinaryValueInDB(new AutoRefreshTimeOut(true, 15), new byte[] { Convert.ToByte(true), 15 })
			};
		}

		protected override void AssertValuesEqualForCheckingDefault(string message, object lhs, object rhs)
		{
			AssertValuesEqual(message, lhs, rhs);
		}

		protected override void AssertValuesEqual(string message, object lhs, object rhs)
		{
			var lhsAutoRefresh = (AutoRefreshTimeOut)lhs;
			var rhsAutoRefresh = (AutoRefreshTimeOut)rhs;

			AssertEquals(true, lhsAutoRefresh.IsEnabled == rhsAutoRefresh.IsEnabled);
			AssertEquals(true, lhsAutoRefresh.RefreshTimeInMinutes == rhsAutoRefresh.RefreshTimeInMinutes);
		}
	}
}
