using System;
using NUnit.Framework;

namespace Enterprise.Dat.Implementation.Testing
{
	sealed class TestDiscoveryTypeAttributeExtensionsTest : TestCase
	{
		public void TestHasAttribute_TestClassWithAttribute()
		{
			Assert(
				"DummyAllAttribute is present, which should detected.",
				typeof(TestClassWithAttribute).HasAttribute<DummyAllAttribute>(null));

			var methodWithNoAttribute = typeof(TestClassWithAttribute).GetMethod(nameof(TestClassWithAttribute.TestMethodWithNoAttribute));
			Assert("methodWithNoAttribute can be found", methodWithNoAttribute != null);
			Assert(
				"DummyAllAttribute is present on class, which should detected.",
				typeof(TestClassWithAttribute).HasAttribute<DummyAllAttribute>(methodWithNoAttribute));
			Assert(
				"DummyMethodTest is not present on method, which should not detected.",
				!typeof(TestClassWithAttribute).HasAttribute<DummyMethodAttribute>(methodWithNoAttribute));

			var methodWithAttribute = typeof(TestClassWithAttribute).GetMethod(nameof(TestClassWithAttribute.TestMethodWithAttribute));
			Assert("methodWithAttribute can be found", methodWithAttribute != null);
			Assert(
				"DummyMethodTest is present on method, which should detected.",
				typeof(TestClassWithAttribute).HasAttribute<DummyMethodAttribute>(methodWithAttribute));
		}

		public void TestHasAttribute_TestClassWithNoAttribute()
		{
			Assert(
				"DummyAllAttribute not is present, which should not detected.",
				!typeof(TestClassWithNoAttribute).HasAttribute<DummyAllAttribute>(null));

			var methodWithNoAttribute = typeof(TestClassWithNoAttribute).GetMethod(nameof(TestClassWithNoAttribute.TestMethodWithNoAttribute));
			Assert("methodWithNoAttribute can be found", methodWithNoAttribute != null);
			Assert(
				"DummyAllAttribute is not present on class or method, which should not detected.",
				!typeof(TestClassWithNoAttribute).HasAttribute<DummyAllAttribute>(methodWithNoAttribute));

			var methodWithAttribute = typeof(TestClassWithNoAttribute).GetMethod(nameof(TestClassWithNoAttribute.TestMethodWithAttribute));
			Assert("methodWithAttribute can be found", methodWithAttribute != null);
			Assert(
				"DummyMethodTest is present on method, which should detected.",
				typeof(TestClassWithNoAttribute).HasAttribute<DummyMethodAttribute>(methodWithAttribute));
		}

		public void TestGetAssemblyAttribute()
		{
			Assert(
				"CLSCompliantAttribute is present, which should detected.",
				typeof(TestDiscoveryTypeAttributeExtensionsTest).HasAttribute<CLSCompliantAttribute>(null));

			Assert(
				"ComVisibleAttribute is not present, which should not be detected.",
				!typeof(TestDiscoveryTypeAttributeExtensionsTest).HasAttribute<DummyAssemblyAttribute>(null));
		}
	}
}
