using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZPropertyInfoRetrieverTest : TestCaseWithFactory
	{
		public void TestGetZPropertyInfo()
		{
			var propertyDescriptor = new PropertyDescriptorWithZPropertyInfoRetriever("xxx");
			var columnInfo = new ZTextBoxColumnStyleInfo { ColumnName = "xxx" };
			((IOverridablePropertyDescriptor)columnInfo).PropertyDescriptor = propertyDescriptor;

			using (var columnStyle = new ZTextBoxColumnStyle(columnInfo))
			{
				var dummy = Factory.New<DummyBusinessObject>();
				AssertNull(ZPropertyInfoRetriever.GetZPropertyInfo(columnStyle, dummy));

				propertyDescriptor.Info = dummy.FindPropertyInfo("Z0_Code");
				AssertEquals("Z0_Code", ZPropertyInfoRetriever.GetZPropertyInfo(columnStyle, dummy).Name);

				columnStyle.MappingName = "Z0_Number";
				AssertEquals("Z0_Number", ZPropertyInfoRetriever.GetZPropertyInfo(columnStyle, dummy).Name);
			}
		}

		class PropertyDescriptorWithZPropertyInfoRetriever : KPropertyDescriptor, IZPropertyInfoRetriever
		{
			public PropertyDescriptorWithZPropertyInfoRetriever(string name) : base(null, name, null) { }

			public ZPropertyInfo Info { get; set; }

			ZPropertyInfo IZPropertyInfoRetriever.GetZPropertyInfo(BusinessObject businessObject)
			{
				return Info;
			}
		}
	}
}
