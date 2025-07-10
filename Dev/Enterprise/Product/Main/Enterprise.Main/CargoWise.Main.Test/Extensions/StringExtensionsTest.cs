using System.Collections.Generic;
using System.Linq;
using CargoWise.Main.Extensions;
using NUnit.Framework;

namespace CargoWise.Main.Test.Extensions;

public class StringExtensionsTest : TestCase
{
	public void TestReplaceWhitespaces()
	{
		var testCase = new List<(string, string)>
		{
			("First\nSecond\nThird", "FirstSecondThird"),
			("First\r\nSecond\r\nThird", "FirstSecondThird"),
			("First\rSecond\rThird", "FirstSecondThird"),
			("First\nSecond\r\nThird\r", "FirstSecondThird"),
			("\nFirst\nSecond\r\nThird\r", "FirstSecondThird"),
			("\n\n\r\n", ""),
			("", ""),
			("Test", "Test"),
		};

		foreach (var (index, input, expected) in testCase.Select((tc, i) => (i, tc.Item1, tc.Item2)))
		{
			AssertEquals($"({index})", expected, input.ReplaceWhitespaces());
		}
	}

	public void TestReplaceWhitespaces_WhenReplacementNotEmpty()
	{
		var str = "\nFirst\rSecond\nThird\r";

		var testCase = new List<(string, string)>
		{
			(" ", " First Second Third "),
			("*", "*First*Second*Third*"),
			("-", "-First-Second-Third-"),
			(" | ", " | First | Second | Third | "),
		};

		foreach (var (index, input, expected) in testCase.Select((tc, i) => (i, tc.Item1, tc.Item2)))
		{
			AssertEquals($"({index})", expected, str.ReplaceWhitespaces(replacement: input));
		}
	}
}
