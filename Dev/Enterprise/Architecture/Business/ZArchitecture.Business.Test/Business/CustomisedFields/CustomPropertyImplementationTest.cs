using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class CustomPropertyImplementationTest : TestCaseWithFactory
	{
		public void TestCustomPropertyImplementation()
		{
			var property1 = new CustomPropertyImplementation<BusinessObject>("abc", typeof(string));
			AssertEquals("abc", property1.Identifier);
			AssertEquals(typeof(string), property1.Info.Type);
			Assert(property1.Info.ReadOnly);
			Assert(property1.Info.Visible);

			var property2 = new CustomPropertyImplementation<BusinessObject>("xyz", typeof(int), null, (bizo, value) => false);
			AssertEquals("xyz", property2.Identifier);
			AssertEquals(typeof(int), property2.Info.Type);
			Assert(!property2.Info.ReadOnly);
			Assert(property2.Info.Visible);

			var property3 = new CustomPropertyImplementation<BusinessObject>("xyz", "caption", typeof(int), null, (bizo, value) => false, visible: false);
			AssertEquals("xyz", property3.Identifier);
			AssertEquals(typeof(int), property3.Info.Type);
			Assert(!property3.Info.ReadOnly);
			Assert(!property3.Info.Visible);

			var property4 = new CustomPropertyImplementation<BusinessObject>("xyz", "caption", typeof(int), null, (bizo, value) => false, visible: true);
			AssertEquals("xyz", property4.Identifier);
			AssertEquals(typeof(int), property4.Info.Type);
			Assert(!property4.Info.ReadOnly);
			Assert(property4.Info.Visible);
		}

		public void TestGetValue()
		{
			string value1 = "111";
			var property1 = new CustomPropertyImplementation<BusinessObject>("abc", typeof(string), parent => value1);

			AssertEquals("111", property1.GetValue(null));
			AssertEquals("111", property1.GetValue(Factory.New<DummyBusinessObject>()));
			value1 = "222";
			AssertEquals("222", property1.GetValue(null));

			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Number = 111;
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Number = 222;
			var property2 = new CustomPropertyImplementation<BusinessObject>("abc", typeof(ZInt), parent => parent["Z0_Number"]);

			AssertEquals(111, property2.GetValue(dummy1));
			AssertEquals(222, property2.GetValue(dummy2));
			dummy1.Z0_Number = 333;
			AssertEquals(333, property2.GetValue(dummy1));

			var property3 = new CustomPropertyImplementation<BusinessObject>("abc", typeof(int));
			AssertNull(property3.GetValue(null));
		}

		public void TestSetValue()
		{
			string value1 = string.Empty;
			var property1 = new CustomPropertyImplementation<BusinessObject>("abc", typeof(string), parent => value1, (parent, value) => { value1 = (string)value; return true; });
			Assert(string.IsNullOrEmpty(value1));
			property1.TrySetValue(null, "111");
			AssertEquals("111", value1);
			property1.TrySetValue(null, "222");
			AssertEquals("222", value1);

			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Number = 0;
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Number = 0;
			var property2 = new CustomPropertyImplementation<BusinessObject>("abc", typeof(ZInt), null, (parent, value) => { parent["Z0_Number"] = new ZInt(value); return true; });

			AssertEquals(0, dummy1.Z0_Number);
			AssertEquals(0, dummy2.Z0_Number);
			property2.TrySetValue(dummy1, 111);
			property2.TrySetValue(dummy2, 222);
			AssertEquals(111, dummy1.Z0_Number);
			AssertEquals(222, dummy2.Z0_Number);

			var property3 = new CustomPropertyImplementation<BusinessObject>("abc", typeof(string));
			AssertNoExceptionThrown(() => property3.TrySetValue(null, "333"));
		}
	}
}
