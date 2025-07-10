using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	class OverridableTest : TestCase
	{
		public void TestOverrideNullableNoDefault()
		{
			var overridable = new Overridable<string>();
			AssertNull(overridable.Value);
			AssertEquals(false, overridable.IsOverriden);
			overridable.Value = "one";
			AssertEquals("one", overridable.Value);
			AssertEquals(true, overridable.IsOverriden);
			overridable.Value = null;
			AssertNull(overridable.Value);
			AssertEquals(false, overridable.IsOverriden);
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

		public void TestOverrideNullableWithDefault()
		{
			var overridable = new Overridable<string>("default");
			AssertEquals("default", overridable.Value);
			AssertEquals(false, overridable.IsOverriden);
			overridable.Value = "one";
			AssertEquals("one", overridable.Value);
			AssertEquals(true, overridable.IsOverriden);
			overridable.Value = "default";
			AssertEquals("default", overridable.Value);
			AssertEquals(false, overridable.IsOverriden);
			overridable.Value = null;
			AssertNull(overridable.Value);
			AssertEquals(true, overridable.IsOverriden);
			overridable.Value = "default";
			AssertEquals("default", overridable.Value);
			AssertEquals(false, overridable.IsOverriden);
			overridable.Value = "two";
			AssertEquals("two", overridable.Value);
			AssertEquals(true, overridable.IsOverriden);
			Overridable.ResetAll();
			AssertEquals("default", overridable.Value);
			AssertEquals(false, overridable.IsOverriden);
		}

		public void TestOverrideValueTypeNoDefault()
		{
			var overridable = new Overridable<int>();
			AssertEquals(0, overridable.Value);
			AssertEquals(false, overridable.IsOverriden);
			overridable.Value = 1;
			AssertEquals(1, overridable.Value);
			AssertEquals(true, overridable.IsOverriden);
			overridable.Value = 0;
			AssertEquals(0, overridable.Value);
			AssertEquals(false, overridable.IsOverriden);
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

		public void TestOverrideValueTypeWithDefault()
		{
			var overridable = new Overridable<int>(42);
			AssertEquals(42, overridable.Value);
			AssertEquals(false, overridable.IsOverriden);
			overridable.Value = 1;
			AssertEquals(1, overridable.Value);
			AssertEquals(true, overridable.IsOverriden);
			overridable.Value = 42;
			AssertEquals(42, overridable.Value);
			AssertEquals(false, overridable.IsOverriden);
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

		public void TestResetDisposes()
		{
			var overridable = new Overridable<DummyDisposable>(new DummyDisposable());
			var newValue = new DummyDisposable();
			overridable.Value = newValue;
			overridable.ResetValue();
			AssertEquals(true, newValue.IsDisposed);
			AssertEquals(false, overridable.Value.IsDisposed);
			overridable.ResetValue();
			AssertEquals(false, overridable.Value.IsDisposed);
		}

		public void TestConcurrentResetsDoNotDisposeDefaultValue()
		{
			// Arrange
			var defaultValue = new DummyDisposable();
			var overridable = new Overridable<DummyDisposable>(defaultValue) { DisposeIfIDisposable = true };

			var overrideValue = new DummyDisposable();
			overridable.Value = overrideValue;

			// Act
			Parallel.Invoke(Enumerable.Repeat(overridable.ResetValue, 10).ToArray());

			AssertEquals(true, overrideValue.IsDisposed);
			AssertEquals("Should not call Dispose on the default value", false, defaultValue.IsDisposed);
		}

		class DummyDisposable : IDisposable
		{
			public void Dispose()
			{
				IsDisposed = true;
			}

			public bool IsDisposed;

			public override bool Equals(object obj)
			{
				Thread.Sleep(TimeSpan.FromMilliseconds(100));
				return ReferenceEquals(this, obj);
			}

			public override int GetHashCode() { return 0; }
		}
	}
}
