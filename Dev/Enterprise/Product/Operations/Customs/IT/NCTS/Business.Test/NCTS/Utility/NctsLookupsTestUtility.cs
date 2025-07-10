using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

static class NctsLookupsTestUtility
{
	public static void AssertLookups(ZString combineAssertionMessage, CodeDescriptionPairList actualLookupsValues, params (ZString Code, ZString Description)[] expectedLookupsValues)
	{
		Assertion.CombineAssertions(combineAssertionMessage, () =>
		{
			Assertion.AssertEquals("Lookup count", expectedLookupsValues.Length, actualLookupsValues.Count);
			foreach (var (code, description) in expectedLookupsValues)
			{
				Assertion.AssertEquals(description, actualLookupsValues.GetDescriptionFromCode(code));
			}
		});
	}
}
