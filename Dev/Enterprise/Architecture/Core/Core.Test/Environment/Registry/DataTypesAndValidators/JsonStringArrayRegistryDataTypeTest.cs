using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(JsonStringArrayRegistryDataType))]
	sealed class JsonStringArrayRegistryDataTypeTest : StringArrayRegistryDataTypeTest<JsonStringArrayRegistryDataType>
	{
		public void TestDeserialiseSerialise()
		{
			var binaryValue = Encoding.Unicode.GetBytes(@"[""helloworld"",""goodbye world""]");
			var stringArray = DataType.Deserialise(binaryValue);
			AssertSequencesEqual(new string[] { "helloworld", "goodbye world" }, stringArray);
			var backBinaryValue = DataType.Serialise(stringArray);
			AssertSequencesEqual(binaryValue, backBinaryValue);
		}

		#region Implementation

		protected override JsonStringArrayRegistryDataType GetNewDataType()
		{
			return new JsonStringArrayRegistryDataType();
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
				new ValidSampleAndBinaryValueInDB(new string[] { "hello world" }, Encoding.Unicode.GetBytes(@"[""hello world""]")),
				new ValidSampleAndBinaryValueInDB(new string[] { "helloworld", "goodbye world" }, Encoding.Unicode.GetBytes(@"[""helloworld"",""goodbye world""]")),
			};
		}

		#endregion
	}
}
