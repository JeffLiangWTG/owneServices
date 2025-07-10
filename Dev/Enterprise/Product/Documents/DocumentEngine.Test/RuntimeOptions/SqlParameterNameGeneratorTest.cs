using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class SqlParameterNameGeneratorTest : TestCase
	{
		public SqlParameterNameGeneratorTest() : base()
		{ }

		public void TestCounterReset()
		{
			bool resetTriggered = false;
			for (long i = 0; i < 1000001; i++)
			{
				if (SqlParameterNameGenerator.Next() == "@p999999")
				{
					AssertEquals("@p0", SqlParameterNameGenerator.Next());
					resetTriggered = true;
					AssertEquals("@p1", SqlParameterNameGenerator.Next());
					break;
				}
			}
			Assert(resetTriggered);
		}

		public void TestExplicitCounterReset()
		{
			while (SqlParameterNameGenerator.Next() == "@p0")
			{ }
			AssertNotEquals("@p0", SqlParameterNameGenerator.Next());
			SqlParameterNameGenerator.Reset();
			AssertEquals("@p0", SqlParameterNameGenerator.Next());
		}

		public void TestCounterFormat()
		{
			Assert("Should be of the form @p1234", Regex.IsMatch(SqlParameterNameGenerator.Next(), @"^@p[0-9]+$"));
		}

		public void TestCounterUniqueness()
		{
			Assert("Should not return the same value twice", SqlParameterNameGenerator.Next() != SqlParameterNameGenerator.Next());
		}
	}
}
