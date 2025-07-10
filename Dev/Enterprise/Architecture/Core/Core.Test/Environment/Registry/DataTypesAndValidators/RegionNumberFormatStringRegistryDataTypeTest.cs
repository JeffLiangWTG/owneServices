using System;
using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(RegionNumberFormatStringRegistryDataType))]
	sealed class RegionNumberFormatStringRegistryDataTypeTest : StringRegistryDataTypeTest
	{
		public void TestRegionNumberFormatStringRegistryDataType_SameValueAsConflictingRegistryItem()
		{
			var dataType = new RegionNumberFormatStringRegistryDataType(() => EnvProxy.Instance.Registry.RawRegistry.CurrencyGroupSeparator);

			var registryItem = EnvProxy.Instance.Registry.RawRegistry.CurrencyDecimalSeparator;
			var exceptionMessage = "The Currency Decimal Separator cannot be set as the same value as Currency Group Separator.";

			var currencyGroupSeparator = EnvProxy.Instance.Registry.RawRegistry.CurrencyGroupSeparator.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			AssertNoExceptionThrown(exceptionMessage, () => dataType.Validate(registryItem, ",,", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, currencyGroupSeparator, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		protected override StringRegistryDataType GetNewDataType()
		{
			return new RegionNumberFormatStringRegistryDataType(() => new StringRegistryItem(null)) { MaxLength = 10 };
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(".", Encoding.Unicode.GetBytes(".")),
				new ValidSampleAndBinaryValueInDB("*", Encoding.Unicode.GetBytes("*")),
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { "1234567890!" };
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValue => true;
	}
}
