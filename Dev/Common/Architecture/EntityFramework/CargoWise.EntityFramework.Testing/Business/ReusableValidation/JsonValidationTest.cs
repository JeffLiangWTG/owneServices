using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class JsonValidationTest : TestCaseWithDummyForValidationTesting
	{
		public void TestValidateJsonWhenInvalidAnErrorIsAdded()
		{
			AssertJsonPropertyHasError("{");
			AssertJsonPropertyHasError("}");
			AssertJsonPropertyHasError("[");
			AssertJsonPropertyHasError("]");
			AssertJsonPropertyHasError("[{]}");
			AssertJsonPropertyHasError("{[}]");
			AssertJsonPropertyHasError("{\"a\":}");
			AssertJsonPropertyHasError("abc");
			AssertJsonPropertyHasError("a b c");
			AssertJsonPropertyHasError("");
		}

		public void TestValidateJsonWhenValidNoErrorsAreAdded()
		{
			AssertJsonPropertyHasNoError("{}");
			AssertJsonPropertyHasNoError("[]");
			AssertJsonPropertyHasNoError("\"abc\"");
			AssertJsonPropertyHasNoError("{\"a\": 1}");
			AssertJsonPropertyHasNoError("{\"a\": \"a\"}");
			AssertJsonPropertyHasNoError("{'a': 1}");
			AssertJsonPropertyHasNoError("{'a': 'a'}");
			AssertJsonPropertyHasNoError("{\"a\": []}");
			AssertJsonPropertyHasNoError("[{}, {}]");
			AssertJsonPropertyHasNoError("[[], []]");
			AssertJsonPropertyHasNoError(" { \"a\"  :    1    } ");
		}

		void AssertJsonPropertyHasError(ZString property)
		{
			Dummy.Z0_NVarChar = property;
			JsonValidation.ValidateJson(Dummy.Z0_NVarCharInfo);
			AssertHasErrors(Dummy.Z0_NVarCharInfo);
		}

		void AssertJsonPropertyHasNoError(ZString property)
		{
			Dummy.Z0_NVarChar = property;
			JsonValidation.ValidateJson(Dummy.Z0_NVarCharInfo);
			AssertNoErrors("Expected no errors for JSON: " + property, Dummy.Z0_NVarCharInfo);
		}
	}
}
