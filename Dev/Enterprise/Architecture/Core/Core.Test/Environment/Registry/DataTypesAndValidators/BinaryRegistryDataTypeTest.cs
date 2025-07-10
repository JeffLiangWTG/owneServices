using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(BinaryRegistryDataType))]
	sealed class BinaryRegistryDataTypeTest : RegistryDataTypeTestCase<BinaryRegistryDataType>
	{
		protected override BinaryRegistryDataType GetNewDataType()
		{
			return new BinaryRegistryDataType();
		}

		protected override object GetNullRepresentation()
		{
			return new byte[] { 34, 104, 23, 82, 126 }; // Don't change this just because the magic number changes; existing clients matter.
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new[]
			{
				new ValidSampleAndBinaryValueInDB(null, (byte[])GetNullRepresentation()),
				new ValidSampleAndBinaryValueInDB(new byte[5] { 1, 1, 2, 3, 5 }, new byte[5] { 1, 1, 2, 3, 5 }),
				new ValidSampleAndBinaryValueInDB(new byte[5] { 1, 4, 9, 16, 25 }, new byte[5] { 1, 4, 9, 16, 25 })
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
	}
}
