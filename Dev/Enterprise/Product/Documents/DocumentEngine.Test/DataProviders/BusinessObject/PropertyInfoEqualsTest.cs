using NUnit.Framework;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	sealed class PropertyInfoEqualsTest : TestCase
	{
		public int Foo
		{
			get { return 42; }
			set { }
		}

		/// <summary>
		/// Tests that PropertyInfo objects that point to the same property are the same object.
		/// This is not guaranteed by .NET; it is an implementation detail which is expected to change in Whidbey.
		/// If this test fails, we need to write some code to compare PropertyInfo values, so that the BusinessObjectReflectorTest will work.
		/// </summary>
		public void TestPropertyInfoObjectIdentity()
		{
			AssertSame("Two PropertyInfo pointing to the same property should be equal", GetType().GetProperty("Foo"), typeof(PropertyInfoEqualsTest).GetProperty("Foo"));
			AssertSame("Two property getters pointing to the same property should be equal", GetType().GetProperty("Foo").GetGetMethod(true), typeof(PropertyInfoEqualsTest).GetProperty("Foo").GetGetMethod(true));
			AssertSame("Two ToString() method pointing to the same method should be equal", GetType().GetMethod("ToString"), typeof(PropertyInfoEqualsTest).GetMethod("ToString"));
		}
	}
}
