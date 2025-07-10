using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class ProperCaseExcludeWordValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckWord()
		{
			var excludeWordCollection = new ProperCaseExcludeWordCollection(Factory);
			var excludeWord1 = excludeWordCollection.AddNew();

			excludeWord1.Word = "abc";
			excludeWord1.Validation.ValidateAll();
			AssertNoErrors(excludeWord1.WordInfo);

			var excludeWord2 = excludeWordCollection.AddNew();
			excludeWord2.Word = "abc";

			excludeWord1.Validation.ValidateAll();
			AssertHasError(excludeWord1.WordInfo, "The Word has been duplicated and must be unique.");
			excludeWord2.Validation.ValidateAll();
			AssertHasError(excludeWord2.WordInfo, "The Word has been duplicated and must be unique.");

			excludeWord1.Word = "abc dfg";
			excludeWord1.Validation.ValidateAll();
			AssertHasError(excludeWord1.WordInfo, "Word can not contain spaces");
		}
	}
}
