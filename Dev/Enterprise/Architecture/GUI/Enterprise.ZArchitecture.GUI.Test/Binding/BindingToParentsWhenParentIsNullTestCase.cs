using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	sealed class BindingToParentsWhenParentIsNullTestCase : TestCaseWithFactory
	{
		public void TestGetItemPropertiesOnParentParent()
		{
			var bc = new BindingContext();
			var data = Factory.New<TestDataSource>();
			var bm = bc[data, "Parent.Parent"];
			var property = bm.GetItemProperties()["StringProperty"];

			Assert("Property returned and of correct type", property is KPropertyDescriptor);
		}

		public class TestDataSource : DummyBusinessObject
		{
			public TestDataSource(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
			public TestRelatedDataSource Parent { get; set; }
		}

		public class TestRelatedDataSource : DummyBusinessObject
		{
			public TestRelatedDataSource(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
			public TestRelatedRelatedDataSource Parent { get; set; }
		}

		public class TestRelatedRelatedDataSource : DummyBusinessObject
		{
			public TestRelatedRelatedDataSource(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public string StringProperty { get; set; }
		}
	}
}
