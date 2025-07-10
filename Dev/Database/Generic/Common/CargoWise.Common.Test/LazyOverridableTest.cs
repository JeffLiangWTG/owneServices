using System;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	class LazyOverridableTest : TestCase
	{
		public void TestOverrideNullableWithNull()
		{
			var overridables = new[] { new LazyOverridable<string>(() => null), new LazyOverridable<string>(() => null, LazyThreadSafetyMode.PublicationOnly) };

			CombineAssertions(() =>
			{
				foreach (var overridable in overridables)
				{
					AssertNull(overridable.Value);
					AssertEquals(false, overridable.IsOverriden);
					overridable.Value = "one";
					AssertEquals("one", overridable.Value);
					AssertEquals(true, overridable.IsOverriden);
					overridable.Value = null;
					AssertNull(overridable.Value);
					AssertEquals(true, overridable.IsOverriden);
					overridable.Value = "two";
					AssertEquals("two", overridable.Value);
					AssertEquals(true, overridable.IsOverriden);
					overridable.Value = "three";
					AssertEquals("three", overridable.Value);
					AssertEquals(true, overridable.IsOverriden);
					Overridable.ResetAll();
					AssertNull(overridable.Value);
					AssertEquals(false, overridable.IsOverriden);
				}
			});
		}

		public void TestOverrideNullableWithNonNull()
		{
			var overridables = new[] { new LazyOverridable<string>(() => "default"), new LazyOverridable<string>(() => "default", LazyThreadSafetyMode.PublicationOnly) };

			CombineAssertions(() =>
			{
				foreach (var overridable in overridables)
				{
					AssertEquals("default", overridable.Value);
					AssertEquals(false, overridable.IsOverriden);
					overridable.Value = "one";
					AssertEquals("one", overridable.Value);
					AssertEquals(true, overridable.IsOverriden);
					overridable.Value = "default";
					AssertEquals("default", overridable.Value);
					AssertEquals(true, overridable.IsOverriden);
					overridable.Value = null;
					AssertNull(overridable.Value);
					AssertEquals(true, overridable.IsOverriden);
					overridable.Value = "default";
					AssertEquals("default", overridable.Value);
					AssertEquals(true, overridable.IsOverriden);
					overridable.Value = "two";
					AssertEquals("two", overridable.Value);
					AssertEquals(true, overridable.IsOverriden);
					Overridable.ResetAll();
					AssertEquals("default", overridable.Value);
					AssertEquals(false, overridable.IsOverriden);
				}
			});
		}

		public void TestOverrideValueTypeWithDefault()
		{
			var overridables = new[] { new LazyOverridable<int>(() => 0), new LazyOverridable<int>(() => 0, LazyThreadSafetyMode.PublicationOnly) };

			CombineAssertions(() =>
			{
				foreach (var overridable in overridables)
				{
					AssertEquals(0, overridable.Value);
					AssertEquals(false, overridable.IsOverriden);
					overridable.Value = 1;
					AssertEquals(1, overridable.Value);
					AssertEquals(true, overridable.IsOverriden);
					overridable.Value = 0;
					AssertEquals(0, overridable.Value);
					AssertEquals(true, overridable.IsOverriden);
					overridable.Value = 2;
					AssertEquals(2, overridable.Value);
					AssertEquals(true, overridable.IsOverriden);
					overridable.Value = 3;
					AssertEquals(3, overridable.Value);
					AssertEquals(true, overridable.IsOverriden);
					Overridable.ResetAll();
					AssertEquals(0, overridable.Value);
					AssertEquals(false, overridable.IsOverriden);
				}
			});
		}

		public void TestOverrideValueTypeWithNonDefault()
		{
			var overridables = new[] { new LazyOverridable<int>(() => 42), new LazyOverridable<int>(() => 42, LazyThreadSafetyMode.PublicationOnly) };

			CombineAssertions(() =>
			{
				foreach (var overridable in overridables)
				{
					AssertEquals(42, overridable.Value);
					AssertEquals(false, overridable.IsOverriden);
					overridable.Value = 1;
					AssertEquals(1, overridable.Value);
					AssertEquals(true, overridable.IsOverriden);
					overridable.Value = 42;
					AssertEquals(42, overridable.Value);
					AssertEquals(true, overridable.IsOverriden);
					overridable.Value = 0;
					AssertEquals(0, overridable.Value);
					AssertEquals(true, overridable.IsOverriden);
					overridable.Value = 2;
					AssertEquals(2, overridable.Value);
					AssertEquals(true, overridable.IsOverriden);
					Overridable.ResetAll();
					AssertEquals(42, overridable.Value);
					AssertEquals(false, overridable.IsOverriden);
				}
			});
		}

		public void TestLazyOverridable()
		{
			var isValueCreated = false;
			var overridables = new[]
			{
				new LazyOverridable<int>(() => {
					isValueCreated = true;
					return 3;
				}),
				new LazyOverridable<int>(() =>
				{
					isValueCreated = true;
					return 3;
				})
			};

			CombineAssertions(() =>
			{
				foreach (var overridable in overridables)
				{
					isValueCreated = false;
					AssertEquals(false, overridable.IsOverriden);
					AssertEquals(false, isValueCreated);
					overridable.Value = 3;
					AssertEquals(true, overridable.IsOverriden);
					AssertEquals(false, isValueCreated);
					Overridable.ResetAll();
					AssertEquals(3, overridable.Value);
					AssertEquals(false, overridable.IsOverriden);
					AssertEquals(true, isValueCreated);
					Overridable.ResetAll();
					AssertEquals(false, overridable.IsOverriden);
					AssertEquals(true, isValueCreated);
				}
			});
		}

		public void TestLazyOverridableNotCacheExceptionWhenModeIsPublicationOnly()
		{
			// Arrange
			var isFirstVisit = true;
			var expectedValue = "ValidValue";
			var lazyOverridable = new LazyOverridable<string>(CalculateLazyValue, LazyThreadSafetyMode.PublicationOnly);

			// Act
			// Assert
			AssertExceptionThrown<Exception>(() =>
			{
				var test = lazyOverridable.Value;
			});
			AssertEquals(expectedValue, lazyOverridable.Value);

			string CalculateLazyValue()
			{
				if (isFirstVisit)
				{
					isFirstVisit = false;
					throw new Exception("Exception in lazy value calculation");
				}
				else
				{
					return expectedValue;
				}
			}
		}
	}
}