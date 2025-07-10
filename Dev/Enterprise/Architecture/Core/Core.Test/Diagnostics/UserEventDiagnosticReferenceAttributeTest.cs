using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class UserEventDiagnosticReferenceAttributeTest : TestCase
	{
		public void TestRender()
		{
			object obj = new object();
			A a = new A() { StringProperty = "p1", StringFieldWrapper = "Field1" };
			B b = new B() { StringProperty = "p2", StringFieldWrapper = "Field2" };
			C c = new C() { StringProperty = "p3", StringFieldWrapper = "Field3" };

			CombineAssertions(delegate
			{
				AssertEquals("Render(obj)", null, UserEventDiagnosticReferenceAttribute.Render(obj));
				AssertEquals("Render(a)", "p1 / Field1", UserEventDiagnosticReferenceAttribute.Render(a));
				AssertEquals("Render(b)", "p2 / Field2", UserEventDiagnosticReferenceAttribute.Render(b));
				AssertEquals("Render(c) : AProperty == null", "[]", UserEventDiagnosticReferenceAttribute.Render(c));

				c.AProperty = a;
				AssertEquals("Render(c) : AProperty = a", "[p1]", UserEventDiagnosticReferenceAttribute.Render(c));
			});
		}

		#region Implementation

		[UserEventDiagnosticReference("{StringProperty} / {stringField}")]
		class A
		{
			public string StringProperty { get; set; }
			public string StringFieldWrapper
			{
				get { return stringField; }
				set { stringField = value; }
			}

			protected string stringField = "Field";
		}

		class B : A
		{
		}

		[UserEventDiagnosticReference("[{AProperty.StringProperty}]")]
		class C : B
		{
			public A AProperty { get; set; }
		}

		#endregion
	}
}
