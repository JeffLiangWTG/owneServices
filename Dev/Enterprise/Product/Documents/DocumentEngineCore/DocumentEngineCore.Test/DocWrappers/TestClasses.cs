using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	public static class TestClasses
	{
		public class TestBO : NonPersistentBusinessObject
		{
			public ZString TextField
			{
				get { return textField; }
				set { textField = value; }
			}
			ZString textField;

			public ZInt IntField
			{
				get { return intField; }
				set { intField = value; }
			}
			ZInt intField;

			public ZDecimal DecimalField
			{
				get { return decimalField; }
				set { decimalField = value; }
			}
			ZDecimal decimalField;
		}

		public class TestBOCollection : NonPersistentBusinessObjectCollection<TestBO>
		{
			public TestBOCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new TestBO();
			}
		}

		public class DocTestBOWrapper : DocumentWrapper
		{
			DocTestBOWrapper(TestBO bo, BusinessObjectFactory factoryToWrap)
				: base(bo, factoryToWrap)
			{
			}

			public static DocTestBOWrapper New(BusinessObjectFactory factory, ZGuid pk)
			{
				return New(factory.Load<TestBO>(pk), factory);
			}

			public static DocTestBOWrapper New(TestBO bo, BusinessObjectFactory factoryToWrap)
			{
				if (bo == null)
				{
					return null;
				}
				else
				{
					return new DocTestBOWrapper(bo, factoryToWrap);
				}
			}

			protected new TestBO WrappedObject
			{
				get { return (TestBO)base.WrappedObject; }
			}

			public override string ToString()
			{
				return WrappedObject.ToString();
			}

			public ZString TextField
			{
				get { return WrappedObject.TextField; }
			}

			public ZInt IntField
			{
				get { return WrappedObject.IntField; }
			}

			public ZDecimal DecimalField
			{
				get { return WrappedObject.DecimalField; }
			}
		}

		public class DocTestBOWrapperCollection : DocumentWrapperCollection<DocTestBOWrapper>
		{
			public DocTestBOWrapperCollection(TestBOCollection collectionSource)
				: base(collectionSource, collectionSource.Factory)
			{
			}
		}
	}
}
