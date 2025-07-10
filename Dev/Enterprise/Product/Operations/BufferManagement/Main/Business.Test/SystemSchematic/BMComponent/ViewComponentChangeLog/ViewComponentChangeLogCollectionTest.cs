using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ViewComponentChangeLogCollection))]
	class ViewComponentChangeLogCollectionTest : ActiveBusinessObjectCollectionTestCase<ViewComponentChangeLogCollection>
	{
		public void TestAllowNew_ShouldNotBeSupported()
		{
			AssertEquals(false, ((IBindingList)Collection).AllowNew);
		}
	}
}
