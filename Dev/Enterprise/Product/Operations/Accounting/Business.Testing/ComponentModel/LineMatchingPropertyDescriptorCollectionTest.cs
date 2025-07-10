using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ComponentModel.Testing
{
	using NUnit.Framework;

	public class LineMatchingPropertyDescriptorCollectionTest : TestCase
	{
		public void TestProperties()
		{
			LineMatchingPropertyDescriptorCollection properties = LineMatchingPropertyDescriptorCollection.FromType(typeof(MockClass));
			AssertEquals("The number of properties on ILineMatching should equal the number of properties returned by the MatchingDescriptorProvider when passed the MockClass since InvoiceBatchNumber is a property on the IMatching interface", TypeDescriptor.GetProperties(typeof(ILineMatching)).Count, properties.Count);
		}

		[PropertyDescriptorCollection(typeof(LineMatchingPropertyDescriptorCollection))]
		public class MockClass
		{
		}
	}
}