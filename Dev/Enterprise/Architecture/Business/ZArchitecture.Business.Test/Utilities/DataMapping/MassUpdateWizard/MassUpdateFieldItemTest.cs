using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class MassUpdateFieldItemTest : TestCase
	{
		public void TestCount()
		{
			ImportPropertyInfoImpl property1 = new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Number) { HeaderText = "Num" };
			ImportPropertyInfoImpl property2 = new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_VarCharMax) { HeaderText = "Txt" };
			ImportPropertyInfoImpl property3 = new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Bool) { HeaderText = "Bool" };
			ImportPropertyInfoImpl property4 = new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_DateTimeOffset) { HeaderText = "DateTimeOffset" };
			ImportPropertyInfoImpl property5 = new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Geography) { HeaderText = "Geography" };
			MassUpdateFieldItemList list = new MassUpdateFieldItemList();
			AssertEquals(0, list.Count);
			list.Add(property1);
			AssertEquals(1, list.Count);
			list.Add(property2);
			AssertEquals(2, list.Count);
			list.Add(property3);
			AssertEquals(3, list.Count);
			list.Add(property4);
			AssertEquals(4, list.Count);
			list.Add(property5);
			AssertEquals(5, list.Count);
		}

		public void TestAddAndIndexing()
		{
			ImportPropertyInfoImpl property1 = new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Number) { HeaderText = "Num" };
			ImportPropertyInfoImpl property2 = new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_VarCharMax) { HeaderText = "Txt" };
			ImportPropertyInfoImpl property3 = new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Bool) { HeaderText = "Bool" };
			ImportPropertyInfoImpl property4 = new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_DateTimeOffset) { HeaderText = "DateTimeOffset" };
			ImportPropertyInfoImpl property5 = new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Geography) { HeaderText = "Geography" };
			MassUpdateFieldItemList list = new MassUpdateFieldItemList();
			AssertNull(list["Z0_Number (Num)"]);
			list.Add(property1);
			MassUpdateFieldItem item = list["Num (Z0_Number)"];
			AssertEquals(property1, item.propertyInfo);
			list.Add(property2);
			list.Add(property3);
			item = list["Txt (Z0_VarCharMax)"];
			AssertEquals(property2, item.propertyInfo);
			list.Add(property4);
			item = list["DateTimeOffset (Z0_DateTimeOffset)"];
			AssertEquals(property4, item.propertyInfo);
			list.Add(property5);
			item = list["Geography (Z0_Geography)"];
			AssertEquals(property5, item.propertyInfo);
		}
	}
}
