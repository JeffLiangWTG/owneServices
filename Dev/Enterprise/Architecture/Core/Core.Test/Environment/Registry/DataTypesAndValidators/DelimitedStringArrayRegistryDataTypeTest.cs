using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(DelimitedStringArrayRegistryDataType))]
	sealed class DelimitedStringArrayRegistryDataTypeTest : StringArrayRegistryDataTypeTest<DelimitedStringArrayRegistryDataType>
	{
		#region Implementation

		protected override DelimitedStringArrayRegistryDataType GetNewDataType()
		{
			return new DelimitedStringArrayRegistryDataType();
		}

		protected override void AssertValuesEqual(string message, object lhs, object rhs)
		{
			string[] lhsStrings = (string[])lhs;
			string[] rhsStrings = (string[])rhs;

			AssertEquals("Length", lhsStrings.Length, rhsStrings.Length);

			for (int i = 0; i < lhsStrings.Length; i++)
			{
				AssertEquals("[" + i + "]", lhsStrings[i], rhsStrings[i]);
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(System.Array.Empty<string>(), System.Array.Empty<byte>()),
				new ValidSampleAndBinaryValueInDB(new string[] { "hello world" }, Encoding.Unicode.GetBytes("hello world")),
				new ValidSampleAndBinaryValueInDB(new string[] { "helloworld", "goodbye world" }, Encoding.Unicode.GetBytes("helloworld◄◘►goodbye world")),
			};
		}

		#endregion
	}
}
