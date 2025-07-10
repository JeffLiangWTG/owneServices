using System;
using System.Text;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(NumberGroupSizesStringListRegistryDataType))]
	public class NumberGroupSizesStringListRegistryDataTypeTest : RegistryDataTypeTestCase<NumberGroupSizesStringListRegistryDataType>
	{
		public void TestNumberGroupSizesStringListRegistryDataTypeValidation()
		{
			var numberGroupSizesStringListRegistryItem = new NumberGroupSizesStringListRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);

			var dataType = (NumberGroupSizesStringListRegistryDataType)numberGroupSizesStringListRegistryItem.DataType;
			var invalidNumberGroupSizesList = new[] { "3,", "aa", "10", "00", "3,4,", "" };
			foreach (var invalidNumberGroupSizes in invalidNumberGroupSizesList)
			{
				AssertExceptionThrown(typeof(RegistryValidationException), "Invalid input: Please input valid Number Group Sizes",
					() => dataType.Validate(numberGroupSizesStringListRegistryItem, invalidNumberGroupSizes, EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			}

			AssertNoExceptionThrown(() => dataType.Validate(numberGroupSizesStringListRegistryItem, "3,4", EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
		}

		protected override NumberGroupSizesStringListRegistryDataType GetNewDataType()
		{
			return new NumberGroupSizesStringListRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB("3", Encoding.Unicode.GetBytes("3")),
				new ValidSampleAndBinaryValueInDB("4", Encoding.Unicode.GetBytes("4")),
				new ValidSampleAndBinaryValueInDB("3,4", Encoding.Unicode.GetBytes("3,4")),
				new ValidSampleAndBinaryValueInDB("5,6,7", Encoding.Unicode.GetBytes("5,6,7"))
			};
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}
	}
}
