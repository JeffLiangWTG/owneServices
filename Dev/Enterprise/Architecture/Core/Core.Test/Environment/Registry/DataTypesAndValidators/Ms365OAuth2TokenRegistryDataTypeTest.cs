using System.Text;
using System.Text.Json;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(Ms365OAuth2TokenRegistryDataType))]
	sealed class Ms365OAuth2TokenRegistryDataTypeTest : RegistryDataTypeTestCase<Ms365OAuth2TokenRegistryDataType>
	{
		public void TestSerialization()
		{
			var dataType = new Ms365OAuth2TokenRegistryDataType();
			var tokenBefore = new Ms365OAuth2Token
			{
				Identifier = "Id1",
				Token = new byte[] { 11, 22 },
				User = "test@email.com"
			};

			var tokenDuring = dataType.Serialise(tokenBefore);
			var tokenAfter = dataType.Deserialise(tokenDuring);

			AssertValuesEqual("Deserialized data should match the data before serialization.", tokenBefore, tokenAfter);
		}

		#region Implementation

		protected override Ms365OAuth2TokenRegistryDataType GetNewDataType()
		{
			return new Ms365OAuth2TokenRegistryDataType();
		}

		protected override void AssertValuesEqual(string message, object lhs, object rhs)
		{
			var lhsToken = (Ms365OAuth2Token)lhs;
			var rhsToken = (Ms365OAuth2Token)rhs;

			AssertEquals(message, lhsToken?.Identifier, rhsToken?.Identifier);
			AssertEquals(message, lhsToken?.User, rhsToken?.User);
			AssertEquals(message, lhsToken?.Token, rhsToken?.Token);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var token1 = new Ms365OAuth2Token { Identifier = "Id1", Token = new byte[] { 1, 2 }, User = "test@email.com" };
			var token2 = new Ms365OAuth2Token { Identifier = "Id2", Token = new byte[] { 3, 4 }, User = "anothertest@email.com" };

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(token1, Encoding.Unicode.GetBytes(JsonSerializer.Serialize(token1))),
				new ValidSampleAndBinaryValueInDB(token2, Encoding.Unicode.GetBytes(JsonSerializer.Serialize(token2)))
			};
		}

		#endregion
	}
}
