using System;
using CargoWise.Database.TestFramework.ObjectModel;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	class NUnitAssertionProviderTest : TestCase
	{
		#region TestBuildAssertion

		public void TestBuildAssertion_Equals()
		{
			IAssertionProvider provider = NUnitAssertionProvider.Instance;
			var assertion = provider.BuildAssertion(AssertionType.Equals, "Test", (string x, string y) => x + y, "Bar");

			var assertionFailureMessage = Html("Test") + FormatFailedComparison("Bar", "Ba");
			AssertExceptionThrown(typeof(AssertionFailedError), assertionFailureMessage, () => assertion("B", "a"));
			AssertNoExceptionThrown(() => assertion("B", "ar"));
		}

		public void TestBuildAssertion_NotEquals()
		{
			IAssertionProvider provider = NUnitAssertionProvider.Instance;
			var assertion = provider.BuildAssertion(AssertionType.NotEquals, "Test", (string x, string y) => x + y, "Bar");

			var assertionFailureMessage = Html("Test") +
				"<br>" +
				" expected " + HtmlFormatGoodValue("Bar") +
				" not to be equal to " + HtmlFormatBadValue("Bar");

			AssertExceptionThrown(typeof(AssertionFailedError), assertionFailureMessage, () => assertion("B", "ar"));
			AssertNoExceptionThrown(() => assertion("B", "a"));
		}

		public void TestBuildAssertion_IsNull()
		{
			IAssertionProvider provider = NUnitAssertionProvider.Instance;
			var assertion = provider.BuildAssertion(AssertionType.IsNull, "Test", (string x, string y) => x == y ? null : "NOT NULL", "IRRELEVANT");

			var assertionFailureMessage = Html("Test - should be [null].") + FormatFailedComparison(null, "NOT NULL");
			AssertExceptionThrown(typeof(AssertionFailedError), assertionFailureMessage, () => assertion("B", "A"));
			AssertNoExceptionThrown(() => assertion("B", "B"));
		}

		public void TestBuildAssertion_NotNull()
		{
			IAssertionProvider provider = NUnitAssertionProvider.Instance;
			var assertion = provider.BuildAssertion(AssertionType.NotNull, "Test", (string x, string y) => x == y ? null : "NOT NULL", "IRRELEVANT");
			var assertionFailureMessage = Html("Test - should not be [null].");

			AssertExceptionThrown(typeof(AssertionFailedError), assertionFailureMessage, () => assertion("B", "B"));
			AssertNoExceptionThrown(() => assertion("B", "A"));
		}

		public void TestBuildAssertion_InvalidAssertionType()
		{
			IAssertionProvider provider = NUnitAssertionProvider.Instance;
			AssertExceptionThrown(typeof(NotSupportedException), "Assertion type None is not supported for NUnitAssertionProvider.",
				() => provider.BuildAssertion(AssertionType.None, "Test", (string x, string y) => x + y, "Bar"));
		}

		#endregion

		#region TestFailAssertionRun

		public void TestFailAssertionRun()
		{
			IAssertionProvider provider = NUnitAssertionProvider.Instance;
			AssertExceptionThrown(typeof(AssertionFailedError), Html("Test Error"), () => provider.FailAssertionRun("Test Error"));
		}

		#endregion

		#region TestRunAllAssertions

		public void TestRunAllAssertions()
		{
			IAssertionProvider provider = NUnitAssertionProvider.Instance;

			bool assertionWasRun = false;
			var expectedCombineAssertionsErrorMessage = $"<b>{Html("All Assertions Message")}</b><br/>"
				+ Html($"The following list of failures occurred ({1}):-")
				+ "<br/><br/>Fail!!<br/>";

			AssertExceptionThrown(typeof(AssertionFailedError), expectedCombineAssertionsErrorMessage,
				() => provider.RunAllAssertions("All Assertions Message", "Dummy", "State", (objectToAssert, stateForAssertions) =>
				{
					Fail("Fail!!");
					AssertEquals("Should Have passed in Correct Object.", "Dummy", objectToAssert);
					AssertEquals("Should Have passed in Correct State.", "State", stateForAssertions);

					assertionWasRun = true;
				}));

			AssertEquals("Assertions should have been Invoked.", true, assertionWasRun);
		}

		#endregion

		#region TestRunNestedAssertions

		public void TestRunNestedAssertions()
		{
			IAssertionProvider provider = NUnitAssertionProvider.Instance;

			bool assertionWasRun = false;
			provider.RunNestedAssertions("Dummy", "State", (objectToAssert, stateForAssertions) =>
			{
				AssertEquals("Should Have passed in Correct Object.", "Dummy", objectToAssert);
				AssertEquals("Should Have passed in Correct State.", "State", stateForAssertions);

				assertionWasRun = true;
			});

			AssertEquals("Assertions should have been Invoked.", true, assertionWasRun);
		}

		#endregion
	}
}
