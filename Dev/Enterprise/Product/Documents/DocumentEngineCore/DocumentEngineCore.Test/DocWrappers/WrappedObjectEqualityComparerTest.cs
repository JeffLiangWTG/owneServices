using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	sealed class WrappedObjectEqualityComparerTest : TestCaseWithFactory
	{
		public void TestEquals()
		{
			WrappedObjectEqualityComparer<DummyWrapper> comparer = new WrappedObjectEqualityComparer<DummyWrapper>();

			AssertEquals("wrapping different objects", false, comparer.Equals(Wrap(new object()), Wrap(new object())));
			AssertEquals("wrapping equivilant objects", true, comparer.Equals(Wrap("BOB"), Wrap("BOB")));
			AssertEquals("wrapped null == wrapped null", true, comparer.Equals(Wrap(null), Wrap(null)));
			AssertEquals("null == null", true, comparer.Equals(null, null));

			AssertEquals("object != null", false, comparer.Equals(Wrap(new object()), null));
			AssertEquals("null != object", false, comparer.Equals(null, Wrap(new object())));

			AssertEquals("wrapped null != null", false, comparer.Equals(null, Wrap(null)));
			AssertEquals("null != wrapped null", false, comparer.Equals(Wrap(null), null));

			AssertEquals("object != wrapped null", false, comparer.Equals(Wrap(new object()), Wrap(null)));
			AssertEquals("wrapped null != object", false, comparer.Equals(Wrap(null), Wrap(new object())));
		}

		public void TestGetHashCode()
		{
			WrappedObjectEqualityComparer<DummyWrapper> comparer = new WrappedObjectEqualityComparer<DummyWrapper>();

			AssertEquals("null", 0, comparer.GetHashCode(null));
			AssertEquals("wrapping null", 0, comparer.GetHashCode(Wrap(null)));
			AssertEquals("wrapping non-null", "BOB".GetHashCode(), comparer.GetHashCode(Wrap("BOB")));
		}

		DummyWrapper Wrap(object obj)
		{
			return new DummyWrapper(obj, Factory);
		}

		class DummyWrapper : DocumentWrapper
		{
			public DummyWrapper(object wrappedObject, BusinessObjectFactory factory)
				: base(wrappedObject, factory)
			{
			}
		}
	}
}
