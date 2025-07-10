using System;
using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(IPAddressRangesDataType))]
	sealed class IPAddressRangesDataTypeTest : RegistryDataTypeTestCase<IPAddressRangesDataType>
	{
		protected override IPAddressRangesDataType GetNewDataType()
		{
			return new IPAddressRangesDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new[]
			{
				new ValidSampleAndBinaryValueInDB("134.170.188.216/29", Encoding.Unicode.GetBytes("134.170.188.216/29")),
				new ValidSampleAndBinaryValueInDB("203.62.211.4/30", Encoding.Unicode.GetBytes("203.62.211.4/30"))
			};
		}

		public void TestValidate()
		{
			AssertNoExceptionThrown(() => new IPAddressRangesDataType().Validate(null, "", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => new IPAddressRangesDataType().Validate(null, " ", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => new IPAddressRangesDataType().Validate(null, "134.170.188.216/29", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => new IPAddressRangesDataType().Validate(null, "134.170.188.216", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => new IPAddressRangesDataType().Validate(null, "134.170.188.221/30;134.170.188.216/29", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertExceptionThrown<RegistryValidationException>(() => new IPAddressRangesDataType().Validate(null, "bla", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertExceptionThrown<RegistryValidationException>(() => new IPAddressRangesDataType().Validate(null, "134.170.188.221/30;bla", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertExceptionThrown<RegistryValidationException>(() => new IPAddressRangesDataType().Validate(null, "134.170.188.221/55", Guid.Empty, Guid.Empty, Guid.Empty));
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}
	}
}
