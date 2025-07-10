using System.Reflection;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class ImportInitializationMethodNameTest : TestCase
	{
		public void TestGetInitializationMethod_Found()
		{
			Assert(ImportInitializationMethodNameAttribute.HasAttributeApplied(this, nameof(Foo)));
			MethodInfo methodInfo = ImportInitializationMethodNameAttribute.GetInitializationMethod(this, nameof(Foo));
			AssertNotNull(methodInfo);
			AssertEquals("Bar", methodInfo.Name);
			AssertNoExceptionThrown(() => methodInfo.Invoke(this, null));
		}

		public void TestGetInitializationMethod_NotFound()
		{
			Assert(!ImportInitializationMethodNameAttribute.HasAttributeApplied(this, nameof(FooBar)));
			MethodInfo methodInfo = ImportInitializationMethodNameAttribute.GetInitializationMethod(this, nameof(FooBar));
			AssertNull(methodInfo);
		}

		public void TestGetInitializationMethod_NoMatchingMethod()
		{
			Assert(ImportInitializationMethodNameAttribute.HasAttributeApplied(this, nameof(Qux)));
			MethodInfo methodInfo = ImportInitializationMethodNameAttribute.GetInitializationMethod(this, nameof(Qux));
			AssertNull(methodInfo);
		}

		public void TestGetInitializationMethod_Found_NotNull()
		{
			Assert(ImportInitializationMethodNameAttribute.HasAttributeApplied(this, nameof(Baz)));
			MethodInfo methodInfo = ImportInitializationMethodNameAttribute.GetInitializationMethod(this, nameof(Baz));
			AssertNull(methodInfo);
		}

		public void TestGetInitializationMethod_NullArg()
		{
			Assert(!ImportInitializationMethodNameAttribute.HasAttributeApplied(null, nameof(Qux)));
			MethodInfo methodInfo = ImportInitializationMethodNameAttribute.GetInitializationMethod(null, nameof(Qux));
			AssertNull(methodInfo);
			Assert(!ImportInitializationMethodNameAttribute.HasAttributeApplied(this, null));
			methodInfo = ImportInitializationMethodNameAttribute.GetInitializationMethod(this, null);
			AssertNull(methodInfo);
			Assert(!ImportInitializationMethodNameAttribute.HasAttributeApplied(null, null));
			methodInfo = ImportInitializationMethodNameAttribute.GetInitializationMethod(null, null);
			AssertNull(methodInfo);
		}

		public void TestGetInitializationMethod_NoAmbiguousMatchException()
		{
			C1 c1 = new C1();
			Assert(ImportInitializationMethodNameAttribute.HasAttributeApplied(c1, "Property1"));
			MethodInfo methodInfo = ImportInitializationMethodNameAttribute.GetInitializationMethod(c1, "Property1");
			AssertNotNull(methodInfo);
			AssertEquals(typeof(C1), methodInfo.DeclaringType);
			AssertEquals("Method1", methodInfo.Name);
			AssertNoExceptionThrown(() => methodInfo.Invoke(c1, null));

			C2 c2 = new C2();
			Assert(ImportInitializationMethodNameAttribute.HasAttributeApplied(c2, "Property1"));
			methodInfo = ImportInitializationMethodNameAttribute.GetInitializationMethod(c2, "Property1");
			AssertNotNull(methodInfo);
			AssertEquals(typeof(C2), methodInfo.DeclaringType);
			AssertEquals("Method2", methodInfo.Name);
			AssertNoExceptionThrown(() => methodInfo.Invoke(c2, null));

			C3 c3 = new C3();
			Assert(ImportInitializationMethodNameAttribute.HasAttributeApplied(c3, "Property1"));
			methodInfo = ImportInitializationMethodNameAttribute.GetInitializationMethod(c3, "Property1");
			AssertNotNull(methodInfo);
			AssertEquals(typeof(C3), methodInfo.DeclaringType);
			AssertEquals("Method3", methodInfo.Name);
			AssertNoExceptionThrown(() => methodInfo.Invoke(c3, null));
		}

		#region Implementation

		[ImportInitializationMethodName("Bar")]
		public int? Foo { get { return null; } }

		public int? FooBar { get { return null; } }

		[ImportInitializationMethodName("Bar")]
		public int? Baz { get { return 1; } }

		[ImportInitializationMethodName("Zum")]
		public int? Qux { get { return null; } }

		public void Bar()
		{
		}

		class C1
		{
			[ImportInitializationMethodName("Method1")]
			public int? Property1 { get; set; }
			public void Method1()
			{
			}
			public void Method2()
			{
			}
			public void Method3()
			{
			}
		}

		class C2 : C1
		{
			[ImportInitializationMethodName("Method2")]
			public new string Property1 { get; set; }
			public new void Method2()
			{
			}
		}

		class C3 : C2
		{
			[ImportInitializationMethodName("Method3")]
			public new object Property1 { get; set; }
			public new void Method3()
			{
			}
		}

		#endregion
	}
}
