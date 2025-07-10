using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	sealed class WrapperTypeNameAttributeTest : TestCaseWithFactory
	{
		public void TestGetNameForClassBasedOnDocumentWrapper()
		{
			AssertEquals("WrapperTypeNameAttribute.GetName(typeof(TestWrapper))", "Test", WrapperTypeNameAttribute.GetName(typeof(TestWrapper)));
		}

		public void TestGetNameForClassBasedOnDocumentWrapperCollection()
		{
			AssertEquals("WrapperTypeNameAttribute.GetName(typeof(TestWrapperCollection))", "Test Collection", WrapperTypeNameAttribute.GetName(typeof(TestWrapperCollection)));
		}

		public void TestGetNameForClassWithWrapperTypeNameAttribute()
		{
			AssertEquals("WrapperTypeNameAttribute.GetName(typeof(TestWrapperWropper))", "Wropper", WrapperTypeNameAttribute.GetName(typeof(TestWrapperWropper)));
		}

		public void TestGetNameForCollectionClassWithWrapperTypeNameAttributeOnContainedObject()
		{
			AssertEquals("WrapperTypeNameAttribute.GetName(typeof(TestWrapperWropperCollection))", "Wropper Collection", WrapperTypeNameAttribute.GetName(typeof(TestWrapperWropperCollection)));
		}

		#region Implementation
		abstract class TestWrapper : DocumentWrapper
		{
			protected TestWrapper() : base(null, null) { }
		}

		abstract class TestWrapperCollection : DocumentWrapperCollection
		{
			protected TestWrapperCollection() : base(null, null) { }

			public new TestWrapper this[int index]
			{
				get { return (TestWrapper)base[index]; }
			}
		}

		[WrapperTypeName("Wropper")]
		abstract class TestWrapperWropper : DocumentWrapper
		{
			protected TestWrapperWropper() : base(null, null) { }
		}

		abstract class TestWrapperWropperCollection : DocumentWrapperCollection
		{
			protected TestWrapperWropperCollection() : base(null, null) { }

			public new TestWrapperWropper this[int index]
			{
				get { return (TestWrapperWropper)base[index]; }
			}
		}
		#endregion
	}
}
