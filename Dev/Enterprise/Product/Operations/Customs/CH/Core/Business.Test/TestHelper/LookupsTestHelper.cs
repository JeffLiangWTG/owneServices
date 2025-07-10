using System;
using Enterprise.ZArchitecture.Core;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.CH.Business.Testing;

public static class LookupsTestHelper
{
	public static void AssertListCodesAndIsCached(Func<CodeDescriptionPairList> getList, string expectedCodesAsString)
	{
		AssertListCodesAndIsCached(null, getList, expectedCodesAsString);
	}

	public static void AssertListCodesAndIsCached(string assertionMessage, Func<CodeDescriptionPairList> getList, string expectedCodesAsString)
	{
		if (assertionMessage != null)
		{
			assertionMessage += " - ";
		}
		var list = getList();
		AssertEquals($"{assertionMessage}Codes", expectedCodesAsString, list.CodesAsString);
		AssertSame($"{assertionMessage}Cached", list, getList());
	}
}
