using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(WrapperCollection<DummyBizo>))]
	public class WrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WrapperCollection<WrapperCollectionTest.DummyBizo>>
	{
		protected override WrapperCollection<DummyBizo> GetCollectionToTest()
		{
			return new WrapperCollection<DummyBizo>(Factory);
		}

		public class DummyBizo : NonPersistentBusinessObject
		{
			public DummyBizo() : base() { }
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DummyBizo();
		}

		public class TestObject : NonPersistentBusinessObject
		{
			public TestObject() : base() { }
		}

		public void TestExceptionOnCreateObject()
		{
			bool correctExceptionFound = false;
			var wrapper = new WrapperCollection<TestObject>(Factory);
			try
			{
				wrapper.AddNew();
			}
			catch (NotImplementedException e)
			{
				correctExceptionFound = e.Message == "Allow new is false so shouldn't get called";
			}
			Assert("Correct Exception Found", correctExceptionFound);
		}
	}
}
