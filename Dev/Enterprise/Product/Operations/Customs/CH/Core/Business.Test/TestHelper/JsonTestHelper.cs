using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

public static class JsonTestHelper
{
	public static void AssertEquals(string assertionMessage, string expectedString, string actualString)
	{
		AssertEquals(assertionMessage, JsonSerializer.Deserialize<Dictionary<string, string>>(expectedString), actualString);
	}

	public static void AssertEquals(string assertionMessage, Dictionary<string, string> expectedObject, string actualString)
	{
		Assertion.AssertNotNullOrEmpty(assertionMessage, actualString);
		if (!string.IsNullOrEmpty(actualString))
		{
			AssertEquals(assertionMessage, expectedObject, JsonSerializer.Deserialize<Dictionary<string, string>>(actualString));
		}
	}

	public static void AssertEquals(string assertionMessage, Dictionary<string, string> expectedObject, Dictionary<string, string> actualObject)
	{
		var messages = new StringBuilder();
		foreach (var key in expectedObject.Keys.Intersect(actualObject.Keys).Where(k => !IsEqual(expectedObject[k], actualObject[k])))
		{
			messages.Append($"\nExpected: \"{key}\"=\"{expectedObject[key]}\", but was \"{key}\"=\"{actualObject[key]}\"");
		}
		foreach (var key in expectedObject.Keys.Except(actualObject.Keys))
		{
			messages.Append($"\nExpected: \"{key}\"=\"{expectedObject[key]}\", but key not found");
		}
		foreach (var key in actualObject.Keys.Except(expectedObject.Keys))
		{
			messages.Append($"\nUnexpected: \"{key}\"=\"{actualObject[key]}\"");
		}
		Assertion.Assert(assertionMessage + messages.ToString(), messages.Length == 0);
		bool IsEqual(string expected, string actual)
		{
			if (actual != null)
			{
				switch (expected)
				{
					case NotEmpty:
						return actual.Length > 0;
					case AnyGuid:
						return Guid.TryParse(actual, out var _);
				}
			}
			return expected == actual;
		}
	}

	public const string AnyGuid = "--any-guid--";
	public const string NotEmpty = "--not-empty--";
}
