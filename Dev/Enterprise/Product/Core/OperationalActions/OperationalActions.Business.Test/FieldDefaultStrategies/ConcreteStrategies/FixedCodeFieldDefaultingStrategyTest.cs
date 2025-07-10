using System.Collections;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class FixedCodeFieldDefaultingStrategyTest : TestCaseWithFactory
	{
		public void TestDetail()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("ALP", "Alpha");
			list.AddPair("BET", "Beta");
			OperationalActionCodeFieldSupporter supporter = new OperationalActionCodeFieldSupporter("fieldName", false, list);
			FixedCodeFieldDefaultingStrategy strategy = new FixedCodeFieldDefaultingStrategy(supporter);
			AssertEquals(FieldType.TextDropEdit, strategy.DetailFieldType);
			AssertEquals(3, strategy.DetailMaxLength);
			AssertEquals("ALP", strategy.GetDefaultValue("ALP"));
			AssertEquals("BET", strategy.GetDefaultValue("BET"));
			IList boundCollection = strategy.GetBoundCollection(Factory);
			AssertType(typeof(CodeDescriptionPairList), boundCollection);
			AssertMultilineASCIIEquals("", list.ElementsAsString, ((CodeDescriptionPairList)boundCollection).ElementsAsString);
		}
	}
}
