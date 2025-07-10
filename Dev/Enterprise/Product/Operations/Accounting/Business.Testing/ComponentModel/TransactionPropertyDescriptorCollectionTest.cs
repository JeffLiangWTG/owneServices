using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;

namespace Enterprise.Accounting.Business.ComponentModel.Testing
{
	using NUnit.Framework;

	public class TransactionPropertyDescriptorCollectionTest : TestCase
	{
		public void TestProperties()
		{
			TransactionPropertyDescriptorCollection properties = TransactionPropertyDescriptorCollection.FromType(typeof(MockClass));
			AssertEquals("The number of properties on ITransaction should equal the number of properties returned by the TransactionDescriptorProvider when passed the MockClass since InvoiceBatchNumber is a property on the ITransaction interface", TypeDescriptor.GetProperties(typeof(ITransaction)).Count, properties.Count);
		}

		[PropertyDescriptorCollection(typeof(TransactionPropertyDescriptorCollection))]
		public class MockClass
		{
			public ZString TransactionNumber
			{
				get;
				set;
			}
		}
	}
}