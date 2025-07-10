using System.ComponentModel;
using System.Data;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ChildEditableAttributeTest : TestCaseWithFactory
	{
		public void TestGetValue()
		{
			PropertyDescriptor childEditable = TypeDescriptor.GetProperties(typeof(Dummy))["ChildEditable"];
			PropertyDescriptor explicitlyChildEditable = TypeDescriptor.GetProperties(typeof(Dummy))["ExplicitlyChildEditable"];
			PropertyDescriptor nonChildEditable = TypeDescriptor.GetProperties(typeof(Dummy))["NonChildEditable"];
			PropertyDescriptor explicitlyNonChildEditable = TypeDescriptor.GetProperties(typeof(Dummy))["ExplicitlyNonChildEditable"];

			AssertEquals("ChildEditable", true, ChildEditableAttribute.GetValue(childEditable));
			AssertEquals("ExplicitlyChildEditable", true, ChildEditableAttribute.GetValue(explicitlyChildEditable));
			AssertEquals("NonChildEditable", false, ChildEditableAttribute.GetValue(nonChildEditable));
			AssertEquals("ExplicitlyNonChildEditable", false, ChildEditableAttribute.GetValue(explicitlyNonChildEditable));
		}

		#region Test Classes

		class Dummy : DummyBusinessObject
		{
			public Dummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[ChildEditable(true)]
			public DummyDependentBusinessObjectCollection ChildEditable
			{
				get { return null; }
			}

			[ChildEditable(true)]
			public DummyDependentBusinessObjectCollection ExplicitlyChildEditable
			{
				get { return null; }
			}

			public DummyDependentBusinessObjectCollection NonChildEditable
			{
				get { return null; }
			}

			[ChildEditable(false)]
			public DummyDependentBusinessObjectCollection ExplicitlyNonChildEditable
			{
				get { return null; }
			}
		}

		#endregion
	}
}
