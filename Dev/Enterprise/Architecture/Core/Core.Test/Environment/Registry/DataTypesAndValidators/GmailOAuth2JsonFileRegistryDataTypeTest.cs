using System.Text;
using System.Text.Json;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(GmailOAuth2JsonFileRegistryDataType))]
	sealed class GmailOAuth2JsonFileRegistryDataTypeTest : RegistryDataTypeTestCase<GmailOAuth2JsonFileRegistryDataType>
	{
		public void TestSerialization()
		{
			var dataType = new GmailOAuth2JsonFileRegistryDataType();
			var tokenBefore = new GmailOAuth2JsonFile
			{
				JsonText = "test json text",
				FileName = "test.json"
			};

			var tokenDuring = dataType.Serialise(tokenBefore);
			var tokenAfter = dataType.Deserialise(tokenDuring);

			AssertValuesEqual("Deserialized data should match the data before serialization.", tokenBefore, tokenAfter);
		}

		#region Implementation

		protected override GmailOAuth2JsonFileRegistryDataType GetNewDataType()
		{
			return new GmailOAuth2JsonFileRegistryDataType();
		}

		protected override void AssertValuesEqual(string message, object lhs, object rhs)
		{
			var lhsJsonFile = (GmailOAuth2JsonFile)lhs;
			var rhsJsonFile = (GmailOAuth2JsonFile)rhs;

			AssertEquals(message, lhsJsonFile?.JsonText, rhsJsonFile?.JsonText);
			AssertEquals(message, lhsJsonFile?.FileName, rhsJsonFile?.FileName);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var token1 = new GmailOAuth2JsonFile { JsonText = "test json text 1", FileName = "test1.json" };
			var token2 = new GmailOAuth2JsonFile { JsonText = "test json text 2", FileName = "test2.json" };

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(token1, Encoding.Unicode.GetBytes(JsonSerializer.Serialize(token1))),
				new ValidSampleAndBinaryValueInDB(token2, Encoding.Unicode.GetBytes(JsonSerializer.Serialize(token2)))
			};
		}

		#endregion
	}
}
