using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	abstract class StringArrayRegistryDataTypeTest<T> : RegistryDataTypeTestCase<T> where T : StringArrayRegistryDataType
	{
		public void TestDefaults()
		{
			AssertEquals("MaximumLength", 256, DataType.MaximumLength);
		}

		[ExpectExceptionMessage(typeof(RegistryValidationException), "Values maximum length cannot be more than 10.")]
		public void TestValidateStringMaximumLength()
		{
			DataType.MaximumLength = 10;
			DataType.Validate(null, new string[] { "hello world" }, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		public void TestCharacterCase()
		{
			AssertEquals(CharacterCase.Normal, DataType.CharacterCase);
		}
	}
}
