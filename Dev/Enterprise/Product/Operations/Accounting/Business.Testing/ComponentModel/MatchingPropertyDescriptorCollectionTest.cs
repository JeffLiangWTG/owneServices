using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ComponentModel.Testing
{
	using NUnit.Framework;

	public class MatchingPropertyDescriptorCollectionTest : TestCase
	{
		public void TestProperties()
		{
			MatchingPropertyDescriptorCollection properties = MatchingPropertyDescriptorCollection.FromType(typeof(MockClass));
			AssertEquals("The number of properties on IMatching should equal the number of properties returned by the MatchingDescriptorProvider when passed the MockClass since InvoiceBatchNumber is a property on the IMatching interface", TypeDescriptor.GetProperties(typeof(IMatching)).Count, properties.Count);
		}

		[PropertyDescriptorCollection(typeof(MatchingPropertyDescriptorCollection))]
		public class MockClass
		{
			public ZString InvoiceBatchNumber
			{
				get
				{
					return ZString.Empty;
				}
			}
		}
	}
}