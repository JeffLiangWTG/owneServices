using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(DSBJobManagementCollection))]
	public class DSBJobManagementCollectionTest : ActiveBusinessObjectCollectionTestCase<DSBJobManagementCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, ((IBindingList)Collection).AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals(false, ((IBindingList)Collection).AllowRemove);
		}
	}
}
