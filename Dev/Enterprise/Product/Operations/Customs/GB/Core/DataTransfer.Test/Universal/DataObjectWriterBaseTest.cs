using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.GB.DataTransfer.Test.Universal
{
	abstract class DataObjectWriterBaseTest : TestCaseWithFactory
	{
		protected void AssertCodeDescriptionPair(string message, ICodeDescriptionDataObject codeDescriptionPair, string expectedCode, string expectedDescription)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("Code", expectedCode, codeDescriptionPair.Code);
				AssertEquals("Description", expectedDescription, codeDescriptionPair.Description);
			});
		}

		protected void AssertValueTypePair(string message, ValueTypePair valueTypePair, string expectedType, string expectedValue)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("Type", expectedType, valueTypePair.Type);
				AssertEquals("Value", expectedValue, valueTypePair.Value);
			});
		}

		protected void AssertUNLOCO(string message, UNLOCO unloco, string expectedCode, string expectedName)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("Code", expectedCode, unloco.Code);
				AssertEquals("Name", expectedName, unloco.Name);
			});
		}
	}
}
