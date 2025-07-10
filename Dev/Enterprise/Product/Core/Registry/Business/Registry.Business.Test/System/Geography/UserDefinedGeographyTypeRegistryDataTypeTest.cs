using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(UserDefinedGeographyTypeRegistryDataType))]
	sealed class UserDefinedGeographyTypeRegistryDataTypeTest : CodeDescriptionPairListRegistryDataTypeTest
	{
		public void TestValidateCode()
		{
			var dataType = new UserDefinedGeographyTypeRegistryDataTypeForTest();
			var list = new CodeDescriptionPairList();

			list.AddPair("", "Description");
			var exception = AssertExceptionThrown<RegistryValidationException>(() => dataType.ValidateCore(null, list, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Code should consist of 4 characters.", exception.Message);

			list.Clear();
			list.AddPair("UAB", "Description");
			exception = AssertExceptionThrown<RegistryValidationException>(() => dataType.ValidateCore(null, list, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Code should consist of 4 characters.", exception.Message);

			list.Clear();
			list.AddPair("ABCD", "Description");
			exception = AssertExceptionThrown<RegistryValidationException>(() => dataType.ValidateCore(null, list, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Code should start with 'U'.", exception.Message);

			list.Clear();
			list.AddPair("U.#$", "Description");
			exception = AssertExceptionThrown<RegistryValidationException>(() => dataType.ValidateCore(null, list, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Code can only contain numbers and letters.", exception.Message);

			list.Clear();
			list.AddPair("U  A", "Description");
			exception = AssertExceptionThrown<RegistryValidationException>(() => dataType.ValidateCore(null, list, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Code can only contain numbers and letters.", exception.Message);

			list.Clear();
			list.AddPair("UABV", "Description");
			AssertNoExceptionThrown(() => dataType.ValidateCore(null, list, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		protected override CodeDescriptionPairListRegistryDataType GetNewDataType()
		{
			return new UserDefinedGeographyTypeRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var list1 = new CodeDescriptionPairList();
			var list2 = new CodeDescriptionPairList();
			list2.AddPair("UABC", "Description For UABC");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(list1, list1.ToXMLByteArray()),
				new ValidSampleAndBinaryValueInDB(list2, list2.ToXMLByteArray())
			};
		}

		class UserDefinedGeographyTypeRegistryDataTypeForTest : UserDefinedGeographyTypeRegistryDataType
		{
			public new void ValidateCore(IRegistryItem registryItem, ReadOnlyCodeDescriptionPairList proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			}
		}
	}
}
