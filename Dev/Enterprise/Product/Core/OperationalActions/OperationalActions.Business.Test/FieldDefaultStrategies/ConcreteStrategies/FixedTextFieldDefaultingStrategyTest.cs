using System.Collections;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class FixedTextFieldDefaultingStrategyTest : TestCaseWithFactory
	{
		public void TestDetail()
		{
			OperationalActionTextFieldSupporter supporter = new OperationalActionTextFieldSupporter("fieldName", false, 10);
			FixedTextFieldDefaultingStrategy strategy = new FixedTextFieldDefaultingStrategy(supporter);
			AssertEquals(FieldType.Text, strategy.DetailFieldType);
			AssertEquals(10, strategy.DetailMaxLength);
			AssertEquals("Bob", strategy.GetDefaultValue("Bob"));
			AssertEquals("Fread", strategy.GetDefaultValue("Fread"));
			IList boundCollection = strategy.GetBoundCollection(Factory);
			AssertType(typeof(ReadOnlyCodeDescriptionPairList), boundCollection);
			AssertMultilineASCIIEquals("", "", ((ReadOnlyCodeDescriptionPairList)boundCollection).ElementsAsString);
		}
	}
}
