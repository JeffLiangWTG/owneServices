using Enterprise.DocumentWrappers.Customs.EU;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class EmptyBox16CountryOfOriginEvaluatorTest : TestCase
{
	public void TestEvaluate()
	{
		var evaluator = (IBox16CountryOfOriginEvaluator)new EmptyBox16CountryOfOriginEvaluator();
		AssertEquals("Result", "", evaluator.Evaluate());
	}
}
