using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class MatcherTest : TestCaseWithFactory
	{
		public void TestBothNegative()
		{
			MatcherTestHelper testMatcher = new MatcherTestHelper(false, false, "FOO", "BAR");
			AssertEquals("Match", false, testMatcher.Match());
			AssertEquals("Result", ZString.Empty, testMatcher.Result);
		}

		public void TestBothPositive()
		{
			MatcherTestHelper testMatcher = new MatcherTestHelper(true, true, "FOO", "BAR");
			AssertEquals("Match", true, testMatcher.Match());
			AssertEquals("Result is first one matched", "FOO", testMatcher.Result);
		}

		public void TestFirstPositive()
		{
			MatcherTestHelper testMatcher = new MatcherTestHelper(true, false, "FOO", "BAR");
			AssertEquals("Match", true, testMatcher.Match());
			AssertEquals("Result is first one", "FOO", testMatcher.Result);
		}

		public void TestSecondPositive()
		{
			MatcherTestHelper testMatcher = new MatcherTestHelper(false, true, "FOO", "BAR");
			AssertEquals("Match", true, testMatcher.Match());
			AssertEquals("Result is second one", "BAR", testMatcher.Result);
		}

		#region Implementation

		class MatcherTestHelper : Matcher<ZString>
		{
			public MatcherTestHelper(bool matchOneMatches, bool matchTwoMatches, ZString matchOneResult, ZString matchTwoResult)
			{
				this.MatchOneMatches = matchOneMatches;
				this.MatchTwoMatches = matchTwoMatches;
				this.MatchOneResult = matchOneResult;
				this.MatchTwoResult = matchTwoResult;
			}

			protected MatchResult MatchOne()
			{
				ZString result = null;
				if (MatchOneMatches)
				{
					result = MatchOneResult;
				}

				return new MatchResult(MatchOneMatches, result);
			}

			protected MatchResult MatchTwo()
			{
				ZString result = null;
				if (MatchOneMatches)
				{
					throw new Exception("MatchTwo should never be called if MatchOne is matched!");
				}

				if (MatchTwoMatches)
				{
					result = MatchTwoResult;
				}

				return new MatchResult(MatchTwoMatches, result);
			}

			protected override MatchDelegate[] matchDelegates
			{
				get { return new MatchDelegate[] { new MatchDelegate(MatchOne), new MatchDelegate(MatchTwo) }; }
			}

			readonly bool MatchOneMatches;
			readonly bool MatchTwoMatches;
			readonly ZString MatchOneResult;
			readonly ZString MatchTwoResult;
		}

		#endregion
	}
}
