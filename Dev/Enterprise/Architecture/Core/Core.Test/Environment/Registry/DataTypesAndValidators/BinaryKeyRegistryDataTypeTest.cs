using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(BinaryKeyRegistryDataType))]
	sealed class BinaryKeyRegistryDataTypeTest : RegistryDataTypeTestCase<BinaryKeyRegistryDataType>
	{
		const int KeySizeForTest = 64;

		protected override BinaryKeyRegistryDataType GetNewDataType()
		{
			return new BinaryKeyRegistryDataType(keySize: KeySizeForTest);
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var firstData = new byte[]
			{
				0xC6, 0x8E, 0xD4, 0x14, 0xE1, 0x8E, 0x4E, 0x7C, 0x72, 0x5B, 0xD4, 0xA7, 0xB9, 0xB4, 0x93, 0x12,
				0x96, 0xDE, 0x1E, 0xB1, 0x37, 0x9A, 0xFD, 0xA6, 0x1B, 0x0D, 0x60, 0xD7, 0x1A, 0x73, 0x1E, 0x64,
				0xC3, 0xDA, 0x08, 0xC9, 0xFF, 0x56, 0xEF, 0x6E, 0xBC, 0x7B, 0xD6, 0xF4, 0xC5, 0x0E, 0x7A, 0x10,
				0xB4, 0xB7, 0x33, 0xF6, 0xE9, 0x86, 0xA6, 0xDA, 0x75, 0xBD, 0x51, 0xD1, 0xC1, 0xFF, 0x8E, 0x09
			};

			var firstHex = "C68ED414E18E4E7C725BD4A7B9B4931296DE1EB1379AFDA61B0D60D71A731E64C3DA08C9FF56EF6EBC7BD6F4C50E7A10B4B733F6E986A6DA75BD51D1C1FF8E09";

			var secondData = new byte[]
			{
				0xFF, 0x8A, 0xE4, 0x14, 0xE1, 0x8E, 0x5D, 0x7C, 0x72, 0x5B, 0xD4, 0xA7, 0xB9, 0xB4, 0x93, 0x12,
				0x96, 0xDE, 0x1E, 0xB1, 0x37, 0x9A, 0xFD, 0xA6, 0x1B, 0x00, 0x60, 0xD7, 0x1A, 0x73, 0xFA, 0x64,
				0xC3, 0xDA, 0x09, 0xC9, 0xFF, 0x56, 0xEF, 0x6E, 0xBC, 0x7B, 0xD6, 0xF4, 0xC5, 0x0E, 0x7A, 0x10,
				0xB4, 0xB7, 0x33, 0xF6, 0xE9, 0x86, 0xA6, 0xDA, 0x75, 0xBD, 0x51, 0xD1, 0xC1, 0xFF, 0x8E, 0x11
			};

			var secondHex = "FF8AE414E18E5D7C725BD4A7B9B4931296DE1EB1379AFDA61B0D60071A73FA64C3DA09C9FF56EF6EBC7BD6F4C50E7A10B4B733F6E986A6DA75BD51D1C1FF8E11";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(firstHex, firstData),
				new ValidSampleAndBinaryValueInDB(secondHex, secondData)
			};
		}

		protected override void AssertValuesEqual(string message, object lhs, object rhs)
		{
			byte[] lhsBytes = lhs as byte[];
			byte[] rhsBytes = rhs as byte[];
			Assert(message, (lhsBytes == null && rhsBytes == null) || (lhsBytes != null && rhsBytes != null));
			if (lhsBytes != null)
			{
				AssertEquals(message, lhsBytes.Length, rhsBytes.Length);
				for (int i = 0; i < lhsBytes.Length; i++)
				{
					AssertEquals(message + "; incorrect at byte position " + i, lhsBytes[i], rhsBytes[i]);
				}
			}
		}

		public void TestValidateNonHexString()
		{
			var registryItem = GetNewRegistryItem(null);
			var ex = AssertExceptionThrown<RegistryValidationException>(() => DataType.Validate(registryItem, "ThisIsNotHex", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Input must be valid hexadecimal characters only (0-9, A-F)", ex.Message);
		}

		public void TestValidateHexStringOfWrongLength()
		{
			var registryItem = GetNewRegistryItem(null);
			var ex = AssertExceptionThrown<RegistryValidationException>(() => DataType.Validate(registryItem, "AAA", Guid.Empty, Guid.Empty, Guid.Empty));

			var expectedMessage = string.Format("Key must be {0} bytes ({1} hexadecimal characters), but was 3 hexadecimal characters.", KeySizeForTest, KeySizeForTest * 2);
			AssertEquals(expectedMessage, ex.Message);
		}
	}
}
