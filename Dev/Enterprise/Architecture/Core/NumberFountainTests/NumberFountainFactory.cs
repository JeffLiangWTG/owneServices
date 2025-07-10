using System;

namespace Enterprise.NumberFountain.Testing
{
	using NUnit.Framework;

	public class NumberFountainFactoryTest : TestCase
	{
		const string TestKey = "TestKey";

		/// <summary>
		/// - Name: cannot be an empty string
		/// </summary>
		public void TestInvalidFountainNameThrowsException()
		{
			AssertExceptionThrown(
				"fountainName cannot be null or empty.",
				typeof(ArgumentException),
				() => new NumberFountainFactoryForTest(null, Guid.Empty)
				);
			AssertExceptionThrown(
				"fountainName cannot be null or empty.",
				typeof(ArgumentException),
				() => new NumberFountainFactoryForTest("", Guid.Empty)
				);
		}

		/// <summary>
		/// - MaxValue: cannot be greater than DefaultMaxNumber
		/// </summary>
		public void TestInvalidMaxValueThrowsException()
		{
			new NumberFountainFactoryForTest(TestKey, Guid.Empty,
				minValue: 10, maxValue: 10);

			AssertExceptionThrown(
				typeof(ArgumentOutOfRangeException),
				() => new NumberFountainFactoryForTest(TestKey, Guid.Empty, minValue: 10, maxValue: 10 - 1)
				);
		}

		/// <summary>
		/// - MinValue: cannot be less than DefaultMinNumber
		/// </summary>
		public void TestInvalidMinValueThrowsException()
		{
			AssertExceptionThrown(
				typeof(ArgumentOutOfRangeException),
				() => new NumberFountainFactoryForTest(TestKey, Guid.Empty, minValue: -1)
				);
			AssertExceptionThrown(
				typeof(ArgumentOutOfRangeException),
				() => new NumberFountainFactoryForTest(TestKey, Guid.Empty, minValue: 0)
				);
		}
	}

	internal class NumberFountainFactoryForTest : NumberFountainFactory
	{
		public NumberFountainFactoryForTest(string fountainName, Guid ownerPk, bool rollOver = false, long minValue = FountainUtils.MinNumber, long maxValue = FountainUtils.MaxNumber)
			: base(fountainName, ownerPk, rollOver, minValue, maxValue)
		{
		}

		public override INumberFountain New()
		{
			throw new NotImplementedException();
		}
	}
}
