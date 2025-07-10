using System;
using CargoWise.Common;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class CachedPropertyTest : TestCaseWithDummy
	{
		public void TestValueDoesNotExpireImmediatelyWhenLoadingBizObjDuringCalculation()
		{
			var pk1 = Guid.NewGuid();
			var pk2 = Guid.NewGuid();
			DummyTableCreator.AddDummyBusinessObjectsToDB(pk1, pk2);

			var newFactory = new BusinessObjectFactory();
			int cachedValueDelegateCallCount = 0;
			var dummyCachedValue = new CachedProperty<int>(newFactory, () =>
			{
				newFactory.Load(typeof(DummyBusinessObject), pk1);
				cachedValueDelegateCallCount++;
				return 0;
			});
			_ = dummyCachedValue.Value;
			AssertEquals(1, cachedValueDelegateCallCount);
			_ = dummyCachedValue.Value;
			AssertEquals(1, cachedValueDelegateCallCount);
		}

		public void TestValueNotCachedOnException()
		{
			var cachedValue = new CachedProperty<int>(Factory, () => { throw new NotImplementedException(); });

			AssertExceptionThrown<NotImplementedException>(() =>
			{
				int x = cachedValue.Value;
			});

			AssertExceptionThrown<NotImplementedException>(() =>
			{
				int x = cachedValue.Value;
			});
		}

		public void TestConstructorExceptionFactory()
		{
			try
			{
				new CachedProperty<bool>(null, delegate
				{ return false; });
				Fail("Should throw");
			}
			catch (ArgumentNullException ex)
			{
				Assert(ex.Message.Contains("factory"));
			}
		}

		public void TestConstructorExceptionDelegate()
		{
			try
			{
				new CachedProperty<bool>(Factory, null);
				Fail("Should throw");
			}
			catch (ArgumentNullException ex)
			{
				Assert(ex.Message.Contains("getValueDelegate"));
			}
		}

		public void TestGetValue()
		{
			CachedProperty<ZString> cachedProperty = null;
			var i = 0;
			GetValueDelegate<ZString> getValue = () => (ZString)(++i).ToString();
			AssertEquals("1", Factory.GetValue(ref cachedProperty, getValue));
			AssertEquals("1", Factory.GetValue(ref cachedProperty, getValue));
			AssertNotNull(cachedProperty);
			AssertEquals("1", cachedProperty.Value);
			var cachedProperty1 = cachedProperty;
			cachedProperty = null;
			AssertEquals("2", Factory.GetValue(ref cachedProperty, getValue));
			AssertEquals("2", Factory.GetValue(ref cachedProperty, getValue));
			AssertNotNull(cachedProperty);
			AssertEquals("2", cachedProperty.Value);
			AssertEquals(false, object.ReferenceEquals(cachedProperty1, cachedProperty));
		}

		public void TestDelegateIsCalledOnFirstUseOfValue()
		{
			bool hasCalled = false;
			CachedProperty<bool> cache = new CachedProperty<bool>(Factory, delegate
			{
				hasCalled = true;
				return false;
			});
			AssertEquals("Precondition", false, hasCalled);
			AssertEquals(false, cache.Value);
			AssertEquals(true, hasCalled);
		}

		public void TestDelegateIsCalledWhenCacheInvalidated()
		{
			bool hasCalled = false;
			CachedProperty<bool> cache = new CachedProperty<bool>(Factory, delegate
			{
				hasCalled = true;
				return false;
			});
			AssertEquals(false, cache.Value);
			AssertEquals(true, hasCalled);
			hasCalled = false;
			Factory.InvalidateCachedProperties();
			AssertEquals(false, cache.Value);
			AssertEquals(true, hasCalled);
		}

		public void TestDelegateIsNotCalledTwiceWhenCacheNotInvalidated()
		{
			int callCount = 0;
			CachedProperty<bool> cache = new CachedProperty<bool>(Factory, delegate
			{
				callCount++;
				return false;
			});
			AssertEquals(false, cache.Value);
			AssertEquals(1, callCount);
			AssertEquals(false, cache.Value);
			AssertEquals(1, callCount);
		}

		public void TestCacheVersionIncreaseForcesDelegateCallForNextValue()
		{
			int callCount = 0;
			CachedProperty<bool> cache = new CachedProperty<bool>(Factory, delegate
			{
				callCount++;
				return false;
			});
			AssertEquals(false, cache.Value);
			AssertEquals(1, callCount);
			Factory.InvalidateCachedProperties();
			AssertEquals(false, cache.Value);
			AssertEquals(2, callCount);
		}

		public void TestCachedPropertyWhenParentObjectWasDeleted()
		{
			var dummy = Factory.New<CachedPropertyForTest>();
			dummy.Z0_Long = 101;
			CombineAssertions(() =>
			{
				AssertEquals("cached property value should correct", 104, dummy.RelatedZ0_Int.Value);
				AssertEquals("cached property value should correct", 106, dummy.RelatedZ0_Int_WithDeletedMonitor.Value);
			});
			dummy.Delete();
			CombineAssertions(() =>
			{
				ErrorReporter.Clear();
				AssertEquals("cached property value should be 3", 3, dummy.RelatedZ0_Int.Value);
				AssertEquals("ErrorReporter should have 1 error", 1, ErrorReporter.TotalErrorCount);
				AssertContains("ErrorReporter key shows the property which accessed after parent object was deleted", "Z0_Long", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				AssertEquals("cached property value should return the designated value", 0, dummy.RelatedZ0_Int_WithDeletedMonitor.Value);
			});
		}

		public void TestCachedPropertyConstructorOnNullParentObject()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CachedProperty<bool>(Factory, () => true, null, false));
		}
	}
}
